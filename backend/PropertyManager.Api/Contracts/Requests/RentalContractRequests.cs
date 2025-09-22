using PropertyManager.Api.Domain.Enums;

namespace PropertyManager.Api.Contracts.Requests;

public record CreateRentalContractRequest(
    Guid TenantId,
    Guid RentalUnitId,
    DateTime StartDate,
    DateTime? EndDate,
    decimal MonthlyRent,
    decimal DepositAmount,
    int PaymentDueDay,
    PaymentSchedule PaymentSchedule,
    string? Notes
);

public record UpdateRentalContractRequest(
    DateTime StartDate,
    DateTime? EndDate,
    decimal MonthlyRent,
    decimal DepositAmount,
    int PaymentDueDay,
    PaymentSchedule PaymentSchedule,
    ContractStatus Status,
    string? Notes
);
