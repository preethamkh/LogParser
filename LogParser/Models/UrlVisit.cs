namespace LogParser.Models;

/// <summary>
/// Represents a URL and its visit count.
/// </summary>
// todo: consider moving to AnalysisResult.cs if only used there
public class UrlVisit
{
    public string Url { get; set; } = string.Empty;
    public int VisitCount { get; set; }

    public override string ToString() => $"{Url} ({VisitCount} visits)";
}
