using PropertyManager.Api.Domain.Enums;

namespace PropertyManager.Api.Contracts.Requests;

public record CreateRentPaymentRequest(
    Guid RentalContractId,
    DateTime DueDate,
    decimal AmountDue,
    string? Notes,
    Currency Currency = Currency.XAF
);

public record RecordRentPaymentRequest(
    decimal AmountPaid,
    DateTime PaidDate,
    PaymentMethod PaymentMethod,
    string? ReferenceNumber,
    decimal? LateFee,
    string? Notes
);

public record SendReceiptRequest(
    string RecipientEmail,
    string? RecipientName,
    bool AttachPdf
);
