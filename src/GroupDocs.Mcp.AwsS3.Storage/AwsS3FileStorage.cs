using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using GroupDocs.Mcp.Core;
using GroupDocs.Mcp.Core.Entities;

namespace GroupDocs.Mcp.AwsS3.Storage;

public class AwsS3FileStorage : IFileStorage
{
    private readonly AwsS3Options _options;

    public AwsS3FileStorage(AwsS3Options options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrEmpty(options.BucketName))
            throw new ArgumentException("BucketName is required.", nameof(options));

        if (string.IsNullOrEmpty(options.Region))
            throw new ArgumentException("Region is required.", nameof(options));

        _options = options;
        _options.S3Config.RegionEndpoint = RegionEndpoint.GetBySystemName(options.Region);
    }

    public async Task<IEnumerable<FileSystemEntry>> ListDirsAndFilesAsync(
        string dirPath, CancellationToken ct = default)
    {
        using var client = CreateClient();
        var entries = new List<FileSystemEntry>();

        var request = new ListObjectsV2Request
        {
            BucketName = _options.BucketName,
            Prefix = string.IsNullOrEmpty(dirPath) ? null : dirPath,
            Delimiter = "/"
        };

        ListObjectsV2Response response;
        do
        {
            response = await client.ListObjectsV2Async(request, ct);

            foreach (var prefix in response.CommonPrefixes)
                entries.Add(FileSystemEntry.Directory(
                    GetObjectName(prefix), prefix, 0L));

            foreach (var obj in response.S3Objects)
                entries.Add(FileSystemEntry.File(
                    GetObjectName(obj.Key), obj.Key, obj.Size ?? 0L));

            request.ContinuationToken = response.NextContinuationToken;
        } while (response.IsTruncated == true);

        return entries;
    }

    public async Task<Stream> ReadFileStreamAsync(
        string filePath, CancellationToken ct = default)
    {
        using var client = CreateClient();
        var request = new GetObjectRequest
        {
            BucketName = _options.BucketName,
            Key = filePath
        };

        var response = await client.GetObjectAsync(request, ct);
        return response.ResponseStream;
    }

    public async Task<string> WriteFileAsync(
        string fileName, byte[] bytes, bool rewrite, CancellationToken ct = default)
    {
        using var client = CreateClient();
        var key = rewrite ? fileName : await GetFreeFileName(client, fileName, ct);

        using var stream = new MemoryStream(bytes);
        var request = new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = key,
            InputStream = stream
        };

        await client.PutObjectAsync(request, ct);
        return key;
    }

    public Task<string?> GetDownloadUrlAsync(
        string filePath, TimeSpan expiry, CancellationToken ct = default)
    {
        using var client = CreateClient();
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _options.BucketName,
            Key = filePath,
            Expires = DateTime.UtcNow.Add(expiry)
        };

        var url = client.GetPreSignedURL(request);
        return Task.FromResult<string?>(url);
    }

    private IAmazonS3 CreateClient()
    {
        var keysProvided = !string.IsNullOrEmpty(_options.AccessKey) &&
                           !string.IsNullOrEmpty(_options.SecretKey);

        return keysProvided
            ? new AmazonS3Client(_options.AccessKey, _options.SecretKey, _options.S3Config)
            : new AmazonS3Client(_options.S3Config);
    }

    private async Task<string> GetFreeFileName(IAmazonS3 client, string filePath, CancellationToken ct)
    {
        try
        {
            await client.GetObjectMetadataAsync(_options.BucketName, filePath, ct);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return filePath; // doesn't exist, name is free
        }

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

            try
            {
                await client.GetObjectMetadataAsync(_options.BucketName, candidate, ct);
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return candidate;
            }
        } while (number < 1000);

        return candidate;
    }

    private static string GetObjectName(string key) =>
        key.TrimEnd('/').Split('/').Last();
}
