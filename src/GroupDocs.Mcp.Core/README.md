# GroupDocs.Mcp.Core

Core abstractions for building MCP (Model Context Protocol) servers that expose GroupDocs document-processing APIs as AI-callable tools.

This package is a **library** — it does not start an MCP server by itself. It provides the shared infrastructure consumed by every GroupDocs product MCP server (Metadata, Parser, Viewer, Conversion, …).

## What's inside

- `IFileStorage` — pluggable storage abstraction (Local, S3, Azure Blob implementations live in sibling packages).
- `IFileResolver` / `FileResolver` — resolves `filePath` (from storage) or `fileContent` (base64) into a stream.
- `McpConfig` — runtime configuration: license path, size limits, URL expiry.
- `LicenseManager` (abstract) — base class for per-product GroupDocs license application.
- `OutputHelper` — text truncation and file-output formatting for tool results.
- `LoggingFileResolver` / `LoggingFileStorage` — decorator wrappers with `ILogger` + `ActivitySource` telemetry.

## Installation

```bash
dotnet add package GroupDocs.Mcp.Core
dotnet add package GroupDocs.Mcp.Local.Storage     # or AwsS3.Storage / AzureBlob.Storage
```

## Minimal wiring

```csharp
builder.Services
    .AddGroupDocsMcp(config =>
    {
        config.LicensePath = "/license/GroupDocs.Total.lic";
    })
    .AddLocalStorage();
```

## Related packages

| Package | Role |
|---|---|
| `GroupDocs.Mcp.Local.Storage` | Filesystem `IFileStorage` |
| `GroupDocs.Mcp.AwsS3.Storage` | Amazon S3 `IFileStorage` with presigned URLs |
| `GroupDocs.Mcp.AzureBlob.Storage` | Azure Blob `IFileStorage` with SAS URLs |

Source: https://github.com/groupdocs/GroupDocs.Mcp.Core
License: MIT
