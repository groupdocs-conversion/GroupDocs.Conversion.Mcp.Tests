using System.Text.Json;
using GroupDocs.Conversion.Mcp.IntegrationTests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace GroupDocs.Conversion.Mcp.IntegrationTests;

[Collection(McpServerCollection.Name)]
public class GetSupportedFormatsTests
{
    private readonly McpServerFixture _fixture;
    private readonly ITestOutputHelper _output;

    public GetSupportedFormatsTests(McpServerFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task GetSupportedFormats_AuthoredPdf_ReturnsTargetList()
    {
        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.SupportedFormats.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.AuthoredPdf },
            });

        Assert.False(response.IsError ?? false,
            $"Tool reported an error: {ToolResponse.Text(response)}");

        // Response can be large (PDFs convert to ~100 formats) and may be truncated mid-JSON.
        // Verify by substring rather than full JsonDocument.Parse.
        var body = ToolResponse.Text(response);
        _output.WriteLine(body);

        Assert.Contains("\"sourceExtension\"", body);
        Assert.Contains("\"targets\"", body);
        Assert.Contains("\"isPrimary\"", body);
        // Primary PDF target conversions should at least include common formats.
        Assert.Contains("docx", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetSupportedFormats_PlainJpeg_ReturnsImageTargets()
    {
        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.SupportedFormats.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.PlainJpeg },
            });

        Assert.False(response.IsError ?? false,
            $"Tool reported an error: {ToolResponse.Text(response)}");

        // JPEG response fits in budget — parse strict JSON.
        var json = ToolResponse.Json(response);
        _output.WriteLine(json.ToString());

        Assert.Equal("jpg", json.GetProperty("sourceExtension").GetString(), ignoreCase: true);
        var targets = json.GetProperty("targets");
        Assert.True(targets.GetArrayLength() > 0, "Expected at least one target conversion for JPEG.");
    }

    public static IEnumerable<object[]> RealSampleData() => new[]
    {
        new object[] { SampleDocuments.SamplePdf  },
        new object[] { SampleDocuments.SampleDocx },
        new object[] { SampleDocuments.SampleXlsx },
        new object[] { SampleDocuments.SamplePng  },
    };

    [Theory]
    [MemberData(nameof(RealSampleData))]
    public async Task GetSupportedFormats_RealSample_AdvertisesNonEmptyTargets(string fileName)
    {
        if (!File.Exists(Path.Combine(_fixture.StoragePath, fileName)))
        {
            _output.WriteLine($"Sample '{fileName}' not present in storage — skipping.");
            return;
        }

        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.SupportedFormats.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = fileName },
            });

        Assert.False(response.IsError ?? false,
            $"Tool reported an error for '{fileName}': {ToolResponse.Text(response)}");

        var body = ToolResponse.Text(response);
        _output.WriteLine(body);

        Assert.Contains("\"targets\"", body);
        // Quick sanity: at least one isPrimary entry — every real format has one.
        Assert.Contains("\"isPrimary\":true", body.Replace(" ", string.Empty));
    }
}
