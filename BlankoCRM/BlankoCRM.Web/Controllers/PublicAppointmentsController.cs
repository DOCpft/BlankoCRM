using AutoMapper;
using BlankoCRM.Application.Abstract;
using BlankoCRM.Application.DTOs;
using BlankoCRM.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BlankoCRM.Web.Controllers;

/// <summary>
/// Публичные клиентские заявки через VK Mini App.
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/public/{organizationSlug}/appointments")]
public sealed class PublicAppointmentsController : ControllerBase
{
    private readonly IPublicAppointmentService _appointmentService;
    private readonly IPublicScheduleService _scheduleService;
    private readonly IMapper _mapper;

    public PublicAppointmentsController(
        IPublicAppointmentService appointmentService,
        IPublicScheduleService scheduleService,
        IMapper mapper)
    {
        _appointmentService = appointmentService;
        _scheduleService = scheduleService;
        _mapper = mapper;
    }

    [HttpGet("slots")]
    [EnableRateLimiting("public-appointment-read")]
    [ProducesResponseType(typeof(IReadOnlyList<PublicFreeSlotDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PublicFreeSlotDto>>> GetFreeSlots(
        [FromQuery] DateTimeOffset from,
        [FromQuery] DateTimeOffset to,
        CancellationToken cancellationToken,
        [FromQuery] int limit = 100)
    {
        var slots = await _scheduleService.GetFreeSlotsAsync(
            from.UtcDateTime,
            to.UtcDateTime,
            limit,
            cancellationToken);
        return Ok(slots);
    }

    [HttpPost("mine")]
    [EnableRateLimiting("public-appointment-read")]
    [ProducesResponseType(typeof(IReadOnlyList<AppointmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<AppointmentDto>>> GetMyAppointments(
        [FromBody] VkMiniAppAuthRequest request,
        CancellationToken cancellationToken)
    {
        var appointments = await _appointmentService.GetMyAppointmentsAsync(
            request.VkParams,
            cancellationToken);
        return Ok(_mapper.Map<List<AppointmentDto>>(appointments));
    }

    /// <summary>
    /// Создать заявку, ожидающую подтверждения мастером.
    /// </summary>
    [HttpPost]
    [EnableRateLimiting("public-appointment-create")]
    [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AppointmentDto>> Create(
        [FromBody] CreatePublicAppointmentRequest request,
        CancellationToken cancellationToken)
    {
        var appointment = await _appointmentService.CreatePendingAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, _mapper.Map<AppointmentDto>(appointment));
    }

    /// <summary>
    /// Отозвать собственную Pending-заявку после проверки подписи VK.
    /// </summary>
    [HttpPost("{id:guid}/withdraw")]
    [EnableRateLimiting("public-appointment-withdraw")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Withdraw(
        Guid id,
        [FromBody] VkMiniAppAuthRequest request,
        CancellationToken cancellationToken)
    {
        await _appointmentService.WithdrawAsync(id, request.VkParams, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/cancel")]
    [EnableRateLimiting("public-appointment-withdraw")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(
        Guid id,
        [FromBody] VkMiniAppAuthRequest request,
        CancellationToken cancellationToken)
    {
        await _appointmentService.CancelAsync(id, request.VkParams, cancellationToken);
        return NoContent();
    }
}
