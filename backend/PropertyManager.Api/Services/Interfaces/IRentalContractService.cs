using PropertyManager.Api.Contracts.Requests;
using PropertyManager.Api.Contracts.Responses;

namespace PropertyManager.Api.Services.Interfaces;

public interface IRentalContractService
{
    Task<IEnumerable<RentalContractResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<RentalContractResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<RentalContractResponse> CreateAsync(CreateRentalContractRequest request, CancellationToken cancellationToken = default);
    Task<RentalContractResponse?> UpdateAsync(Guid id, UpdateRentalContractRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
