using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TaskScheduler.Jobs;
using TaskScheduler.Models;
using TaskScheduler.Services;

namespace TaskScheduler.Tests.Jobs;

public class ScheduledJobTests
{
    private readonly Mock<JobExecutionService> _executionServiceMock;
    private readonly JobConfiguration _jobConfiguration;

    public ScheduledJobTests()
    {
        var loggerMock = new Mock<ILogger<JobExecutionService>>();
        var emailLogger = new Mock<ILogger<EmailNotificationService>>();
        var smtpSettings = new SmtpSettings { Enabled = false };
        var emailService = new EmailNotificationService(smtpSettings, emailLogger.Object);
        
        _executionServiceMock = new Mock<JobExecutionService>(loggerMock.Object, emailService);
        
        _jobConfiguration = new JobConfiguration
        {
            Name = "TestJob",
            Type = "PowerShell",
            Path = "C:\\test.ps1",
            CronExpression = "*/5 * * * *",
            MaxExecutionTimeMinutes = 10,
            Enabled = true
        };
    }

    [Fact]
    public void Constructor_ShouldInitializeWithValidParameters()
    {
        // Arrange & Act
        var job = new ScheduledJob(_jobConfiguration, _executionServiceMock.Object);

        // Assert
        job.Should().NotBeNull();
    }

    [Fact]
    public async Task Invoke_ShouldCallExecuteJobAsync()
    {
        // Arrange
        var job = new ScheduledJob(_jobConfiguration, _executionServiceMock.Object);
        
        _executionServiceMock
            .Setup(x => x.ExecuteJobAsync(It.IsAny<JobConfiguration>()))
            .Returns(Task.CompletedTask);

        // Act
        await job.Invoke();

        // Assert
        _executionServiceMock.Verify(
            x => x.ExecuteJobAsync(It.Is<JobConfiguration>(j => j.Name == "TestJob")),
            Times.Once);
    }

    [Fact]
    public async Task Invoke_ShouldPassCorrectJobConfiguration()
    {
        // Arrange
        var specificConfig = new JobConfiguration
        {
            Name = "SpecificJob",
            Type = "Executable",
            Path = "C:\\app.exe",
            Arguments = "--arg1 value1",
            CronExpression = "0 * * * *",
            MaxExecutionTimeMinutes = 30,
            Enabled = true
        };

        var job = new ScheduledJob(specificConfig, _executionServiceMock.Object);
        
        JobConfiguration? capturedConfig = null;
        _executionServiceMock
            .Setup(x => x.ExecuteJobAsync(It.IsAny<JobConfiguration>()))
            .Callback<JobConfiguration>(config => capturedConfig = config)
            .Returns(Task.CompletedTask);

        // Act
        await job.Invoke();

        // Assert
        capturedConfig.Should().NotBeNull();
        capturedConfig!.Name.Should().Be("SpecificJob");
        capturedConfig.Type.Should().Be("Executable");
        capturedConfig.Path.Should().Be("C:\\app.exe");
        capturedConfig.Arguments.Should().Be("--arg1 value1");
        capturedConfig.CronExpression.Should().Be("0 * * * *");
        capturedConfig.MaxExecutionTimeMinutes.Should().Be(30);
        capturedConfig.Enabled.Should().BeTrue();
    }

    [Fact]
    public async Task Invoke_MultipleInvocations_ShouldExecuteEachTime()
    {
        // Arrange
        var job = new ScheduledJob(_jobConfiguration, _executionServiceMock.Object);
        
        _executionServiceMock
            .Setup(x => x.ExecuteJobAsync(It.IsAny<JobConfiguration>()))
            .Returns(Task.CompletedTask);

        // Act
        await job.Invoke();
        await job.Invoke();
        await job.Invoke();

        // Assert
        _executionServiceMock.Verify(
            x => x.ExecuteJobAsync(It.IsAny<JobConfiguration>()),
            Times.Exactly(3));
    }

    [Fact]
    public async Task Invoke_WhenExecutionThrows_ShouldPropagateException()
    {
        // Arrange
        var job = new ScheduledJob(_jobConfiguration, _executionServiceMock.Object);
        
        _executionServiceMock
            .Setup(x => x.ExecuteJobAsync(It.IsAny<JobConfiguration>()))
            .ThrowsAsync(new InvalidOperationException("Test exception"));

        // Act
        var act = async () => await job.Invoke();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Test exception");
    }

