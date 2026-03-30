using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Hospital.Web.Infrastructure.AI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Hospital.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = WebSiteRoles.Website_Admin)]
    public class AiAssistantController : Controller
    {
        private readonly IAiService _ai;
        private readonly IAppointmentService _appts;
        private readonly IBillService _bills;
        private readonly ILabService _labs;
        private readonly IApplicationUserService _users;

        public AiAssistantController(
            IAiService ai, IAppointmentService appts,
            IBillService bills, ILabService labs, IApplicationUserService users)
        {
            _ai = ai; _appts = appts; _bills = bills; _labs = labs; _users = users;
        }

        [HttpGet]
        public IActionResult Index() { ViewBag.IsConfigured = _ai.IsConfigured; return View(); }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AnalyseStats(CancellationToken ct)
        {
            // Build a concise stats summary for the AI
            var stats = new
            {
                Patients     = _users.GetAllPatients(1, 1).TotalCount,
                Doctors      = _users.GetAllDoctors(1, 1).TotalCount,
                Appointments = _appts.GetAll(1, 1).TotalCount,
                Bills        = _bills.GetAll(1, 1).TotalCount,
                LabOrders    = _labs.GetAll(1, 1).TotalCount,
                RecentAppts  = _appts.GetAll(1, 20).Items.Select(a => new { a.Status, a.AppointmentDate }),
                RecentBills  = _bills.GetAll(1, 20).Items.Select(b => new { b.Status, b.TotalBill }),
            };
            var statsJson = JsonSerializer.Serialize(stats, new JsonSerializerOptions { WriteIndented = false });
            var result = await _ai.AnalyseHospitalStatsAsync(statsJson, ct);
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
