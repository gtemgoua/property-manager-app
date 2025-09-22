using PropertyManager.Api.Domain.Enums;

namespace PropertyManager.Api.Domain.Entities;

public class DocumentLog : BaseEntity
{
    public DocumentType DocumentType { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public Guid? RentPaymentId { get; set; }
    public RentPayment? RentPayment { get; set; }
    public string? Metadata { get; set; }
}
