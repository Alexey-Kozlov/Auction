using System.Reflection;
using System.Text.Json;
using Common.Contracts.EventSourcing;
using Common.Contracts.Image;
using Common.Contracts.Processing;
using Common.Utils.Logging;
using EventSourcingService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace EventSourcingService.Services;

public class RestoreSnapShotProcessing
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly EventSourcingDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly CheckAuctionFinished _checkAuctionFinished;

    public RestoreSnapShotProcessing(IPublishEndpoint publishEndpoint, EventSourcingDbContext dbContext,
        IConfiguration configuration, CheckAuctionFinished checkAuctionFinished)
    {
        _publishEndpoint = publishEndpoint;
        _dbContext = dbContext;
        _configuration = configuration;
        _checkAuctionFinished = checkAuctionFinished;
    }

    public async Task ProcessESLog(ConsumeContext<ESContract> context)
    {
        try
        {
            var listItems = new DataForProcessingServicesList
            {
                DataObjects = new List<DataForProcessingService>()
            };
            if (context.Message.EntityType == nameof(RequestRestoreItems))
            {
                //В процедуре Postgres делаем:
                //- запись в ES лог о выполнении восстановления БД из лога
                //- набор записей о восстановлении записей ставок для сервисов BiddingService,FinanceService,NotificationService,
                //SearchService,CommunicationService. Для ImageService - отдельно
                var result = await _dbContext.restore_snap_shot_items(
                    context.Message.CorrelationId,
                    context.Message.EventData).ToListAsync();
                //возвращаем список записей для изменения соответствующих БД в нужных сервисах
                foreach (var item in result)
                {
                    listItems.DataObjects.Add
                    (
                        new DataForProcessingService
                        {
                            DataType = item.entitytype,
                            Data = item.eventdata,
                            CRUD = (CRUD)item.crud
                        }
                    );
                }
                var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                    _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
                sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, context.Message.CorrelationId);
                sendObject.GetType().GetProperty("DataItems").SetValue(sendObject, listItems);
                sendObject.GetType().GetProperty("BatchCount").SetValue(sendObject, -1);

                await _publishEndpoint.Publish(sendObject);
            }
            else
            {
                //обрабатываем изображения
                await GetESLogImages(context);
            }
        }
        catch (Exception e)
        {
            //ошибки прочие
            var messageObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
            messageObject.GetType().GetProperty("CorrelationId").SetValue(messageObject, context.Message.CorrelationId);
            messageObject.GetType().GetProperty("ErrorMessage").SetValue(messageObject, GetErrorMessage.GetMessage(e));
            messageObject.GetType().GetProperty("ErrorExceptionStack").SetValue(messageObject, e.StackTrace + e.InnerException?.Message);
            messageObject.GetType().GetProperty("ErrorExceptionInputData").SetValue(messageObject, GetErrorMessage.GetExceptionStringData(context.Message));
            messageObject.GetType().GetProperty("ErrorServiceName").SetValue(messageObject, "EventSourcingService_RestoreSnapShot");
            messageObject.GetType().GetProperty("UserLogin").SetValue(messageObject, context.Message.UserLogin);
            messageObject.GetType().GetProperty("ItemId").SetValue(messageObject, context.Message.AuctionId);


            var faultType = typeof(FaultMessage<>);
            var typeParams = new Type[] { messageObject.GetType() };
            var faultObjectType = faultType.MakeGenericType(typeParams);

            var faultObject = Activator.CreateInstance(faultObjectType, new object[] { messageObject });
            await _publishEndpoint.Publish(faultObject.GetType().GetMethod("CastItem").Invoke(faultObject, null));
        }
    }

    private async Task GetESLogImages(ConsumeContext<ESContract> context)
    {
        var listItems = new DataForProcessingServicesList
        {
            DataObjects = new List<DataForProcessingService>()
        };
        var typedItem = JsonSerializer.Deserialize<RequestRestoreImages>(context.Message.EventData);
        //счетчик номеров частей передаваемого сообщения
        var MessagePartNumber = 0;
        //общее количество частей передаваемого сообщения
        var MessagePartCounts = 0;
        //идентификатор передаваемого сообщения
        var MessagePartId = Guid.NewGuid();
        //счетчик обработанных записей, для извлечения порций записей из БД
        var OffSet = 0;
        //получаем общее количество записей
        var result = await _dbContext.restore_snap_shot_images(OffSet, 0, typedItem.RestoreDate).ToListAsync();
        var AllItemsCount = result[0].recordscount;
        //максимальный размер передаваемого пакета из БД
        var MaxMessageSizeMb = int.Parse(_configuration["MaxMessageSizeMb"]);
        var partsMessageList = new List<DataForProcessingService>();
        //в цикле получаем записи общим размером не превышающим размер сообщения (если суммарный размер
        //изображений меньше размера сообщения)
        //Если размер изображения больше сообщения - разделяем изображение на несколько
        do
        {
            result = await _dbContext.restore_snap_shot_images(OffSet, MaxMessageSizeMb, typedItem.RestoreDate).ToListAsync();
            OffSet += result.Count();
            var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
            foreach (var item in result)
            {
                var ImageBase64 = Convert.ToBase64String(item.image);
                MessagePartId = Guid.NewGuid();
                //если изображение меньше свободного размера сообщения - добавляем его в сообщение
                //сообщение отсылаем
                var freeMessageSize = MaxMessageSizeMb * 1000000;
                var imageLastPart = item.image.Length;
                if (item.image.Length <= freeMessageSize)
                {
                    listItems.DataObjects.Add
                    (
                        new DataForProcessingService
                        {
                            ItemId = item.itemid.Value,
                            DataType = nameof(ImageItem),
                            Data = JsonSerializer.Serialize(new ImageDTO
                            {
                                ItemId = item.itemid.Value,
                                Image = ImageBase64,
                                UserLogin = item.userlogin
                            }),
                            CRUD = CRUD.Create,
                            MessagePartCounts = 1,
                            MessagePartId = Guid.NewGuid(),
                            MessagePartNumber = 1,
                        }
                    );
                    sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, context.Message.CorrelationId);
                    sendObject.GetType().GetProperty("DataItems").SetValue(sendObject, listItems);
                    sendObject.GetType().GetProperty("AllItemsCount").SetValue(sendObject, AllItemsCount);
                    sendObject.GetType().GetProperty("BatchCount").SetValue(sendObject, OffSet);
                    await _publishEndpoint.Publish(sendObject);
                    listItems.DataObjects.Clear();
                    MessagePartCounts = 0;
                    MessagePartNumber = 0;
                    continue;
                }
                //изображение не влезает в сообщение - заполняем сообщение и посылаем сообщение
                //для последующего сбора в сервисе ImageService
                else
                {
                    MessagePartId = Guid.NewGuid();
                    var splitPointer = 0;
                    //цикл по разбиванию изображения на части
                    do
                    {
                        //если длина оставшейся части изображения больше свободной части сообщения - 
                        //добавляем эту часть изображения в коллекцию частей изображения и посылаем сообщение
                        partsMessageList.Add
                        (
                            new DataForProcessingService
                            {
                                ItemId = item.itemid.Value,
                                DataType = nameof(ImageItem),
                                Data = JsonSerializer.Serialize(new ImageDTO
                                {
                                    ItemId = item.itemid.Value,
                                    Image = ImageBase64.Substring(splitPointer,
                                    imageLastPart > freeMessageSize ? freeMessageSize : imageLastPart),
                                    UserLogin = item.userlogin
                                }),
                                CRUD = CRUD.Create,
                                MessagePartCounts = 0,
                                MessagePartId = MessagePartId,
                                MessagePartNumber = MessagePartNumber + 1
                            }
                        );
                        splitPointer += imageLastPart > freeMessageSize ? freeMessageSize : imageLastPart;
                        MessagePartNumber++;
                        MessagePartCounts++;
                        imageLastPart = ImageBase64.Length - splitPointer;
                    } while (ImageBase64.Length > splitPointer);
                    //обновляем общее количество частей
                    foreach (var part in partsMessageList)
                    {
                        part.MessagePartCounts = MessagePartCounts;
                        listItems.DataObjects.Add(part);
                        //посылаем заполненную часть
                        sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, context.Message.CorrelationId);
                        sendObject.GetType().GetProperty("DataItems").SetValue(sendObject, listItems);
                        sendObject.GetType().GetProperty("AllItemsCount").SetValue(sendObject, AllItemsCount);
                        sendObject.GetType().GetProperty("BatchCount").SetValue(sendObject, OffSet);
                        await _publishEndpoint.Publish(sendObject);
                        listItems.DataObjects.Clear();
                    }

                    //сбрасываем параметры сообщения
                    splitPointer = 0;
                    MessagePartCounts = 0;
                    MessagePartNumber = 0;
                    partsMessageList.Clear();
                }
            }
        } while (AllItemsCount > OffSet);
    }
}