namespace PropertyManager.Api.Domain.Entities;

public class PaymentAlert : BaseEntity
{
    public Guid RentPaymentId { get; set; }
    public RentPayment RentPayment { get; set; } = default!;
    public string Message { get; set; } = string.Empty;
    public bool IsAcknowledged { get; set; }
    public DateTime AlertDate { get; set; } = DateTime.UtcNow;
}
