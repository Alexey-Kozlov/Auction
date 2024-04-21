using ImageService.Data;
using AutoMapper;
using Common.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ImageService.Controllers;

[ApiController]
[Route("api/images")]
public class ImageController : ControllerBase
{
    private readonly ImageDbContext _context;
    private readonly IMapper _mapper;

    public ImageController(ImageDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ApiResponse<List<ImageDTO>>> GetAllAuctions()
    {
        return new ApiResponse<List<ImageDTO>>()
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            IsSuccess = true,
            Result = _mapper.Map<List<ImageDTO>>(await _context.Images.ToListAsync())
        };
    }

    [HttpGet("{id}")]
    public async Task<ApiResponse<ImageDTO>> GetAuctionById(Guid id)
    {
        var image = await _context.Images.FirstOrDefaultAsync(p => p.Id == id);

        return new ApiResponse<ImageDTO>()
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            IsSuccess = true,
            Result = _mapper.Map<ImageDTO>(image)
        };
    }

}
