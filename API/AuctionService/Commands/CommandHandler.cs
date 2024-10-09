
using System.Text.Json;
using System.Text.Json.Serialization;
using AuctionService.Data;
using AuctionService.Entities;
using Common.Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Commands;

public class CommandHandler : ICommandHandler
{
    private readonly AuctionDbContext _context;
    private readonly ITopicProducer<IMessage> _topicProducer;

    public CommandHandler(AuctionDbContext context, ITopicProducer<IMessage> topicProducer)
    {
        _context = context;
        _topicProducer = topicProducer;
    }

    public async Task HandlerAsync(UpdateAuctionCommand command)
    {
        var auction = await _context.Auctions.Include(p => p.Item).Where(p => p.Id == command.Id).FirstOrDefaultAsync();
        if (auction == null)
        {
            throw new Exception($"Запись {command.Title} для редактирования не найдена");
        }
        await SaveChangesAsync(command);
    }

    public async Task HandlerAsync(CreateAuctionCommand command)
    {
        await SaveChangesAsync(command);
    }

    public async Task HandlerAsync(DeleteAuctionCommand command)
    {
        var auction = await _context.Auctions.Include(p => p.Item).Where(p => p.Id == command.Id).FirstOrDefaultAsync();
        if (auction == null)
        {
            throw new Exception($"Запись {command.Id} для удаления не найдена");
        }
        await SaveChangesAsync(command);
    }

    private async Task SaveChangesAsync(BaseCommand command)
    {
        JsonSerializerOptions options = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        var aggregate = JsonSerializer.Serialize(command, command.GetType(), options);
        //добавляем запись в EventSourcing
        _context.EventsLogs.Add(new EventsLog
        {
            Id = Guid.NewGuid(),
            CreateAt = DateTime.Now.ToUniversalTime(),
            Aggregate = JsonDocument.Parse(aggregate)
        });
        await _context.SaveChangesAsync();
        //создаем новое сообщение в Kafka
        var message = new Message(aggregate);
        await SendEventAsync(message);
    }

    private async Task SendEventAsync(IMessage message)
    {
        await _topicProducer.Produce(message);
    }
}