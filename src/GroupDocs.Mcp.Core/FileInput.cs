using System.ComponentModel;

namespace GroupDocs.Mcp.Core;

public class FileInput
{
    [Description("File path or name in the configured storage")]
    public string? FilePath { get; set; }

    [Description("Base64-encoded file content")]
    public string? FileContent { get; set; }

    [Description("Original filename with extension (required with fileContent)")]
    public string? FileName { get; set; }
}
