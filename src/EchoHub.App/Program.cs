using EchoHub.App.Hubs;
using EchoHub.App.Services;
using EchoHub.Core.Interfaces;
using EchoHub.Core.Services;
using EchoHub.Infrastructure.Data;
using EchoHub.Infrastructure.Repositories;
using EchoHub.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting EchoHub API");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .WriteTo.Console()
        .WriteTo.File("logs/echohub-.log", rollingInterval: RollingInterval.Day));

    builder.Services.AddControllers();
    builder.Services.AddOpenApi();
    builder.Services.AddSwaggerGen();
    builder.Services.AddSignalR();

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseInMemoryDatabase("EchoHub"));

    builder.Services.AddMemoryCache();
    var ghToken = builder.Configuration["GitHub:Token"];
    var maskedToken = string.IsNullOrEmpty(ghToken) ? "(not set)" : $"{ghToken[..Math.Min(4, ghToken.Length)]}***";
    Log.Information("GitHub token: {MaskedToken}", maskedToken);

    builder.Services.AddHttpClient<IAppService, AppService>((_, client) =>
    {
        client.Timeout = TimeSpan.FromSeconds(10);
        client.DefaultRequestHeaders.UserAgent.ParseAdd("EchoHub-Web");

        if (!string.IsNullOrEmpty(ghToken))
            client.DefaultRequestHeaders.Authorization = new("Bearer", ghToken);
    });

    builder.Services.AddScoped<IServerRepository, ServerRepository>();
    builder.Services.AddScoped<IServerService, ServerService>();
    builder.Services.AddHostedService<InactiveServerCleanupService>();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowClient", policy =>
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader());
    });

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseSerilogRequestLogging();
    app.UseCors("AllowClient");
    app.MapGet("/api", () => "OK");
    app.MapControllers();
    app.MapHub<ServerHub>("/hubs/servers");

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}
