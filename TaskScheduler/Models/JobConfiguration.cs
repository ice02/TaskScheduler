namespace TaskScheduler.Models;

public class JobConfiguration
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "PowerShell" or "Executable"
    public string Path { get; set; } = string.Empty;
    public string Arguments { get; set; } = string.Empty;
    public string CronExpression { get; set; } = string.Empty;
    public int MaxExecutionTimeMinutes { get; set; }
    public bool Enabled { get; set; }
}
