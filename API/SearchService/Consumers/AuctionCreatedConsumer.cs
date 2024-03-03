using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers;

public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
{
    private readonly IMapper _mapper;

    public AuctionCreatedConsumer(IMapper mapper)
    {
        _mapper = mapper;
    }
    public async Task Consume(ConsumeContext<AuctionCreated> context)
    {
        var item = _mapper.Map<Item>(context.Message);
        //для тестирования обработки ошибок из очереди
        if(item.Model == "Error") throw new ArgumentException("Ошибка машины!");
        await item.SaveAsync();
        Console.WriteLine("--> Получение сообщения создать аукцион");
    }
}
