using AutoMapper;
using BlankoCRM.Application.Abstract;
using BlankoCRM.Application.DTOs;
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
    private readonly IMapper _mapper;

    public PublicAppointmentsController(IPublicAppointmentService appointmentService, IMapper mapper)
    {
        _appointmentService = appointmentService;
        _mapper = mapper;
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
}
