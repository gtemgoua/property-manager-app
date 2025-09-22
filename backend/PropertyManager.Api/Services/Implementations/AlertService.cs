using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using PropertyManager.Api.Contracts.Responses;
using PropertyManager.Api.Data;
using PropertyManager.Api.Domain.Entities;
using PropertyManager.Api.Domain.Enums;
using PropertyManager.Api.Services.Interfaces;

namespace PropertyManager.Api.Services.Implementations;

public class AlertService(ApplicationDbContext context, IMapper mapper) : IAlertService
{
    public async Task<IEnumerable<PaymentAlertResponse>> GetActiveAlertsAsync(CancellationToken cancellationToken = default)
    {
        return await context.PaymentAlerts
            .AsNoTracking()
            .Where(a => !a.IsAcknowledged)
            .OrderByDescending(a => a.AlertDate)
            .ProjectTo<PaymentAlertResponse>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }

    public async Task AcknowledgeAlertAsync(Guid alertId, CancellationToken cancellationToken = default)
    {
        var alert = await context.PaymentAlerts.FindAsync(new object?[] { alertId }, cancellationToken);
        if (alert is null)
        {
            return;
        }

        alert.IsAcknowledged = true;
        alert.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task GenerateAlertsAsync(CancellationToken cancellationToken = default)
    {
        var thresholdDate = DateTime.UtcNow.Date.AddDays(-3);
        var latePayments = await context.RentPayments
            .Include(p => p.RentalContract).ThenInclude(c => c.Tenant)
            .Where(p => p.Status != RentPaymentStatus.Paid && p.DueDate <= thresholdDate)
            .ToListAsync(cancellationToken);

        foreach (var payment in latePayments)
        {
            var exists = await context.PaymentAlerts.AnyAsync(a => a.RentPaymentId == payment.Id && !a.IsAcknowledged, cancellationToken);
            if (exists)
            {
                continue;
            }

            payment.Status = RentPaymentStatus.Late;
            var alert = new PaymentAlert
            {
                RentPaymentId = payment.Id,
                Message = $"Payment {payment.ReceiptNumber} for tenant {payment.RentalContract.Tenant.FirstName} {payment.RentalContract.Tenant.LastName} is overdue.",
                AlertDate = DateTime.UtcNow
            };
            context.PaymentAlerts.Add(alert);
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
