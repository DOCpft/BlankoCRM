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
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;
    private readonly IMapper _mapper;

    public AppointmentsController(IAppointmentService appointmentService, IMapper mapper)
    {
        _appointmentService = appointmentService;
        _mapper = mapper;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AppointmentDto>> GetById(Guid id)
    {
        var appointment = await _appointmentService.GetByIdAsync(id);
        if (appointment is null)
        {
            return NotFound();
        }

        return Ok(_mapper.Map<AppointmentDto>(appointment));
    }

    [HttpGet("all")]
    public async Task<ActionResult<List<AppointmentDto>>> GetAllAsync()
    {
        var appointments = await _appointmentService.GetAllAppointmentsAsync();
        return Ok(_mapper.Map<List<AppointmentDto>>(appointments));
    }

    [HttpGet("calendar")]
    public async Task<ActionResult<List<AppointmentDto>>> GetForDay([FromQuery] DateTime date)
    {
        var appointments = await _appointmentService.GetForDayAsync(date);
        var dtos = _mapper.Map<List<AppointmentDto>>(appointments);
        return Ok(dtos.ToList());
    }

    [HttpPost]
    public async Task<ActionResult<AppointmentDto>> Create([FromBody] CreateAppointmentRequest request)
    {
        var appointment = await _appointmentService.CreateAsync(
            request.ClientId,
            request.Services,
            request.StartTime,
            request.Notes,
            request.SalonId);

        return CreatedAtAction(nameof(GetById), new { id = appointment.Id }, _mapper.Map<AppointmentDto>(appointment));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAppointmentRequest request)
    {
        await _appointmentService.UpdateAsync(id, request.StartTime, request.Status, request.Notes);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _appointmentService.DeleteAsync(id);
        return NoContent();
    }

    [HttpPatch("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        await _appointmentService.CancelAsync(id);
        return NoContent();
    }
}

