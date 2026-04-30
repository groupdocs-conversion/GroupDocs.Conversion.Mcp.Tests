using GroupDocs.Conversion.Mcp.IntegrationTests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace GroupDocs.Conversion.Mcp.IntegrationTests;

/// GroupDocs.Conversion.Convert produces output in evaluation mode (with
/// watermarks) so happy-path assertions work in both eval and licensed mode.
[Collection(McpServerCollection.Name)]
public class ConvertTests
{
    private readonly McpServerFixture _fixture;
    private readonly ITestOutputHelper _output;

    public ConvertTests(McpServerFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task Convert_AuthoredPdf_ToHtml_ProducesOutputFile()
    {
        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.Convert.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.AuthoredPdf },
                ["format"] = "html",
            });

        Assert.False(response.IsError ?? false,
            $"Tool reported an error: {ToolResponse.Text(response)}");

        var body = ToolResponse.Text(response);
        _output.WriteLine(body);

        var outputPath = Path.Combine(_fixture.StoragePath, "authored.html");
        Assert.True(File.Exists(outputPath),
            $"Expected converted file at '{outputPath}'. Response body:\n{body}");
    }

    public static IEnumerable<object[]> ConvertibleSamples() => new[]
    {
        new object[] { SampleDocuments.SampleDocx, "pdf",  "sample.pdf"  },
        new object[] { SampleDocuments.SampleDocx, "html", "sample.html" },
        new object[] { SampleDocuments.SamplePdf,  "html", "sample.html" },
        new object[] { SampleDocuments.SampleXlsx, "pdf",  "sample.pdf"  },
    };

    [Theory]
    [MemberData(nameof(ConvertibleSamples))]
    public async Task Convert_RealSample_ProducesExpectedOutputFile(string fileName, string targetFormat, string expectedOutputName)
    {
        if (!File.Exists(Path.Combine(_fixture.StoragePath, fileName)))
        {
            _output.WriteLine($"Sample '{fileName}' not present in storage — skipping.");
            return;
        }

        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.Convert.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = fileName },
                ["format"] = targetFormat,
            });

        Assert.False(response.IsError ?? false,
            $"Convert failed for '{fileName}' -> '{targetFormat}': {ToolResponse.Text(response)}");

        var body = ToolResponse.Text(response);
        _output.WriteLine(body);

        var outputPath = Path.Combine(_fixture.StoragePath, expectedOutputName);
        Assert.True(File.Exists(outputPath),
            $"Expected converted file at '{outputPath}'. Response body:\n{body}");

        var size = new FileInfo(outputPath).Length;
        Assert.True(size > 0, $"Converted file '{outputPath}' is empty.");
    }

    [Fact]
    public async Task Convert_UnsupportedTargetFormat_ReturnsClearMessage()
    {
        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.Convert.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.PlainJpeg },
                ["format"] = "xyz",  // not a real format
            });

        var body = ToolResponse.Text(response);
        _output.WriteLine(body);

        // Tool's own error path: returns a string starting with "Format 'xyz' is not supported".
        var isErrorReported = (response.IsError ?? false)
            || body.Contains("not supported", StringComparison.OrdinalIgnoreCase)
            || body.Contains("xyz", StringComparison.OrdinalIgnoreCase);

        Assert.True(isErrorReported,
            $"Expected an unsupported-format error. Response:\n{body}");
    }
}
