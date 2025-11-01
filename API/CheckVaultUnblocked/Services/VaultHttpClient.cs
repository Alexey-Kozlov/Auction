using CheckVaultUnblocked.DTO;

namespace CheckVaultUnblocked.Services;

public class VaultHttpClient
{
    private readonly HttpClient _client;
    private readonly IConfiguration _config;

    public VaultHttpClient(HttpClient client, IConfiguration config)
    {
        _client = client;
        _config = config;
        _client.DefaultRequestHeaders.Add("X-Vault-Token", _config["Vault:Token"]);
    }

    public async Task<bool> GetVaultStatus()
    {
        // проверяем - откликается ли волт и разблокирован ли он
        try
        {
            var response = await _client.GetFromJsonAsync<SecretDto>(_config["Vault:Address"]);
            return response != null && response.data != null &&
                !string.IsNullOrEmpty(response.data.data.secret);
        }
        catch
        {
            return false;
        }
    }
}