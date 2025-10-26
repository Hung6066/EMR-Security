[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PrescriptionsController : ControllerBase
{
    private readonly IPdfService _pdfService;

    public PrescriptionsController(IPdfService pdfService)
    {
        _pdfService = pdfService;
    }

    [HttpGet("{id}/pdf")]
    public async Task<IActionResult> DownloadPdf(int id)
    {
        try
        {
            var pdfBytes = await _pdfService.GeneratePrescriptionPdfAsync(id);
            return File(pdfBytes, "application/pdf", $"DonThuoc_{id}_{DateTime.Now:yyyyMMdd}.pdf");
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}