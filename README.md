# GroupDocs MCP Core

Core infrastructure and storage providers for [GroupDocs MCP](https://github.com/groupdocs) servers.

## Packages

| Package | Description | NuGet |
|---|---|---|
| `GroupDocs.Mcp.Core` | Core framework: file resolution, licensing, diagnostics, DI builder | [![NuGet](https://img.shields.io/nuget/v/GroupDocs.Mcp.Core.svg)](https://www.nuget.org/packages/GroupDocs.Mcp.Core) |
| `GroupDocs.Mcp.Local.Storage` | Local filesystem storage provider with Docker env-var auto-detection | [![NuGet](https://img.shields.io/nuget/v/GroupDocs.Mcp.Local.Storage.svg)](https://www.nuget.org/packages/GroupDocs.Mcp.Local.Storage) |
| `GroupDocs.Mcp.AwsS3.Storage` | AWS S3 storage provider with presigned download URLs | [![NuGet](https://img.shields.io/nuget/v/GroupDocs.Mcp.AwsS3.Storage.svg)](https://www.nuget.org/packages/GroupDocs.Mcp.AwsS3.Storage) |
| `GroupDocs.Mcp.AzureBlob.Storage` | Azure Blob Storage provider with SAS download URLs | [![NuGet](https://img.shields.io/nuget/v/GroupDocs.Mcp.AzureBlob.Storage.svg)](https://www.nuget.org/packages/GroupDocs.Mcp.AzureBlob.Storage) |

## Installation

Every GroupDocs MCP product server depends on Core and at least one storage provider.

```bash
# Always required
dotnet add package GroupDocs.Mcp.Core
dotnet add package GroupDocs.Mcp.Local.Storage

# Optional — add only what you need
dotnet add package GroupDocs.Mcp.AwsS3.Storage
dotnet add package GroupDocs.Mcp.AzureBlob.Storage
```

## Usage

### Local Storage (default)

```csharp
builder.Services
    .AddGroupDocsMcp(config =>
    {
        config.SetLicensePath("/license/GroupDocs.Total.lic");
    })
    .AddLocalStorage();
```

Environment variables are checked automatically:

| Variable | Description |
|---|---|
| `GROUPDOCS_MCP_STORAGE_PATH` | Base folder for input files |
| `GROUPDOCS_MCP_OUTPUT_PATH` | Folder where output files are written (defaults to storage path) |
| `GROUPDOCS_LICENSE_PATH` | Path to GroupDocs license file |

### AWS S3 Storage

```csharp
builder.Services
    .AddGroupDocsMcp()
    .AddAwsS3Storage(options =>
    {
        options.BucketName = "my-bucket";
        options.Region = "us-east-1";
        // AccessKey/SecretKey optional — uses IAM role if omitted
    });
```

### Azure Blob Storage

```csharp
builder.Services
    .AddGroupDocsMcp()
    .AddAzureBlobStorage(options =>
    {
        options.ConnectionString = "DefaultEndpointsProtocol=https;...";
        options.ContainerName = "documents";
    });
```

## Architecture

```
GroupDocs.Mcp.Core          (abstractions: IFileStorage, IFileResolver, McpConfig, LicenseManager)
    ↑
GroupDocs.Mcp.Local.Storage (implements IFileStorage — filesystem)
GroupDocs.Mcp.AwsS3.Storage (implements IFileStorage — Amazon S3)
GroupDocs.Mcp.AzureBlob.Storage (implements IFileStorage — Azure Blob)
    ↑
GroupDocs.{Product}.Mcp     (product MCP servers — separate repositories)
```

## License

MIT — see [LICENSE](LICENSE)
