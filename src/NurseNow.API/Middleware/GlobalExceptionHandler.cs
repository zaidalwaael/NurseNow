using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace NurseNow.API.Middleware;
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException && context.RequestAborted.IsCancellationRequested)
            return true;
        var sql = exception as SqlException ?? exception.InnerException as SqlException;
        var status = exception switch
        {
            ArgumentException => 400,
            DbUpdateConcurrencyException => 409,
            _ when sql?.Number is 2601 or 2627 or 1205 => 409,
            _ => 500
        };
        logger.LogError(exception, "Request failed. TraceId: {TraceId}", context.TraceIdentifier);
        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(new ProblemDetails { Status = status, Title = status == 409 ? "The resource changed. Refresh and retry." : status == 400 ? "Invalid request." : "An unexpected error occurred.", Extensions = { ["traceId"] = context.TraceIdentifier } }, cancellationToken);
        return true;
    }
}
