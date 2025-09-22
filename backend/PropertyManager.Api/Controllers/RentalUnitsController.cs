using Microsoft.AspNetCore.Mvc;
using PropertyManager.Api.Contracts.Requests;
using PropertyManager.Api.Services.Interfaces;

namespace PropertyManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RentalUnitsController(IRentalUnitService rentalUnitService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var units = await rentalUnitService.GetAllAsync(cancellationToken);
        return Ok(units);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var unit = await rentalUnitService.GetByIdAsync(id, cancellationToken);
        return unit is null ? NotFound() : Ok(unit);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRentalUnitRequest request, CancellationToken cancellationToken)
    {
        var unit = await rentalUnitService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = unit.Id }, unit);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRentalUnitRequest request, CancellationToken cancellationToken)
    {
        var unit = await rentalUnitService.UpdateAsync(id, request, cancellationToken);
        return unit is null ? NotFound() : Ok(unit);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await rentalUnitService.DeleteAsync(id, cancellationToken);
            return deleted ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}
