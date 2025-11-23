# Scripts Directory

This directory contains utility scripts for managing the Task Scheduler Service.

## Management Scripts

### Install-Service.ps1
Installs the Task Scheduler as a Windows Service.

**Usage:**
```powershell
.\Install-Service.ps1
```

**Requirements:**
- Must be run as Administrator
- TaskScheduler.exe must be in the same directory

**What it does:**
- Checks for administrator privileges
- Verifies executable exists
- Removes existing service if present (with confirmation)
- Creates new Windows Service
- Configures automatic restart on failure
- Starts the service

---

### Uninstall-Service.ps1
Removes the Task Scheduler Windows Service.

**Usage:**
```powershell
.\Uninstall-Service.ps1
```

**Requirements:**
- Must be run as Administrator

**What it does:**
- Checks for administrator privileges
- Verifies service exists
- Prompts for confirmation
- Stops the service if running
- Removes the service registration
- Preserves logs and configuration

---

### Test-Service.ps1
Interactive utility for testing and monitoring the service.

**Usage:**
```powershell
# Interactive menu
.\Test-Service.ps1

# Check status only
.\Test-Service.ps1 -CheckStatus

# View recent logs
.\Test-Service.ps1 -ViewLogs
```

**Features:**
1. Check service status and process information
2. View recent log entries
3. Watch logs in real-time
4. Test configuration file validity
5. List configured jobs
6. Restart the service
7. Search for errors in logs
8. View Windows Event Log entries

**No administrator privileges required** (except for restart function)

---

### Example-Script.ps1
A sample PowerShell script demonstrating proper job implementation.

**Features:**
- Parameter handling
- Output logging
- Proper exit codes
- Execution information

**Use this as a template** for creating your own job scripts.

---

## Creating Your Own Job Scripts

### PowerShell Script Template

```powershell
# MyJobScript.ps1
param(
    [string]$Param1 = "DefaultValue",
    [int]$Param2 = 0
)

$ErrorActionPreference = "Stop"

try {
    Write-Output "Job started at $(Get-Date)"
    Write-Output "Parameter 1: $Param1"
    Write-Output "Parameter 2: $Param2"
    
    # Your job logic here
    # ...
    
    Write-Output "Job completed successfully"
    exit 0  # Success
}
catch {
    Write-Error "Job failed: $_"
    exit 1  # Failure
}
```

### Best Practices

1. **Always use absolute paths** in your scripts
2. **Set `$ErrorActionPreference = "Stop"`** to catch errors
3. **Use try-catch blocks** for error handling
4. **Write informative output** for logging
5. **Use proper exit codes**:
   - `exit 0` for success
   - `exit 1` for failure
6. **Clean up resources** (close connections, dispose objects)
7. **Test scripts manually** before scheduling
8. **Handle missing files/resources gracefully**
9. **Use parameters** instead of hardcoded values
10. **Document your script** with comments

### Testing Your Scripts

Before adding a script to appsettings.json, test it manually:

```powershell
# Test script execution
powershell -ExecutionPolicy Bypass -File C:\Scripts\MyScript.ps1 -Param1 "TestValue"

# Check exit code
echo $LASTEXITCODE
```

### Debugging Tips

If your job fails:

1. **Check the logs** in `logs\taskscheduler-*.log`
2. **Run the script manually** with the same parameters
3. **Verify file paths** are correct and accessible
4. **Check permissions** - the service account must have access
5. **Review error messages** in the logs
6. **Use Write-Output** for debugging information

---

## Quick Reference

### Install Service
```powershell
cd C:\Services\TaskScheduler
.\Scripts\Install-Service.ps1
```

### Uninstall Service
```powershell
cd C:\Services\TaskScheduler
.\Scripts\Uninstall-Service.ps1
```

### Test Service
```powershell
cd C:\Services\TaskScheduler
.\Scripts\Test-Service.ps1
```

### View Logs
```powershell
Get-Content C:\Services\TaskScheduler\logs\taskscheduler-*.log -Tail 50 -Wait
```

### Check Service Status
```powershell
Get-Service TaskSchedulerService
```

### Restart Service
```powershell
Restart-Service TaskSchedulerService
```

---

## Troubleshooting

### Script Execution Policy

If you get execution policy errors:

```powershell
# Set execution policy for current user
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser

# Or bypass for a single script
powershell -ExecutionPolicy Bypass -File .\Install-Service.ps1
```

### Permission Denied

Scripts must be run as Administrator:

1. Right-click PowerShell
2. Select "Run as Administrator"
3. Navigate to script directory
4. Run the script

### Service Not Found

If Install-Service.ps1 can't find the executable:

- Ensure you're running the script from the application directory
- Verify TaskScheduler.exe exists in the same folder
- Check the `$ExecutablePath` variable in the script

---

## Additional Resources

- **Main Documentation**: See `README.md` in parent directory
- **Deployment Guide**: See `DEPLOYMENT.md`
- **Quick Start**: See `QUICKSTART.md`
- **Job Examples**: See `EXAMPLES.md`
