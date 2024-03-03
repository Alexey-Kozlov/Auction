using System.Text.Json;
using System.Text.Json.Serialization;
using AuctionService.Data;
using AuctionService.DTO;
using AuctionService.Entities;
using AutoMapper;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionController : ControllerBase
{
    private readonly AuctionDbContext _context;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuctionController(AuctionDbContext context, IMapper mapper, 
        IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuctionDTO>>> GetAllAuctions(string date)
    {
        var query = _context.Auctions
            .Include(p => p.Item)
            .OrderBy(p => p.Item.First().Make).AsQueryable();
        if(!string.IsNullOrEmpty(date))
        {
            query = query.Where(p => p.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);
        }
        return _mapper.Map<List<AuctionDTO>>(await query.ToListAsync());
        //return await query.ProjectTo<AuctionDTO>(_mapper.ConfigurationProvider).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDTO>> GetAuctionById(Guid id)
    {
        var auction = await _context.Auctions
            .Include(p => p.Item)
            .FirstOrDefaultAsync(p => p.Id == id);

        if(auction == null) return NotFound();

        return _mapper.Map<AuctionDTO>(auction);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<AuctionDTO>> CreateAuction([FromBody]CreateAuctionDTO auctionDto)
    {
        var auction = _mapper.Map<Auction>(auctionDto);
        auction.Seller = User.Identity.Name;
        _context.Auctions.Add(auction);

        var newAuction = _mapper.Map<AuctionDTO>(auction);
        
        await _publishEndpoint.Publish(_mapper.Map<AuctionCreated>(newAuction));

        var result = await _context.SaveChangesAsync() > 0;

        if(!result) return BadRequest("Error save auction");
        return CreatedAtAction(nameof(GetAuctionById), new {auction.Id}, newAuction);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAuction(Guid id,[FromBody] UpdateAuctionDTO updateAuctionDTO)
    {
        var auction = await _context.Auctions.Include(p => p.Item)
            .FirstOrDefaultAsync(p => p.Id == id);
        if(auction == null) return BadRequest("Запись не найдена");
        if(auction.Seller != User.Identity.Name) return Forbid();

        _mapper.Map(updateAuctionDTO, auction);
        var transferAuction = _mapper.Map<AuctionUpdated>(auction);
        await _publishEndpoint.Publish(transferAuction);

        var result = await _context.SaveChangesAsync() > 0;
        if(result) return Ok();
        return BadRequest("Ошибка обновления записи");
    }


    [Authorize]
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAuction(Guid id)
    {
        var auction = await _context.Auctions.FindAsync(id);
        if(auction == null) return BadRequest("Запись не найдена");

        if(auction.Seller != User.Identity.Name) return Forbid();
        _context.Auctions.Remove(auction);

        await _publishEndpoint.Publish<AuctionDeleted>(new {Id = id.ToString()});

        var result = await _context.SaveChangesAsync() > 0;
        if(result) return Ok();
        return BadRequest("Ошибка удаления записи");
    }

}
