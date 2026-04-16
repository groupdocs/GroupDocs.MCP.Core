using Amazon.S3;

namespace GroupDocs.Mcp.AwsS3.Storage;

public class AwsS3Options
{
    public string? AccessKey { get; set; }
    public string? SecretKey { get; set; }
    public string Region { get; set; } = "us-east-1";
    public string BucketName { get; set; } = string.Empty;
    public AmazonS3Config S3Config { get; set; } = new();
}
