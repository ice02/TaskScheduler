# ? JOB EXECUTION LOGGING - VERSION 1.4.0

## ?? STATUS: IMPLEMENTED AND OPERATIONAL

The detailed per-job execution logging system with statuses is now operational!

---

## ?? What Was Added

### 1. Information Logged for Each Execution

#### At Job Start
```
+-----------------------------------------------------------
¦ JOB EXECUTION STARTED
¦-----------------------------------------------------------
¦ Job Name: BackupDatabase
¦ Execution ID: a3f7b2c9
¦ Execution #: 5
¦ Job Type: PowerShell
¦ Script/Executable: C:\Scripts\backup.ps1
¦ Arguments: -DatabaseName Production -FullBackup
¦ Max Execution Time: 30 minutes
¦ Start Time: 2024-01-15 14:30:45.123
+-----------------------------------------------------------
```

#### At Job End (Success)
```
+-----------------------------------------------------------
¦ JOB EXECUTION COMPLETED
¦-----------------------------------------------------------
¦ Job Name: BackupDatabase
¦ Execution ID: a3f7b2c9
¦ Status: ? SUCCESS
¦ Exit Code: 0
¦ Duration: 00:04:27.333
¦ End Time: 2024-01-15 14:35:12.456
+-----------------------------------------------------------
Job BackupDatabase execution summary - Status: SUCCESS, Duration: 00:04:27.333, Execution #5
```

#### On Failure
```
+-----------------------------------------------------------
¦ JOB EXECUTION FAILED
¦-----------------------------------------------------------
¦ Job Name: DataExport
¦ Execution ID: b8c2d4e1
¦ Status: ? FAILED
¦ Duration: 00:00:05.234
¦ End Time: 2024-01-15 15:00:05.234
¦ Error Type: InvalidOperationException
¦ Error Message: Process exited with code 1. Error output: Cannot access file
¦ Exit Code: -1
+-----------------------------------------------------------
Job DataExport execution summary - Status: FAILED, Duration: 00:00:05.234, Execution #3
```

#### On Timeout
```
+-----------------------------------------------------------
¦ JOB EXECUTION TIMEOUT
¦-----------------------------------------------------------
¦ Job Name: LongRunningTask
¦ Execution ID: c9d3e5f2
¦ Status: ? TIMEOUT
¦ Max Time: 5 minutes
¦ Actual Duration: 00:05:00.123
¦ End Time: 2024-01-15 16:05:00.123
¦ Error: Job exceeded maximum execution time of 5 minutes and was terminated.
+-----------------------------------------------------------
Job LongRunningTask execution summary - Status: TIMEOUT, Duration: 00:05:00.123, Execution #1
```

---

## ?? New Features

### 1. Unique Execution ID
Each execution receives an 8-character unique execution ID:
- Examples: `a3f7b2c9`, `b8c2d4e1`, `c9d3e5f2`
- Enables tracing a specific run in the logs
- Useful for debugging and auditing

### 2. Execution Counter
The service tracks the number of executions per job:
- Starts at 1 when the service starts
- Increments on each run
- Resets when the service restarts
- Visible in logs: `Execution #: 5`

### 3. Four Execution Statuses

| Status | Symbol | Description |
|--------|--------|-------------|
| **SUCCESS** | ? | Job completed successfully (exit code 0) |
| **FAILED** | ? | Job failed with an error |
| **TIMEOUT** | ? | Job exceeded the maximum allowed time |
| **SKIPPED** | ?? | Job skipped because it was already running (overlap) |

### 4. Precise Timing
- Uses `Stopwatch` for exact durations
- Format: `hh:mm:ss.fff` (milliseconds)
- Start and end timestamps: `yyyy-MM-dd HH:mm:ss.fff`

### 5. Formatted Logs
- Visual boxes using characters for easy scanning
- Easy to spot in log files
- Simplifies parsing with scripts

### 6. Summary Line
Each execution ends with a compact summary line:
```
Job BackupDatabase execution summary - Status: SUCCESS, Duration: 00:04:27.333, Execution #5
```
Useful for quick parsing and aggregation.

---

## ?? Captured Information

For each execution the following fields are captured:

