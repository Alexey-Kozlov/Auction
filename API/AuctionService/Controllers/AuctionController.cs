using AuctionService.Data;
using AuctionService.DTO;
using AuctionService.Entities;
using AutoMapper;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Common.Utils;
using Microsoft.AspNetCore.Authorization;
using AuctionService.Commands;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Common.Contracts;
using System.Security.Claims;
using AuctionService.Bus;
using MassTransit.DependencyInjection;
using Confluent.Kafka;

namespace AuctionService.Controllers;
[Route("api/auctions")]
[ApiController]
public class AuctionController : ControllerBase
{
    private readonly AuctionDbContext _context;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpointRabbit;
    private readonly ICommandDispatcher _commandDispatcher;
    private readonly ILogger<AuctionController> _logger;
    private readonly Bind<ISecondBus, IPublishEndpoint> _publishEndpointKafka;
    private readonly ITopicProducer<IMessage> _topicProducer;


    public AuctionController(AuctionDbContext context, IMapper mapper,
        IPublishEndpoint publishEndpointRabbit, ICommandDispatcher commandDispatcher,
        ILogger<AuctionController> logger, ITopicProducer<IMessage> topicProducer)
    {
        _context = context;
        _mapper = mapper;
        _publishEndpointRabbit = publishEndpointRabbit;
        _commandDispatcher = commandDispatcher;
        _logger = logger;
        _topicProducer = topicProducer;
    }


    //[Authorize]
    [HttpPost("createauction")]
    public async Task CreateAuction([FromBody] CreateAuctionDTO par)
    {
        var cmd = _mapper.Map<CreateAuctionCommand>(par);
        cmd.EditUser = ((ClaimsIdentity)User.Identity).Claims.Where(p => p.Type == "Login").Select(p => p.Value).FirstOrDefault();
        await _commandDispatcher.SendAsync(cmd);
    }

    //[Authorize]
    [HttpPost("updateauction")]
    public async Task UpdateAuction([FromBody] UpdateAuctionDTO par)
    {
        var cmd = _mapper.Map<UpdateAuctionCommand>(par);
        cmd.EditUser = ((ClaimsIdentity)User.Identity).Claims.Where(p => p.Type == "Login").Select(p => p.Value).FirstOrDefault();
        await _commandDispatcher.SendAsync(cmd);
    }

    //[Authorize]
    [HttpPost("deleteauction")]
    public async Task DeleteAuction([FromBody] DeleteAuctionDTO par)
    {
        var cmd = new DeleteAuctionCommand
        {
            EditUser = ((ClaimsIdentity)User.Identity).Claims.Where(p => p.Type == "Login").Select(p => p.Value).FirstOrDefault(),
            Id = par.Id,
            Type = typeof(DeleteAuctionCommand).ToString()
        };
        await _commandDispatcher.SendAsync(cmd);
    }

    [HttpPost("convert")]
    public async Task<string> Convert()
    {
        var i = 0;
        JsonSerializerOptions options = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        foreach (var item in await _context.Auctions.Include(p => p.Item).ToListAsync())
        {
            //если уже перенесли запись - пропускаем
            if (await _context.EventsLogs.FirstOrDefaultAsync(p => p.Id == item.Id) != null)
            {
                continue;
            }
            i++;
            var cmd = _mapper.Map<TransferAuctionCommand>(item);
            _context.EventsLogs.Add(new EventsLog
            {
                Id = item.Id,
                CreateAt = item.CreateAt,
                Aggregate = JsonDocument.Parse(JsonSerializer.Serialize(cmd, cmd.GetType(), options))
            });
        }
        await _context.SaveChangesAsync();
        return await Task.FromResult("Обработано " + i.ToString() + " записей.");
    }
}
