using Microsoft.EntityFrameworkCore;
using GymSaaS.Domain.Entities;
using GymSaaS.Domain.Interfaces;

namespace GymSaaS.Persistence;

public class GymDbContext : DbContext
{
    private readonly ITenantProvider _tenantProvider;

    public GymDbContext(DbContextOptions<GymDbContext> options, ITenantProvider tenantProvider)
        : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Member> Members { get; set; }
    public DbSet<Trainer> Trainers { get; set; }
    public DbSet<MembershipPackage> Packages { get; set; }
    public DbSet<Membership> Memberships { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Attendance> Attendances { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<TrainerAssignment> TrainerAssignments { get; set; }
    public DbSet<Branch> Branches { get; set; }
    public DbSet<WorkingHours> WorkingHours { get; set; }
    public DbSet<PaymentSchedule> PaymentSchedules { get; set; }
    public DbSet<PaymentType> PaymentTypes { get; set; }
    public DbSet<TrainerType> TrainerTypes { get; set; }
    public DbSet<ServiceSetting> ServiceSettings { get; set; }
    public DbSet<GymClass> GymClasses { get; set; }
    public DbSet<ClassType> ClassTypes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ─── Global tenant filters ───────────────────────────
        // Every query automatically becomes: WHERE TenantId = CURRENT_TENANT
        // This prevents Gym A from accessing Gym B's data.

        modelBuilder.Entity<User>()
            .HasQueryFilter(u => u.TenantId == _tenantProvider.TenantId);

        modelBuilder.Entity<Member>()
            .HasQueryFilter(m => m.TenantId == _tenantProvider.TenantId);

        modelBuilder.Entity<Trainer>()
            .HasQueryFilter(t => t.TenantId == _tenantProvider.TenantId);

        modelBuilder.Entity<MembershipPackage>()
            .HasQueryFilter(p => p.TenantId == _tenantProvider.TenantId);

        modelBuilder.Entity<Membership>()
            .HasQueryFilter(ms => ms.TenantId == _tenantProvider.TenantId);

        modelBuilder.Entity<Payment>()
            .HasQueryFilter(py => py.TenantId == _tenantProvider.TenantId);

        modelBuilder.Entity<Attendance>()
            .HasQueryFilter(a => a.TenantId == _tenantProvider.TenantId);

        modelBuilder.Entity<Role>()
            .HasQueryFilter(r => r.TenantId == _tenantProvider.TenantId);

        modelBuilder.Entity<AuditLog>()
            .HasQueryFilter(al => al.TenantId == _tenantProvider.TenantId);

        modelBuilder.Entity<TrainerAssignment>()
            .HasQueryFilter(ta => ta.TenantId == _tenantProvider.TenantId);

        modelBuilder.Entity<Branch>()
            .HasQueryFilter(b => b.TenantId == _tenantProvider.TenantId);

        modelBuilder.Entity<TrainerType>()
            .HasQueryFilter(tt => tt.TenantId == _tenantProvider.TenantId);

        modelBuilder.Entity<ServiceSetting>()
            .HasQueryFilter(ss => ss.TenantId == _tenantProvider.TenantId);

        modelBuilder.Entity<GymClass>()
            .HasQueryFilter(gc => gc.TenantId == _tenantProvider.TenantId);

        modelBuilder.Entity<ClassType>()
            .HasQueryFilter(ct => ct.TenantId == _tenantProvider.TenantId);

        modelBuilder.Entity<WorkingHours>()
            .HasQueryFilter(wh => wh.TenantId == _tenantProvider.TenantId);

        modelBuilder.Entity<PaymentSchedule>()
            .HasQueryFilter(ps => ps.TenantId == _tenantProvider.TenantId);

        // ─── Entity configurations ───────────────────────────

        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.GymName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.SubDomain).IsUnique();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.Email).IsUnique();
        });

        modelBuilder.Entity<Member>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
            entity.Property(e => e.MembershipNumber).IsRequired().HasMaxLength(20);
            entity.HasIndex(e => new { e.TenantId, e.MembershipNumber }).IsUnique();
            entity.HasOne(e => e.Trainer).WithMany(t => t.Members).HasForeignKey(e => e.TrainerId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Branch).WithMany(b => b.Members).HasForeignKey(e => e.BranchId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Trainer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.HasOne(e => e.Branch).WithMany(b => b.Trainers).HasForeignKey(e => e.BranchId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.TrainerType).WithMany(tt => tt.Trainers).HasForeignKey(e => e.TrainerTypeId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<TrainerType>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        modelBuilder.Entity<ClassType>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        modelBuilder.Entity<Branch>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.Phone).HasMaxLength(20);
        });

        modelBuilder.Entity<WorkingHours>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Day).IsRequired().HasMaxLength(20);
            entity.Property(e => e.OpenTime).IsRequired().HasMaxLength(10);
            entity.Property(e => e.CloseTime).IsRequired().HasMaxLength(10);
        });
        modelBuilder.Entity<MembershipPackage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<Membership>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Discount).HasColumnType("decimal(18,2)");
            entity.HasOne(e => e.Member).WithMany(m => m.Memberships).HasForeignKey(e => e.MemberId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Package).WithMany(p => p.Memberships).HasForeignKey(e => e.PackageId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.PaymentTypeId).HasDefaultValue(GymSaaS.Shared.AppConstants.PaymentTypeIds.RegularMonthly);
            entity.HasOne(e => e.Member).WithMany(m => m.Payments).HasForeignKey(e => e.MemberId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.PaymentSchedule).WithMany(s => s.Payments).HasForeignKey(e => e.PaymentScheduleId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.PaymentType).WithMany(pt => pt.Payments).HasForeignKey(e => e.PaymentTypeId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PaymentSchedule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.PaymentTypeId).HasDefaultValue(GymSaaS.Shared.AppConstants.PaymentTypeIds.RegularMonthly);
            entity.HasOne(e => e.Membership).WithMany().HasForeignKey(e => e.MembershipId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Member).WithMany().HasForeignKey(e => e.MemberId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.PaymentType).WithMany(pt => pt.PaymentSchedules).HasForeignKey(e => e.PaymentTypeId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PaymentType>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasData(
                new PaymentType { Id = 1, Name = "Registration Fee",  Description = "One-time enrollment fee",        IsActive = true },
                new PaymentType { Id = 2, Name = "First Month Payment", Description = "Initial monthly installment", IsActive = true },
                new PaymentType { Id = 3, Name = "Monthly Payment",    Description = "Regular monthly subscription", IsActive = true }
            );
        });

        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Member).WithMany(m => m.Attendances).HasForeignKey(e => e.MemberId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GymClass>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.StartTime).IsRequired().HasMaxLength(10);
            entity.Property(e => e.EndTime).IsRequired().HasMaxLength(10);
            entity.Property(e => e.DefaultAmount).HasColumnType("decimal(18,2)");
            entity.HasOne(e => e.Instructor).WithMany().HasForeignKey(e => e.InstructorId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Branch).WithMany().HasForeignKey(e => e.BranchId).OnDelete(DeleteBehavior.SetNull);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Ensure all DateTime properties are UTC for PostgreSQL
        foreach (var entry in ChangeTracker.Entries())
        {
            foreach (var property in entry.Properties)
            {
                if (property.Metadata.ClrType == typeof(DateTime) || property.Metadata.ClrType == typeof(DateTime?))
                {
                    if (property.CurrentValue is DateTime dt && dt.Kind == DateTimeKind.Unspecified)
                    {
                        property.CurrentValue = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
                    }
                }
            }
        }

        // Auto-set TenantId on new entities and update timestamps
        foreach (var entry in ChangeTracker.Entries<BaseTenantEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                // Only override TenantId if it hasn't been explicitly set (which happens during Tenant Registration)
                if (entry.Entity.TenantId == Guid.Empty)
                {
                    entry.Entity.TenantId = _tenantProvider.TenantId;
                }
                entry.Entity.CreatedAt = DateTime.UtcNow;
            }
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
