using Microsoft.EntityFrameworkCore;
using PropertyManager.Api.Domain.Entities;
using PropertyManager.Api.Domain.Enums;

namespace PropertyManager.Api.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<RentalUnit> RentalUnits => Set<RentalUnit>();
    public DbSet<RentalContract> RentalContracts => Set<RentalContract>();
    public DbSet<RentPayment> RentPayments => Set<RentPayment>();
    public DbSet<PaymentAlert> PaymentAlerts => Set<PaymentAlert>();
    public DbSet<DocumentLog> DocumentLogs => Set<DocumentLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(128);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(128);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(32);
        });

        modelBuilder.Entity<RentalUnit>(entity =>
        {
            entity.Property(e => e.Name).IsRequired().HasMaxLength(128);
            entity.Property(e => e.AddressLine1).IsRequired().HasMaxLength(256);
            entity.Property(e => e.City).IsRequired().HasMaxLength(128);
            entity.Property(e => e.State).IsRequired().HasMaxLength(64);
            entity.Property(e => e.PostalCode).IsRequired().HasMaxLength(16);
        });

        modelBuilder.Entity<RentalContract>(entity =>
        {
            entity.Property(e => e.MonthlyRent).HasColumnType("numeric(18,2)");
            entity.Property(e => e.DepositAmount).HasColumnType("numeric(18,2)");
            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.Contracts)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.RentalUnit)
                .WithMany(u => u.Contracts)
                .HasForeignKey(e => e.RentalUnitId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RentPayment>(entity =>
        {
            entity.Property(e => e.AmountDue).HasColumnType("numeric(18,2)");
            entity.Property(e => e.AmountPaid).HasColumnType("numeric(18,2)");
            entity.Property(e => e.LateFee).HasColumnType("numeric(18,2)");
            entity.Property(e => e.ReceiptNumber).HasMaxLength(64);
            entity.HasIndex(e => new { e.RentalContractId, e.DueDate }).IsUnique();
            entity.HasOne(e => e.RentalContract)
                .WithMany(c => c.Payments)
                .HasForeignKey(e => e.RentalContractId);
        });

        modelBuilder.Entity<PaymentAlert>(entity =>
        {
            entity.HasOne(e => e.RentPayment)
                .WithMany()
                .HasForeignKey(e => e.RentPaymentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DocumentLog>(entity =>
        {
            entity.Property(e => e.FileName).HasMaxLength(256);
            entity.Property(e => e.ContentType).HasMaxLength(128);
            entity.HasIndex(e => e.DocumentType);
        });

        SeedDemoData(modelBuilder);
    }

    private static void SeedDemoData(ModelBuilder modelBuilder)
    {
        var tenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var unitId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var contractId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var paymentId = Guid.Parse("44444444-4444-4444-4444-444444444444");

        modelBuilder.Entity<Tenant>().HasData(new Tenant
        {
            Id = tenantId,
            FirstName = "Ada",
            LastName = "Lovelace",
            Email = "ada@example.com",
            PhoneNumber = "+11234567890",
            CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        modelBuilder.Entity<RentalUnit>().HasData(new RentalUnit
        {
            Id = unitId,
            Name = "Unit 101",
            AddressLine1 = "123 Main Street",
            City = "Metropolis",
            State = "CA",
            PostalCode = "94016",
            MonthlyRent = 1800m,
            Bedrooms = 2,
            Bathrooms = 2,
            SquareFeet = 950,
            Status = RentalUnitStatus.Occupied,
            CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        modelBuilder.Entity<RentalContract>().HasData(new RentalContract
        {
            Id = contractId,
            TenantId = tenantId,
            RentalUnitId = unitId,
            StartDate = new DateTime(2024, 1, 1),
            EndDate = null,
            MonthlyRent = 1800m,
            DepositAmount = 1800m,
            PaymentDueDay = 5,
            PaymentSchedule = PaymentSchedule.Monthly,
            Status = ContractStatus.Active,
            Currency = Currency.XAF,
            CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        modelBuilder.Entity<RentPayment>().HasData(new RentPayment
        {
            Id = paymentId,
            RentalContractId = contractId,
            DueDate = new DateTime(2024, 1, 5),
            AmountDue = 1800m,
            AmountPaid = 0,
            LateFee = 0,
            Status = RentPaymentStatus.Pending,
            ReceiptNumber = "RCPT-DEMO-202401",
            Currency = Currency.XAF,
            CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}
