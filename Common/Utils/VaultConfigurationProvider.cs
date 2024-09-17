using VaultSharp;
using VaultSharp.V1.AuthMethods.AppRole;
using VaultSharp.V1.Commons;
using Microsoft.Extensions.Configuration;

namespace Common.Utils;

public class VaultConfigurationProvider : ConfigurationProvider
{
    public VaultOptions _config;
    private IVaultClient _client;

    public VaultConfigurationProvider(VaultOptions config)
    {
        _config = config;

        var vaultClientSettings = new VaultClientSettings(
            _config.Address,
            new AppRoleAuthMethodInfo(_config.Role, _config.Secret)
        );

        _client = new VaultClient(vaultClientSettings);
    }

    public override void Load()
    {
        LoadAsync().Wait();
    }

    public async Task LoadAsync()
    {
        await GetDatabaseCredentials();
        await GetRabbitCredentials();
        await GetApiSecret();
        await GetELKCredentials();
    }

    private async Task GetDatabaseCredentials()
    {
        if (!string.IsNullOrEmpty(_config.SecretPathPg))
        {
            Secret<SecretData> secrets = await _client.V1.Secrets.KeyValue.V2.ReadSecretAsync(
              _config.SecretPathPg, null, "secret");

            Data.Add("pg:database", secrets.Data.Data["database"].ToString());
            Data.Add("pg:username", secrets.Data.Data["username"].ToString());
            Data.Add("pg:password", secrets.Data.Data["password"].ToString());
            Data.Add("pg:host", secrets.Data.Data["host"].ToString());
        }
    }

    private async Task GetRabbitCredentials()
    {
        if (!string.IsNullOrEmpty(_config.SecretPathRt))
        {
            Secret<SecretData> secrets = await _client.V1.Secrets.KeyValue.V2.ReadSecretAsync(
              _config.SecretPathRt, null, "secret");

            Data.Add("rt:username", secrets.Data.Data["username"].ToString());
            Data.Add("rt:password", secrets.Data.Data["password"].ToString());
            Data.Add("rt:host", secrets.Data.Data["host"].ToString());
        }

    }

    private async Task GetApiSecret()
    {
        if (!string.IsNullOrEmpty(_config.SecretPathApi))
        {
            Secret<SecretData> secrets = await _client.V1.Secrets.KeyValue.V2.ReadSecretAsync(
              _config.SecretPathApi, null, "secret");

            Data.Add("api:secret", secrets.Data.Data["secret"].ToString());
        }

    }

    private async Task GetELKCredentials()
    {
        if (!string.IsNullOrEmpty(_config.SecretPathElk))
        {
            Secret<SecretData> secrets = await _client.V1.Secrets.KeyValue.V2.ReadSecretAsync(
              _config.SecretPathElk, null, "secret");

            Data.Add("elk:fingerprint", secrets.Data.Data["fingerprint"].ToString());
            Data.Add("elk:username", secrets.Data.Data["username"].ToString());
            Data.Add("elk:password", secrets.Data.Data["password"].ToString());
            Data.Add("elk:host", secrets.Data.Data["host"].ToString());
        }
    }
}

public class VaultConfigurationSource : IConfigurationSource
{
    private VaultOptions _config;

    public VaultConfigurationSource(Action<VaultOptions> config)
    {
        _config = new VaultOptions();
        config.Invoke(_config);
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new VaultConfigurationProvider(_config);
    }
}

public class VaultOptions
{
    public string Address { get; set; }
    public string Role { get; set; }

    public string Secret { get; set; }
    public string SecretPathPg { get; set; }
    public string SecretPathRt { get; set; }
    public string SecretPathApi { get; set; }
    public string SecretPathElk { get; set; }
}

public static class VaultExtensions
{
    public static IConfigurationBuilder AddVault(this IConfigurationBuilder configuration,
    Action<VaultOptions> options)
    {
        var vaultOptions = new VaultConfigurationSource(options);
        configuration.Add(vaultOptions);
        return configuration;
    }
}
