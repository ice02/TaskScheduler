# Test Execution Commands

## Basic Test Execution

### Run All Tests
```powershell
dotnet test
```

### Run with Detailed Output
```powershell
dotnet test --verbosity detailed
```

### Run with Minimal Output
```powershell
dotnet test --verbosity minimal
```

## Running Specific Tests

### Run Tests from Specific Class
```powershell
dotnet test --filter "FullyQualifiedName~JobConfigurationTests"
```

### Run Tests from Specific Namespace
```powershell
dotnet test --filter "FullyQualifiedName~TaskScheduler.Tests.Models"
```

### Run Tests by Name Pattern
```powershell
dotnet test --filter "Name~EmailNotification"
```

### Run Single Test
```powershell
dotnet test --filter "FullyQualifiedName=TaskScheduler.Tests.Models.JobConfigurationTests.JobConfiguration_ShouldInitializeWithDefaultValues"
```

## Code Coverage

### Collect Coverage (Windows)
```powershell
dotnet test --collect:"XPlat Code Coverage"
```

### Generate HTML Report
```powershell
# Install reportgenerator tool (first time only)
dotnet tool install --global dotnet-reportgenerator-globaltool

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Generate HTML report
reportgenerator -reports:"**\coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html

# Open report
start coveragereport\index.html
```

### Coverage with Threshold
```powershell
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:Threshold=80
```

## Test Results

### Export Test Results to File
```powershell
dotnet test --logger "trx;LogFileName=test-results.trx"
```

### Export to Multiple Formats
```powershell
dotnet test --logger "trx" --logger "html"
```

## Debugging Tests

### List All Tests (Without Running)
```powershell
dotnet test --list-tests
```

### Run Tests in Debug Mode
```powershell
# In Visual Studio: Right-click test ? Debug Test
# In VS Code: Use Debug Test option
```

## Performance Testing

### Run Tests with Performance Metrics
```powershell
dotnet test --collect:"Code Coverage" --collect:"XPlat Code Coverage" --verbosity detailed
```

### Run Tests with Timeout
```powershell
# Tests that exceed 60 seconds will be killed
dotnet test -- RunConfiguration.TestSessionTimeout=60000
```

## Continuous Integration

### GitHub Actions Example
```yaml
name: Run Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: windows-latest
    
    steps:
    - uses: actions/checkout@v2
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v1
      with:
        dotnet-version: 8.0.x
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore
    
    - name: Test
      run: dotnet test --no-build --verbosity normal --collect:"XPlat Code Coverage"
    
    - name: Upload coverage
      uses: codecov/codecov-action@v2
      with:
        files: '**/coverage.cobertura.xml'
```

### Azure DevOps Pipeline Example
```yaml
trigger:
- main

pool:
  vmImage: 'windows-latest'

steps:
- task: UseDotNet@2
  inputs:
    version: '8.0.x'

- task: DotNetCoreCLI@2
  displayName: 'Restore'
  inputs:
    command: 'restore'

- task: DotNetCoreCLI@2
  displayName: 'Build'
  inputs:
    command: 'build'
    arguments: '--configuration Release'

- task: DotNetCoreCLI@2
  displayName: 'Test'
  inputs:
    command: 'test'
    arguments: '--configuration Release --collect:"XPlat Code Coverage"'
    publishTestResults: true

- task: PublishCodeCoverageResults@1
  inputs:
    codeCoverageTool: 'Cobertura'
    summaryFileLocation: '$(Agent.TempDirectory)/**/coverage.cobertura.xml'
```

## Watch Mode

### Run Tests in Watch Mode
```powershell
dotnet watch test
```

This will automatically re-run tests when code changes are detected.

## Parallel Execution

### Run Tests in Parallel (Default)
```powershell
dotnet test --parallel
```

### Run Tests Sequentially
```powershell
dotnet test --parallel false
```

### Set Max Parallel Jobs
```powershell
dotnet test --parallel --max-parallel-jobs:4
```

## Filtering Tests

### Run Only Unit Tests (Convention)
```powershell
dotnet test --filter "FullyQualifiedName!~Integration"
```

### Run Only Integration Tests
```powershell
dotnet test --filter "FullyQualifiedName~Integration"
```

### Run Tests by Category (if using Trait attribute)
```powershell
dotnet test --filter "Category=Unit"
dotnet test --filter "Category=Integration"
```

## Environment Variables

### Set Environment Variables for Tests
```powershell
$env:ASPNETCORE_ENVIRONMENT="Testing"
dotnet test
```

## Common Issues and Solutions

### Issue: Tests Not Found
```powershell
# Solution: Clean and rebuild
dotnet clean
dotnet build
dotnet test
```

### Issue: Test Timeout
```powershell
# Solution: Increase timeout
dotnet test -- RunConfiguration.TestSessionTimeout=120000
```

### Issue: Coverage Report Not Generated
```powershell
# Solution: Ensure collector is installed
dotnet add package coverlet.collector
dotnet test --collect:"XPlat Code Coverage"
```

## Quick Reference

| Command | Description |
|---------|-------------|
| `dotnet test` | Run all tests |
| `dotnet test --filter "Name~Test"` | Filter by name |
| `dotnet test --collect:"XPlat Code Coverage"` | With coverage |
| `dotnet test --logger trx` | Export results |
| `dotnet test --list-tests` | List tests |
| `dotnet watch test` | Watch mode |
| `dotnet test --verbosity detailed` | Detailed output |

## Test Organization

### Recommended Test Execution Order

1. **Quick Validation** (< 1 second)
   ```powershell
   dotnet test --filter "FullyQualifiedName~Models"
   ```

2. **Service Tests** (~5 seconds)
   ```powershell
   dotnet test --filter "FullyQualifiedName~Services"
   ```

3. **Integration Tests** (~10 seconds)
   ```powershell
   dotnet test --filter "FullyQualifiedName~Integration"
   ```

4. **Full Test Suite** (~20 seconds)
   ```powershell
   dotnet test
   ```

## Performance Tips

- Use `--no-build` if already built
- Use `--no-restore` if dependencies are restored
- Run unit tests before integration tests
- Use `--filter` for focused testing
- Use parallel execution (default)

## Example Workflows

### Development Workflow
```powershell
# 1. Make code changes
# 2. Run related tests
dotnet test --filter "FullyQualifiedName~MyChangedClass"

# 3. If pass, run all unit tests
dotnet test --filter "FullyQualifiedName!~Integration"

# 4. If pass, run full suite
dotnet test
```

### Pre-Commit Workflow
```powershell
# Run all tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Verify coverage meets threshold
reportgenerator -reports:"**\coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
```

### CI/CD Workflow
```powershell
# Restore
dotnet restore

# Build
dotnet build --no-restore --configuration Release

# Test
dotnet test --no-build --configuration Release --collect:"XPlat Code Coverage" --logger "trx;LogFileName=test-results.trx"

# Generate report
reportgenerator -reports:"**\coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:HtmlInline_AzurePipelines
```

---

For more information, see [README.md](README.md) in this directory.
