using System.Text;
using Common.Contracts.Processing;

namespace EventSourcingService.Services;

public class RestoreImageService
{
    private List<DataForProcessingService> PartsList;

    public RestoreImageService()
    {
        PartsList = new List<DataForProcessingService>();
    }

    public string GetImageString(DataForProcessingService part)
    {
        //изображение не влезло в сообщение, накапливаем части сообщения пока не соберутся все части
        //PartsList.Add(part);
        PartsList.Add(part);
        var parts = PartsList.Where(p => p.MessagePartId == part.MessagePartId);
        if (parts.Count() == part.MessagePartCounts)
        {
            //все части изображения собраны - сохранем в БД
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