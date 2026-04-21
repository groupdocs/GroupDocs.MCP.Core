---
id: 002
date: 2026-04-21
version: 26.4.1
type: fix
---

# Fix repository URL casing in package metadata

## What changed

- `PackageProjectUrl`, `RepositoryUrl`, and `PackageReleaseNotes` in [build/dependencies.props](build/dependencies.props) now use the canonical repo URL `https://github.com/groupdocs/GroupDocs.Mcp.Core` (with dots and proper casing) instead of the hyphenated form.
- Per-package README files (packed into each nupkg) updated with the same URL.

## Why

The hyphenated form `groupdocs/groupdocs-mcp-core` works for `git clone` via GitHub's rename redirect, but **deep links like `/releases/tag/26.4.0` do not reliably redirect** — users clicking the "Project website" link on the 26.4.0 NuGet package page land on a GitHub 404 instead of the release notes.

## Migration / impact

None — API-surface unchanged. The nuget.org "Project website" and "Repository" links on future releases will resolve correctly.
