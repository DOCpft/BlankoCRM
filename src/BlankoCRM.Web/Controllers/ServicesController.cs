using BlankoCRM.Application.Abstract;
using BlankoCRM.Application.DTOs;
using BlankoCRM.Application.DTOs.Mapping;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlankoCRM.Web.Controllers;

/// <summary>
/// Управление услугами (прайс-лист организации).
/// </summary>
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

    /// <summary>
    /// Получить список всех услуг.
    /// </summary>
    /// <response code="200">Возвращает список всех услуг.</response>
    /// <response code="401">Пользователь не авторизован.</response>
    [ProducesResponseType(typeof(List<ServiceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpGet]
    public async Task<ActionResult<List<ServiceDto>>> GetAll()
    {
        var services = await _servicesService.GetAllAsync();
        var dtos = _mapper.Map<List<ServiceDto>>(services);
        return Ok(dtos);
    }

    /// <summary>
    /// Получить услугу по ID.
    /// </summary>
    /// <param name="id">Идентификатор услуги (GUID).</param>
    /// <response code="200">Возвращает найденную услугу.</response>
    /// <response code="401">Пользователь не авторизован.</response>
    /// <response code="404">Услуга с указанным ID не найдена.</response>
    [ProducesResponseType(typeof(ServiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ServiceDto>> GetById(Guid id)
    {
        var service = await _servicesService.GetByIdAsync(id);

        return Ok(_mapper.Map<ServiceDto>(service));
    }

    /// <summary>
    /// Создать новую услугу.
    /// </summary>
    /// <param name="request">Данные для создания услуги.</param>
    /// <response code="201">Услуга успешно создана, возвращает созданную услугу.</response>
    /// <response code="400">Некорректные входные данные.</response>
    /// <response code="401">Пользователь не авторизован.</response>
    [ProducesResponseType(typeof(ServiceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

    /// <summary>
    /// Обновить данные услуги.
    /// </summary>
    /// <param name="id">Идентификатор услуги (GUID).</param>
    /// <param name="request">Данные для обновления услуги.</param>
    /// <response code="204">Услуга успешно обновлена (без тела ответа).</response>
    /// <response code="400">Некорректные входные данные.</response>
    /// <response code="401">Пользователь не авторизован.</response>
    /// <response code="404">Услуга с указанным ID не найдена.</response>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

    /// <summary>
    /// Удалить услугу.
    /// </summary>
    /// <param name="id">Идентификатор услуги (GUID).</param>
    /// <response code="204">Услуга успешно удалена (без тела ответа).</response>
    /// <response code="401">Пользователь не авторизован.</response>
    /// <response code="404">Услуга с указанным ID не найдена.</response>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _servicesService.DeleteAsync(id);
        return NoContent();
    }
}