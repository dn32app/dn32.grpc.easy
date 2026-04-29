using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace dn32.grpc.easy.server.exceptions;

public static class ExceptionHelpers
{
    public static RpcException Handle<T>(this Exception exception, ServerCallContext context, ILogger<T> logger, Guid correlationId)
    {
        return exception switch
        {
            TimeoutException => HandleTimeoutException((TimeoutException)exception, context, logger, correlationId),
            //SqlException => HandleSqlException((SqlException)exception, context, logger, correlationId),
            RpcException => HandleRpcException((RpcException)exception, logger, correlationId),
            _ => HandleDefault(exception, context, logger, correlationId)
        };
    }

    private static RpcException HandleTimeoutException<T>(TimeoutException exception, ServerCallContext context, ILogger<T> logger, Guid correlationId)
    {
        logger.LogError(exception, $"CorrelationId: {correlationId} - A timeout occurred");

        var status = new Status(StatusCode.Internal, "An external resource did not answer within the time limit");

        return new RpcException(status, CreateTrailers(correlationId));
    }

    //private static RpcException HandleSqlException<T>(SqlException exception, ServerCallContext context, ILogger<T> logger, Guid correlationId)
    //{
    //    logger.LogError(exception, $"CorrelationId: {correlationId} - An SQL error occurred");
    //    Status status;

    //    if (exception.Number == -2)
    //    {
    //        status = new Status(StatusCode.DeadlineExceeded, "SQL timeout");
    //    }
    //    else
    //    {
    //        status = new Status(StatusCode.Internal, "SQL error");
    //    }
    //    return new RpcException(status, CreateTrailers(correlationId));
    //}

    private static RpcException HandleRpcException<T>(RpcException exception, ILogger<T> logger, Guid correlationId)
    {
        logger.LogError(exception, $"CorrelationId: {correlationId} - An error occurred");
        var trailers = exception.Trailers;
        trailers.Add(CreateTrailers(correlationId)[0]);

        // Unwrap the innermost status detail to avoid deeply nested Status(...) strings
        var rootDetail = UnwrapRpcMessage(exception.Status.Detail);
        return new RpcException(new Status(exception.StatusCode, rootDetail), trailers);
    }

    private static RpcException HandleDefault<T>(Exception exception, ServerCallContext context, ILogger<T> logger, Guid correlationId)
    {
        var rootException = GetRootCause(exception);
        var location = GetExceptionLocation(rootException);
        var detail = $"[{correlationId}] {rootException.GetType().Name}: {rootException.Message}{location}";

        logger.LogError(exception, $"CorrelationId: {correlationId} - An error occurred");
        return new RpcException(new Status(StatusCode.Internal, detail), CreateTrailers(correlationId));
    }

    private static Exception GetRootCause(Exception ex)
    {
        while (ex.InnerException is not null)
            ex = ex.InnerException;
        return ex;
    }

    private static string GetExceptionLocation(Exception ex)
    {
        if (ex.StackTrace is null) return string.Empty;

        // Find first frame that belongs to user code (not System/Microsoft/gRPC internals)
        var frame = ex.StackTrace
            .Split('\n')
            .Select(l => l.Trim())
            .FirstOrDefault(l => l.StartsWith("at ") && !l.Contains("System.") && !l.Contains("Microsoft.") && !l.Contains("Grpc.") && !l.Contains("dn32.grpc"));

        if (frame is null) return string.Empty;

        // Extract "in File.cs:line N" part if available
        var inIndex = frame.IndexOf(" in ", StringComparison.Ordinal);
        if (inIndex >= 0)
            return $" | {frame[(inIndex + 4)..]}";

        // Fallback: return method signature
        var atIndex = frame.IndexOf("at ", StringComparison.Ordinal);
        var parenIndex = frame.IndexOf('(');
        if (atIndex >= 0 && parenIndex > atIndex)
            return $" | em {frame[(atIndex + 3)..parenIndex]}";

        return string.Empty;
    }

    private static string UnwrapRpcMessage(string detail)
    {
        // Status(StatusCode="Internal", Detail="...") can nest multiple layers — extract innermost message
        const string prefix = "Detail=\"";
        while (detail.StartsWith("Status(", StringComparison.Ordinal))
        {
            var start = detail.IndexOf(prefix, StringComparison.Ordinal);
            if (start < 0) break;
            start += prefix.Length;
            var end = detail.LastIndexOf('"');
            if (end <= start) break;
            detail = detail[start..end];
        }
        return detail;
    }

    /// <summary>
    ///  Adding the correlation to Response Trailers
    /// </summary>
    /// <param name="correlationId"></param>
    /// <returns></returns>
    private static Metadata CreateTrailers(Guid correlationId)
    {
        var trailers = new Metadata
        {
            { "CorrelationId", correlationId.ToString() }
        };
        return trailers;
    }
}