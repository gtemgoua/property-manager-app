using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using PropertyManager.Api.Contracts.Requests;
using PropertyManager.Api.Contracts.Responses;
using PropertyManager.Api.Data;
using PropertyManager.Api.Domain.Entities;
using PropertyManager.Api.Domain.Enums;
using PropertyManager.Api.Services.Interfaces;

namespace PropertyManager.Api.Services.Implementations;

public class RentalContractService(ApplicationDbContext context, IMapper mapper) : IRentalContractService
{
    public async Task<IEnumerable<RentalContractResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.RentalContracts
            .AsNoTracking()
            .Include(c => c.Tenant)
            .Include(c => c.RentalUnit)
            .OrderByDescending(c => c.StartDate)
            .ProjectTo<RentalContractResponse>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }

    public async Task<RentalContractResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.RentalContracts
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Include(c => c.Tenant)
            .Include(c => c.RentalUnit)
            .ProjectTo<RentalContractResponse>(mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<RentalContractResponse> CreateAsync(CreateRentalContractRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateTenantAndUnitAsync(request.TenantId, request.RentalUnitId, cancellationToken);

        var entity = new RentalContract
        {
            TenantId = request.TenantId,
            RentalUnitId = request.RentalUnitId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            MonthlyRent = request.MonthlyRent,
            DepositAmount = request.DepositAmount,
            PaymentDueDay = request.PaymentDueDay,
            PaymentSchedule = request.PaymentSchedule,
            Notes = request.Notes,
            Status = ContractStatus.Active
        };

        context.RentalContracts.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        entity = await context.RentalContracts
            .Include(c => c.Tenant)
            .Include(c => c.RentalUnit)
            .SingleAsync(c => c.Id == entity.Id, cancellationToken);

        await GenerateInitialPaymentScheduleAsync(entity, cancellationToken);

        return mapper.Map<RentalContractResponse>(entity);
    }

    public async Task<RentalContractResponse?> UpdateAsync(Guid id, UpdateRentalContractRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await context.RentalContracts
            .Include(c => c.Tenant)
            .Include(c => c.RentalUnit)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        entity.StartDate = request.StartDate;
        entity.EndDate = request.EndDate;
        entity.MonthlyRent = request.MonthlyRent;
        entity.DepositAmount = request.DepositAmount;
        entity.PaymentDueDay = request.PaymentDueDay;
        entity.PaymentSchedule = request.PaymentSchedule;
        entity.Status = request.Status;
        entity.Notes = request.Notes;
        entity.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return mapper.Map<RentalContractResponse>(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await context.RentalContracts
            .Include(c => c.Payments)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (entity is null)
        {
            return false;
        }

        if (entity.Payments.Any(p => p.Status != Domain.Enums.RentPaymentStatus.Pending))
        {
            throw new InvalidOperationException("Cannot delete contract with processed payments.");
        }

        context.RentalContracts.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task ValidateTenantAndUnitAsync(Guid tenantId, Guid unitId, CancellationToken cancellationToken)
    {
        var tenantExists = await context.Tenants.AnyAsync(t => t.Id == tenantId, cancellationToken);
        if (!tenantExists)
        {
            throw new InvalidOperationException("Tenant not found.");
        }

        var unit = await context.RentalUnits.FirstOrDefaultAsync(u => u.Id == unitId, cancellationToken);
        if (unit is null)
        {
            throw new InvalidOperationException("Rental unit not found.");
        }

        if (unit.Status == Domain.Enums.RentalUnitStatus.Occupied)
        {
            throw new InvalidOperationException("Rental unit already occupied.");
        }

        unit.Status = Domain.Enums.RentalUnitStatus.Occupied;
    }

    private async Task GenerateInitialPaymentScheduleAsync(RentalContract contract, CancellationToken cancellationToken)
    {
        if (contract.PaymentSchedule != PaymentSchedule.Monthly)
        {
            return; // simplified – generation for monthly only
        }

        var today = DateTime.UtcNow.Date;
        var firstDueDate = new DateTime(today.Year, today.Month, Math.Clamp(contract.PaymentDueDay, 1, 28));
        if (firstDueDate < today)
        {
            firstDueDate = firstDueDate.AddMonths(1);
        }

        var payment = new RentPayment
        {
            RentalContractId = contract.Id,
            DueDate = firstDueDate,
            AmountDue = contract.MonthlyRent,
            AmountPaid = 0,
            LateFee = 0,
            Status = Domain.Enums.RentPaymentStatus.Pending,
            ReceiptNumber = GenerateReceiptNumber(contract)
        };

        context.RentPayments.Add(payment);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static string GenerateReceiptNumber(RentalContract contract) =>
        $"RCPT-{contract.Id.ToString().Substring(0, 8).ToUpperInvariant()}-{DateTime.UtcNow:yyyyMMdd}";
}
