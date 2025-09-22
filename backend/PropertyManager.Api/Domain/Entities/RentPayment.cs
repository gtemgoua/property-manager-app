using PropertyManager.Api.Domain.Enums;

namespace PropertyManager.Api.Domain.Entities;

public class RentPayment : BaseEntity
{
    public Guid RentalContractId { get; set; }
    public RentalContract RentalContract { get; set; } = default!;

    public DateTime DueDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public decimal AmountDue { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal? LateFee { get; set; }
    public Currency Currency { get; set; } = Currency.XAF;
    public RentPaymentStatus Status { get; set; } = RentPaymentStatus.Pending;
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Unknown;
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
    public bool ReceiptSent { get; set; }
    public DateTime? ReceiptSentAt { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
}
