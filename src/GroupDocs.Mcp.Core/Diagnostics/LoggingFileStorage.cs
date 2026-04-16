using System.Diagnostics;
using GroupDocs.Mcp.Core.Entities;
using Microsoft.Extensions.Logging;

namespace GroupDocs.Mcp.Core.Diagnostics;

public class LoggingFileStorage : IFileStorage
{
    private readonly IFileStorage _inner;
    private readonly ILogger<LoggingFileStorage> _logger;

    public LoggingFileStorage(IFileStorage inner, ILogger<LoggingFileStorage> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<IEnumerable<FileSystemEntry>> ListDirsAndFilesAsync(
        string dirPath, CancellationToken ct = default)
    {
        using var activity = McpActivitySource.Source.StartActivity("Storage.ListDirsAndFiles");
        activity?.SetTag("storage.dirPath", dirPath);

        _logger.LogDebug("Listing files in {DirPath}", dirPath);
        var result = await _inner.ListDirsAndFilesAsync(dirPath, ct);

        return result;
    }

    public async Task<Stream> ReadFileStreamAsync(
        string filePath, CancellationToken ct = default)
    {
        using var activity = McpActivitySource.Source.StartActivity("Storage.ReadFile");
        activity?.SetTag("storage.filePath", filePath);

        var sw = Stopwatch.StartNew();
        _logger.LogInformation("Reading file {FilePath} from storage", filePath);

        var stream = await _inner.ReadFileStreamAsync(filePath, ct);

        _logger.LogInformation("Read file {FilePath} in {ElapsedMs}ms",
            filePath, sw.ElapsedMilliseconds);
        return stream;
    }

    public async Task<string> WriteFileAsync(
        string fileName, byte[] bytes, bool rewrite, CancellationToken ct = default)
    {
        using var activity = McpActivitySource.Source.StartActivity("Storage.WriteFile");
        activity?.SetTag("storage.fileName", fileName);
        activity?.SetTag("storage.bytes", bytes.Length);

        var sw = Stopwatch.StartNew();
        _logger.LogInformation("Writing file {FileName} ({Bytes} bytes) to storage",
            fileName, bytes.Length);

        var savedPath = await _inner.WriteFileAsync(fileName, bytes, rewrite, ct);

        _logger.LogInformation("Wrote file {FileName} to {SavedPath} in {ElapsedMs}ms",
            fileName, savedPath, sw.ElapsedMilliseconds);
        return savedPath;
    }

    public async Task<string?> GetDownloadUrlAsync(
        string filePath, TimeSpan expiry, CancellationToken ct = default)
    {
        var url = await _inner.GetDownloadUrlAsync(filePath, expiry, ct);
        if (url != null)
            _logger.LogDebug("Generated download URL for {FilePath}, expiry {Expiry}", filePath, expiry);
        return url;
    }
}
