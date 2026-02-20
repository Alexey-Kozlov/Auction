using System.Net.Http.Json;
using Common.Contracts;
using Common.Contracts.Settings;
using Microsoft.Extensions.Configuration;

namespace Common.Utils.Settings;

//сервис синхронного вызова REST-сервиса - получаем данные настроек системы
public class SettingsHttpClient
{
    private readonly HttpClient _client;
    private readonly IConfiguration _config;

    public SettingsHttpClient(HttpClient client, IConfiguration config)
    {
        _client = client;
        _config = config;
    }

    public async Task<SettingsDTO> GetAdminModeSetting()
    {
        // проверяем - откликается ли волт и разблокирован ли он
        try
        {
            var result = new SettingsDTO { CheckResult = true };
            var response = await _client.GetFromJsonAsync<ApiResponse<CurrentItem>>(_config["Settings"]);
            if (response.IsSuccess)
            {
                return new SettingsDTO { CheckResult = true, AdminMode = response.Result.AdminMode };
            }
            return new SettingsDTO { CheckResult = false };

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return new SettingsDTO { CheckResult = false };
        }

    }
}
