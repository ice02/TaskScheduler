# Deployment Guide - Task Scheduler Service

## Prerequisites Checklist

Before deploying to Windows Server 2022, ensure:

- [ ] .NET 8.0 Runtime installed on target server
- [ ] Administrator access to the server
- [ ] PowerShell 5.1 or higher available
- [ ] Network access for SMTP (if using email notifications)
- [ ] Appropriate file permissions for script/executable locations

## Deployment Steps

### 1. Verify .NET Runtime

On the target server, check if .NET 8 is installed:

```powershell
dotnet --list-runtimes
```

If not installed, download and install from:
https://dotnet.microsoft.com/download/dotnet/8.0

You need **ASP.NET Core Runtime 8.0.x** (includes .NET Runtime)

### 2. Build and Publish Application

On your development machine:

```powershell
# Navigate to project directory
cd C:\Path\To\TaskScheduler

# Publish for Windows x64
dotnet publish -c Release -r win-x64 --self-contained false -o publish

# Or for self-contained (includes .NET runtime, larger size)
dotnet publish -c Release -r win-x64 --self-contained true -o publish
```

**Recommended**: Use `--self-contained false` if .NET is installed on server
**Alternative**: Use `--self-contained true` for servers without .NET installed

### 3. Package for Deployment

Create a deployment package:

```powershell
# Compress the publish folder
Compress-Archive -Path publish\* -DestinationPath TaskScheduler-Deploy.zip
```

### 4. Transfer to Server

Transfer `TaskScheduler-Deploy.zip` to the target server using:
- Remote Desktop copy/paste
- Network share
- SCP/SFTP
- USB drive

### 5. Extract on Server

```powershell
# On Windows Server 2022
$DestinationPath = "C:\Services\TaskScheduler"
New-Item -Path $DestinationPath -ItemType Directory -Force
Expand-Archive -Path TaskScheduler-Deploy.zip -DestinationPath $DestinationPath
```

### 6. Configure Application

Edit the configuration file:

```powershell
notepad C:\Services\TaskScheduler\appsettings.json
```

**Important Configuration Items:**

1. **Log Path**: Ensure the service has write permissions
   ```json
   "path": "C:\\Services\\TaskScheduler\\logs\\taskscheduler-.log"
   ```

2. **Job Paths**: Use absolute paths for scripts/executables
   ```json
   "Path": "C:\\Scripts\\MyScript.ps1"
   ```

3. **SMTP Settings**: Configure if using email notifications

### 7. Test in Console Mode

Before installing as service, test in console mode:

```powershell
cd C:\Services\TaskScheduler
.\TaskScheduler.exe
```

Watch for:
- Application starts without errors
- Jobs are loaded from configuration
- Logs are written to the logs folder

Press Ctrl+C to stop.

### 8. Install as Windows Service

Run the installation script as Administrator:

```powershell
cd C:\Services\TaskScheduler
.\Scripts\Install-Service.ps1
```

### 9. Verify Service Installation

```powershell
# Check service status
Get-Service TaskSchedulerService

# View service details
Get-Service TaskSchedulerService | Format-List *

# Check if service is set to start automatically
Get-WmiObject Win32_Service -Filter "Name='TaskSchedulerService'" | Select-Object StartMode
```

### 10. Monitor Initial Operation

Watch the logs for successful operation:

```powershell
Get-Content C:\Services\TaskScheduler\logs\taskscheduler-*.log -Tail 50 -Wait
```

Look for:
- "Task Scheduler Service starting..."
- "Scheduling job: [JobName]"
- "Job Scheduler Service started successfully"
- Job execution messages (based on cron schedule)

## Post-Deployment Configuration

### Configure Service Recovery

Set the service to restart automatically on failure:

```powershell
sc.exe failure TaskSchedulerService reset= 86400 actions= restart/60000/restart/60000/restart/60000
```

This sets:
- Reset failure count after 24 hours (86400 seconds)
- Restart service after 1 minute on first failure
- Restart service after 1 minute on second failure
- Restart service after 1 minute on subsequent failures

### Configure Service Account (Optional)

If jobs need specific permissions:

1. Create a service account with required permissions
2. Update service to run as that account:

```powershell
$ServiceName = "TaskSchedulerService"
$Username = "DOMAIN\ServiceAccount"
$Password = "SecurePassword"

$service = Get-WmiObject Win32_Service -Filter "Name='$ServiceName'"
$service.Change($null,$null,$null,$null,$null,$null,$Username,$Password)
```

Or use Services GUI (services.msc):
1. Right-click service ? Properties
2. Log On tab ? This account
3. Enter credentials

### Set Up Monitoring

#### Windows Event Log Monitoring

The service logs to Windows Application Event Log. Set up alerts for:
- Event Source: TaskSchedulerService
- Event Level: Error, Critical

#### Log File Monitoring

Set up a scheduled task to monitor log file size:

