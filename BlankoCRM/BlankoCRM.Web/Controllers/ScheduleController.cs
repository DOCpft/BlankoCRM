using BlankoCRM.Application.Abstract;
using BlankoCRM.Application.DTOs;
using BlankoCRM.Application.DTOs.Mapping;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlankoCRM.Web.Controllers
{
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

        [HttpGet("{id:guid}", Name = "GetSlotById")]
        public async Task<ActionResult<ScheduleDto>> GetSlotByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var slot = await _scheduleService.GetSlotByIdAsync(id, cancellationToken);

            return Ok(_mapper.Map<ScheduleDto>(slot));
        }


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


        [HttpPost("free")]
        public async Task<ActionResult<ScheduleDto>> AddFreeSlotAsync([FromBody] CreateFreeSlotRequest request, CancellationToken cancellationToken)
        {
            var slot = await _scheduleService.AddFreeSlotAsync(request, cancellationToken);
            return CreatedAtAction("GetSlotById", new { id = slot.Id }, _mapper.Map<ScheduleDto>(slot));
        }

        [HttpPost("occupied")]
        public async Task<ActionResult<ScheduleDto>> AddOccupiedSlotAsync([FromBody] CreateOccupiedSlotRequest request, CancellationToken cancellationToken)
        {
            var slot = await _scheduleService.AddOccupiedSlotAsync(request, cancellationToken);
            return CreatedAtAction("GetSlotById", new { id = slot.Id }, _mapper.Map<ScheduleDto>(slot));
        }

        [HttpPost("free/bulk")]
        public async Task<ActionResult<List<ScheduleDto>>> AddSlotsAsync([FromBody] List<CreateFreeSlotRequest> requests, CancellationToken cancellationToken)
        {
            var slots = await _scheduleService.AddSlotsAsync(requests, cancellationToken);

            var dtos = _mapper.Map<List<ScheduleDto>>(slots);

            return Ok(dtos);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteSlotAsync(Guid id, CancellationToken cancellationToken)
        {
            var isDeleted = await _scheduleService.RemoveSlotAsync(id, cancellationToken);

            return Ok(isDeleted);
        }

        [HttpDelete("delete/bulk")]
        public async Task<ActionResult<int>> DeleteScheduleAsync([FromBody] List<Guid> ids, CancellationToken cancellationToken)
        {
            if(ids == null || !ids.Any()) return BadRequest("Запрос на удаление должен содержать хотя бы 1 элемент.");

            var qtyRemoved = await _scheduleService.RemoveSlotsAsync(ids, cancellationToken);

            if (qtyRemoved == 0) return NotFound();
            
            return Ok(qtyRemoved);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateSlotAsync(Guid id, [FromBody] UpdateSlotRequest updateSlotRequest, CancellationToken cancellationToken)
        {
            await _scheduleService.UpdateSlotAsync(id, updateSlotRequest, cancellationToken);

            return NoContent();
        }

    }
}

