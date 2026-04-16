using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using GroupDocs.Mcp.Core;
using GroupDocs.Mcp.Core.Entities;

namespace GroupDocs.Mcp.AzureBlob.Storage;

public class AzureBlobFileStorage : IFileStorage
{
    private readonly AzureBlobOptions _options;
    private BlobContainerClient? _client;
    private readonly object _lock = new();
    private bool _containerEnsured;

    public AzureBlobFileStorage(AzureBlobOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrEmpty(options.ContainerName))
            throw new ArgumentException("ContainerName is required.", nameof(options));

        _options = options;
    }

    public async Task<IEnumerable<FileSystemEntry>> ListDirsAndFilesAsync(
        string dirPath, CancellationToken ct = default)
    {
        var client = GetClient();
        var entries = new List<FileSystemEntry>();

        await foreach (var item in client.GetBlobsByHierarchyAsync(
            traits: BlobTraits.None,
            states: BlobStates.None,
            delimiter: "/",
            prefix: string.IsNullOrEmpty(dirPath) ? null : dirPath,
            cancellationToken: ct))
        {
            if (item.IsPrefix)
            {
                entries.Add(FileSystemEntry.Directory(
                    GetObjectName(item.Prefix), item.Prefix, 0L));
            }
            else if (item.IsBlob)
            {
                entries.Add(FileSystemEntry.File(
                    GetObjectName(item.Blob.Name),
                    item.Blob.Name,
                    item.Blob.Properties.ContentLength ?? 0L));
            }
        }

        return entries;
    }

    public async Task<Stream> ReadFileStreamAsync(
        string filePath, CancellationToken ct = default)
    {
        var client = GetClient();
        var blob = client.GetBlobClient(filePath);
        return await blob.OpenReadAsync(cancellationToken: ct);
    }

    public async Task<string> WriteFileAsync(
        string fileName, byte[] bytes, bool rewrite, CancellationToken ct = default)
    {
        var client = GetClient();
        var targetName = rewrite ? fileName : await GetFreeFileName(client, fileName, ct);
        var blob = client.GetBlobClient(targetName);

        await blob.UploadAsync(new BinaryData(bytes), overwrite: rewrite, cancellationToken: ct);
        return targetName;
    }

    public Task<string?> GetDownloadUrlAsync(
        string filePath, TimeSpan expiry, CancellationToken ct = default)
    {
        var client = GetClient();
        var blob = client.GetBlobClient(filePath);

        if (!blob.CanGenerateSasUri)
            return Task.FromResult<string?>(null);

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _options.ContainerName,
            BlobName = filePath,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.Add(expiry)
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        var uri = blob.GenerateSasUri(sasBuilder);
        return Task.FromResult<string?>(uri.ToString());
    }

    private BlobContainerClient GetClient()
    {
        if (_containerEnsured && _client != null)
            return _client;

        lock (_lock)
        {
            if (_containerEnsured && _client != null)
                return _client;

            if (_options.TokenCredential != null)
            {
                if (string.IsNullOrEmpty(_options.AccountName))
                    throw new ArgumentException("AccountName is required with TokenCredential.");

                var serviceUri = new Uri(
                    $"https://{_options.AccountName}.blob.core.windows.net/{_options.ContainerName}");
                _client = new BlobContainerClient(serviceUri, _options.TokenCredential, _options.ClientOptions);
            }
            else if (!string.IsNullOrEmpty(_options.ConnectionString))
            {
                _client = new BlobContainerClient(
                    _options.ConnectionString, _options.ContainerName, _options.ClientOptions);
            }
            else
            {
                if (string.IsNullOrEmpty(_options.AccountName) || string.IsNullOrEmpty(_options.AccountKey))
                    throw new ArgumentException(
                        "Provide ConnectionString, TokenCredential, or AccountName + AccountKey.");

                var connStr = $"DefaultEndpointsProtocol=https;AccountName={_options.AccountName};AccountKey={_options.AccountKey}";
                _client = new BlobContainerClient(connStr, _options.ContainerName, _options.ClientOptions);
            }

            _client.CreateIfNotExists();
            _containerEnsured = true;
        }

        return _client;
    }

    private async Task<string> GetFreeFileName(
        BlobContainerClient client, string filePath, CancellationToken ct)
    {
        var blob = client.GetBlobClient(filePath);
        if (!await blob.ExistsAsync(ct))
            return filePath;

        var nameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
        var ext = Path.GetExtension(filePath);
        var dir = Path.GetDirectoryName(filePath)?.Replace('\\', '/') ?? string.Empty;
        var number = 1;

        string candidate;
        do
        {
            var newName = $"{nameWithoutExt} ({number}){ext}";
            candidate = string.IsNullOrEmpty(dir) ? newName : $"{dir}/{newName}";
            number++;
        } while (await client.GetBlobClient(candidate).ExistsAsync(ct));

        return candidate;
    }

    private static string GetObjectName(string key) =>
        key.TrimEnd('/').Split('/').Last();
}