```powershell
# Example: Alert if logs directory exceeds 1GB
$LogPath = "C:\Services\TaskScheduler\logs"
$MaxSizeGB = 1
$CurrentSizeGB = (Get-ChildItem $LogPath -Recurse | Measure-Object -Property Length -Sum).Sum / 1GB

if ($CurrentSizeGB -gt $MaxSizeGB) {
    # Send alert
    Write-EventLog -LogName Application -Source "TaskScheduler" -EntryType Warning -EventId 1001 -Message "Log directory size exceeded $MaxSizeGB GB"
}
```

#### Health Check Script

Create a health check script:

```powershell
# HealthCheck.ps1
$ServiceName = "TaskSchedulerService"
$LogPath = "C:\Services\TaskScheduler\logs"

# Check service status
$service = Get-Service $ServiceName -ErrorAction SilentlyContinue
if ($service.Status -ne 'Running') {
    Write-Host "ERROR: Service is not running!" -ForegroundColor Red
    Start-Service $ServiceName
    exit 1
}

# Check if logs are being written (updated in last hour)
$latestLog = Get-ChildItem $LogPath -Filter "*.log" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if ($latestLog) {
    $hoursSinceUpdate = (New-TimeSpan -Start $latestLog.LastWriteTime -End (Get-Date)).TotalHours
    if ($hoursSinceUpdate -gt 1) {
        Write-Host "WARNING: No log activity in the last hour" -ForegroundColor Yellow
    }
    else {
        Write-Host "OK: Service is healthy" -ForegroundColor Green
    }
}

exit 0
```

Schedule this to run every 15 minutes using Task Scheduler.

## Firewall Configuration

If using SMTP email notifications, ensure outbound SMTP port is allowed:

```powershell
# Allow outbound SMTP (port 587 for TLS)
New-NetFirewallRule -DisplayName "TaskScheduler SMTP" -Direction Outbound -Protocol TCP -RemotePort 587 -Action Allow

# Or for port 465 (SSL)
New-NetFirewallRule -DisplayName "TaskScheduler SMTP SSL" -Direction Outbound -Protocol TCP -RemotePort 465 -Action Allow
```

## Security Hardening

### File System Permissions

Restrict access to application directory:

```powershell
$AppPath = "C:\Services\TaskScheduler"

# Remove inherited permissions
$acl = Get-Acl $AppPath
$acl.SetAccessRuleProtection($true, $false)

# Add necessary permissions
$adminRule = New-Object System.Security.AccessControl.FileSystemAccessRule("BUILTIN\Administrators", "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow")
$systemRule = New-Object System.Security.AccessControl.FileSystemAccessRule("NT AUTHORITY\SYSTEM", "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow")

$acl.SetAccessRule($adminRule)
$acl.SetAccessRule($systemRule)
Set-Acl $AppPath $acl
```

### Encrypt Sensitive Configuration

Consider using Windows Data Protection API (DPAPI) for passwords:

```powershell
# Encrypt password
$SecurePassword = ConvertTo-SecureString "YourPassword" -AsPlainText -Force
$EncryptedPassword = ConvertFrom-SecureString $SecurePassword

# Store encrypted password in config or separate file
```

Or use Azure Key Vault, Windows Credential Manager, or other secret management solutions.

## Backup and Recovery

### Backup Configuration

Create a backup script:

```powershell
# Backup-TaskSchedulerConfig.ps1
$SourcePath = "C:\Services\TaskScheduler\appsettings.json"
$BackupPath = "C:\Backups\TaskScheduler"
$BackupFile = Join-Path $BackupPath "appsettings_$(Get-Date -Format 'yyyyMMdd_HHmmss').json"

New-Item -Path $BackupPath -ItemType Directory -Force -ErrorAction SilentlyContinue
Copy-Item $SourcePath $BackupFile

# Keep only last 30 backups
Get-ChildItem $BackupPath -Filter "appsettings_*.json" | 
    Sort-Object CreationTime -Descending | 
    Select-Object -Skip 30 | 
    Remove-Item
```

Schedule this before making configuration changes.

### Disaster Recovery

Document your disaster recovery procedure:

1. **Service Failure**: Service restarts automatically (configured in Step 10)
2. **Configuration Corruption**: Restore from backup
3. **System Failure**: Reinstall from deployment package, restore configuration

## Updating the Service

To update the service with a new version:

```powershell
# 1. Stop the service
Stop-Service TaskSchedulerService

# 2. Backup current version
Copy-Item C:\Services\TaskScheduler C:\Services\TaskScheduler.backup -Recurse

# 3. Extract new version (overwrite)
Expand-Archive -Path TaskScheduler-Deploy-New.zip -DestinationPath C:\Services\TaskScheduler -Force

# 4. Preserve existing appsettings.json
Copy-Item C:\Services\TaskScheduler.backup\appsettings.json C:\Services\TaskScheduler\appsettings.json

# 5. Start the service
Start-Service TaskSchedulerService

# 6. Verify
Get-Service TaskSchedulerService
Get-Content C:\Services\TaskScheduler\logs\taskscheduler-*.log -Tail 20
```

