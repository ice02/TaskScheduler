# Job Execution Logging - Detailed Status Tracking

## Overview

The Task Scheduler Service now includes **comprehensive job execution logging** with detailed status tracking for every job run. Each execution is tracked with unique IDs, execution counters, timing information, and formatted log output.

---

## Features

### 1. Detailed Job Start Logging

Every job execution starts with a formatted log box containing:
- **Job Name** - The name of the job
- **Execution ID** - Unique 8-character identifier for this execution
- **Execution #** - Sequential execution counter (1, 2, 3...)
- **Job Type** - PowerShell or Executable
- **Script/Executable Path** - Full path to the file
- **Arguments** - Command-line arguments (or "(none)")
- **Max Execution Time** - Timeout in minutes
- **Start Time** - Precise timestamp with milliseconds

### 2. Status Tracking

Each execution is tracked with one of four statuses:
- ? **SUCCESS** - Job completed successfully (exit code 0)
- ? **FAILED** - Job failed with error
- ?? **TIMEOUT** - Job exceeded max execution time
- ?? **SKIPPED** - Job already running (overlap detection)

### 3. Execution Counter

The service maintains a counter for each job, tracking how many times it has run since service start.

### 4. Unique Execution IDs

Each job run gets a unique 8-character execution ID (e.g., `a3f7b2c9`) for easy tracking in logs.

### 5. Duration Tracking

Precise duration tracking with millisecond accuracy using `Stopwatch`.

### 6. Formatted Log Output

Logs use visual box formatting (?????) for easy reading and parsing.

---

## Log Examples

### Successful Execution

```
[2024-01-15 14:30:45.123 +01:00] [INF] ????????????????????????????????????????????????????????????
[2024-01-15 14:30:45.124 +01:00] [INF] ? JOB EXECUTION STARTED
[2024-01-15 14:30:45.125 +01:00] [INF] ????????????????????????????????????????????????????????????
[2024-01-15 14:30:45.126 +01:00] [INF] ? Job Name: BackupDatabase
[2024-01-15 14:30:45.127 +01:00] [INF] ? Execution ID: a3f7b2c9
[2024-01-15 14:30:45.128 +01:00] [INF] ? Execution #: 5
[2024-01-15 14:30:45.129 +01:00] [INF] ? Job Type: PowerShell
[2024-01-15 14:30:45.130 +01:00] [INF] ? Script/Executable: C:\Scripts\backup.ps1
[2024-01-15 14:30:45.131 +01:00] [INF] ? Arguments: -DatabaseName Production -FullBackup
[2024-01-15 14:30:45.132 +01:00] [INF] ? Max Execution Time: 30 minutes
[2024-01-15 14:30:45.133 +01:00] [INF] ? Start Time: 2024-01-15 14:30:45.123
[2024-01-15 14:30:45.134 +01:00] [INF] ????????????????????????????????????????????????????????????

[... job execution output ...]

[2024-01-15 14:35:12.456 +01:00] [INF] ????????????????????????????????????????????????????????????
[2024-01-15 14:35:12.457 +01:00] [INF] ? JOB EXECUTION COMPLETED
[2024-01-15 14:35:12.458 +01:00] [INF] ????????????????????????????????????????????????????????????
[2024-01-15 14:35:12.459 +01:00] [INF] ? Job Name: BackupDatabase
[2024-01-15 14:35:12.460 +01:00] [INF] ? Execution ID: a3f7b2c9
[2024-01-15 14:35:12.461 +01:00] [INF] ? Status: ? SUCCESS
[2024-01-15 14:35:12.462 +01:00] [INF] ? Exit Code: 0
[2024-01-15 14:35:12.463 +01:00] [INF] ? Duration: 00:04:27.333
[2024-01-15 14:35:12.464 +01:00] [INF] ? End Time: 2024-01-15 14:35:12.456
[2024-01-15 14:35:12.465 +01:00] [INF] ????????????????????????????????????????????????????????????
[2024-01-15 14:35:12.466 +01:00] [INF] Job BackupDatabase execution summary - Status: SUCCESS, Duration: 00:04:27.333, Execution #5
```

---

### Failed Execution

