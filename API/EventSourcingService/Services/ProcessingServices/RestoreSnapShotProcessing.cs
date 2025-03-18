using System.Reflection;
using System.Text.Json;
using Common.Contracts.Auction;
using Common.Contracts.EventSourcing;
using Common.Contracts.Image;
using Common.Contracts.Processing;
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
        var listItems = new DataForProcessingServicesList
        {
            DataObjects = new List<DataForProcessingService>()
        };
        switch (context.Message.EntityType)
        {
            case nameof(ESLogResetSnapShot):
                //Выполняем удаление всех записей в BiddingService,FinanceService,NotificationService,
                //SearchService,ImageService
                _dbContext.Database.ExecuteSqlRaw("Call public.reset_snap_shot()", new object[] { });
                break;
            case nameof(RequestRestoreItems):
                //В процедуре Postgres делаем:
                //- запись в ES лог о выполнении восстановления БД из лога
                //- набор записей о восстановлении записей ставок для сервисов BiddingService,FinanceService,NotificationService,
                //SearchService. Для ImageService - отдельно
                var result = await _dbContext.restore_snap_shot_items(
                    context.Message.CorrelationId,
                    context.Message.EventData,
                    context.Message.UserLogin).ToListAsync();
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
                    if (item.entitytype == "AuctionItem")
                    {
                        var auction = JsonSerializer.Deserialize<AuctionItem>(item.eventdata);
                        await _checkAuctionFinished.UpdateFinishTasks(auction.AuctionId,
                         auction.AuctionEnd, CRUD.Create);
                    }
                }
                break;
            case nameof(RequestRestoreImages):
                //получаем изображения из ESLog
                await GetESLogImages(context);
                return;
            default:
                break;
        }

        var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
            _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
        sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, context.Message.CorrelationId);
        sendObject.GetType().GetProperty("DataItems").SetValue(sendObject, listItems);
        sendObject.GetType().GetProperty("BatchCount").SetValue(sendObject, -1);

        await _publishEndpoint.Publish(sendObject);
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
        //получаем общее количество записей
        var result = await _dbContext.restore_snap_shot_images(
            context.Message.CorrelationId,
            JsonSerializer.Serialize(typedItem),
            context.Message.UserLogin).ToListAsync();
        var AllItemsCount = int.Parse(result[0].entitytype);
        typedItem.MaxMessageSizeMb = int.Parse(_configuration["MaxMessageSizeMb"]);
        var partsMessageList = new List<DataForProcessingService>();
        //в цикле получаем записи общим размером не превышающим размер сообщения (если суммарный размер
        //изображений меньше размера сообщения)
        //Если размер изображения больше сообщения - разделяем изображение на несколько
        do
        {
            result = await _dbContext.restore_snap_shot_images(
            context.Message.CorrelationId,
            JsonSerializer.Serialize(typedItem),
            context.Message.UserLogin).ToListAsync();
            typedItem.StartNumber += result.Count();
            var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
            foreach (var item in result)
            {
                var typed_data = JsonSerializer.Deserialize<ImageDTO>(item.eventdata);
                var item_image = typed_data.Image;
                MessagePartId = Guid.NewGuid();
                //если изображение меньше свободного размера сообщения - добавляем его в сообщение
                //сообщение отсылаем
                var freeMessageSize = typedItem.MaxMessageSizeMb * 1000000;
                var imageLastPart = item_image.Length;
                if (item_image.Length <= freeMessageSize)
                {
                    listItems.DataObjects.Add
                    (
                        new DataForProcessingService
                        {
                            DataType = nameof(ImageItem),
                            Data = JsonSerializer.Serialize(new ImageDTO
                            {
                                AuctionId = typed_data.AuctionId,
                                Image = item_image
                            }),
                            CRUD = 0,
                            MessagePartCounts = 1,
                            MessagePartId = Guid.NewGuid(),
                            MessagePartNumber = 1,
                            MessagePartSize = 0
                        }
                    );
                    sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, context.Message.CorrelationId);
                    sendObject.GetType().GetProperty("DataItems").SetValue(sendObject, listItems);
                    sendObject.GetType().GetProperty("AllItemsCount").SetValue(sendObject, AllItemsCount);
                    sendObject.GetType().GetProperty("BatchCount").SetValue(sendObject, typedItem.StartNumber);
                    await _publishEndpoint.Publish(sendObject);
                    listItems.DataObjects.Clear();
                    MessagePartCounts = 0;
                    MessagePartNumber = 0;
                    continue;
                }

                //изображение не влезает в сообщение - заполняем сообщение и посылаем сообщение
                //для последующего сбора в сервисе ImageService
                if (item_image.Length > freeMessageSize)
                {
                    var splitPointer = 0;
                    MessagePartId = Guid.NewGuid();

                    //цикл по разбиванию изображения на части
                    do
                    {
                        //если длина оставшейся части изображения больше свободной части сообщения - 
                        //добавляем эту часть изображения в коллекцию частей изображения и посылаем сообщение
                        partsMessageList.Add
                        (
                            new DataForProcessingService
                            {
                                DataType = nameof(ImageItem),
                                Data = JsonSerializer.Serialize(new ImageDTO
                                {
                                    AuctionId = typed_data.AuctionId,
                                    Image = item_image.Substring(splitPointer,
                                        imageLastPart > freeMessageSize ? freeMessageSize : imageLastPart)
                                }),
                                CRUD = 0,
                                MessagePartCounts = 0,
                                MessagePartId = MessagePartId,
                                MessagePartNumber = MessagePartNumber + 1
                            }
                        );
                        splitPointer += imageLastPart > freeMessageSize ? freeMessageSize : imageLastPart;
                        MessagePartNumber++;
                        MessagePartCounts++;
                        imageLastPart = item_image.Length - splitPointer;
                    } while (item_image.Length > splitPointer);
                    //обновляем общее количество частей
                    foreach (var part in partsMessageList)
                    {
                        part.MessagePartCounts = MessagePartCounts;
                        listItems.DataObjects.Add(part);
                        //посылаем заполненную часть
                        sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, context.Message.CorrelationId);
                        sendObject.GetType().GetProperty("DataItems").SetValue(sendObject, listItems);
                        sendObject.GetType().GetProperty("AllItemsCount").SetValue(sendObject, AllItemsCount);
                        sendObject.GetType().GetProperty("BatchCount").SetValue(sendObject, typedItem.StartNumber);
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
        } while (AllItemsCount > typedItem.StartNumber);

    }
}