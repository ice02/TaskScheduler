# Contributing to Task Scheduler Service

First off, thank you for considering contributing to Task Scheduler Service! It's people like you that make this tool better for everyone.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [How Can I Contribute?](#how-can-i-contribute)
- [Development Setup](#development-setup)
- [Pull Request Process](#pull-request-process)
- [Coding Guidelines](#coding-guidelines)
- [Commit Messages](#commit-messages)
- [Testing Guidelines](#testing-guidelines)

## Code of Conduct

This project and everyone participating in it is governed by our Code of Conduct. By participating, you are expected to uphold this code.

### Our Standards

**Positive behavior includes:**
- Being respectful and inclusive
- Accepting constructive criticism gracefully
- Focusing on what's best for the community
- Showing empathy towards others

**Unacceptable behavior includes:**
- Harassment or discriminatory comments
- Trolling or insulting remarks
- Publishing others' private information
- Unprofessional conduct

## How Can I Contribute?

### Reporting Bugs

Before creating bug reports, please check existing issues. When creating a bug report, include:

- Clear title and description
- Steps to reproduce
- Expected vs actual behavior
- Environment details (OS, .NET version, etc.)
- Log files or error messages
- Screenshots if applicable

Use the bug report template in `.github/ISSUE_TEMPLATE/bug_report.md`

### Suggesting Enhancements

Enhancement suggestions are welcome! Include:

- Clear use case
- Detailed description of proposed feature
- Why this enhancement would be useful
- Possible implementation approach

Use the feature request template in `.github/ISSUE_TEMPLATE/feature_request.md`

### Documentation Improvements

Documentation improvements are always welcome:

- Fix typos or grammar
- Clarify existing documentation
- Add missing information
- Update outdated content
- Add examples

### Code Contributions

1. **Find an Issue**: Look for issues labeled `good first issue` or `help wanted`
2. **Discuss**: Comment on the issue to discuss your approach
3. **Fork**: Fork the repository
4. **Branch**: Create a feature branch
5. **Code**: Implement your changes
6. **Test**: Write and run tests
7. **Document**: Update documentation
8. **Submit**: Create a pull request

## Development Setup

### Prerequisites

- Visual Studio 2022 or VS Code
- .NET 8.0 SDK
- Git
- Windows 10/11 or Windows Server 2022
- PowerShell 5.1+

### Setup Steps

```powershell
# Clone your fork
git clone https://github.com/YOUR_USERNAME/TaskScheduler.git
cd TaskScheduler

# Add upstream remote
git remote add upstream https://github.com/ORIGINAL_OWNER/TaskScheduler.git

# Create a branch
git checkout -b feature/my-feature

# Restore dependencies
dotnet restore

# Build
dotnet build

# Run tests
dotnet test
```

### Project Structure

```
TaskScheduler/
??? TaskScheduler/           # Main application
?   ??? Models/             # Data models
?   ??? Services/           # Business logic
?   ??? Jobs/               # Job wrappers
??? TaskScheduler.Tests/    # Test project
??? .github/                # GitHub configuration
```

## Pull Request Process

### Before Submitting

1. **Update Documentation**: Ensure documentation reflects your changes
2. **Add Tests**: Include tests for new functionality
3. **Run Tests**: Ensure all tests pass locally
4. **Update CHANGELOG**: Add entry to CHANGELOG.md
5. **Check Code Style**: Follow existing code style
6. **Self-Review**: Review your own code first

### PR Guidelines

1. **Title**: Use descriptive title (e.g., "Add support for job dependencies")
2. **Description**: Fill out PR template completely
3. **Link Issues**: Reference related issues
4. **Small PRs**: Keep PRs focused and reasonably sized
5. **Commits**: Use clear commit messages

### PR Template

Use the template in `.github/pull_request_template.md`

### Review Process

1. **Automated Checks**: CI/CD must pass
2. **Code Review**: At least one approval required
3. **Discussion**: Address reviewer feedback
4. **Merge**: Maintainer will merge when ready

### After Merge

- Delete your branch
- Update your fork
- Close related issues

## Coding Guidelines

### C# Style

Follow Microsoft C# Coding Conventions:

```csharp
// Classes: PascalCase
public class JobScheduler { }

// Methods: PascalCase
public void ExecuteJob() { }

// Private fields: _camelCase
private readonly ILogger _logger;

// Properties: PascalCase
public string JobName { get; set; }

// Parameters: camelCase
public void ProcessJob(string jobName) { }

// Constants: PascalCase
private const int MaxRetries = 3;
```

### Code Organization

```csharp
// 1. Using directives (sorted)
using System;
using System.Threading.Tasks;

// 2. Namespace
namespace TaskScheduler.Services;

// 3. Class
public class MyService
{
    // 4. Fields
    private readonly ILogger _logger;
    
    // 5. Constructor
    public MyService(ILogger logger)
    {
        _logger = logger;
    }
    
    // 6. Properties
    public string Name { get; set; }
    
    // 7. Public methods
    public void PublicMethod() { }
    
    // 8. Private methods
    private void PrivateMethod() { }
}
```

### Best Practices

1. **Error Handling**: Always use try-catch appropriately
2. **Async/Await**: Use async/await for I/O operations
3. **Logging**: Log important events and errors
4. **Comments**: Comment complex logic
5. **SOLID**: Follow SOLID principles
6. **DRY**: Don't repeat yourself
7. **YAGNI**: You aren't gonna need it

### XML Documentation

Document public APIs:

```csharp
/// <summary>
/// Executes a scheduled job with the specified configuration.
/// </summary>
/// <param name="job">The job configuration to execute.</param>
/// <returns>A task representing the asynchronous operation.</returns>
/// <exception cref="ArgumentNullException">Thrown when job is null.</exception>
public async Task ExecuteJobAsync(JobConfiguration job)
{
    // Implementation
}
```

## Commit Messages

### Format

```
<type>(<scope>): <subject>

<body>

<footer>
```

### Types

- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting)
- `refactor`: Code refactoring
- `test`: Adding or updating tests
- `chore`: Maintenance tasks
- `perf`: Performance improvements

### Examples

```
feat(scheduler): add job dependency support

Implement job dependency checking to ensure jobs run in order.
Jobs can now specify prerequisites in configuration.

Closes #123
```

```
fix(email): resolve SMTP timeout issue

Increase timeout and add retry logic for transient failures.

Fixes #456
```

### Guidelines

- Use imperative mood ("add" not "added")
- Keep subject line under 50 characters
- Capitalize subject line
- No period at end of subject
- Separate subject from body with blank line
- Wrap body at 72 characters
- Reference issues in footer

## Testing Guidelines

### Test Structure

```csharp
[Fact]
public void MethodName_Scenario_ExpectedBehavior()
{
    // Arrange
    var service = new MyService();
    var input = "test";

    // Act
    var result = service.Method(input);

    // Assert
    result.Should().Be("expected");
}
```

### Test Requirements

1. **Coverage**: Aim for 80%+ code coverage
2. **All Paths**: Test happy path and error cases
3. **Edge Cases**: Include boundary conditions
4. **Isolation**: Tests should be independent
5. **Fast**: Tests should run quickly (< 1 second each)
6. **Deterministic**: Tests should always produce same result

### Test Categories

- **Unit Tests**: Test individual components
- **Integration Tests**: Test component interaction
- **Edge Cases**: Test boundary conditions
- **Error Cases**: Test error handling

### Mocking

Use Moq for mocking:

```csharp
var mockLogger = new Mock<ILogger<MyService>>();
var service = new MyService(mockLogger.Object);
```

### Assertions

Use FluentAssertions:

```csharp
result.Should().NotBeNull();
result.Should().BeOfType<JobConfiguration>();
list.Should().HaveCount(5);
```

## Documentation Guidelines

### README Updates

Update README.md when:
- Adding new features
- Changing configuration
- Updating requirements
- Modifying installation steps

### Code Comments

Comment when:
- Logic is complex
- Workarounds are used
- Performance considerations exist
- External dependencies are used

Don't comment:
- Obvious code
- Well-named methods
- Self-documenting code

### Examples

Add examples to EXAMPLES.md for:
- New job types
- New configuration options
- Common use cases
- Best practices

## Review Checklist

Before submitting PR, ensure:

- [ ] Code compiles without errors
- [ ] All tests pass
- [ ] New tests added for new functionality
- [ ] Documentation updated
- [ ] CHANGELOG.md updated
- [ ] Code follows style guidelines
- [ ] No unnecessary changes included
- [ ] Commit messages are clear
- [ ] PR template filled out

## Getting Help

- **Questions**: Open an issue with question template
- **Discussion**: Use GitHub Discussions
- **Chat**: Join our community chat (if available)
- **Documentation**: Check README and other docs

## Recognition

Contributors will be recognized in:
- CHANGELOG.md
- GitHub contributors page
- Release notes

## License

By contributing, you agree that your contributions will be licensed under the same license as the project (MIT License).

---

Thank you for contributing to Task Scheduler Service! ??
