namespace PropertyManager.Api.Contracts.Responses;

public record PaymentAlertResponse(
    Guid Id,
    Guid RentPaymentId,
    string Message,
    bool IsAcknowledged,
    DateTime AlertDate
);
