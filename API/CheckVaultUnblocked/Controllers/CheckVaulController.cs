using System.Net;
using CheckVaultUnblocked.Services;
using Microsoft.AspNetCore.Mvc;

namespace CheckVaultUnblocked.Controllers;

[ApiController]
[Route("checkVault")]
public class CheckVaulController : ControllerBase
{
    private readonly CheckVault _checkVault;
    public CheckVaulController(CheckVault checkVault)
    {
        _checkVault = checkVault;
    }

    [HttpGet]
    public IActionResult CheckVaultUnblocked()
    {
        return _checkVault.IsVaultRun ? StatusCode(200) : StatusCode(500);
    }

}