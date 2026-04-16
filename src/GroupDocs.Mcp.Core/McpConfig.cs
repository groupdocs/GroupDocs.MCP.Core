namespace GroupDocs.Mcp.Core;

public class McpConfig
{
    /// <summary>
    /// Path to the GroupDocs license file.
    /// Also checked via GROUPDOCS_LICENSE_PATH environment variable.
    /// </summary>
    public string? LicensePath { get; set; }

    /// <summary>
    /// Maximum characters to return in text output before truncation.
    /// Default: 5000.
    /// </summary>
    public int MaxOutputCharacters { get; set; } = 5000;

    /// <summary>
    /// Maximum size in bytes for base64-encoded file content.
    /// Default: 10 MB.
    /// </summary>
    public long MaxBase64SizeBytes { get; set; } = 10 * 1024 * 1024;

    /// <summary>
    /// Default expiry for download URLs.
    /// Default: 1 hour.
    /// </summary>
    public TimeSpan DownloadUrlExpiry { get; set; } = TimeSpan.FromHours(1);

    public void SetLicensePath(string path) => LicensePath = path;
}
