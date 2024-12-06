using System.Reflection;
using System.Text.Json;
using Common.Contracts.EventSourcing;
using Common.Contracts.Image;
using Common.Contracts.Processing;
using ImageService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ImageService.Consumers;

public class SetSnapShotConsumer : IConsumer<ESContract>
{
    private readonly ImageDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;

    public SetSnapShotConsumer(ImageDbContext dbContext,
        IPublishEndpoint publishEndpoint, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _configuration = configuration;
    }
    public async Task Consume(ConsumeContext<ESContract> context)
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

        var result = await _dbContext.get_snap_shot_images(JsonSerializer.Serialize(typedItem)).ToListAsync();
        var AllItemsCount = int.Parse(result[0].entitytype);
        typedItem.MaxMessageSizeMb = int.Parse(_configuration["MaxMessageSizeMb"]);
        //получаем батчи в цикле, размером не больше MaxMessageSizeMb
        do
        {
            result = await _dbContext.get_snap_shot_images(JsonSerializer.Serialize(typedItem)).ToListAsync();
            typedItem.StartNumber += result.Count();
            foreach (var item in result)
            {
                listItems.DataObjects.Add
                (
                    new DataForProcessingService
                    {
                        DataType = nameof(ImageItem),
                        Data = item.eventdata,
                        CRUD = CRUD.Create
                    }
                );
            }
            var sendObject = Assembly.LoadFrom(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                _configuration["CommonAssembly"]).CreateInstance(context.Message.CallBackType);
            sendObject.GetType().GetProperty("CorrelationId").SetValue(sendObject, context.Message.CorrelationId);
            sendObject.GetType().GetProperty("DataItems").SetValue(sendObject, listItems);
            sendObject.GetType().GetProperty("AllItemsCount").SetValue(sendObject, AllItemsCount);

            await _publishEndpoint.Publish(sendObject);
            listItems.DataObjects.Clear();

        } while (AllItemsCount > typedItem.StartNumber);

    }
}

