using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Areas.Pharmacist.Controllers
{
    [Area("Pharmacist")]
    [Authorize(Roles = WebSiteRoles.Website_Pharmacist)]
    public class HomeController : Controller
    {
        private readonly IMedicineService _meds;
        private readonly IPatientReportService _reports;
        public HomeController(IMedicineService m, IPatientReportService r)
        { _meds = m; _reports = r; }

        public IActionResult Index()
        {
            ViewBag.MedicineCount = _meds.GetAll(1, 1).TotalCount;
            ViewBag.PrescriptionCount = _reports.GetAll(1, 1).TotalCount;
            return View();
        }
    }
}
