using Microsoft.AspNetCore.Mvc;
using PropertyManager.Api.Services.Interfaces;

namespace PropertyManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlertsController(IAlertService alertService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetActive(CancellationToken cancellationToken)
    {
        var alerts = await alertService.GetActiveAlertsAsync(cancellationToken);
        return Ok(alerts);
    }

    [HttpPost("{id:guid}/acknowledge")]
    public async Task<IActionResult> Acknowledge(Guid id, CancellationToken cancellationToken)
    {
        await alertService.AcknowledgeAlertAsync(id, cancellationToken);
        return NoContent();
    }
}
