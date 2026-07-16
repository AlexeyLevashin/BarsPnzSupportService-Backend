using System.ComponentModel.DataAnnotations;

namespace Application.Dto.Requests.Requests;

public class OperatorRequest
{
    [Required]
    public Guid OperatorId { get; set; }
}