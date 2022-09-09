using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using VendingMachine.Core;
using VendingMachine.Model;

namespace VendingMachineApi.Authorization;

/// <summary>
/// Extension of the standard <see cref="Microsoft.AspNetCore.Authorization.AuthorizeAttribute"/>
/// to be able to specify and resolve a <see cref="Role"/>.
/// If an incoming user request is not authenticated, or does not have the appropriate roles, a HTTP
/// 401 and 403 is returned respectively.
/// </summary>
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
        // return 403 when user does not have the required role
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