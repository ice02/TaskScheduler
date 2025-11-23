using Coravel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaskScheduler.Models;

namespace TaskScheduler.Services;

public class JobSchedulerService : BackgroundService
{
    private readonly ILogger<JobSchedulerService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly List<JobConfiguration> _jobs;

    public JobSchedulerService(
        ILogger<JobSchedulerService> logger,
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _jobs = configuration.GetSection("Jobs").Get<List<JobConfiguration>>() ?? new List<JobConfiguration>();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Job Scheduler Service is starting");

        if (_jobs.Count == 0)
        {
            _logger.LogWarning("No jobs configured in appsettings.json");
        }

        foreach (var job in _jobs.Where(j => j.Enabled))
        {
            _logger.LogInformation("Scheduling job: {JobName} with cron expression: {CronExpression}", 
                job.Name, job.CronExpression);

            _serviceProvider.UseScheduler(scheduler =>
            {
                scheduler.ScheduleAsync(async () =>
                {
                    using var scope = _serviceProvider.CreateScope();
                    var executionService = scope.ServiceProvider.GetRequiredService<JobExecutionService>();
                    await executionService.ExecuteJobAsync(job);
                }).Cron(job.CronExpression);
            });
        }

        _logger.LogInformation("Job Scheduler Service started successfully with {JobCount} active jobs", 
            _jobs.Count(j => j.Enabled));

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Job Scheduler Service is stopping");
        await base.StopAsync(cancellationToken);
    }
}
