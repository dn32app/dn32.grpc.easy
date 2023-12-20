using dn32.grpc.easy.client.model;
using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProtoBuf.Grpc.Client;
using ProtoBuf.Grpc.Configuration;
using System;
using System.Net.Http;
using System.Net.Security;
using System.Security.Authentication;

namespace dn32.grpc.easy.client.extensions;

public static class GrpcClientExtension
{
    public static IServiceCollection AddRemoteGrpcController<TIController>(this IServiceCollection services, string serverUrl, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped) where TIController : class
    {
        var grpcControllerData = new GrpcControllerData
        {
            Type = typeof(TIController),
            ServiceLifetime = serviceLifetime,
            ServerUrl = serverUrl
        };

        var grpcRetryPolicy = new GrpcRetryPolicy();
        var grpcSocketsHttpHandler = new GrpcSocketsHttpHandler();

        var descriptor = ServiceDescriptor.Describe(grpcControllerData.Type, (serviceProvider) => ConnectToRemoteGrpcController(grpcControllerData, grpcRetryPolicy, grpcSocketsHttpHandler), grpcControllerData.ServiceLifetime);

        if (!services.Contains(descriptor))
            services.Add(descriptor);

        return services;
    }

    private static object ConnectToRemoteGrpcController(GrpcControllerData dadosDeClienteGrpc, GrpcRetryPolicy grpcRetryPolicy, GrpcSocketsHttpHandler grpcSocketsHttpHandler)
    {//Este método é chamado somente quando o consumidor da dependência necessita da instância da dependência e não na inicialização da aplicação
        var canalGrpc = CreateGrpcChannel(dadosDeClienteGrpc.ServerUrl, grpcRetryPolicy, grpcSocketsHttpHandler);
        var metodo = typeof(GrpcClientFactory).GetMethod(nameof(GrpcClientFactory.CreateGrpcService), [typeof(ChannelBase), typeof(ClientFactory)]) ?? throw new Exception($"Method not found: '{nameof(GrpcClientFactory.CreateGrpcService)}'");
        metodo = metodo.MakeGenericMethod(dadosDeClienteGrpc.Type) ?? throw new Exception($"Method not found: '{nameof(GrpcClientFactory.CreateGrpcService)}'");

#pragma warning disable CS8603 // Static class
        return metodo.Invoke(null, [canalGrpc, default(ClientFactory)]);
#pragma warning restore CS8603 // Static class
    }

