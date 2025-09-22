using Microsoft.AspNetCore.Mvc;
using PropertyManager.Api.Contracts.Requests;
using PropertyManager.Api.Services.Interfaces;

namespace PropertyManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RentPaymentsController(IRentPaymentService paymentService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetUpcoming([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken)
    {
        var payments = await paymentService.GetUpcomingAsync(from, to, cancellationToken);
        return Ok(payments);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var payment = await paymentService.GetByIdAsync(id, cancellationToken);
        return payment is null ? NotFound() : Ok(payment);
    }

    [HttpGet("contract/{contractId:guid}")]
    public async Task<IActionResult> GetByContract(Guid contractId, CancellationToken cancellationToken)
    {
        var payments = await paymentService.GetByContractAsync(contractId, cancellationToken);
        return Ok(payments);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRentPaymentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var payment = await paymentService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = payment.Id }, payment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/record")]
    public async Task<IActionResult> Record(Guid id, [FromBody] RecordRentPaymentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var payment = await paymentService.RecordPaymentAsync(id, request, cancellationToken);
            return payment is null ? NotFound() : Ok(payment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id:guid}/receipt")]
    public async Task<IActionResult> DownloadReceipt(Guid id, CancellationToken cancellationToken)
    {
        var pdf = await paymentService.GenerateReceiptPdfAsync(id, cancellationToken);
        return File(pdf, "application/pdf", $"receipt-{id}.pdf");
    }

    [HttpPost("{id:guid}/send-receipt")]
    public async Task<IActionResult> SendReceipt(Guid id, [FromBody] SendReceiptRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await paymentService.SendReceiptAsync(id, request, cancellationToken);
            return result ? Accepted() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
