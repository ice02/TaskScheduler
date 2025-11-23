# GitHub Integration Guide

This document explains all the GitHub files and workflows included in this project.

## Table of Contents

- [GitHub Actions Workflows](#github-actions-workflows)
- [Issue Templates](#issue-templates)
- [Pull Request Template](#pull-request-template)
- [Community Files](#community-files)
- [Automation](#automation)

## GitHub Actions Workflows

### 1. Build and Test (`build-and-test.yml`)

**Triggers:**
- Push to `main` or `develop` branches
- Pull requests to `main` or `develop` branches

**What it does:**
- Checks out code
- Sets up .NET 8
- Restores dependencies
- Builds the project
- Runs all tests
- Collects code coverage
- Uploads test results and coverage reports
- Comments coverage on PRs

**Required Secrets:** None (uses default GITHUB_TOKEN)

### 2. Release (`release.yml`)

**Triggers:**
- When a tag matching `v*` is pushed (e.g., `v1.0.0`)

**What it does:**
- Builds release version
- Publishes the application
- Copies documentation and scripts
- Creates deployment package (ZIP)
- Creates GitHub release with notes from CHANGELOG.md
- Uploads the package as release asset

**How to use:**
```powershell
# Create and push a tag
git tag v1.0.0
git push origin v1.0.0
```

**Required Secrets:** None (uses default GITHUB_TOKEN)

### 3. Code Quality (`code-quality.yml`)

**Triggers:**
- Push to `main` or `develop` branches
- Pull requests to `main` or `develop` branches

**What it does:**
- Runs code analysis
- Checks code formatting
- Enforces code style rules

**Required Secrets:** None

### 4. Dependency Check (`dependency-check.yml`)

**Triggers:**
- Weekly on Monday at midnight (scheduled)
- Manual trigger (workflow_dispatch)

**What it does:**
- Lists outdated NuGet packages
- Creates an issue if updates are available

**Required Secrets:** None (uses default GITHUB_TOKEN)

## Issue Templates

Located in `.github/ISSUE_TEMPLATE/`

### 1. Bug Report (`bug_report.md`)

Use this template to report bugs. Includes:
- Bug description
- Steps to reproduce
- Expected vs actual behavior
- Environment details
- Configuration snippets
- Log output
- Severity classification

### 2. Feature Request (`feature_request.md`)

Use this template to suggest new features. Includes:
- Problem statement
- Proposed solution
- Use cases
- Alternative approaches
- Priority level
- Affected components

### 3. Question (`question.md`)

Use this template to ask questions. Includes:
- Clear question
- Context and goals
- What you've tried
- Environment (if relevant)
- Configuration (if relevant)

### 4. Documentation Issue (`documentation.md`)

Use this template to report documentation issues. Includes:
- Issue type (typo, incorrect info, etc.)
- Location (file, section, line)
- Current content
- Suggested fix
- Impact level

### 5. Config (`config.yml`)

Configures issue template chooser and adds links to:
- Documentation
- Discussions
- Security advisories

## Pull Request Template

Located at `.github/pull_request_template.md`

**Sections:**
- Description of changes
- Type of change (bug fix, feature, etc.)
- Related issues
- Changes made
- Testing performed
- Code quality checklist
- Documentation updates
- Breaking changes (if any)
- Screenshots (if applicable)
- Reviewer notes

**How to use:**
The template automatically appears when you create a PR.

## Community Files

### 1. CONTRIBUTING.md

Located at `.github/CONTRIBUTING.md`

**Contents:**
- Code of conduct reference
- How to contribute (bugs, features, docs, code)
- Development setup
- Pull request process
- Coding guidelines
- Commit message format
- Testing guidelines
- Documentation guidelines
- Review checklist

### 2. CODE_OF_CONDUCT.md

Located at root: `CODE_OF_CONDUCT.md`

Based on Contributor Covenant 2.0. Defines:
- Community standards
- Expected behavior
- Unacceptable behavior
- Enforcement responsibilities
- Reporting process
- Enforcement guidelines

### 3. SECURITY.md

Located at root: `SECURITY.md`

**Contents:**
- Supported versions
- How to report vulnerabilities
- Response timeline
- Security best practices
- Known security considerations
- Vulnerability disclosure policy
- Security update process

### 4. LICENSE

Located at root: `LICENSE`

MIT License - Free to use, modify, and distribute.

## Automation

### Dependabot

Located at `.github/dependabot.yml`

**What it does:**
- Automatically checks for dependency updates
- Creates PRs for outdated packages
- Runs weekly on Monday morning
- Checks both NuGet packages and GitHub Actions

**Configuration:**
- Maximum 5 open PRs at a time
- Ignores major version updates for stable dependencies
- Auto-assigns reviewers
- Adds appropriate labels
- Uses conventional commit messages

## Setting Up Your Repository

### Step 1: Update Placeholders

Replace these placeholders in the files:

1. **In `.github/ISSUE_TEMPLATE/config.yml`:**
   - Replace `YOUR_USERNAME` with your GitHub username

2. **In `.github/dependabot.yml`:**
   - Replace `YOUR_USERNAME` with your GitHub username

3. **In `SECURITY.md`:**
   - Replace `[security@yourdomain.com]` with your email

4. **In `LICENSE`:**
   - Replace `[Your Name or Organization]` with your name

5. **In `README.md` (root):**
   - Update repository URL
   - Update badges (if using shields.io)

### Step 2: Configure Repository Settings

1. **Enable GitHub Actions:**
   - Go to repository Settings ? Actions ? General
   - Allow all actions and reusable workflows

2. **Enable Dependabot:**
   - Already configured via `dependabot.yml`

3. **Branch Protection (Recommended):**
   - Settings ? Branches ? Add rule for `main`
   - Enable:
     - Require pull request reviews before merging
     - Require status checks to pass (select your workflows)
     - Require branches to be up to date

4. **Enable Discussions (Optional):**
   - Settings ? Features ? Discussions

5. **Configure Issue Templates:**
   - Templates are already in `.github/ISSUE_TEMPLATE/`

### Step 3: Add Repository Topics

Add relevant topics to help people find your project:
- `dotnet`
- `csharp`
- `windows-service`
- `task-scheduler`
- `powershell`
- `coravel`
- `serilog`
- `windows-server`

### Step 4: Configure Secrets (If Needed)

Currently, no secrets are required. The workflows use the default `GITHUB_TOKEN`.

Future secrets might include:
- `CODECOV_TOKEN` - For Codecov integration
- `NUGET_API_KEY` - If publishing to NuGet
- `DISCORD_WEBHOOK` - For Discord notifications

## Using GitHub Actions

### Running Workflows Manually

Some workflows support manual triggers:

```powershell
# Via GitHub UI
# 1. Go to Actions tab
# 2. Select workflow
# 3. Click "Run workflow"

# Via GitHub CLI
gh workflow run dependency-check.yml
```

### Viewing Workflow Results

1. Go to Actions tab
2. Click on a workflow run
3. View logs and artifacts

### Downloading Artifacts

Test results and coverage reports are uploaded as artifacts:

```powershell
# Via GitHub CLI
gh run download <run-id>
```

## Best Practices

### For Contributors

1. **Always use templates** when creating issues or PRs
2. **Follow the commit message format** specified in CONTRIBUTING.md
3. **Run tests locally** before pushing
4. **Keep PRs small and focused**
5. **Update documentation** with your changes

### For Maintainers

1. **Review PRs promptly**
2. **Use labels** to organize issues
3. **Close stale issues** with explanation
4. **Keep CHANGELOG.md updated**
5. **Create releases** for major versions
6. **Monitor GitHub Actions** for failures
7. **Review Dependabot PRs** regularly

## Troubleshooting GitHub Actions

### Workflow Fails on Windows-Latest

**Problem:** Workflow uses Windows-specific commands

**Solution:** All workflows are configured for `windows-latest`. Ensure PowerShell scripts use correct syntax.

### Coverage Report Not Generated

**Problem:** Coverage files not found

**Solution:** Ensure tests run with `--collect:"XPlat Code Coverage"`

### Release Not Created

**Problem:** Tag push doesn't trigger release

**Solution:** Ensure tag follows `v*` pattern (e.g., `v1.0.0`)

### Dependabot PRs Not Created

**Problem:** No PRs after a week

**Solution:**
1. Check `.github/dependabot.yml` syntax
2. Verify Dependabot is enabled in repository settings
3. Check if dependencies are actually outdated

## Monitoring and Maintenance

### Weekly Tasks

- [ ] Review new issues
- [ ] Review and merge Dependabot PRs
- [ ] Check GitHub Actions for failures
- [ ] Review open pull requests

### Monthly Tasks

- [ ] Review and close stale issues
- [ ] Update documentation if needed
- [ ] Check for security advisories
- [ ] Review and update labels

### Quarterly Tasks

- [ ] Review and update GitHub Actions versions
- [ ] Audit security policies
- [ ] Review contributor statistics
- [ ] Update roadmap/milestones

## Additional Resources

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [About Issue Templates](https://docs.github.com/en/communities/using-templates-to-encourage-useful-issues-and-pull-requests)
- [Dependabot Documentation](https://docs.github.com/en/code-security/dependabot)
- [Branch Protection Rules](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/defining-the-mergeability-of-pull-requests/about-protected-branches)

## Support

For issues with GitHub integration:
- Check workflow logs in Actions tab
- Review this guide
- Create an issue using the question template

---

**Last Updated:** 2024-01-15  
**Version:** 1.0.0
