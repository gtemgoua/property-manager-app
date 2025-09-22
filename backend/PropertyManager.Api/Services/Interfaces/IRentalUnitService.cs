using PropertyManager.Api.Contracts.Requests;
using PropertyManager.Api.Contracts.Responses;

namespace PropertyManager.Api.Services.Interfaces;

public interface IRentalUnitService
{
    Task<IEnumerable<RentalUnitResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<RentalUnitResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<RentalUnitResponse> CreateAsync(CreateRentalUnitRequest request, CancellationToken cancellationToken = default);
    Task<RentalUnitResponse?> UpdateAsync(Guid id, UpdateRentalUnitRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
