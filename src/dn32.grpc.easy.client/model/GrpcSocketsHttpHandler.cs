namespace dn32.grpc.easy.client.model;

public class GrpcSocketsHttpHandler
{
    public int HandlerPooledConnectionIdleTimeout { get; set; } = 60;

    public int HandlerKeepAlivePingDelay { get; set; } = 60;

    public int HandlerKeepAlivePingTimeout { get; set; } = 30;

    public bool HandlerEnableMultipleHttp2Connections { get; set; } = true;

    public int HandlerConnectTimeout { get; set; } = 30;

    public int HandlerMaxConnectionsPerServer { get; set; } = 2048;

    public int HandlerMaxResponseDrainSize { get; set; } = 1024;

    public int HandlerMaxResponseHeadersLength { get; set; } = 10;//KB
}