using Azure.Core;
using Azure.Storage.Blobs;

namespace GroupDocs.Mcp.AzureBlob.Storage;

public class AzureBlobOptions
{
    public string? ConnectionString { get; set; }
    public string? AccountName { get; set; }
    public string? AccountKey { get; set; }
    public string ContainerName { get; set; } = string.Empty;
    public TokenCredential? TokenCredential { get; set; }
    public BlobClientOptions? ClientOptions { get; set; }
}
