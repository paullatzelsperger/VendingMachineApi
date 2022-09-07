using System.Net.Http.Headers;
using System.Security.Principal;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using VendingMachineApi.Services;

namespace VendingMachineApi.Authentication;

public class BasicAuthHandler : AuthenticationHandler<BasicAuthOptions>
{
    private readonly IUserService userService;

    public BasicAuthHandler(IOptionsMonitor<BasicAuthOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, IUserService userService) : base(options, logger, encoder, clock)
    {
        this.userService = userService;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        try
        {
            var context = Context;
            var authHeader = AuthenticationHeaderValue.Parse(context.Request.Headers["Authorization"]);
            var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
            var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);
            var username = credentials[0];
            var password = credentials[1];

            // authenticate credentials with user service and attach user to http context
            var user = await userService.Authenticate(username, password);

            if (user.Succeeded)
            {
                context.Items["User"] = user.Content; // gives controllers access to the authenticated user
                return AuthenticateResult.Success(new AuthenticationTicket(new GenericPrincipal(new GenericIdentity(user.Content!.Username), user.Content.Roles), "Basic"));
            }

            return AuthenticateResult.Fail(user.FailureMessage!);
        }
        catch (Exception ex)
        {
            // do nothing if invalid auth header
            // user is not attached to context so request won't have access to secure routes
            return AuthenticateResult.Fail(ex);
        }
    }
}

public class BasicAuthOptions : AuthenticationSchemeOptions
{
}