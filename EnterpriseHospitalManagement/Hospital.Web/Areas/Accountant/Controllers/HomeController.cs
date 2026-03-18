using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Areas.Accountant.Controllers
{
    [Area("Accountant")]
    [Authorize(Roles = WebSiteRoles.Website_Accountant)]
    public class HomeController : Controller
    {
        private readonly IBillService _bills;
        private readonly IPayrollService _payrolls;
        public HomeController(IBillService b, IPayrollService p)
        { _bills = b; _payrolls = p; }

        public IActionResult Index()
        {
            ViewBag.BillCount = _bills.GetAll(1, 1).TotalCount;
            ViewBag.PayrollCount = _payrolls.GetAll(1, 1).TotalCount;
            return View();
        }
    }
}
