using System.Text;
using System.Text.Json;
using Common.Contracts.Image;
using Common.Contracts.Processing;

namespace ImageService.Services;

public class RestoreImageService
{
    private List<DataForProcessingService> PartsList = new List<DataForProcessingService>();

    public string GetImageString(DataForProcessingService part)
    {
        if (part.MessagePartCounts == 1)
        {
            //изображение влезло в сообщение, возвращаем строку
            //return JsonSerializer.Deserialize<ImageDTO>(part.Data).Image;
            return part.Data;
        }
        else
        {
            //изображение не влезло в сообщение, накапливаем части сообщения пока не соберутся все части
            PartsList.Add(part);
            var parts = PartsList.Where(p => p.MessagePartId == part.MessagePartId);
            if (parts.Count() == part.MessagePartCounts)
            {
                //все части изображения собраны - создаем итоговое изображение
                var image = new StringBuilder();
                foreach (var item in parts.OrderBy(p => p.MessagePartNumber))
                {
                    image.Append(item.Data);
                }
                //освобождаем ресурсы
                PartsList.RemoveAll(p => p.MessagePartId == part.MessagePartId);
                //собрали в одну строку все части сообщения
                return image.ToString();
            }
            return "";
        }
    }
}