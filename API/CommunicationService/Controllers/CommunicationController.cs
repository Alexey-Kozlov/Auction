using Common.Contracts;
using CommunicationService.DTO;
using CommunicationService.Services;
using Microsoft.AspNetCore.Mvc;

namespace CommunicationService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommunicationController : ControllerBase
{
    private readonly GetItemsService _itemsService;

    public CommunicationController(GetItemsService itemsService)
    {
        _itemsService = itemsService;
    }

    //при первоначальном открытии на клиенте списка сообщений чата
    [HttpGet("{auctionId:guid}")]
    public async Task<ApiResponse<List<CommunicationDTO>>> GetCommunicationItems([FromRoute] Guid auctionId)
    {
        return await _itemsService.GetCommunicationItems(auctionId);
    }
}
