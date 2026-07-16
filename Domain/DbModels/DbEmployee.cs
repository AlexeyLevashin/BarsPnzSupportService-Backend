namespace Domain.DbModels;

public class DbEmployee
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string Name { get; set; }
    public string Surname { get; set; }
    public string? Patronymic { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public bool IsUser { get; set; } 
    public bool IsDeleted { get; set; } = false;

    public DbUser? User { get; set; }   
    public List<DbEmployeeInstitution> EmployeeInstitutions { get; set; } = new();
}