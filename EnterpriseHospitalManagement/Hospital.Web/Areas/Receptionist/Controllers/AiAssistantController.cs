using Hospital.Utilities;
using Hospital.Web.Infrastructure.AI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Areas.Receptionist.Controllers
{
    [Area("Receptionist")]
    [Authorize(Roles = WebSiteRoles.Website_Receptionist)]
    public class AiAssistantController : Controller
    {
        private readonly IAiService _ai;
        public AiAssistantController(IAiService ai) => _ai = ai;

        [HttpGet]
        public IActionResult Index() { ViewBag.IsConfigured = _ai.IsConfigured; return View(); }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Triage([FromForm] string symptoms, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(symptoms)) return BadRequest("Symptoms are required.");
            var result = await _ai.TriageSymptomsAsync(symptoms, ct);
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
