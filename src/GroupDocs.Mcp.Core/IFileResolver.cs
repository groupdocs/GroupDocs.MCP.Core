namespace GroupDocs.Mcp.Core;

public interface IFileResolver
{
    Task<ResolvedFile> ResolveAsync(FileInput input, CancellationToken ct = default);
}
