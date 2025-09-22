using PropertyManager.Api.Domain.Enums;

namespace PropertyManager.Api.Contracts.Responses;

public class RentalContractResponse
{
    // Parameterless DTO so AutoMapper.ProjectTo can construct instances during IQueryable projection
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public Guid RentalUnitId { get; set; }
    public string RentalUnitName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal MonthlyRent { get; set; }
    public decimal DepositAmount { get; set; }
    public Currency Currency { get; set; } = Currency.XAF;
    public int PaymentDueDay { get; set; }
    public PaymentSchedule PaymentSchedule { get; set; }
    public ContractStatus Status { get; set; }
    public string? Notes { get; set; }
}
