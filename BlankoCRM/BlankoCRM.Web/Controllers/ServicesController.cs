using BlankoCRM.Application.Abstract;
using BlankoCRM.Application.DTOs;
using BlankoCRM.Application.DTOs.Mapping;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlankoCRM.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ServicesController : ControllerBase
{
    private readonly IServicesService _servicesService;
    private readonly IMapper _mapper;

    public ServicesController(IServicesService servicesService, IMapper mapper)
    {
        _servicesService = servicesService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<List<ServiceDto>>> GetAll()
    {
        var services = await _servicesService.GetAllAsync();
        var dtos = _mapper.Map<List<ServiceDto>>(services);
        return Ok(dtos);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ServiceDto>> GetById(Guid id)
    {
        var service = await _servicesService.GetByIdAsync(id);

        return Ok(_mapper.Map<ServiceDto>(service));
    }

    [HttpPost]
    public async Task<ActionResult<ServiceDto>> Create([FromBody] CreateServiceRequest request)
    {

        var service = await _servicesService.CreateAsync(
            request.Name,
            request.DurationMinutes,
            request.Price,
            request.Description,
            request.SalonId);

        return CreatedAtAction(nameof(GetById), new { id = service.Id }, _mapper.Map<ServiceDto>(service));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateServiceRequest request)
    {

        await _servicesService.UpdateAsync(
            id,
            request.Name,
            request.DurationMinutes,
            request.Price,
            request.Description,
            request.SalonId);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _servicesService.DeleteAsync(id);
        return NoContent();
    }
}

