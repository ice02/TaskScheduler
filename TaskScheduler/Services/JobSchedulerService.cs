using Coravel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using TaskScheduler.Models;

namespace TaskScheduler.Services;

public class JobSchedulerService : BackgroundService
{
    private readonly ILogger<JobSchedulerService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private List<JobConfiguration> _jobs;
    private IDisposable? _configReloadToken;
    private readonly object _reloadLock = new();

    public JobSchedulerService(
        ILogger<JobSchedulerService> logger,
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        _jobs = _configuration.GetSection("Jobs").Get<List<JobConfiguration>>() ?? new List<JobConfiguration>();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Job Scheduler Service is starting");

        // Initial job scheduling
        ScheduleAllJobs();

        // Watch for configuration changes
        ChangeToken.OnChange(
            () => _configuration.GetReloadToken(),
            () => OnConfigurationChanged());

        _logger.LogInformation("Configuration file monitoring enabled - jobs will reload automatically on changes");

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private void ScheduleAllJobs()
    {
        lock (_reloadLock)
        {
            if (_jobs.Count == 0)
            {
                _logger.LogWarning("No jobs configured in appsettings.json");
                return;
            }

            var enabledJobs = _jobs.Where(j => j.Enabled).ToList();
            
            if (enabledJobs.Count == 0)
            {
                _logger.LogWarning("No enabled jobs found in configuration");
                return;
            }

            foreach (var job in enabledJobs)
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
                enabledJobs.Count);
        }
    }

    private void OnConfigurationChanged()
    {
        lock (_reloadLock)
        {
            try
            {
                _logger.LogInformation("Configuration file changed detected - reloading jobs...");

                // Reload jobs from configuration
                var newJobs = _configuration.GetSection("Jobs").Get<List<JobConfiguration>>() 
                    ?? new List<JobConfiguration>();

                // Check if jobs have actually changed
                if (JobsAreEqual(_jobs, newJobs))
                {
                    _logger.LogInformation("No changes detected in job configuration");
                    return;
                }

                _logger.LogInformation("Job configuration changes detected:");
                _logger.LogInformation("  - Old job count: {OldCount} (enabled: {OldEnabled})", 
                    _jobs.Count, _jobs.Count(j => j.Enabled));
                _logger.LogInformation("  - New job count: {NewCount} (enabled: {NewEnabled})", 
                    newJobs.Count, newJobs.Count(j => j.Enabled));

                // Update jobs list
                _jobs = newJobs;

                // Note: Coravel doesn't support dynamic unscheduling of jobs
                // The service needs to be restarted for changes to take full effect
                _logger.LogWarning("Job configuration reloaded. NOTE: To fully apply changes, please restart the service.");
                _logger.LogInformation("New jobs will be scheduled, but old jobs will continue running until service restart.");

                // Schedule new jobs
                ScheduleAllJobs();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reloading configuration");
            }
        }
    }

    private bool JobsAreEqual(List<JobConfiguration> jobs1, List<JobConfiguration> jobs2)
    {
        if (jobs1.Count != jobs2.Count)
            return false;

        for (int i = 0; i < jobs1.Count; i++)
        {
            var job1 = jobs1[i];
            var job2 = jobs2[i];

            if (job1.Name != job2.Name ||
                job1.Type != job2.Type ||
                job1.Path != job2.Path ||
                job1.Arguments != job2.Arguments ||
                job1.CronExpression != job2.CronExpression ||
                job1.MaxExecutionTimeMinutes != job2.MaxExecutionTimeMinutes ||
                job1.Enabled != job2.Enabled)
            {
                return false;
            }
        }

        return true;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Job Scheduler Service is stopping");
        _configReloadToken?.Dispose();
        await base.StopAsync(cancellationToken);
    }
}
