using FluentAssertions;
using LogParser.Analyzers;
using LogParser.Models;

namespace LogParser.Tests.Analyzers;

/// <summary>
/// Unit tests for the LogAnalyzer class.
/// </summary>
public class LogAnalyzerTests
{
    private readonly LogAnalyzer _analyzer = new();
    #region Basic Functionality Tests

    [Fact]
    public void Analyze_WithNullEntries_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => _analyzer.AnalyzeLogs(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Analyze_WithEmptyList_ReturnsZeroUniqueIps()
    {
        // Act
        var result = _analyzer.AnalyzeLogs(new List<LogEntry>());

        // Assert
        result.UniqueIpAddressCount.Should().Be(0);
        result.TopUrls.Should().BeEmpty();
        result.TopIpAddresses.Should().BeEmpty();
    }

    [Fact]
    public void Analyze_WithSingleEntry_ReturnsCorrectCounts()
    {
        // Arrange
        var entries = new List<LogEntry>
        {
            CreateLogEntry("192.168.1.1", "/home")
        };

        // Act
        var result = _analyzer.AnalyzeLogs(entries);

        // Assert
        result.UniqueIpAddressCount.Should().Be(1);
        result.TopUrls.Should().HaveCount(1);
        result.TopUrls[0].Url.Should().Be("/home");
        result.TopUrls[0].VisitCount.Should().Be(1);
        result.TopIpAddresses.Should().HaveCount(1);
        result.TopIpAddresses[0].IpAddress.Should().Be("192.168.1.1");
        result.TopIpAddresses[0].RequestCount.Should().Be(1);
    }

    #endregion

    #region Unique IP Address Tests

    [Fact]
    public void Analyze_WithMultipleSameIp_CountsAsOneUniqueIp()
    {
        // Arrange
        var entries = new List<LogEntry>
        {
            CreateLogEntry("192.168.1.1", "/page1"),
            CreateLogEntry("192.168.1.1", "/page2"),
            CreateLogEntry("192.168.1.1", "/page3")
        };

        // Act
        var result = _analyzer.AnalyzeLogs(entries);

        // Assert
        result.UniqueIpAddressCount.Should().Be(1);
    }

    [Fact]
    public void Analyze_WithDifferentIps_CountsAllUniqueIps()
    {
        // Arrange
        var entries = new List<LogEntry>
        {
            CreateLogEntry("192.168.1.1", "/page1"),
            CreateLogEntry("192.168.1.2", "/page2"),
            CreateLogEntry("192.168.1.3", "/page3"),
            CreateLogEntry("192.168.1.1", "/page4") // Duplicate IP
        };

        // Act
        var result = _analyzer.AnalyzeLogs(entries);

        // Assert
        result.UniqueIpAddressCount.Should().Be(3); // Only unique IPs
    }

    #endregion

    #region Top URLs Tests

    [Fact]
    public void Analyze_WithMultipleUrls_ReturnsTopUrlsByVisitCount()
    {
        // Arrange
        var entries = new List<LogEntry>
        {
            CreateLogEntry("192.168.1.1", "/home"),
            CreateLogEntry("192.168.1.2", "/home"),
            CreateLogEntry("192.168.1.3", "/home"),
            CreateLogEntry("192.168.1.1", "/about"),
            CreateLogEntry("192.168.1.2", "/about"),
            CreateLogEntry("192.168.1.1", "/contact")
        };

        // Act
        var result = _analyzer.AnalyzeLogs(entries);

        // Assert
        result.TopUrls.Should().HaveCount(3);
        result.TopUrls[0].Url.Should().Be("/home");
        result.TopUrls[0].VisitCount.Should().Be(3);
        result.TopUrls[1].Url.Should().Be("/about");
        result.TopUrls[1].VisitCount.Should().Be(2);
        result.TopUrls[2].Url.Should().Be("/contact");
        result.TopUrls[2].VisitCount.Should().Be(1);
    }

    [Fact]
    public void Analyze_WithMoreThanThreeUrls_ReturnsOnlyTopThree()
    {
        // Arrange
        var entries = new List<LogEntry>
        {
            CreateLogEntry("192.168.1.1", "/url1"),
            CreateLogEntry("192.168.1.1", "/url1"),
            CreateLogEntry("192.168.1.1", "/url1"),
            CreateLogEntry("192.168.1.1", "/url1"),
            CreateLogEntry("192.168.1.1", "/url1"), // 5 visits
            CreateLogEntry("192.168.1.1", "/url2"),
            CreateLogEntry("192.168.1.1", "/url2"),
            CreateLogEntry("192.168.1.1", "/url2"),
            CreateLogEntry("192.168.1.1", "/url2"), // 4 visits
            CreateLogEntry("192.168.1.1", "/url3"),
            CreateLogEntry("192.168.1.1", "/url3"),
            CreateLogEntry("192.168.1.1", "/url3"), // 3 visits
            CreateLogEntry("192.168.1.1", "/url4"),
            CreateLogEntry("192.168.1.1", "/url4"), // 2 visits - should not be in top 3
            CreateLogEntry("192.168.1.1", "/url5")  // 1 visit - should not be in top 3
        };

        // Act
        var result = _analyzer.AnalyzeLogs(entries);

        // Assert
        result.TopUrls.Should().HaveCount(3);
        result.TopUrls[0].Url.Should().Be("/url1");
        result.TopUrls[1].Url.Should().Be("/url2");
        result.TopUrls[2].Url.Should().Be("/url3");
    }

    [Fact]
    public void Analyze_WithCustomTopCount_ReturnsCorrectNumber()
    {
        // Arrange
        var entries = new List<LogEntry>
        {
            CreateLogEntry("192.168.1.1", "/url1"),
            CreateLogEntry("192.168.1.1", "/url2"),
            CreateLogEntry("192.168.1.1", "/url3"),
            CreateLogEntry("192.168.1.1", "/url4"),
            CreateLogEntry("192.168.1.1", "/url5")
        };

        // Act
        var result = _analyzer.AnalyzeLogs(entries, topCount: 5);

        // Assert
        result.TopUrls.Should().HaveCount(5);
    }

    [Fact]
    public void Analyze_WithFewerUrlsThanTopCount_ReturnsAllUrls()
    {
        // Arrange
        var entries = new List<LogEntry>
        {
            CreateLogEntry("192.168.1.1", "/url1"),
            CreateLogEntry("192.168.1.1", "/url2")
        };

        // Act
        var result = _analyzer.AnalyzeLogs(entries, topCount: 10);

        // Assert - Should return only 2 URLs, not 10
        result.TopUrls.Should().HaveCount(2);
    }

    [Fact]
    public void Analyze_WithTiedUrlCounts_SortsByUrlAlphabetically()
    {
        // Arrange - All URLs have same count
        var entries = new List<LogEntry>
        {
            CreateLogEntry("192.168.1.1", "/zebra"),
            CreateLogEntry("192.168.1.1", "/alpha"),
            CreateLogEntry("192.168.1.1", "/beta")
        };

        // Act
        var result = _analyzer.AnalyzeLogs(entries);

        // Assert - Should be sorted alphabetically when counts are equal
        result.TopUrls[0].Url.Should().Be("/alpha");
        result.TopUrls[1].Url.Should().Be("/beta");
        result.TopUrls[2].Url.Should().Be("/zebra");
    }

    #endregion

    #region Top IP Addresses Tests

    [Fact]
    public void Analyze_WithMultipleIps_ReturnsTopIpsByRequestCount()
    {
        // Arrange
        var entries = new List<LogEntry>
        {
            CreateLogEntry("192.168.1.1", "/page1"),
            CreateLogEntry("192.168.1.1", "/page2"),
            CreateLogEntry("192.168.1.1", "/page3"),
            CreateLogEntry("192.168.1.2", "/page1"),
            CreateLogEntry("192.168.1.2", "/page2"),
            CreateLogEntry("192.168.1.3", "/page1")
        };

        // Act
        var result = _analyzer.AnalyzeLogs(entries);

        // Assert
        result.TopIpAddresses.Should().HaveCount(3);
        result.TopIpAddresses[0].IpAddress.Should().Be("192.168.1.1");
        result.TopIpAddresses[0].RequestCount.Should().Be(3);
        result.TopIpAddresses[1].IpAddress.Should().Be("192.168.1.2");
        result.TopIpAddresses[1].RequestCount.Should().Be(2);
        result.TopIpAddresses[2].IpAddress.Should().Be("192.168.1.3");
        result.TopIpAddresses[2].RequestCount.Should().Be(1);
    }

    [Fact]
    public void Analyze_WithMoreThanThreeIps_ReturnsOnlyTopThree()
    {
        // Arrange
        var entries = new List<LogEntry>
        {
            CreateLogEntry("192.168.1.1", "/page"), // 4 requests
            CreateLogEntry("192.168.1.1", "/page"),
            CreateLogEntry("192.168.1.1", "/page"),
            CreateLogEntry("192.168.1.1", "/page"),
            CreateLogEntry("192.168.1.2", "/page"), // 3 requests
            CreateLogEntry("192.168.1.2", "/page"),
            CreateLogEntry("192.168.1.2", "/page"),
            CreateLogEntry("192.168.1.3", "/page"), // 2 requests
            CreateLogEntry("192.168.1.3", "/page"),
            CreateLogEntry("192.168.1.4", "/page")  // 1 request - should not be in top 3
        };

        // Act
        var result = _analyzer.AnalyzeLogs(entries);

        // Assert
        result.TopIpAddresses.Should().HaveCount(3);
        result.TopIpAddresses[0].IpAddress.Should().Be("192.168.1.1");
        result.TopIpAddresses[1].IpAddress.Should().Be("192.168.1.2");
        result.TopIpAddresses[2].IpAddress.Should().Be("192.168.1.3");
    }

    [Fact]
    public void Analyze_WithTiedIpCounts_SortsByIpAlphabetically()
    {
        // Arrange - All IPs have same count
        var entries = new List<LogEntry>
        {
            CreateLogEntry("192.168.1.3", "/page"),
            CreateLogEntry("192.168.1.1", "/page"),
            CreateLogEntry("192.168.1.2", "/page")
        };

        // Act
        var result = _analyzer.AnalyzeLogs(entries);

        // Assert - Should be sorted by IP when counts are equal
        result.TopIpAddresses[0].IpAddress.Should().Be("192.168.1.1");
        result.TopIpAddresses[1].IpAddress.Should().Be("192.168.1.2");
        result.TopIpAddresses[2].IpAddress.Should().Be("192.168.1.3");
    }

    #endregion

    #region Invalid Input Tests

    [Fact]
    public void Analyze_WithInvalidTopCount_ThrowsArgumentException()
    {
        // Arrange
        var entries = new List<LogEntry> { CreateLogEntry("192.168.1.1", "/page") };

        // Act
        Action act = () => _analyzer.AnalyzeLogs(entries, topCount: 0);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*must be at least 1*");
    }

    [Fact]
    public void Analyze_WithNegativeTopCount_ThrowsArgumentException()
    {
        // Arrange
        var entries = new List<LogEntry> { CreateLogEntry("192.168.1.1", "/page") };

        // Act
        Action act = () => _analyzer.AnalyzeLogs(entries, topCount: -1);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates a test log entry with specified IP and URL.
    /// </summary>
    /// <remarks>
    /// Clean Code: Test Data Builder pattern
    /// Makes tests more readable by hiding boilerplate
    /// </remarks>
    private static LogEntry CreateLogEntry(string ip, string url)
    {
        return new LogEntry
        {
            IpAddress = ip,
            Url = url,
            Timestamp = "10/Jul/2018:22:21:28 +0200",
            HttpMethod = "GET",
            HttpVersion = "HTTP/1.1",
            StatusCode = 200,
            ResponseSize = 3574,
            UserAgent = "Mozilla/5.0",
            RawLine = $"{ip} - - [10/Jul/2018:22:21:28 +0200] \"GET {url} HTTP/1.1\" 200 3574 \"-\" \"Mozilla/5.0\""
        };
    }

    #endregion

}
