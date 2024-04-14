using Microsoft.AspNetCore.Identity;

namespace IdentityService.Models;

public class ApplicationUser : IdentityUser
{
    public ICollection<UserFinance> UserFinance { get; set; }
}
