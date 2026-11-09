using System.ComponentModel.DataAnnotations;

namespace Application.Dto.Requests.Requests;

public class MergeRequestsRequest
{
    [Required(ErrorMessage = "Не выбраны заявки для объединения")]
    [MinLength(1, ErrorMessage = "Не выбраны заявки для объединения")]
    public List<Guid> DuplicateRequestIds { get; set; } = new();
}
