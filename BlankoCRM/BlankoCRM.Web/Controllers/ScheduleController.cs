using BlankoCRM.Application.Abstract;
using BlankoCRM.Application.DTOs;
using BlankoCRM.Application.DTOs.Mapping;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlankoCRM.Web.Controllers;

/// <summary>
/// Управление слотами расписания (свободные и занятые интервалы).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ScheduleController : ControllerBase
{
    private readonly IScheduleService _scheduleService;
    private readonly IMapper _mapper;

    public ScheduleController(IScheduleService scheduleService, IMapper mapper)
    {
        _scheduleService = scheduleService;
        _mapper = mapper;
    }

    /// <summary>
    /// Получить слот расписания по ID.
    /// </summary>
    /// <param name="id">Идентификатор слота (GUID).</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <response code="200">Возвращает найденный слот.</response>
    /// <response code="401">Пользователь не авторизован.</response>
    /// <response code="404">Слот с указанным ID не найден.</response>
    [ProducesResponseType(typeof(ScheduleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("{id:guid}", Name = "GetSlotById")]
    public async Task<ActionResult<ScheduleDto>> GetSlotByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var slot = await _scheduleService.GetSlotByIdAsync(id, cancellationToken);

        return Ok(_mapper.Map<ScheduleDto>(slot));
    }

    /// <summary>
    /// Получить расписание за указанный период.
    /// </summary>
    /// <param name="dtFrom">Начало периода.</param>
    /// <param name="dtTo">Конец периода.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <response code="200">Возвращает список слотов за период.</response>
    /// <response code="400">Дата начала больше или равна дате конца периода.</response>
    /// <response code="401">Пользователь не авторизован.</response>
    [ProducesResponseType(typeof(List<ScheduleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpGet("get/period")]
    public async Task<ActionResult<List<ScheduleDto>>> GetScheduleByPeriod(
        [FromQuery] DateTime dtFrom,
        [FromQuery] DateTime dtTo, CancellationToken cancellationToken)
    {
        if (dtFrom >= dtTo)
            return BadRequest("Дата начала должна быть меньше даты конца периода.");

        var schedule = await _scheduleService.GetScheduleByPeriodAsync(dtFrom, dtTo, cancellationToken);

        var dtos = _mapper.Map<List<ScheduleDto>>(schedule);

        return Ok(dtos);
    }

    /// <summary>
    /// Добавить свободный слот в расписание.
    /// </summary>
    /// <param name="request">Данные свободного слота.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <response code="201">Слот успешно создан, возвращает созданный слот.</response>
    /// <response code="400">Некорректные входные данные.</response>
    /// <response code="401">Пользователь не авторизован.</response>
    [ProducesResponseType(typeof(ScheduleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpPost("free")]
    public async Task<ActionResult<ScheduleDto>> AddFreeSlotAsync([FromBody] CreateFreeSlotRequest request, CancellationToken cancellationToken)
    {
        var slot = await _scheduleService.AddFreeSlotAsync(request, cancellationToken);
        return CreatedAtAction("GetSlotById", new { id = slot.Id }, _mapper.Map<ScheduleDto>(slot));
    }

    /// <summary>
    /// Добавить занятый слот (со связанной записью на приём).
    /// </summary>
    /// <param name="request">Данные занятого слота.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <response code="201">Слот успешно создан, возвращает созданный слот.</response>
    /// <response code="400">Некорректные входные данные.</response>
    /// <response code="401">Пользователь не авторизован.</response>
    [ProducesResponseType(typeof(ScheduleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpPost("occupied")]
    public async Task<ActionResult<ScheduleDto>> AddOccupiedSlotAsync([FromBody] CreateOccupiedSlotRequest request, CancellationToken cancellationToken)
    {
        var slot = await _scheduleService.AddOccupiedSlotAsync(request, cancellationToken);
        return CreatedAtAction("GetSlotById", new { id = slot.Id }, _mapper.Map<ScheduleDto>(slot));
    }

    /// <summary>
    /// Добавить несколько свободных слотов.
    /// </summary>
    /// <param name="requests">Список свободных слотов для создания.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <response code="200">Возвращает список созданных слотов.</response>
    /// <response code="400">Некорректные входные данные (пустой список).</response>
    /// <response code="401">Пользователь не авторизован.</response>
    [ProducesResponseType(typeof(List<ScheduleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpPost("free/bulk")]
    public async Task<ActionResult<List<ScheduleDto>>> AddSlotsAsync([FromBody] List<CreateFreeSlotRequest> requests, CancellationToken cancellationToken)
    {
        var slots = await _scheduleService.AddSlotsAsync(requests, cancellationToken);

        var dtos = _mapper.Map<List<ScheduleDto>>(slots);

        return Ok(dtos);
    }

    /// <summary>
    /// Удалить слот по ID.
    /// </summary>
    /// <param name="id">Идентификатор слота (GUID).</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <response code="200">Возвращает true, если слот удалён.</response>
    /// <response code="401">Пользователь не авторизован.</response>
    /// <response code="404">Слот с указанным ID не найден.</response>
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteSlotAsync(Guid id, CancellationToken cancellationToken)
    {
        var isDeleted = await _scheduleService.RemoveSlotAsync(id, cancellationToken);

        return Ok(isDeleted);
    }

    /// <summary>
    /// Удалить несколько слотов по списку ID.
    /// </summary>
    /// <param name="ids">Список идентификаторов слотов для удаления.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <response code="200">Возвращает количество удалённых слотов.</response>
    /// <response code="400">Список ID пуст.</response>
    /// <response code="401">Пользователь не авторизован.</response>
    /// <response code="404">Ни один из указанных слотов не найден.</response>
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpDelete("delete/bulk")]
    public async Task<ActionResult<int>> DeleteScheduleAsync([FromBody] List<Guid> ids, CancellationToken cancellationToken)
    {
        if(ids == null || !ids.Any()) return BadRequest("Запрос на удаление должен содержать хотя бы 1 элемент.");

        var qtyRemoved = await _scheduleService.RemoveSlotsAsync(ids, cancellationToken);

        if (qtyRemoved == 0) return NotFound();
        
        return Ok(qtyRemoved);
    }

    /// <summary>
    /// Обновить слот по ID.
    /// </summary>
    /// <param name="id">Идентификатор слота (GUID).</param>
    /// <param name="updateSlotRequest">Данные для обновления слота.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <response code="204">Слот успешно обновлён (без тела ответа).</response>
    /// <response code="400">Некорректные входные данные.</response>
    /// <response code="401">Пользователь не авторизован.</response>
    /// <response code="404">Слот с указанным ID не найден.</response>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateSlotAsync(Guid id, [FromBody] UpdateSlotRequest updateSlotRequest, CancellationToken cancellationToken)
    {
        await _scheduleService.UpdateSlotAsync(id, updateSlotRequest, cancellationToken);

        return NoContent();
    }
}