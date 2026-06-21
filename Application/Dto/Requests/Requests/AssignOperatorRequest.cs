using System.ComponentModel.DataAnnotations;

namespace Application.Dto.Requests.Requests;

public class AssignOperatorRequest
{
    [Required]
    public Guid OperatorId { get; set; }
}