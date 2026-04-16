using GroupDocs.Mcp.Core;
using GroupDocs.Mcp.Core.Entities;
using Microsoft.Extensions.Options;
using Moq;

namespace GroupDocs.Mcp.Core.Tests;

public class FileResolverTests
{
    private readonly Mock<IFileStorage> _storageMock = new();

    private FileResolver CreateResolver(McpConfig? config = null)
    {
        var options = Options.Create(config ?? new McpConfig());
        return new FileResolver(_storageMock.Object, options);
    }

    [Fact]
    public async Task ResolveAsync_WithFileContent_ReturnsMemoryStream()
    {
        var resolver = CreateResolver();
        var bytes = "hello"u8.ToArray();
        var base64 = Convert.ToBase64String(bytes);

        var result = await resolver.ResolveAsync(new FileInput
        {
            FileContent = base64,
            FileName = "test.txt"
        });

        Assert.Equal("test.txt", result.FileName);
        Assert.IsType<MemoryStream>(result.Stream);
    }

    [Fact]
    public async Task ResolveAsync_WithFileContent_MissingFileName_Throws()
    {
        var resolver = CreateResolver();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            resolver.ResolveAsync(new FileInput
            {
                FileContent = Convert.ToBase64String("data"u8.ToArray())
            }));
    }

    [Fact]
    public async Task ResolveAsync_WithFilePath_ReturnsStorageStream()
    {
        var stream = new MemoryStream("content"u8.ToArray());
        _storageMock
            .Setup(s => s.ReadFileStreamAsync("report.pdf", It.IsAny<CancellationToken>()))
            .ReturnsAsync(stream);

        var resolver = CreateResolver();

        var result = await resolver.ResolveAsync(new FileInput { FilePath = "report.pdf" });

        Assert.Equal("report.pdf", result.FileName);
    }

    [Fact]
    public async Task ResolveAsync_NoInput_Throws()
    {
        var resolver = CreateResolver();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            resolver.ResolveAsync(new FileInput()));
    }

    [Fact]
    public async Task ResolveAsync_FileNotFound_IncludesAvailableFiles()
    {
        _storageMock
            .Setup(s => s.ReadFileStreamAsync("missing.pdf", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FileNotFoundException());

        _storageMock
            .Setup(s => s.ListDirsAndFilesAsync("", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                FileSystemEntry.File("existing.pdf", "existing.pdf", 1024)
            });

        var resolver = CreateResolver();

        var ex = await Assert.ThrowsAsync<FileNotFoundException>(() =>
            resolver.ResolveAsync(new FileInput { FilePath = "missing.pdf" }));

        Assert.Contains("existing.pdf", ex.Message);
    }
}
