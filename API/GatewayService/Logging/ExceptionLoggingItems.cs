namespace GatewayService.Logging;

public class ExceptionLoggingItems
{
    //условия для записи запроса в лог
    public static bool CheckPathToInclude(string path)
    {
        if (path.Contains(".ico")) return true; //исключаем из иконки
        if (path.Contains(".js")) return true; //исключаем скрипты
        if (path.Contains("/negotiate")) return true; //исключаем синхронизацию
        if (path.Contains("cache=true")) return true; //исключаем обращения к кешу
        return false;
    }
}