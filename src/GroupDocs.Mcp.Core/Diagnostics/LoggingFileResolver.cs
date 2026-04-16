using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace GroupDocs.Mcp.Core.Diagnostics;

public class LoggingFileResolver : IFileResolver
{
    private readonly IFileResolver _inner;
    private readonly ILogger<LoggingFileResolver> _logger;

    public LoggingFileResolver(IFileResolver inner, ILogger<LoggingFileResolver> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<ResolvedFile> ResolveAsync(FileInput input, CancellationToken ct = default)
    {
        var source = !string.IsNullOrEmpty(input.FileContent) ? "base64" : "storage";
        var name = input.FileName ?? input.FilePath ?? "unknown";

        using var activity = McpActivitySource.Source.StartActivity("ResolveFile");
        activity?.SetTag("file.source", source);
        activity?.SetTag("file.name", name);

        var sw = Stopwatch.StartNew();
        _logger.LogInformation("Resolving file {FileName} from {Source}", name, source);

        var resolved = await _inner.ResolveAsync(input, ct);

        _logger.LogInformation("Resolved file {FileName} from {Source} in {ElapsedMs}ms",
            resolved.FileName, source, sw.ElapsedMilliseconds);
        return resolved;
    }
}
