# Architecture Documentation

## System Overview

The Task Scheduler Service is a .NET 8 Windows Service application designed for reliable, scheduled execution of PowerShell scripts and executables. The architecture follows clean separation of concerns with distinct layers for scheduling, execution, notification, and logging.

## Architecture Diagram

```
???????????????????????????????????????????????????????????????
?                     Windows Service Host                      ?
?  (Microsoft.Extensions.Hosting.WindowsServices)              ?
???????????????????????????????????????????????????????????????
                         ?
                         ?
???????????????????????????????????????????????????????????????
?                    Program.cs (Entry Point)                  ?
?  - Configuration loading (appsettings.json)                  ?
?  - Serilog initialization                                     ?
?  - Dependency injection setup                                 ?
?  - Service registration                                       ?
???????????????????????????????????????????????????????????????
                         ?
                         ?
???????????????????????????????????????????????????????????????
?              JobSchedulerService (BackgroundService)          ?
?  - Loads job configurations                                   ?
?  - Registers jobs with Coravel scheduler                      ?
?  - Manages service lifecycle                                  ?
???????????????????????????????????????????????????????????????
            ?
            ?
???????????????????????????????????????????????????????????????
?                    Coravel Scheduler                          ?
?  - Cron-based job scheduling                                  ?
?  - Job triggering based on schedule                           ?
???????????????????????????????????????????????????????????????
            ?
            ?
???????????????????????????????????????????????????????????????
?               JobExecutionService (Core Logic)                ?
?  ?????????????????????????????????????????????????????????? ?
?  ? Overlap Detection                                       ? ?
?  ?  - Tracks running jobs                                  ? ?
?  ?  - Prevents concurrent executions                       ? ?
?  ?????????????????????????????????????????????????????????? ?
?  ?????????????????????????????????????????????????????????? ?
?  ? Job Execution                                           ? ?
?  ?  - PowerShell script execution                          ? ?
?  ?  - Executable process execution                         ? ?
?  ?  - Output capture and logging                           ? ?
?  ?????????????????????????????????????????????????????????? ?
?  ?????????????????????????????????????????????????????????? ?
?  ? Timeout Management                                      ? ?
?  ?  - CancellationToken-based timeout                      ? ?
?  ?  - Automatic process termination                        ? ?
?  ?????????????????????????????????????????????????????????? ?
???????????????????????????????????????????????????????????????
            ?                    ?
            ?                    ?
????????????????????????  ?????????????????????????????????????
?   Serilog Logger     ?  ?  EmailNotificationService          ?
?  - File logging      ?  ?  - SMTP email sending              ?
?  - Console logging   ?  ?  - Error notifications             ?
?  - Structured logs   ?  ?  - MailKit integration             ?
????????????????????????  ?????????????????????????????????????
```

## Component Breakdown

### 1. Entry Point (Program.cs)

**Responsibilities:**
- Application initialization
- Configuration loading from appsettings.json
- Serilog logger setup
- Dependency injection container configuration
- Windows Service registration
- Host building and execution

**Key Technologies:**
- Microsoft.Extensions.Hosting
- Microsoft.Extensions.Configuration
- Serilog

**Lifecycle:**
```
Start ? Load Config ? Init Logger ? Register Services ? Build Host ? Run
```

---

### 2. JobSchedulerService (Background Service)

**Type:** `BackgroundService` (long-running background task)

**Responsibilities:**
- Load job configurations from appsettings.json
- Register enabled jobs with Coravel scheduler
- Maintain service lifecycle
- Handle graceful shutdown

**Key Methods:**
- `ExecuteAsync()`: Main execution loop
- `StopAsync()`: Cleanup on shutdown

**Job Registration Flow:**
```
Load Config ? Filter Enabled Jobs ? For Each Job:
  ? Create Schedule ? Register with Coravel ? Apply Cron Expression
```

**Configuration Source:**
```json
"Jobs": [
  {
    "Name": "JobName",
    "Type": "PowerShell|Executable",
    "Path": "...",
    "CronExpression": "...",
    ...
  }
]
```

---

### 3. JobExecutionService (Core Execution Engine)

**Responsibilities:**
- Execute individual jobs
- Prevent job overlaps
- Manage execution timeouts
- Capture and log output
- Handle errors and notifications

**Concurrency Control:**
```csharp
private readonly Dictionary<string, bool> _runningJobs = new();
private readonly object _lock = new();

// Before execution:
lock (_lock) {
    if (_runningJobs[jobName] == true) {
        // Job already running, skip
        return;
    }
    _runningJobs[jobName] = true;
}
```

**Execution Flow:**
```
Check if Running ? Mark as Running ? Execute Job ? 
  ? Success: Log & Complete
  ? Timeout: Kill & Notify
  ? Error: Log & Notify
? Mark as Not Running
```

**PowerShell Execution:**
```
powershell.exe -ExecutionPolicy Bypass -NoProfile -File "script.ps1" [args]
```

**Executable Execution:**
```
executable.exe [args]
```

