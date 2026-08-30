using BlankoCRM.Application.Abstract;
using BlankoCRM.Application.DTOs;
using BlankoCRM.Application.DTOs.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlankoCRM.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AppointmentDto>> GetById(Guid id)
    {
        var appointment = await _appointmentService.GetByIdAsync(id);
        if (appointment is null)
        {
            return NotFound();
        }

        return Ok(appointment.ToDto());
    }

    [HttpGet("all")]
    public async Task<ActionResult<List<AppointmentDto>>> GetAllAsync()
    {
        var appointments = await _appointmentService.GetAllAppointmentsAsync();
        return Ok(appointments.Select(a => a.ToDto()).ToList());
    }

    [HttpGet("calendar")]
    public async Task<ActionResult<List<AppointmentDto>>> GetForDay([FromQuery] DateTime date)
    {
        var appointments = await _appointmentService.GetForDayAsync(date);
        var dtos = appointments.Select(a => a.ToDto());
        return Ok(dtos.ToList());
    }

    [HttpPost]
    public async Task<ActionResult<AppointmentDto>> Create([FromBody] CreateAppointmentRequest request)
    {
        var appointment = await _appointmentService.CreateAsync(
            request.ClientId,
            request.Services,
            request.StartTime,
            request.Notes);

        return CreatedAtAction(nameof(GetById), new { id = appointment.Id }, appointment.ToDto());
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAppointmentRequest request)
    {
        try
        {
            await _appointmentService.UpdateAsync(id, request.StartTime, request.Status, request.Notes);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
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
