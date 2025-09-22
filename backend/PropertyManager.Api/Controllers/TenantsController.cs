using Microsoft.AspNetCore.Mvc;
using PropertyManager.Api.Contracts.Requests;
using PropertyManager.Api.Services.Interfaces;

namespace PropertyManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TenantsController(ITenantService tenantService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var tenants = await tenantService.GetAllAsync(cancellationToken);
        return Ok(tenants);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var tenant = await tenantService.GetByIdAsync(id, cancellationToken);
        return tenant is null ? NotFound() : Ok(tenant);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTenantRequest request, CancellationToken cancellationToken)
    {
        var tenant = await tenantService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = tenant.Id }, tenant);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTenantRequest request, CancellationToken cancellationToken)
    {
        var tenant = await tenantService.UpdateAsync(id, request, cancellationToken);
        return tenant is null ? NotFound() : Ok(tenant);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await tenantService.DeleteAsync(id, cancellationToken);
            return deleted ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}
