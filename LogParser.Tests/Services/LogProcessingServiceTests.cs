using FluentAssertions;
using LogParser.Analyzers;
using LogParser.Parsers;
using LogParser.Services;

namespace LogParser.Tests.Services;

/// <summary>
/// Tests for the LogProcessingService class.
/// These are integration tests that verify the entire workflow.
/// </summary>
public class LogProcessingServiceTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullParser_ThrowsArgumentNullException()
    {
        var analyzer = new LogAnalyzer();

        // Act
        var exception = Record.Exception(() => new LogProcessingService(null!, analyzer));

        // Assert
        exception.Should().BeOfType<ArgumentNullException>()
            .Which.ParamName.Should().Be("parser");
    }

    [Fact]
    public void Constructor_WithNullAnalyzer_ThrowsArgumentNullException()
    {
        var parser = new ApacheLogParser();

        // Act
        var exception = Record.Exception(() => new LogProcessingService(parser, null!));

        // Assert
        exception.Should().BeOfType<ArgumentNullException>()
            .Which.ParamName.Should().Be("analyzer");
    }


    #endregion

    #region ProcessLogFile Integration Tests

    [Fact]
    public void ProcessLogFile_WithValidFile_ReturnsCompleteAnalysis()
    {
        // Arrange
        var service = CreateService();
        var tempFile = CreateSampleLogFile();

        try
        {
            // Act
            var result = service.ProcessLogFile(tempFile);

            // Assert
            result.Should().NotBeNull();
            result.UniqueIpAddressCount.Should().BeGreaterThan(0);
            result.TopUrls.Should().NotBeEmpty();
            result.TopIpAddresses.Should().NotBeEmpty();
            result.ParseMetadata.Should().NotBeNull();
            result.ParseMetadata.HasSuccessfulEntries.Should().BeTrue();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ProcessLogFile_WithSampleData_ReturnsCorrectUniqueIpCount()
    {
        // Arrange
        var service = CreateService();
        var tempFile = CreateSampleLogFile();

        try
        {
            // Act
            var result = service.ProcessLogFile(tempFile);

            // Assert - The sample file has 4 unique IPs
            result.UniqueIpAddressCount.Should().Be(4);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ProcessLogFile_WithSampleData_ReturnsTop3Urls()
    {
        // Arrange
        var service = CreateService();
        var tempFile = CreateSampleLogFile();

        try
        {
            // Act
            var result = service.ProcessLogFile(tempFile);

            // Assert
            result.TopUrls.Should().HaveCount(3);

            // /page1 appears 5 times
            result.TopUrls[0].Url.Should().Be("/page1");
            result.TopUrls[0].VisitCount.Should().Be(5);

            // /page2 appears 3 times
            result.TopUrls[1].Url.Should().Be("/page2");
            result.TopUrls[1].VisitCount.Should().Be(3);

            // /page3 appears 2 times
            result.TopUrls[2].Url.Should().Be("/page3");
            result.TopUrls[2].VisitCount.Should().Be(2);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ProcessLogFile_WithSampleData_ReturnsTop3IpAddresses()
    {
        // Arrange
        var service = CreateService();
        var tempFile = CreateSampleLogFile();

        try
        {
            // Act
            var result = service.ProcessLogFile(tempFile);

            // Assert
            result.TopIpAddresses.Should().HaveCount(3);

            // 192.168.1.1 makes 5 requests
            result.TopIpAddresses[0].IpAddress.Should().Be("192.168.1.1");
            result.TopIpAddresses[0].RequestCount.Should().Be(5);

            // 192.168.1.2 makes 3 requests
            result.TopIpAddresses[1].IpAddress.Should().Be("192.168.1.2");
            result.TopIpAddresses[1].RequestCount.Should().Be(3);

            // 192.168.1.3 makes 2 requests
            result.TopIpAddresses[2].IpAddress.Should().Be("192.168.1.3");
            result.TopIpAddresses[2].RequestCount.Should().Be(2);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ProcessLogFile_WithMalformedLines_IncludesParseMetadata()
    {
        // Arrange
        var service = CreateService();
        var tempFile = CreateFileWithMalformedLines();

        try
        {
            // Act
            var result = service.ProcessLogFile(tempFile);

            // Assert
            result.ParseMetadata.Should().NotBeNull();
            result.ParseMetadata.HasFailures.Should().BeTrue();
            result.ParseMetadata.FailureCount.Should().Be(2);
            result.ParseMetadata.SuccessCount.Should().Be(3);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ProcessLogFile_WithCustomTopCount_ReturnsCorrectCount()
    {
        // Arrange
        var service = CreateService();
        var tempFile = CreateSampleLogFile();

        try
        {
            // Act
            var result = service.ProcessLogFile(tempFile, topCount: 2);

            // Assert
            result.TopUrls.Should().HaveCount(2);
            result.TopIpAddresses.Should().HaveCount(2);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    #endregion

    #region GetFileInfo Tests

    [Fact]
    public void GetFileInfo_WithValidFile_ReturnsFileInformation()
    {
        // Arrange
        var service = CreateService();
        var tempFile = CreateSampleLogFile();

        try
        {
            // Act
            var info = service.GetFileInfo(tempFile);

            // Assert
            info.Should().Contain("File:");
            info.Should().Contain("Size:");
            info.Should().Contain("Lines:");
            info.Should().Contain("Last Modified:");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void GetFileInfo_WithNonExistentFile_ReturnsErrorMessage()
    {
        // Arrange
        var service = CreateService();
        const string nonExistentFile = "this-file-does-not-exist.log";

        // Act
        var info = service.GetFileInfo(nonExistentFile);

        // Assert
        info.Should().Contain("File not found");
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates a LogProcessingService with real dependencies.
    /// </summary>
    /// <remarks>
    /// In a more complex application, we might use mocks here.
    /// But since we're testing integration, we use real implementations.
    /// </remarks>
    private static LogProcessingService CreateService()
    {
        var parser = new ApacheLogParser();
        var analyzer = new LogAnalyzer();
        return new LogProcessingService(parser, analyzer);
    }

    /// <summary>
    /// Creates a sample log file for testing.
    /// </summary>
    /// <remarks>
    /// This creates a realistic dataset that mimics the actual log format.
    /// It includes:
    /// - Multiple IPs with varying request counts
    /// - Multiple URLs with varying visit counts
    /// - Different HTTP status codes
    /// </remarks>
    private static string CreateSampleLogFile()
    {
        var lines = new[]
        {
            """
            192.168.1.1 - - [10/Jul/2018:22:21:28 +0200] "GET /page1 HTTP/1.1" 200 3574 "-" "Mozilla/5.0"
            """,
            """
            192.168.1.1 - - [10/Jul/2018:22:22:28 +0200] "GET /page1 HTTP/1.1" 200 3574 "-" "Mozilla/5.0"
            """,
            """
            192.168.1.1 - - [10/Jul/2018:22:23:28 +0200] "GET /page2 HTTP/1.1" 200 3574 "-" "Mozilla/5.0"
            """,
            """
            192.168.1.1 - - [10/Jul/2018:22:24:28 +0200] "GET /page3 HTTP/1.1" 200 3574 "-" "Mozilla/5.0"
            """,
            """
            192.168.1.1 - - [10/Jul/2018:22:25:28 +0200] "GET /page4 HTTP/1.1" 200 3574 "-" "Mozilla/5.0"
            """,
            """
            192.168.1.2 - - [10/Jul/2018:22:21:28 +0200] "GET /page1 HTTP/1.1" 200 3574 "-" "Mozilla/5.0"
            """,
            """
            192.168.1.2 - - [10/Jul/2018:22:22:28 +0200] "GET /page2 HTTP/1.1" 200 3574 "-" "Mozilla/5.0"
            """,
            """
            192.168.1.2 - - [10/Jul/2018:22:23:28 +0200] "GET /page3 HTTP/1.1" 200 3574 "-" "Mozilla/5.0"
            """,
            """
            192.168.1.3 - - [10/Jul/2018:22:21:28 +0200] "GET /page1 HTTP/1.1" 200 3574 "-" "Mozilla/5.0"
            """,
            """
            192.168.1.3 - - [10/Jul/2018:22:22:28 +0200] "GET /page2 HTTP/1.1" 200 3574 "-" "Mozilla/5.0"
            """,
            """
            192.168.1.4 - - [10/Jul/2018:22:21:28 +0200] "GET /page1 HTTP/1.1" 404 3574 "-" "Mozilla/5.0"
            """
        };

        var tempPath = Path.GetTempFileName();
        File.WriteAllLines(tempPath, lines);
        return tempPath;
    }

    /// <summary>
    /// Creates a log file with some malformed lines.
    /// </summary>
    private static string CreateFileWithMalformedLines()
    {
        var lines = new[]
        {
            """
            192.168.1.1 - - [10/Jul/2018:22:21:28 +0200] "GET /page1 HTTP/1.1" 200 3574 "-" "Mozilla/5.0"
            """,
            "This is a malformed line",
            """
            192.168.1.2 - - [10/Jul/2018:22:22:28 +0200] "GET /page2 HTTP/1.1" 200 3574 "-" "Mozilla/5.0"
            """,
            "Another malformed line",
            """
            192.168.1.3 - - [10/Jul/2018:22:23:28 +0200] "GET /page3 HTTP/1.1" 200 3574 "-" "Mozilla/5.0"
            """
        };

        var tempPath = Path.GetTempFileName();
        File.WriteAllLines(tempPath, lines);
        return tempPath;
    }

    #endregion
}
