using Common.Contracts.Logging;
using GatewayService.Services;
using MassTransit;

namespace GatewayService.Logging;

public class SendMessage
{
    private readonly ITopicProducer<ItemLoggingContract> _topicProducer;
    private readonly UserCurrentPage _userCurrentPage;

    public SendMessage(ITopicProducer<ItemLoggingContract> topicProducer, UserCurrentPage userCurrentPage)
    {
        _topicProducer = topicProducer;
        _userCurrentPage = userCurrentPage;
    }

    public async Task SendLogTopic(ItemLoggingContract message)
    {
        await _topicProducer.Produce(message);
        /*
        Отслеживаем поступление запросов типом RequestType = UsersCurrentPage, записываем значение страницы в кеш Редиса        
        Нужно для типов запросов - ReadDetail или Communications, для отслеживания КРАЙНЕГО
        перехода пользователя на страницы просмотра аукциона - там есть ставки, 
        либо внутри просмотра аукциона на страницу чата - там есть сообщения пользователей. 
        В обоих случаях сохраняем информацию о переходе пользователя на эти страницы в кеше чтобы потом
        пересылать уведомления об обновлении интерфейса адресно этим пользователям.
        */
        if (message.RequestType == "UsersCurrentPage")
        {
            await _userCurrentPage.SetUserForCurrentPage(message.UserLogin, message.RequestLoggingContract.Body);
        }
    }
}