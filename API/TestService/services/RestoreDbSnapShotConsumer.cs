namespace SearchService.Consumers;

public class RestoreDbSnapShotConsumer
{

    public void Consume()
    {
        //делаем generic-тип вида RestoreSnapShotItems<наименование проекта>
        //нужно для автоматической передачи сообщений на нужный консьюмер в нужном проекте

        //либо, как вариант - проверять тип и ручками делать нужный тип объекта для рассылки
        //оставил так, на вид сложнее, но более универсальный - не потребуется переписывать при 
        //рассылке сообщений для новых консьюмеров
        Type elementType = Type.GetType("Common.Contracts.Search,Contracts", true);
        Type[] types = { elementType };
        Type baseType = typeof(List<>);
        Type sendType = baseType.MakeGenericType(types);
        //через рефлексию делаем нужный generic-тип
        var sendObject = Activator.CreateInstance(sendType);
        //через рефлексию заполняем нужные свойства
        //можно было назначить интерфейс и заполнять не через рефлексию, это как другой вариант.
        sendObject.GetType().GetProperty("UserLogin").SetValue(sendObject, "UserLogin");
        sendObject.GetType().GetProperty("Items").SetValue(sendObject, new List<string>());


    }
}