## Rollback Procedure

If the update fails:

```powershell
# 1. Stop the service
Stop-Service TaskSchedulerService

# 2. Restore backup
Remove-Item C:\Services\TaskScheduler -Recurse -Force
Copy-Item C:\Services\TaskScheduler.backup C:\Services\TaskScheduler -Recurse

# 3. Start the service
Start-Service TaskSchedulerService
```

## Multi-Server Deployment

For deploying to multiple servers:

### 1. Create a Deployment Script

```powershell
# Deploy-ToServers.ps1
$Servers = @("SERVER01", "SERVER02", "SERVER03")
$SourcePackage = "\\FileServer\Share\TaskScheduler-Deploy.zip"
$DestPath = "C:\Services\TaskScheduler"

foreach ($Server in $Servers) {
    Write-Host "Deploying to $Server..." -ForegroundColor Cyan
    
    # Copy package
    Copy-Item $SourcePackage "\\$Server\C$\Temp\" -Force
    
    # Install remotely
    Invoke-Command -ComputerName $Server -ScriptBlock {
        param($DestPath, $PackagePath)
        
        # Extract
        New-Item -Path $DestPath -ItemType Directory -Force
        Expand-Archive -Path $PackagePath -DestinationPath $DestPath -Force
        
        # Install service
        & "$DestPath\Scripts\Install-Service.ps1"
        
    } -ArgumentList $DestPath, "C:\Temp\TaskScheduler-Deploy.zip"
    
    Write-Host "Deployment to $Server completed" -ForegroundColor Green
}
```

### 2. Centralized Configuration Management

Consider storing configurations in a central location:
- Network share
- Configuration management tool (Ansible, Chef, Puppet)
- Version control system (Git)

## Troubleshooting Deployment Issues

### Service Won't Start

**Check Event Viewer:**
```powershell
Get-EventLog -LogName Application -Source TaskSchedulerService -Newest 10
```

**Common issues:**
- .NET Runtime not installed or wrong version
- Missing dependencies
- Invalid appsettings.json
- File permission issues

### Jobs Not Executing

**Verify service account permissions:**
- Read access to script/executable files
- Execute permissions
- Access to any resources the jobs use

**Test manually:**
```powershell
# Run as service account
runas /user:DOMAIN\ServiceAccount cmd

# Then test script
powershell -ExecutionPolicy Bypass -File C:\Scripts\MyScript.ps1
```

### Email Notifications Not Working

**Test SMTP connectivity:**
```powershell
Test-NetConnection -ComputerName smtp.gmail.com -Port 587
```

**Test from PowerShell:**
```powershell
Send-MailMessage -SmtpServer smtp.gmail.com -Port 587 -UseSsl `
    -Credential (Get-Credential) `
    -From "test@example.com" -To "admin@example.com" `
    -Subject "Test" -Body "Test message"
```

## Performance Tuning

### Optimize Log File Size

Adjust retention in appsettings.json:

```json
"retainedFileCountLimit": 30,  // Keep 30 days
"rollingInterval": "Day"        // Or "Hour" for high-volume
```

### Monitor Resource Usage

```powershell
# Check service memory usage
Get-Process | Where-Object { $_.ProcessName -eq "TaskScheduler" } | Format-Table ProcessName, WS, CPU

# Monitor over time
while ($true) {
    $proc = Get-Process -Name TaskScheduler -ErrorAction SilentlyContinue
    if ($proc) {
        Write-Host "$(Get-Date) - Memory: $([math]::Round($proc.WS/1MB,2)) MB, CPU: $($proc.CPU)"
    }
    Start-Sleep -Seconds 60
}
```

## Compliance and Auditing

### Enable Detailed Logging

For compliance requirements, set log level to Debug:

```json
"MinimumLevel": {
  "Default": "Debug"
}
```

### Log Retention Policy

Set appropriate retention based on compliance requirements:
- Financial: 7 years
- Healthcare: 6 years
- General: 1 year minimum

### Audit Trail

All job executions are logged with:
- Timestamp
- Job name
- Execution result
- Error details (if any)

## Support Contacts

Document your support contacts:
- **Application Owner**: [Name/Email]
- **Server Admin**: [Name/Email]
- **On-Call Contact**: [Phone/Email]

## Deployment Checklist

Before declaring deployment complete:

- [ ] Service installed and running
- [ ] All jobs configured correctly
- [ ] Jobs executing on schedule
- [ ] Logs being written successfully
- [ ] Email notifications tested (if enabled)
- [ ] Service recovery configured
- [ ] Monitoring/alerts set up
- [ ] Backup procedure documented
- [ ] Documentation updated
- [ ] Team trained on operations

## Next Steps After Deployment

1. Monitor service for 24-48 hours
2. Review logs for any errors or warnings
3. Verify all scheduled jobs execute successfully
4. Test failure scenarios (job timeout, email notification)
5. Document any environment-specific configurations
6. Schedule regular maintenance windows
7. Plan for future updates and enhancements
