using PropertyManager.Api.Contracts.Responses;

namespace PropertyManager.Api.Services.Interfaces;

public interface IReportService
{
    Task<DashboardMetricsResponse> GetDashboardMetricsAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<byte[]> GeneratePaymentsReportExcelAsync(DateTime from, DateTime to, Domain.Enums.Currency? currency = null, CancellationToken cancellationToken = default);
    Task<byte[]> GeneratePaymentsReportPdfAsync(DateTime from, DateTime to, Domain.Enums.Currency? currency = null, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateTestPaymentsPdfAsync(CancellationToken cancellationToken = default);
}
