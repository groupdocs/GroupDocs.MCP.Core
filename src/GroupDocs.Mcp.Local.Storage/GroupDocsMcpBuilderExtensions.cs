using GroupDocs.Mcp.Core;
using GroupDocs.Mcp.Core.Builders;
using GroupDocs.Mcp.Core.Diagnostics;
using GroupDocs.Mcp.Local.Storage;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection;

public static class GroupDocsMcpLocalStorageExtensions
{
    /// <summary>
    /// Adds local filesystem storage. Path resolution:
    /// 1. GROUPDOCS_MCP_STORAGE_PATH environment variable
    /// 2. Explicit storagePath parameter (if provided)
    /// 3. Current directory fallback
    ///
    /// Output path resolution (where written files are saved):
    /// 1. GROUPDOCS_MCP_OUTPUT_PATH environment variable
    /// 2. Falls back to the resolved storage path
    /// </summary>
    public static GroupDocsMcpBuilder AddLocalStorage(
        this GroupDocsMcpBuilder builder, string? storagePath = null)
    {
        var path = Environment.GetEnvironmentVariable("GROUPDOCS_MCP_STORAGE_PATH")
            ?? storagePath
            ?? Directory.GetCurrentDirectory();

        var outputPath = Environment.GetEnvironmentVariable("GROUPDOCS_MCP_OUTPUT_PATH")
            ?? path;

        builder.Services.AddSingleton(new LocalFileStorage(path, outputPath));
        builder.Services.AddSingleton<IFileStorage>(sp =>
            new LoggingFileStorage(
                sp.GetRequiredService<LocalFileStorage>(),
                sp.GetRequiredService<ILogger<LoggingFileStorage>>()));

        return builder;
    }
}
