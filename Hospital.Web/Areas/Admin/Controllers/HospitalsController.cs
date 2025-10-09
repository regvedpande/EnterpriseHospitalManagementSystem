using Hospital.Models;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Areas.Admin.Controllers
{
    public class HospitalsController : Controller
    {
        private HospitalInfo _hospitalInfo;
        public HospitalsController(HospitalInfo hospitalInfo)
        {
            _hospitalInfo = hospitalInfo;
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
