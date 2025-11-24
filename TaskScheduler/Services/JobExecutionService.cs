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
    private readonly Dictionary<string, int> _jobExecutionCount = new();
    private readonly object _lock = new();

    public JobExecutionService(ILogger<JobExecutionService> logger, EmailNotificationService emailService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
    }

    public async Task ExecuteJobAsync(JobConfiguration job)
    {
        ArgumentNullException.ThrowIfNull(job);

        // Check if job is enabled
        if (!job.Enabled)
        {
            _logger.LogDebug("Job {JobName} is disabled, skipping execution", job.Name);
            return;
        }

        // Check for job overlap
        lock (_lock)
        {
            if (_runningJobs.ContainsKey(job.Name) && _runningJobs[job.Name])
            {
                var message = $"Job {job.Name} is already running. Skipping this execution.";
                _logger.LogWarning("???????????????????????????????????????????????????????????");
                _logger.LogWarning("JOB OVERLAP DETECTED");
                _logger.LogWarning("Job Name: {JobName}", job.Name);
                _logger.LogWarning("Status: SKIPPED (already running)");
                _logger.LogWarning("???????????????????????????????????????????????????????????");
                
                _ = _emailService.SendErrorNotificationAsync(
                    $"Task Scheduler - Job Overlap: {job.Name}", 
                    message);
                return;
            }

            _runningJobs[job.Name] = true;
            
            // Increment execution counter
            if (!_jobExecutionCount.ContainsKey(job.Name))
                _jobExecutionCount[job.Name] = 0;
            _jobExecutionCount[job.Name]++;
        }

        var executionId = Guid.NewGuid().ToString("N")[..8];
        var executionNumber = _jobExecutionCount[job.Name];
        var startTime = DateTime.Now;
        var stopwatch = Stopwatch.StartNew();

        // Log job start with detailed information
        _logger.LogInformation("????????????????????????????????????????????????????????????");
        _logger.LogInformation("? JOB EXECUTION STARTED");
        _logger.LogInformation("????????????????????????????????????????????????????????????");
        _logger.LogInformation("? Job Name: {JobName}", job.Name);
        _logger.LogInformation("? Execution ID: {ExecutionId}", executionId);
        _logger.LogInformation("? Execution #: {ExecutionNumber}", executionNumber);
        _logger.LogInformation("? Job Type: {JobType}", job.Type);
        _logger.LogInformation("? Script/Executable: {Path}", job.Path);
        _logger.LogInformation("? Arguments: {Arguments}", string.IsNullOrEmpty(job.Arguments) ? "(none)" : job.Arguments);
        _logger.LogInformation("? Max Execution Time: {MaxTime} minutes", job.MaxExecutionTimeMinutes);
        _logger.LogInformation("? Start Time: {StartTime:yyyy-MM-dd HH:mm:ss.fff}", startTime);
        _logger.LogInformation("????????????????????????????????????????????????????????????");

        string status = "UNKNOWN";
        string errorDetails = string.Empty;
        int? exitCode = null;

        try
        {
            using var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromMinutes(job.MaxExecutionTimeMinutes));

            var executeTask = job.Type.ToLower() switch
            {
                "powershell" => ExecutePowerShellAsync(job, cts.Token),
                "executable" => ExecuteExecutableAsync(job, cts.Token),
                _ => throw new InvalidOperationException($"Unknown job type: {job.Type}")
            };

            var result = await executeTask;
            exitCode = result.ExitCode;
            
            status = "SUCCESS";
            stopwatch.Stop();
            
            // Log successful completion
            _logger.LogInformation("????????????????????????????????????????????????????????????");
            _logger.LogInformation("? JOB EXECUTION COMPLETED");
            _logger.LogInformation("????????????????????????????????????????????????????????????");
            _logger.LogInformation("? Job Name: {JobName}", job.Name);
            _logger.LogInformation("? Execution ID: {ExecutionId}", executionId);
            _logger.LogInformation("? Status: ? {Status}", status);
            _logger.LogInformation("? Exit Code: {ExitCode}", exitCode);
            _logger.LogInformation("? Duration: {Duration:hh\\:mm\\:ss\\.fff}", stopwatch.Elapsed);
            _logger.LogInformation("? End Time: {EndTime:yyyy-MM-dd HH:mm:ss.fff}", DateTime.Now);
            _logger.LogInformation("????????????????????????????????????????????????????????????");
        }
        catch (OperationCanceledException)
        {
            status = "TIMEOUT";
            stopwatch.Stop();
            errorDetails = $"Job exceeded maximum execution time of {job.MaxExecutionTimeMinutes} minutes and was terminated.";
            
            _logger.LogError("????????????????????????????????????????????????????????????");
            _logger.LogError("? JOB EXECUTION TIMEOUT");
            _logger.LogError("????????????????????????????????????????????????????????????");
            _logger.LogError("? Job Name: {JobName}", job.Name);
            _logger.LogError("? Execution ID: {ExecutionId}", executionId);
            _logger.LogError("? Status: ? {Status}", status);
            _logger.LogError("? Max Time: {MaxTime} minutes", job.MaxExecutionTimeMinutes);
            _logger.LogError("? Actual Duration: {Duration:hh\\:mm\\:ss\\.fff}", stopwatch.Elapsed);
            _logger.LogError("? End Time: {EndTime:yyyy-MM-dd HH:mm:ss.fff}", DateTime.Now);
            _logger.LogError("? Error: {Error}", errorDetails);
            _logger.LogError("????????????????????????????????????????????????????????????");
            
            await _emailService.SendErrorNotificationAsync(
                $"Task Scheduler - Job Timeout: {job.Name}", 
                errorDetails);
        }
        catch (Exception ex)
        {
            status = "FAILED";
            stopwatch.Stop();
            errorDetails = ex.Message;
            exitCode = ex is InvalidOperationException ? -1 : null;
            
            _logger.LogError("????????????????????????????????????????????????????????????");
            _logger.LogError("? JOB EXECUTION FAILED");
            _logger.LogError("????????????????????????????????????????????????????????????");
            _logger.LogError("? Job Name: {JobName}", job.Name);
            _logger.LogError("? Execution ID: {ExecutionId}", executionId);
            _logger.LogError("? Status: ? {Status}", status);
            _logger.LogError("? Duration: {Duration:hh\\:mm\\:ss\\.fff}", stopwatch.Elapsed);
            _logger.LogError("? End Time: {EndTime:yyyy-MM-dd HH:mm:ss.fff}", DateTime.Now);
            _logger.LogError("? Error Type: {ErrorType}", ex.GetType().Name);
            _logger.LogError("? Error Message: {ErrorMessage}", ex.Message);
            if (exitCode.HasValue)
            {
                _logger.LogError("? Exit Code: {ExitCode}", exitCode);
            }
            _logger.LogError("????????????????????????????????????????????????????????????");
            _logger.LogDebug("Stack Trace: {StackTrace}", ex.StackTrace);
            
            var message = $"Job {job.Name} failed with error: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}";
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
            
            // Log summary
            _logger.LogInformation("Job {JobName} execution summary - Status: {Status}, Duration: {Duration:hh\\:mm\\:ss\\.fff}, Execution #{ExecutionNumber}", 
                job.Name, status, stopwatch.Elapsed, executionNumber);
        }
    }

    private async Task<(int ExitCode, string Output, string Error)> ExecutePowerShellAsync(JobConfiguration job, CancellationToken cancellationToken)
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

        return await ExecuteProcessAsync(processStartInfo, job.Name, cancellationToken);
    }

    private async Task<(int ExitCode, string Output, string Error)> ExecuteExecutableAsync(JobConfiguration job, CancellationToken cancellationToken)
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

        return await ExecuteProcessAsync(processStartInfo, job.Name, cancellationToken);
    }

    private async Task<(int ExitCode, string Output, string Error)> ExecuteProcessAsync(
        ProcessStartInfo startInfo, 
        string jobName, 
        CancellationToken cancellationToken)
    {
        using var process = new Process { StartInfo = startInfo };
        
        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        process.OutputDataReceived += (sender, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data))
            {
                outputBuilder.AppendLine(args.Data);
                _logger.LogDebug("[{JobName}] OUTPUT: {Output}", jobName, args.Data);
            }
        };

        process.ErrorDataReceived += (sender, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data))
            {
                errorBuilder.AppendLine(args.Data);
                _logger.LogWarning("[{JobName}] ERROR: {Error}", jobName, args.Data);
            }
        };

        _logger.LogDebug("[{JobName}] Starting process: {FileName} {Arguments}", 
            jobName, startInfo.FileName, startInfo.Arguments);

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync(cancellationToken);

        var exitCode = process.ExitCode;
        var output = outputBuilder.ToString();
        var error = errorBuilder.ToString();

        if (exitCode != 0)
        {
            _logger.LogWarning("[{JobName}] Process exited with non-zero code: {ExitCode}", jobName, exitCode);
            throw new InvalidOperationException(
                $"Process exited with code {exitCode}. Error output: {error}");
        }

        _logger.LogDebug("[{JobName}] Process completed successfully with exit code 0", jobName);
        
        return (exitCode, output, error);
    }
}
