namespace GroupDocs.Mcp.Core.Licensing;

public interface ILicenseManager
{
    bool IsLicensed { get; }

    void SetLicense();
}
