# Changelog

All notable changes to the Task Scheduler Service project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2024-01-15

### Added
- Initial release of Task Scheduler Service
- PowerShell script execution support
- Executable (.exe) execution support
- Cron-based job scheduling using Coravel
- Job overlap prevention mechanism
- Automatic job timeout and termination
- Structured logging with Serilog
  - File logging with daily rotation
  - Console logging
  - Configurable log levels
- Email notification system using MailKit
  - SMTP support with TLS/SSL
  - Notifications for job failures
  - Notifications for job overlaps
  - Notifications for job timeouts
- Dual execution mode
  - Console application mode for testing
  - Windows Service mode for production
- Windows Service integration
  - Automatic restart on failure
  - Proper lifecycle management
- Configuration via appsettings.json
  - Hot reload support
  - Job configuration
  - SMTP settings
  - Logging settings
- Management scripts
  - Install-Service.ps1 for service installation
  - Uninstall-Service.ps1 for service removal
  - Test-Service.ps1 for diagnostics and monitoring
  - Example-Script.ps1 as job template
- Comprehensive documentation
  - README.md with full documentation
  - QUICKSTART.md for rapid deployment
  - DEPLOYMENT.md for production deployment
  - EXAMPLES.md with common job patterns
  - PROJECT_SUMMARY.md for overview
- .NET 8.0 support
- Windows Server 2022 compatibility

### Technical Details
- Target Framework: .NET 8.0
- C# Version: 12.0
- Dependencies:
  - Coravel 5.0.3
  - Serilog 3.1.1
  - MailKit 4.3.0
  - Microsoft.Extensions.Hosting 8.0.0
  - Microsoft.Extensions.Hosting.WindowsServices 8.0.0

### Security
- Service account isolation support
- Secure password handling in configuration
- TLS/SSL support for SMTP
- File system permission recommendations

### Known Limitations
- Local machine execution only (no remote scheduling)
- Manual configuration file editing (no GUI)
- Windows-only platform support

## [Unreleased]

### Planned Features
- Web-based configuration interface
- Job execution history database
- Real-time dashboard
- Advanced scheduling options (holidays, blackout periods)
- Job dependencies and chaining
- Retry logic with exponential backoff
- Prometheus metrics export
- Docker container support
- Cross-platform support (Linux, macOS)
- REST API for job management
- Job templates library
- Multi-tenancy support
- Role-based access control

---

## Version History Summary

- **1.0.0** (2024-01-15): Initial release with core functionality
