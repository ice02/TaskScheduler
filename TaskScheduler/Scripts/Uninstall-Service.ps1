# Uninstall Task Scheduler Windows Service
# This script must be run as Administrator

$ServiceName = "TaskSchedulerService"

# Check if script is running as Administrator
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Write-Host "ERROR: This script must be run as Administrator!" -ForegroundColor Red
    Write-Host "Please right-click PowerShell and select 'Run as Administrator'" -ForegroundColor Yellow
    exit 1
}

# Check if service exists
$existingService = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue

if (-not $existingService) {
    Write-Host "Service '$ServiceName' does not exist or is already uninstalled." -ForegroundColor Yellow
    exit 0
}

Write-Host "Found service: $ServiceName" -ForegroundColor Cyan
Write-Host "Status: $($existingService.Status)" -ForegroundColor Cyan
Write-Host ""

$response = Read-Host "Are you sure you want to uninstall this service? (y/n)"

if ($response -ne 'y' -and $response -ne 'Y') {
    Write-Host "Uninstallation cancelled." -ForegroundColor Yellow
    exit 0
}

# Stop the service if it's running
if ($existingService.Status -eq 'Running') {
    Write-Host "Stopping service..." -ForegroundColor Yellow
    Stop-Service -Name $ServiceName -Force
    Start-Sleep -Seconds 3
    
    $existingService = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
    if ($existingService.Status -eq 'Running') {
        Write-Host "WARNING: Service is still running. Forcing termination..." -ForegroundColor Yellow
        Start-Sleep -Seconds 2
    }
}

# Delete the service
Write-Host "Removing service..." -ForegroundColor Yellow
sc.exe delete $ServiceName

if ($?) {
    Write-Host ""
    Write-Host "SUCCESS! Service '$ServiceName' has been uninstalled." -ForegroundColor Green
    Write-Host ""
    Write-Host "Note: Log files and configuration files have been preserved." -ForegroundColor Cyan
    Write-Host "You can manually delete them if needed." -ForegroundColor Cyan
    Write-Host ""
}
else {
    Write-Host ""
    Write-Host "ERROR: Failed to uninstall service." -ForegroundColor Red
    Write-Host "The service may be in use or locked by another process." -ForegroundColor Yellow
    Write-Host "Try restarting your computer and running this script again." -ForegroundColor Yellow
    exit 1
}

Write-Host "Uninstallation completed!" -ForegroundColor Green
