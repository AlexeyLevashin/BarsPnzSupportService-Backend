using Domain.DbModels;
using Microsoft.EntityFrameworkCore;

namespace Persistence;

public class ApplicationContext : DbContext
{
    public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
    {
    }
    public DbSet<DbAttachment> Attachments { get; set; }
    public DbSet<DbInstitution> Institutions { get; set; }
    public DbSet<DbMessage> Messages { get; set; }
    public DbSet<DbRequest> Requests { get; set; }
    public DbSet<DbUser> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<DbUser>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();

            entity.Property(u => u.Role).HasConversion<string>();
            
            entity.Property(u => u.IsDeleted)
                .HasDefaultValue(false);
            
            entity.HasOne(u => u.Institution)
                .WithMany(i => i.Users)
                .HasForeignKey(u => u.InstitutionId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        modelBuilder.Entity<DbUser>().HasQueryFilter(u => !u.IsDeleted);

        modelBuilder.Entity<DbMessage>(entity =>
        {
            entity.Property(m => m.Type).HasConversion<string>();
            entity.HasIndex(m => new { m.RequestId, m.CreatedAt });
            
            entity.HasOne(m => m.Request)
                .WithMany(r => r.Messages) 
                .HasForeignKey(m => m.RequestId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DbRequest>(entity =>
        {
            entity.Property(r => r.Status).HasConversion<string>();
            entity.Property(r => r.Priority).HasConversion<string>();
            
            entity.HasIndex(r => r.Status);
            entity.HasIndex(r => r.CreatedAt);
            
            entity.HasOne(r => r.Client)
                .WithMany()
                .HasForeignKey(r => r.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(r => r.Operators)
                .WithMany(u => u.Requests)
                .UsingEntity(j => j.ToTable("RequestOperators"));
        });

        modelBuilder.Entity<DbInstitution>(entity =>
        {
            entity.HasIndex(i => i.INN)
                .IsUnique();
            
            entity.Property(i => i.INN)
                .HasMaxLength(12)
                .IsRequired();
        });

        modelBuilder.Entity<DbAttachment>(entity =>
        {
            entity.HasIndex(a => a.StorageKey)
                .IsUnique();
        });
    }
}