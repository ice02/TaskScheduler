# .github Directory

This directory contains GitHub-specific configuration files for the Task Scheduler Service project.

## Contents

### ?? workflows/
GitHub Actions workflow files for CI/CD automation.

- **build-and-test.yml** - Builds and tests on every push/PR
- **release.yml** - Creates releases when tags are pushed
- **code-quality.yml** - Enforces code quality standards
- **dependency-check.yml** - Weekly dependency update checks

### ?? ISSUE_TEMPLATE/
Templates for creating standardized issues.

- **bug_report.md** - Report bugs
- **feature_request.md** - Suggest new features
- **question.md** - Ask questions
- **documentation.md** - Report documentation issues
- **config.yml** - Issue template configuration

### ?? Files

- **pull_request_template.md** - Template for pull requests
- **CONTRIBUTING.md** - Contribution guidelines
- **dependabot.yml** - Dependabot configuration
- **GITHUB_INTEGRATION.md** - Complete guide to GitHub integration

## Quick Links

- [Contributing Guidelines](CONTRIBUTING.md)
- [GitHub Integration Guide](GITHUB_INTEGRATION.md)
- [Bug Report Template](ISSUE_TEMPLATE/bug_report.md)
- [Feature Request Template](ISSUE_TEMPLATE/feature_request.md)

## Usage

### Creating an Issue

1. Go to Issues tab
2. Click "New issue"
3. Choose appropriate template
4. Fill in the template
5. Submit

### Creating a Pull Request

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Push to your fork
4. Create a pull request
5. The template will auto-populate

### Triggering Workflows

**Automatic:**
- Push to main/develop ? Build & Test
- Create PR ? Build & Test + Code Quality
- Push tag v* ? Release

**Manual:**
- Go to Actions tab
- Select workflow
- Click "Run workflow"

## For Maintainers

### Setting Up

1. Review [GITHUB_INTEGRATION.md](GITHUB_INTEGRATION.md)
2. Update placeholders (YOUR_USERNAME, etc.)
3. Configure branch protection
4. Enable Dependabot
5. Add repository topics

### Regular Tasks

- Review new issues weekly
- Merge Dependabot PRs
- Monitor workflow failures
- Update documentation as needed

## Documentation

See [GITHUB_INTEGRATION.md](GITHUB_INTEGRATION.md) for complete documentation.
