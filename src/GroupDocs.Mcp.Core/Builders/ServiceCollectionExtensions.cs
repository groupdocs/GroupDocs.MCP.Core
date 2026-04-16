using GroupDocs.Mcp.Core;
using GroupDocs.Mcp.Core.Builders;
using GroupDocs.Mcp.Core.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection;

public static class GroupDocsMcpServiceCollectionExtensions
{
    public static GroupDocsMcpBuilder AddGroupDocsMcp(
        this IServiceCollection services, Action<McpConfig>? configure = null)
    {
        services
            .AddOptions<McpConfig>()
            .Configure(config =>
            {
                configure?.Invoke(config);
            });

        // Register logging-wrapped FileResolver.
        // FileResolver depends on IFileStorage, which is registered later by .AddLocalStorage() etc.
        services.AddTransient<FileResolver>();
        services.AddTransient<IFileResolver>(sp =>
        {
            var inner = sp.GetRequiredService<FileResolver>();
            var logger = sp.GetRequiredService<ILogger<LoggingFileResolver>>();
            return new LoggingFileResolver(inner, logger);
        });

        // Register OutputHelper
        services.AddTransient<OutputHelper>();

        return new GroupDocsMcpBuilder(services);
    }
}
