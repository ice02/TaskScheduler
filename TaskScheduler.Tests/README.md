# Task Scheduler Service - Test Suite

This project contains comprehensive unit and integration tests for the Task Scheduler Service.

## Test Framework

- **xUnit** - Testing framework
- **Moq** - Mocking framework
- **FluentAssertions** - Assertion library

## Test Structure

```
TaskScheduler.Tests/
??? Models/                      # Model tests
?   ??? JobConfigurationTests.cs
?   ??? SmtpSettingsTests.cs
?
??? Services/                    # Service tests
?   ??? EmailNotificationServiceTests.cs
?   ??? JobExecutionServiceTests.cs
?   ??? JobSchedulerServiceTests.cs
?
??? Jobs/                        # Job wrapper tests
?   ??? ScheduledJobTests.cs
?
??? Integration/                 # Integration tests
    ??? ServiceIntegrationTests.cs
```

## Running Tests

### Visual Studio

1. Open Test Explorer (Test > Test Explorer)
2. Click "Run All Tests"

### Command Line

```powershell
# Run all tests
dotnet test

# Run with detailed output
dotnet test --verbosity detailed

# Run specific test class
dotnet test --filter "FullyQualifiedName~JobConfigurationTests"

# Run tests with code coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Visual Studio Code

1. Install C# Dev Kit extension
2. Open Test Explorer
3. Run tests from the explorer

## Test Categories

### Unit Tests

#### Model Tests
- **JobConfigurationTests**: Tests for job configuration model
  - Default values initialization
  - Property setting
  - Valid job types
  - Cron expression validation
  - Execution timeout validation

- **SmtpSettingsTests**: Tests for SMTP settings model
  - Default values initialization
  - Property setting
  - Port validation
  - Multiple recipients handling

#### Service Tests

- **EmailNotificationServiceTests**: Tests for email notification service
  - Service initialization
  - Disabled notifications handling
  - Empty recipients handling
  - Input validation
  - Error handling

- **JobExecutionServiceTests**: Tests for job execution engine
  - Service initialization
  - Disabled job handling
  - Job overlap detection
  - Invalid job type handling
  - File not found handling
  - Concurrent execution
  - Timeout handling
  - Argument passing

- **JobSchedulerServiceTests**: Tests for background scheduler service
  - Service initialization
  - Configuration loading
  - Multiple jobs handling
  - Enabled/disabled jobs
  - Cron expression validation
  - Job type validation
  - Invalid configuration handling

#### Job Wrapper Tests

- **ScheduledJobTests**: Tests for Coravel job wrapper
  - Job initialization
  - Invoke method
  - Configuration passing
  - Multiple invocations
  - Exception propagation
  - Concurrent invocations
  - Job type support
  - Arguments passing

### Integration Tests

- **ServiceIntegrationTests**: End-to-end integration tests
  - Dependency injection resolution
  - Configuration loading
  - Service integration
  - Multiple scopes handling
  - Singleton vs Scoped lifetimes

## Test Coverage

The test suite covers:

- ? **Models** - 100% coverage
  - JobConfiguration
  - SmtpSettings

- ? **Services** - High coverage
  - EmailNotificationService
  - JobExecutionService
  - JobSchedulerService

- ? **Jobs** - 100% coverage
  - ScheduledJob

- ? **Integration** - Key scenarios
  - Dependency injection
  - Configuration
  - Service interaction

## Key Test Scenarios

### Happy Path Tests
- Service initialization with valid configuration
- Job execution with valid parameters
- Email notifications when enabled
- Configuration loading

### Error Handling Tests
- Null parameter validation
- Invalid configuration handling
- File not found scenarios
- Network errors (SMTP)
- Timeout scenarios

### Edge Cases
- Empty configurations
- Disabled jobs
- Concurrent execution
- Multiple recipients
- Long-running operations

## Testing Best Practices

### Arrange-Act-Assert Pattern

All tests follow the AAA pattern:

```csharp
[Fact]
public void TestName()
{
    // Arrange - Set up test data and dependencies
    var service = new MyService();
    
    // Act - Execute the code under test
    var result = service.DoSomething();
    
    // Assert - Verify the results
    result.Should().Be(expectedValue);
}
```

### Naming Convention

Tests follow the pattern: `MethodName_Scenario_ExpectedBehavior`

Examples:
- `ExecuteJobAsync_WithDisabledJob_ShouldSkipExecution`
- `Constructor_WithNullLogger_ShouldThrow`
- `SendEmailAsync_WhenDisabled_ShouldLogDebugAndReturn`

### Mocking Strategy

- Use Moq for mocking dependencies
- Mock external dependencies (file system, network)
- Don't mock the class under test
- Use `Mock<T>` for interfaces and abstract classes

### Assertions

Use FluentAssertions for readable assertions:

```csharp
// Instead of:
Assert.Equal(expected, actual);

