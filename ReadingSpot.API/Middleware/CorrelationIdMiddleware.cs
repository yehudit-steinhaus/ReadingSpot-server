using System.Diagnostics;

namespace ReadingSpot.API.Middleware;

// מזהה ייחודי לכל בקשה: נכנס ל-TraceIdentifier, מוחזר ב-header ומופיע בכל שורת לוג (דרך NLog).
public class CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
{
    public const string Header = "X-Correlation-Id";

    public Task InvokeAsync(HttpContext ctx)
    {
        var incoming = ctx.Request.Headers[Header].FirstOrDefault();
        ctx.TraceIdentifier = IsValid(incoming) ? incoming! : Guid.NewGuid().ToString("N");

        var stopwatch = Stopwatch.StartNew();

        ctx.Response.OnStarting(() =>
        {
            ctx.Response.Headers[Header] = ctx.TraceIdentifier;
            return Task.CompletedTask;
        });

        // OnCompleted – כדי לרשום את הסטטוס הסופי, גם אם ה-ExceptionMiddleware שינה אותו.
        ctx.Response.OnCompleted(() =>
        {
            logger.LogInformation("{Method} {Path} -> {Status} ({Elapsed} ms)",
                ctx.Request.Method, ctx.Request.Path, ctx.Response.StatusCode, stopwatch.ElapsedMilliseconds);
            return Task.CompletedTask;
        });

        return next(ctx);
    }

    // מונע הזרקת טקסט ארוך או חשוד ללוג.
    private static bool IsValid(string? id)
        => !string.IsNullOrWhiteSpace(id) && id.Length <= 64 && id.All(c => char.IsLetterOrDigit(c) || c == '-');
}
