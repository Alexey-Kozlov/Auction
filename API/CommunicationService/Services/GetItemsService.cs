using AutoMapper;
using Common.Contracts;
using CommunicationService.Data;
using CommunicationService.DTO;
using Microsoft.EntityFrameworkCore;

namespace CommunicationService.Services;

public class GetItemsService
{
    private readonly CommunicationDbContext _dbContext;
    private readonly IMapper _mapper;

    public GetItemsService(CommunicationDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<ApiResponse<List<CommunicationDTO>>> GetCommunicationItems(string auctionId)
    {
        //сортируем записи потом, на клиенте
        var items = await _dbContext.Communications.Where(p => p.Commited && p.AuctionId == Guid.Parse(auctionId))
            .ToListAsync();
        return new ApiResponse<List<CommunicationDTO>>()
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            IsSuccess = true,
            Result = items.Select(_mapper.Map<CommunicationDTO>).ToList()
        };
    }

}