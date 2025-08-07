
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Common.Utils.Logging;

public class ConsoleLogging
{
    public static void RunApp(WebApplication app)
    {
        app.Start();

        var server = app.Services.GetService<IServer>();
        var addressFeature = server.Features.Get<IServerAddressesFeature>();

        if (addressFeature.Addresses.Count == 0)
        {
            Console.WriteLine($"{DateTime.Now.AddHours(3).ToString("dd.MM.yyyy HH:mm:ss")} - Web-server has no any adresses");
        }
        else
        {
            foreach (var address in addressFeature.Addresses)
            {
                if (app.Environment.IsDevelopment())
                {
                    Console.WriteLine($"{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")} - Kestrel is listening on address: {address}");
                }
                else
                {
                    Console.WriteLine($"{DateTime.Now.AddHours(3).ToString("dd.MM.yyyy HH:mm:ss")} - Kestrel is listening on address: {address}");
                }
            }
        }
        app.WaitForShutdown();
    }
}