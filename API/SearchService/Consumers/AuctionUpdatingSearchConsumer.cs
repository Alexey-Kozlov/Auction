using AutoMapper;
using Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SearchService.Data;
using SearchService.Entities;

namespace SearchService.Consumers;

public class AuctionUpdatingSearchConsumer : IConsumer<AuctionUpdatingSearch>
{
    private readonly IMapper _mapper;
    private readonly SearchDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuctionUpdatingSearchConsumer(IMapper mapper, SearchDbContext context, IPublishEndpoint publishEndpoint)
    {
        _mapper = mapper;
        _context = context;
        _publishEndpoint = publishEndpoint;
    }
    public async Task Consume(ConsumeContext<AuctionUpdatingSearch> context)
    {
        Console.WriteLine("--> Получение сообщения обновить аукцион");
        var updatedItem = _mapper.Map<Item>(context.Message);
        var item = await _context.Items.FirstOrDefaultAsync(p => p.Id == updatedItem.Id);
        if (item != null)
        {
            _mapper.Map(updatedItem, item);
            await _context.SaveChangesAsync();
            await _publishEndpoint.Publish(new AuctionUpdatedSearch(context.Message.CorrelationId));
            Console.WriteLine($"{DateTime.Now} - Аукцион {updatedItem.Id} успешно обновлен.");
            return;
        }
        Console.WriteLine("Ошибка обновления записи - запись " + updatedItem.Id + " не найдена.");
    }
}
