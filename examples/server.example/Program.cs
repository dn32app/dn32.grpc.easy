using dn32.grpc.easy.server.extensions;
using server.example.grpc;
using shared.example.contract;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

// gRPC configuration
var grpcControllers = services.InitGrptServer();
services
    .AddHttpContextAccessor()
    .AddGrpcController<IExampleGrpcController, ExampleGrpcController>(grpcControllers)
    .AddGrpcServerDefaultInitialize(grpcControllers, builder, 5230, 5231);
// End gRPC configuration

services.AddAuthorization();
services.AddAuthentication();

var app = builder.Build();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.Run();
