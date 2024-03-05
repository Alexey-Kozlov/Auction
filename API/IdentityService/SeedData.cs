using System.Security.Claims;
using IdentityService.Data;
using IdentityService.Models;
using Microsoft.AspNetCore.Identity;

namespace IdentityService;

public class SeedData
{
    public static void EnsureSeedData(WebApplication app)
    {
        using var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetService<ILogger<SeedData>>();

        var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        logger.LogInformation("Seed started");
        if(userMgr.Users.Any()) return;

        var alice = userMgr.FindByNameAsync("alice").Result;
        if (alice == null)
        {
            alice = new ApplicationUser
            {
                UserName = "AliceSmith",
                Email = "alice",
                EmailConfirmed = true,
            };
            var result = userMgr.CreateAsync(alice, "alice").Result;
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }

            result = userMgr.AddClaimsAsync(alice, new Claim[]{
                            new Claim("Name", "Alice Smith"),
                            new Claim("Login", "alice")
                        }).Result;
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }
            logger.LogInformation("alice created");
        }
        else
        {
            logger.LogInformation("alice already exists");
        }

        var bob = userMgr.FindByNameAsync("bob").Result;
        if (bob == null)
        {
            bob = new ApplicationUser
            {
                UserName = "BobSmith",
                Email = "bob",
                EmailConfirmed = true
            };
            var result = userMgr.CreateAsync(bob, "bob").Result;
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }

            result = userMgr.AddClaimsAsync(bob, new Claim[]{
                            new Claim("name", "Bob Smith"),
                            new Claim("Login", "bob")
                        }).Result;
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }
            logger.LogInformation("bob created");
        }
        else
        {
            logger.LogInformation("bob already exists");
        }
    }
}
