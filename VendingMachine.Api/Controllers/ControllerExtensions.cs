using Microsoft.AspNetCore.Mvc;
using VendingMachine.Model;

namespace VendingMachineApi.Controllers;

public static class ControllerExtensions
{
    public static User GetAuthenticatedUser(this ControllerBase controller)
    {
        var authenticatedUser = controller.HttpContext.Items["User"] as User;
        return authenticatedUser!;
    }


}