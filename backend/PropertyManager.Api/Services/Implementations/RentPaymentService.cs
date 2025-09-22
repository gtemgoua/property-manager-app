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

public class RentPaymentService(
    ApplicationDbContext context,
    IMapper mapper,
    IPdfService pdfService,
    IEmailService emailService) : IRentPaymentService
{
    public async Task<IEnumerable<RentPaymentResponse>> GetUpcomingAsync(DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
    {
        var query = context.RentPayments
            .AsNoTracking()
            .Include(p => p.RentalContract).ThenInclude(c => c.Tenant)
            .Include(p => p.RentalContract).ThenInclude(c => c.RentalUnit)
            .AsQueryable();

        if (from.HasValue)
        {
            query = query.Where(p => p.DueDate >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(p => p.DueDate <= to.Value);
        }

        return await query
            .OrderBy(p => p.DueDate)
            .ProjectTo<RentPaymentResponse>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<RentPaymentResponse>> GetByContractAsync(Guid contractId, CancellationToken cancellationToken = default)
    {
        return await context.RentPayments
            .AsNoTracking()
            .Where(p => p.RentalContractId == contractId)
            .Include(p => p.RentalContract).ThenInclude(c => c.Tenant)
            .Include(p => p.RentalContract).ThenInclude(c => c.RentalUnit)
            .OrderByDescending(p => p.DueDate)
            .ProjectTo<RentPaymentResponse>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }

    public async Task<RentPaymentResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.RentPayments
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Include(p => p.RentalContract).ThenInclude(c => c.Tenant)
            .Include(p => p.RentalContract).ThenInclude(c => c.RentalUnit)
            .ProjectTo<RentPaymentResponse>(mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<RentPaymentResponse> CreateAsync(CreateRentPaymentRequest request, CancellationToken cancellationToken = default)
    {
        var contract = await context.RentalContracts
            .Include(c => c.Tenant)
            .Include(c => c.RentalUnit)
            .FirstOrDefaultAsync(c => c.Id == request.RentalContractId, cancellationToken);

        if (contract is null)
        {
            throw new InvalidOperationException("Contract not found.");
        }

        // Prevent duplicate payments for the same contract and due date
        var exists = await context.RentPayments
            .AsNoTracking()
            .AnyAsync(p => p.RentalContractId == contract.Id && p.DueDate == request.DueDate, cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException("A rent payment for the given contract and due date already exists.");
        }

        var entity = new RentPayment
        {
            RentalContractId = contract.Id,
            DueDate = request.DueDate,
            AmountDue = request.AmountDue,
            AmountPaid = 0,
            LateFee = 0,
            Status = RentPaymentStatus.Pending,
            Notes = request.Notes,
            ReceiptNumber = GenerateReceiptNumber(contract),
            Currency = request.Currency == default ? contract.Currency : request.Currency
        };

        context.RentPayments.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        entity = await context.RentPayments
            .Include(p => p.RentalContract).ThenInclude(c => c.Tenant)
            .Include(p => p.RentalContract).ThenInclude(c => c.RentalUnit)
            .SingleAsync(p => p.Id == entity.Id, cancellationToken);

        return mapper.Map<RentPaymentResponse>(entity);
    }

    public async Task<RentPaymentResponse?> RecordPaymentAsync(Guid paymentId, RecordRentPaymentRequest request, CancellationToken cancellationToken = default)
    {
        var payment = await context.RentPayments
            .Include(p => p.RentalContract).ThenInclude(c => c.Tenant)
            .Include(p => p.RentalContract).ThenInclude(c => c.RentalUnit)
            .FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken);

        if (payment is null)
        {
            return null;
        }

        payment.AmountPaid = request.AmountPaid;
        payment.PaidDate = request.PaidDate;
        payment.PaymentMethod = request.PaymentMethod;
        payment.ReferenceNumber = request.ReferenceNumber;
        payment.LateFee = request.LateFee;
        payment.Notes = request.Notes;
        payment.UpdatedAt = DateTime.UtcNow;
        payment.Status = DeriveStatus(payment);

        await context.SaveChangesAsync(cancellationToken);

        return mapper.Map<RentPaymentResponse>(payment);
    }

    public async Task<byte[]> GenerateReceiptPdfAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        var payment = await context.RentPayments
            .Include(p => p.RentalContract).ThenInclude(c => c.Tenant)
            .Include(p => p.RentalContract).ThenInclude(c => c.RentalUnit)
            .FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken) ?? throw new InvalidOperationException("Payment not found");

        var pdfBytes = await pdfService.GenerateRentReceiptAsync(payment, cancellationToken);
        await PersistDocumentAsync(payment, pdfBytes, "application/pdf", cancellationToken);
        return pdfBytes;
    }

    public async Task<bool> SendReceiptAsync(Guid paymentId, SendReceiptRequest request, CancellationToken cancellationToken = default)
    {
        var payment = await context.RentPayments
            .Include(p => p.RentalContract).ThenInclude(c => c.Tenant)
            .Include(p => p.RentalContract).ThenInclude(c => c.RentalUnit)
            .FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken);

        if (payment is null)
        {
            return false;
        }

        if (payment.Status is not RentPaymentStatus.Paid and not RentPaymentStatus.Partial)
        {
            throw new InvalidOperationException("Cannot send receipt for unpaid invoice.");
        }

        byte[]? pdfBytes = null;
        if (request.AttachPdf)
        {
            pdfBytes = await pdfService.GenerateRentReceiptAsync(payment, cancellationToken);
            await PersistDocumentAsync(payment, pdfBytes, "application/pdf", cancellationToken);
        }

        var subject = $"Rent receipt {payment.ReceiptNumber}";
        var body = $"<p>Dear {request.RecipientName ?? payment.RentalContract.Tenant.FirstName},</p>" +
                   "<p>Please find your rent receipt attached. Thank you for your payment.</p>" +
                   $"<p><strong>Amount Paid:</strong> {payment.AmountPaid:C}<br/>" +
                   $"<strong>Due Date:</strong> {payment.DueDate:MMMM dd, yyyy}<br/>" +
                   $"<strong>Paid Date:</strong> {payment.PaidDate:MMMM dd, yyyy}</p>";

        await emailService.SendEmailAsync(
            request.RecipientEmail,
            subject,
            body,
            pdfBytes,
            pdfBytes is not null ? $"{payment.ReceiptNumber}.pdf" : null,
            pdfBytes is not null ? "application/pdf" : null,
            cancellationToken);

        payment.ReceiptSent = true;
        payment.ReceiptSentAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static RentPaymentStatus DeriveStatus(RentPayment payment)
    {
        if (payment.AmountPaid <= 0)
        {
            return payment.DueDate.Date < DateTime.UtcNow.Date ? RentPaymentStatus.Late : RentPaymentStatus.Pending;
        }

        if (payment.AmountPaid < payment.AmountDue)
        {
            return payment.DueDate.Date < DateTime.UtcNow.Date ? RentPaymentStatus.Late : RentPaymentStatus.Partial;
        }

        return RentPaymentStatus.Paid;
    }

    private static string GenerateReceiptNumber(RentalContract contract) =>
        $"RCPT-{contract.Id.ToString().Substring(0, 8).ToUpperInvariant()}-{DateTime.UtcNow:yyyyMMddHHmm}";

    private async Task PersistDocumentAsync(RentPayment payment, byte[] pdfBytes, string contentType, CancellationToken cancellationToken)
    {
        var document = new DocumentLog
        {
            DocumentType = DocumentType.Receipt,
            FileName = $"{payment.ReceiptNumber}.pdf",
            ContentType = contentType,
            Content = pdfBytes,
            RentPaymentId = payment.Id,
            Metadata = $"{{\"tenant\":\"{payment.RentalContract.Tenant.FirstName} {payment.RentalContract.Tenant.LastName}\"}}"
        };

        context.DocumentLogs.Add(document);
        await context.SaveChangesAsync(cancellationToken);
    }
}
