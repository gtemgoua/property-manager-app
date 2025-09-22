using PropertyManager.Api.Domain.Enums;

namespace PropertyManager.Api.Contracts.Responses;

public class RentPaymentResponse
{
    public Guid Id { get; set; }
    public Guid RentalContractId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public string RentalUnitName { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public decimal AmountDue { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal? LateFee { get; set; }
    public Currency Currency { get; set; } = Currency.XAF;
    public RentPaymentStatus Status { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public bool ReceiptSent { get; set; }
    public DateTime? ReceiptSentAt { get; set; }
}
