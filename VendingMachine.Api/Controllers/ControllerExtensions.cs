using Microsoft.AspNetCore.Mvc;
using VendingMachineApi.Models;

namespace VendingMachineApi.Controllers;

public static class ControllerExtensions
{
    public static User GetAuthenticatedUser(this ControllerBase controller)
    {
        var authenticatedUser = controller.HttpContext.Items["User"] as User;
        return authenticatedUser!;
    }

    public static UserDto AsDto(this User user)
    {
        return new UserDto(user.Id, user.Username!, user.Deposit!.Value, user.Roles);
    }
}