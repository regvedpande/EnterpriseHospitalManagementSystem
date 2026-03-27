using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();

        public IActionResult Error() => View();

        public new IActionResult StatusCode(int code)
        {
            ViewBag.StatusCode = code;
            return View();
        }
    }
}
