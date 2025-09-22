using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using PropertyManager.Api.Domain.Entities;
using PropertyManager.Api.Services.Interfaces;

namespace PropertyManager.Api.Services.Implementations;

public class PdfService : IPdfService
{
    static PdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public Task<byte[]> GenerateTestPdfAsync(CancellationToken cancellationToken = default)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Content().Column(col =>
                {
                    col.Item().Text("PDF TEST: QuestPDF generation is working").FontSize(16).Bold().FontFamily("Helvetica");
                });
            });
        });

        return Task.FromResult(document.GeneratePdf());
    }

    public Task<byte[]> GenerateRentReceiptAsync(RentPayment payment, CancellationToken cancellationToken = default)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Content().Column(col =>
                {
                    col.Item().Text("Rent Payment Receipt").FontSize(20).Bold();
                    col.Item().Text($"Receipt #: {payment.ReceiptNumber}");
                    col.Item().Text($"Tenant: {payment.RentalContract.Tenant.FirstName} {payment.RentalContract.Tenant.LastName}");
                    col.Item().Text($"Unit: {payment.RentalContract.RentalUnit.Name}");
                    col.Item().Text($"Due Date: {payment.DueDate:MMMM dd, yyyy}");
                    col.Item().Text($"Paid Date: {(payment.PaidDate.HasValue ? payment.PaidDate.Value.ToString("MMMM dd, yyyy") : "Pending")}");
                    col.Item().Text($"Amount Due: {CurrencyFormatter.FormatAmount(payment.AmountDue, payment.Currency)}");
                    col.Item().Text($"Amount Paid: {CurrencyFormatter.FormatAmount(payment.AmountPaid, payment.Currency)}");
                    if (payment.LateFee.HasValue)
                    {
                        col.Item().Text($"Late Fee: {CurrencyFormatter.FormatAmount(payment.LateFee.Value, payment.Currency)}");
                    }
                    col.Item().Text($"Status: {payment.Status}");
                    if (!string.IsNullOrWhiteSpace(payment.Notes))
                    {
                        col.Item().PaddingTop(10).Text($"Notes: {payment.Notes}");
                    }
                });
            });
        });

        return Task.FromResult(document.GeneratePdf());
    }

    public Task<byte[]> GeneratePaymentsReportAsync(IEnumerable<RentPayment> payments, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var paymentList = payments.ToList();
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Content().Column(col =>
                {
                    // Add a small debug header and force a common font family for text clarity
                    col.Item().Text("DEBUG: PAYMENTS REPORT GENERATED").FontSize(8).FontFamily("Helvetica");
                    col.Item().Text("Rent Payments Report").FontSize(20).Bold().FontFamily("Helvetica");
                    col.Item().Text($"Period: {from:MMMM dd, yyyy} - {to:MMMM dd, yyyy}").FontFamily("Helvetica");
                    // Fallback simple layout: list each payment as several lines to guarantee text rendering
                    foreach (var payment in paymentList)
                    {
                        col.Item().Text($"Receipt: {payment.ReceiptNumber}").FontFamily("Helvetica");
                        col.Item().Text($"Tenant: {payment.RentalContract.Tenant.FirstName} {payment.RentalContract.Tenant.LastName}").FontFamily("Helvetica");
                        col.Item().Text($"Unit: {payment.RentalContract.RentalUnit.Name}").FontFamily("Helvetica");
                        col.Item().Text($"Due: {payment.DueDate:MM/dd/yyyy}").FontFamily("Helvetica");
                        col.Item().Text($"Paid: {payment.PaidDate?.ToString("MM/dd/yyyy") ?? "-"}").FontFamily("Helvetica");
                        col.Item().Text($"Status: {payment.Status}").FontFamily("Helvetica");
                        col.Item().PaddingTop(6);
                    }

                    // Totals grouped by currency
                    var totalsByCurrency = paymentList.GroupBy(p => p.Currency)
                        .Select(g => new
                        {
                            Currency = g.Key,
                            TotalDue = g.Sum(p => p.AmountDue),
                            TotalCollected = g.Sum(p => p.AmountPaid)
                        })
                        .ToList();

                    col.Item().PaddingTop(15).Text("Totals:").FontSize(12).Bold();
                    foreach (var t in totalsByCurrency)
                    {
                        col.Item().Text($"{CurrencyFormatter.ToCode(t.Currency)} - Total Due: {CurrencyFormatter.FormatAmount(t.TotalDue, t.Currency)}");
                        col.Item().Text($"{CurrencyFormatter.ToCode(t.Currency)} - Collected: {CurrencyFormatter.FormatAmount(t.TotalCollected, t.Currency)}");
                        col.Item().Text($"{CurrencyFormatter.ToCode(t.Currency)} - Outstanding: {CurrencyFormatter.FormatAmount(t.TotalDue - t.TotalCollected, t.Currency)}");
                    }
                });
            });
        });

        return Task.FromResult(document.GeneratePdf());
    }
}
