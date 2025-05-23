﻿using Grpc.Core;
using Grpc.Core.Interceptors;

namespace client.example.authentication;

public class AuthHeadersInterceptor(IHttpContextAccessor HttpContextAccessor) : Interceptor
{
    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        var metadata = new Metadata
        {
            {"Authorization", $"Bearer <JWT_TOKEN>"}
        };

        var userIdentity = HttpContextAccessor?.HttpContext?.User?.Identity;
        if (userIdentity?.IsAuthenticated == true)
        {
            metadata.Add("User", userIdentity?.Name ?? string.Empty);
        }

        var callOption = context.Options.WithHeaders(metadata);
        context = new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, callOption);

        return base.AsyncUnaryCall(request, context, continuation);
    }
}