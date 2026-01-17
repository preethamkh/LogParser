using LogParser.Contracts;
using LogParser.Models;

namespace LogParser.Parsers;

public class LogAnalyzer : ILogAnalyzer
{
    public AnalysisResult AnalyzeLogs(IEnumerable<LogEntry> logEntries, int topCount = 3)
    {
        // Clean Code: Fail Fast - validate inputs
        ArgumentNullException.ThrowIfNull(logEntries);

        if (topCount < 1)
        {
            throw new ArgumentException("Top count must be at least 1.", nameof(topCount));
        }

        // Convert to list to avoid multiple enumeration
        // LINQ operations enumerate each time - this prevents re-reading data
        var entries = logEntries.ToList();

        // Delegate to focused methods (Single Level of Abstraction)
        var uniqueIpCount = CountUniqueIpAddresses(entries);
        var topUrls = GetTopUrls(entries, topCount);
        var topIps = GetTopIpAddresses(entries, topCount);

        return new AnalysisResult
        {
            UniqueIpAddressCount = uniqueIpCount,
            TopUrls = topUrls,
            TopIpAddresses = topIps
        };
    }

    /// <summary>
    /// Counts the number of unique IP addresses in the log entries.
    /// </summary>
    /// <remarks>
    /// Clean Code: Intention-Revealing Name
    /// - Method name clearly states what it does
    /// - No need for comments explaining the logic
    /// </remarks>
    private static int CountUniqueIpAddresses(IReadOnlyList<LogEntry> entries)
    {
        var uniqueIps = new HashSet<string>();

        foreach (var entry in entries)
        {
            uniqueIps.Add(entry.IpAddress);
        }

        return uniqueIps.Count;
    }

    /// <summary>
    /// Gets the top N most visited URLs from the log entries.
    /// </summary>
    /// <remarks>
    /// Algorithm:
    /// 1. Group entries by URL (normalized)
    /// 2. Count occurrences of each URL
    /// 3. Sort by count descending
    /// 4. Take top N
    /// </remarks>
    private static IReadOnlyList<UrlVisit> GetTopUrls(
        IReadOnlyList<LogEntry> entries,
        int topCount)
    {
        // Count URL occurrences
        var urlCounts = new Dictionary<string, int>();

        foreach (var entry in entries)
        {
            // URLs are already normalized in the parser
            var url = entry.Url;

            if (urlCounts.ContainsKey(url))
            {
                urlCounts[url]++;
            }
            else
            {
                urlCounts[url] = 1;
            }
        }

        // Get top N using LINQ 
        // OrderByDescending is stable sort - maintains insertion order for ties
        return urlCounts
            .OrderByDescending(kvp => kvp.Value)  // Sort by count
            .ThenBy(kvp => kvp.Key)               // Then by URL (predictable/deterministic)
            .Take(topCount)                       // Take top N
            .Select(kvp => new UrlVisit           // Project to result type
            {
                Url = kvp.Key,
                VisitCount = kvp.Value
            })
            .ToList();
    }

    /// <summary>
    /// Gets the top N most active IP addresses from the log entries.
    /// </summary>
    /// <remarks>
    /// Same algorithm as GetTopUrls - we could extract a generic helper
    /// </remarks>
    private static IReadOnlyList<IpActivity> GetTopIpAddresses(
        IReadOnlyList<LogEntry> entries,
        int topCount)
    {
        var ipCounts = new Dictionary<string, int>();

        foreach (var entry in entries)
        {
            var ip = entry.IpAddress;

            if (ipCounts.ContainsKey(ip))
            {
                ipCounts[ip]++;
            }
            else
            {
                ipCounts[ip] = 1;
            }
        }

        // Get top N
        return ipCounts
            .OrderByDescending(kvp => kvp.Value)
            .ThenBy(kvp => kvp.Key)
            .Take(topCount)
            .Select(kvp => new IpActivity
            {
                IpAddress = kvp.Key,
                RequestCount = kvp.Value
            })
            .ToList();
    }
}
