using GroupDocs.Mcp.Core;
using GroupDocs.Mcp.Core.Builders;
using GroupDocs.Mcp.Core.Diagnostics;
using GroupDocs.Mcp.AwsS3.Storage;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection;

public static class GroupDocsMcpAwsS3Extensions
{
    public static GroupDocsMcpBuilder AddAwsS3Storage(
        this GroupDocsMcpBuilder builder, Action<AwsS3Options> configure)
    {
        var options = new AwsS3Options();
        configure(options);

        builder.Services.AddSingleton(new AwsS3FileStorage(options));
        builder.Services.AddSingleton<IFileStorage>(sp =>
            new LoggingFileStorage(
                sp.GetRequiredService<AwsS3FileStorage>(),
                sp.GetRequiredService<ILogger<LoggingFileStorage>>()));

        return builder;
    }
}
