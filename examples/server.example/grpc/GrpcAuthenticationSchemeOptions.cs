using Microsoft.AspNetCore.Authentication;

namespace server.example.grpc;

internal class GrpcAuthenticationSchemeOptions : AuthenticationSchemeOptions
{
    public bool AlwaysAuthenticate { get; set; }
}