using EchoHub.App.Interfaces;
using EchoHub.App.Services;
using EchoHub.Core.Interfaces;
using EchoHub.Infrastructure.Data;
using EchoHub.Infrastructure.Repositories;
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

    builder.Services.AddScoped<IServerRepository, ServerRepository>();
    builder.Services.AddScoped<IServerService, ServerService>();
    builder.Services.AddHostedService<EchoHub.Infrastructure.Services.InactiveServerCleanupService>();

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
    app.MapControllers();
    app.MapHub<EchoHub.Infrastructure.Hubs.ServerHub>("/hubs/servers");

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
