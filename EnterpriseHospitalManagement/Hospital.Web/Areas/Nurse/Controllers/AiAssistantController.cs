using Hospital.Utilities;
using Hospital.Web.Infrastructure.AI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Areas.Nurse.Controllers
{
    [Area("Nurse")]
    [Authorize(Roles = WebSiteRoles.Website_Nurse)]
    public class AiAssistantController : Controller
    {
        private readonly IAiService _ai;
        public AiAssistantController(IAiService ai) => _ai = ai;

        [HttpGet]
        public IActionResult Index() { ViewBag.IsConfigured = _ai.IsConfigured; return View(); }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> InterpretVitals([FromForm] string vitals, [FromForm] string? context, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(vitals)) return BadRequest("Vitals are required.");
            var result = await _ai.InterpretVitalsAsync(vitals, context ?? "Not specified", ct);
            return Json(new { result });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CarePlan([FromForm] string condition, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(condition)) return BadRequest("Condition is required.");
            var result = await _ai.GetNursingCarePlanAsync(condition, ct);
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
