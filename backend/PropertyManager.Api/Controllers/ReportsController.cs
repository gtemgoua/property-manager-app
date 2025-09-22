using Microsoft.AspNetCore.Mvc;
using PropertyManager.Api.Services.Interfaces;

namespace PropertyManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(IReportService reportService, ILogger<ReportsController> logger)
    {
        _reportService = reportService;
        _logger = logger;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken)
    {
        var end = to ?? DateTime.UtcNow.Date;
        var start = from ?? end.AddMonths(-12);
        var metrics = await _reportService.GetDashboardMetricsAsync(start, end, cancellationToken);
        return Ok(metrics);
    }

    [HttpGet("payments/excel")]
    public async Task<IActionResult> GetPaymentsExcel([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] Domain.Enums.Currency? currency, CancellationToken cancellationToken)
    {
        var end = (to ?? DateTime.UtcNow.Date).ToUniversalTime();
        var start = (from ?? end.AddMonths(-3)).ToUniversalTime();
        var file = await _reportService.GeneratePaymentsReportExcelAsync(start, end, currency, cancellationToken);
        var currencySuffix = currency.HasValue ? $"-{currency.Value}" : string.Empty;
        var excelFilename = $"payments-{start:yyyyMMdd}-{end:yyyyMMdd}{currencySuffix}.xlsx";
        return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelFilename);
    }

    [HttpGet("payments/pdf")]
    public async Task<IActionResult> GetPaymentsPdf([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] Domain.Enums.Currency? currency, CancellationToken cancellationToken)
    {
        var end = (to ?? DateTime.UtcNow.Date).ToUniversalTime();
        var start = (from ?? end.AddMonths(-3)).ToUniversalTime();
        try
        {
            var file = await _reportService.GeneratePaymentsReportPdfAsync(start, end, currency, cancellationToken);
            var currencySuffix = currency.HasValue ? $"-{currency.Value}" : string.Empty;
            var pdfFilename = $"payments-{start:yyyyMMdd}-{end:yyyyMMdd}{currencySuffix}.pdf";
            return File(file, "application/pdf", pdfFilename);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("PDF generation cancelled for range {Start} - {End}", start, end);
            return StatusCode(503, "PDF generation was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating payments PDF for range {Start} - {End}", start, end);
            // Return a ProblemDetails response with a short error message (no stacktrace in production)
            return Problem(detail: "An error occurred while generating the PDF. Check server logs for details.", statusCode: 500);
        }
    }

    [HttpGet("testpdf")]
    public async Task<IActionResult> GetTestPdf(CancellationToken cancellationToken)
    {
        var file = await _reportService.GenerateTestPaymentsPdfAsync(cancellationToken);
        return File(file, "application/pdf", "test.pdf");
    }
}
