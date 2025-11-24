using Coravel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using TaskScheduler.Models;
using TaskScheduler.Services;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

try
{
    Log.Information("Task Scheduler Service starting...");

    var builder = Host.CreateApplicationBuilder(args);

    builder.Services.AddSerilog();

    var smtpSettings = configuration.GetSection("SmtpSettings").Get<SmtpSettings>() 
        ?? new SmtpSettings();
    builder.Services.AddSingleton(smtpSettings);

    builder.Services.AddSingleton<EmailNotificationService>();
    builder.Services.AddScoped<JobExecutionService>();

    builder.Services.AddScheduler();

    builder.Services.AddHostedService<JobSchedulerService>();

    builder.Services.AddWindowsService(options =>
    {
        options.ServiceName = "TaskSchedulerService";
    });

    var host = builder.Build();

    host.Services.UseScheduler(scheduler =>
    {
        // Scheduler configuration will be done by JobSchedulerService
    });

    if (Environment.UserInteractive && !args.Contains("--service"))
    {
        Log.Information("Running in console mode. Press Ctrl+C to exit.");
        Console.WriteLine("??????????????????????????????????????????????????????????????");
        Console.WriteLine("?    Task Scheduler Service - Console Mode                  ?");
        Console.WriteLine("??????????????????????????????????????????????????????????????");
        Console.WriteLine();
        Console.WriteLine("? Configuration hot-reload: ENABLED");
        Console.WriteLine("  (Changes to appsettings.json will be detected automatically)");
        Console.WriteLine();
        Console.WriteLine("Press Ctrl+C to exit.");
        Console.WriteLine();
    }
    else
    {
        Log.Information("Running as Windows Service");
        Log.Information("Configuration hot-reload enabled");
    }

    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Task Scheduler Service terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
