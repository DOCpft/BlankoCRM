using BlankoCRM.Application.Abstract;
using BlankoCRM.Application.DTOs;
using BlankoCRM.Application.DTOs.Mapping;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlankoCRM.Web.Controllers;

/// <summary>
/// Управление записями на приём (назначенные услуги клиентам).
/// </summary>
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

    /// <summary>
    /// Получить запись по ID.
    /// </summary>
    /// <param name="id">Идентификатор записи (GUID).</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <response code="200">Возвращает найденную запись.</response>
    /// <response code="401">Пользователь не авторизован.</response>
    /// <response code="404">Запись с указанным ID не найдена.</response>
    [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

    /// <summary>
    /// Получить все записи текущей организации.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <response code="200">Возвращает список всех записей.</response>
    /// <response code="401">Пользователь не авторизован.</response>
    [ProducesResponseType(typeof(List<AppointmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpGet("all")]
    public async Task<ActionResult<List<AppointmentDto>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var appointments = await _appointmentService.GetAllAppointmentsAsync(cancellationToken);
        return Ok(_mapper.Map<List<AppointmentDto>>(appointments));
    }

    /// <summary>
    /// Получить записи за конкретный день (календарь).
    /// </summary>
    /// <param name="date">Дата, за которую требуется получить записи.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <response code="200">Возвращает список записей за указанный день.</response>
    /// <response code="401">Пользователь не авторизован.</response>
    [ProducesResponseType(typeof(List<AppointmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpGet("calendar")]
    public async Task<ActionResult<List<AppointmentDto>>> GetForDay([FromQuery] DateTime date, CancellationToken cancellationToken)
    {
        var appointments = await _appointmentService.GetForDayAsync(date, cancellationToken);
        var dtos = _mapper.Map<List<AppointmentDto>>(appointments);
        return Ok(dtos.ToList());
    }

    /// <summary>
    /// Создать новую запись на приём.
    /// </summary>
    /// <param name="request">Данные для создания записи.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <response code="201">Запись успешно создана, возвращает созданную запись.</response>
    /// <response code="400">Некорректные входные данные.</response>
    /// <response code="401">Пользователь не авторизован.</response>
    [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

    /// <summary>
    /// Обновить существующую запись.
    /// </summary>
    /// <param name="id">Идентификатор записи (GUID).</param>
    /// <param name="request">Данные для обновления записи.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <response code="204">Запись успешно обновлена (без тела ответа).</response>
    /// <response code="400">Некорректные входные данные.</response>
    /// <response code="401">Пользователь не авторизован.</response>
    /// <response code="404">Запись с указанным ID не найдена.</response>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAppointmentRequest request, CancellationToken cancellationToken)
    {
        await _appointmentService.UpdateAsync(id, request, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Удалить запись.
    /// </summary>
    /// <param name="id">Идентификатор записи (GUID).</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <response code="204">Запись успешно удалена (без тела ответа).</response>
    /// <response code="401">Пользователь не авторизован.</response>
    /// <response code="404">Запись с указанным ID не найдена.</response>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _appointmentService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Отменить запись.
    /// </summary>
    /// <param name="id">Идентификатор записи (GUID).</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <response code="204">Запись успешно отменена (без тела ответа).</response>
    /// <response code="401">Пользователь не авторизован.</response>
    /// <response code="404">Запись с указанным ID не найдена.</response>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpPatch("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        await _appointmentService.CancelAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet("pending")]
    [ProducesResponseType(typeof(List<AppointmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<AppointmentDto>>> GetPending(CancellationToken cancellationToken)
    {
        var appointments = await _appointmentService.GetPendingAppointmentsAsync(cancellationToken);
        return Ok(_mapper.Map<List<AppointmentDto>>(appointments));
    }

    [HttpPost("{id:guid}/confirm")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Confirm(Guid id, CancellationToken cancellationToken)
    {
        await _appointmentService.ConfirmAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Reject(Guid id, CancellationToken cancellationToken)
    {
        await _appointmentService.RejectAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Complete(Guid id, CancellationToken cancellationToken)
    {
        await _appointmentService.CompleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/no-show")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> MarkNoShow(Guid id, CancellationToken cancellationToken)
    {
        await _appointmentService.MarkNoShowAsync(id, cancellationToken);
        return NoContent();
    }
}