using System.Reflection;
using System.Text.Json;
using Common.Contracts.EventSourcing;
using Common.Contracts.Image;
using Common.Contracts.Processing;
using Common.Utils;
using ImageService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ImageService.Consumers;

public class SetSnapShotConsumer : IConsumer<ESContract>
{
    private readonly ImageDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;
    private static readonly AwaitLocker _locker = new AwaitLocker();

    public SetSnapShotConsumer(ImageDbContext dbContext,
        IPublishEndpoint publishEndpoint, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _configuration = configuration;
    }
    public async Task Consume(ConsumeContext<ESContract> context)
    {
        try
        {
            await _locker.LockAsync(async () =>
            {
                var listItems = new DataForProcessingServicesList
                {
                    DataObjects = new List<DataForProcessingService>()
                };

                var OffSet = 0;

                //счетчик номеров частей передаваемого сообщения
                var MessagePartNumber = 0;
                //общее количество частей передаваемого сообщения
                var MessagePartCounts = 0;
                //идентификатор передаваемого сообщения
                var MessagePartId = Guid.NewGuid();
                //получаем общее количество записей в таблице "ImageItems"
                var result = await _dbContext.get_snap_shot_images(0, 0).ToListAsync();
                var AllItemsCount = result[0].recordscount;
                if (AllItemsCount == 0)
                {
                    //если в таблице ImageItems не было ни одной записи для создания снимка
                    var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                        _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
                    sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, context.Message.CorrelationId);
                    sendObject.GetType().GetProperty("AllItemsCount").SetValue(sendObject, AllItemsCount);
                    await _publishEndpoint.Publish(sendObject);
                    return;
                }
                var MaxMessageSizeMb = int.Parse(_configuration["MaxMessageSizeMb"]);
                var partsMessageList = new List<DataForProcessingService>();
                //получаем батчи в цикле, размером не больше MaxMessageSizeMb
                do
                {
                    var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                        _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
                    result = await _dbContext.get_snap_shot_images(OffSet, MaxMessageSizeMb).ToListAsync();
                    OffSet += result.Count();
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
                                    Id = item.id ?? Guid.NewGuid(),
                                    DataType = nameof(ImageItem),
                                    Data = JsonSerializer.Serialize(new ImageDTO
                                    {
                                        Id = item.id,
                                        AuctionId = item.auctionid ?? Guid.NewGuid(),
                                        Image = ImageBase64
                                    }),
                                    CRUD = CRUD.Create,
                                    MessagePartCounts = 1,
                                    MessagePartId = Guid.NewGuid(),
                                    MessagePartNumber = 1
                                }
                            );
                            sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, context.Message.CorrelationId);
                            sendObject.GetType().GetProperty("DataItems").SetValue(sendObject, listItems);
                            sendObject.GetType().GetProperty("AllItemsCount").SetValue(sendObject, AllItemsCount);
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
                                        Id = item.id ?? Guid.NewGuid(),
                                        DataType = nameof(ImageItem),
                                        Data = JsonSerializer.Serialize(new ImageDTO
                                        {
                                            Id = item.id,
                                            AuctionId = item.auctionid ?? Guid.NewGuid(),
                                            Image = ImageBase64.Substring(splitPointer,
                                                imageLastPart > freeMessageSize ? freeMessageSize : imageLastPart)
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
            });
        }
        catch (Exception e)
        {
            //ошибки, в т.ч. штатные
            var messageObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
            messageObject.GetType().GetProperty("CorrelationId").SetValue(messageObject, context.Message.CorrelationId);
            messageObject.GetType().GetProperty("ErrorMessage").SetValue(messageObject, e.Message);
            messageObject.GetType().GetProperty("ErrorExceptionMessage").SetValue(messageObject, e.StackTrace);
            messageObject.GetType().GetProperty("ErrorServiceName").SetValue(messageObject, "ImageService_SetSnapShot");
            messageObject.GetType().GetProperty("UserLogin").SetValue(messageObject, "");
            messageObject.GetType().GetProperty("AuctionId").SetValue(messageObject, null);
            messageObject.GetType().GetProperty("IsError").SetValue(messageObject, true);
            var faultType = typeof(FaultMessage<>);
            var typeParams = new Type[] { messageObject.GetType() };
            var faultObjectType = faultType.MakeGenericType(typeParams);
            var faultObject = Activator.CreateInstance(faultObjectType, new object[] { messageObject });

            await _publishEndpoint.Publish(faultObject.GetType().GetMethod("CastItem").Invoke(faultObject, null));
        }
    }
}