| Field | Description | Example |
|-------|-------------|---------|
| Job Name | Name of the job | BackupDatabase |
| Execution ID | 8-char unique ID | a3f7b2c9 |
| Execution # | Sequential run number | 5 |
| Job Type | PowerShell or Executable | PowerShell |
| Path | Full path to script/executable | C:\Scripts\backup.ps1 |
| Arguments | Command-line arguments | -DatabaseName Production |
| Max Time | Configured timeout | 30 minutes |
| Start Time | Start timestamp | 2024-01-15 14:30:45.123 |
| End Time | End timestamp | 2024-01-15 14:35:12.456 |
| Duration | Precise duration | 00:04:27.333 |
| Status | SUCCESS/FAILED/TIMEOUT/SKIPPED | SUCCESS |
| Exit Code | Process exit code | 0 |

For failures additional fields are logged (Error Type, Error Message, Stack Trace at Debug level).

---

## ?? Usage Examples

View all executions today:

```powershell
Get-Content logs\taskscheduler-*.log | Select-String "JOB EXECUTION (STARTED|COMPLETED|FAILED|TIMEOUT)"
```

Count successes vs failures:

```powershell
Get-Content logs\taskscheduler-*.log |
  Select-String "execution summary - Status: (\w+)" |
  ForEach-Object { $_.Matches.Groups[1].Value } |
  Group-Object | Select-Object Count, Name
```

Find slow jobs (> 10 minutes):

```powershell
Get-Content logs\taskscheduler-*.log |
  Select-String "Duration: (\d+):(\d+):(\d+)\." |
  Where-Object { [int]$_.Matches.Groups[2].Value -ge 10 }
```

Trace a specific job:

```powershell
Get-Content logs\taskscheduler-*.log | Select-String "Job Name: BackupDatabase"
```

Monitor in real time:

```powershell
Get-Content logs\taskscheduler-*.log -Tail 50 -Wait
```

Extract statistics:

```powershell
$stats = Get-Content logs\taskscheduler-*.log |
  Select-String "Job (\S+) execution summary - Status: (\w+), Duration: ([\d:\.]+)" |
  ForEach-Object {
    [PSCustomObject]@{
      JobName = $_.Matches.Groups[1].Value
      Status = $_.Matches.Groups[2].Value
      Duration = [TimeSpan]::Parse($_.Matches.Groups[3].Value)
    }
  }

$stats | Group-Object JobName, Status | Select-Object Count, @{N='Job';E={$_.Group[0].JobName}}, @{N='Status';E={$_.Group[0].Status}}
```

---

## ?? Log Levels

| Level | Usage | Examples |
|-------|-------|----------|
| Information | Normal operation, success | JOB STARTED, JOB COMPLETED, Summary |
| Warning | Non-critical issues | Overlap detected, non-zero exit code, process error output |
| Error | Failures and timeouts | JOB FAILED, JOB TIMEOUT |
| Debug | Execution details | Process output, process start, stack traces |

---

## ?? Configuration

To see process output and more details set `Serilog` default level to `Debug` in `appsettings.json`:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug",
      "Override": { "Microsoft": "Warning", "System": "Warning" }
    }
  }
}
```

With `Debug` you will see process stdout/stderr and detailed messages.

---

## ?? Modified Files

| File | Changes |
|------|---------|
| `JobExecutionService.cs` | Detailed logging added |
| `CHANGELOG.md` | Version 1.4.0 documented |
| `JOB-LOGGING.md` | Full documentation (20+ pages) |
| This file | Quick summary |

---

## ?? Use Cases

1. Audit & compliance: full trace per execution with unique IDs and timestamps
2. Troubleshooting: readable formatted logs, error context and stack traces
3. Monitoring: execution counters, statuses, durations and success rates
4. Performance: millisecond-precision durations to spot slow jobs

---

## ? Validation

Build:

```powershell
dotnet build
# Build succeeded
```

Tests:

```powershell
dotnet test
# 82 tests pass
```

---

**Version:** 1.4.0
**Date:** 2024-01-15
**Feature:** ENHANCED LOGGING ENABLED
**Status:** PRODUCTION READY