```
[2024-01-15 15:00:00.000 +01:00] [INF] ????????????????????????????????????????????????????????????
[2024-01-15 15:00:00.001 +01:00] [INF] ? JOB EXECUTION STARTED
[2024-01-15 15:00:00.002 +01:00] [INF] ????????????????????????????????????????????????????????????
[2024-01-15 15:00:00.003 +01:00] [INF] ? Job Name: DataExport
[2024-01-15 15:00:00.004 +01:00] [INF] ? Execution ID: b8c2d4e1
[2024-01-15 15:00:00.005 +01:00] [INF] ? Execution #: 3
[2024-01-15 15:00:00.006 +01:00] [INF] ? Job Type: Executable
[2024-01-15 15:00:00.007 +01:00] [INF] ? Script/Executable: C:\Tools\export.exe
[2024-01-15 15:00:00.008 +01:00] [INF] ? Arguments: --format csv --output D:\exports
[2024-01-15 15:00:00.009 +01:00] [INF] ? Max Execution Time: 15 minutes
[2024-01-15 15:00:00.010 +01:00] [INF] ? Start Time: 2024-01-15 15:00:00.000
[2024-01-15 15:00:00.011 +01:00] [INF] ????????????????????????????????????????????????????????????

[... job execution attempts ...]

[2024-01-15 15:00:05.234 +01:00] [ERR] ????????????????????????????????????????????????????????????
[2024-01-15 15:00:05.235 +01:00] [ERR] ? JOB EXECUTION FAILED
[2024-01-15 15:00:05.236 +01:00] [ERR] ????????????????????????????????????????????????????????????
[2024-01-15 15:00:05.237 +01:00] [ERR] ? Job Name: DataExport
[2024-01-15 15:00:05.238 +01:00] [ERR] ? Execution ID: b8c2d4e1
[2024-01-15 15:00:05.239 +01:00] [ERR] ? Status: ? FAILED
[2024-01-15 15:00:05.240 +01:00] [ERR] ? Duration: 00:00:05.234
[2024-01-15 15:00:05.241 +01:00] [ERR] ? End Time: 2024-01-15 15:00:05.234
[2024-01-15 15:00:05.242 +01:00] [ERR] ? Error Type: InvalidOperationException
[2024-01-15 15:00:05.243 +01:00] [ERR] ? Error Message: Process exited with code 1. Error output: Cannot access file
[2024-01-15 15:00:05.244 +01:00] [ERR] ? Exit Code: -1
[2024-01-15 15:00:05.245 +01:00] [ERR] ????????????????????????????????????????????????????????????
[2024-01-15 15:00:05.246 +01:00] [INF] Job DataExport execution summary - Status: FAILED, Duration: 00:00:05.234, Execution #3
```

---

### Timeout Execution

```
[2024-01-15 16:00:00.000 +01:00] [INF] ????????????????????????????????????????????????????????????
[2024-01-15 16:00:00.001 +01:00] [INF] ? JOB EXECUTION STARTED
[2024-01-15 16:00:00.002 +01:00] [INF] ????????????????????????????????????????????????????????????
[2024-01-15 16:00:00.003 +01:00] [INF] ? Job Name: LongRunningTask
[2024-01-15 16:00:00.004 +01:00] [INF] ? Execution ID: c9d3e5f2
[2024-01-15 16:00:00.005 +01:00] [INF] ? Execution #: 1
[2024-01-15 16:00:00.006 +01:00] [INF] ? Job Type: PowerShell
[2024-01-15 16:00:00.007 +01:00] [INF] ? Script/Executable: C:\Scripts\long-task.ps1
[2024-01-15 16:00:00.008 +01:00] [INF] ? Arguments: (none)
[2024-01-15 16:00:00.009 +01:00] [INF] ? Max Execution Time: 5 minutes
[2024-01-15 16:00:00.010 +01:00] [INF] ? Start Time: 2024-01-15 16:00:00.000
[2024-01-15 16:00:00.011 +01:00] [INF] ????????????????????????????????????????????????????????????

[... job runs for 5 minutes ...]

[2024-01-15 16:05:00.123 +01:00] [ERR] ????????????????????????????????????????????????????????????
[2024-01-15 16:05:00.124 +01:00] [ERR] ? JOB EXECUTION TIMEOUT
[2024-01-15 16:05:00.125 +01:00] [ERR] ????????????????????????????????????????????????????????????
[2024-01-15 16:05:00.126 +01:00] [ERR] ? Job Name: LongRunningTask
[2024-01-15 16:05:00.127 +01:00] [ERR] ? Execution ID: c9d3e5f2
[2024-01-15 16:05:00.128 +01:00] [ERR] ? Status: ? TIMEOUT
[2024-01-15 16:05:00.129 +01:00] [ERR] ? Max Time: 5 minutes
[2024-01-15 16:05:00.130 +01:00] [ERR] ? Actual Duration: 00:05:00.123
[2024-01-15 16:05:00.131 +01:00] [ERR] ? End Time: 2024-01-15 16:05:00.123
[2024-01-15 16:05:00.132 +01:00] [ERR] ? Error: Job exceeded maximum execution time of 5 minutes and was terminated.
[2024-01-15 16:05:00.133 +01:00] [ERR] ????????????????????????????????????????????????????????????
[2024-01-15 16:05:00.134 +01:00] [INF] Job LongRunningTask execution summary - Status: TIMEOUT, Duration: 00:05:00.123, Execution #1
```

---

