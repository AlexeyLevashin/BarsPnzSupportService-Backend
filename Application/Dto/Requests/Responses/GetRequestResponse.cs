using Application.Dto.Users.Responses;
using Domain.Enums;

namespace Application.Dto.Requests.Responses;

public class GetRequestResponse
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string Theme { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; set; }
    public Guid ClientId { get; set; }
    public string ClientFullName { get; set; }
    public Guid? InstitutionId { get; set; }
    public string? InstitutionName { get; set; }
    public List<GetOperatorResponse> Operators { get; set; } = new();
    public RequestStatus Status { get; set; }
    public Priority Priority { get; set; }
}