using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Hospital.Web.Infrastructure.AI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace Hospital.Web.Areas.Doctor.Controllers
{
    [Area("Doctor")]
    [Authorize(Roles = WebSiteRoles.Website_Doctor)]
    public class AiAssistantController : Controller
    {
        private readonly IAiService _ai;
        private readonly IPatientReportService _reports;

        public AiAssistantController(IAiService ai, IPatientReportService reports)
        {
            _ai     = ai;
            _reports = reports;
        }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.IsConfigured = _ai.IsConfigured;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Diagnose(
            [FromForm] string symptoms,
            [FromForm] string? patientContext,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(symptoms))
                return BadRequest("Symptoms are required.");

            var result = await _ai.GetDiagnosisSuggestionAsync(symptoms, patientContext ?? "", ct);
            return Json(new { result });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Chat(
            [FromBody] ChatRequest req,
            CancellationToken ct)
        {
            if (req?.Messages == null || req.Messages.Count == 0)
                return BadRequest("Messages are required.");

            var aiMessages = req.Messages.Select(m => new AiMessage(m.Role, m.Content));
            var result = await _ai.ChatAsync(aiMessages, ct);
            return Json(new { result });
        }

        public record ChatMessageDto(string Role, string Content);
        public record ChatRequest(List<ChatMessageDto> Messages);
    }
}
