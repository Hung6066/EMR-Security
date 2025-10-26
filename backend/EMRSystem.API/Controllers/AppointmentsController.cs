// AppointmentsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMRSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _service;

        public AppointmentsController(IAppointmentService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<ActionResult<AppointmentDto>> Create([FromBody] CreateAppointmentDto dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var appointment = await _service.CreateAppointmentAsync(dto, userId);
                return CreatedAtAction(nameof(GetById), new { id = appointment.Id }, appointment);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AppointmentDto>> GetById(int id)
        {
            try
            {
                var appointment = await _service.GetByIdAsync(id);
                return Ok(appointment);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("patient/{patientId}")]
        public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetByPatient(int patientId)
        {
            var appointments = await _service.GetByPatientIdAsync(patientId);
            return Ok(appointments);
        }

        [HttpGet("doctor/{doctorId}")]
        public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetByDoctor(
            int doctorId,
            [FromQuery] DateTime date)
        {
            var appointments = await _service.GetByDoctorIdAsync(doctorId, date);
            return Ok(appointments);
        }

        [HttpGet("range")]
        public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetByDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            var appointments = await _service.GetByDateRangeAsync(startDate, endDate);
            return Ok(appointments);
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin,Doctor,Receptionist")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateAppointmentStatusDto dto)
        {
            try
            {
                await _service.UpdateStatusAsync(id, dto);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id, [FromBody] string reason)
        {
            try
            {
                await _service.CancelAppointmentAsync(id, reason);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("check-availability")]
        public async Task<ActionResult<bool>> CheckAvailability(
            [FromQuery] int doctorId,
            [FromQuery] DateTime date,
            [FromQuery] TimeSpan time)
        {
            var isAvailable = await _service.IsTimeSlotAvailableAsync(doctorId, date, time);
            return Ok(new { isAvailable });
        }
    }
}