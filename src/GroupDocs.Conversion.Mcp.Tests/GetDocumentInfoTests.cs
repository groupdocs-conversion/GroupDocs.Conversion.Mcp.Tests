using GroupDocs.Conversion.Mcp.IntegrationTests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace GroupDocs.Conversion.Mcp.IntegrationTests;

[Collection(McpServerCollection.Name)]
public class GetDocumentInfoTests
{
    private readonly McpServerFixture _fixture;
    private readonly ITestOutputHelper _output;

    public GetDocumentInfoTests(McpServerFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task GetDocumentInfo_AuthoredPdf_ReturnsInfoTypeAndProperties()
    {
        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.DocumentInfo.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.AuthoredPdf },
            });

        Assert.False(response.IsError ?? false,
            $"Tool reported an error: {ToolResponse.Text(response)}");

        // PdfDocumentInfo is the runtime subtype — verify it surfaces alongside common props.
        // PDF responses can exceed the budget — use substring checks.
        var body = ToolResponse.Text(response);
        _output.WriteLine(body);

        Assert.Contains("\"infoType\"", body);
        Assert.Contains("PdfDocumentInfo", body, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("\"info\"", body);
    }

    [Fact]
    public async Task GetDocumentInfo_AuthoredPdf_SurfacesKnownAuthorOrTitle()
    {
        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.DocumentInfo.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = SampleDocuments.AuthoredPdf },
            });

        var body = ToolResponse.Text(response);

        Assert.True(
            body.Contains(SampleDocuments.KnownAuthor, StringComparison.Ordinal) ||
            body.Contains(SampleDocuments.KnownTitle, StringComparison.Ordinal),
            $"Expected to find '{SampleDocuments.KnownAuthor}' or '{SampleDocuments.KnownTitle}' in response:\n{body}");
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
    public async Task GetDocumentInfo_RealSample_ReturnsInfoType(string fileName)
    {
        if (!File.Exists(Path.Combine(_fixture.StoragePath, fileName)))
        {
            _output.WriteLine($"Sample '{fileName}' not present in storage — skipping.");
            return;
        }

        var catalog = await ToolCatalog.LoadAsync(_fixture.Client);

        var response = await _fixture.Client.CallToolAsync(
            catalog.DocumentInfo.Name,
            new Dictionary<string, object?>
            {
                ["file"] = new Dictionary<string, object?> { ["filePath"] = fileName },
            });

        Assert.False(response.IsError ?? false,
            $"Tool reported an error for '{fileName}': {ToolResponse.Text(response)}");

        var body = ToolResponse.Text(response);
        _output.WriteLine(body);

        Assert.Contains("\"infoType\"", body);
        Assert.Contains("DocumentInfo", body);  // every concrete subtype name ends in DocumentInfo
    }
}
