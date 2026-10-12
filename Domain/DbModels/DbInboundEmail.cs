namespace Domain.DbModels;

public class DbInboundEmail
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public long UidValidity { get; set; }
    public long Uid { get; set; }
    public string? MessageId { get; set; }
    public Guid RequestId { get; set; }
    public DbRequest Request { get; set; }
}
