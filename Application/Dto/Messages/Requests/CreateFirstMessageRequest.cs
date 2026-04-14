using Microsoft.AspNetCore.Http;

namespace Application.Dto.Messages.Requests;

public class CreateFirstMessageRequest
{
    public string? Text { get; set; }
    public List<IFormFile>? Files { get; set; } = new();
}