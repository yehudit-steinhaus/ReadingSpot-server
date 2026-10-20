using NLog.Web;
using ReadingSpot.API.Extensions;
using ReadingSpot.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// NLog במקום ספקי הלוג המובנים. רמות הלוג נקבעות ב-appsettings.
builder.Logging.ClearProviders();
builder.Host.UseNLog(new NLogAspNetCoreOptions { RemoveLoggerFactoryFilter = false });

builder.Services
    .AddApi(builder.Configuration)
    .AddDataLayer(builder.Configuration)
    .AddServiceLayer()
    .AddJwtAuth(builder.Configuration);

var app = builder.Build();

await app.InitializeDatabaseAsync();

app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
