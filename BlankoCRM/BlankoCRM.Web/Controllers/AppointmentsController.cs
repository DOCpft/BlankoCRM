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
    public async Task<ActionResult<AppointmentDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentService.GetByIdAsync(id, cancellationToken);
        if (appointment is null)
        {
            return NotFound();
        }

        return Ok(_mapper.Map<AppointmentDto>(appointment));
    }

    [HttpGet("all")]
    public async Task<ActionResult<List<AppointmentDto>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var appointments = await _appointmentService.GetAllAppointmentsAsync(cancellationToken);
        return Ok(_mapper.Map<List<AppointmentDto>>(appointments));
    }

    [HttpGet("calendar")]
    public async Task<ActionResult<List<AppointmentDto>>> GetForDay([FromQuery] DateTime date, CancellationToken cancellationToken)
    {
        var appointments = await _appointmentService.GetForDayAsync(date, cancellationToken);
        var dtos = _mapper.Map<List<AppointmentDto>>(appointments);
        return Ok(dtos.ToList());
    }

    [HttpPost]
    public async Task<ActionResult<AppointmentDto>> Create([FromBody] CreateAppointmentRequest request, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentService.CreateAsync(
            request.ClientId,
            request.Services,
            request.StartTime,
            request.Notes,
            request.SalonId,
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = appointment.Id }, _mapper.Map<AppointmentDto>(appointment));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAppointmentRequest request, CancellationToken cancellationToken)
    {
        await _appointmentService.UpdateAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _appointmentService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        await _appointmentService.CancelAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        [FromBody] UpdateAppointmentStatusRequest request,
        CancellationToken cancellationToken)
    {
        await _appointmentService.UpdateStatusAsync(id, request.Status, cancellationToken);
        return NoContent();
    }
}

