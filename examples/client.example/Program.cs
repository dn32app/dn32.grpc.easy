using client.example.authentication;
using dn32.grpc.easy.client.extensions;
using shared.example.contract;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
var services = builder.Services;

// gRPC configuration
services
    .AddHttpContextAccessor()
    .AddTransient<ClientLoggingInterceptor>()
    .AddTransient<AuthHeadersInterceptor>()
    .AddDn32Grpc<IExampleGrpcController>("http://localhost:5230")
    //.AddDnController<IMoreControllers>()
    .AddGrpcInterceptor<ClientLoggingInterceptor>()
    .AddGrpcInterceptor<AuthHeadersInterceptor>()
    .BuildDn32Grpc();
// gRPC configuration

var app = builder.Build();
app.MapControllers();
app.Run();
