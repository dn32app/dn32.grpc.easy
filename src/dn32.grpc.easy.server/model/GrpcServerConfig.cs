using System.IO.Compression;

namespace dn32.grpc.easy.server.model;

public class GrpcServerConfig
{
    public required bool EnableDetailedErrors { get; set; }
    public required int MaxReceiveMessageSize { get; set; }
    public required int MaxSendMessageSize { get; set; }
    public required CompressionLevel ResponseCompressionLevel { get; set; }
}
