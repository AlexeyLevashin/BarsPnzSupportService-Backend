using System.ComponentModel.DataAnnotations;

namespace Application.Dto.JobTitles.Requests;

public class CreateJobTitleRequest
{
    [Required(ErrorMessage = "Название должности обязательно для заполнения")]
    [MinLength(1, ErrorMessage = "Название должности не может быть пустым")]
    public string Name { get; set; }
}
