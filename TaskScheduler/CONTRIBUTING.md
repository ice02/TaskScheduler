# Contributing to Task Scheduler Service

Thank you for your interest in contributing to the Task Scheduler Service project! This document provides guidelines and instructions for contributors.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [Code Style](#code-style)
- [Testing](#testing)
- [Submitting Changes](#submitting-changes)
- [Reporting Bugs](#reporting-bugs)
- [Feature Requests](#feature-requests)

## Code of Conduct

### Our Standards

- Be respectful and inclusive
- Accept constructive criticism
- Focus on what's best for the project
- Show empathy towards others

### Unacceptable Behavior

- Harassment or discriminatory comments
- Trolling or insulting remarks
- Publishing private information
- Unprofessional conduct

## Getting Started

### Prerequisites

- Visual Studio 2022 or Visual Studio Code
- .NET 8.0 SDK
- Git
- Windows 10/11 or Windows Server 2022
- PowerShell 5.1 or higher

### Development Environment Setup

1. **Clone the Repository**
   ```bash
   git clone https://github.com/your-org/task-scheduler-service.git
   cd task-scheduler-service
   ```

2. **Restore Dependencies**
   ```bash
   dotnet restore
   ```

3. **Build the Project**
   ```bash
   dotnet build
   ```

4. **Run in Console Mode**
   ```bash
   dotnet run --project TaskScheduler
   ```

## Development Setup

### IDE Configuration

#### Visual Studio 2022
- Use .editorconfig for consistent formatting
- Enable Code Analysis
- Install SonarLint extension (optional)

#### Visual Studio Code
- Install C# Dev Kit extension
- Install C# extension
- Use included .editorconfig

### Recommended Extensions

- **Visual Studio 2022**:
  - ReSharper (optional)
  - SonarLint
  - Git Extensions

- **Visual Studio Code**:
  - C# Dev Kit
  - C#
  - PowerShell
  - GitLens

## Code Style

### C# Coding Standards

Follow Microsoft C# Coding Conventions:
https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions

### Naming Conventions

```csharp
// Classes: PascalCase
public class JobExecutionService { }

// Interfaces: I prefix + PascalCase
public interface IJobScheduler { }

// Methods: PascalCase
public void ExecuteJob() { }

// Private fields: _camelCase
private readonly ILogger _logger;

// Properties: PascalCase
public string JobName { get; set; }

// Parameters: camelCase
public void Execute(string jobName) { }

// Constants: PascalCase
private const int MaxRetries = 3;
```

### File Organization

```csharp
// 1. Using directives (sorted)
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

// 2. Namespace
namespace TaskScheduler.Services;

// 3. Class/Interface
public class MyService
{
    // 4. Private fields
    private readonly ILogger _logger;
    
    // 5. Constructor
    public MyService(ILogger logger)
    {
        _logger = logger;
    }
    
    // 6. Public properties
    public string Name { get; set; }
    
    // 7. Public methods
    public void PublicMethod() { }
    
    // 8. Private methods
    private void PrivateMethod() { }
}
```

### Code Comments

```csharp
// Use XML comments for public APIs
/// <summary>
/// Executes a scheduled job with the specified configuration.
/// </summary>
/// <param name="job">The job configuration to execute.</param>
/// <returns>A task representing the asynchronous operation.</returns>
public async Task ExecuteJobAsync(JobConfiguration job)
{
    // Use inline comments for complex logic
    // Check if job is already running to prevent overlap
    if (_runningJobs.ContainsKey(job.Name))
    {
        return;
    }
    
    // Execute the job
    await RunJobAsync(job);
}
```

### Error Handling

```csharp
// Always use specific exceptions
throw new ArgumentNullException(nameof(parameter));

// Use try-catch appropriately
try
{
    await ExecuteJobAsync(job);
}
catch (OperationCanceledException)
{
    _logger.LogWarning("Job was cancelled");
}
catch (Exception ex)
{
    _logger.LogError(ex, "Job execution failed");
    throw;
}
finally
{
    CleanupResources();
}
```

### Async/Await Best Practices

```csharp
// Use async all the way
public async Task ExecuteAsync()
{
    await DoWorkAsync();
}

// ConfigureAwait for library code
await operation.ConfigureAwait(false);

// Use CancellationToken
public async Task DoWorkAsync(CancellationToken cancellationToken)
{
    await Task.Delay(1000, cancellationToken);
}
```

## Testing

### Unit Tests

Create unit tests for all new functionality:

```csharp
[TestClass]
public class JobExecutionServiceTests
{
    [TestMethod]
    public async Task ExecuteJob_ValidJob_CompletesSuccessfully()
    {
        // Arrange
        var service = CreateService();
        var job = CreateValidJob();
        
        // Act
        await service.ExecuteJobAsync(job);
        
        // Assert
        Assert.IsTrue(service.JobCompleted);
    }
}
```

### Integration Tests

Test integration with external systems:

```csharp
[TestClass]
public class EmailNotificationIntegrationTests
{
    [TestMethod]
    public async Task SendEmail_ValidSettings_SendsSuccessfully()
    {
        // Test with real SMTP server (or test server)
    }
}
```

### Manual Testing

Before submitting:

1. Test in console mode
2. Test as Windows Service
3. Verify job execution
4. Check log output
5. Test email notifications
6. Verify error handling

### Test Coverage

- Aim for >80% code coverage
- Focus on critical paths
- Test error scenarios
- Test edge cases

## Submitting Changes

### Branch Naming

```
feature/short-description
bugfix/issue-number-description
hotfix/critical-issue
docs/documentation-update
refactor/component-name
```

Examples:
- `feature/add-job-dependencies`
- `bugfix/123-fix-timeout-issue`
- `docs/update-readme`

### Commit Messages

Follow Conventional Commits format:

```
<type>(<scope>): <subject>

<body>

<footer>
```

**Types:**
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation
- `style`: Formatting
- `refactor`: Code restructuring
- `test`: Adding tests
- `chore`: Maintenance

**Examples:**
```
feat(scheduler): add job dependency support

Implement job dependency checking to ensure jobs run in order.

Closes #123
```

```
fix(email): resolve SMTP timeout issue

Increase timeout and add retry logic for transient failures.

Fixes #456
```

### Pull Request Process

1. **Create Feature Branch**
   ```bash
   git checkout -b feature/my-feature
   ```

2. **Make Changes**
   - Write code
   - Add tests
   - Update documentation

3. **Commit Changes**
   ```bash
   git add .
   git commit -m "feat: add new feature"
   ```

4. **Push to Repository**
   ```bash
   git push origin feature/my-feature
   ```

5. **Create Pull Request**
   - Provide clear description
   - Reference related issues
   - Add screenshots if applicable
   - Request reviewers

6. **Address Feedback**
   - Respond to comments
   - Make requested changes
   - Update PR

7. **Merge**
   - Wait for approval
   - Ensure CI passes
   - Squash and merge

### Pull Request Template

```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Testing
- [ ] Unit tests added/updated
- [ ] Integration tests added/updated
- [ ] Manual testing completed

## Checklist
- [ ] Code follows style guidelines
- [ ] Self-review completed
- [ ] Documentation updated
- [ ] No new warnings
- [ ] Tests pass locally

## Related Issues
Closes #123
```

## Reporting Bugs

### Before Submitting

1. Check existing issues
2. Verify it's reproducible
3. Test with latest version
4. Gather diagnostic information

### Bug Report Template

```markdown
**Description**
Clear description of the bug

**To Reproduce**
1. Configure job with...
2. Run service...
3. Observe error...

**Expected Behavior**
What should happen

**Actual Behavior**
What actually happens

**Environment**
- OS: Windows Server 2022
- .NET Version: 8.0.1
- Service Version: 1.0.0

**Logs**
```
Paste relevant log entries
```

**Additional Context**
Any other relevant information
```

### Severity Levels

- **Critical**: Service crash, data loss
- **High**: Core functionality broken
- **Medium**: Feature not working as expected
- **Low**: Minor issue, cosmetic

## Feature Requests

### Feature Request Template

```markdown
**Problem Statement**
What problem does this solve?

**Proposed Solution**
How should it work?

**Alternatives Considered**
Other approaches?

**Use Cases**
Real-world scenarios

**Additional Context**
Mockups, diagrams, examples
```

## Code Review Guidelines

### As a Reviewer

- Be respectful and constructive
- Explain reasoning
- Suggest improvements
- Approve when ready

### As an Author

- Accept feedback gracefully
- Ask for clarification
- Respond promptly
- Make requested changes

## Development Workflow

```
1. Pick Issue
   ?
2. Create Branch
   ?
3. Develop Feature
   ?
4. Write Tests
   ?
5. Update Docs
   ?
6. Self Review
   ?
7. Create PR
   ?
8. Address Feedback
   ?
9. Merge
   ?
10. Delete Branch
```

## Documentation

### When to Update Documentation

- New features
- API changes
- Configuration changes
- Breaking changes

### Documentation Files to Update

- **README.md**: User-facing changes
- **ARCHITECTURE.md**: Architectural changes
- **EXAMPLES.md**: New job examples
- **CHANGELOG.md**: All changes
- **API docs**: Code comments

## Release Process

1. **Version Bump**
   - Update version in .csproj
   - Update CHANGELOG.md

2. **Testing**
   - Run all tests
   - Manual testing
   - Integration testing

3. **Documentation**
   - Update documentation
   - Update examples

4. **Release**
   - Create release branch
   - Tag version
   - Build artifacts
   - Publish release

5. **Announcement**
   - Update release notes
   - Notify users

## Getting Help

- **Questions**: Open a discussion
- **Bugs**: File an issue
- **Features**: Submit feature request
- **Security**: Email security@example.com

## License

By contributing, you agree that your contributions will be licensed under the same license as the project.

---

Thank you for contributing to Task Scheduler Service!
