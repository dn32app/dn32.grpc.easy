using dn32.grpc.easy.server.extensions;
using server.example.grpc;
using shared.example.contract;

var builder = WebApplication.CreateBuilder(args);

// gRPC configuration
builder.Services.AddGrpcController<IExampleGrpcController, ExampleGrpcController>();
builder.Services.AddGrpcServerDefaultInitialize(builder, 5230, 5231);
// End gRPC configuration

var app = builder.Build();

// gRPC configuration
app.UseGrpcServerDefaultInitialize();
// End gRPC configuration

app.Run();
