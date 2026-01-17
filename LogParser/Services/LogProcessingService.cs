using LogParser.Contracts;
using LogParser.Models;

namespace LogParser.Services;

public class LogProcessingService
{
    private readonly ILogParser _parser;
    private readonly ILogAnalyzer _analyzer;

    /// <summary>
    /// Creates a new log processing service with the specified parser and analyzer.
    /// </summary>
    /// <remarks>
    /// Clean Code: Constructor Injection
    /// - Dependencies are explicit and required
    /// - Makes testing easy (inject mocks)
    /// - Follows Dependency Inversion Principle
    /// </remarks>
    public LogProcessingService(ILogParser parser, ILogAnalyzer analyzer)
    {
        _parser = parser;
        _analyzer = analyzer;
    }

    public AnalysisResult ProcessLogFile(string filePath, int topCount = 3)
    {
        // Parse the log file
        var parseResult = _parser.ParseFile(filePath);

        // Analyze the parsed entries
        var analysisResult = _analyzer.AnalyzeLogs(parseResult.SuccessfulEntries, topCount);

        analysisResult.ParseMetadata = parseResult;
        return analysisResult;
    }

    /// <summary>
    /// Gets basic information about a log file.
    /// </summary>
    public string GetFileInfo(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return $"File not found: {filePath}";
        }

        var fileInfo = new FileInfo(filePath);
        var lineCount = File.ReadLines(filePath).Count();

        return $"File: {fileInfo.Name}\n" +
               $"Size: {fileInfo.Length:N0} bytes\n" +
               $"Lines: {lineCount:N0}\n" +
               $"Last Modified: {fileInfo.LastWriteTime:yyyy-MM-dd HH:mm:ss}";
    }
}
