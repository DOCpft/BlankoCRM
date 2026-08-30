using BlankoCRM.Application.Abstract;
using BlankoCRM.Application.DTOs;
using BlankoCRM.Application.DTOs.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlankoCRM.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ServicesController : ControllerBase
{
    private readonly IServicesService _servicesService;

    public ServicesController(IServicesService servicesService)
    {
        _servicesService = servicesService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ServiceDto>>> GetAll()
    {
        var services = await _servicesService.GetAllAsync();
        if (services.Count == 0) return NotFound();
        var dtos = services.Select(s => s.ToDto());
        return Ok(dtos);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ServiceDto>> GetById(Guid id)
    {
        var service = await _servicesService.GetByIdAsync(id);

        return Ok(service!.ToDto());
    }

    [HttpPost]
    public async Task<ActionResult<ServiceDto>> Create([FromBody] CreateServiceRequest request)
    {

        var service = await _servicesService.CreateAsync(
            request.Name,
            request.DurationMinutes,
            request.Price,
            request.Description);

        return CreatedAtAction(nameof(GetById), new { id = service.Id }, service.ToDto());
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateServiceRequest request)
    {

        await _servicesService.UpdateAsync(
            id,
            request.Name,
            request.DurationMinutes,
            request.Price,
            request.Description);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _servicesService.DeleteAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }
}
