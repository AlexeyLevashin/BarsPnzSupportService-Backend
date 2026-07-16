namespace Application.Dto.Users.Responses;

public class UserInstitutionResponse
{
    public Guid InstitutionId { get; set; }
    public string InstitutionName { get; set; }
    public Guid? JobTitleId { get; set; } 
    public string? JobTitleName { get; set; }
}