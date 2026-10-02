using BlankoCRM.Application.Abstract;
using BlankoCRM.Application.DTOs;
using BlankoCRM.Application.DTOs.Mapping;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlankoCRM.Web.Controllers;

/// <summary>
/// Управление клиентами (база клиентов организации).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClientsController : ControllerBase
{
    private readonly IClientService _clientService;
    private readonly IMapper _mapper;

    public ClientsController(IClientService clientService, IMapper mapper)
    {
        _clientService = clientService;
        _mapper = mapper;
    }

    /// <summary>
    /// Получить список клиентов с возможностью поиска.
    /// </summary>
    /// <param name="search">Необязательная строка поиска по имени/телефону.</param>
    /// <response code="200">Возвращает список клиентов.</response>
    /// <response code="401">Пользователь не авторизован.</response>
    [ProducesResponseType(typeof(List<ClientDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpGet("all")]
    public async Task<ActionResult<List<ClientDto>>> GetAll([FromQuery] string? search)
    {
        var clients = await _clientService.GetAllAsync(search);
        var dtos = _mapper.Map<List<ClientDto>>(clients);
        return Ok(dtos);
    }

    /// <summary>
    /// Получить клиента по ID.
    /// </summary>
    /// <param name="id">Идентификатор клиента (GUID).</param>
    /// <response code="200">Возвращает найденного клиента.</response>
    /// <response code="401">Пользователь не авторизован.</response>
    /// <response code="404">Клиент с указанным ID не найден.</response>
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ClientDto>> GetById(Guid id)
    {
        var client = await _clientService.GetByIdAsync(id);

        return Ok(_mapper.Map<ClientDto>(client));
    }

    /// <summary>
    /// Создать нового клиента.
    /// </summary>
    /// <param name="request">Данные для создания клиента.</param>
    /// <response code="201">Клиент успешно создан, возвращает созданного клиента.</response>
    /// <response code="400">Некорректные входные данные.</response>
    /// <response code="401">Пользователь не авторизован.</response>
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpPost("create")]
    public async Task<ActionResult<ClientDto>> Create([FromBody] CreateClientRequest request)
    {
        var client = await _clientService.CreateAsync(
            request.Name,
            request.Phone,
            request.Contacts,
            request.BirthDate,
            request.Notes,
            request.SalonId);

        return CreatedAtAction(nameof(GetById), new { id = client.Id }, _mapper.Map<ClientDto>(client));
    }

    /// <summary>
    /// Обновить данные клиента.
    /// </summary>
    /// <param name="id">Идентификатор клиента (GUID).</param>
    /// <param name="request">Данные для обновления клиента.</param>
    /// <response code="204">Клиент успешно обновлён (без тела ответа).</response>
    /// <response code="400">Некорректные входные данные.</response>
    /// <response code="401">Пользователь не авторизован.</response>
    /// <response code="404">Клиент с указанным ID не найден.</response>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpPut("update/{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateClientRequest request)
    {
        await _clientService.UpdateAsync(
            id,
            request.Name,
            request.Phone,
            request.Contacts,
            request.BirthDate,
            request.Notes,
            request.SalonId);

        return NoContent();
    }

    /// <summary>
    /// Удалить клиента.
    /// </summary>
    /// <param name="id">Идентификатор клиента (GUID).</param>
    /// <response code="204">Клиент успешно удалён (без тела ответа).</response>
    /// <response code="401">Пользователь не авторизован.</response>
    /// <response code="404">Клиент с указанным ID не найден.</response>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpDelete("delete/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _clientService.DeleteAsync(id);
        return NoContent();
    }
}