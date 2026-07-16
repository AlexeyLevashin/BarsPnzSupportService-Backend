namespace Domain.DbModels;

public class DbEmployeeInstitution
{
    public Guid EmployeeId { get; set; }
    public DbEmployee Employee { get; set; }

    public Guid InstitutionId { get; set; }
    public DbInstitution Institution { get; set; }

    public Guid? JobTitleId { get; set; }
    public DbJobTitle? JobTitle { get; set; }
}