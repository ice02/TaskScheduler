# Task Scheduler Service for Windows Server 2022

A .NET 8 Windows Service that executes scheduled PowerShell scripts and executables based on configuration files. Supports automatic restart on failure, comprehensive logging with Serilog, and email notifications for errors.

## Table of Contents

- [Features](#features)
- [Requirements](#requirements)
- [Installation](#installation)
- [Configuration](#configuration)
- [Usage](#usage)
- [Job Configuration](#job-configuration)
- [Logging](#logging)
- [Email Notifications](#email-notifications)
- [Troubleshooting](#troubleshooting)
- [Uninstallation](#uninstallation)

## Features

- **Dual Execution Mode**: Runs as a Windows Service or Console application
- **Flexible Job Scheduling**: Execute PowerShell scripts or executables on cron-based schedules
- **Job Protection**: Prevents overlapping executions of the same job
- **Automatic Timeout**: Terminates jobs that exceed maximum execution time
- **Comprehensive Logging**: Structured logging with Serilog to file and console
- **Email Notifications**: Optional SMTP notifications for job failures and errors
- **Hot Configuration Reload**: Configuration changes are picked up automatically
- **Windows Service Integration**: Automatic restart on failure, proper lifecycle management

## Requirements

- **Operating System**: Windows Server 2022 (or Windows 10/11)
- **.NET Runtime**: .NET 8.0 or higher
- **Permissions**: Administrator rights for service installation
- **PowerShell**: Version 5.1 or higher (for PowerShell job execution)

## Installation

### Step 1: Build the Application

```powershell
cd TaskScheduler
dotnet build -c Release
```

### Step 2: Publish the Application

```powershell
dotnet publish -c Release -o publish
```

### Step 3: Install as Windows Service

Navigate to the publish directory and run the installation script as Administrator:

```powershell
cd publish
.\Scripts\Install-Service.ps1
```

The script will:
- Verify administrator privileges
- Check for existing service installation
- Create the Windows Service
- Configure automatic restart on failure
- Start the service

### Verify Installation

Check the service status:

```powershell
Get-Service TaskSchedulerService
```

Or use the Windows Services Manager (services.msc).

## Configuration

The service is configured through `appsettings.json` located in the application directory.

### Basic Configuration Structure

```json
{
  "Serilog": { /* Logging configuration */ },
  "SmtpSettings": { /* Email notification settings */ },
  "Jobs": [ /* Job definitions */ ]
}
```

### Serilog Configuration

Configure logging behavior:

```json
"Serilog": {
  "MinimumLevel": {
    "Default": "Information",
    "Override": {
      "Microsoft": "Warning",
      "System": "Warning"
    }
  },
  "WriteTo": [
    {
      "Name": "File",
      "Args": {
        "path": "logs/taskscheduler-.log",
        "rollingInterval": "Day",
        "retainedFileCountLimit": 30
      }
    }
  ]
}
```

**Log Levels**: Debug, Information, Warning, Error, Fatal

### SMTP Settings

Configure email notifications (optional):

```json
"SmtpSettings": {
  "Enabled": true,
  "Host": "smtp.gmail.com",
  "Port": 587,
  "UseSsl": true,
  "Username": "your-email@gmail.com",
  "Password": "your-app-password",
  "FromEmail": "taskscheduler@yourdomain.com",
  "FromName": "Task Scheduler Service",
  "ToEmails": [
    "admin@yourdomain.com",
    "alerts@yourdomain.com"
  ]
}
```

**Note**: Set `"Enabled": false` to disable email notifications.

## Job Configuration

Jobs are defined in the `"Jobs"` array in `appsettings.json`.

### Job Properties

| Property | Type | Description |
|----------|------|-------------|
| `Name` | string | Unique identifier for the job |
| `Type` | string | "PowerShell" or "Executable" |
| `Path` | string | Full path to the script or executable |
| `Arguments` | string | Command-line arguments |
| `CronExpression` | string | Cron schedule expression |
| `MaxExecutionTimeMinutes` | int | Maximum execution time before timeout |
| `Enabled` | bool | Whether the job is active |

### PowerShell Job Example

```json
{
  "Name": "DatabaseBackup",
  "Type": "PowerShell",
  "Path": "C:\\Scripts\\Backup-Database.ps1",
  "Arguments": "-Server localhost -Database MyDB",
  "CronExpression": "0 2 * * *",
  "MaxExecutionTimeMinutes": 60,
  "Enabled": true
}
```

### Executable Job Example

```json
{
  "Name": "DataExport",
  "Type": "Executable",
  "Path": "C:\\Tools\\DataExporter.exe",
  "Arguments": "--output C:\\Exports --format CSV",
  "CronExpression": "0 */4 * * *",
  "MaxExecutionTimeMinutes": 30,
  "Enabled": true
}
```

### Cron Expression Format

The service uses standard cron expressions with 5 fields:

```
* * * * *
? ? ? ? ?
? ? ? ? ???? Day of week (0-6, Sunday=0)
? ? ? ?????? Month (1-12)
? ? ???????? Day of month (1-31)
? ?????????? Hour (0-23)
???????????? Minute (0-59)
```

#### Common Examples

| Expression | Description |
|------------|-------------|
| `* * * * *` | Every minute |
| `*/5 * * * *` | Every 5 minutes |
| `0 * * * *` | Every hour |
| `0 */4 * * *` | Every 4 hours |
| `0 2 * * *` | Daily at 2:00 AM |
| `0 2 * * 1` | Every Monday at 2:00 AM |
| `0 0 1 * *` | First day of month at midnight |
| `30 14 * * 1-5` | Weekdays at 2:30 PM |

## Usage

### Running as Console Application

For testing or debugging, run the application in console mode:

```powershell
.\TaskScheduler.exe
```

Press Ctrl+C to stop.

### Running as Windows Service

The service starts automatically after installation. Manage it using:

**PowerShell Commands:**

```powershell
# Check status
Get-Service TaskSchedulerService

# Start service
Start-Service TaskSchedulerService

# Stop service
Stop-Service TaskSchedulerService

# Restart service
Restart-Service TaskSchedulerService
```

**Services Manager:**

1. Press Win+R, type `services.msc`
2. Find "Task Scheduler Service"
3. Right-click to Start, Stop, or Restart

### Viewing Logs

Logs are written to the `logs` directory:

```powershell
# View latest log
Get-Content logs\taskscheduler-*.log -Tail 50 -Wait

# View all logs
Get-ChildItem logs\*.log | ForEach-Object { Get-Content $_.FullName }
```

## Logging

The service provides comprehensive logging:

### Log Levels

- **Information**: Job start/completion, service lifecycle
- **Warning**: Job overlap detection, minor issues
- **Error**: Job failures, timeout events
- **Debug**: Process output, detailed execution flow

### Log Format

```
2024-01-15 14:30:00.123 +00:00 [INF] Starting job execution: DatabaseBackup
2024-01-15 14:30:45.456 +00:00 [INF] Job DatabaseBackup completed successfully
```

### Log Rotation

Logs rotate daily and are retained for 30 days by default. Configure in `appsettings.json`:

```json
"retainedFileCountLimit": 30
```

## Email Notifications

When SMTP is configured and enabled, the service sends email notifications for:

### Job Overlap

Sent when a job is scheduled to run but is already executing:

```
Subject: Task Scheduler - Job Overlap: [JobName]
Body: Job [JobName] is already running. Skipping this execution.
```

### Job Timeout

Sent when a job exceeds its maximum execution time:

```
Subject: Task Scheduler - Job Timeout: [JobName]
Body: Job [JobName] exceeded maximum execution time of X minutes and was terminated.
```

### Job Failure

Sent when a job fails with an error:

```
Subject: Task Scheduler - Job Failed: [JobName]
Body: Job [JobName] failed with error: [Error Message]
      Stack Trace: [Full Stack Trace]
```

## Troubleshooting

### Service Won't Start

**Check logs:**
```powershell
Get-Content logs\taskscheduler-*.log -Tail 100
```

**Common causes:**
- Invalid JSON in appsettings.json
- Missing file permissions
- .NET Runtime not installed

**Verify .NET installation:**
```powershell
dotnet --list-runtimes
```

### Job Not Executing

**Verify job is enabled:**
```json
"Enabled": true
```

**Check cron expression:**
Use an online cron validator to test your expression.

**Verify file paths:**
Ensure PowerShell script or executable exists:
```powershell
Test-Path "C:\Scripts\YourScript.ps1"
```

**Check permissions:**
Service runs under Local System by default. Ensure it has access to:
- Script/executable files
- Any resources the job accesses (databases, network shares, etc.)

### PowerShell Execution Policy

If PowerShell scripts fail, check execution policy:

```powershell
Get-ExecutionPolicy
```

The service uses `-ExecutionPolicy Bypass` by default, but ensure scripts are not blocked:

```powershell
Unblock-File "C:\Scripts\YourScript.ps1"
```

### Email Notifications Not Working

**Test SMTP settings:**
```powershell
# Test SMTP connectivity
Test-NetConnection -ComputerName smtp.gmail.com -Port 587
```

**For Gmail:**
- Use App Password, not your regular password
- Enable "Less secure app access" or use OAuth2

**Check logs:**
Email errors are logged with details about the failure.

### Job Overlapping

This is expected behavior. The service prevents overlapping executions to protect system resources. If you see overlap warnings:

1. Increase `MaxExecutionTimeMinutes`
2. Reduce execution frequency (adjust cron expression)
3. Optimize the job's performance

### Service Crashes

The service is configured to restart automatically on failure. Check:

**Event Viewer:**
1. Win+R ? `eventvwr.msc`
2. Windows Logs ? Application
3. Look for errors from "TaskSchedulerService"

**Service Recovery Settings:**
```powershell
sc.exe qfailure TaskSchedulerService
```

## Uninstallation

### Step 1: Run Uninstall Script

Navigate to the application directory and run as Administrator:

```powershell
cd [application-directory]
.\Scripts\Uninstall-Service.ps1
```

The script will:
- Stop the service if running
- Remove the Windows Service registration
- Preserve logs and configuration files

### Step 2: Clean Up (Optional)

Manually delete the application directory if you want to remove everything:

```powershell
Remove-Item -Path "C:\Path\To\TaskScheduler" -Recurse -Force
```

## Advanced Configuration

### Changing Service Account

By default, the service runs as Local System. To change:

1. Open Services (services.msc)
2. Right-click "Task Scheduler Service" ? Properties
3. Go to "Log On" tab
4. Select "This account" and enter credentials
5. Restart the service

### Multiple Instances

To run multiple instances with different configurations:

1. Copy the application to different directories
2. Modify the service name in Install-Service.ps1
3. Update appsettings.json in each instance
4. Install each instance as a separate service

### Security Considerations

- **Passwords in Configuration**: Consider using Windows Credential Manager or Azure Key Vault
- **File Permissions**: Restrict access to appsettings.json
- **Service Account**: Use least-privilege account for service execution
- **Script Signing**: Sign PowerShell scripts in production environments

## Best Practices

1. **Test Jobs First**: Run jobs manually before scheduling
2. **Set Realistic Timeouts**: Allow enough time but prevent hanging
3. **Monitor Logs**: Regularly review logs for issues
4. **Use Descriptive Names**: Clear job names help with troubleshooting
5. **Version Control**: Keep appsettings.json in version control
6. **Backup Configuration**: Before making changes, backup appsettings.json
7. **Gradual Rollout**: Enable jobs one at a time in production

## Support and Maintenance

### Regular Maintenance Tasks

- Review logs weekly
- Monitor disk space for log files
- Test email notifications monthly
- Update .NET Runtime as needed
- Review and optimize job schedules

### Performance Monitoring

Monitor these metrics:
- Job execution duration
- Job overlap frequency
- Log file size growth
- Service memory usage

Use Windows Performance Monitor or PowerShell:

```powershell
# Check service process
Get-Process | Where-Object { $_.ProcessName -eq "TaskScheduler" }
```

## Version History

**Version 1.0.0**
- Initial release
- PowerShell and Executable job support
- Cron-based scheduling with Coravel
- Serilog logging
- Email notifications
- Windows Service integration
- Overlap prevention
- Automatic timeout handling

## License

[Your License Here]

## Authors

[Your Name/Team]

## Acknowledgments

- **Coravel**: Scheduling framework
- **Serilog**: Logging library
- **MailKit**: Email functionality
