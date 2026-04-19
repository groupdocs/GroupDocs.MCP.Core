using GroupDocs.Mcp.Core;
using GroupDocs.Mcp.Core.Entities;

namespace GroupDocs.Mcp.Local.Storage;

public class LocalFileStorage : IFileStorage
{
    // Windows and macOS (default HFS+/APFS) are case-insensitive; Linux is case-sensitive.
    private static readonly StringComparison PathComparison =
        OperatingSystem.IsWindows() || OperatingSystem.IsMacOS()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

    private readonly string _storagePath;
    private readonly string _storagePathWithSeparator;
    private readonly string _outputPath;

    public LocalFileStorage(string storagePath, string? outputPath = null)
    {
        _storagePath = Path.GetFullPath(storagePath);
        _outputPath = Path.GetFullPath(outputPath ?? storagePath);

        _storagePathWithSeparator = _storagePath.EndsWith(Path.DirectorySeparatorChar)
            ? _storagePath
            : _storagePath + Path.DirectorySeparatorChar;

        if (!Directory.Exists(_storagePath))
            Directory.CreateDirectory(_storagePath);

        if (_outputPath != _storagePath && !Directory.Exists(_outputPath))
            Directory.CreateDirectory(_outputPath);
    }

    public Task<IEnumerable<FileSystemEntry>> ListDirsAndFilesAsync(
        string dirPath, CancellationToken ct = default)
    {
        var fullPath = string.IsNullOrEmpty(dirPath)
            ? _storagePath
            : Path.Combine(_storagePath, dirPath);

        if (!Directory.Exists(fullPath))
            return Task.FromResult(Enumerable.Empty<FileSystemEntry>());

        var dirs = Directory.GetDirectories(fullPath)
            .Select(d => new DirectoryInfo(d))
            .Where(d => !d.Attributes.HasFlag(FileAttributes.Hidden))
            .OrderBy(d => d.Name)
            .Select(d => FileSystemEntry.Directory(
                d.Name,
                Path.GetRelativePath(_storagePath, d.FullName),
                0L));

        var files = Directory.GetFiles(fullPath)
            .Select(f => new FileInfo(f))
            .Where(f => !f.Attributes.HasFlag(FileAttributes.Hidden))
            .OrderBy(f => f.Name)
            .Select(f => FileSystemEntry.File(
                f.Name,
                Path.GetRelativePath(_storagePath, f.FullName),
                f.Length));

        IEnumerable<FileSystemEntry> result = dirs.Concat(files);
        return Task.FromResult(result);
    }

    public Task<Stream> ReadFileStreamAsync(
        string filePath, CancellationToken ct = default)
    {
        var fullPath = ResolvePath(filePath);
        ValidatePath(fullPath);

        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"File not found: {filePath}", filePath);

        Stream stream = File.Open(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult(stream);
    }

    public async Task<string> WriteFileAsync(
        string fileName, byte[] bytes, bool rewrite, CancellationToken ct = default)
    {
        var targetName = rewrite ? fileName : GetFreeFileName(fileName);
        var fullPath = Path.Combine(_outputPath, targetName);

        var directory = Path.GetDirectoryName(fullPath);
        if (directory != null && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        var fileMode = rewrite ? FileMode.Create : FileMode.CreateNew;
        await using var fs = File.Open(fullPath, fileMode, FileAccess.Write, FileShare.None);
        await fs.WriteAsync(bytes, ct);

        // Return absolute path so the user knows exactly where the file is
        return fullPath;
    }

    public Task<string?> GetDownloadUrlAsync(
        string filePath, TimeSpan expiry, CancellationToken ct = default)
    {
        // Local storage doesn't support URL generation
        return Task.FromResult<string?>(null);
    }

    private string ResolvePath(string filePath)
    {
        return Path.GetFullPath(Path.Combine(_storagePath, filePath));
    }

    private void ValidatePath(string fullPath)
    {
        // Use trailing-separator boundary so /a/b doesn't prefix-match /a/bc.
        // Case sensitivity follows host OS conventions.
        if (!string.Equals(fullPath, _storagePath, PathComparison) &&
            !fullPath.StartsWith(_storagePathWithSeparator, PathComparison))
        {
            throw new UnauthorizedAccessException(
                "Access denied — file is outside the allowed storage directory.");
        }
    }

    private string GetFreeFileName(string fileName)
    {
        var fullPath = Path.Combine(_outputPath, fileName);
        if (!File.Exists(fullPath))
            return fileName;

        var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
        var ext = Path.GetExtension(fileName);
        var dir = Path.GetDirectoryName(fileName) ?? string.Empty;
        var number = 1;

        string candidate;
        do
        {
            var newName = $"{nameWithoutExt} ({number}){ext}";
            candidate = string.IsNullOrEmpty(dir) ? newName : Path.Combine(dir, newName);
            number++;
        } while (File.Exists(Path.Combine(_outputPath, candidate)));

        return candidate;
    }
}
