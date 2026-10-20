using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ReadingSpot.API.Common;
using ReadingSpot.API.Middleware;
using ReadingSpot.Core.Common;
using ReadingSpot.Core.Interfaces;
using ReadingSpot.Data;
using ReadingSpot.Data.Repositories;
using ReadingSpot.Data.Seed;
using ReadingSpot.Service.Auth;
using ReadingSpot.Service.Bookings;
using ReadingSpot.Service.Mapping;
using ReadingSpot.Service.Spots;
using ReadingSpot.Service.TimeSlots;

namespace ReadingSpot.API.Extensions;

// כל רישומי ה-DI מרוכזים כאן, כדי ש-Program.cs יישאר קצר.
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration config)
    {
        services.AddControllers().ConfigureApiBehaviorOptions(o =>
            o.InvalidModelStateResponseFactory = ctx =>
            {
                var message = string.Join(" ", ctx.ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                return new BadRequestObjectResult(
                    new ErrorResponse(StatusCodes.Status400BadRequest, message, ctx.HttpContext.TraceIdentifier));
            });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "ReadingSpot API", Version = "v1" });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "הדביקי כאן את ה-token שהתקבל בהתחברות",
            });

            var bearerRef = new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
            };
            c.AddSecurityRequirement(new OpenApiSecurityRequirement { [bearerRef] = new List<string>() });
        });

        var origins = config.GetSection("Cors:Origins").Get<string[]>() ?? [];
        // הכתובות מ-appsettings, וגם כל כתובת מקומית (localhost / 127.0.0.1 בכל פורט) – לפיתוח.
        services.AddCors(o => o.AddDefaultPolicy(p => p
            .SetIsOriginAllowed(origin => origins.Contains(origin) || new Uri(origin).IsLoopback)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithExposedHeaders(CorrelationIdMiddleware.Header)));

        return services;
    }

    public static IServiceCollection AddDataLayer(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "חסר ConnectionStrings:Default. יש להגדיר אותו עם dotnet user-secrets (ראי README).");

        // DbContext רשום כ-Scoped (ברירת המחדל) – מופע אחד לכל בקשה.
        services.AddDbContext<AppDbContext>(o => o.UseSqlServer(connectionString));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<ISpotRepository, SpotRepository>();
        services.AddScoped<ITimeSlotRepository, TimeSlotRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();

        return services;
    }

    public static IServiceCollection AddServiceLayer(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile));

        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ISpotService, SpotService>();
        services.AddScoped<ITimeSlotService, TimeSlotService>();
        services.AddScoped<IBookingService, BookingService>();

        return services;
    }

    public static IServiceCollection AddJwtAuth(this IServiceCollection services, IConfiguration config)
    {
        var section = config.GetSection(JwtSettings.Section);

        // השרת לא יעלה בלי מפתח תקין – עדיף להיכשל מוקדם.
        services.AddOptions<JwtSettings>()
            .Bind(section)
            .Validate(s => s.Key.Length >= 32, "Jwt:Key חייב להכיל לפחות 32 תווים (הגדרה עם dotnet user-secrets).")
            .ValidateOnStart();

        var jwt = section.Get<JwtSettings>() ?? new JwtSettings();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.MapInboundClaims = false; // שמות ה-claims נשארים sub / email / role
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                    ClockSkew = TimeSpan.FromMinutes(1),
                    NameClaimType = AppClaims.Email,
                    RoleClaimType = AppClaims.Role,
                };

                // גם 401 ו-403 חוזרים באותו פורמט JSON.
                o.Events = new JwtBearerEvents
                {
                    OnChallenge = ctx =>
                    {
                        ctx.HandleResponse();
                        return ErrorResponse.WriteAsync(ctx.HttpContext, StatusCodes.Status401Unauthorized, "נדרשת התחברות.");
                    },
                    OnForbidden = ctx =>
                        ErrorResponse.WriteAsync(ctx.HttpContext, StatusCodes.Status403Forbidden, "אין לך הרשאה לפעולה זו."),
                };
            });

        return services;
    }

    // Migrations + חלונות זמן לשבוע הקרוב, בעליית השרת.
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        await DbInitializer.InitializeAsync(scope.ServiceProvider.GetRequiredService<AppDbContext>());
    }
}
