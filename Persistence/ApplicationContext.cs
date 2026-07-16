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
    public DbSet<DbEmployee> Employees { get; set; }
    public DbSet<DbJobTitle> JobTitles { get; set; }
    public DbSet<DbEmployeeInstitution> EmployeeInstitutions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<DbUser>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Role).HasConversion<string>();
            
            entity.HasOne(u => u.Employee)
                .WithOne(e => e.User)
                .HasForeignKey<DbUser>(u => u.EmployeeId)
                .IsRequired(); 
        });
        
        modelBuilder.Entity<DbEmployee>(entity =>
        {
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });
        
        modelBuilder.Entity<DbEmployee>().HasQueryFilter(e => !e.IsDeleted);

        modelBuilder.Entity<DbUser>(entity =>
        {
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });

        modelBuilder.Entity<DbUser>().HasQueryFilter(e => !e.IsDeleted);
        
        modelBuilder.Entity<DbEmployeeInstitution>(entity =>
        {
            entity.HasKey(ei => new { ei.EmployeeId, ei.InstitutionId });

            entity.HasOne(ei => ei.Employee)
                .WithMany(e => e.EmployeeInstitutions)
                .HasForeignKey(ei => ei.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ei => ei.Institution)
                .WithMany(i => i.EmployeeInstitutions)
                .HasForeignKey(ei => ei.InstitutionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ei => ei.JobTitle)
                .WithMany(jt => jt.EmployeeInstitutions)
                .HasForeignKey(ei => ei.JobTitleId)
                .OnDelete(DeleteBehavior.SetNull); 
        });
        
        modelBuilder.Entity<DbInstitution>(entity =>
        {
            entity.HasIndex(i => i.INN).IsUnique();
            entity.Property(i => i.INN).HasMaxLength(12).IsRequired();

            entity.Property(i => i.PhoneNumber).HasMaxLength(50);
            entity.Property(i => i.Email).HasMaxLength(256);
            
            entity.HasOne(i => i.Head)
                .WithMany() 
                .HasForeignKey(i => i.HeadId)
                .OnDelete(DeleteBehavior.SetNull); 
        });
        
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

        modelBuilder.Entity<DbAttachment>(entity =>
        {
            entity.HasIndex(a => a.StorageKey).IsUnique();
        });
    }
}