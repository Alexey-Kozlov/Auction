using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers;

public class AuctionUpdatedConsumer : IConsumer<AuctionUpdated>
{
    private readonly IMapper _mapper;

    public AuctionUpdatedConsumer(IMapper mapper)
    {
        _mapper = mapper;
    }
    public async Task Consume(ConsumeContext<AuctionUpdated> context)
    {
        var item = _mapper.Map<Item>(context.Message);
        var result = await DB.Update<Item>()
            .Match(p => p.ID == context.Message.Id)
            .ModifyOnly(p => new
            {
                p.Color,
                p.Make,
                p.Model,
                p.Year,
                p.Mileage,
                p.Image
            }, item)
            .ExecuteAsync();
            
        if(!result.IsAcknowledged)
        {
            throw new MessageException(typeof(AuctionUpdated), "Ошибка обновления записи в MongoDb");
        }
        Console.WriteLine("--> Получение сообщения обновить аукцион");
    }
}
