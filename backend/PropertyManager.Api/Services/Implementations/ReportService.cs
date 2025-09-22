using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using PropertyManager.Api.Contracts.Responses;
using PropertyManager.Api.Data;
using PropertyManager.Api.Services.Interfaces;

namespace PropertyManager.Api.Services.Implementations;

public class ReportService(ApplicationDbContext context, IPdfService pdfService) : IReportService
{
    public async Task<DashboardMetricsResponse> GetDashboardMetricsAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var tenantsCount = await context.Tenants.CountAsync(cancellationToken);
        var unitsCount = await context.RentalUnits.CountAsync(cancellationToken);
        var occupiedUnits = await context.RentalUnits.CountAsync(u => u.Status == Domain.Enums.RentalUnitStatus.Occupied, cancellationToken);
        var vacantUnits = unitsCount - occupiedUnits;

        var payments = await context.RentPayments
            .Include(p => p.RentalContract).ThenInclude(c => c.RentalUnit)
            .Where(p => p.DueDate >= from && p.DueDate <= to)
            .ToListAsync(cancellationToken);

        var monthlyRevenue = payments.Where(p => p.DueDate.Month == DateTime.UtcNow.Month && p.DueDate.Year == DateTime.UtcNow.Year)
            .Sum(p => p.AmountDue);
        var monthlyCollected = payments.Where(p => p.PaidDate.HasValue && p.PaidDate.Value.Month == DateTime.UtcNow.Month && p.PaidDate.Value.Year == DateTime.UtcNow.Year)
            .Sum(p => p.AmountPaid);
        var monthlyOutstanding = monthlyRevenue - monthlyCollected;

        var rentCollection = payments
            .GroupBy(p => new { p.DueDate.Year, p.DueDate.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new RentCollectionChartPoint(
                g.Key.Year,
                g.Key.Month,
                g.Sum(p => p.AmountDue),
                g.Sum(p => p.AmountPaid)))
            .ToList();

        var occupancy = await context.RentalContracts
            .Where(c => c.StartDate <= to && (c.EndDate == null || c.EndDate >= from))
            .GroupBy(c => new { c.StartDate.Year, c.StartDate.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new OccupancyChartPoint(
                g.Key.Year,
                g.Key.Month,
                g.Count(c => c.Status == Domain.Enums.ContractStatus.Active),
                unitsCount - g.Count(c => c.Status == Domain.Enums.ContractStatus.Active)))
            .ToListAsync(cancellationToken);

        return new DashboardMetricsResponse(
            tenantsCount,
            unitsCount,
            occupiedUnits,
            vacantUnits,
            monthlyRevenue,
            monthlyCollected,
            monthlyOutstanding,
            rentCollection,
            occupancy
        );
    }

    public async Task<byte[]> GeneratePaymentsReportExcelAsync(DateTime from, DateTime to, Domain.Enums.Currency? currency = null, CancellationToken cancellationToken = default)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        var paymentsBase = context.RentPayments
            .Include(p => p.RentalContract).ThenInclude(c => c.Tenant)
            .Include(p => p.RentalContract).ThenInclude(c => c.RentalUnit)
            .Where(p => p.DueDate >= from && p.DueDate <= to);

        if (currency.HasValue)
        {
            paymentsBase = paymentsBase.Where(p => p.Currency == currency.Value);
        }

        var payments = await paymentsBase.OrderBy(p => p.DueDate).ToListAsync(cancellationToken);

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Payments");

        worksheet.Cells[1, 1].Value = "Receipt";
        worksheet.Cells[1, 2].Value = "Tenant";
        worksheet.Cells[1, 3].Value = "Unit";
        worksheet.Cells[1, 4].Value = "Due";
        worksheet.Cells[1, 5].Value = "Paid";
    worksheet.Cells[1, 6].Value = "Amount Due";
    worksheet.Cells[1, 7].Value = "Amount Paid";
    worksheet.Cells[1, 8].Value = "Late Fee";
    worksheet.Cells[1, 9].Value = "Status";
    worksheet.Cells[1, 10].Value = "Currency";

        var row = 2;
        foreach (var payment in payments)
        {
            worksheet.Cells[row, 1].Value = payment.ReceiptNumber;
            worksheet.Cells[row, 2].Value = $"{payment.RentalContract.Tenant.FirstName} {payment.RentalContract.Tenant.LastName}";
            worksheet.Cells[row, 3].Value = payment.RentalContract.RentalUnit.Name;
            worksheet.Cells[row, 4].Value = payment.DueDate.ToShortDateString();
            worksheet.Cells[row, 5].Value = payment.PaidDate?.ToShortDateString();
            worksheet.Cells[row, 6].Value = (double)payment.AmountDue;
            worksheet.Cells[row, 6].Style.Numberformat.Format = CurrencyFormatter.ToExcelNumberFormat(payment.Currency);

            worksheet.Cells[row, 7].Value = (double)payment.AmountPaid;
            worksheet.Cells[row, 7].Style.Numberformat.Format = CurrencyFormatter.ToExcelNumberFormat(payment.Currency);

            worksheet.Cells[row, 8].Value = payment.LateFee.HasValue ? (double)payment.LateFee.Value : null;
            if (payment.LateFee.HasValue)
                worksheet.Cells[row, 8].Style.Numberformat.Format = CurrencyFormatter.ToExcelNumberFormat(payment.Currency);

            worksheet.Cells[row, 9].Value = payment.Status.ToString();
            worksheet.Cells[row, 10].Value = CurrencyFormatter.ToCode(payment.Currency);
            row++;
        }

        worksheet.Cells.AutoFitColumns();
        return package.GetAsByteArray();
    }

    public async Task<byte[]> GeneratePaymentsReportPdfAsync(DateTime from, DateTime to, Domain.Enums.Currency? currency = null, CancellationToken cancellationToken = default)
    {
        var paymentsBasePdf = context.RentPayments
            .Include(p => p.RentalContract).ThenInclude(c => c.Tenant)
            .Include(p => p.RentalContract).ThenInclude(c => c.RentalUnit)
            .Where(p => p.DueDate >= from && p.DueDate <= to);

        if (currency.HasValue)
        {
            paymentsBasePdf = paymentsBasePdf.Where(p => p.Currency == currency.Value);
        }

        var payments = await paymentsBasePdf.OrderBy(p => p.DueDate).ToListAsync(cancellationToken);

        return await pdfService.GeneratePaymentsReportAsync(payments, from, to, cancellationToken);
    }

    public Task<byte[]> GenerateTestPaymentsPdfAsync(CancellationToken cancellationToken = default)
    {
        return pdfService.GenerateTestPdfAsync(cancellationToken);
    }
}
