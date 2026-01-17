using LogParser.Models;

namespace LogParser.Contracts;

/// <summary>
/// Defines the contract for analyzing parsed log entries.
/// </summary>
public interface ILogAnalyzer
{
    /// <summary>
    /// Analyzes a collection of log entries and produces summary statistics.
    /// </summary>
    /// <param name="logEntries">The log entries to analyze.</param>
    /// <param name="topCount">Number of top results to return. Default is 3.</param>
    /// <returns>
    /// An AnalysisResult containing unique IP count, top URLs, and top IP addresses.
    /// </returns>
    AnalysisResult AnalyzeLogs(IEnumerable<LogEntry> logEntries, int topCount = 3);
}
