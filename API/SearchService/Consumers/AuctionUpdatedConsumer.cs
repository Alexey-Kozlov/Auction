using AutoMapper;
using Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SearchService.Data;
using SearchService.Entities;

namespace SearchService.Consumers;

public class AuctionUpdatedConsumer : IConsumer<AuctionUpdated>
{
    private readonly IMapper _mapper;
    private readonly SearchDbContext _context;

    public AuctionUpdatedConsumer(IMapper mapper, SearchDbContext context)
    {
        _mapper = mapper;
        _context = context;
    }
    public async Task Consume(ConsumeContext<AuctionUpdated> consumeContext)
    {
        Console.WriteLine("--> Получение сообщения обновить аукцион");
        var updatedItem = _mapper.Map<Item>(consumeContext.Message);
        var item = await _context.Items.FirstOrDefaultAsync(p => p.Id == updatedItem.Id);
        if (item != null)
        {
            _mapper.Map(updatedItem, item);
            await _context.SaveChangesAsync();
            return;
        }
        Console.WriteLine("Ошибка обновления записи - запись " + updatedItem.Id + " не найдена.");
    }
}
