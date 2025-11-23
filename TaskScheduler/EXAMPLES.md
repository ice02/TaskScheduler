# Job Configuration Examples

This document provides common job configuration examples for the Task Scheduler Service.

## Table of Contents

1. [Database Backup Jobs](#database-backup-jobs)
2. [File Maintenance Jobs](#file-maintenance-jobs)
3. [Data Processing Jobs](#data-processing-jobs)
4. [Monitoring Jobs](#monitoring-jobs)
5. [Integration Jobs](#integration-jobs)
6. [Cleanup Jobs](#cleanup-jobs)

---

## Database Backup Jobs

### SQL Server Database Backup

**PowerShell Script** (`Backup-SqlDatabase.ps1`):
```powershell
param(
    [string]$ServerInstance = "localhost",
    [string]$Database = "MyDatabase",
    [string]$BackupPath = "C:\Backups\SQL"
)

$ErrorActionPreference = "Stop"

try {
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $backupFile = Join-Path $BackupPath "${Database}_${timestamp}.bak"
    
    # Ensure backup directory exists
    New-Item -Path $BackupPath -ItemType Directory -Force -ErrorAction SilentlyContinue
    
    # Perform backup using SQL Server cmdlets
    Backup-SqlDatabase -ServerInstance $ServerInstance -Database $Database -BackupFile $backupFile
    
    Write-Output "Backup completed successfully: $backupFile"
    
    # Cleanup old backups (keep last 7 days)
    Get-ChildItem $BackupPath -Filter "*.bak" | 
        Where-Object { $_.CreationTime -lt (Get-Date).AddDays(-7) } | 
        Remove-Item -Force
    
    Write-Output "Old backups cleaned up"
    exit 0
}
catch {
    Write-Error "Backup failed: $_"
    exit 1
}
```

**appsettings.json Configuration**:
```json
{
  "Name": "SqlDatabaseBackup",
  "Type": "PowerShell",
  "Path": "C:\\Scripts\\Backup-SqlDatabase.ps1",
  "Arguments": "-ServerInstance localhost -Database MyDatabase -BackupPath C:\\Backups\\SQL",
  "CronExpression": "0 2 * * *",
  "MaxExecutionTimeMinutes": 120,
  "Enabled": true
}
```

---

## File Maintenance Jobs

### Clean Temporary Files

**PowerShell Script** (`Clean-TempFiles.ps1`):
```powershell
param(
    [string]$Path = "C:\Temp",
    [int]$DaysOld = 7
)

$ErrorActionPreference = "Stop"

try {
    $cutoffDate = (Get-Date).AddDays(-$DaysOld)
    $files = Get-ChildItem -Path $Path -Recurse -File | Where-Object { $_.LastWriteTime -lt $cutoffDate }
    
    $totalSize = ($files | Measure-Object -Property Length -Sum).Sum
    $fileCount = $files.Count
    
    Write-Output "Found $fileCount files older than $DaysOld days (Total size: $([math]::Round($totalSize/1MB, 2)) MB)"
    
    $files | Remove-Item -Force -ErrorAction Continue
    
    Write-Output "Cleanup completed successfully"
    exit 0
}
catch {
    Write-Error "Cleanup failed: $_"
    exit 1
}
```

**appsettings.json Configuration**:
```json
{
  "Name": "CleanTempFiles",
  "Type": "PowerShell",
  "Path": "C:\\Scripts\\Clean-TempFiles.ps1",
  "Arguments": "-Path C:\\Temp -DaysOld 7",
  "CronExpression": "0 3 * * *",
  "MaxExecutionTimeMinutes": 30,
  "Enabled": true
}
```

### Archive Old Log Files

**PowerShell Script** (`Archive-LogFiles.ps1`):
```powershell
param(
    [string]$SourcePath = "C:\Logs",
    [string]$ArchivePath = "C:\Archives",
    [int]$DaysOld = 30
)

$ErrorActionPreference = "Stop"

try {
    $cutoffDate = (Get-Date).AddDays(-$DaysOld)
    $archiveDate = Get-Date -Format "yyyyMMdd"
    
    # Find old log files
    $logFiles = Get-ChildItem -Path $SourcePath -Filter "*.log" -Recurse | 
        Where-Object { $_.LastWriteTime -lt $cutoffDate }
    
    if ($logFiles.Count -eq 0) {
        Write-Output "No log files to archive"
        exit 0
    }
    
    # Create archive directory
    $archiveDir = Join-Path $ArchivePath "Logs_$archiveDate"
    New-Item -Path $archiveDir -ItemType Directory -Force | Out-Null
    
    # Move files to archive
    foreach ($file in $logFiles) {
        $destination = Join-Path $archiveDir $file.Name
        Move-Item -Path $file.FullName -Destination $destination -Force
    }
    
    # Compress archive
    $zipFile = "$archiveDir.zip"
    Compress-Archive -Path $archiveDir -DestinationPath $zipFile -CompressionLevel Optimal
    Remove-Item -Path $archiveDir -Recurse -Force
    
    Write-Output "Archived $($logFiles.Count) log files to $zipFile"
    exit 0
}
catch {
    Write-Error "Archive failed: $_"
    exit 1
}
```

**appsettings.json Configuration**:
```json
{
  "Name": "ArchiveLogFiles",
  "Type": "PowerShell",
  "Path": "C:\\Scripts\\Archive-LogFiles.ps1",
  "Arguments": "-SourcePath C:\\Logs -ArchivePath C:\\Archives -DaysOld 30",
  "CronExpression": "0 1 1 * *",
  "MaxExecutionTimeMinutes": 60,
  "Enabled": true
}
```

---

## Data Processing Jobs

### CSV Data Import

**PowerShell Script** (`Import-CsvData.ps1`):
```powershell
param(
    [string]$CsvPath = "C:\Data\Import\data.csv",
    [string]$ConnectionString = "Server=localhost;Database=MyDB;Integrated Security=True;"
)

$ErrorActionPreference = "Stop"

try {
    if (-not (Test-Path $CsvPath)) {
        Write-Output "CSV file not found: $CsvPath"
        exit 0
    }
    
    # Import CSV
    $data = Import-Csv -Path $CsvPath
    Write-Output "Loaded $($data.Count) records from CSV"
    
    # Connect to database
    $connection = New-Object System.Data.SqlClient.SqlConnection($ConnectionString)
    $connection.Open()
    
    # Insert data
    $insertedCount = 0
    foreach ($row in $data) {
        $query = "INSERT INTO ImportTable (Column1, Column2, Column3) VALUES (@Col1, @Col2, @Col3)"
        $command = $connection.CreateCommand()
        $command.CommandText = $query
        $command.Parameters.AddWithValue("@Col1", $row.Column1) | Out-Null
        $command.Parameters.AddWithValue("@Col2", $row.Column2) | Out-Null
        $command.Parameters.AddWithValue("@Col3", $row.Column3) | Out-Null
        $command.ExecuteNonQuery() | Out-Null
        $insertedCount++
    }
    
    $connection.Close()
    
    # Move processed file
    $processedPath = "C:\Data\Processed"
    New-Item -Path $processedPath -ItemType Directory -Force -ErrorAction SilentlyContinue
    Move-Item -Path $CsvPath -Destination "$processedPath\$(Split-Path $CsvPath -Leaf).$(Get-Date -Format 'yyyyMMddHHmmss')" -Force
    
    Write-Output "Successfully imported $insertedCount records"
    exit 0
}
catch {
    Write-Error "Import failed: $_"
    exit 1
}
finally {
    if ($connection -and $connection.State -eq 'Open') {
        $connection.Close()
    }
}
```

**appsettings.json Configuration**:
```json
{
  "Name": "ImportCsvData",
  "Type": "PowerShell",
  "Path": "C:\\Scripts\\Import-CsvData.ps1",
  "Arguments": "-CsvPath C:\\Data\\Import\\data.csv",
  "CronExpression": "*/15 * * * *",
  "MaxExecutionTimeMinutes": 15,
  "Enabled": true
}
```

### Data Export to Excel

**PowerShell Script** (`Export-DataToExcel.ps1`):
```powershell
param(
    [string]$ConnectionString = "Server=localhost;Database=MyDB;Integrated Security=True;",
    [string]$OutputPath = "C:\Reports\Export.xlsx",
    [string]$Query = "SELECT * FROM DataTable"
)

$ErrorActionPreference = "Stop"

try {
    # Query database
    $connection = New-Object System.Data.SqlClient.SqlConnection($ConnectionString)
    $connection.Open()
    
    $command = $connection.CreateCommand()
    $command.CommandText = $Query
    
    $adapter = New-Object System.Data.SqlClient.SqlDataAdapter($command)
    $dataSet = New-Object System.Data.DataSet
    $adapter.Fill($dataSet) | Out-Null
    
    $connection.Close()
    
    $data = $dataSet.Tables[0]
    Write-Output "Retrieved $($data.Rows.Count) rows"
    
    # Export to CSV (Excel compatible)
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $csvPath = $OutputPath -replace '\.xlsx$', "_$timestamp.csv"
    
    $data | Export-Csv -Path $csvPath -NoTypeInformation
    
    Write-Output "Exported data to $csvPath"
    exit 0
}
catch {
    Write-Error "Export failed: $_"
    exit 1
}
finally {
    if ($connection -and $connection.State -eq 'Open') {
        $connection.Close()
    }
}
```

**appsettings.json Configuration**:
```json
{
  "Name": "ExportDataToExcel",
  "Type": "PowerShell",
  "Path": "C:\\Scripts\\Export-DataToExcel.ps1",
  "Arguments": "-OutputPath C:\\Reports\\DailyExport.xlsx",
  "CronExpression": "0 18 * * *",
  "MaxExecutionTimeMinutes": 30,
  "Enabled": true
}
```

---

## Monitoring Jobs

### Check Disk Space

**PowerShell Script** (`Check-DiskSpace.ps1`):
```powershell
param(
    [int]$ThresholdPercent = 80
)

$ErrorActionPreference = "Stop"

try {
    $drives = Get-PSDrive -PSProvider FileSystem | Where-Object { $_.Used -ne $null }
    $alerts = @()
    
    foreach ($drive in $drives) {
        $usedPercent = [math]::Round(($drive.Used / ($drive.Used + $drive.Free)) * 100, 2)
        
        Write-Output "$($drive.Name): Used $usedPercent% ($([math]::Round($drive.Free/1GB, 2)) GB free)"
        
        if ($usedPercent -gt $ThresholdPercent) {
            $alerts += "$($drive.Name): drive is at $usedPercent% capacity"
        }
    }
    
    if ($alerts.Count -gt 0) {
        Write-Error "ALERT: Disk space threshold exceeded on $($alerts.Count) drive(s):`n$($alerts -join "`n")"
        exit 1
    }
    
    Write-Output "All drives within acceptable limits"
    exit 0
}
catch {
    Write-Error "Check failed: $_"
    exit 1
}
```

**appsettings.json Configuration**:
```json
{
  "Name": "CheckDiskSpace",
  "Type": "PowerShell",
  "Path": "C:\\Scripts\\Check-DiskSpace.ps1",
  "Arguments": "-ThresholdPercent 80",
  "CronExpression": "0 */4 * * *",
  "MaxExecutionTimeMinutes": 5,
  "Enabled": true
}
```

### Monitor Windows Services

**PowerShell Script** (`Monitor-Services.ps1`):
```powershell
param(
    [string[]]$ServiceNames = @("MSSQLSERVER", "W3SVC")
)

$ErrorActionPreference = "Stop"

try {
    $failedServices = @()
    
    foreach ($serviceName in $ServiceNames) {
        $service = Get-Service -Name $serviceName -ErrorAction SilentlyContinue
        
        if (-not $service) {
            $failedServices += "$serviceName (Not Found)"
            continue
        }
        
        Write-Output "$serviceName : $($service.Status)"
        
        if ($service.Status -ne 'Running') {
            $failedServices += "$serviceName ($($service.Status))"
            
            # Attempt to start the service
            Write-Output "Attempting to start $serviceName..."
            Start-Service -Name $serviceName -ErrorAction Continue
            Start-Sleep -Seconds 5
            
            $service = Get-Service -Name $serviceName
            if ($service.Status -eq 'Running') {
                Write-Output "$serviceName started successfully"
            }
            else {
                Write-Error "$serviceName failed to start"
            }
        }
    }
    
    if ($failedServices.Count -gt 0) {
        Write-Error "ALERT: $($failedServices.Count) service(s) not running:`n$($failedServices -join "`n")"
        exit 1
    }
    
    Write-Output "All monitored services are running"
    exit 0
}
catch {
    Write-Error "Monitor failed: $_"
    exit 1
}
```

**appsettings.json Configuration**:
```json
{
  "Name": "MonitorServices",
  "Type": "PowerShell",
  "Path": "C:\\Scripts\\Monitor-Services.ps1",
  "Arguments": "-ServiceNames MSSQLSERVER,W3SVC",
  "CronExpression": "*/5 * * * *",
  "MaxExecutionTimeMinutes": 5,
  "Enabled": true
}
```

---

## Integration Jobs

### Sync Files to FTP

**PowerShell Script** (`Sync-ToFtp.ps1`):
```powershell
param(
    [string]$LocalPath = "C:\Export",
    [string]$FtpServer = "ftp://ftp.example.com",
    [string]$Username = "ftpuser",
    [string]$Password = "ftppassword"
)

$ErrorActionPreference = "Stop"

try {
    $files = Get-ChildItem -Path $LocalPath -File
    Write-Output "Found $($files.Count) files to upload"
    
    $uploadedCount = 0
    foreach ($file in $files) {
        $ftpUri = "$FtpServer/$($file.Name)"
        
        # Create FTP request
        $request = [System.Net.FtpWebRequest]::Create($ftpUri)
        $request.Method = [System.Net.WebRequestMethods+Ftp]::UploadFile
        $request.Credentials = New-Object System.Net.NetworkCredential($Username, $Password)
        
        # Upload file
        $fileContent = [System.IO.File]::ReadAllBytes($file.FullName)
        $request.ContentLength = $fileContent.Length
        
        $requestStream = $request.GetRequestStream()
        $requestStream.Write($fileContent, 0, $fileContent.Length)
        $requestStream.Close()
        
        $response = $request.GetResponse()
        Write-Output "Uploaded: $($file.Name) ($($response.StatusDescription))"
        $response.Close()
        
        $uploadedCount++
    }
    
    Write-Output "Successfully uploaded $uploadedCount files"
    exit 0
}
catch {
    Write-Error "FTP sync failed: $_"
    exit 1
}
```

**appsettings.json Configuration**:
```json
{
  "Name": "SyncToFtp",
  "Type": "PowerShell",
  "Path": "C:\\Scripts\\Sync-ToFtp.ps1",
  "Arguments": "-LocalPath C:\\Export -FtpServer ftp://ftp.example.com -Username ftpuser -Password ftppassword",
  "CronExpression": "0 */6 * * *",
  "MaxExecutionTimeMinutes": 45,
  "Enabled": true
}
```

---

## Cleanup Jobs

### Purge Old Database Records

**PowerShell Script** (`Purge-OldRecords.ps1`):
```powershell
param(
    [string]$ConnectionString = "Server=localhost;Database=MyDB;Integrated Security=True;",
    [string]$TableName = "LogTable",
    [string]$DateColumn = "CreatedDate",
    [int]$RetentionDays = 90
)

$ErrorActionPreference = "Stop"

try {
    $cutoffDate = (Get-Date).AddDays(-$RetentionDays)
    
    $connection = New-Object System.Data.SqlClient.SqlConnection($ConnectionString)
    $connection.Open()
    
    # Count records to be deleted
    $countQuery = "SELECT COUNT(*) FROM $TableName WHERE $DateColumn < @CutoffDate"
    $countCommand = $connection.CreateCommand()
    $countCommand.CommandText = $countQuery
    $countCommand.Parameters.AddWithValue("@CutoffDate", $cutoffDate) | Out-Null
    
    $recordCount = $countCommand.ExecuteScalar()
    Write-Output "Found $recordCount records older than $RetentionDays days"
    
    if ($recordCount -eq 0) {
        Write-Output "No records to purge"
        $connection.Close()
        exit 0
    }
    
    # Delete records
    $deleteQuery = "DELETE FROM $TableName WHERE $DateColumn < @CutoffDate"
    $deleteCommand = $connection.CreateCommand()
    $deleteCommand.CommandText = $deleteQuery
    $deleteCommand.Parameters.AddWithValue("@CutoffDate", $cutoffDate) | Out-Null
    
    $deletedCount = $deleteCommand.ExecuteNonQuery()
    $connection.Close()
    
    Write-Output "Successfully purged $deletedCount records"
    exit 0
}
catch {
    Write-Error "Purge failed: $_"
    exit 1
}
finally {
    if ($connection -and $connection.State -eq 'Open') {
        $connection.Close()
    }
}
```

**appsettings.json Configuration**:
```json
{
  "Name": "PurgeOldRecords",
  "Type": "PowerShell",
  "Path": "C:\\Scripts\\Purge-OldRecords.ps1",
  "Arguments": "-TableName LogTable -DateColumn CreatedDate -RetentionDays 90",
  "CronExpression": "0 4 * * 0",
  "MaxExecutionTimeMinutes": 120,
  "Enabled": true
}
```

---

## Using External Executables

### Running 7-Zip for Compression

**appsettings.json Configuration**:
```json
{
  "Name": "CompressBackups",
  "Type": "Executable",
  "Path": "C:\\Program Files\\7-Zip\\7z.exe",
  "Arguments": "a -tzip C:\\Archives\\backup.zip C:\\Backups\\*.bak",
  "CronExpression": "0 5 * * *",
  "MaxExecutionTimeMinutes": 60,
  "Enabled": true
}
```

### Running Robocopy for File Sync

**appsettings.json Configuration**:
```json
{
  "Name": "SyncToBackupServer",
  "Type": "Executable",
  "Path": "C:\\Windows\\System32\\Robocopy.exe",
  "Arguments": "C:\\Data \\\\BackupServer\\Share\\Data /MIR /R:3 /W:10 /LOG:C:\\Logs\\robocopy.log",
  "CronExpression": "0 22 * * *",
  "MaxExecutionTimeMinutes": 180,
  "Enabled": true
}
```

---

## Best Practices

1. **Exit Codes**: Always use `exit 0` for success and `exit 1` for failure
2. **Error Handling**: Use `$ErrorActionPreference = "Stop"` and try-catch blocks
3. **Logging**: Write meaningful output that appears in logs
4. **Timeouts**: Set realistic `MaxExecutionTimeMinutes` values
5. **Testing**: Test scripts manually before scheduling
6. **Security**: Store credentials securely (Windows Credential Manager, Azure Key Vault)
7. **Paths**: Always use absolute paths, not relative paths
8. **Resources**: Clean up resources (close connections, file handles)
9. **Idempotency**: Design jobs to be safely re-runnable
10. **Documentation**: Comment your scripts and document job purposes
