namespace LogParser.Models;

/// <summary>
/// Represents a single log entry parsed from a log file.
/// </summary>
/// <remarks>
/// The given log entry includes properties such as timestamp, log level, message, and any associated metadata and the fields below represent those properties.
/// </remarks>
public class LogEntry
{
    /// <summary>
    /// The IP address that made the request.
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// The timestamp when the request was made.
    /// </summary>
    public string Timestamp { get; set; } = string.Empty;

    /// <summary>
    /// The HTTP method used (GET, POST, etc.).
    /// </summary>
    public string HttpMethod { get; set; } = string.Empty;

    /// <summary>
    /// The URL path that was requested (normalized - trailing slashes removed).
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// The HTTP protocol version.
    /// </summary>
    public string HttpVersion { get; set; } = string.Empty;

    /// <summary>
    /// The HTTP status code returned.
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// The size of the response in bytes.
    /// </summary>
    public int ResponseSize { get; set; }

    /// <summary>
    /// The user agent string (browser/client information).
    /// </summary>
    public string UserAgent { get; set; } = string.Empty;

    /// <summary>
    /// The raw, unparsed log line (useful for debugging).
    /// </summary>
    public string RawLine { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"[{Timestamp}] {IpAddress} {HttpMethod} {Url} -> {StatusCode}";
        // todo: might not need the one below, remove if so towards the end of project if unused/during cleanup
        //return $"{IpAddress} - [{Timestamp}] \"{HttpMethod} {Url} {HttpVersion}\" {StatusCode} {ResponseSize} \"{UserAgent}\"";
    }

}
