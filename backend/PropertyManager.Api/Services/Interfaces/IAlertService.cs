using PropertyManager.Api.Contracts.Responses;

namespace PropertyManager.Api.Services.Interfaces;

public interface IAlertService
{
    Task<IEnumerable<PaymentAlertResponse>> GetActiveAlertsAsync(CancellationToken cancellationToken = default);
    Task AcknowledgeAlertAsync(Guid alertId, CancellationToken cancellationToken = default);
    Task GenerateAlertsAsync(CancellationToken cancellationToken = default);
}
