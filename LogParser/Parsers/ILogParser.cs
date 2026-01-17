using LogParser.Models;

namespace LogParser.Parsers;

/// <summary>
/// Defines the contract for parsing log files.
/// </summary>
public interface ILogParser
{
    /// <summary>
    /// Parses the specified log file and returns the result.
    /// </summary>
    /// <param name="filePath">The path to the log file.</param>
    /// <returns>A ParseResult containing successfully parsed entries and any lines that failed to parse.</returns>
    ParseResult ParseFile(string filePath);

    /// <summary>
    /// Attempts to parse a single log line.
    /// </summary>
    /// <param name="line">A single line from a log file.</param>
    /// <param name="logEntry">The parsed log entry if successful; null if parsing failed.</param>
    /// <returns>True if the line was successfully parsed; false otherwise.</returns>
    bool TryParseLine(string line, out LogEntry? logEntry);
}
