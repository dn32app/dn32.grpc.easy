using dn32.grpc.easy.server.exceptions;
using dn32.grpc.easy.server.model;
using Grpc.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using ProtoBuf.Grpc.Server;
using ProtoBuf.Meta;
using System.IO.Compression;

namespace dn32.grpc.easy.server.extensions;

public static class GrpcServererExtension
{
    public static List<InternalValuesForGrpcControllers> InitGrptServer(this IServiceCollection services)
    {
        return [];
    }

    public static IServiceCollection AddGrpcController<TService, TImplementation>(this IServiceCollection services, List<InternalValuesForGrpcControllers> controllers) where TService : class where TImplementation : class, TService
    {
        controllers.Add(new InternalValuesForGrpcControllers
        {
            InterfaceType = typeof(TService),
            ConcreteType = typeof(TImplementation)
        });

        return services;
    }

    public static IServiceCollection AddGrpcServerDefaultInitialize(this IServiceCollection services, List<InternalValuesForGrpcControllers> controllers, WebApplicationBuilder builder, int grpcPort, int? restPort = null, List<Type>? interceptors = null, GrpcServerConfig? configValue = null)
    {
        RuntimeTypeModel.Default.IncludeDateTimeKind = true;

        services.AddGrpc();
        services.AddCodeFirstGrpc(config =>
        {
            config.EnableDetailedErrors = configValue?.EnableDetailedErrors ?? true;
            config.MaxReceiveMessageSize = configValue?.MaxReceiveMessageSize ?? 20 * 1024 * 1024; // 20MB
            config.MaxSendMessageSize = configValue?.MaxSendMessageSize ?? 20 * 1024 * 1024; // 20MB
            config.ResponseCompressionLevel = configValue?.ResponseCompressionLevel ?? CompressionLevel.Optimal;
            interceptors ??= [];
            interceptors.Add(typeof(ExceptionInterceptor));

            foreach (var interceptor in interceptors)
            {
                config.Interceptors.Add(interceptor);
            }
        });

        if (!controllers.Any()) throw new Exception($"No gRPC controllers added. Use {nameof(GrpcServererExtension)}.{nameof(AddGrpcController)} to add gRPC controllers.");
        controllers.ForEach(controladores => services.AddScoped(controladores.InterfaceType, controladores.ConcreteType));

        builder.WebHost.ConfigureKestrel((context, options) => { });

        builder.WebHost.UseKestrel().ConfigureKestrel((context, serverOptions) =>
        {
            serverOptions.ListenAnyIP(grpcPort, listenOptions => listenOptions.Protocols = HttpProtocols.Http2);

            if (restPort != null)
            {
                serverOptions.ListenAnyIP(restPort.Value, listenOptions => listenOptions.Protocols = HttpProtocols.Http1);
            }
        });

        return services;
    }

    public static void UseGrpcServerDefaultInitialize(this WebApplication app, List<InternalValuesForGrpcControllers> controllers)
    {
        var tipo = typeof(GrpcEndpointRouteBuilderExtensions);
        var metodo = tipo.GetMethod(nameof(GrpcEndpointRouteBuilderExtensions.MapGrpcService));

        app.Use(async (context, next) =>
        {
            try
            {
                await next();
            }
            catch (Exception ex)
            {
                await ResultExceptionAsync(context, ex);
                return;
            }

            if (context.Response.StatusCode != StatusCodes.Status200OK)
            {
                await ResultExceptionAsync(context);
            }
        });


        foreach (var item in controllers)
        {
            var metodoComGenerico = metodo?.MakeGenericMethod(item.ConcreteType);
            metodoComGenerico?.Invoke(null, new[] { app });
        }
    }

    private static async Task ResultExceptionAsync(HttpContext context, Exception? ex = null)
    {
        context.Response.Clear();
        context.Response.StatusCode = StatusCodes.Status200OK;
        context.Response.ContentType = "application/grpc";
        context.Response.Headers.GrpcEncoding = "identity";
        await context.Response.Body.FlushAsync();

        if (context.Items.TryGetValue("grpc-status", out var statusCode) && statusCode is StatusCode)
            context.Response.AppendTrailer("grpc-status", ((int)statusCode).ToString());
        else
            context.Response.AppendTrailer("grpc-status", ((int)StatusCode.Internal).ToString());

        context.Response.AppendTrailer("grpc-message", Uri.EscapeDataString(ex?.Message ?? "Grpc error"));
        context.Response.AppendTrailer("grpc-endpoint", Uri.EscapeDataString(context.GetEndpoint()?.DisplayName ?? string.Empty));

        if (ex is RpcException grpException)
            foreach (var trailers in grpException.Trailers)
                context.Response.AppendTrailer(Uri.EscapeDataString(trailers.Key), Uri.EscapeDataString(trailers.Value));

        foreach (var item in context.Items)
        {
            var key = item.Key as string;
            var value = item.Value as string;
            if (string.IsNullOrEmpty(key)) continue;
            if (string.IsNullOrEmpty(value)) continue;
            context.Response.AppendTrailer(Uri.EscapeDataString(key), Uri.EscapeDataString(value));
        }
    }
}