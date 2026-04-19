# GroupDocs.Mcp.Local.Storage

Local-filesystem `IFileStorage` implementation for the GroupDocs MCP Core framework.

Path-sandboxed: all reads/writes are constrained to the configured storage directory. Picks up Docker-friendly environment variables automatically.

## Installation

```bash
dotnet add package GroupDocs.Mcp.Core
dotnet add package GroupDocs.Mcp.Local.Storage
```

## Usage

```csharp
builder.Services
    .AddGroupDocsMcp()
    .AddLocalStorage("/var/data/groupdocs");
```

Path resolution order:

1. `GROUPDOCS_MCP_STORAGE_PATH` — environment variable (wins if set)
2. Explicit `storagePath` argument
3. `Directory.GetCurrentDirectory()` fallback

Output directory (written files):

1. `GROUPDOCS_MCP_OUTPUT_PATH` — environment variable
2. Falls back to the resolved storage path

## Behaviour

- Input paths resolving outside the storage root throw `UnauthorizedAccessException`.
- Writes honor a `rewrite` flag; when `false`, colliding file names get a ` (1)`, ` (2)`… suffix.
- `GetDownloadUrlAsync` returns `null` — local storage has no URL scheme. Callers fall back to the saved path.

Source: https://github.com/groupdocs/groupdocs-mcp-core
License: MIT