    [Fact]
    public void Constructor_WithNullConfiguration_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new ScheduledJob(null!, _executionServiceMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithNullExecutionService_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new ScheduledJob(_jobConfiguration, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task Invoke_WithDisabledJob_ShouldStillCallExecuteJobAsync()
    {
        // Arrange
        var disabledConfig = new JobConfiguration
        {
            Name = "DisabledJob",
            Type = "PowerShell",
            Path = "C:\\test.ps1",
            Enabled = false,
            MaxExecutionTimeMinutes = 10
        };

        var job = new ScheduledJob(disabledConfig, _executionServiceMock.Object);
        
        _executionServiceMock
            .Setup(x => x.ExecuteJobAsync(It.IsAny<JobConfiguration>()))
            .Returns(Task.CompletedTask);

        // Act
        await job.Invoke();

        // Assert - ScheduledJob should pass to ExecutionService, 
        // which will handle the disabled state
        _executionServiceMock.Verify(
            x => x.ExecuteJobAsync(It.Is<JobConfiguration>(j => !j.Enabled)),
            Times.Once);
    }

    [Fact]
    public async Task Invoke_ConcurrentInvocations_ShouldAllComplete()
    {
        // Arrange
        var job = new ScheduledJob(_jobConfiguration, _executionServiceMock.Object);
        
        _executionServiceMock
            .Setup(x => x.ExecuteJobAsync(It.IsAny<JobConfiguration>()))
            .Returns(Task.Delay(10)); // Small delay to simulate work

        // Act
        var task1 = job.Invoke();
        var task2 = job.Invoke();
        var task3 = job.Invoke();

        await Task.WhenAll(task1, task2, task3);

        // Assert
        _executionServiceMock.Verify(
            x => x.ExecuteJobAsync(It.IsAny<JobConfiguration>()),
            Times.Exactly(3));
    }

    [Theory]
    [InlineData("PowerShell")]
    [InlineData("Executable")]
    public async Task Invoke_WithDifferentJobTypes_ShouldExecute(string jobType)
    {
        // Arrange
        var config = new JobConfiguration
        {
            Name = "TypedJob",
            Type = jobType,
            Path = "C:\\test",
            Enabled = true,
            MaxExecutionTimeMinutes = 10
        };

        var job = new ScheduledJob(config, _executionServiceMock.Object);
        
        _executionServiceMock
            .Setup(x => x.ExecuteJobAsync(It.IsAny<JobConfiguration>()))
            .Returns(Task.CompletedTask);

        // Act
        await job.Invoke();

        // Assert
        _executionServiceMock.Verify(
            x => x.ExecuteJobAsync(It.Is<JobConfiguration>(j => j.Type == jobType)),
            Times.Once);
    }

    [Fact]
    public async Task Invoke_WithArguments_ShouldPassThemThrough()
    {
        // Arrange
        var config = new JobConfiguration
        {
            Name = "JobWithArgs",
            Type = "PowerShell",
            Path = "C:\\test.ps1",
            Arguments = "-Param1 Value1 -Param2 Value2",
            Enabled = true,
            MaxExecutionTimeMinutes = 10
        };

        var job = new ScheduledJob(config, _executionServiceMock.Object);
        
        JobConfiguration? capturedConfig = null;
        _executionServiceMock
            .Setup(x => x.ExecuteJobAsync(It.IsAny<JobConfiguration>()))
            .Callback<JobConfiguration>(c => capturedConfig = c)
            .Returns(Task.CompletedTask);

        // Act
        await job.Invoke();

        // Assert
        capturedConfig.Should().NotBeNull();
        capturedConfig!.Arguments.Should().Be("-Param1 Value1 -Param2 Value2");
    }

    [Fact]
    public async Task Invoke_WithLongRunningExecution_ShouldWaitForCompletion()
    {
        // Arrange
        var job = new ScheduledJob(_jobConfiguration, _executionServiceMock.Object);
        var completionSource = new TaskCompletionSource();
        
        _executionServiceMock
            .Setup(x => x.ExecuteJobAsync(It.IsAny<JobConfiguration>()))
            .Returns(completionSource.Task);

        // Act
        var invokeTask = job.Invoke();
        
        // Verify it's still running
        invokeTask.IsCompleted.Should().BeFalse();
        
        // Complete the execution
        completionSource.SetResult();
        await invokeTask;

        // Assert
        invokeTask.IsCompleted.Should().BeTrue();
    }
}
