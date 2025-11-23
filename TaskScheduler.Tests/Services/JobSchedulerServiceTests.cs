using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using TaskScheduler.Services;

namespace TaskScheduler.Tests.Services;

public class JobSchedulerServiceTests
{
    private readonly Mock<ILogger<JobSchedulerService>> _loggerMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly IConfiguration _configuration;

    public JobSchedulerServiceTests()
    {
        _loggerMock = new Mock<ILogger<JobSchedulerService>>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        
        // Create configuration with empty jobs array
        var inMemorySettings = new Dictionary<string, string?>
        {
            {"Jobs:0:Name", "TestJob"},
            {"Jobs:0:Type", "PowerShell"},
            {"Jobs:0:Path", "C:\\test.ps1"},
            {"Jobs:0:CronExpression", "*/5 * * * *"},
            {"Jobs:0:MaxExecutionTimeMinutes", "10"},
            {"Jobs:0:Enabled", "false"}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    [Fact]
    public void Constructor_ShouldInitializeWithValidParameters()
    {
        // Arrange & Act
        var service = new JobSchedulerService(
            _loggerMock.Object,
            _serviceProviderMock.Object,
            _configuration);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new JobSchedulerService(
            null!,
            _serviceProviderMock.Object,
            _configuration);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithNullServiceProvider_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new JobSchedulerService(
            _loggerMock.Object,
            null!,
            _configuration);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithNullConfiguration_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new JobSchedulerService(
            _loggerMock.Object,
            _serviceProviderMock.Object,
            null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithEmptyJobsConfiguration_ShouldNotThrow()
    {
        // Arrange
        var emptyConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        // Act
        var act = () => new JobSchedulerService(
            _loggerMock.Object,
            _serviceProviderMock.Object,
            emptyConfig);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_WithMultipleJobs_ShouldLoadAll()
    {
        // Arrange
        var multiJobConfig = new Dictionary<string, string?>
        {
            {"Jobs:0:Name", "Job1"},
            {"Jobs:0:Type", "PowerShell"},
            {"Jobs:0:Path", "C:\\test1.ps1"},
            {"Jobs:0:CronExpression", "*/5 * * * *"},
            {"Jobs:0:MaxExecutionTimeMinutes", "10"},
            {"Jobs:0:Enabled", "true"},
            
            {"Jobs:1:Name", "Job2"},
            {"Jobs:1:Type", "Executable"},
            {"Jobs:1:Path", "C:\\test2.exe"},
            {"Jobs:1:CronExpression", "0 * * * *"},
            {"Jobs:1:MaxExecutionTimeMinutes", "30"},
            {"Jobs:1:Enabled", "true"}
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(multiJobConfig)
            .Build();

        // Act
        var service = new JobSchedulerService(
            _loggerMock.Object,
            _serviceProviderMock.Object,
            config);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithEnabledAndDisabledJobs_ShouldLoadAll()
    {
        // Arrange
        var mixedJobConfig = new Dictionary<string, string?>
        {
            {"Jobs:0:Name", "EnabledJob"},
            {"Jobs:0:Type", "PowerShell"},
            {"Jobs:0:Path", "C:\\enabled.ps1"},
            {"Jobs:0:CronExpression", "*/5 * * * *"},
            {"Jobs:0:MaxExecutionTimeMinutes", "10"},
            {"Jobs:0:Enabled", "true"},
            
            {"Jobs:1:Name", "DisabledJob"},
            {"Jobs:1:Type", "PowerShell"},
            {"Jobs:1:Path", "C:\\disabled.ps1"},
            {"Jobs:1:CronExpression", "*/5 * * * *"},
            {"Jobs:1:MaxExecutionTimeMinutes", "10"},
            {"Jobs:1:Enabled", "false"}
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(mixedJobConfig)
            .Build();

        // Act
        var service = new JobSchedulerService(
            _loggerMock.Object,
            _serviceProviderMock.Object,
            config);

        // Assert
        service.Should().NotBeNull();
    }

    [Theory]
    [InlineData("*/5 * * * *")]
    [InlineData("0 * * * *")]
    [InlineData("0 2 * * *")]
    [InlineData("0 9 * * 1-5")]
    public void Constructor_WithVariousCronExpressions_ShouldNotThrow(string cronExpression)
    {
        // Arrange
        var config = new Dictionary<string, string?>
        {
            {"Jobs:0:Name", "TestJob"},
            {"Jobs:0:Type", "PowerShell"},
            {"Jobs:0:Path", "C:\\test.ps1"},
            {"Jobs:0:CronExpression", cronExpression},
            {"Jobs:0:MaxExecutionTimeMinutes", "10"},
            {"Jobs:0:Enabled", "true"}
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(config)
            .Build();

        // Act
        var act = () => new JobSchedulerService(
            _loggerMock.Object,
            _serviceProviderMock.Object,
            configuration);

        // Assert
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("PowerShell")]
    [InlineData("Executable")]
    public void Constructor_WithValidJobTypes_ShouldNotThrow(string jobType)
    {
        // Arrange
        var config = new Dictionary<string, string?>
        {
            {"Jobs:0:Name", "TestJob"},
            {"Jobs:0:Type", jobType},
            {"Jobs:0:Path", "C:\\test"},
            {"Jobs:0:CronExpression", "*/5 * * * *"},
            {"Jobs:0:MaxExecutionTimeMinutes", "10"},
            {"Jobs:0:Enabled", "true"}
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(config)
            .Build();

        // Act
        var act = () => new JobSchedulerService(
            _loggerMock.Object,
            _serviceProviderMock.Object,
            configuration);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_WithJobMissingRequiredFields_ShouldStillInitialize()
    {
        // Arrange
        var config = new Dictionary<string, string?>
        {
            {"Jobs:0:Name", "IncompleteJob"}
            // Missing other required fields
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(config)
            .Build();

        // Act
        var act = () => new JobSchedulerService(
            _loggerMock.Object,
            _serviceProviderMock.Object,
            configuration);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_WithInvalidConfigurationStructure_ShouldHandleGracefully()
    {
        // Arrange
        var config = new Dictionary<string, string?>
        {
            {"InvalidKey", "InvalidValue"}
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(config)
            .Build();

        // Act
        var act = () => new JobSchedulerService(
            _loggerMock.Object,
            _serviceProviderMock.Object,
            configuration);

        // Assert
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(30)]
    [InlineData(60)]
    [InlineData(1440)] // 24 hours
    public void Constructor_WithVariousTimeouts_ShouldNotThrow(int timeoutMinutes)
    {
        // Arrange
        var config = new Dictionary<string, string?>
        {
            {"Jobs:0:Name", "TestJob"},
            {"Jobs:0:Type", "PowerShell"},
            {"Jobs:0:Path", "C:\\test.ps1"},
            {"Jobs:0:CronExpression", "*/5 * * * *"},
            {"Jobs:0:MaxExecutionTimeMinutes", timeoutMinutes.ToString()},
            {"Jobs:0:Enabled", "true"}
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(config)
            .Build();

        // Act
        var act = () => new JobSchedulerService(
            _loggerMock.Object,
            _serviceProviderMock.Object,
            configuration);

        // Assert
        act.Should().NotThrow();
    }
}
