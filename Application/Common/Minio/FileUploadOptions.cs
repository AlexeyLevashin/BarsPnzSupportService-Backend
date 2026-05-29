namespace Application.Common.Minio;

public class FileUploadOptions
{
    public int MaxFileSizeMb { get; set; } = 200;
    public string[] AllowedContentTypes { get; set; } = Array.Empty<string>();
}