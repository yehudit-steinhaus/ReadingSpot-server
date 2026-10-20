namespace ReadingSpot.Core.Exceptions;

// בסיס לכל החריגות העסקיות. ה-ExceptionMiddleware ממפה StatusCode לתשובת HTTP – במקום אחד בלבד.
public abstract class AppException(int statusCode, string message, Exception? inner = null)
    : Exception(message, inner)
{
    public int StatusCode { get; } = statusCode;
}

public class NotFoundException(string message) : AppException(404, message);

public class ValidationException(string message) : AppException(400, message);

public class UnauthorizedException(string message) : AppException(401, message);

public class ForbiddenException(string message = "אין לך הרשאה לפעולה זו.") : AppException(403, message);

public class ConflictException(string message, Exception? inner = null) : AppException(409, message, inner);
