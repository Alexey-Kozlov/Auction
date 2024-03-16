using AuctionService.Data;
using AutoMapper;
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
    public async Task<ActionResult<List<ImageDTO>>> GetAllAuctions()
    {
        var dd = await _context.Images.ToListAsync();
        return _mapper.Map<List<ImageDTO>>(await _context.Images.ToListAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ImageDTO>> GetAuctionById(Guid id)
    {
        var image = await _context.Images.FirstOrDefaultAsync(p => p.Id == id);

        if (image == null) return new ImageDTO { Id = "", Image = "" };

        return _mapper.Map<ImageDTO>(image);
    }

}
