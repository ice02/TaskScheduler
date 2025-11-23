using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using TaskScheduler.Models;

namespace TaskScheduler.Services;

public class JobExecutionService
{
    private readonly ILogger<JobExecutionService> _logger;
    private readonly EmailNotificationService _emailService;
    private readonly Dictionary<string, bool> _runningJobs = new();
    private readonly object _lock = new();

    public JobExecutionService(ILogger<JobExecutionService> logger, EmailNotificationService emailService)
    {
        _logger = logger;
        _emailService = emailService;
    }

    public async Task ExecuteJobAsync(JobConfiguration job)
    {
        if (!job.Enabled)
        {
            _logger.LogDebug("Job {JobName} is disabled, skipping execution", job.Name);
            return;
        }

        lock (_lock)
        {
            if (_runningJobs.ContainsKey(job.Name) && _runningJobs[job.Name])
            {
                var message = $"Job {job.Name} is already running. Skipping this execution.";
                _logger.LogWarning(message);
                _ = _emailService.SendErrorNotificationAsync(
                    $"Task Scheduler - Job Overlap: {job.Name}", 
                    message);
                return;
            }

            _runningJobs[job.Name] = true;
        }

        try
        {
            _logger.LogInformation("Starting job execution: {JobName}", job.Name);
            
            using var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromMinutes(job.MaxExecutionTimeMinutes));

            var executeTask = job.Type.ToLower() switch
            {
                "powershell" => ExecutePowerShellAsync(job, cts.Token),
                "executable" => ExecuteExecutableAsync(job, cts.Token),
                _ => throw new InvalidOperationException($"Unknown job type: {job.Type}")
            };

            await executeTask;
            
            _logger.LogInformation("Job {JobName} completed successfully", job.Name);
        }
        catch (OperationCanceledException)
        {
            var message = $"Job {job.Name} exceeded maximum execution time of {job.MaxExecutionTimeMinutes} minutes and was terminated.";
            _logger.LogError(message);
            await _emailService.SendErrorNotificationAsync(
                $"Task Scheduler - Job Timeout: {job.Name}", 
                message);
        }
        catch (Exception ex)
        {
            var message = $"Job {job.Name} failed with error: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}";
            _logger.LogError(ex, "Job {JobName} failed", job.Name);
            await _emailService.SendErrorNotificationAsync(
                $"Task Scheduler - Job Failed: {job.Name}", 
                message);
        }
        finally
        {
            lock (_lock)
            {
                _runningJobs[job.Name] = false;
            }
        }
    }

    private async Task ExecutePowerShellAsync(JobConfiguration job, CancellationToken cancellationToken)
    {
        if (!File.Exists(job.Path))
        {
            throw new FileNotFoundException($"PowerShell script not found: {job.Path}");
        }

        var processStartInfo = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-ExecutionPolicy Bypass -NoProfile -File \"{job.Path}\" {job.Arguments}",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        await ExecuteProcessAsync(processStartInfo, job.Name, cancellationToken);
    }

    private async Task ExecuteExecutableAsync(JobConfiguration job, CancellationToken cancellationToken)
    {
        if (!File.Exists(job.Path))
        {
            throw new FileNotFoundException($"Executable not found: {job.Path}");
        }

        var processStartInfo = new ProcessStartInfo
        {
            FileName = job.Path,
            Arguments = job.Arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        await ExecuteProcessAsync(processStartInfo, job.Name, cancellationToken);
    }

    private async Task ExecuteProcessAsync(ProcessStartInfo startInfo, string jobName, CancellationToken cancellationToken)
    {
        using var process = new Process { StartInfo = startInfo };
        
        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        process.OutputDataReceived += (sender, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data))
            {
                outputBuilder.AppendLine(args.Data);
                _logger.LogDebug("[{JobName}] {Output}", jobName, args.Data);
            }
        };

        process.ErrorDataReceived += (sender, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data))
            {
                errorBuilder.AppendLine(args.Data);
                _logger.LogWarning("[{JobName}] {Error}", jobName, args.Data);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            var errorMessage = errorBuilder.ToString();
            throw new InvalidOperationException(
                $"Process exited with code {process.ExitCode}. Error output: {errorMessage}");
        }

        _logger.LogInformation("[{JobName}] Process completed with exit code 0", jobName);
    }
}