// Use:
actual.Should().Be(expected);

// More examples:
list.Should().HaveCount(5);
result.Should().NotBeNull();
exception.Should().BeOfType<ArgumentNullException>();
```

## Continuous Integration

Tests are designed to run in CI/CD pipelines:

```yaml
# Example GitHub Actions
- name: Run tests
  run: dotnet test --logger trx --results-directory "TestResults"
  
- name: Upload test results
  uses: actions/upload-artifact@v2
  with:
    name: test-results
    path: TestResults
```

## Known Limitations

### Tests That Require Real Resources

Some tests are limited because they require actual resources:

1. **SMTP Connection Tests**
   - Cannot test actual email sending without SMTP server
   - Tests verify logic, not actual network calls

2. **PowerShell/Executable Execution**
   - Tests verify execution logic, not actual script execution
   - Real execution tests would require specific scripts/executables

3. **File System Operations**
   - Tests use non-existent paths to verify error handling
   - Real file operations not tested to avoid test environment pollution

### Workarounds

For real-world testing:

1. **Manual Integration Tests**
   - Run service in console mode
   - Use test scripts and executables
   - Verify logs and email notifications

2. **Test Environment**
   - Set up test SMTP server (e.g., Papercut, MailHog)
   - Create test scripts in known locations
   - Use test configuration file

## Adding New Tests

### Template for New Test Class

```csharp
using FluentAssertions;
using Moq;
using Xunit;

namespace TaskScheduler.Tests.Services;

public class MyNewServiceTests
{
    private readonly Mock<IDependency> _dependencyMock;
    private readonly MyNewService _service;

    public MyNewServiceTests()
    {
        _dependencyMock = new Mock<IDependency>();
        _service = new MyNewService(_dependencyMock.Object);
    }

    [Fact]
    public void MethodName_Scenario_ExpectedBehavior()
    {
        // Arrange
        var input = "test";

        // Act
        var result = _service.MethodName(input);

        // Assert
        result.Should().Be("expected");
    }

    [Theory]
    [InlineData("input1", "output1")]
    [InlineData("input2", "output2")]
    public void MethodName_WithVariousInputs_ReturnsCorrectOutput(
        string input, 
        string expected)
    {
        // Arrange & Act
        var result = _service.MethodName(input);

        // Assert
        result.Should().Be(expected);
    }
}
```

### Test Checklist

When adding new tests, ensure:

- [ ] Test follows AAA pattern
- [ ] Test name is descriptive
- [ ] Uses FluentAssertions
- [ ] Mocks external dependencies
- [ ] Tests both happy path and error cases
- [ ] Includes edge cases
- [ ] Uses Theory for parametrized tests
- [ ] Properly disposes resources
- [ ] Doesn't depend on other tests
- [ ] Runs quickly (< 1 second)

## Troubleshooting Tests

### Test Not Found

```powershell
# Rebuild test project
dotnet build TaskScheduler.Tests

# Clean and rebuild
dotnet clean
dotnet build
```

### Test Failing Intermittently

- Check for timing issues (use appropriate waits)
- Ensure tests are isolated (no shared state)
- Verify mocks are reset between tests

### Debugging Tests

In Visual Studio:
1. Set breakpoint in test
2. Right-click test ? Debug

In VS Code:
1. Set breakpoint
2. Use Debug Test option

## Test Metrics

Current test metrics:

- **Total Tests**: 80+
- **Test Classes**: 7
- **Average Execution Time**: < 5 seconds
- **Pass Rate**: 100%

## Contributing Tests

When contributing new tests:

1. Follow existing patterns
2. Ensure all tests pass locally
3. Add descriptive test names
4. Include both positive and negative cases
5. Update this README if adding new test categories

## Resources

- [xUnit Documentation](https://xunit.net/)
- [Moq Documentation](https://github.com/moq/moq4)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [.NET Testing Best Practices](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)

---

**Last Updated**: 2024-01-15  
**Test Framework Version**: xUnit 2.5.3  
**Code Coverage Goal**: 80%+
