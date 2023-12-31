using dn32.grpc.easy.server.extensions;
using server.example.grpc;
using shared.example.contract;

var builder = WebApplication.CreateBuilder(args);

// gRPC configuration
builder.Services
        .AddGrpcController<IExampleGrpcController, ExampleGrpcController>()
        .AddGrpcServerDefaultInitialize(builder, 5230, 5231)
        .AddAuthentication(GrpcAuthenticationHandler.SchemeName)
        .AddScheme<GrpcAuthenticationSchemeOptions, GrpcAuthenticationHandler>(GrpcAuthenticationHandler.SchemeName, options => options.AlwaysAuthenticate = true);
// End gRPC configuration

var app = builder.Build();

// gRPC configuration
app.UseGrpcServerDefaultInitialize();
// End gRPC configuration

app.Run();
