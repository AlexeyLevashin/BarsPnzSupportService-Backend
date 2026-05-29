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
   
   public async Task AttachFilesToMessageAsync(List<Guid> fileIds, Guid messageId)
   {
      await _context.Attachments
         .Where(a => fileIds.Contains(a.Id))
         .ExecuteUpdateAsync(s => s.SetProperty(x => x.MessageId, messageId));
   }

   public void DeleteAsync(DbAttachment dbAttachment)
   {
      _context.Remove(dbAttachment);
   }
}