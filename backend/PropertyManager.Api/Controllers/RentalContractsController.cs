using Microsoft.AspNetCore.Mvc;
using PropertyManager.Api.Contracts.Requests;
using PropertyManager.Api.Services.Interfaces;

namespace PropertyManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RentalContractsController(IRentalContractService contractService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var contracts = await contractService.GetAllAsync(cancellationToken);
        return Ok(contracts);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var contract = await contractService.GetByIdAsync(id, cancellationToken);
        return contract is null ? NotFound() : Ok(contract);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRentalContractRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var contract = await contractService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = contract.Id }, contract);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRentalContractRequest request, CancellationToken cancellationToken)
    {
        var contract = await contractService.UpdateAsync(id, request, cancellationToken);
        return contract is null ? NotFound() : Ok(contract);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await contractService.DeleteAsync(id, cancellationToken);
            return deleted ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}
