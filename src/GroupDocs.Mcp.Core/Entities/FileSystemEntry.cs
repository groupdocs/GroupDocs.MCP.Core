namespace GroupDocs.Mcp.Core.Entities;

public class FileSystemEntry
{
    public string FileName { get; private set; } = string.Empty;

    public string FilePath { get; private set; } = string.Empty;

    public bool IsDirectory { get; private set; }

    public long Size { get; private set; }

    private FileSystemEntry() { }

    public static FileSystemEntry Directory(string name, string path, long size) =>
        new()
        {
            FileName = name,
            FilePath = path,
            IsDirectory = true,
            Size = size
        };

    public static FileSystemEntry File(string name, string path, long size) =>
        new()
        {
            FileName = name,
            FilePath = path,
            IsDirectory = false,
            Size = size
        };
}