**Output Handling:**
- Stdout ? Information logs
- Stderr ? Warning logs
- Exit code 0 ? Success
- Exit code ? 0 ? Failure

---

### 4. EmailNotificationService

**Responsibilities:**
- Send email notifications for errors
- SMTP connection management
- Email formatting

**Notification Triggers:**
1. **Job Overlap**: When a job is scheduled but already running
2. **Job Timeout**: When a job exceeds MaxExecutionTimeMinutes
3. **Job Failure**: When a job exits with error or throws exception

**Email Flow:**
```
Error Occurs ? Check if SMTP Enabled ? Build Message ? 
  ? Connect to SMTP ? Authenticate ? Send ? Disconnect ? Log Result
```

**Technologies:**
- MailKit: Modern SMTP library
- MimeKit: Email message construction

---

### 5. Configuration System

**Configuration Hierarchy:**
```
appsettings.json (root)
??? Serilog (logging configuration)
?   ??? MinimumLevel
?   ??? WriteTo (sinks)
??? SmtpSettings (email notifications)
?   ??? Enabled
?   ??? Host, Port, UseSsl
?   ??? Credentials
?   ??? Recipients
??? Jobs (job definitions array)
    ??? JobConfiguration[]
        ??? Name
        ??? Type
        ??? Path
        ??? Arguments
        ??? CronExpression
        ??? MaxExecutionTimeMinutes
        ??? Enabled
```

**Hot Reload:**
- Configuration changes detected automatically
- Service restart required for job schedule changes
- Log settings apply immediately

---

## Data Flow

### Job Execution Flow

```
1. Timer Triggers (Coravel)
   ?
2. JobSchedulerService receives trigger
   ?
3. Creates scope, resolves JobExecutionService
   ?
4. JobExecutionService.ExecuteJobAsync()
   ?
5. Check if job already running
   ?? Yes ? Log warning, send email, exit
   ?? No ? Continue
   ?
6. Mark job as running
   ?
7. Create CancellationToken with timeout
   ?
8. Execute process (PowerShell or Executable)
   ?? Capture stdout ? Log as Information
   ?? Capture stderr ? Log as Warning
   ?? Monitor for completion or timeout
   ?
9. Process completes
   ?? Success (exit 0) ? Log success
   ?? Failure (exit ? 0) ? Log error, send email
   ?? Timeout ? Kill process, log error, send email
   ?
10. Mark job as not running
   ?
11. Cleanup resources
```

### Error Notification Flow

```
Error Detected
   ?
Check if SMTP enabled
   ?? No ? Log only
   ?? Yes ? Continue
   ?
Build email message
   ?? Subject: Job name and error type
   ?? Body: Error details, stack trace
   ?
Attempt to send email
   ?? Success ? Log "Email sent"
   ?? Failure ? Log "Email failed to send"
```

---

## Threading Model

### Service Lifecycle Thread
- **Main Thread**: Runs `JobSchedulerService.ExecuteAsync()`
- **Lifetime**: Entire service lifetime
- **Purpose**: Keep service alive, wait for shutdown signal

### Scheduler Threads (Coravel)
- **Thread Pool**: Managed by Coravel
- **Concurrency**: Multiple jobs can run simultaneously
- **Isolation**: Each job execution in separate scope

### Job Execution Threads
- **Per Job**: One thread per job execution
- **Process Monitoring**: Async/await pattern
- **Timeout**: CancellationToken for timeout enforcement

### Thread Safety
- **_runningJobs Dictionary**: Protected by `lock` statement
- **Logging**: Serilog is thread-safe
- **DI Scopes**: Each job execution in separate scope

---

## Dependency Injection

### Service Registration

```csharp
// Singleton: One instance for entire application lifetime
builder.Services.AddSingleton<SmtpSettings>(smtpSettings);
builder.Services.AddSingleton<EmailNotificationService>();

// Scoped: One instance per job execution
builder.Services.AddScoped<JobExecutionService>();

// Hosted Service: Background service
builder.Services.AddHostedService<JobSchedulerService>();

// Coravel Scheduler
builder.Services.AddScheduler();
```

### Lifetime Scopes

```
Application (Singleton)
??? EmailNotificationService (Singleton)
??? SmtpSettings (Singleton)
??? JobSchedulerService (Hosted Service)
    ??? Job Execution (Scoped)
        ??? JobExecutionService (Scoped)
```

**Why Scoped for JobExecutionService?**
- Isolated state per execution
- Clean resource management
- Prevents state leakage between jobs

---

## Logging Architecture

### Serilog Configuration

```
Serilog Pipeline:
Events ? MinimumLevel Filter ? Enrichers ? Sinks
```

**Sinks:**
1. **File Sink**: Persistent logging to disk
   - Rolling by day
   - 30-day retention
   - Structured format

2. **Console Sink**: Real-time logging (console mode)
   - Colored output
   - Same format as file

**Log Levels:**
- **Debug**: Detailed execution flow
- **Information**: Normal operations, job start/complete
- **Warning**: Non-critical issues, job overlap
- **Error**: Job failures, timeout events
- **Fatal**: Service crashes

