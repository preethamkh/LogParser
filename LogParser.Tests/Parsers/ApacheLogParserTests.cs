using FluentAssertions;
using LogParser.Parsers;

namespace LogParser.Tests.Parsers;

/// <summary>
/// Unit tests for the ApacheLogParser class
/// Test naming pattern: MethodName_Scenario_ExpectedBehavior
/// Makes test failures immediately understandable
/// </summary>
/// <remarks>
/// Testing Strategy:
/// 1. Test happy path (valid log lines)
/// 2. Test edge cases (empty, null, malformed)
/// 3. Test business rules (URL normalization)
/// 4. Test error handling (invalid files)
/// 
/// Clean Code in Tests:
/// - Test names describe what is being tested and expected outcome
/// - AAA Pattern: Arrange, Act, Assert
/// - One assertion per test (mostly - sometimes multiple related assertions)
/// - Use FluentAssertions for readable assertions
/// - Tests serve as documentation
/// </remarks>

// Initialize parser in the constructor for reuse across tests
public class ApacheLogParserTests
{
    private readonly ApacheLogParser _parser = new();

    #region TryParseLine Tests

    [Fact]
    public void TryParseLine_WithValidLogLine_ReturnsTrue()
    {
        // Arrange
        var logLine =
            """
            177.71.128.21 - - [10/Jul/2018:22:21:28 +0200] "GET /intranet-analytics/ HTTP/1.1" 200 3574 "-" "Mozilla/5.0"
            """;

        // Act
        var result = _parser.TryParseLine(logLine, out var logEntry);

        // Assert
        result.Should().BeTrue();
        logEntry.Should().NotBeNull();
    }

    [Fact]
    public void TryParseLine_WithValidLogLine_ExtractsIpAddress()
    {
        // Arrange
        var line = """
                   177.71.128.21 - - [10/Jul/2018:22:21:28 +0200] "GET /intranet-analytics/ HTTP/1.1" 200 3574 "-" "Mozilla/5.0"
                   """;

        // Act
        _parser.TryParseLine(line, out var logEntry);

        // Assert
        logEntry!.IpAddress.Should().Be("177.71.128.21");
    }

    [Fact]
    public void TryParseLine_WithValidLogLine_ExtractsTimestamp()
    {
        // Arrange
        var line = @"177.71.128.21 - - [10/Jul/2018:22:21:28 +0200] ""GET /intranet-analytics/ HTTP/1.1"" 200 3574 ""-"" ""Mozilla/5.0""";

        // Act
        _parser.TryParseLine(line, out var logEntry);

        // Assert
        logEntry!.Timestamp.Should().Be("10/Jul/2018:22:21:28 +0200");
    }

    [Fact]
    public void TryParseLine_WithValidLogLine_ExtractsHttpMethod()
    {
        // Arrange
        var line = @"177.71.128.21 - - [10/Jul/2018:22:21:28 +0200] ""GET /intranet-analytics/ HTTP/1.1"" 200 3574 ""-"" ""Mozilla/5.0""";

        // Act
        _parser.TryParseLine(line, out var logEntry);

        // Assert
        logEntry!.HttpMethod.Should().Be("GET");
    }

    [Fact]
    public void TryParseLine_WithValidLogLine_ExtractsUrl()
    {
        // Arrange
        var line = @"177.71.128.21 - - [10/Jul/2018:22:21:28 +0200] ""GET /intranet-analytics/ HTTP/1.1"" 200 3574 ""-"" ""Mozilla/5.0""";

        // Act
        _parser.TryParseLine(line, out var logEntry);

        // Assert - URL should be normalized (trailing slash removed)
        logEntry!.Url.Should().Be("/intranet-analytics");
    }

    [Fact]
    public void TryParseLine_WithValidLogLine_ExtractsStatusCode()
    {
        // Arrange
        var line = @"177.71.128.21 - - [10/Jul/2018:22:21:28 +0200] ""GET /intranet-analytics/ HTTP/1.1"" 200 3574 ""-"" ""Mozilla/5.0""";

        // Act
        _parser.TryParseLine(line, out var logEntry);

        // Assert
        logEntry!.StatusCode.Should().Be(200);
    }

    [Fact]
    public void TryParseLine_WithValidLogLine_ExtractsResponseSize()
    {
        // Arrange
        var line = @"177.71.128.21 - - [10/Jul/2018:22:21:28 +0200] ""GET /intranet-analytics/ HTTP/1.1"" 200 3574 ""-"" ""Mozilla/5.0""";

        // Act
        _parser.TryParseLine(line, out var logEntry);

        // Assert
        logEntry!.ResponseSize.Should().Be(3574);
    }

