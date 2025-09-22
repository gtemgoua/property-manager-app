using PropertyManager.Api.Domain.Entities;

namespace PropertyManager.Api.Services.Interfaces;

public interface IPdfService
{
    Task<byte[]> GenerateRentReceiptAsync(RentPayment payment, CancellationToken cancellationToken = default);
    Task<byte[]> GeneratePaymentsReportAsync(IEnumerable<RentPayment> payments, DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateTestPdfAsync(CancellationToken cancellationToken = default);
}
