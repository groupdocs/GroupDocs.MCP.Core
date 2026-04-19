# GroupDocs.Mcp.AzureBlob.Storage

Azure Blob Storage `IFileStorage` implementation for the GroupDocs MCP Core framework.

Output files are exposed as **time-limited SAS download URLs** so MCP clients can fetch results directly from the blob container without streaming through the server.

## Installation

```bash
dotnet add package GroupDocs.Mcp.Core
dotnet add package GroupDocs.Mcp.AzureBlob.Storage
```

## Usage

### Connection string

```csharp
builder.Services
    .AddGroupDocsMcp()
    .AddAzureBlobStorage(options =>
    {
        options.ConnectionString = "DefaultEndpointsProtocol=https;AccountName=...;AccountKey=...";
        options.ContainerName    = "documents";
    });
```

### Account name + key

```csharp
options.AccountName   = "mystorage";
options.AccountKey    = "...";
options.ContainerName = "documents";
```

### Managed identity / TokenCredential

```csharp
options.AccountName     = "mystorage";
options.TokenCredential = new DefaultAzureCredential();
options.ContainerName   = "documents";
```

> SAS URL generation requires the client to be able to sign — connection string / account key work out of the box; user-delegation SAS over `TokenCredential` requires the managed identity to hold the appropriate RBAC role.

## Behaviour

- Container is created on first use if missing (`CreateIfNotExists`).
- Writes honor a `rewrite` flag; when `false`, colliding blob names get a ` (1)`, ` (2)`… suffix.
- `GetDownloadUrlAsync` returns a SAS URL (read-only) whose expiry is controlled by `McpConfig.DownloadUrlExpiry` (default: 1h). Returns `null` if the client cannot generate SAS.

Source: https://github.com/groupdocs/groupdocs-mcp-core
License: MIT