### Job Overlap (Skipped)

```
[2024-01-15 17:00:00.000 +01:00] [WRN] ???????????????????????????????????????????????????????????
[2024-01-15 17:00:00.001 +01:00] [WRN] JOB OVERLAP DETECTED
[2024-01-15 17:00:00.002 +01:00] [WRN] Job Name: BackupDatabase
[2024-01-15 17:00:00.003 +01:00] [WRN] Status: SKIPPED (already running)
[2024-01-15 17:00:00.004 +01:00] [WRN] ???????????????????????????????????????????????????????????
```

---

## Log Levels

The service uses different log levels for different types of information:

| Level | Usage | Examples |
|-------|-------|----------|
| **Information** | Normal operation, success | Job started, Job completed, Summary |
| **Warning** | Non-critical issues | Overlap detected, Non-zero exit code |
| **Error** | Failures and timeouts | Job failed, Timeout exceeded |
| **Debug** | Detailed execution info | Process output, Process start |

---

## Log Information Captured

### For Every Execution

- **Job Name** - Identifies the job
- **Execution ID** - Unique identifier for this run
- **Execution Counter** - How many times this job has run
- **Job Type** - PowerShell or Executable
- **Path** - Full path to script/executable
- **Arguments** - Command-line arguments
- **Max Time** - Configured timeout
- **Start Time** - When execution began
- **End Time** - When execution finished
- **Duration** - Elapsed time (hh:mm:ss.fff)
- **Status** - SUCCESS, FAILED, TIMEOUT, or SKIPPED
- **Exit Code** - Process exit code (if available)

### For Failures

Additional information for failed executions:
- **Error Type** - Exception type (e.g., FileNotFoundException)
- **Error Message** - Detailed error message
- **Stack Trace** - Full stack trace (debug level)

### Process Output

- **Standard Output** - Captured at debug level with `[JobName] OUTPUT:` prefix
- **Standard Error** - Captured at warning level with `[JobName] ERROR:` prefix

---

## Parsing Logs

### Extract Job Executions

```powershell
# Get all job executions
Get-Content logs\taskscheduler-*.log | 
  Select-String "JOB EXECUTION (STARTED|COMPLETED|FAILED|TIMEOUT)"

# Get executions for specific job
Get-Content logs\taskscheduler-*.log | 
  Select-String "Job Name: BackupDatabase"

# Get failed jobs only
Get-Content logs\taskscheduler-*.log | 
  Select-String "JOB EXECUTION FAILED"
```

### Extract Execution Statistics

```powershell
# Count executions by status
Get-Content logs\taskscheduler-*.log | 
  Select-String "execution summary - Status: (\w+)" | 
  ForEach-Object { $_.Matches.Groups[1].Value } | 
  Group-Object | 
  Select-Object Count, Name

# Calculate average duration
Get-Content logs\taskscheduler-*.log | 
  Select-String "Duration: (\d+:\d+:\d+\.\d+)" | 
  ForEach-Object { [TimeSpan]::Parse($_.Matches.Groups[1].Value) } | 
  Measure-Object -Average -Property TotalSeconds
```

### Monitor Real-Time

```powershell
# Follow logs in real-time
Get-Content logs\taskscheduler-*.log -Tail 50 -Wait

# Filter for specific job
Get-Content logs\taskscheduler-*.log -Wait | 
  Select-String "BackupDatabase"

# Show only errors and warnings
Get-Content logs\taskscheduler-*.log -Wait | 
  Select-String "\[(WRN|ERR)\]"
```

---

## Integration with Monitoring Tools

### Splunk

```spl
index=taskscheduler source="taskscheduler-*.log"
| rex field=_raw "Job Name: (?<job_name>[^\s]+)"
| rex field=_raw "Status: [??????]?\s*(?<status>\w+)"
| rex field=_raw "Duration: (?<duration>[\d:\.]+)"
| rex field=_raw "Execution #: (?<execution_num>\d+)"
| stats count by job_name, status
```

### ELK Stack (Elasticsearch, Logstash, Kibana)

Logstash configuration:
```conf
filter {
  grok {
    match => { "message" => "%{TIMESTAMP_ISO8601:timestamp} \[%{LOGLEVEL:level}\] ? Job Name: %{DATA:job_name}" }
    match => { "message" => "Status: [??]?\s*%{WORD:status}" }
    match => { "message" => "Duration: %{TIME:duration}" }
    match => { "message" => "Execution #: %{NUMBER:execution_num}" }
  }
}
```

### Prometheus / Grafana

Export metrics from logs:
```promql
# Success rate
sum(rate(job_executions_total{status="SUCCESS"}[5m])) 
/ 
sum(rate(job_executions_total[5m]))

# Average duration by job
avg(job_execution_duration_seconds) by (job_name)

# Failure count
sum(increase(job_executions_total{status="FAILED"}[1h])) by (job_name)
```