**Structured Logging Benefits:**
- Machine-readable
- Easy parsing
- Rich context
- Query-friendly

---

## Security Architecture

### Execution Isolation
```
Service Process (Local System or Custom Account)
??? Job Process (Inherits service account)
    ??? PowerShell.exe
    ??? Custom.exe
```

### Security Boundaries
1. **Service Account**: Controls file system and network access
2. **Process Isolation**: Jobs run in separate processes
3. **No Elevation**: Jobs cannot escalate privileges
4. **Configuration Protection**: Secure appsettings.json permissions

### Recommendations
- Use least-privilege service account
- Restrict appsettings.json file permissions
- Encrypt sensitive configuration values
- Use Windows Credential Manager for passwords
- Enable SMTP over TLS/SSL

---

## Error Handling Strategy

### Layered Error Handling

```
Layer 1: Process Level
?? Exit Code Checking
?? Stderr Capture

Layer 2: Execution Service
?? Try-Catch Blocks
?? Timeout Cancellation
?? Overlap Detection

Layer 3: Service Level
?? Serilog Error Logging
?? Email Notifications

Layer 4: OS Level
?? Windows Service Recovery
?? Event Log Recording
```

### Error Recovery

**Transient Errors:**
- Job execution failures ? Retry on next schedule
- Email send failures ? Logged but not blocking
- Timeout ? Process killed, logged

**Permanent Errors:**
- Configuration errors ? Service won't start
- Missing .NET runtime ? Service won't start
- Invalid cron expression ? Job not scheduled

**Automatic Recovery:**
- Service crash ? Windows restarts service
- Job timeout ? Process killed, next run proceeds
- Job failure ? Next scheduled run proceeds

---

## Performance Considerations

### Resource Management
- **Memory**: Each job process isolated
- **CPU**: Controlled by process priority
- **Disk**: Log rotation prevents growth
- **Network**: SMTP connections pooled

### Scalability Limits
- **Concurrent Jobs**: Limited by system resources
- **Job Count**: Hundreds of jobs supported
- **Log Volume**: Managed by rotation
- **Email Rate**: Limited by SMTP server

### Optimization Tips
- Use appropriate MaxExecutionTimeMinutes
- Schedule jobs to avoid overlap
- Monitor log file growth
- Tune Serilog minimum level
- Archive old logs

---

## Extension Points

### Adding New Features

**New Job Types:**
1. Create new execution method in `JobExecutionService`
2. Update configuration model
3. Extend type switch in `ExecuteJobAsync()`

**New Notification Channels:**
1. Create new notification service
2. Register as singleton
3. Call from `JobExecutionService`

**New Scheduling Options:**
1. Extend `JobConfiguration` model
2. Update `JobSchedulerService` registration
3. Leverage Coravel scheduling options

**Custom Authentication:**
1. Implement authentication provider
2. Inject into `EmailNotificationService`
3. Use in SMTP connection

---

## Testing Strategy

### Unit Testing Targets
- Job execution logic
- Overlap detection
- Timeout handling
- Email notification formatting

### Integration Testing Targets
- Configuration loading
- Serilog integration
- SMTP connectivity
- Windows Service registration

### Manual Testing
- Use console mode
- Test with sample scripts
- Verify email notifications
- Check log output

---

## Monitoring & Observability

### Metrics to Monitor
1. **Service Health**
   - Service status (running/stopped)
   - Memory usage
   - CPU usage

2. **Job Execution**
   - Success rate
   - Execution duration
   - Overlap frequency

3. **System Health**
   - Log file size
   - Disk space
   - Network connectivity (SMTP)

### Log Analysis
```powershell
# Count errors
(Get-Content logs\*.log | Select-String "\[ERR\]").Count

# Job execution times
Get-Content logs\*.log | Select-String "completed successfully"

# Most recent errors
Get-Content logs\*.log | Select-String "\[ERR\]" | Select-Object -Last 10
```

---

## Deployment Architecture

### Single Server
```
Windows Server 2022
??? Task Scheduler Service (One Instance)
??? Scheduled Jobs (Multiple)
??? Log Files (Local Disk)
```

### Multi-Server (Distributed)
```
Server 1                Server 2                Server 3
??? Service Instance 1  ??? Service Instance 2  ??? Service Instance 3
??? Jobs A, B, C       ??? Jobs D, E, F       ??? Jobs G, H, I
??? Local Logs         ??? Local Logs         ??? Local Logs
```

**Note**: Current version does not support centralized coordination. Each instance operates independently.

---

## Future Architecture Considerations

### Potential Enhancements
1. **Centralized Configuration**: Database or Redis
2. **Distributed Coordination**: Leader election, job distribution
3. **Metrics Export**: Prometheus, Application Insights
4. **Web Dashboard**: React/Blazor UI
5. **REST API**: Job management endpoints
6. **Message Queue**: RabbitMQ for job queuing
7. **Container Support**: Docker/Kubernetes deployment

---

This architecture provides a solid foundation for reliable, maintainable scheduled job execution in enterprise Windows environments.
