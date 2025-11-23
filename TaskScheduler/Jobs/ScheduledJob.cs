using Coravel.Invocable;
using TaskScheduler.Models;

namespace TaskScheduler.Jobs;

public class ScheduledJob : IInvocable
{
    private readonly JobConfiguration _configuration;
    private readonly Services.JobExecutionService _executionService;

    public ScheduledJob(JobConfiguration configuration, Services.JobExecutionService executionService)
    {
        _configuration = configuration;
        _executionService = executionService;
    }

    public async Task Invoke()
    {
        await _executionService.ExecuteJobAsync(_configuration);
    }
}
