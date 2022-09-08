using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using VendingMachineApi.Core;
using VendingMachineApi.Models;

namespace VendingMachineApi.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeAttribute : Attribute, IAuthorizationFilter
{
    private readonly IList<Role> roles;

    public AuthorizeAttribute(params Role[] roles)
    {
        this.roles = roles;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // skip authorization if action is decorated with [AllowAnonymous] attribute
        var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();
        if (allowAnonymous)
            return;

        //return 401 when not authenticated
        if (context.HttpContext.Items["User"] is not User user)
        {
            context.Result = new StatusCodeResult(StatusCodes.Status401Unauthorized);
        }
        // return 403 when user does not have correct role
        else if (roles.Any() && !UserHasRequiredRoles(user))
        {
            context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
        }
    }

    private bool UserHasRequiredRoles(User user)
    {
        var requiredRoles = roles.Select(r => r.ToString()).ToList();
        return requiredRoles.All(rr => user.Roles.Contains(rr, StringComparer.OrdinalIgnoreCase));
    }
}