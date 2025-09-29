using Application.Services;
using Application.Services.JobServices;
using Application.Services.JobServices.Attributes;
using Coravel;
using Domain;
using Hangfire;
using Microsoft.Extensions.Options;
using Serilog;

var builder = WebApplication.CreateBuilder(args);


var configurationBuilder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("serilogs.json", false, true);

var configuration = builder.Configuration;
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configurationBuilder.Build())
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Services.AddServices(configuration);

builder.Host.UseSerilog();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = new[] { new DashboardAuthorizationFilter() }
    });
}

app.UseHttpsRedirection();

app.UseRouting();

var appSetting = app.Services.GetRequiredService<IOptionsMonitor<AppSetting>>();

app.MapGet("/",
    async context =>
    {
        await context.Response.WriteAsync($"Background job Distributed Lock Running - Instance '{appSetting.CurrentValue.AppId}'");
    });
app.MapControllers();
app.MapHangfireDashboard();

app.Services.UseScheduler(scheduler =>
{
    scheduler.OnWorker(QueueName.CoravelQueue);
    scheduler.Schedule<HeartbeatCoravelJob>()
        .Hourly()
        .PreventOverlapping(nameof(HeartbeatCoravelJob));
});

RecurringJob.AddOrUpdate<IHeartbeatHangFireJob>(nameof(HeartbeatHangFireJob),
    service => service.InvokeAsync(default),
    Cron.Hourly, TimeZoneInfo.Utc, QueueName.HangFireQueue);

app.Run();