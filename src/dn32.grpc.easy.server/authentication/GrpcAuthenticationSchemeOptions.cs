using Microsoft.AspNetCore.Authentication;

namespace dn32.grpc.easy.server.authentication;

internal class GrpcAuthenticationSchemeOptions : AuthenticationSchemeOptions
{
    public bool AlwaysAuthenticate { get; set; }
}