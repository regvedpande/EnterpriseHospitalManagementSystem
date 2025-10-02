using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Areas.Patient.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
