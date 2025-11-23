# ?? Task Scheduler Service for Windows

A production-ready .NET 8 Windows Service for executing scheduled PowerShell scripts and executables on Windows Server 2022.

[![.NET Version](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Platform](https://img.shields.io/badge/platform-Windows-lightgrey.svg)](https://www.microsoft.com/windows-server)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)

## ? Features

- ?? **Flexible Scheduling**: Cron-based job scheduling with Coravel
- ?? **PowerShell Support**: Execute PowerShell scripts with parameters
- ?? **Executable Support**: Run any Windows executable or batch file
- ??? **Overlap Prevention**: Automatic detection and prevention of concurrent job executions
- ?? **Timeout Protection**: Automatic termination of long-running jobs
- ?? **Structured Logging**: Comprehensive logging with Serilog (file and console)
- ?? **Email Notifications**: Optional SMTP alerts for job failures
- ?? **Windows Service**: Runs as a background service with automatic recovery
- ??? **Console Mode**: Also runs as console application for testing

## ?? Quick Start

### Prerequisites
- Windows Server 2022 or Windows 10/11
- .NET 8.0 Runtime
- Administrator privileges (for service installation)

### Installation

```powershell
# 1. Download or build the application
dotnet publish -c Release -o C:\TaskScheduler

# 2. Navigate to application directory
cd C:\TaskScheduler

# 3. Install as Windows Service (run as Administrator)
.\Scripts\Install-Service.ps1

# 4. Verify service is running
Get-Service TaskSchedulerService
```

### Basic Configuration

Edit `appsettings.json`:

```json
{
  "Jobs": [
    {
      "Name": "MyFirstJob",
      "Type": "PowerShell",
      "Path": "C:\\Scripts\\MyScript.ps1",
      "Arguments": "",
      "CronExpression": "*/5 * * * *",
      "MaxExecutionTimeMinutes": 10,
      "Enabled": true
    }
  ]
}
```

**That's it!** The service will now execute your job every 5 minutes.

## ?? Documentation

| Document | Description |
|----------|-------------|
| [?? README](TaskScheduler/README.md) | Complete documentation |
| [?? Quick Start](TaskScheduler/QUICKSTART.md) | Get started in 10 minutes |
| [?? Deployment Guide](TaskScheduler/DEPLOYMENT.md) | Production deployment instructions |
| [?? Examples](TaskScheduler/EXAMPLES.md) | Common job configuration patterns |
| [??? Architecture](TaskScheduler/ARCHITECTURE.md) | Technical architecture details |
| [?? Changelog](TaskScheduler/CHANGELOG.md) | Version history |
| [?? Contributing](TaskScheduler/CONTRIBUTING.md) | How to contribute |

## ?? Use Cases

### Database Operations
- Automated database backups
- Log table cleanup
- Index rebuilds
- Data archival

### File Management
- File synchronization
- Backup rotation
- Temporary file cleanup
- Archive compression

### Data Processing
- ETL operations
- Report generation
- Data imports/exports
- Batch processing

### System Monitoring
- Disk space monitoring
- Service health checks
- Performance metrics
- Alert generation

## ?? What's Included

```
TaskScheduler/
??? ?? Source Code
?   ??? Program.cs (Entry point)
?   ??? Models/ (Configuration models)
?   ??? Services/ (Business logic)
?   ??? Jobs/ (Job wrappers)
?
??? ?? Scripts
?   ??? Install-Service.ps1 (Installation)
?   ??? Uninstall-Service.ps1 (Removal)
?   ??? Test-Service.ps1 (Diagnostics)
?   ??? Example-Script.ps1 (Template)
?
??? ?? Documentation
    ??? README.md (Complete docs)
    ??? QUICKSTART.md (Quick start)
    ??? DEPLOYMENT.md (Deployment)
    ??? EXAMPLES.md (Examples)
    ??? ARCHITECTURE.md (Architecture)
```

## ??? Technologies

- **Framework**: .NET 8.0
- **Scheduling**: Coravel 5.0.3
- **Logging**: Serilog 3.1.1
- **Email**: MailKit 4.3.0
- **Hosting**: Microsoft.Extensions.Hosting

## ?? Common Cron Schedules

| Expression | Description |
|------------|-------------|
| `*/5 * * * *` | Every 5 minutes |
| `0 * * * *` | Every hour |
| `0 2 * * *` | Daily at 2 AM |
| `0 9 * * 1-5` | Weekdays at 9 AM |
| `0 0 1 * *` | Monthly on 1st |

## ?? Management Commands

```powershell
# Check service status
Get-Service TaskSchedulerService

# View logs
Get-Content C:\TaskScheduler\logs\taskscheduler-*.log -Tail 50 -Wait

# Test configuration
.\Scripts\Test-Service.ps1

# Restart service
Restart-Service TaskSchedulerService
```

## ?? System Requirements

- **OS**: Windows Server 2022, Windows 10, or Windows 11
- **.NET**: .NET 8.0 Runtime or SDK
- **PowerShell**: 5.1 or higher
- **Disk Space**: 50 MB + space for logs
- **Memory**: 100 MB minimum
- **Permissions**: Administrator (for service installation)

## ?? Security

- Service account isolation
- File system permissions
- Encrypted SMTP credentials support
- Audit logging
- No external dependencies required

## ?? Key Benefits

? **Reliable**: Automatic restart on failure, overlap prevention  
? **Observable**: Comprehensive logging, email notifications  
? **Flexible**: Supports PowerShell and executables  
? **Simple**: JSON configuration, easy deployment  
? **Production-Ready**: Windows Service with proper lifecycle management  

## ?? Performance

- **Startup Time**: < 2 seconds
- **Memory Footprint**: ~50-100 MB
- **CPU Usage**: Minimal (event-driven)
- **Concurrent Jobs**: Supports dozens simultaneously
- **Log Rotation**: Automatic daily rotation

## ?? Testing

```powershell
# Run in console mode for testing
cd C:\TaskScheduler
.\TaskScheduler.exe

# Use diagnostic utility
.\Scripts\Test-Service.ps1
```

## ?? Support

- **Documentation**: See [README.md](TaskScheduler/README.md)
- **Examples**: See [EXAMPLES.md](TaskScheduler/EXAMPLES.md)
- **Issues**: File a bug report
- **Questions**: Open a discussion

## ?? Contributing

Contributions are welcome! Please see [CONTRIBUTING.md](TaskScheduler/CONTRIBUTING.md) for guidelines.

## ?? License

This project is licensed under the MIT License - see the LICENSE file for details.

## ????? Authors

[Your Name/Organization]

## ?? Acknowledgments

- **Coravel** - Excellent scheduling framework
- **Serilog** - Structured logging
- **MailKit** - Email functionality
- Microsoft .NET Team

## ?? Contact

- **Email**: [your-email@example.com]
- **GitHub**: [https://github.com/your-org/task-scheduler]
- **Website**: [https://your-website.com]

---

## ?? Get Started Now!

```powershell
# Clone the repository
git clone https://github.com/your-org/task-scheduler-service.git
cd task-scheduler-service

# Build and publish
dotnet publish -c Release -o C:\TaskScheduler

# Install service
cd C:\TaskScheduler
.\Scripts\Install-Service.ps1

# You're ready to go! ??
```

---

**Made with ?? for Windows Server Administrators**
