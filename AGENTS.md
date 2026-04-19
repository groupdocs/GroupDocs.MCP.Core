# AGENTS.md — Guide for AI coding agents

Brief orientation for AI coding agents (Claude Code, Copilot, Cursor, Aider, Amp, Codex) working in this repository.

## What this repo is

Shared infrastructure for all GroupDocs MCP (Model Context Protocol) servers.
Four NuGet packages are built from this repo — they are **libraries consumed by product MCP repos**, not standalone MCP servers themselves.

| Package | Purpose |
|---|---|
| `GroupDocs.Mcp.Core` | Core abstractions: `IFileStorage`, `IFileResolver`, `ILicenseManager`, `McpConfig`, DI builder |
| `GroupDocs.Mcp.Local.Storage` | Local-filesystem `IFileStorage` implementation (env-var driven) |
| `GroupDocs.Mcp.AwsS3.Storage` | Amazon S3 `IFileStorage` with presigned download URLs |
| `GroupDocs.Mcp.AzureBlob.Storage` | Azure Blob `IFileStorage` with SAS download URLs |

## Folder layout

```
src/                          ← all projects + sln + Directory.Build.props
  GroupDocs.Mcp.Core/
  GroupDocs.Mcp.Local.Storage/
  GroupDocs.Mcp.AwsS3.Storage/
  GroupDocs.Mcp.AzureBlob.Storage/
  GroupDocs.Mcp.Core.Tests/
  GroupDocs.Mcp.Core.sln
  Directory.Build.props
build/
  dependencies.props          ← single source of truth for ALL versions & NuGet metadata
changelog/                    ← one MD file per change, NNN-slug.md format (see changelog/README.md)
.github/workflows/            ← build_packages.yml, run_tests.yml, publish_prod.yml
```

## Commands you can run

```bash
# Restore + build
dotnet restore
dotnet build src/GroupDocs.Mcp.Core.sln -c Release

# Test
dotnet test src/GroupDocs.Mcp.Core.sln -c Release

# Local pack (writes to ./build_out)
pwsh ./build.ps1

# Signed release pack — CI only (requires BUILD_TYPE=PROD and signing secrets)
```

## Version scheme

CalVer `YY.MM.N`:
- `YY` = 2-digit year, `MM` = month (no leading zero), `N` = patch
- Current version lives in [build/dependencies.props](build/dependencies.props) — `GroupDocsMcpCore`, `GroupDocsMcpLocalStorage`, `GroupDocsMcpAwsS3Storage`, `GroupDocsMcpAzureBlobStorage` (all four kept in lockstep — they release together).

**Never hardcode versions in `.csproj` files** — always reference the `$(PropertyName)` from `dependencies.props`.

## House rules

1. **Infrastructure only** — this repo does not define MCP tools. Tool classes live in product repos (`groupdocs-metadata-mcp`, `groupdocs-parser-mcp`, etc.).
2. **No backward-compat shims** between releases — CalVer versions, consumers pin exact.
3. **SourceLink is on** (disabled on non-Windows via `Directory.Build.props` conditional).
4. **Tests use xUnit + Moq** — follow the pattern in [src/GroupDocs.Mcp.Core.Tests/FileResolverTests.cs](src/GroupDocs.Mcp.Core.Tests/FileResolverTests.cs).
5. **Changelog entries required** — any PR that changes behaviour adds a file under `changelog/` (see [changelog/README.md](changelog/README.md)).

## Release flow

See [RELEASE.md](RELEASE.md) for the exact per-release checklist (bump versions, add changelog, tag, etc.).

## What NOT to change

- Do not modify files under `src/*/obj/` or `build_out/` (build artifacts).
- Do not move `build/dependencies.props` or `src/Directory.Build.props` — paths are referenced from multiple workflows and scripts.
- Do not add new public APIs without a matching test.
- Do not change the `McpNetVersions` target framework list without discussion — downstream product repos depend on it.
