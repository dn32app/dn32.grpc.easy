using Microsoft.Extensions.DependencyInjection;
using System;

namespace dn32.grpc.easy.client.model;

internal class GrpcControllerData
{
    public required Type Type { get; set; }
    public required ServiceLifetime ServiceLifetime { get; set; }
    public required string ServerUrl { get; set; }
}