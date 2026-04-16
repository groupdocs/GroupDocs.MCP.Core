using Microsoft.Extensions.Options;

namespace GroupDocs.Mcp.Core;

/// <summary>
/// Helpers for formatting tool output — truncation, file output with URL/path.
/// </summary>
public class OutputHelper
{
    private readonly IFileStorage _storage;
    private readonly McpConfig _config;

    public OutputHelper(IFileStorage storage, IOptions<McpConfig> config)
    {
        _storage = storage;
        _config = config.Value;
    }

    /// <summary>
    /// Truncates text output if it exceeds the configured limit.
    /// </summary>
    public string TruncateText(string text, string? paginationHint = null)
    {
        if (text.Length <= _config.MaxOutputCharacters)
            return text;

        var truncated = text[.._config.MaxOutputCharacters];
        var hint = paginationHint ?? "Use 'page' parameter to extract specific pages.";

        return $"{truncated}\n\n" +
               $"[Output truncated — showing first {_config.MaxOutputCharacters:N0} of {text.Length:N0} characters. {hint}]";
    }

    /// <summary>
    /// Builds the output message for a file that was saved to storage.
    /// Returns a download URL if the storage supports it, otherwise the saved path.
    /// </summary>
    public async Task<string> BuildFileOutputAsync(
        string savedPath, string description, CancellationToken ct = default)
    {
        var url = await _storage.GetDownloadUrlAsync(
            savedPath, _config.DownloadUrlExpiry, ct);

        return url != null
            ? $"{description}\nDownload: {url}"
            : $"{description}\nSaved to: {savedPath}";
    }
}
