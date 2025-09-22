using PropertyManager.Api.Contracts.Requests;
using PropertyManager.Api.Contracts.Responses;

namespace PropertyManager.Api.Services.Interfaces;

public interface IRentPaymentService
{
    Task<IEnumerable<RentPaymentResponse>> GetUpcomingAsync(DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<RentPaymentResponse>> GetByContractAsync(Guid contractId, CancellationToken cancellationToken = default);
    Task<RentPaymentResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<RentPaymentResponse> CreateAsync(CreateRentPaymentRequest request, CancellationToken cancellationToken = default);
    Task<RentPaymentResponse?> RecordPaymentAsync(Guid paymentId, RecordRentPaymentRequest request, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateReceiptPdfAsync(Guid paymentId, CancellationToken cancellationToken = default);
    Task<bool> SendReceiptAsync(Guid paymentId, SendReceiptRequest request, CancellationToken cancellationToken = default);
}
