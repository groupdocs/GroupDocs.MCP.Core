using Microsoft.Extensions.DependencyInjection;

namespace GroupDocs.Mcp.Core.Builders;

public class GroupDocsMcpBuilder
{
    public IServiceCollection Services { get; }

    public GroupDocsMcpBuilder(IServiceCollection services)
    {
        Services = services ?? throw new ArgumentNullException(nameof(services));
    }
}
