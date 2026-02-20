using Common.Contracts;
using Common.Contracts.Settings;
using Microsoft.EntityFrameworkCore;
using SettingsService.Data;

namespace SettingsService.Services;

public class CurrentSettingsService
{
    private readonly SettingsDbContext _context;

    public CurrentSettingsService(SettingsDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<CurrentItem>> GetCurrentSettings()
    {
        var currentSettings = await _context.Current.FirstOrDefaultAsync();

        return new ApiResponse<CurrentItem>
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            IsSuccess = true,
            Result = new CurrentItem()
            {
                AdminMode = currentSettings.AdminMode
            }
        };
    }
}