---

## Configuration

### Adjust Log Levels

To see more detailed process output, change log level in `appsettings.json`:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug",  // Changed from "Information"
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    }
  }
}
```

### Custom Output Template

Customize the log format:

```json
{
  "Serilog": {
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      }
    ]
  }
}
```

---

## Benefits

### 1. Complete Audit Trail
- Every job execution is logged with full details
- Unique execution IDs for tracking
- Execution counters show job history

### 2. Easy Troubleshooting
- Formatted logs are easy to read
- Error details captured with context
- Stack traces available for debugging

### 3. Performance Monitoring
- Duration tracking with millisecond precision
- Identify slow-running jobs
- Track performance over time

### 4. Status Tracking
- Know exactly what happened to each job
- Success/failure rates
- Timeout detection

### 5. Integration Ready
- Structured logging format
- Easy to parse with scripts
- Compatible with monitoring tools

---

## Examples

### Monitor Job Health

```powershell
# Get last 24 hours of job executions
$yesterday = (Get-Date).AddDays(-1)
Get-Content logs\taskscheduler-*.log | 
  Select-String "execution summary" | 
  Where-Object { 
    $timestamp = [DateTime]::Parse(($_ -split '\[')[0])
    $timestamp -gt $yesterday
  } | 
  ForEach-Object {
    if ($_ -match "Job (\S+) execution summary - Status: (\w+), Duration: ([\d:\.]+), Execution #(\d+)") {
      [PSCustomObject]@{
        JobName = $Matches[1]
        Status = $Matches[2]
        Duration = $Matches[3]
        ExecutionNumber = $Matches[4]
      }
    }
  } | 
  Format-Table -AutoSize
```

### Find Long-Running Jobs

```powershell
# Find jobs that took longer than 10 minutes
Get-Content logs\taskscheduler-*.log | 
  Select-String "Duration: (\d+):(\d+):(\d+)\." | 
  Where-Object { 
    $minutes = [int]$_.Matches.Groups[2].Value
    $minutes -ge 10
  }
```

### Track Failure Patterns

```powershell
# Group failures by error type
Get-Content logs\taskscheduler-*.log | 
  Select-String "Error Type: (\w+)" | 
  ForEach-Object { $_.Matches.Groups[1].Value } | 
  Group-Object | 
  Sort-Object Count -Descending
```

---

## Best Practices

### 1. Regular Log Review
Check logs daily for failures and warnings:
```powershell
Get-Content logs\taskscheduler-*.log | Select-String "\[ERR\]|\[WRN\]"
```

### 2. Archive Old Logs
Keep logs organized:
```powershell
# Archive logs older than 30 days
Get-ChildItem logs\taskscheduler-*.log | 
  Where-Object { $_.LastWriteTime -lt (Get-Date).AddDays(-30) } | 
  Compress-Archive -DestinationPath "logs\archive\logs-$(Get-Date -Format 'yyyy-MM').zip"
```

### 3. Set Up Alerts
Monitor for critical issues:
```powershell
# Check for recent failures
$recent = Get-Content logs\taskscheduler-*.log -Tail 1000 | 
  Select-String "JOB EXECUTION FAILED"

if ($recent.Count -gt 5) {
    Send-MailMessage -To "admin@example.com" -Subject "Task Scheduler Alerts" `
      -Body "$($recent.Count) jobs failed recently"
}
```

### 4. Performance Baselines
Establish normal execution times:
```powershell
# Calculate baseline durations
Get-Content logs\taskscheduler-*.log | 
  Select-String "Job (\w+) execution summary - Status: SUCCESS, Duration: ([\d:\.]+)" | 
  ForEach-Object {
    $jobName = $_.Matches.Groups[1].Value
    $duration = [TimeSpan]::Parse($_.Matches.Groups[2].Value)
    [PSCustomObject]@{ Job = $jobName; Seconds = $duration.TotalSeconds }
  } | 
  Group-Object Job | 
  ForEach-Object {
    [PSCustomObject]@{
      Job = $_.Name
      AvgSeconds = ($_.Group | Measure-Object Seconds -Average).Average
      MaxSeconds = ($_.Group | Measure-Object Seconds -Maximum).Maximum
    }
  }
```

---

## Summary

The enhanced logging system provides:

? **Complete visibility** into every job execution  
? **Unique execution tracking** with IDs and counters  
? **Precise timing** with millisecond accuracy  
? **Formatted output** for easy reading  
? **Status tracking** (SUCCESS, FAILED, TIMEOUT, SKIPPED)  
? **Error details** with full context  
? **Integration ready** for monitoring tools  
? **Easy to parse** with scripts  

---

**Version:** 1.4.0  
**Last Updated:** 2024-01-15  
**Feature Status:** ? Production Ready
