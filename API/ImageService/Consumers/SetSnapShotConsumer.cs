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
        await _locker.LockAsync(async () =>
        {
            var listItems = new DataForProcessingServicesList
            {
                DataObjects = new List<DataForProcessingService>()
            };
            //получаем количество записей
            var typedItem = new RequestRestoreImages
            {
                MaxMessageSizeMb = 0,
                StartNumber = 0
            };
            //счетчик номеров частей передаваемого сообщения
            var MessagePartNumber = 0;
            //общее количество частей передаваемого сообщения
            var MessagePartCounts = 0;
            //идентификатор передаваемого сообщения
            var MessagePartId = Guid.NewGuid();
            var result = await _dbContext.get_snap_shot_images(JsonSerializer.Serialize(typedItem)).ToListAsync();
            var AllItemsCount = int.Parse(result[0].entitytype);
            typedItem.MaxMessageSizeMb = int.Parse(_configuration["MaxMessageSizeMb"]);
            var partsMessageList = new List<DataForProcessingService>();
            //получаем батчи в цикле, размером не больше MaxMessageSizeMb
            do
            {
                var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                    _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
                result = await _dbContext.get_snap_shot_images(JsonSerializer.Serialize(typedItem)).ToListAsync();
                typedItem.StartNumber += result.Count();
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
                                    CRUD = CRUD.Create,
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
        });
    }
}

