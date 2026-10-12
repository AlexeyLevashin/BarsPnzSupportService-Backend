namespace Application.Interfaces.Repositories;

public interface IMinioFileStorageService
{
    public Task UploadAsync(string objectName, string bucket, Stream content, string contentType);
    public Task<byte[]> DownloadAsync(string objectName, string bucket);
    public Task<string> GetPresignedUrlAsync(string storageKey, string bucketName, string? originalFileName);
    public Task DeleteAsync(string objectName, string bucket);
}
