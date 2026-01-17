namespace LogParser.Models;

/// <summary>
/// Represents the result of parsing a log file.
/// </summary>
public class ParseResult
{
    /// <summary>
    /// Successfully parsed log entries.
    /// </summary>
    public IReadOnlyList<LogEntry> SuccessfulEntries { get; set; } = Array.Empty<LogEntry>();

    /// <summary>
    /// Lines that failed to parse.
    /// </summary>
    public IReadOnlyList<string> FailedLines { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Total number of lines processed.
    /// </summary>
    public int TotalLinesProcessed => SuccessfulEntries.Count + FailedLines.Count;

    /// <summary>
    /// Number of successfully parsed entries.
    /// </summary>
    public int SuccessCount => SuccessfulEntries.Count;

    /// <summary>
    /// Number of lines that failed to parse.
    /// </summary>
    public int FailureCount => FailedLines.Count;

    /// <summary>
    /// Percentage of successfully parsed lines.
    /// </summary>
    public double SuccessRate => TotalLinesProcessed == 0
        ? 0
        : (double)SuccessCount / TotalLinesProcessed * 100;

    // Helper properties for quick checks
    public bool HasSuccessfulEntries => SuccessfulEntries.Count > 0;
    public bool HasFailures => FailedLines.Count > 0;

    public string GetSummary()
    {
        return $"Parsed {TotalLinesProcessed} lines: " +
               $"{SuccessCount} successful ({SuccessRate:F1}%), " +
               $"{FailureCount} failed";
    }

    public override string ToString() => GetSummary();
}
