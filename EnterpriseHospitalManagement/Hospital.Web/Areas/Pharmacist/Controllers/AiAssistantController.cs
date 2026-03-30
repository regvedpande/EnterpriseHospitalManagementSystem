using Hospital.Utilities;
using Hospital.Web.Infrastructure.AI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Areas.Pharmacist.Controllers
{
    [Area("Pharmacist")]
    [Authorize(Roles = WebSiteRoles.Website_Pharmacist)]
    public class AiAssistantController : Controller
    {
        private readonly IAiService _ai;
        public AiAssistantController(IAiService ai) => _ai = ai;

        [HttpGet]
        public IActionResult Index() { ViewBag.IsConfigured = _ai.IsConfigured; return View(); }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckInteraction([FromForm] string medicines, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(medicines)) return BadRequest("Please enter medicines.");
            var result = await _ai.GetPharmacistDrugInteractionAsync(medicines, ct);
            return Json(new { result });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DosageGuide([FromForm] string drug, [FromForm] string? patientInfo, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(drug)) return BadRequest("Drug name is required.");
            var result = await _ai.GetDosageGuideAsync(drug, patientInfo ?? "Not specified", ct);
            return Json(new { result });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> FindSubstitute([FromForm] string drug, [FromForm] string? reason, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(drug)) return BadRequest("Drug name is required.");
            var result = await _ai.GetDrugSubstituteAsync(drug, reason ?? "Not specified", ct);
            return Json(new { result });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Chat([FromBody] ChatRequest req, CancellationToken ct)
        {
            if (req?.Messages == null) return BadRequest();
            var result = await _ai.ChatAsync(req.Messages.Select(m => new AiMessage(m.Role, m.Content)), ct);
            return Json(new { result });
        }

        public record ChatMessageDto(string Role, string Content);
        public record ChatRequest(List<ChatMessageDto> Messages);
    }
}
