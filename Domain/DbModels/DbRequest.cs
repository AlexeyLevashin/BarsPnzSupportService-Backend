using Domain.Enums;

namespace Domain.DbModels;

public class DbRequest
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string Theme { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; set; }
    public Guid ClientId { get; set; }
    public Guid? OperatorId { get; set; }
    public RequestStatus Status { get; set; }
    public Priority Priority { get; set; }
    public DbUser Client { get; set; }
    public DbUser? Operator { get; set; }
    
    public List<DbMessage> Messages { get; set; } = new();
}