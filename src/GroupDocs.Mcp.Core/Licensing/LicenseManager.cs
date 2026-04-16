using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GroupDocs.Mcp.Core.Licensing;

/// <summary>
/// Resolves the license path from config or environment variable
/// and tracks whether a license is active.
/// Actual license application is product-specific — each server
/// overrides SetLicenseFromPath to call its GroupDocs product's SetLicense.
/// </summary>
public abstract class LicenseManager : ILicenseManager
{
    private readonly McpConfig _config;
    private readonly ILogger<LicenseManager> _logger;
    private bool _initialized;

    public bool IsLicensed { get; private set; }

    protected LicenseManager(IOptions<McpConfig> config, ILogger<LicenseManager> logger)
    {
        _config = config.Value;
        _logger = logger;
    }

    public void SetLicense()
    {
        if (_initialized) return;
        _initialized = true;

        var licensePath = _config.LicensePath
            ?? Environment.GetEnvironmentVariable("GROUPDOCS_LICENSE_PATH");

        if (string.IsNullOrEmpty(licensePath))
        {
            _logger.LogWarning(
                "No license configured. Running in evaluation mode. " +
                "Set GROUPDOCS_LICENSE_PATH or call config.SetLicensePath().");
            return;
        }

        if (!File.Exists(licensePath))
        {
            _logger.LogWarning("License file not found at {LicensePath}. Running in evaluation mode.", licensePath);
            return;
        }

        try
        {
            SetLicenseFromPath(licensePath);
            IsLicensed = true;
            _logger.LogInformation("License applied from {LicensePath}.", licensePath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to apply license from {LicensePath}. Running in evaluation mode.", licensePath);
        }
    }

    /// <summary>
    /// Product-specific license application.
    /// Each server implements this to call its GroupDocs product's SetLicense method.
    /// </summary>
    protected abstract void SetLicenseFromPath(string licensePath);
}
