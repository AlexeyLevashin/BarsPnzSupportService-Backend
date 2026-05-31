using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Common;

public class MinioOptions
{
    [Required]
    public string Endpoint { get; set; } = string.Empty;
    [Required]
    public string AccessKey { get; set; } = string.Empty;
    [Required]
    public string SecretKey { get; set; } = string.Empty;
    public bool UseSSL { get; set; }
    public int PresignedUrlExpiryHours { get; set; } = 24;
    public string ExternalEndpoint { get; set; }
}