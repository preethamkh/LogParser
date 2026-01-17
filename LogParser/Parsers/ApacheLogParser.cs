using System.Text.RegularExpressions;
using LogParser.Contracts;
using LogParser.Models;

namespace LogParser.Parsers;

/// <summary>
/// Parse Apache/Nginx combined log format files.
/// </summary>
/// <remarks>
/// Example log line:
/// 177.71.128.21 - - [10/Jul/2018:22:21:28 +0200] "GET /intranet-analytics/ HTTP/1.1" 200 3574 "-" "Mozilla/5.0..."
/// </remarks>
public partial class ApacheLogParser : ILogParser
{
    /// <summary>
    /// Regex pattern for parsing Apache/Nginx combined log format.
    /// Uses named capture groups to extract each component of the log entry.
    /// </summary>

    // source: https://regex101.com/library?orderBy=RELEVANCE&search=apache
    // Used a .NET 7+ regex feature (GeneratedRegex) for performance benefits.
    // The regex is compiled at build time rather than runtime
    // and requires the property to be static partial method.
    [GeneratedRegex(@"^(?<ip>\S+) \S+ \S+ \[(?<timestamp>[^\]]+)\] ""(?<method>\S+) (?<url>\S+) (?<version>[^""]+)"" (?<status>\d{3}) (?<size>\d+|-) ""(?<referrer>[^""]*)"" ""(?<useragent>[^""]*)"".*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase,
        matchTimeoutMilliseconds: 1000)]
    private static partial Regex LogLineRegex();

    public ParseResult ParseFile(string filePath)
    {
        // Validate input and exit early if the file path is invalid
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Log file not found: {filePath}", filePath);
        }

        var successfulEntries = new List<LogEntry>();
        var failedLines = new List<string>();

        try
        {
            foreach (var line in File.ReadLines(filePath))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (TryParseLine(line, out var logEntry))
                {
                    successfulEntries.Add(logEntry!);
                }
                else
                {
                    failedLines.Add(line);
                }
            }
        }
        catch (IOException ex)
        {
            throw new IOException($"Error reading log file: {filePath}", ex);
        }

        return new ParseResult
        {
            SuccessfulEntries = successfulEntries,
            FailedLines = failedLines
        };
    }

    public bool TryParseLine(string line, out LogEntry? logEntry)
    {
        // need to assign out parameter first
        logEntry = null;

        if (string.IsNullOrWhiteSpace(line))
        {
            return false;
        }

        var match = LogLineRegex().Match(line);

        if (!match.Success)
        {
            return false;
        }

        try
        {
            var ipAddress = match.Groups["ip"].Value;
            var timestamp = match.Groups["timestamp"].Value;
            var httpMethod = match.Groups["method"].Value;
            var url = match.Groups["url"].Value;
            var httpVersion = match.Groups["version"].Value;
            var statusStr = match.Groups["status"].Value;
            var size = match.Groups["size"].Value;
            //var referrer = match.Groups["referrer"].Value;
            var userAgent = match.Groups["useragent"].Value;

            // Parse timestamp
            //if (!DateTime.TryParseExact(timestampStr, "dd/MMM/yyyy:HH:mm:ss zzz",
            //    System.Globalization.CultureInfo.InvariantCulture,
            //    System.Globalization.DateTimeStyles.None,
            //    out var timestamp))
            //{
            //    return false;
            //}

            // Parse status code
            if (!int.TryParse(statusStr, out var statusCode))
            {
                return false;
            }

            // Parse size (can be '-' for no size)
            // todo: for context/future, the file size can be particularly large for file downloads or media streaming, hence using "long" here
            //long size = 0;
            //if (responseSize != "-" && !long.TryParse(responseSize, out size))
            //{
            //    return false;
            //}

            var responseSize = size == "-" ? 0 : int.Parse(size);

            // Normalize URL: remove trailing slash (except for root "/")
            var normalizedUrl = NormalizeUrl(url);

            logEntry = new LogEntry
            {
                IpAddress = ipAddress,
                Timestamp = timestamp,
                HttpMethod = httpMethod,
                Url = normalizedUrl,
                HttpVersion = httpVersion,
                StatusCode = statusCode,
                ResponseSize = responseSize,
                //Referrer = referrer,
                UserAgent = userAgent
            };
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Helper to normalize URL by removing trailing slashes (except for root "/").
    /// </summary>
    private static string NormalizeUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url) || url == "/")
        {
            return url;
        }

        return url.TrimEnd('/');
    }
}
