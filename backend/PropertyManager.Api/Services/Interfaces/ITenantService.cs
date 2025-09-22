using PropertyManager.Api.Contracts.Requests;
using PropertyManager.Api.Contracts.Responses;

namespace PropertyManager.Api.Services.Interfaces;

public interface ITenantService
{
    Task<IEnumerable<TenantResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TenantResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TenantResponse> CreateAsync(CreateTenantRequest request, CancellationToken cancellationToken = default);
    Task<TenantResponse?> UpdateAsync(Guid id, UpdateTenantRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
