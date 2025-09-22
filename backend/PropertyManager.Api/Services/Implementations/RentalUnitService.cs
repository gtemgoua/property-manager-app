using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using PropertyManager.Api.Contracts.Requests;
using PropertyManager.Api.Contracts.Responses;
using PropertyManager.Api.Data;
using PropertyManager.Api.Domain.Entities;
using PropertyManager.Api.Services.Interfaces;

namespace PropertyManager.Api.Services.Implementations;

public class RentalUnitService(ApplicationDbContext context, IMapper mapper) : IRentalUnitService
{
    public async Task<IEnumerable<RentalUnitResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.RentalUnits
            .AsNoTracking()
            .OrderBy(u => u.Name)
            .ProjectTo<RentalUnitResponse>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }

    public async Task<RentalUnitResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.RentalUnits
            .AsNoTracking()
            .Where(u => u.Id == id)
            .ProjectTo<RentalUnitResponse>(mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<RentalUnitResponse> CreateAsync(CreateRentalUnitRequest request, CancellationToken cancellationToken = default)
    {
        var entity = mapper.Map<RentalUnit>(request);
        context.RentalUnits.Add(entity);
        await context.SaveChangesAsync(cancellationToken);
        return mapper.Map<RentalUnitResponse>(entity);
    }

    public async Task<RentalUnitResponse?> UpdateAsync(Guid id, UpdateRentalUnitRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await context.RentalUnits.FindAsync(new object?[] { id }, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        mapper.Map(request, entity);
        entity.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);
        return mapper.Map<RentalUnitResponse>(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await context.RentalUnits
            .Include(u => u.Contracts)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (entity is null)
        {
            return false;
        }

        if (entity.Contracts.Any())
        {
            throw new InvalidOperationException("Cannot delete rental unit with active contracts.");
        }

        context.RentalUnits.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
