using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Hospital.Web.Infrastructure.AI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Hospital.Web.Areas.Doctor.Controllers
{
    [Area("Doctor")]
    [Authorize(Roles = WebSiteRoles.Website_Doctor)]
    public class AiAssistantController : Controller
    {
        private readonly IAiService _ai;
        private readonly IPatientReportService _reports;
        private readonly IAppointmentService _appts;
        private readonly IApplicationUserService _users;

        public AiAssistantController(
            IAiService ai,
            IPatientReportService reports,
            IAppointmentService appts,
            IApplicationUserService users)
        {
            _ai = ai; _reports = reports; _appts = appts; _users = users;
        }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.IsConfigured = _ai.IsConfigured;
            ViewBag.Patients = _users.GetAllPatients(1, 200).Items;
            return View();
        }

        // ── Diagnose symptoms ──────────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Diagnose(
            [FromForm] string symptoms,
            [FromForm] string? patientId,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(symptoms))
                return BadRequest("Symptoms are required.");

            var context = await BuildPatientContextAsync(patientId);
            var result  = await _ai.GetDiagnosisSuggestionAsync(symptoms, context, ct);
            return Json(new { result });
        }

        // ── Medicine recommendation ────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RecommendMedicines(
            [FromForm] string diagnosis,
            [FromForm] string? patientId,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(diagnosis))
                return BadRequest("Diagnosis is required.");

            var context = await BuildPatientContextAsync(patientId);
            var result  = await _ai.GetMedicineRecommendationAsync(diagnosis, context, ct);
            return Json(new { result });
        }

        // ── Drug interaction check ─────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckInteractions(
            [FromForm] string medicines,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(medicines))
                return BadRequest("Please enter medicines.");

            var result = await _ai.GetDrugInteractionCheckAsync(medicines, ct);
            return Json(new { result });
        }

        // ── Early symptom alert ────────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EarlyAlert(
            [FromForm] string symptoms,
            [FromForm] string? patientId,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(symptoms))
                return BadRequest("Symptoms are required.");

            var history = await BuildPatientContextAsync(patientId);
            var result  = await _ai.GetEarlySymptomAlertAsync(symptoms, history, ct);
            return Json(new { result });
        }

        // ── Treatment plan ─────────────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> TreatmentPlan(
            [FromForm] string diagnosis,
            [FromForm] string? patientId,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(diagnosis))
                return BadRequest("Diagnosis is required.");

            var context = await BuildPatientContextAsync(patientId);
            var result  = await _ai.GetTreatmentPlanAsync(diagnosis, context, ct);
            return Json(new { result });
        }

        // ── Free chat ──────────────────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Chat(
            [FromBody] ChatRequest req,
            CancellationToken ct)
        {
            if (req?.Messages == null || req.Messages.Count == 0)
                return BadRequest("Messages required.");

            var result = await _ai.ChatAsync(req.Messages.Select(m => new AiMessage(m.Role, m.Content)), ct);
            return Json(new { result });
        }

        // ── Helpers ────────────────────────────────────────────────────────────
        private Task<string> BuildPatientContextAsync(string? patientId)
        {
            if (string.IsNullOrWhiteSpace(patientId))
                return Task.FromResult("No specific patient selected.");

            var sb = new System.Text.StringBuilder();

            // Fetch patient info from patients list (find by id)
            var patients = _users.GetAllPatients(1, 1000).Items;
            var patient  = patients.FirstOrDefault(p => p.Id == patientId);
            if (patient != null)
                sb.AppendLine($"Patient: {patient.Name}, City: {patient.City}, Gender: {patient.Gender}");

            var reports = _reports.GetByPatient(patientId, 1, 5).Items;
            if (reports.Any())
            {
                sb.AppendLine("Recent diagnoses:");
                foreach (var r in reports)
                    sb.AppendLine($"  - {r.CreatedDate:yyyy-MM-dd}: {r.Diagnose}");
            }

            var appts = _appts.GetByPatient(patientId, 1, 5).Items;
            if (appts.Any())
            {
                sb.AppendLine("Recent appointments:");
                foreach (var a in appts)
                    sb.AppendLine($"  - {a.AppointmentDate:yyyy-MM-dd}: {a.Type} ({a.Status})");
            }

            return Task.FromResult(sb.Length > 0 ? sb.ToString() : "No prior history found.");
        }

        public record ChatMessageDto(string Role, string Content);
        public record ChatRequest(List<ChatMessageDto> Messages);
    }
}
