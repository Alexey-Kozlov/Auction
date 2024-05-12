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
}
