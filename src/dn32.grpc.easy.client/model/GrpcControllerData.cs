using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace dn32.grpc.easy.client.model;

public class GrpcControllerData
{
    public required Type Type { get; set; }
    public required ServiceLifetime ServiceLifetime { get; set; }
    public required string ServerUrl { get; set; }
    public required List<Type> Interceptors { get; set; }
    public required IServiceCollection Services { get; set; }
}