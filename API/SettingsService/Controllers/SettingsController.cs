using Common.Contracts;
using Common.Contracts.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SettingsService.Services;

namespace SettingsService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SettingsController : ControllerBase
{
    private readonly CurrentSettingsService _currentSettingsService;

    public SettingsController(CurrentSettingsService currentSettingsService)
    {
        _currentSettingsService = currentSettingsService;
    }


    [HttpGet("GetCurrentSettings")]
    public async Task<ApiResponse<CurrentItem>> GetCurrentSettings()
    {
        //возвращаем текущие настройки системы
        return await _currentSettingsService.GetCurrentSettings();
    }
}