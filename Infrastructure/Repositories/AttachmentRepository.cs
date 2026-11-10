using Domain.DbModels;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Repositories;

public class AttachmentRepository : IAttachmentRepository
{
   private readonly ApplicationContext _context;

   public AttachmentRepository(ApplicationContext context)
   {
      _context = context;
   }

   public async Task AddAsync(DbAttachment dbAttachment)
   {
      await _context.Attachments.AddAsync(dbAttachment);
   }

   public async Task<DbAttachment?> GetByIdAsync(Guid fileId)
   {
      return await _context.Attachments.FirstOrDefaultAsync(i => i.Id == fileId);
   }
   
   public Task<int> AttachFilesToMessageAsync(List<Guid> fileIds, Guid messageId)
   {
      // Привязываем только непривязанные файлы (MessageId == null)
      return _context.Attachments
         .Where(a => fileIds.Contains(a.Id) && a.MessageId == null)
         .ExecuteUpdateAsync(s => s.SetProperty(x => x.MessageId, messageId));
   }

   public void DeleteAsync(DbAttachment dbAttachment)
   {
      _context.Remove(dbAttachment);
   }
}