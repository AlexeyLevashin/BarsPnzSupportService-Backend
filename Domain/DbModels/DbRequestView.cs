namespace Domain.DbModels;

public class DbRequestView
{
    public Guid RequestId { get; set; }
    public DbRequest Request { get; set; }
    public Guid UserId { get; set; }
    public DbUser User { get; set; }
    public DateTime LastViewedAt { get; set; }
}