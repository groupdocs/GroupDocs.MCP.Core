using GroupDocs.Mcp.Core;
using GroupDocs.Mcp.Core.Builders;
using GroupDocs.Mcp.Core.Diagnostics;
using GroupDocs.Mcp.AzureBlob.Storage;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection;

public static class GroupDocsMcpAzureBlobExtensions
{
    public static GroupDocsMcpBuilder AddAzureBlobStorage(
        this GroupDocsMcpBuilder builder, Action<AzureBlobOptions> configure)
    {
        var options = new AzureBlobOptions();
        configure(options);

        builder.Services.AddSingleton(new AzureBlobFileStorage(options));
        builder.Services.AddSingleton<IFileStorage>(sp =>
            new LoggingFileStorage(
                sp.GetRequiredService<AzureBlobFileStorage>(),
                sp.GetRequiredService<ILogger<LoggingFileStorage>>()));

        return builder;
    }
}
