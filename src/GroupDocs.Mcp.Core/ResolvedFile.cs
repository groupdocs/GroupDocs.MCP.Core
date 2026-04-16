namespace GroupDocs.Mcp.Core;

public class ResolvedFile : IDisposable
{
    public required Stream Stream { get; init; }

    public required string FileName { get; init; }

    public void Dispose() => Stream?.Dispose();
}
