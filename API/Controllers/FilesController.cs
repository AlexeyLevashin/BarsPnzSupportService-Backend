using API.Controllers.Abstractions;
using Application.Common.Minio;
using Application.Dto.Attachments.Requests;
using Application.Exceptions.Attachments;
using Application.Exceptions.Files;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace API.Controllers;

[Route("api/files")]
public class FilesController : BaseController
{
    private readonly IMinioFileStorageService _storage;
    private readonly FileUploadOptions _options;
    private readonly IAttachmentService _attachmentService;

    public FilesController(IMinioFileStorageService storage, IOptions<FileUploadOptions> options, IAttachmentService attachmentService)
    {
        _storage = storage;
        _options = options.Value;
        _attachmentService = attachmentService;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new FileIsEmptyException();

        if (!_options.AllowedContentTypes.Contains(file.ContentType))
            throw new FileFormatIsNotAllowed();
        
        
        int maxFileSize = _options.MaxFileSizeMb * 1024 * 1024; 
        if (file.Length > maxFileSize)
            throw new MaxFileSizeException();
        
        var extension = Path.GetExtension(file.FileName);
        var id = Guid.NewGuid();
        var storageKey = $"{id}{extension}";
        
        await using var stream = file.OpenReadStream();

        await _storage.UploadAsync(
            storageKey,
            "attachments",
            stream,
            file.ContentType);

        var request = new AddAttachmentRequest
        {
            Id = id,
            FileName = file.FileName,
            StorageKey = storageKey,
            ContentType = file.ContentType,
            FileSize = file.Length
        };

        await _attachmentService.AddAsync(request);
        
        return Ok(new { fileId = id });
    }

    [HttpGet("{fileId}/url")]
    public async Task<IActionResult> GetDownloadUrl(Guid fileId)
    {
        var fileRecord = await _attachmentService.GetByIdAsync(fileId);
        if (fileRecord is null)
        {
            throw new AttachmentNotFoundException();
        }
        
        string stringKeyForMinio = fileRecord.StorageKey;
        var url = await _storage.GetPresignedUrlAsync(stringKeyForMinio, "attachments", fileRecord.FileName);

        return Ok(new { Url = url });
    }
    
    [HttpDelete("{fileId}")]
    public async Task<IActionResult> Delete(Guid fileId)
    {
        var fileRecord = await _attachmentService.GetByIdAsync(fileId);
        if (fileRecord is null)
        {
            throw new AttachmentNotFoundException();
        }
        
        string stringKeyForMinio = fileRecord.StorageKey;
        await _storage.DeleteAsync(stringKeyForMinio, "attachments");

        await _attachmentService.DeleteAsync(fileId);
        
        return NoContent();
    }
}