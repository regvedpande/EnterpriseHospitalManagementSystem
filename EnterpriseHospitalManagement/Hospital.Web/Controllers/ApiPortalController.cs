using System.Security.Claims;
using Hospital.Models;
using Hospital.Models.Enums;
using Hospital.Repositories;
using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Hospital.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Web.Controllers
{
    [Route("api/portal")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [IgnoreAntiforgeryToken]
    public class ApiPortalController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IAiAssistantService _assistantService;

        public ApiPortalController(ApplicationDbContext db, IAiAssistantService assistantService)
        {
            _db = db;
            _assistantService = assistantService;
        }

        [HttpGet("bootstrap")]
        public async Task<ActionResult<PortalBootstrapViewModel>> Bootstrap(CancellationToken ct)
        {
            var user = await GetCurrentUserAsync(ct);
            if (user == null)
                return Unauthorized();

            var role = GetCurrentRole();
            var aiRole = TryMapAiRole(role);

            return Ok(new PortalBootstrapViewModel
            {
                User = BuildUser(user, role),
                Navigation = BuildNavigation(role, aiRole.HasValue),
                Dashboard = await BuildDashboardAsync(user, role, ct),
                AiAssistant = aiRole.HasValue
                    ? await _assistantService.BuildAsync(aiRole.Value, user.Id, user.Name, null, ct)
                    : null
            });
        }

        [HttpPost("assistant")]
        public async Task<ActionResult<AiAssistantPageViewModel>> AskAssistant([FromBody] PortalAiPromptRequest request, CancellationToken ct)
        {
            var user = await GetCurrentUserAsync(ct);
            if (user == null)
                return Unauthorized();

            var role = GetCurrentRole();
            var aiRole = TryMapAiRole(role);
            if (!aiRole.HasValue)
                return Forbid();

            var model = await _assistantService.BuildAsync(aiRole.Value, user.Id, user.Name, request.Prompt, ct);
            return Ok(model);
        }

        private async Task<ApplicationUser?> GetCurrentUserAsync(CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return null;

            return await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, ct);
        }

        private string GetCurrentRole()
        {
            return User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
        }

        private static PortalUserViewModel BuildUser(ApplicationUser user, string role)
        {
            var displayName = string.IsNullOrWhiteSpace(user.Name) ? (user.Email ?? user.UserName ?? "User") : user.Name;
            var parts = displayName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var initials = parts.Length switch
            {
                0 => "U",
                1 => parts[0][0].ToString().ToUpperInvariant(),
                _ => $"{char.ToUpperInvariant(parts[0][0])}{char.ToUpperInvariant(parts[1][0])}"
            };

            return new PortalUserViewModel
            {
                Id = user.Id,
                Name = displayName,
                Email = user.Email ?? user.UserName ?? "",
                Role = role,
                RoleDisplay = RoleDisplay(role),
                DefaultRoute = "/dashboard",
                Initials = initials
            };
        }

        private static List<PortalNavigationItemViewModel> BuildNavigation(string role, bool hasAi)
        {
            var items = new List<PortalNavigationItemViewModel>
            {
                Nav("dashboard", "Dashboard", "/dashboard", "fa-chart-pie")
            };

            if (hasAi)
                items.Add(Nav("ai", "AI Assistant", "/assistant", "fa-sparkles"));

            items.Add(Nav("about", "Deployment Notes", "/about", "fa-cloud-arrow-up"));

            if (role == WebSiteRoles.Website_Admin)
                items.Add(Nav("ops", "Operations", "/dashboard", "fa-hospital"));

            return items;
        }

        private async Task<PortalDashboardViewModel> BuildDashboardAsync(ApplicationUser user, string role, CancellationToken ct)
        {
            return role switch
            {
                WebSiteRoles.Website_Admin => await BuildAdminDashboardAsync(ct),
                WebSiteRoles.Website_Doctor => await BuildDoctorDashboardAsync(user.Id, ct),
                WebSiteRoles.Website_Nurse => await BuildNurseDashboardAsync(ct),
                WebSiteRoles.Website_Pharmacist => await BuildPharmacistDashboardAsync(ct),
                WebSiteRoles.Website_LabTech => await BuildLabTechDashboardAsync(user.Id, ct),
                WebSiteRoles.Website_Receptionist => await BuildReceptionistDashboardAsync(ct),
                WebSiteRoles.Website_Patient => await BuildPatientDashboardAsync(user.Id, ct),
                WebSiteRoles.Website_Accountant => await BuildAccountantDashboardAsync(ct),
                _ => new PortalDashboardViewModel
                {
                    Title = "Portal overview",
                    Subtitle = "No dashboard is configured for this role yet."
                }
            };
        }

        private async Task<PortalDashboardViewModel> BuildAdminDashboardAsync(CancellationToken ct)
        {
            var patients = await CountUsersInRoleAsync(WebSiteRoles.Website_Patient, ct);
            var doctors = await CountUsersInRoleAsync(WebSiteRoles.Website_Doctor, ct);
            var appointments = await _db.Appointments.AsNoTracking().ToListAsync(ct);
            var bills = await _db.Bills.AsNoTracking().ToListAsync(ct);
            var labs = await _db.Labs.AsNoTracking().ToListAsync(ct);
            var rooms = await _db.Rooms.AsNoTracking().ToListAsync(ct);
            var paidRevenue = bills.Where(b => b.Status == BillStatus.Paid).Sum(b => b.TotalBill);
            var months = Enumerable.Range(0, 6).Select(offset => DateTime.UtcNow.AddMonths(offset - 5)).ToList();

            return new PortalDashboardViewModel
            {
                Title = "Hospital command center",
                Subtitle = "Live operational snapshot for staffing, clinical throughput, and finance.",
                Metrics = new List<PortalMetricViewModel>
                {
                    Metric("Patients", patients, "fa-user-injured", "blue"),
                    Metric("Doctors", doctors, "fa-user-doctor", "green"),
                    Metric("Appointments", appointments.Count, "fa-calendar-check", "teal"),
                    Metric("Pending labs", labs.Count(l => l.Status != LabTestStatus.Completed && l.Status != LabTestStatus.Cancelled), "fa-flask", "orange"),
                    Metric("Paid revenue", paidRevenue.ToString("C0"), "fa-money-bill-wave", "purple"),
                    Metric("Rooms", rooms.Count, "fa-bed", "pink")
                },
                Charts = new List<PortalChartViewModel>
                {
                    Chart(
                        "Appointments by month",
                        "Last 6 months",
                        months.Select(m => m.ToString("MMM yy")),
                        months.Select(m => (decimal)appointments.Count(a => a.AppointmentDate.Year == m.Year && a.AppointmentDate.Month == m.Month))),
                    Chart(
                        "Appointment status",
                        "Current distribution",
                        new[] { "Scheduled", "Confirmed", "In Progress", "Completed", "Cancelled", "No Show" },
                        new decimal[]
                        {
                            appointments.Count(a => a.Status == AppointmentStatus.Scheduled),
                            appointments.Count(a => a.Status == AppointmentStatus.Confirmed),
                            appointments.Count(a => a.Status == AppointmentStatus.InProgress),
                            appointments.Count(a => a.Status == AppointmentStatus.Completed),
                            appointments.Count(a => a.Status == AppointmentStatus.Cancelled),
                            appointments.Count(a => a.Status == AppointmentStatus.NoShow)
                        })
                },
                RecentItems = await _db.Appointments
                    .AsNoTracking()
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                    .OrderByDescending(a => a.AppointmentDate)
                    .Take(6)
                    .Select(a => Recent(a.Type, $"{a.Patient!.Name} with {a.Doctor!.Name}", a.AppointmentDate.ToString("dd MMM yyyy hh:mm tt"), a.Status.ToString(), "fa-calendar-day", a.Status == AppointmentStatus.Cancelled ? "orange" : "blue"))
                    .ToListAsync(ct)
            };
        }

        private async Task<PortalDashboardViewModel> BuildDoctorDashboardAsync(string userId, CancellationToken ct)
        {
            var appointments = await _db.Appointments
                .AsNoTracking()
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == userId)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync(ct);
            var reports = await _db.PatientReports
                .AsNoTracking()
                .Include(r => r.Patient)
                .Where(r => r.DoctorId == userId)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync(ct);
            var months = Enumerable.Range(0, 6).Select(offset => DateTime.UtcNow.AddMonths(offset - 5)).ToList();

            return new PortalDashboardViewModel
            {
                Title = "Doctor dashboard",
                Subtitle = "Your clinic load, patient flow, and recent case output.",
                Metrics = new List<PortalMetricViewModel>
                {
                    Metric("Appointments", appointments.Count, "fa-calendar-check", "blue"),
                    Metric("Today", appointments.Count(a => a.AppointmentDate.Date == DateTime.Today), "fa-calendar-day", "green"),
                    Metric("Reports", reports.Count, "fa-file-medical", "teal"),
                    Metric("Upcoming 7 days", appointments.Count(a => a.AppointmentDate >= DateTime.Now && a.AppointmentDate <= DateTime.Now.AddDays(7)), "fa-user-clock", "orange")
                },
                Charts = new List<PortalChartViewModel>
                {
                    Chart(
                        "Status breakdown",
                        "Your appointment pipeline",
                        new[] { "Scheduled", "Confirmed", "In Progress", "Completed", "Cancelled", "No Show" },
                        new decimal[]
                        {
                            appointments.Count(a => a.Status == AppointmentStatus.Scheduled),
                            appointments.Count(a => a.Status == AppointmentStatus.Confirmed),
                            appointments.Count(a => a.Status == AppointmentStatus.InProgress),
                            appointments.Count(a => a.Status == AppointmentStatus.Completed),
                            appointments.Count(a => a.Status == AppointmentStatus.Cancelled),
                            appointments.Count(a => a.Status == AppointmentStatus.NoShow)
                        }),
                    Chart(
                        "Appointments by month",
                        "Last 6 months",
                        months.Select(m => m.ToString("MMM")),
                        months.Select(m => (decimal)appointments.Count(a => a.AppointmentDate.Year == m.Year && a.AppointmentDate.Month == m.Month)))
                },
                RecentItems = appointments.Take(4).Select(a => Recent(a.Type, a.Patient?.Name ?? "Patient", a.AppointmentDate.ToString("dd MMM yyyy hh:mm tt"), a.Status.ToString(), "fa-user-doctor", "blue"))
                    .Concat(reports.Take(3).Select(r => Recent(r.Diagnose, r.Patient?.Name ?? "Patient", r.CreatedDate.ToString("dd MMM yyyy"), "Report", "fa-file-waveform", "green")))
                    .ToList()
            };
        }

        private async Task<PortalDashboardViewModel> BuildNurseDashboardAsync(CancellationToken ct)
        {
            var appointments = await _db.Appointments.AsNoTracking().OrderByDescending(a => a.AppointmentDate).ToListAsync(ct);
            var patientCount = await CountUsersInRoleAsync(WebSiteRoles.Website_Patient, ct);
            var roomCount = await _db.Rooms.CountAsync(ct);

            return new PortalDashboardViewModel
            {
                Title = "Nursing station",
                Subtitle = "Monitor patient volume, room context, and appointment flow.",
                Metrics = new List<PortalMetricViewModel>
                {
                    Metric("Appointments", appointments.Count, "fa-calendar-check", "blue"),
                    Metric("Patients", patientCount, "fa-user-injured", "teal"),
                    Metric("Rooms", roomCount, "fa-bed", "green")
                },
                Charts = new List<PortalChartViewModel>
                {
                    Chart(
                        "Appointment status",
                        "Nursing visibility",
                        new[] { "Scheduled", "Confirmed", "In Progress", "Completed", "Cancelled", "No Show" },
                        new decimal[]
                        {
                            appointments.Count(a => a.Status == AppointmentStatus.Scheduled),
                            appointments.Count(a => a.Status == AppointmentStatus.Confirmed),
                            appointments.Count(a => a.Status == AppointmentStatus.InProgress),
                            appointments.Count(a => a.Status == AppointmentStatus.Completed),
                            appointments.Count(a => a.Status == AppointmentStatus.Cancelled),
                            appointments.Count(a => a.Status == AppointmentStatus.NoShow)
                        })
                },
                RecentItems = await _db.Appointments
                    .AsNoTracking()
                    .Include(a => a.Patient)
                    .OrderByDescending(a => a.AppointmentDate)
                    .Take(6)
                    .Select(a => Recent(a.Type, a.Patient!.Name, a.AppointmentDate.ToString("dd MMM yyyy hh:mm tt"), a.Status.ToString(), "fa-user-nurse", "blue"))
                    .ToListAsync(ct)
            };
        }

        private async Task<PortalDashboardViewModel> BuildPharmacistDashboardAsync(CancellationToken ct)
        {
            var medicines = await _db.Medicines.AsNoTracking().OrderBy(m => m.Name).ToListAsync(ct);
            var reports = await _db.PatientReports
                .AsNoTracking()
                .Include(r => r.Patient)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync(ct);
            var grouped = medicines
                .GroupBy(m => string.IsNullOrWhiteSpace(m.Type) ? "Other" : m.Type)
                .OrderByDescending(g => g.Count())
                .Take(6)
                .ToList();

            return new PortalDashboardViewModel
            {
                Title = "Pharmacy dashboard",
                Subtitle = "Catalog health, prescription throughput, and inventory context.",
                Metrics = new List<PortalMetricViewModel>
                {
                    Metric("Medicines", medicines.Count, "fa-pills", "blue"),
                    Metric("Prescription reports", reports.Count, "fa-prescription-bottle-medical", "green"),
                    Metric("Low stock proxy", medicines.Count(m => m.Cost < 5), "fa-triangle-exclamation", "orange")
                },
                Charts = new List<PortalChartViewModel>
                {
                    Chart("Medicines by type", "Top catalog segments", grouped.Select(g => g.Key), grouped.Select(g => (decimal)g.Count()))
                },
                RecentItems = medicines.Take(4).Select(m => Recent(m.Name, m.Type, m.Cost.ToString("C"), "Catalog", "fa-capsules", "blue"))
                    .Concat(reports.Take(3).Select(r => Recent(r.Diagnose, r.Patient?.Name ?? "Patient", r.CreatedDate.ToString("dd MMM yyyy"), "Prescription source", "fa-file-prescription", "green")))
                    .ToList()
            };
        }

        private async Task<PortalDashboardViewModel> BuildLabTechDashboardAsync(string userId, CancellationToken ct)
        {
            var labs = await _db.Labs
                .AsNoTracking()
                .Include(l => l.Patient)
                .OrderByDescending(l => l.CreatedDate)
                .ToListAsync(ct);

            return new PortalDashboardViewModel
            {
                Title = "Lab operations",
                Subtitle = "Track sample flow, technician workload, and pending results.",
                Metrics = new List<PortalMetricViewModel>
                {
                    Metric("Total tests", labs.Count, "fa-vials", "blue"),
                    Metric("Assigned to me", labs.Count(l => l.TechnicianId == userId), "fa-user-check", "green"),
                    Metric("Pending", labs.Count(l => l.Status != LabTestStatus.Completed && l.Status != LabTestStatus.Cancelled), "fa-hourglass-half", "orange")
                },
                Charts = new List<PortalChartViewModel>
                {
                    Chart(
                        "Lab status",
                        "Current pipeline",
                        new[] { "Ordered", "Sample Collected", "In Progress", "Completed", "Cancelled" },
                        new decimal[]
                        {
                            labs.Count(l => l.Status == LabTestStatus.Ordered),
                            labs.Count(l => l.Status == LabTestStatus.SampleCollected),
                            labs.Count(l => l.Status == LabTestStatus.InProgress),
                            labs.Count(l => l.Status == LabTestStatus.Completed),
                            labs.Count(l => l.Status == LabTestStatus.Cancelled)
                        })
                },
                RecentItems = labs.Take(6).Select(l => Recent(l.TestType, l.Patient?.Name ?? "Patient", l.CreatedDate.ToString("dd MMM yyyy"), l.Status.ToString(), "fa-flask", "teal")).ToList()
            };
        }

        private async Task<PortalDashboardViewModel> BuildReceptionistDashboardAsync(CancellationToken ct)
        {
            var appointments = await _db.Appointments
                .AsNoTracking()
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync(ct);
            var patients = await CountUsersInRoleAsync(WebSiteRoles.Website_Patient, ct);
            var doctors = await CountUsersInRoleAsync(WebSiteRoles.Website_Doctor, ct);

            return new PortalDashboardViewModel
            {
                Title = "Reception desk",
                Subtitle = "Manage booking load, staff availability, and front-desk urgency.",
                Metrics = new List<PortalMetricViewModel>
                {
                    Metric("Appointments", appointments.Count, "fa-calendar-check", "blue"),
                    Metric("Patients", patients, "fa-users", "green"),
                    Metric("Doctors", doctors, "fa-user-doctor", "teal")
                },
                Charts = new List<PortalChartViewModel>
                {
                    Chart(
                        "Appointment status",
                        "Front desk queue",
                        new[] { "Scheduled", "Confirmed", "Completed", "Cancelled" },
                        new decimal[]
                        {
                            appointments.Count(a => a.Status == AppointmentStatus.Scheduled),
                            appointments.Count(a => a.Status == AppointmentStatus.Confirmed),
                            appointments.Count(a => a.Status == AppointmentStatus.Completed),
                            appointments.Count(a => a.Status == AppointmentStatus.Cancelled)
                        })
                },
                RecentItems = appointments.Take(6).Select(a => Recent(a.Patient?.Name ?? "Patient", a.Doctor?.Name ?? "Doctor", a.AppointmentDate.ToString("dd MMM yyyy hh:mm tt"), a.Status.ToString(), "fa-desk-bell", "blue")).ToList()
            };
        }

        private async Task<PortalDashboardViewModel> BuildPatientDashboardAsync(string userId, CancellationToken ct)
        {
            var appointments = await _db.Appointments
                .AsNoTracking()
                .Include(a => a.Doctor)
                .Where(a => a.PatientId == userId)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync(ct);
            var bills = await _db.Bills.AsNoTracking().Where(b => b.PatientId == userId).OrderByDescending(b => b.CreatedDate).ToListAsync(ct);
            var labs = await _db.Labs.AsNoTracking().Where(l => l.PatientId == userId).OrderByDescending(l => l.CreatedDate).ToListAsync(ct);
            var reports = await _db.PatientReports
                .AsNoTracking()
                .Include(r => r.Doctor)
                .Where(r => r.PatientId == userId)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync(ct);
            var hospitals = await _db.Hospitals.AsNoTracking().CountAsync(ct);
            var doctors = await CountUsersInRoleAsync(WebSiteRoles.Website_Doctor, ct);

            return new PortalDashboardViewModel
            {
                Title = "Patient portal",
                Subtitle = "Your appointments, bills, labs, and follow-up information in one place.",
                Metrics = new List<PortalMetricViewModel>
                {
                    Metric("Hospitals", hospitals, "fa-hospital", "blue"),
                    Metric("Available doctors", doctors, "fa-user-doctor", "green"),
                    Metric("Appointments", appointments.Count, "fa-calendar-check", "teal"),
                    Metric("Bills", bills.Count, "fa-file-invoice-dollar", "orange"),
                    Metric("Labs", labs.Count, "fa-flask", "purple"),
                    Metric("Reports", reports.Count, "fa-file-medical", "pink")
                },
                Charts = new List<PortalChartViewModel>
                {
                    Chart(
                        "Appointment status",
                        "Your visit pipeline",
                        new[] { "Scheduled", "Confirmed", "Completed", "Cancelled" },
                        new decimal[]
                        {
                            appointments.Count(a => a.Status == AppointmentStatus.Scheduled),
                            appointments.Count(a => a.Status == AppointmentStatus.Confirmed),
                            appointments.Count(a => a.Status == AppointmentStatus.Completed),
                            appointments.Count(a => a.Status == AppointmentStatus.Cancelled)
                        })
                },
                RecentItems = appointments.Take(3).Select(a => Recent(a.Type, a.Doctor?.Name ?? "Doctor", a.AppointmentDate.ToString("dd MMM yyyy hh:mm tt"), a.Status.ToString(), "fa-calendar-day", "blue"))
                    .Concat(bills.Take(2).Select(b => Recent($"Bill #{b.BillNumber}", "Billing summary", b.TotalBill.ToString("C"), b.Status.ToString(), "fa-wallet", "orange")))
                    .Concat(labs.Take(2).Select(l => Recent(l.TestType, l.TestCode, l.CreatedDate.ToString("dd MMM yyyy"), l.Status.ToString(), "fa-vial", "teal")))
                    .Concat(reports.Take(2).Select(r => Recent(r.Diagnose, r.Doctor?.Name ?? "Doctor", r.CreatedDate.ToString("dd MMM yyyy"), "Report", "fa-file-waveform", "green")))
                    .Take(8)
                    .ToList()
            };
        }

        private async Task<PortalDashboardViewModel> BuildAccountantDashboardAsync(CancellationToken ct)
        {
            var bills = await _db.Bills.AsNoTracking().OrderByDescending(b => b.CreatedDate).ToListAsync(ct);
            var payrolls = await _db.Payrolls
                .AsNoTracking()
                .Include(p => p.Employee)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync(ct);
            var months = Enumerable.Range(0, 6).Select(offset => DateTime.UtcNow.AddMonths(offset - 5)).ToList();

            return new PortalDashboardViewModel
            {
                Title = "Finance dashboard",
                Subtitle = "Revenue collection, pending receivables, and payroll visibility.",
                Metrics = new List<PortalMetricViewModel>
                {
                    Metric("Bills", bills.Count, "fa-file-invoice-dollar", "blue"),
                    Metric("Payroll runs", payrolls.Count, "fa-money-check-dollar", "green"),
                    Metric("Paid revenue", bills.Where(b => b.Status == BillStatus.Paid).Sum(b => b.TotalBill).ToString("C0"), "fa-sack-dollar", "teal"),
                    Metric("Pending revenue", bills.Where(b => b.Status == BillStatus.Pending || b.Status == BillStatus.PartiallyPaid).Sum(b => b.TotalBill).ToString("C0"), "fa-clock", "orange")
                },
                Charts = new List<PortalChartViewModel>
                {
                    Chart(
                        "Revenue by month",
                        "Paid bills over last 6 months",
                        months.Select(m => m.ToString("MMM yy")),
                        months.Select(m => bills.Where(b => b.Status == BillStatus.Paid && b.CreatedDate.Year == m.Year && b.CreatedDate.Month == m.Month).Sum(b => b.TotalBill))),
                    Chart(
                        "Bill status",
                        "Current collection state",
                        new[] { "Pending", "Paid", "Overdue", "Partial", "Cancelled" },
                        new decimal[]
                        {
                            bills.Count(b => b.Status == BillStatus.Pending),
                            bills.Count(b => b.Status == BillStatus.Paid),
                            bills.Count(b => b.Status == BillStatus.Overdue),
                            bills.Count(b => b.Status == BillStatus.PartiallyPaid),
                            bills.Count(b => b.Status == BillStatus.Cancelled)
                        })
                },
                RecentItems = bills.Take(4).Select(b => Recent($"Bill #{b.BillNumber}", "Receivable", b.TotalBill.ToString("C"), b.Status.ToString(), "fa-file-invoice", "blue"))
                    .Concat(payrolls.Take(3).Select(p => Recent(p.Employee?.Name ?? "Employee", $"{p.PayPeriodStart:dd MMM} - {p.PayPeriodEnd:dd MMM}", p.NetSalary.ToString("C"), p.Status.ToString(), "fa-money-check-dollar", "green")))
                    .ToList()
            };
        }

        private static PortalMetricViewModel Metric(string label, object value, string icon, string tone, string? description = null)
        {
            return new PortalMetricViewModel
            {
                Label = label,
                Value = value.ToString() ?? "0",
                Icon = icon,
                Tone = tone,
                Description = description
            };
        }

        private async Task<int> CountUsersInRoleAsync(string roleName, CancellationToken ct)
        {
            return await (
                from userRole in _db.UserRoles.AsNoTracking()
                join role in _db.Roles.AsNoTracking() on userRole.RoleId equals role.Id
                where role.Name == roleName
                select userRole.UserId
            ).Distinct().CountAsync(ct);
        }

        private static PortalChartViewModel Chart(string title, string subtitle, IEnumerable<string> labels, IEnumerable<decimal> values)
        {
            return new PortalChartViewModel
            {
                Title = title,
                Subtitle = subtitle,
                Labels = labels.ToList(),
                Values = values.ToList()
            };
        }

        private static PortalRecentItemViewModel Recent(string title, string subtitle, string meta, string status, string icon, string tone)
        {
            return new PortalRecentItemViewModel
            {
                Title = title,
                Subtitle = subtitle,
                Meta = meta,
                Status = status,
                Icon = icon,
                Tone = tone
            };
        }

        private static PortalNavigationItemViewModel Nav(string key, string label, string route, string icon)
        {
            return new PortalNavigationItemViewModel
            {
                Key = key,
                Label = label,
                Route = route,
                Icon = icon
            };
        }

        private static AiAssistantRole? TryMapAiRole(string role)
        {
            return role switch
            {
                WebSiteRoles.Website_Admin => AiAssistantRole.Admin,
                WebSiteRoles.Website_Doctor => AiAssistantRole.Doctor,
                WebSiteRoles.Website_Patient => AiAssistantRole.Patient,
                WebSiteRoles.Website_Nurse => AiAssistantRole.Nurse,
                WebSiteRoles.Website_Pharmacist => AiAssistantRole.Pharmacist,
                WebSiteRoles.Website_LabTech => AiAssistantRole.LabTech,
                WebSiteRoles.Website_Receptionist => AiAssistantRole.Receptionist,
                _ => null
            };
        }

        private static string RoleDisplay(string role)
        {
            return role switch
            {
                WebSiteRoles.Website_Admin => "Administrator",
                WebSiteRoles.Website_Doctor => "Doctor",
                WebSiteRoles.Website_Patient => "Patient",
                WebSiteRoles.Website_Nurse => "Nurse",
                WebSiteRoles.Website_Pharmacist => "Pharmacist",
                WebSiteRoles.Website_LabTech => "Lab Technician",
                WebSiteRoles.Website_Receptionist => "Receptionist",
                WebSiteRoles.Website_Accountant => "Accountant",
                _ => "User"
            };
        }
    }
}
