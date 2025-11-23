using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaskScheduler.Models;
using TaskScheduler.Services;

namespace TaskScheduler.Tests.Integration;

public class ServiceIntegrationTests
{
    [Fact]
    public void DependencyInjection_ShouldResolveAllServices()
    {
        // Arrange
        var services = new ServiceCollection();
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"Jobs:0:Name", "TestJob"},
                {"Jobs:0:Type", "PowerShell"},
                {"Jobs:0:Path", "C:\\test.ps1"},
                {"Jobs:0:CronExpression", "*/5 * * * *"},
                {"Jobs:0:MaxExecutionTimeMinutes", "10"},
                {"Jobs:0:Enabled", "false"}
            })
            .Build();

        var smtpSettings = new SmtpSettings { Enabled = false };

        services.AddLogging();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton(smtpSettings);
        services.AddSingleton<EmailNotificationService>();
        services.AddScoped<JobExecutionService>();

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var emailService = serviceProvider.GetService<EmailNotificationService>();
        var executionService = serviceProvider.GetService<JobExecutionService>();

        // Assert
        emailService.Should().NotBeNull();
        executionService.Should().NotBeNull();
    }

    [Fact]
    public void Configuration_ShouldLoadJobsCorrectly()
    {
        // Arrange
        var configData = new Dictionary<string, string?>
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
            {"Jobs:1:Enabled", "false"}
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        // Act
        var jobs = configuration.GetSection("Jobs").Get<List<JobConfiguration>>();

        // Assert
        jobs.Should().NotBeNull();
        jobs.Should().HaveCount(2);
        
        jobs![0].Name.Should().Be("Job1");
        jobs[0].Type.Should().Be("PowerShell");
        jobs[0].Enabled.Should().BeTrue();
        
        jobs[1].Name.Should().Be("Job2");
        jobs[1].Type.Should().Be("Executable");
        jobs[1].Enabled.Should().BeFalse();
    }

    [Fact]
    public void Configuration_ShouldLoadSmtpSettings()
    {
        // Arrange
        var configData = new Dictionary<string, string?>
        {
            {"SmtpSettings:Enabled", "true"},
            {"SmtpSettings:Host", "smtp.example.com"},
            {"SmtpSettings:Port", "587"},
            {"SmtpSettings:UseSsl", "true"},
            {"SmtpSettings:Username", "user@example.com"},
            {"SmtpSettings:Password", "password123"},
            {"SmtpSettings:FromEmail", "noreply@example.com"},
            {"SmtpSettings:FromName", "Test Service"},
            {"SmtpSettings:ToEmails:0", "admin1@example.com"},
            {"SmtpSettings:ToEmails:1", "admin2@example.com"}
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        // Act
        var smtpSettings = configuration.GetSection("SmtpSettings").Get<SmtpSettings>();

        // Assert
        smtpSettings.Should().NotBeNull();
        smtpSettings!.Enabled.Should().BeTrue();
        smtpSettings.Host.Should().Be("smtp.example.com");
        smtpSettings.Port.Should().Be(587);
        smtpSettings.UseSsl.Should().BeTrue();
        smtpSettings.Username.Should().Be("user@example.com");
        smtpSettings.Password.Should().Be("password123");
        smtpSettings.FromEmail.Should().Be("noreply@example.com");
        smtpSettings.FromName.Should().Be("Test Service");
        smtpSettings.ToEmails.Should().HaveCount(2);
        smtpSettings.ToEmails.Should().Contain("admin1@example.com");
        smtpSettings.ToEmails.Should().Contain("admin2@example.com");
    }

    [Fact]
    public void EmailNotificationService_ShouldIntegrateWithLogger()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        
        var smtpSettings = new SmtpSettings { Enabled = false };
        services.AddSingleton(smtpSettings);
        services.AddSingleton<EmailNotificationService>();

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var emailService = serviceProvider.GetService<EmailNotificationService>();

        // Assert
        emailService.Should().NotBeNull();
    }

    [Fact]
    public void JobExecutionService_ShouldIntegrateWithDependencies()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        
        var smtpSettings = new SmtpSettings { Enabled = false };
        services.AddSingleton(smtpSettings);
        services.AddSingleton<EmailNotificationService>();
        services.AddScoped<JobExecutionService>();

        var serviceProvider = services.BuildServiceProvider();

        // Act
        using var scope = serviceProvider.CreateScope();
        var executionService = scope.ServiceProvider.GetService<JobExecutionService>();

        // Assert
        executionService.Should().NotBeNull();
    }

    [Fact]
    public async Task JobExecutionService_WithDisabledJob_ShouldNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        
        var smtpSettings = new SmtpSettings { Enabled = false };
        services.AddSingleton(smtpSettings);
        services.AddSingleton<EmailNotificationService>();
        services.AddScoped<JobExecutionService>();

        var serviceProvider = services.BuildServiceProvider();
        
        var job = new JobConfiguration
        {
            Name = "DisabledJob",
            Type = "PowerShell",
            Path = "C:\\test.ps1",
            Enabled = false,
            MaxExecutionTimeMinutes = 1
        };

        // Act
        using var scope = serviceProvider.CreateScope();
        var executionService = scope.ServiceProvider.GetRequiredService<JobExecutionService>();
        var act = async () => await executionService.ExecuteJobAsync(job);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void JobSchedulerService_ShouldInitializeWithConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"Jobs:0:Name", "TestJob"},
                {"Jobs:0:Type", "PowerShell"},
                {"Jobs:0:Path", "C:\\test.ps1"},
                {"Jobs:0:CronExpression", "*/5 * * * *"},
                {"Jobs:0:MaxExecutionTimeMinutes", "10"},
                {"Jobs:0:Enabled", "false"}
            })
            .Build();

        services.AddSingleton<IConfiguration>(configuration);
        
        var smtpSettings = new SmtpSettings { Enabled = false };
        services.AddSingleton(smtpSettings);
        services.AddSingleton<EmailNotificationService>();
        services.AddScoped<JobExecutionService>();

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var act = () => new JobSchedulerService(
            serviceProvider.GetRequiredService<ILogger<JobSchedulerService>>(),
            serviceProvider,
            configuration);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Configuration_WithMissingJobFields_ShouldHandleGracefully()
    {
        // Arrange
        var configData = new Dictionary<string, string?>
        {
            {"Jobs:0:Name", "IncompleteJob"}
            // Missing other fields
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        // Act
        var jobs = configuration.GetSection("Jobs").Get<List<JobConfiguration>>();

        // Assert
        jobs.Should().NotBeNull();
        jobs.Should().HaveCount(1);
        jobs![0].Name.Should().Be("IncompleteJob");
        jobs[0].Type.Should().BeEmpty();
        jobs[0].Path.Should().BeEmpty();
    }

    [Fact]
    public void ServiceProvider_ShouldSupportMultipleScopes()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        
        var smtpSettings = new SmtpSettings { Enabled = false };
        services.AddSingleton(smtpSettings);
        services.AddSingleton<EmailNotificationService>();
        services.AddScoped<JobExecutionService>();

        var serviceProvider = services.BuildServiceProvider();

        // Act
        JobExecutionService? service1;
        JobExecutionService? service2;
        
        using (var scope1 = serviceProvider.CreateScope())
        {
            service1 = scope1.ServiceProvider.GetService<JobExecutionService>();
        }
        
        using (var scope2 = serviceProvider.CreateScope())
        {
            service2 = scope2.ServiceProvider.GetService<JobExecutionService>();
        }

        // Assert
        service1.Should().NotBeNull();
        service2.Should().NotBeNull();
        service1.Should().NotBeSameAs(service2); // Different instances due to scoped lifetime
    }

    [Fact]
    public void EmailNotificationService_ShouldBeSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        
        var smtpSettings = new SmtpSettings { Enabled = false };
        services.AddSingleton(smtpSettings);
        services.AddSingleton<EmailNotificationService>();

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var service1 = serviceProvider.GetService<EmailNotificationService>();
        var service2 = serviceProvider.GetService<EmailNotificationService>();

        // Assert
        service1.Should().NotBeNull();
        service2.Should().NotBeNull();
        service1.Should().BeSameAs(service2); // Same instance due to singleton lifetime
    }
}
