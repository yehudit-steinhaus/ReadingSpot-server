using ReadingSpot.API.Common;
using ReadingSpot.Core.Exceptions;

namespace ReadingSpot.API.Middleware;

// מיפוי חריגות לסטטוס HTTP – במקום אחד בלבד.
public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await next(ctx);
        }
        catch (AppException ex)
        {
            await ErrorResponse.WriteAsync(ctx, ex.StatusCode, ex.Message);
        }
        catch (OperationCanceledException) when (ctx.RequestAborted.IsCancellationRequested)
        {
            // הלקוח ביטל את הבקשה – אין למי להחזיר תשובה.
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            await ErrorResponse.WriteAsync(ctx, StatusCodes.Status500InternalServerError, "אירעה שגיאה בלתי צפויה. נסי שוב מאוחר יותר.");
        }
    }
}
