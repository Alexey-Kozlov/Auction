namespace GatewayService.Logging;

public class ExceptionLoggingItems
{
    //условия для записи запроса в лог
    public static bool CheckPathToInclude(string path)
    {
        if (path == "/") return true; //исключаем общий запрос
        if (path.Contains(".ico")) return true; //исключаем из иконки
        if (path.Contains(".js")) return true; //исключаем скрипты
        if (path.Contains("/negotiate")) return true; //исключаем синхронизацию
        if (path.Contains("/notifications")) return true; //исключаем уведомления
        if (path.Contains("/images")) return true; //исключаем изображения
        if (path.Contains("/auctions")) return true; //исключаем общие запросы аукционов
        if (path.Contains("/bids")) return true; //исключаем ставки
        if (path.Contains("/getbalance")) return true; //исключаем запрос баланса
        if (path.Contains("/finance/list")) return true; //исключаем общий запрос финансов
        if (path.EndsWith("/api/identity")) return true; //исключаем лишний запрос идентификации
        return false;
    }
}

