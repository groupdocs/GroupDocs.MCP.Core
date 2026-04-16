using GroupDocs.Mcp.Core.Entities;

namespace GroupDocs.Mcp.Core;

public interface IFileStorage
{
    Task<IEnumerable<FileSystemEntry>> ListDirsAndFilesAsync(
        string dirPath, CancellationToken ct = default);

    Task<Stream> ReadFileStreamAsync(
        string filePath, CancellationToken ct = default);

    Task<string> WriteFileAsync(
        string fileName, byte[] bytes, bool rewrite, CancellationToken ct = default);

    /// <summary>
    /// Generates a temporary download URL for the file.
    /// Returns null if the storage provider doesn't support URL generation.
    /// </summary>
    Task<string?> GetDownloadUrlAsync(
        string filePath, TimeSpan expiry, CancellationToken ct = default);
}
