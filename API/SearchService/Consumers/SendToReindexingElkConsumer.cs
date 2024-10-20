using AutoMapper;
using Common.Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SearchService.Data;
using SearchService.Services;

namespace SearchService.Consumers;

public class SendToReindexingElkConsumer : IConsumer<SendAllItems<SendToReindexingElk>>
{
    private readonly IMapper _mapper;
    private readonly SearchDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ElkReindexingService _elkReindexingService;

    public SendToReindexingElkConsumer(IMapper mapper, SearchDbContext context, IPublishEndpoint publishEndpoint,
        ElkReindexingService elkReindexingService)
    {
        _mapper = mapper;
        _context = context;
        _publishEndpoint = publishEndpoint;
        _elkReindexingService = elkReindexingService;
    }
    public async Task Consume(ConsumeContext<SendAllItems<SendToReindexingElk>> consumeContext)
    {
        var sendObject = new SendToReindexingElk();
        _mapper.Map(await _context.Items.ToListAsync(), sendObject.AuctionItems);
        sendObject.SessionId = consumeContext.Message.SessionId;
        sendObject.UserLogin = consumeContext.Message.UserLogin;
        await _elkReindexingService.ReindexElkItems(sendObject);
        Console.WriteLine("--> Получение сообщения передать все записи из БД");
    }
}
