using PropertyManager.Api.Domain.Enums;

namespace PropertyManager.Api.Domain.Entities;

public class RentalContract : BaseEntity
{
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = default!;

    public Guid RentalUnitId { get; set; }
    public RentalUnit RentalUnit { get; set; } = default!;

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal MonthlyRent { get; set; }
    public decimal DepositAmount { get; set; }
    public Currency Currency { get; set; } = Currency.XAF;
    public int PaymentDueDay { get; set; } = 1;
    public PaymentSchedule PaymentSchedule { get; set; } = PaymentSchedule.Monthly;
    public ContractStatus Status { get; set; } = ContractStatus.Active;
    public string? Notes { get; set; }

    public ICollection<RentPayment> Payments { get; set; } = new List<RentPayment>();
}