    [Fact]
    public void TryParseLine_WithValidLogLine_StoresRawLine()
    {
        // Arrange
        var line = """
                   177.71.128.21 - - [10/Jul/2018:22:21:28 +0200] "GET /intranet-analytics/ HTTP/1.1" 200 3574 "-" "Mozilla/5.0"
                   """;

        // Act
        _parser.TryParseLine(line, out var logEntry);

        // Assert
        logEntry!.RawLine.Should().Be(line);
    }

    #endregion

    #region Edge Cases and Error Handling

    [Fact]
    public void TryParseLine_WithNullLine_ReturnsFalse()
    {
        // Act
        var result = _parser.TryParseLine(null!, out var logEntry);

        // Assert
        result.Should().BeFalse();
        logEntry.Should().BeNull();
    }

    [Fact]
    public void TryParseLine_WithEmptyLine_ReturnsFalse()
    {
        // Act
        var result = _parser.TryParseLine(string.Empty, out var logEntry);

        // Assert
        result.Should().BeFalse();
        logEntry.Should().BeNull();
    }

    [Fact]
    public void TryParseLine_WithWhitespaceLine_ReturnsFalse()
    {
        // Act
        var result = _parser.TryParseLine("   ", out var logEntry);

        // Assert
        result.Should().BeFalse();
        logEntry.Should().BeNull();
    }

    [Fact]
    public void TryParseLine_WithMalformedLine_ReturnsFalse()
    {
        // Arrange - completely invalid format
        var line = "This is not a valid log line";

        // Act
        var result = _parser.TryParseLine(line, out var logEntry);

        // Assert
        result.Should().BeFalse();
        logEntry.Should().BeNull();
    }

    [Fact]
    public void TryParseLine_WithExtraFieldsAtEnd_StillParses()
    {
        // Arrange - has extra fields at the end (from sample data)
        var line = """72.44.32.10 - - [09/Jul/2018:15:48:07 +0200] "GET / HTTP/1.1" 200 3574 "-" "Mozilla/5.0" junk extra""";

        // Act
        var result = _parser.TryParseLine(line, out var logEntry);

        // Assert - should still parse successfully
        result.Should().BeTrue();
        logEntry.Should().NotBeNull();
        logEntry.IpAddress.Should().Be("72.44.32.10");
    }

    #endregion

    #region URL Normalization Tests

    [Fact]
    public void TryParseLine_WithTrailingSlash_RemovesTrailingSlash()
    {
        // Arrange
        var line = @"177.71.128.21 - - [10/Jul/2018:22:21:28 +0200] ""GET /faq/ HTTP/1.1"" 200 3574 ""-"" ""Mozilla/5.0""";

        // Act
        _parser.TryParseLine(line, out var logEntry);

        // Assert
        logEntry!.Url.Should().Be("/faq");
    }

    [Fact]
    public void TryParseLine_WithoutTrailingSlash_LeavesUrlUnchanged()
    {
        // Arrange
        var line = @"177.71.128.21 - - [10/Jul/2018:22:21:28 +0200] ""GET /faq HTTP/1.1"" 200 3574 ""-"" ""Mozilla/5.0""";

        // Act
        _parser.TryParseLine(line, out var logEntry);

        // Assert
        logEntry!.Url.Should().Be("/faq");
    }

    [Fact]
    public void TryParseLine_WithRootUrl_KeepsRootSlash()
    {
        // Arrange
        var line = @"177.71.128.21 - - [10/Jul/2018:22:21:28 +0200] ""GET / HTTP/1.1"" 200 3574 ""-"" ""Mozilla/5.0""";

        // Act
        _parser.TryParseLine(line, out var logEntry);

        // Assert - root "/" should not be removed
        logEntry!.Url.Should().Be("/");
    }

    #endregion

    #region Different HTTP Status Codes

    [Fact]
    public void TryParseLine_With404Status_ParsesCorrectly()
    {
        // Arrange
        var line = @"168.41.191.41 - - [11/Jul/2018:17:41:30 +0200] ""GET /this/page/does/not/exist/ HTTP/1.1"" 404 3574 ""-"" ""Mozilla/5.0""";

        // Act
        _parser.TryParseLine(line, out var logEntry);

        // Assert
        logEntry!.StatusCode.Should().Be(404);
        logEntry.Url.Should().Be("/this/page/does/not/exist");
    }

