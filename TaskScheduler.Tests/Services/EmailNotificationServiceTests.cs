using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TaskScheduler.Models;
using TaskScheduler.Services;

namespace TaskScheduler.Tests.Services;

public class EmailNotificationServiceTests
{
    private readonly Mock<ILogger<EmailNotificationService>> _loggerMock;

    public EmailNotificationServiceTests()
    {
        _loggerMock = new Mock<ILogger<EmailNotificationService>>();
    }

    [Fact]
    public void Constructor_ShouldInitializeWithValidParameters()
    {
        // Arrange
        var settings = new SmtpSettings { Enabled = true };

        // Act
        var service = new EmailNotificationService(settings, _loggerMock.Object);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public async Task SendErrorNotificationAsync_WhenDisabled_ShouldLogDebugAndReturn()
    {
        // Arrange
        var settings = new SmtpSettings { Enabled = false };
        var service = new EmailNotificationService(settings, _loggerMock.Object);

        // Act
        await service.SendErrorNotificationAsync("Test Subject", "Test Body");

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Email notifications are disabled")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendErrorNotificationAsync_WhenNoRecipients_ShouldLogWarningAndReturn()
    {
        // Arrange
        var settings = new SmtpSettings
        {
            Enabled = true,
            ToEmails = new List<string>()
        };
        var service = new EmailNotificationService(settings, _loggerMock.Object);

        // Act
        await service.SendErrorNotificationAsync("Test Subject", "Test Body");

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No recipient email addresses configured")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Constructor_WithNullSmtpSettings_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new EmailNotificationService(null!, _loggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task SendErrorNotificationAsync_WithNullSubject_ShouldThrow()
    {
        // Arrange
        var settings = new SmtpSettings { Enabled = false };
        var service = new EmailNotificationService(settings, _loggerMock.Object);

        // Act
        var act = async () => await service.SendErrorNotificationAsync(null!, "Body");

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SendErrorNotificationAsync_WithNullBody_ShouldThrow()
    {
        // Arrange
        var settings = new SmtpSettings { Enabled = false };
        var service = new EmailNotificationService(settings, _loggerMock.Object);

        // Act
        var act = async () => await service.SendErrorNotificationAsync("Subject", null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SendErrorNotificationAsync_MultipleRecipients_ShouldHandleCorrectly()
    {
        // Arrange
        var settings = new SmtpSettings
        {
            Enabled = false,
            ToEmails = new List<string>
            {
                "admin1@example.com",
                "admin2@example.com",
                "admin3@example.com"
            }
        };
        var service = new EmailNotificationService(settings, _loggerMock.Object);

        // Act
        await service.SendErrorNotificationAsync("Test", "Body");

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void EmailNotificationService_WithNullLogger_ShouldThrow()
    {
        // Arrange
        var settings = new SmtpSettings { Enabled = false };

        // Act
        var act = () => new EmailNotificationService(settings, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task SendErrorNotificationAsync_WithLongBody_ShouldNotThrow()
    {
        // Arrange
        var settings = new SmtpSettings { Enabled = false };
        var service = new EmailNotificationService(settings, _loggerMock.Object);
        var longBody = new string('A', 10000);

        // Act
        var act = async () => await service.SendErrorNotificationAsync("Subject", longBody);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SendErrorNotificationAsync_WithSpecialCharacters_ShouldNotThrow()
    {
        // Arrange
        var settings = new SmtpSettings { Enabled = false };
        var service = new EmailNotificationService(settings, _loggerMock.Object);
        var specialBody = "Test with special chars: <>&\"'{}[]";

        // Act
        var act = async () => await service.SendErrorNotificationAsync("Subject", specialBody);

        // Assert
        await act.Should().NotThrowAsync();
    }
}
