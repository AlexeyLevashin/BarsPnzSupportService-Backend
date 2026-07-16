namespace Domain.DbModels;

public class DbJobTitle
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string Name { get; set; }
    public List<DbEmployeeInstitution> EmployeeInstitutions { get; set; } = new();
}