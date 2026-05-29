namespace Domain.DbModels;

public class DbAttachment
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string FileName { get; set; }
    public string StorageKey { get; set; }
    public string ContentType { get; set; }
    public long FileSize { get; set; }
    public Guid? MessageId { get; set; }
    public DbMessage Message { get; set; }
}