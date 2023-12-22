using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace server.example.grpc;

internal class GrpcAuthenticationHandler : AuthenticationHandler<GrpcAuthenticationSchemeOptions>
{
    public const string SchemeName = "gRPC";

    public GrpcAuthenticationHandler(IOptionsMonitor<GrpcAuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder) : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Options.AlwaysAuthenticate)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var getTokenHere = Context.Request.Headers;

        var claimsIdentity = new ClaimsIdentity(SchemeName);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(claimsIdentity), Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}