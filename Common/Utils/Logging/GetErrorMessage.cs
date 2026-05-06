using System.Text;
using System.Text.Json;

namespace Common.Utils.Logging;

public class GetErrorMessage
{
    private const int dataStringLength = 100000; //максимальная длина сериализованного объекта данных
    public static string GetMessage(Exception e)
    {
        var exceptionMessage = new StringBuilder();
        if (!string.IsNullOrEmpty(e.Message))
        {
            exceptionMessage.Append(e.Message);
        }
        GetInnerException(e, exceptionMessage);
        return exceptionMessage.ToString();
    }

    public static StringBuilder GetInnerException(Exception e, StringBuilder message)
    {

        if (e.InnerException != null)
        {
            if (!string.IsNullOrEmpty(e.InnerException.Message))
            {
                message.Append($", InnerException - {e.InnerException.Message}");
            }
            return GetInnerException(e.InnerException, message);
        }
        else
        {
            return message;
        }
    }

    public static string GetExceptionStringData(object data)
    {
        var dataString = JsonSerializer.Serialize(data);
        if (dataString.Length > dataStringLength)
        {
            return "Слишком большой входной объект данных";
        }
        return dataString;
    }
}