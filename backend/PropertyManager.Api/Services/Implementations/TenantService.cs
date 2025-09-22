using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using PropertyManager.Api.Contracts.Requests;
using PropertyManager.Api.Contracts.Responses;
using PropertyManager.Api.Data;
using PropertyManager.Api.Domain.Entities;
using PropertyManager.Api.Services.Interfaces;

namespace PropertyManager.Api.Services.Implementations;

public class TenantService(ApplicationDbContext context, IMapper mapper) : ITenantService
{
    public async Task<IEnumerable<TenantResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Tenants
            .AsNoTracking()
            .OrderBy(t => t.LastName)
            .ThenBy(t => t.FirstName)
            .ProjectTo<TenantResponse>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }

    public async Task<TenantResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Tenants
            .AsNoTracking()
            .Where(t => t.Id == id)
            .ProjectTo<TenantResponse>(mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<TenantResponse> CreateAsync(CreateTenantRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new Tenant
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            PhoneNumber = request.PhoneNumber.Trim(),
            EmergencyContactName = request.EmergencyContactName,
            EmergencyContactPhone = request.EmergencyContactPhone,
            Notes = request.Notes
        };

        context.Tenants.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return mapper.Map<TenantResponse>(entity);
    }

    public async Task<TenantResponse?> UpdateAsync(Guid id, UpdateTenantRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await context.Tenants.FindAsync(new object?[] { id }, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.FirstName = request.FirstName.Trim();
        entity.LastName = request.LastName.Trim();
        entity.Email = request.Email.Trim().ToLowerInvariant();
        entity.PhoneNumber = request.PhoneNumber.Trim();
        entity.EmergencyContactName = request.EmergencyContactName;
        entity.EmergencyContactPhone = request.EmergencyContactPhone;
        entity.Notes = request.Notes;
        entity.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return mapper.Map<TenantResponse>(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await context.Tenants
            .Include(t => t.Contracts)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (entity is null)
        {
            return false;
        }

        if (entity.Contracts.Any())
        {
            throw new InvalidOperationException("Cannot delete tenant with active contracts.");
        }

        context.Tenants.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
