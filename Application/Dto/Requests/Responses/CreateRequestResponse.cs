using Domain.Enums;

namespace Application.Dto.Requests.Responses;

public class CreateRequestResponse
{
    public Guid Id { get; set; }  = Guid.CreateVersion7();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public RequestStatus Status { get; set; }
}