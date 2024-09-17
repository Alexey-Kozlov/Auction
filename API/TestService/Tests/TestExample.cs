using System.Diagnostics;
using Xunit;

namespace TestService.Tests;

public class AuctionMetricsTest
{
    private static IServiceProvider CreateServiceProvider()
    {
        var serviceCollection = new ServiceCollection();
        var config = CreateIConfiguration();
        serviceCollection.AddSingleton(config);
        serviceCollection.AddSingleton<VaultConfigurationProvider>();
        return serviceCollection.BuildServiceProvider();
    }

    private static IConfiguration CreateIConfiguration()
    {
        var inMemorySettings = new Dictionary<string, string> {
            {"Address", "http://localhost:8200"},
            {"VAULT_ROLE_ID", "71abca6f-df2d-d2d9-7597-4ad55441e12e"},
            {"SecretPath", "auction/auction/"},
            {"VAULT_SECRET_ID", "dca2cf8c-6cc0-acb2-37a5-a6e37f66c6b3"}
            };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();
    }

    [Fact]
    public void Test1()
    {
        var services = CreateServiceProvider();

        var ddd = services.GetRequiredService<IConfiguration>();

        var tt = ddd["database:password"];
        Debug.WriteLine(ddd);
    }

    [Fact]
    public void Test2()
    {
        var per1 = 1;
        Console.WriteLine(per1);
    }
}