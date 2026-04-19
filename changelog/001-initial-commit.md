---
id: 001
date: 2026-04-18
version: 26.4.0
type: feature
---

# Initial public release of GroupDocs MCP Core

## What changed
- Four NuGet packages published together at version `26.4.0`:
  - `GroupDocs.Mcp.Core` — file resolver, licensing, diagnostics, DI builder, core abstractions
  - `GroupDocs.Mcp.Local.Storage` — local filesystem `IFileStorage` implementation
  - `GroupDocs.Mcp.AwsS3.Storage` — Amazon S3 `IFileStorage` with presigned download URLs
  - `GroupDocs.Mcp.AzureBlob.Storage` — Azure Blob `IFileStorage` with SAS download URLs
- Multi-target: `net8.0` and `net10.0`
- Environment variables read automatically by `AddLocalStorage()`:
  `GROUPDOCS_MCP_STORAGE_PATH`, `GROUPDOCS_MCP_OUTPUT_PATH` (optional), `GROUPDOCS_LICENSE_PATH`
- CalVer versioning `YY.MM.N` — all four packages share one version.

## Why
Shared infrastructure layer consumed by every GroupDocs product MCP server
(Metadata, Parser, Viewer, Conversion, etc.). Publishing Core first unblocks the
13 downstream product repos.

## Migration / impact
First release — no migration required.
