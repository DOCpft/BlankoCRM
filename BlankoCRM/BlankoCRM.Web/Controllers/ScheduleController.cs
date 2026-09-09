using BlankoCRM.Application.Abstract;
using BlankoCRM.Application.DTOs;
using BlankoCRM.Application.DTOs.Mapping;
using AutoMapper;
using BlankoCRM.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
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
        public async Task<ActionResult<ScheduleDto>> GetSlotByIdAsync(Guid id)
        {
            var slot = await _scheduleService.GetSlotByIdAsync(id);

            return Ok(_mapper.Map<ScheduleDto>(slot));
        }


        [HttpGet("get/period")]
        public async Task<ActionResult<List<ScheduleDto>>> GetScheduleByPeriod(
            [FromQuery] DateTime dtFrom,
            [FromQuery] DateTime dtTo)
        {
            if (dtFrom >= dtTo)
                return BadRequest("Дата начала должна быть меньше даты конца периода.");

            var schedule = await _scheduleService.GetScheduleByPeriodAsync(dtFrom, dtTo);

            var dtos = _mapper.Map<List<ScheduleDto>>(schedule);

            return Ok(dtos);
        }


        [HttpPost("add")]
        public async Task<ActionResult<ScheduleDto>> AddSlotAsync([FromBody] CreateSlotRequest createSlotRequest)
        {
            var slot = await _scheduleService.AddSlotAsync(createSlotRequest.slotTime, createSlotRequest.AppointmentId);
            if (slot == null)
            {
                return BadRequest();
            }
            return CreatedAtAction("GetSlotById", new { id = slot.Id }, _mapper.Map<ScheduleDto>(slot));
            
        }

        [HttpPost("add/bulk")]
        public async Task<ActionResult<List<ScheduleDto>>> AddSlotsAsync([FromBody] List<CreateSlotRequest> requests)
        {
            var slots = await _scheduleService.AddSlotsAsync(requests);

            var dtos = _mapper.Map<List<ScheduleDto>>(slots);

            return Ok(dtos);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteSlotAsync(Guid id)
        {
            var isDeleted = await _scheduleService.RemoveSlotAsync(id);

            return Ok(isDeleted);
        }

        [HttpDelete("delete/bulk")]
        public async Task<ActionResult<int>> DeleteScheduleAsync([FromBody] List<Guid> ids)
        {
            if(ids == null || !ids.Any()) return BadRequest("Запрос на удаление должен содержать хотя бы 1 элемент.");

            var qtyRemoved = await _scheduleService.RemoveSlotsAsync(ids);

            if (qtyRemoved == 0) return NotFound();
            
            return Ok(qtyRemoved);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateSlotAsync(Guid id, [FromBody] UpdateSlotRequest updateSlotRequest)
        {
            await _scheduleService.UpdateSlotAsync(id, updateSlotRequest.slotTime, updateSlotRequest.AppointmentId);

            return NoContent();
        }

    }
}

