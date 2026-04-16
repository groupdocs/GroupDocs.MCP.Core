using GroupDocs.Mcp.Core.Entities;
using Microsoft.Extensions.Options;

namespace GroupDocs.Mcp.Core;

public class FileResolver : IFileResolver
{
    private readonly IFileStorage _storage;
    private readonly McpConfig _config;

    public FileResolver(IFileStorage storage, IOptions<McpConfig> config)
    {
        _storage = storage;
        _config = config.Value;
    }

    public async Task<ResolvedFile> ResolveAsync(FileInput input, CancellationToken ct = default)
    {
        if (!string.IsNullOrEmpty(input.FileContent))
            return ResolveFromContent(input);

        if (!string.IsNullOrEmpty(input.FilePath))
            return await ResolveFromStorage(input.FilePath, ct);

        throw new ArgumentException(
            "Provide either filePath (name in storage) or fileContent (base64) + fileName.");
    }

    private ResolvedFile ResolveFromContent(FileInput input)
    {
        if (string.IsNullOrEmpty(input.FileName))
            throw new ArgumentException(
                "fileName is required when using fileContent (e.g. 'report.pdf').");

        var base64Length = input.FileContent!.Length;
        var estimatedBytes = base64Length * 3L / 4;

        if (estimatedBytes > _config.MaxBase64SizeBytes)
        {
            var sizeMb = estimatedBytes / (1024 * 1024);
            var limitMb = _config.MaxBase64SizeBytes / (1024 * 1024);
            throw new ArgumentException(
                $"File content is too large for direct transfer ({sizeMb} MB, limit is {limitMb} MB). " +
                "Save the file to storage and use filePath instead of fileContent.");
        }

        var bytes = Convert.FromBase64String(input.FileContent!);
        return new ResolvedFile
        {
            Stream = new MemoryStream(bytes),
            FileName = input.FileName
        };
    }

    private async Task<ResolvedFile> ResolveFromStorage(string filePath, CancellationToken ct)
    {
        try
        {
            var stream = await _storage.ReadFileStreamAsync(filePath, ct);
            return new ResolvedFile
            {
                Stream = stream,
                FileName = Path.GetFileName(filePath)
            };
        }
        catch (FileNotFoundException)
        {
            var message = await BuildFileNotFoundMessage(filePath, ct);
            throw new FileNotFoundException(message);
        }
        catch (DirectoryNotFoundException)
        {
            var message = await BuildFileNotFoundMessage(filePath, ct);
            throw new FileNotFoundException(message);
        }
    }

    private async Task<string> BuildFileNotFoundMessage(string filePath, CancellationToken ct)
    {
        var dirPath = Path.GetDirectoryName(filePath) ?? string.Empty;
        try
        {
            var entries = await _storage.ListDirsAndFilesAsync(dirPath, ct);
            var files = entries
                .Where(e => !e.IsDirectory)
                .Take(20)
                .ToList();

            if (files.Count == 0)
                return $"File '{filePath}' not found in storage. The directory is empty or does not exist.\n" +
                       "Provide the exact file name, or use fileContent to pass the file directly.";

            var listing = string.Join("\n",
                files.Select(f => $"- {f.FileName} ({FormatSize(f.Size)})"));

            return $"File '{filePath}' not found in storage.\n\nAvailable files:\n{listing}\n\n" +
                   "Provide the exact file name, or use fileContent to pass the file directly.";
        }
        catch
        {
            return $"File '{filePath}' not found in storage.\n" +
                   "Provide the exact file name, or use fileContent to pass the file directly.";
        }
    }

    private static string FormatSize(long bytes)
    {
        return bytes switch
        {
            < 1024 => $"{bytes} B",
            < 1024 * 1024 => $"{bytes / 1024.0:F1} KB",
            _ => $"{bytes / (1024.0 * 1024.0):F1} MB"
        };
    }
}
