# Release Process — GroupDocs.MCP.Core

End-to-end checklist for releasing a new version of the 4 Core NuGet packages.
All 4 packages are released together under one CalVer version.

## Versioning — CalVer `YY.MM.N`

- `YY` — 2-digit year (e.g. `26` = 2026)
- `MM` — month without leading zero (e.g. `4` = April)
- `N` — patch increment starting at `0`; increment for hotfixes within the same month

Example: `26.4.0`, `26.4.1`, `26.5.0`.

## Per-release checklist

### 1. Prepare the changelog entry

Add a new file under [changelog/](changelog/):

```
changelog/NNN-short-slug.md
```

Use the next sequential `NNN` and follow the frontmatter in [changelog/README.md](changelog/README.md).
Set `version: {NEW_VERSION}` in the frontmatter.

### 2. Bump versions in `build/dependencies.props`

Edit [build/dependencies.props](build/dependencies.props) and update **all four** version properties to the new release number:

```xml
<GroupDocsMcpCore>{NEW_VERSION}</GroupDocsMcpCore>
<GroupDocsMcpLocalStorage>{NEW_VERSION}</GroupDocsMcpLocalStorage>
<GroupDocsMcpAwsS3Storage>{NEW_VERSION}</GroupDocsMcpAwsS3Storage>
<GroupDocsMcpAzureBlobStorage>{NEW_VERSION}</GroupDocsMcpAzureBlobStorage>
```

All four must match — they release together.

### 3. (Rarely) bump external dependency versions

Only when needed — update the `External Dependency Versions` block in
[build/dependencies.props](build/dependencies.props):

- `MicrosoftExtensionsDependencyInjectionAbstractions`
- `MicrosoftExtensionsLoggingAbstractions`
- `MicrosoftExtensionsOptions`
- `AzureStorageBlobs`
- `AwsSdkS3`
- `MicrosoftSourceLinkGithub`

### 4. (Rarely) bump tool versions

- `CodeSignTool` version — configured as a GitHub repository variable `CODE_SIGN_TOOL_VERSION` (e.g. `1.3.0`). Bump under **Settings → Secrets and variables → Actions → Variables**, not in the workflow file.

### 5. Verify locally

```powershell
# Fresh build + pack all 4 packages
./build.ps1

# Confirm output
ls build_out/*.nupkg
# Expected: 4 .nupkg + 4 .snupkg
```

Run tests:
```powershell
dotnet test src/GroupDocs.Mcp.Core.sln -c Release
```

### 6. Commit

```bash
git add build/dependencies.props changelog/NNN-*.md
git commit -m "Release {NEW_VERSION}"
git push
```

### 7. Wait for CI green

`build_packages.yml` and `run_tests.yml` must pass on `main` before tagging.

### 8. Tag the release

```bash
git tag {NEW_VERSION}
git push origin {NEW_VERSION}
```

**No `v` prefix.** The tag must match `[0-9]+\.[0-9]+\.[0-9]+`.

### 9. CI takes over

`publish_prod.yml` fires on the tag push and will:

1. Build with `BUILD_TYPE=PROD`
2. Pack all 4 .nupkg + .snupkg
3. Sign with SSL.com eSigner (CodeSignTool)
4. Push to NuGet.org using `NUGET_API_KEY_PROD`
5. Create a GitHub Release with the changelog entry attached

### 10. Post-release verification

- [ ] All four packages appear on nuget.org at the new version
- [ ] NuGet listing shows signed package badge
- [ ] GitHub Release is created and links to the changelog entry
- [ ] Symbol packages (.snupkg) are present on nuget.org symbols

## Required GitHub secrets & variables

**Secrets** (`Settings → Secrets and variables → Actions → Secrets`):

| Secret | Purpose |
|---|---|
| `NUGET_API_KEY_PROD` | NuGet.org API key scoped to the 4 Core package IDs |
| `ES_USERNAME` | SSL.com eSigner username |
| `ES_PASSWORD` | SSL.com eSigner password |
| `ES_TOTP_SECRET` | SSL.com eSigner TOTP 2FA secret |
| `CODE_SIGN_CLIENT_ID` | SSL.com eSigner OAuth CLIENT_ID |

**Variables** (`Settings → Secrets and variables → Actions → Variables`):

| Variable | Default | Purpose |
|---|---|---|
| `CODE_SIGN_TOOL_VERSION` | `1.3.0` | CodeSignTool release tag from github.com/SSLcom/CodeSignTool |

## Yanking a bad release

If a published version has a critical bug:

1. On nuget.org, unlist (don't delete) the bad packages
2. Bump `N` in [build/dependencies.props](build/dependencies.props)
3. Add a `type: fix` changelog entry describing the issue
4. Tag and release the patch

Do not re-upload the same version — nuget.org rejects replays.
