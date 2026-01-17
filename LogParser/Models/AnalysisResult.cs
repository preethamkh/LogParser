namespace LogParser.Models;

/// <summary>
/// Represents the result of analyzing parsed log entries.
/// </summary>
public class AnalysisResult
{
    public int UniqueIpAddressCount { get; set; }
    public IReadOnlyList<UrlVisit> TopUrls { get; set; } = Array.Empty<UrlVisit>();
    public IReadOnlyList<IpActivity> TopIpAddresses { get; set; } = Array.Empty<IpActivity>();
    public ParseResult ParseMetadata { get; set; } = new();

    /// <summary>
    /// Formats the analysis results for display.
    /// </summary>
    public string FormatResults()
    {
        var result = new System.Text.StringBuilder();

        result.AppendLine("=== Log Analysis Results ===");
        result.AppendLine();
        result.AppendLine($"Unique IP Addresses: {UniqueIpAddressCount}");
        result.AppendLine();

        result.AppendLine("Top 3 Most Visited URLs:");
        for (int i = 0; i < TopUrls.Count; i++)
        {
            var urlVisit = TopUrls[i];
            result.AppendLine($"  {i + 1}. {urlVisit.Url} - {urlVisit.VisitCount} visits");
        }
        result.AppendLine();

        result.AppendLine("Top 3 Most Active IP Addresses:");
        for (int i = 0; i < TopIpAddresses.Count; i++)
        {
            var ipActivity = TopIpAddresses[i];
            result.AppendLine($"  {i + 1}. {ipActivity.IpAddress} - {ipActivity.RequestCount} requests");
        }
        result.AppendLine();

        result.AppendLine($"Parse Summary: {ParseMetadata.GetSummary()}");

        return result.ToString();
    }
}