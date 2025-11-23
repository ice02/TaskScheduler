# Quick Start Guide - Task Scheduler Service

## Installation (5 minutes)

### 1. Build and Publish

```powershell
cd TaskScheduler
dotnet publish -c Release -o C:\TaskScheduler
```

### 2. Install Service

```powershell
cd C:\TaskScheduler
.\Scripts\Install-Service.ps1
```

## Configuration (5 minutes)

### 1. Edit Configuration File

Open `C:\TaskScheduler\appsettings.json`

### 2. Configure Your First Job

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

### 3. Restart Service

```powershell
Restart-Service TaskSchedulerService
```

## Verify (2 minutes)

### 1. Check Service Status

```powershell
Get-Service TaskSchedulerService
```

Expected output: `Status: Running`

### 2. View Logs

```powershell
cd C:\TaskScheduler
Get-Content logs\taskscheduler-*.log -Tail 20 -Wait
```

### 3. Wait for Job Execution

If you set `CronExpression: "*/5 * * * *"`, the job will run every 5 minutes.

## Common Cron Expressions

| Schedule | Cron Expression |
|----------|----------------|
| Every 5 minutes | `*/5 * * * *` |
| Every hour | `0 * * * *` |
| Daily at 2 AM | `0 2 * * *` |
| Weekdays at 8 AM | `0 8 * * 1-5` |
| Every Monday at 9 AM | `0 9 * * 1` |

## Test Mode (Run as Console)

For testing, run without installing as service:

```powershell
cd C:\TaskScheduler
.\TaskScheduler.exe
```

Press Ctrl+C to stop.

## Enable Email Notifications (Optional)

Edit `appsettings.json`:

```json
{
  "SmtpSettings": {
    "Enabled": true,
    "Host": "smtp.gmail.com",
    "Port": 587,
    "UseSsl": true,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "FromEmail": "taskscheduler@yourdomain.com",
    "FromName": "Task Scheduler",
    "ToEmails": ["admin@yourdomain.com"]
  }
}
```

## Troubleshooting

### Service Won't Start

```powershell
# Check logs
Get-Content C:\TaskScheduler\logs\taskscheduler-*.log -Tail 50
```

### Job Not Running

1. Verify job is enabled: `"Enabled": true`
2. Check file path exists: `Test-Path "C:\Scripts\MyScript.ps1"`
3. Verify cron expression using online validator

### View Service Events

```powershell
Get-EventLog -LogName Application -Source TaskSchedulerService -Newest 10
```

## Uninstall

```powershell
cd C:\TaskScheduler
.\Scripts\Uninstall-Service.ps1
```

## Next Steps

- Read full documentation in README.md
- Add more jobs to appsettings.json
- Configure email notifications
- Set up log monitoring
- Test job overlap prevention

## Support

For issues, check:
1. Application logs: `C:\TaskScheduler\logs\`
2. Windows Event Viewer: Application logs
3. Service status: `Get-Service TaskSchedulerService`

## Example PowerShell Script

Create `C:\Scripts\test.ps1`:

```powershell
Write-Output "Job started at $(Get-Date)"
Write-Output "Hello from Task Scheduler!"
Start-Sleep -Seconds 2
Write-Output "Job completed successfully"
exit 0
```

Add to appsettings.json:

```json
{
  "Name": "TestJob",
  "Type": "PowerShell",
  "Path": "C:\\Scripts\\test.ps1",
  "Arguments": "",
  "CronExpression": "*/5 * * * *",
  "MaxExecutionTimeMinutes": 5,
  "Enabled": true
}
```

Restart service and check logs!
