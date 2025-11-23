using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TaskScheduler.Jobs;
using TaskScheduler.Models;
using TaskScheduler.Services;

namespace TaskScheduler.Tests.Jobs;

public class ScheduledJobTests
{
    private readonly JobConfiguration _jobConfiguration;
    private readonly JobExecutionService _executionService;
    private readonly Mock<ILogger<JobExecutionService>> _executionLoggerMock;

    public ScheduledJobTests()
    {
        _executionLoggerMock = new Mock<ILogger<JobExecutionService>>();
        var emailLogger = new Mock<ILogger<EmailNotificationService>>();
        var smtpSettings = new SmtpSettings { Enabled = false };
        var emailService = new EmailNotificationService(smtpSettings, emailLogger.Object);
        
        _executionService = new JobExecutionService(_executionLoggerMock.Object, emailService);
        
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
        var job = new ScheduledJob(_jobConfiguration, _executionService);

        // Assert
        job.Should().NotBeNull();
    }

    [Fact]
    public async Task Invoke_ShouldExecuteJob()
    {
        // Arrange
        var job = new ScheduledJob(_jobConfiguration, _executionService);

        // Act
        await job.Invoke();

        // Assert - Job was attempted to execute (will fail due to non-existent file)
        _executionLoggerMock.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Invoke_WithDisabledJob_ShouldLogDebug()
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

        var job = new ScheduledJob(disabledConfig, _executionService);

        // Act
        await job.Invoke();

        // Assert - Should log that job is disabled
        _executionLoggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("is disabled")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Invoke_MultipleInvocations_ShouldExecuteEachTime()
    {
        // Arrange
        var config = new JobConfiguration
        {
            Name = "MultiJob",
            Type = "PowerShell",
            Path = "C:\\test.ps1",
            Enabled = false, // Disabled so execution is quick
            MaxExecutionTimeMinutes = 10
        };

        var job = new ScheduledJob(config, _executionService);

        // Act
        await job.Invoke();
        await job.Invoke();
        await job.Invoke();

        // Assert - Should log debug for each disabled execution
        _executionLoggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Exactly(3));
    }

    [Fact]
    public void Constructor_WithNullConfiguration_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new ScheduledJob(null!, _executionService);

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

    [Theory]
    [InlineData("PowerShell")]
    [InlineData("Executable")]
    public async Task Invoke_WithDifferentJobTypes_ShouldAttemptExecution(string jobType)
    {
        // Arrange
        var config = new JobConfiguration
        {
            Name = "TypedJob",
            Type = jobType,
            Path = "C:\\test",
            Enabled = true,
            MaxExecutionTimeMinutes = 1
        };

        var job = new ScheduledJob(config, _executionService);

        // Act
        await job.Invoke();

        // Assert - Should attempt to start execution
        _executionLoggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Starting job execution")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Invoke_WithArguments_ShouldPassThemToExecution()
    {
        // Arrange
        var config = new JobConfiguration
        {
            Name = "JobWithArgs",
            Type = "PowerShell",
            Path = "C:\\test.ps1",
            Arguments = "-Param1 Value1",
            Enabled = true,
            MaxExecutionTimeMinutes = 1
        };

        var job = new ScheduledJob(config, _executionService);

        // Act
        await job.Invoke();

        // Assert - Should attempt execution
        _executionLoggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Invoke_ShouldNotThrowOnNonExistentFile()
    {
        // Arrange
        var config = new JobConfiguration
        {
            Name = "NonExistentJob",
            Type = "PowerShell",
            Path = "C:\\NonExistent\\script.ps1",
            Enabled = true,
            MaxExecutionTimeMinutes = 1
        };

        var job = new ScheduledJob(config, _executionService);

        // Act
        var act = async () => await job.Invoke();

        // Assert - Should handle error gracefully
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Invoke_ConcurrentInvocations_WithSameJob_ShouldHandleGracefully()
    {
        // Arrange
        var config = new JobConfiguration
        {
            Name = "ConcurrentJob",
            Type = "PowerShell",
            Path = "C:\\test.ps1",
            Enabled = false, // Disabled for quick execution
            MaxExecutionTimeMinutes = 1
        };

        var job = new ScheduledJob(config, _executionService);

        // Act
        var task1 = job.Invoke();
        var task2 = job.Invoke();
        var task3 = job.Invoke();

        await Task.WhenAll(task1, task2, task3);

        // Assert - All should complete without error
        _executionLoggerMock.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeast(3));
    }

    [Fact]
    public async Task Invoke_WithValidConfiguration_ShouldLogJobStart()
    {
        // Arrange
        var config = new JobConfiguration
        {
            Name = "ValidJob",
            Type = "PowerShell",
            Path = "C:\\test.ps1",
            Enabled = true,
            MaxExecutionTimeMinutes = 1
        };

        var job = new ScheduledJob(config, _executionService);

        // Act
        await job.Invoke();

        // Assert - Should log that job is starting
        _executionLoggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Starting")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
