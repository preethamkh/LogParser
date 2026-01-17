using LogParser.Analyzers;
using LogParser.Parsers;
using LogParser.Services;

namespace LogParser;

/// <summary>
/// 1. Accepts a log file path as a command-line argument
/// 2. Parses and analyzes the log file
/// 3. Displays the results to the console
/// </summary>
internal class Program
{
    private static void Main(string[] args)
    {
        try
        {
            // Guard Clause - handle invalid input early
            if (args.Length == 0)
            {
                DisplayUsage();
                return;
            }

            var filePath = args[0];

            Console.WriteLine("=== Log Parser ===");
            Console.WriteLine();

            // We would ideally do this via a DI container
            // But for this exercise, manual injection is simpler and more transparent
            var parser = new ApacheLogParser();
            var analyzer = new LogAnalyzer();
            var service = new LogProcessingService(parser, analyzer);

            DisplayFileInfo(service, filePath);

            // Process the log file
            var result = service.ProcessLogFile(filePath, topCount: 3);

            // Display results
            Console.WriteLine(result.FormatResults());

            // 0 = success
            Environment.Exit(0);
        }
        catch (FileNotFoundException ex)
        {
            // Clean Code: Specific exception handling
            Console.Error.WriteLine($"Error: {ex.Message}");
            Console.Error.WriteLine("Please check the file path and try again.");
            Environment.Exit(1);
        }
        catch (ArgumentException ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Environment.Exit(1);
        }
        catch (Exception ex)
        {
            // Catch-all for unexpected errors
            Console.Error.WriteLine($"An unexpected error occurred: {ex.Message}");
            Console.Error.WriteLine($"Stack trace: {ex.StackTrace}");
            Environment.Exit(2);
        }
    }

    /// <summary>
    /// Displays usage instructions to the user.
    /// </summary>
    /// <remarks>
    /// Clean Code: Extract Method
    /// - Keeps Main clean and readable
    /// - Single responsibility: displays help
    /// - Easy to update help text
    /// </remarks>
    private static void DisplayUsage()
    {
        Console.WriteLine("Log Parser - Analyzes Apache/Nginx log files");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  LogParser <path-to-log-file>");
        Console.WriteLine();
        Console.WriteLine("Example:");
        Console.WriteLine("  LogParser ./logs/access.log");
        Console.WriteLine();
        Console.WriteLine("Output:");
        Console.WriteLine("  - Number of unique IP addresses");
        Console.WriteLine("  - Top 3 most visited URLs");
        Console.WriteLine("  - Top 3 most active IP addresses");
    }

    private static void DisplayFileInfo(LogProcessingService service, string filePath)
    {
        Console.WriteLine(service.GetFileInfo(filePath));
        Console.WriteLine();
        Console.WriteLine("Processing log file...");
        Console.WriteLine();
    }
}