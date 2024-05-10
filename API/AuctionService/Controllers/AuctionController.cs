using System.Security.Claims;
using AuctionService.Data;
using AuctionService.DTO;
using AuctionService.Entities;
using AutoMapper;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Common.Utils;

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
    public async Task<ApiResponse<List<AuctionDTO>>> GetAllAuctions(string date)
    {
        var query = _context.Auctions
            .Include(p => p.Item)
            .OrderBy(p => p.Item.First().Title).AsQueryable();
        if (!string.IsNullOrEmpty(date))
        {
            query = query.Where(p => p.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);
        }
        return new ApiResponse<List<AuctionDTO>>()
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            IsSuccess = true,
            Result = _mapper.Map<List<AuctionDTO>>(await query.ToListAsync())
        };
    }

    [HttpGet("{id}")]
    public async Task<ApiResponse<AuctionDTO>> GetAuctionById(Guid id)
    {
        var auction = await _context.Auctions
            .Include(p => p.Item)
            .FirstOrDefaultAsync(p => p.Id == id);

        return new ApiResponse<AuctionDTO>()
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            IsSuccess = true,
            Result = _mapper.Map<AuctionDTO>(auction ?? new Auction())
        };
    }

    [Authorize]
    [HttpPost]
    public async Task<ApiResponse<object>> CreateAuction([FromBody] CreateAuctionDTO auctionDto)
    {
        var auction = _mapper.Map<Auction>(auctionDto);
        var auctionAuthor = ((ClaimsIdentity)User.Identity).Claims.Where(p => p.Type == "Login").Select(p => p.Value).FirstOrDefault();
        auction.Seller = auctionAuthor;
        _context.Auctions.Add(auction);

        var newAuction = _mapper.Map<AuctionDTO>(auction);
        if (!string.IsNullOrEmpty(auctionDto.Image))
        {
            newAuction.Image = auctionDto.Image;
        }
        newAuction.AuctionAuthor = auctionAuthor;
        await _publishEndpoint.Publish(_mapper.Map<AuctionCreated>(newAuction));
        await _context.SaveChangesAsync();

        return new ApiResponse<object>()
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            IsSuccess = true,
            Result = null
        };
    }

    // [Authorize]
    // [HttpDelete("{id}")]
    // public async Task<ApiResponse<object>> DeleteAuction(Guid id)
    // {
    //     var auction = await _context.Auctions.FindAsync(id);
    //     var auctionAuthor = ((ClaimsIdentity)User.Identity).Claims.Where(p => p.Type == "Login").Select(p => p.Value).FirstOrDefault();
    //     if (auction == null)
    //     {
    //         throw new Exception("Запись не найдена");
    //     };

    //     if (auction.Seller != auctionAuthor)
    //     {
    //         throw new Exception("Удалить аукцион может только автор аукциона");
    //     };
    //     _context.Auctions.Remove(auction);

    //     await _publishEndpoint.Publish(new AuctionDeleted(id, auctionAuthor, new Guid()));

    //     await _context.SaveChangesAsync();
    //     return new ApiResponse<object>()
    //     {
    //         StatusCode = System.Net.HttpStatusCode.OK,
    //         IsSuccess = true,
    //         Result = new { data = "Ok" }
    //     };
    // }
}
