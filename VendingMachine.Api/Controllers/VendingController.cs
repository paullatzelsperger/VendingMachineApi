using Microsoft.AspNetCore.Mvc;
using VendingMachine.Core.Services;
using VendingMachineApi.Authorization;
using VendingMachineApi.Core;

namespace VendingMachineApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VendingController : ControllerBase
{
    private readonly IVendingService vendingService;


    public VendingController(IVendingService vendingService)
    {
        this.vendingService = vendingService;
    }

    [Authorize(Role.Buyer)]
    [HttpPost("deposit")]
    public async Task<IActionResult> Deposit([FromQuery] int amount)
    {
        var user = this.GetAuthenticatedUser();
        var updateRes = await vendingService.Deposit(user, amount);
        return MapResult(updateRes);
    }

    [Authorize(Role.Buyer)]
    [HttpPost("buy")]
    public async Task<IActionResult> Buy([FromQuery] string productId, [FromQuery] int amount)
    {
        // get user
        var user = this.GetAuthenticatedUser();

        var purchaseResult = await vendingService.Buy(user, productId, amount);
        return purchaseResult.Succeeded ? Ok(purchaseResult.Content) : BadRequest(purchaseResult.FailureMessage);
    }

    [Authorize]
    [HttpPost("reset")]
    public async Task<IActionResult> ResetBalance()
    {
        var user = this.GetAuthenticatedUser();
        var result = await vendingService.ResetBalance(user);
        return MapResult(result);
    }


    private IActionResult MapResult(ServiceResult<object> result)
    {
        return result.Succeeded ? Ok() : BadRequest(result.FailureMessage);
    }
}