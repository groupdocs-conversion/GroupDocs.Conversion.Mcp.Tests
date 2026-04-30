---
id: 001
date: 2026-04-30
version: 26.5.0
type: feature
---

# Initial integration-tests suite for GroupDocs.Conversion.Mcp

## What changed
- Test repo bootstrapped: launches the published `GroupDocs.Conversion.Mcp@26.5.0` NuGet via `dnx`, wires an MCP stdio client, and exercises every advertised tool.
- Five test classes:
  - `ToolDiscoveryTests` — server info advertises `GroupDocs.Conversion.Mcp`, tool list contains exactly `Convert`, `GetSupportedFormats`, `GetDocumentInfo`, every tool has a description + input schema.
  - `ConvertTests` — `Convert` produces an output file for synthetic PDF→HTML and real samples (DOCX→PDF, DOCX→HTML, PDF→HTML, XLSX→PDF), plus an unsupported-target-format error path.
  - `GetSupportedFormatsTests` — `GetSupportedFormats` returns a non-empty target list for synthetic PDF + JPEG and real samples.
  - `GetDocumentInfoTests` — `GetDocumentInfo` returns the runtime `IDocumentInfo` subtype name plus the source's known author/title.
  - `ErrorHandlingTests` — unknown filename returns a clear error, corrupted file does not crash the server, `password` parameter is accepted without schema rejection.
- Five real samples shipped under `sample-docs/` (sample.docx, .xlsx, .pptx, .pdf, .png) auto-copied to test output and exercised via xUnit theories.
- How-to guides under `how-to/` cover NuGet install, Docker, MCP registry verification, Claude Desktop, VS Code / GitHub Copilot, and running the test suite locally.
- `examples/` ships `claude-desktop.json`, `vscode-mcp.json`, `docker-compose.yml` pinned to `26.5.0`.

## Why
Closes the loop on the published `GroupDocs.Conversion.Mcp` NuGet artifact — every release is exercised end-to-end against live nuget.org so packaging or dnx-shim regressions surface immediately rather than at user install time.

## Migration / impact
First release — no migration required.