    private static GrpcChannel CreateGrpcChannel(string url, GrpcRetryPolicy grpcRetryPolicy, GrpcSocketsHttpHandler grpcSocketsHttpHandler)
    {
        var defaultMethodConfig = new MethodConfig
        {
            //https://learn.microsoft.com/pt-br/aspnet/core/grpc/retries?view=aspnetcore-7.0
            Names = { MethodName.Default },
            RetryPolicy = new RetryPolicy
            {
                MaxAttempts = grpcRetryPolicy.RetryPolicyMaxAttempts, //O número máximo de tentativas de chamada, incluindo a tentativa original. Esse valor é limitado pelo GrpcChannelOptions.MaxRetryAttempts qual o padrão é 5. Um valor é necessário e deve ser maior que 1.
                InitialBackoff = TimeSpan.FromMilliseconds(grpcRetryPolicy.RetryPolicyInitialBackoffEmMilisegundos), //O atraso inicial de retirada entre tentativas de repetição. Um atraso aleatório entre 0 e a retirada atual determina quando a próxima tentativa de repetição é feita. Após cada tentativa, a retirada atual é multiplicada por BackoffMultiplier. Um valor é necessário e deve ser maior que zero.
                MaxBackoff = TimeSpan.FromMilliseconds(grpcRetryPolicy.RetryPolicyMaxBackoffEmMilisegundos), //A retirada máxima coloca um limite superior no crescimento exponencial de retirada. Um valor é necessário e deve ser maior que zero.
                BackoffMultiplier = (double)grpcRetryPolicy.RetryPolicyBackoffMultiplierMultiplicadoPor10 / 10, //A retirada será multiplicada por esse valor após cada tentativa de repetição e aumentará exponencialmente quando o multiplicador for maior que 1. Um valor é necessário e deve ser maior que zero.              
                RetryableStatusCodes = { StatusCode.Unavailable } //Uma coleção de códigos de status. Uma chamada gRPC que falha com um status correspondente será repetida automaticamente. Para obter mais informações sobre códigos de status, consulte códigos de status e seu uso no gRPC. Pelo menos um código de status retrátível é necessário. //https://grpc.github.io/grpc/core/md_doc_statuscodes.html
            }
        };

        var handler = new SocketsHttpHandler
        {
            PooledConnectionIdleTimeout = TimeSpan.FromSeconds(grpcSocketsHttpHandler.HandlerPooledConnectionIdleTimeoutEmSegundos), //O tempo uma conexão pode ficar ociosa no pool para ser considerada reutilizável. (segundos).
            KeepAlivePingDelay = TimeSpan.FromSeconds(grpcSocketsHttpHandler.HandlerKeepAlivePingDelayEmSegundos), //O intervalo de envio de ping para a garantia da conexão (segundos).
            KeepAlivePingTimeout = TimeSpan.FromSeconds(grpcSocketsHttpHandler.HandlerKeepAlivePingTimeoutEmSegundos), //Time-out de uma tentativa de ping com o servidor. O cliente fechará a conexão se não receber nenhum retorno dentro do tempo limite (segundos).
            EnableMultipleHttp2Connections = grpcSocketsHttpHandler.HandlerEnableMultipleHttp2Connections,//Indica se conexões HTTP/2 adicionais podem ser estabelecidas para o mesmo servidor quando o número máximo de fluxos simultâneos é atingido em todas as conexões existentes.
            ConnectTimeout = TimeSpan.FromSeconds(grpcSocketsHttpHandler.HandlerConnectTimeoutEmSegundos), //Obtém ou define o intervalo de espera antes que o tempo limite de estabelecimento da conexão seja atingido (segundos).
            MaxConnectionsPerServer = grpcSocketsHttpHandler.HandlerMaxConnectionsPerServer, //Obtém ou define o número máximo de conexões TCP simultâneas permitido para um único servidor.
            MaxResponseDrainSize = grpcSocketsHttpHandler.HandlerMaxResponseDrainSize, //Volume máximo de dados que pode ser drenagem das respostas em bytes. A drenagem ocorre quando uma solicitação é cancelada ou uma resposta é descartada antes de ler totalmente o conteúdo. Se o número de bytes drenados exceder esse valor, a conexão será fechada em vez de reutilizado (bytes).
            MaxResponseHeadersLength = grpcSocketsHttpHandler.HandlerMaxResponseHeadersLength, //Volume máximo dos cabeçalhos de resposta (KB).
        };

        return ConfigurarHandler(url, handler, defaultMethodConfig);
    }

    private static GrpcChannel ConfigurarHandler(string url, SocketsHttpHandler handler, MethodConfig defaultMethodConfig)
    {
        if (url.StartsWith("https://"))
        {
            handler.SslOptions = new SslClientAuthenticationOptions
            {
                AllowRenegotiation = true,
                EnabledSslProtocols = SslProtocols.Tls12,
            };
            return GrpcChannel.ForAddress(url, new GrpcChannelOptions
            {
                HttpHandler = handler,
                ServiceConfig = new ServiceConfig { MethodConfigs = { defaultMethodConfig } },
            });
        }
        else
        {
            return GrpcChannel.ForAddress(url, new GrpcChannelOptions
            {
                HttpHandler = handler,
                Credentials = ChannelCredentials.Insecure,
                ServiceConfig = new ServiceConfig { MethodConfigs = { defaultMethodConfig } },
            });
        }
    }
}