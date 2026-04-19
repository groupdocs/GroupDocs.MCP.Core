# GroupDocs.Mcp.AwsS3.Storage

Amazon S3 `IFileStorage` implementation for the GroupDocs MCP Core framework.

Output files are exposed as **time-limited presigned download URLs** so MCP clients can fetch results directly from S3 without streaming through the server.

## Installation

```bash
dotnet add package GroupDocs.Mcp.Core
dotnet add package GroupDocs.Mcp.AwsS3.Storage
```

## Usage

```csharp
builder.Services
    .AddGroupDocsMcp()
    .AddAwsS3Storage(options =>
    {
        options.BucketName = "my-documents";
        options.Region     = "us-east-1";
        // AccessKey / SecretKey are optional — omit to use IAM role,
        // environment credentials, or the default AWS credential chain.
    });
```

## Credentials

- **Explicit** — set `AccessKey` + `SecretKey` on `AwsS3Options`.
- **Implicit** — leave them blank; the AWS SDK picks up IAM role / `AWS_*` env vars / profile as usual.

## Behaviour

- Writes honor a `rewrite` flag; when `false`, colliding object keys get a ` (1)`, ` (2)`… suffix (capped at 1000 attempts).
- `GetDownloadUrlAsync` returns a presigned `GET` URL whose expiry is controlled by `McpConfig.DownloadUrlExpiry` (default: 1h).
- `ListDirsAndFilesAsync` uses `/` as delimiter and emulates directory semantics via common prefixes.

Source: https://github.com/groupdocs/groupdocs-mcp-core
License: MIT
