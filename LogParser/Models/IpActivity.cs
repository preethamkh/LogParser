namespace LogParser.Models;

/// <summary>
/// Represents an IP address and its request count.
/// </summary>
// todo: consider moving to AnalysisResult.cs if only used there
public class IpActivity
{
    public string IpAddress { get; set; } = string.Empty;
    public int RequestCount { get; set; }

    public override string ToString() => $"{IpAddress} ({RequestCount} requests)";
}
