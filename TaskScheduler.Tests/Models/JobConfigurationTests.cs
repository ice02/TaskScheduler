using FluentAssertions;
using TaskScheduler.Models;

namespace TaskScheduler.Tests.Models;

public class JobConfigurationTests
{
    [Fact]
    public void JobConfiguration_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var job = new JobConfiguration();

        // Assert
        job.Name.Should().BeEmpty();
        job.Type.Should().BeEmpty();
        job.Path.Should().BeEmpty();
        job.Arguments.Should().BeEmpty();
        job.CronExpression.Should().BeEmpty();
        job.MaxExecutionTimeMinutes.Should().Be(0);
        job.Enabled.Should().BeFalse();
    }

    [Fact]
    public void JobConfiguration_ShouldSetAllProperties()
    {
        // Arrange & Act
        var job = new JobConfiguration
        {
            Name = "TestJob",
            Type = "PowerShell",
            Path = "C:\\Scripts\\test.ps1",
            Arguments = "-Param1 Value1",
            CronExpression = "*/5 * * * *",
            MaxExecutionTimeMinutes = 30,
            Enabled = true
        };

        // Assert
        job.Name.Should().Be("TestJob");
        job.Type.Should().Be("PowerShell");
        job.Path.Should().Be("C:\\Scripts\\test.ps1");
        job.Arguments.Should().Be("-Param1 Value1");
        job.CronExpression.Should().Be("*/5 * * * *");
        job.MaxExecutionTimeMinutes.Should().Be(30);
        job.Enabled.Should().BeTrue();
    }

    [Theory]
    [InlineData("PowerShell")]
    [InlineData("Executable")]
    public void JobConfiguration_ShouldAcceptValidJobTypes(string jobType)
    {
        // Arrange & Act
        var job = new JobConfiguration { Type = jobType };

        // Assert
        job.Type.Should().Be(jobType);
    }

    [Theory]
    [InlineData("*/5 * * * *")]
    [InlineData("0 * * * *")]
    [InlineData("0 2 * * *")]
    [InlineData("0 9 * * 1-5")]
    public void JobConfiguration_ShouldAcceptValidCronExpressions(string cronExpression)
    {
        // Arrange & Act
        var job = new JobConfiguration { CronExpression = cronExpression };

        // Assert
        job.CronExpression.Should().Be(cronExpression);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(30)]
    [InlineData(60)]
    [InlineData(120)]
    public void JobConfiguration_ShouldAcceptValidMaxExecutionTime(int minutes)
    {
        // Arrange & Act
        var job = new JobConfiguration { MaxExecutionTimeMinutes = minutes };

        // Assert
        job.MaxExecutionTimeMinutes.Should().Be(minutes);
    }
}
