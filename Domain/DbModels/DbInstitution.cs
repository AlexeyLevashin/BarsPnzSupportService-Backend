namespace Domain.DbModels;

public class DbInstitution
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string Name { get; set; }
    public string INN { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public Guid? HeadId { get; set; }
    public DbEmployee? Head { get; set; }
    public List<DbEmployeeInstitution> EmployeeInstitutions { get; set; } = new();
}