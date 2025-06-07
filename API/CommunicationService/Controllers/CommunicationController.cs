using AutoMapper;
using Common.Contracts;
using CommunicationService.Data;
using CommunicationService.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CommunicationService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommunicationController : ControllerBase
{
    private readonly CommunicationDbContext _context;
    private readonly IMapper _mapper;

    public CommunicationController(IMapper mapper, CommunicationDbContext context)
    {
        _context = context;
        _mapper = mapper;
    }


    [HttpGet("{auctionId}")]
    public async Task<ApiResponse<List<CommunicationDTO>>> GetCommunicationItems(string auctionId)
    {
        var items = await _context.Communications.Where(p => p.Commited && p.AuctionId == Guid.Parse(auctionId))
            .OrderByDescending(p => p.UpdateAt).ToListAsync();

        return new ApiResponse<List<CommunicationDTO>>()
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            IsSuccess = true,
            Result = items.Select(_mapper.Map<CommunicationDTO>).ToList()
        };
    }
}
