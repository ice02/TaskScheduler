using FluentAssertions;
using TaskScheduler.Models;

namespace TaskScheduler.Tests.Models;

public class SmtpSettingsTests
{
    [Fact]
    public void SmtpSettings_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var settings = new SmtpSettings();

        // Assert
        settings.Enabled.Should().BeFalse();
        settings.Host.Should().BeEmpty();
        settings.Port.Should().Be(0);
        settings.UseSsl.Should().BeFalse();
        settings.Username.Should().BeEmpty();
        settings.Password.Should().BeEmpty();
        settings.FromEmail.Should().BeEmpty();
        settings.FromName.Should().BeEmpty();
        settings.ToEmails.Should().NotBeNull();
        settings.ToEmails.Should().BeEmpty();
    }

    [Fact]
    public void SmtpSettings_ShouldSetAllProperties()
    {
        // Arrange & Act
        var settings = new SmtpSettings
        {
            Enabled = true,
            Host = "smtp.example.com",
            Port = 587,
            UseSsl = true,
            Username = "user@example.com",
            Password = "password123",
            FromEmail = "noreply@example.com",
            FromName = "Test Service",
            ToEmails = new List<string> { "admin@example.com", "alerts@example.com" }
        };

        // Assert
        settings.Enabled.Should().BeTrue();
        settings.Host.Should().Be("smtp.example.com");
        settings.Port.Should().Be(587);
        settings.UseSsl.Should().BeTrue();
        settings.Username.Should().Be("user@example.com");
        settings.Password.Should().Be("password123");
        settings.FromEmail.Should().Be("noreply@example.com");
        settings.FromName.Should().Be("Test Service");
        settings.ToEmails.Should().HaveCount(2);
        settings.ToEmails.Should().Contain("admin@example.com");
        settings.ToEmails.Should().Contain("alerts@example.com");
    }

    [Theory]
    [InlineData(25)]
    [InlineData(465)]
    [InlineData(587)]
    [InlineData(2525)]
    public void SmtpSettings_ShouldAcceptCommonSmtpPorts(int port)
    {
        // Arrange & Act
        var settings = new SmtpSettings { Port = port };

        // Assert
        settings.Port.Should().Be(port);
    }

    [Fact]
    public void SmtpSettings_ShouldHandleMultipleRecipients()
    {
        // Arrange
        var emails = new List<string>
        {
            "admin1@example.com",
            "admin2@example.com",
            "admin3@example.com"
        };

        // Act
        var settings = new SmtpSettings { ToEmails = emails };

        // Assert
        settings.ToEmails.Should().HaveCount(3);
        settings.ToEmails.Should().Equal(emails);
    }

    [Fact]
    public void SmtpSettings_ToEmails_ShouldBeModifiable()
    {
        // Arrange
        var settings = new SmtpSettings();

        // Act
        settings.ToEmails.Add("test@example.com");
        settings.ToEmails.Add("admin@example.com");

        // Assert
        settings.ToEmails.Should().HaveCount(2);
        settings.ToEmails.Should().Contain("test@example.com");
        settings.ToEmails.Should().Contain("admin@example.com");
    }
}
