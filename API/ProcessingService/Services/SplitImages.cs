using System.Text.Json;
using Common.Contracts.Auction;
using Common.Contracts.Image;
using Common.Contracts.Processing;
using MassTransit;

namespace ProcessingService.Services;

public class SplitImages
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _configuration;

    public SplitImages(IPublishEndpoint publishEndpoint, IConfiguration configuration)
    {
        _publishEndpoint = publishEndpoint;
        _configuration = configuration;
    }

    public async Task ProcessImage<T>(T auction) where T : IAuctionImageSplit
    {
        var freeMessageSize = int.Parse(_configuration["MaxMessageSizeMb"]) * 1000000;

        //есть изображение в аукционе и оно большое
        if (auction.UsingImage && auction.Image.Length > freeMessageSize)
        {
            //разбиваем изображение на части, чтобы не перегружать сервисный брокер большими сообщениями
            auction.IsImageSplitted = true;
            var splitPointer = 0;
            var MessagePartNumber = 0;
            var MessagePartCounts = 0;
            var imageLastPart = auction.Image.Length;
            var MessagePartId = Guid.NewGuid();
            var image = auction.Image;
            var partsMessageList = new List<DataForProcessingService>();
            do
            {
                partsMessageList.Add
                (
                    new DataForProcessingService
                    {
                        DataType = nameof(ImageItem),
                        Data = image.Substring(splitPointer,
                                imageLastPart > freeMessageSize ? freeMessageSize : imageLastPart),

                        CRUD = 0,
                        MessagePartCounts = 0,
                        MessagePartId = MessagePartId,
                        MessagePartNumber = MessagePartNumber + 1
                    }
                );
                splitPointer += imageLastPart > freeMessageSize ? freeMessageSize : imageLastPart;
                MessagePartNumber++;
                MessagePartCounts++;
                imageLastPart = image.Length - splitPointer;
            } while (image.Length > splitPointer);
            //обновляем общее количество частей
            foreach (var part in partsMessageList)
            {
                part.MessagePartCounts = MessagePartCounts;
                auction.CorrelationId = Guid.NewGuid();
                auction.Image = JsonSerializer.Serialize(part);
                //посылаем заполненную часть
                await _publishEndpoint.Publish(auction);
            }
        }
        else
        {
            //нет изображения или оно небольшое
            auction.Image = JsonSerializer.Serialize(new DataForProcessingService
            {
                DataType = nameof(ImageItem),
                Data = auction.UsingImage ? auction.Image : "",
                CRUD = 0,
                MessagePartCounts = 1,
                MessagePartId = Guid.NewGuid(),
                MessagePartNumber = 1
            });

            auction.IsImageSplitted = false;
            await _publishEndpoint.Publish((T)auction);
        }
    }
}