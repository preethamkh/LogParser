# Log Parser

A C# application for parsing and analyzing Apache/Nginx Combined Log Format files.

## What It Does

Analyzes log files to answer these questions:
1. The number of unique IP addresses that accessed the server.
2. The top 3 most visited URLs.
3. The top 3 most active IP addresses.

## Quick Start

### Requirements
- .NET 8.0 SDK or later
- Visual Studio 2022, VS Code, or Rider (optional)

### Running the Application

```bash
# Clone and build
git clone https://github.com/YOUR_USERNAME/LogParser.git
cd LogParser
dotnet build

# Run with sample data
dotnet run --project LogParser/LogParser.csproj programming-task-example-data.log
```

### Running Tests

```bash
dotnet test
```

### Example Output
```

Using launch settings from LogParser\Properties\launchSettings.json...
=== Log Parser ===

File: programming-task-example-data.log
Size: 4,864 bytes
Lines: 23
Last Modified: 2026-01-16 19:08:32

Processing log file...

=== Log Analysis Results ===

Unique IP Addresses: 11

Top 3 Most Visited URLs:
  1. /docs/manage-websites - 2 visits
  2. / - 1 visits
  3. /asset.css - 1 visits

Top 3 Most Active IP Addresses:
  1. 168.41.191.40 - 4 requests
  2. 177.71.128.21 - 3 requests
  3. 50.112.00.11 - 3 requests

Parse Summary: Parsed 23 lines: 23 successful (100.0%), 0 failed
```

### Project Structure

```
LogParser/
├── LogParser/                         # Main application
│   ├── Models/                        # Data models (POCOs)
│   │   ├── LogEntry.cs                # Parsed log entry
│   │   ├── ParseResult.cs             # Parsing operation result
│   │   └── AnalysisResult.cs          # Analysis output
│   │   └── IpActivity.cs              # to support Analysis output
│   │   └── UrlVisit.cs                # to support Analysis output
│   ├── Contracts/                     # Parser & Analyzer contracts
│   │   ├── ILogParser.cs              
│   │   ├── ILogAnalyzer.cs            
│   ├── Parsers/                       # Parsing logic
│   │   └── ApacheLogParser.cs         # Apache log format parser
│   ├── Analyzers/                     # Analysis logic
│   │   └── LogAnalyzer.cs             # Log analyzer implementation
│   ├── Services/                      # Orchestration
│   │   └── LogProcessingService.cs    # Coordinates parsing + analysis
│   └── Program.cs                     # Entry point
└── LogParser.Tests/                   # Test project
    ├── Parsers/
    │   └── ApacheLogParserTests.cs
    ├── Analyzers/
    │   └── LogAnalyzerTests.cs
    └── Services/
        └── LogProcessingServiceTests.cs
```

### Design Patterns Used

1. **Dependency Inversion Principle (SOLID)**
   - High-level modules depend on abstractions (`ILogParser`, `ILogAnalyzer`)
   - Easy to swap implementations or add new parsers

2. **Single Responsibility Principle (SOLID)**
   - Each class has one reason to change
   - `ApacheLogParser` only parses
   - `LogAnalyzer` only analyzes
   - `LogProcessingService` only orchestrates

3. **Result Pattern**
   - Methods return rich result objects
   - `ParseResult` contains both successes and failures

4. **Facade Pattern**
   - `LogProcessingService` provides a simple interface to complex subsystem


### Test Coverage

- **Unit Tests**: Test individual components in isolation
- **Integration Tests**: Test the complete workflow end-to-end
- **Edge Case Tests**: Malformed data, empty files, null inputs, etc.

Tests follow the **AAA Pattern** (Arrange, Act, Assert):

## 📄 License

MIT License - see LICENSE file for details