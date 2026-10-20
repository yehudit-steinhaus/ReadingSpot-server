namespace ReadingSpot.API.Common;

// פורמט אחיד לכל תשובת שגיאה: חריגות, ולידציה, 401 ו-403.
public record ErrorResponse(int Status, string Message, string CorrelationId)
{
    public static Task WriteAsync(HttpContext ctx, int status, string message)
    {
        ctx.Response.StatusCode = status;
        return ctx.Response.WriteAsJsonAsync(new ErrorResponse(status, message, ctx.TraceIdentifier));
    }
}
