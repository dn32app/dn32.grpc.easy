using dn32.grpc.easy.client.extensions;
using shared.example.contract;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

// gRPC configuration
builder.Services.AddRemoteGrpcController<IExampleGrpcController>("http://localhost:5230");
// gRPC configuration

var app = builder.Build();
app.MapControllers();
app.Run();
