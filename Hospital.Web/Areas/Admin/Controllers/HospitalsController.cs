using cloudscribe.Pagination.Models;
using Hospital.Models;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Areas.Admin.Controllers
{
    public class HospitalsController : Controller
    {
        private HospitalInfoViewModel _hospitalInfo;
        public HospitalsController(HospitalInfoViewModel hospitalInfo)
        {
            _hospitalInfo = hospitalInfo;
        }
        public IActionResult Index(int pageNumber=1, int pageSize=10)
        {
            return View(_hospitalInfo.GetAll(pageNumber, pageSize));
        }

    }
}
