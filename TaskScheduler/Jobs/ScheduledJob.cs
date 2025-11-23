using Coravel.Invocable;
using TaskScheduler.Models;

namespace TaskScheduler.Jobs;

public class ScheduledJob : IInvocable
{
    private readonly JobConfiguration _configuration;
    private readonly Services.JobExecutionService _executionService;

    public ScheduledJob(JobConfiguration configuration, Services.JobExecutionService executionService)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _executionService = executionService ?? throw new ArgumentNullException(nameof(executionService));
    }

    public async Task Invoke()
    {
        await _executionService.ExecuteJobAsync(_configuration);
    }
}
