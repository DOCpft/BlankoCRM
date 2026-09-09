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
public class ClientsController : ControllerBase
{
    private readonly IClientService _clientService;
    private readonly IMapper _mapper;

    public ClientsController(IClientService clientService, IMapper mapper)
    {
        _clientService = clientService;
        _mapper = mapper;
    }

    [HttpGet("all")]
    public async Task<ActionResult<List<ClientDto>>> GetAll([FromQuery] string? search)
    {
        var clients = await _clientService.GetAllAsync(search);
        var dtos = _mapper.Map<List<ClientDto>>(clients);
        return Ok(dtos);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ClientDto>> GetById(Guid id)
    {
        var client = await _clientService.GetByIdAsync(id);

        return Ok(_mapper.Map<ClientDto>(client));
    }

    [HttpPost("create")]
    public async Task<ActionResult<ClientDto>> Create([FromBody] CreateClientRequest request)
    {
        var client = await _clientService.CreateAsync(
            request.Name,
            request.Phone,
            request.Telegram,
            request.BirthDate,
            request.Notes,
            request.SalonId);

        return CreatedAtAction(nameof(GetById), new { id = client.Id }, _mapper.Map<ClientDto>(client));
    }

    [HttpPut("update/{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateClientRequest request)
    {
        await _clientService.UpdateAsync(
            id,
            request.Name,
            request.Phone,
            request.Telegram,
            request.BirthDate,
            request.Notes,
            request.SalonId);

        return NoContent();
    }

    [HttpDelete("delete/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _clientService.DeleteAsync(id);
        return NoContent();
    }
}