    [Fact]
    public void TryParseLine_With500Status_ParsesCorrectly()
    {
        // Arrange
        var line = @"72.44.32.11 - - [11/Jul/2018:17:42:07 +0200] ""GET /to-an-error HTTP/1.1"" 500 3574 ""-"" ""Mozilla/5.0""";

        // Act
        _parser.TryParseLine(line, out var logEntry);

        // Assert
        logEntry!.StatusCode.Should().Be(500);
    }

    [Fact]
    public void TryParseLine_With301Redirect_ParsesCorrectly()
    {
        // Arrange
        var line = @"168.41.191.43 - - [11/Jul/2018:17:43:40 +0200] ""GET /moved-permanently HTTP/1.1"" 301 3574 ""-"" ""Mozilla/5.0""";

        // Act
        _parser.TryParseLine(line, out var logEntry);

        // Assert
        logEntry!.StatusCode.Should().Be(301);
    }

    #endregion

    #region ParseFile Tests

    [Fact]
    public void ParseFile_WithNullPath_ThrowsArgumentException()
    {
        // Act
        Action act = () => _parser.ParseFile(null!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be null or empty*");
    }

    [Fact]
    public void ParseFile_WithEmptyPath_ThrowsArgumentException()
    {
        // Act
        Action act = () => _parser.ParseFile(string.Empty);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ParseFile_WithNonExistentFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var nonExistentPath = "this-file-does-not-exist.log";

        // Act
        Action act = () => _parser.ParseFile(nonExistentPath);

        // Assert
        act.Should().Throw<FileNotFoundException>()
            .WithMessage($"*{nonExistentPath}*");
    }

    [Fact]
    public void ParseFile_WithValidFile_ReturnsParseResult()
    {
        // Arrange
        var tempFile = CreateTempLogFile([
            """
            177.71.128.21 - - [10/Jul/2018:22:21:28 +0200] "GET /test HTTP/1.1" 200 3574 "-" "Mozilla/5.0"
            """
        ]);

        try
        {
            // Act
            var result = _parser.ParseFile(tempFile);

            // Assert
            result.Should().NotBeNull();
            result.SuccessfulEntries.Should().HaveCount(1);
            result.FailedLines.Should().BeEmpty();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ParseFile_WithMixedValidAndInvalidLines_ReturnsPartialResults()
    {
        // Arrange
        var tempFile = CreateTempLogFile([
            """
            177.71.128.21 - - [10/Jul/2018:22:21:28 +0200] "GET /test HTTP/1.1" 200 3574 "-" "Mozilla/5.0"
            """,
            "This is invalid",
            """
            168.41.191.40 - - [09/Jul/2018:10:11:30 +0200] "GET /test2 HTTP/1.1" 200 3574 "-" "Mozilla/5.0"
            """
        ]);

        try
        {
            // Act
            var result = _parser.ParseFile(tempFile);

            // Assert
            result.SuccessfulEntries.Should().HaveCount(2);
            result.FailedLines.Should().HaveCount(1);
            result.SuccessRate.Should().BeApproximately(66.67, 0.01);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ParseFile_WithEmptyLines_SkipsEmptyLines()
    {
        // Arrange
        var tempFile = CreateTempLogFile([
            """
            177.71.128.21 - - [10/Jul/2018:22:21:28 +0200] "GET /test HTTP/1.1" 200 3574 "-" "Mozilla/5.0"
            """,
            "",
            "   ",
            """
            168.41.191.40 - - [09/Jul/2018:10:11:30 +0200] "GET /test2 HTTP/1.1" 200 3574 "-" "Mozilla/5.0"
            """
        ]);

        try
        {
            // Act
            var result = _parser.ParseFile(tempFile);

            // Assert
            result.SuccessfulEntries.Should().HaveCount(2);
            result.FailedLines.Should().BeEmpty(); // Empty lines are skipped, not failed
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates a temporary log file for testing.
    /// </summary>
    /// <remarks>
    /// Clean Code: Extract helper methods to keep tests readable.
    /// </remarks>
    private static string CreateTempLogFile(string[] lines)
    {
        var tempPath = Path.GetTempFileName();
        File.WriteAllLines(tempPath, lines);
        return tempPath;
    }

    #endregion
}
