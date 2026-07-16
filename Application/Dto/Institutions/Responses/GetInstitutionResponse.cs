namespace Application.Dto.Institutions.Responses;

public class GetInstitutionResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string INN { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public Guid? HeadId { get; set; }
    public string? HeadName { get; set; }
    public string? HeadSurname { get; set; }
    public string? HeadPatronymic { get; set; }
}