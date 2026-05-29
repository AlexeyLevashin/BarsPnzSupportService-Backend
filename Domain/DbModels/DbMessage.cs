using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums;

namespace Domain.DbModels;

public class DbMessage
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string? Text { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public MessageType Type { get; set; }
    public Guid SenderId { get; set; }
    public Guid RequestId { get; set; }
    public DbUser Sender { get; set; }
    public DbRequest Request { get; set; }
    public ICollection<DbAttachment> Attachments { get; set; } = new List<DbAttachment>();
}