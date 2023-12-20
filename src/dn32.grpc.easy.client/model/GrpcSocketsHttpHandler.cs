namespace dn32.grpc.easy.client.model;

public class GrpcSocketsHttpHandler
{
    public int HandlerPooledConnectionIdleTimeoutEmSegundos { get; set; } = 60;

    public int HandlerKeepAlivePingDelayEmSegundos { get; set; } = 60;

    public int HandlerKeepAlivePingTimeoutEmSegundos { get; set; } = 30;

    public bool HandlerEnableMultipleHttp2Connections { get; set; } = true;

    public int HandlerConnectTimeoutEmSegundos { get; set; } = 30;

    public int HandlerMaxConnectionsPerServer { get; set; } = 2048;

    public int HandlerMaxResponseDrainSize { get; set; } = 1024;

    public int HandlerMaxResponseHeadersLength { get; set; } = 10;//KB
}