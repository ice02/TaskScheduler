# Example PowerShell script for testing Task Scheduler
# This script demonstrates logging and exit codes

param(
    [string]$Param1 = "DefaultValue",
    [string]$Param2 = "DefaultValue2"
)

Write-Output "======================================"
Write-Output "Example PowerShell Script Execution"
Write-Output "======================================"
Write-Output "Timestamp: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
Write-Output "Parameter 1: $Param1"
Write-Output "Parameter 2: $Param2"
Write-Output "Computer Name: $env:COMPUTERNAME"
Write-Output "User: $env:USERNAME"
Write-Output "======================================"

# Simulate some work
Write-Output "Starting work simulation..."
Start-Sleep -Seconds 2

Write-Output "Work completed successfully!"
Write-Output "======================================"

# Exit with success code
exit 0
