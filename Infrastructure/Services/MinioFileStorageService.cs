using Application.Interfaces.Repositories;
using Infrastructure.Common;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace Infrastructure.Services;

public class MinioFileStorageService : IMinioFileStorageService
{
    private readonly IMinioClient _minio;
    private readonly MinioOptions _options;

    public MinioFileStorageService(IMinioClient minio, IOptions<MinioOptions> options)
    {
        _minio = minio;
        _options = options.Value;
    }
    
    public async Task UploadAsync(string objectName, string bucket, Stream content, string contentType)
    {   
        await _minio.PutObjectAsync(
            new PutObjectArgs()
                .WithBucket(bucket)
                .WithObject(objectName)
                .WithStreamData(content)
                .WithObjectSize(content.Length)
                .WithContentType(contentType));
    }

    public async Task<string> GetPresignedUrlAsync(string objectName, string bucket, string? originalFileName)
    {
        int expirySeconds = _options.PresignedUrlExpiryHours * 60 * 60;
        
        var args = new PresignedGetObjectArgs()
                .WithBucket(bucket)
                .WithObject(objectName)
                .WithExpiry(expirySeconds);
        
        if (!string.IsNullOrWhiteSpace(originalFileName))
        {
            var encodedFileName = Uri.EscapeDataString(originalFileName);
        
            args = args.WithHeaders(new Dictionary<string, string>
            {
                { "response-content-disposition", $"attachment; filename*=UTF-8''{encodedFileName}" }
            });
        }

        return await _minio.PresignedGetObjectAsync(args);
    }
    
    public async Task DeleteAsync(string objectName, string bucket)
    {
        await _minio.RemoveObjectAsync(
            new RemoveObjectArgs()
                .WithBucket(bucket)
                .WithObject(objectName));
    }
}