// MedicalDocumentsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMRSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MedicalDocumentsController : ControllerBase
    {
        private readonly IMedicalDocumentService _service;

        public MedicalDocumentsController(IMedicalDocumentService service)
        {
            _service = service;
        }

        [HttpPost("upload/{medicalRecordId}")]
        public async Task<ActionResult<MedicalDocumentDto>> Upload(
            int medicalRecordId,
            [FromForm] IFormFile file,
            [FromForm] string description)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var document = await _service.UploadDocumentAsync(medicalRecordId, file, description, userId);
                return Ok(document);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("record/{medicalRecordId}")]
        public async Task<ActionResult<IEnumerable<MedicalDocumentDto>>> GetByRecordId(int medicalRecordId)
        {
            var documents = await _service.GetDocumentsByRecordIdAsync(medicalRecordId);
            return Ok(documents);
        }

        [HttpGet("download/{documentId}")]
        public async Task<IActionResult> Download(int documentId)
        {
            try
            {
                var fileBytes = await _service.DownloadDocumentAsync(documentId);
                return File(fileBytes, "application/octet-stream");
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpDelete("{documentId}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Delete(int documentId)
        {
            try
            {
                await _service.DeleteDocumentAsync(documentId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}