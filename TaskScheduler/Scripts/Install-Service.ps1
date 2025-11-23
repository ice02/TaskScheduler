# Install Task Scheduler Windows Service
# This script must be run as Administrator

$ServiceName = "TaskSchedulerService"
$ServiceDisplayName = "Task Scheduler Service"
$ServiceDescription = "Executes scheduled PowerShell scripts and executables based on configuration"

# Get the current directory (where the script is located)
$ScriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$ExecutablePath = Join-Path $ScriptPath "TaskScheduler.exe"

# Check if script is running as Administrator
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Write-Host "ERROR: This script must be run as Administrator!" -ForegroundColor Red
    Write-Host "Please right-click PowerShell and select 'Run as Administrator'" -ForegroundColor Yellow
    exit 1
}

# Check if executable exists
if (-not (Test-Path $ExecutablePath)) {
    Write-Host "ERROR: TaskScheduler.exe not found at: $ExecutablePath" -ForegroundColor Red
    Write-Host "Please make sure you're running this script from the application directory." -ForegroundColor Yellow
    exit 1
}

# Check if service already exists
$existingService = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue

if ($existingService) {
    Write-Host "Service '$ServiceName' already exists." -ForegroundColor Yellow
    $response = Read-Host "Do you want to reinstall it? (y/n)"
    
    if ($response -eq 'y' -or $response -eq 'Y') {
        Write-Host "Stopping service..." -ForegroundColor Yellow
        Stop-Service -Name $ServiceName -Force -ErrorAction SilentlyContinue
        Start-Sleep -Seconds 2
        
        Write-Host "Removing existing service..." -ForegroundColor Yellow
        sc.exe delete $ServiceName
        Start-Sleep -Seconds 2
    }
    else {
        Write-Host "Installation cancelled." -ForegroundColor Yellow
        exit 0
    }
}

# Create the service
Write-Host "Creating Windows Service..." -ForegroundColor Green
New-Service -Name $ServiceName `
    -BinaryPathName $ExecutablePath `
    -DisplayName $ServiceDisplayName `
    -Description $ServiceDescription `
    -StartupType Automatic

if ($?) {
    Write-Host "Service created successfully!" -ForegroundColor Green
    
    # Set service to restart on failure
    Write-Host "Configuring service recovery options..." -ForegroundColor Green
    sc.exe failure $ServiceName reset= 86400 actions= restart/60000/restart/60000/restart/60000
    
    # Start the service
    Write-Host "Starting service..." -ForegroundColor Green
    Start-Service -Name $ServiceName
    
    Start-Sleep -Seconds 2
    
    # Check service status
    $service = Get-Service -Name $ServiceName
    if ($service.Status -eq 'Running') {
        Write-Host ""
        Write-Host "SUCCESS! Service is now running." -ForegroundColor Green
        Write-Host ""
        Write-Host "Service Name: $ServiceName" -ForegroundColor Cyan
        Write-Host "Display Name: $ServiceDisplayName" -ForegroundColor Cyan
        Write-Host "Status: Running" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "You can manage this service using:" -ForegroundColor Yellow
        Write-Host "  - Services.msc (Windows Services Manager)" -ForegroundColor White
        Write-Host "  - PowerShell: Get-Service $ServiceName" -ForegroundColor White
        Write-Host ""
    }
    else {
        Write-Host "WARNING: Service was created but is not running." -ForegroundColor Yellow
        Write-Host "Status: $($service.Status)" -ForegroundColor Yellow
        Write-Host "Check the logs for more information." -ForegroundColor Yellow
    }
}
else {
    Write-Host "ERROR: Failed to create service." -ForegroundColor Red
    exit 1
}

Write-Host "Installation completed!" -ForegroundColor Green
