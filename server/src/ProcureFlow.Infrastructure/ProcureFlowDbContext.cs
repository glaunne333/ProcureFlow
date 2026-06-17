using Microsoft.EntityFrameworkCore;
using ProcureFlow.Domain;

namespace ProcureFlow.Infrastructure;

public sealed class ProcureFlowDbContext(DbContextOptions<ProcureFlowDbContext> options) : DbContext(options)
{
    public DbSet<ApprovalLog> ApprovalLogs => Set<ApprovalLog>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<ProcurementRequest> ProcurementRequests => Set<ProcurementRequest>();
    public DbSet<RequestItem> RequestItems => Set<RequestItem>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Vendor> Vendors => Set<Vendor>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Department>(entity =>
        {
            entity.ToTable("Departments");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(240).IsRequired();
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            entity.Property(x => x.Role).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.HasOne<Department>()
                .WithMany()
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Vendor>(entity =>
        {
            entity.ToTable("Vendors");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(160).IsRequired();
            entity.Property(x => x.ContactEmail).HasMaxLength(240);
        });

        modelBuilder.Entity<ProcurementRequest>(entity =>
        {
            entity.ToTable("ProcurementRequests");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RequestNo).HasMaxLength(40).IsRequired();
            entity.HasIndex(x => x.RequestNo).IsUnique();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.Property(x => x.EstimatedTotal).HasPrecision(12, 2);
            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.RequestedById)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Department>()
                .WithMany()
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Vendor>()
                .WithMany()
                .HasForeignKey(x => x.VendorId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(x => x.Items)
                .WithOne()
                .HasForeignKey(x => x.RequestId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.ApprovalLogs)
                .WithOne()
                .HasForeignKey(x => x.RequestId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Navigation(x => x.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(x => x.ApprovalLogs).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<RequestItem>(entity =>
        {
            entity.ToTable("RequestItems");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Description).HasMaxLength(300).IsRequired();
            entity.Property(x => x.Category).HasMaxLength(120).IsRequired();
            entity.Property(x => x.UnitCost).HasPrecision(12, 2);
            entity.Ignore(x => x.LineTotal);
        });

        modelBuilder.Entity<ApprovalLog>(entity =>
        {
            entity.ToTable("ApprovalLogs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FromStatus).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.Property(x => x.ToStatus).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.Property(x => x.Remarks).HasMaxLength(1000);
            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.ActorId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
