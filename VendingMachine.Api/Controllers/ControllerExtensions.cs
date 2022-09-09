using Microsoft.AspNetCore.Mvc;
using VendingMachine.Model.Models;

namespace VendingMachineApi.Controllers;

public static class ControllerExtensions
{
    public static User GetAuthenticatedUser(this ControllerBase controller)
    {
        var authenticatedUser = controller.HttpContext.Items["User"] as User;
        return authenticatedUser!;
    }


}