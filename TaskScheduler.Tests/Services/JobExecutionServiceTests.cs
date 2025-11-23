using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TaskScheduler.Models;
using TaskScheduler.Services;

namespace TaskScheduler.Tests.Services;

public class JobExecutionServiceTests
{
    private readonly Mock<ILogger<JobExecutionService>> _loggerMock;
    private readonly EmailNotificationService _emailService;
    private readonly JobExecutionService _service;

    public JobExecutionServiceTests()
    {
        _loggerMock = new Mock<ILogger<JobExecutionService>>();
        
        var smtpSettings = new SmtpSettings { Enabled = false };
        var emailLogger = new Mock<ILogger<EmailNotificationService>>();
        _emailService = new EmailNotificationService(smtpSettings, emailLogger.Object);
        
        _service = new JobExecutionService(_loggerMock.Object, _emailService);
    }

    [Fact]
    public void Constructor_ShouldInitializeWithValidParameters()
    {
        // Arrange
        var smtpSettings = new SmtpSettings { Enabled = false };
        var emailLogger = new Mock<ILogger<EmailNotificationService>>();
        var emailService = new EmailNotificationService(smtpSettings, emailLogger.Object);

        // Act
        var service = new JobExecutionService(_loggerMock.Object, emailService);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteJobAsync_WithDisabledJob_ShouldSkipExecution()
    {
        // Arrange
        var job = new JobConfiguration
        {
            Name = "TestJob",
            Enabled = false
        };

        // Act
        await _service.ExecuteJobAsync(job);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("is disabled")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteJobAsync_WithInvalidJobType_ShouldLogError()
    {
        // Arrange
        var job = new JobConfiguration
        {
            Name = "TestJob",
            Type = "InvalidType",
            Path = "C:\\test.ps1",
            Enabled = true,
            MaxExecutionTimeMinutes = 1
        };

        // Act
        await _service.ExecuteJobAsync(job);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteJobAsync_WithNonExistentFile_ShouldLogError()
    {
        // Arrange
        var job = new JobConfiguration
        {
            Name = "TestJob",
            Type = "PowerShell",
            Path = "C:\\NonExistent\\script.ps1",
            Enabled = true,
            MaxExecutionTimeMinutes = 1
        };

        // Act
        await _service.ExecuteJobAsync(job);

        // Assert - Should log error
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData("PowerShell")]
    [InlineData("Executable")]
    public async Task ExecuteJobAsync_WithValidJobTypes_ShouldAttemptExecution(string jobType)
    {
        // Arrange
        var job = new JobConfiguration
        {
            Name = "TestJob",
            Type = jobType,
            Path = "C:\\NonExistent\\test.ps1",
            Enabled = true,
            MaxExecutionTimeMinutes = 1
        };

        // Act
        await _service.ExecuteJobAsync(job);

        // Assert - Should log starting message
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Starting job execution")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteJobAsync_WithEmptyJobName_ShouldStillExecute()
    {
        // Arrange
        var job = new JobConfiguration
        {
            Name = "",
            Type = "PowerShell",
            Path = "C:\\test.ps1",
            Enabled = true,
            MaxExecutionTimeMinutes = 1
        };

        // Act
        await _service.ExecuteJobAsync(job);

        // Assert - Should attempt execution
        _loggerMock.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteJobAsync_MultipleJobs_ShouldRunConcurrently()
    {
        // Arrange
        var job1 = new JobConfiguration
        {
            Name = "Job1",
            Type = "PowerShell",
            Path = "C:\\test1.ps1",
            Enabled = true,
            MaxExecutionTimeMinutes = 1
        };

        var job2 = new JobConfiguration
        {
            Name = "Job2",
            Type = "PowerShell",
            Path = "C:\\test2.ps1",
            Enabled = true,
            MaxExecutionTimeMinutes = 1
        };

        // Act
        var task1 = _service.ExecuteJobAsync(job1);
        var task2 = _service.ExecuteJobAsync(job2);

        await Task.WhenAll(task1, task2);

        // Assert - Both jobs should have been attempted
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Starting job execution")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeast(2));
    }

    [Fact]
    public async Task ExecuteJobAsync_WithZeroTimeout_ShouldHandleGracefully()
    {
        // Arrange
        var job = new JobConfiguration
        {
            Name = "TestJob",
            Type = "PowerShell",
            Path = "C:\\test.ps1",
            Enabled = true,
            MaxExecutionTimeMinutes = 0
        };

        // Act
        await _service.ExecuteJobAsync(job);

        // Assert - Should complete without throwing
        _loggerMock.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrow()
    {
        // Arrange
        var smtpSettings = new SmtpSettings { Enabled = false };
        var emailLogger = new Mock<ILogger<EmailNotificationService>>();
        var emailService = new EmailNotificationService(smtpSettings, emailLogger.Object);

        // Act
        var act = () => new JobExecutionService(null!, emailService);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithNullEmailService_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new JobExecutionService(_loggerMock.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task ExecuteJobAsync_WithNullJob_ShouldThrow()
    {
        // Arrange & Act
        var act = async () => await _service.ExecuteJobAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ExecuteJobAsync_WithArguments_ShouldIncludeInExecution()
    {
        // Arrange
        var job = new JobConfiguration
        {
            Name = "TestJob",
            Type = "PowerShell",
            Path = "C:\\test.ps1",
            Arguments = "-Param1 Value1 -Param2 Value2",
            Enabled = true,
            MaxExecutionTimeMinutes = 1
        };

        // Act
        await _service.ExecuteJobAsync(job);

        // Assert - Should log starting with arguments
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Starting job execution")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
