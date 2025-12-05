using Hospital.Services.Interfaces;
using Hospital.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class RoomsController : Controller
    {
        private readonly IRoomService _roomService;

        public RoomsController(IRoomService roomService)
        {
            _roomService = roomService;
        }

        public IActionResult Index(int pageNumber = 1, int pageSize = 10)
        {
            return View(_roomService.GetAll(pageNumber, pageSize));
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(RoomViewModel vm)
        {
            if (ModelState.IsValid)
            {
                _roomService.InsertRoom(vm);
                return RedirectToAction("Index");
            }
            return View(vm);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var vm = _roomService.GetRoomById(id);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(RoomViewModel vm)
        {
            if (ModelState.IsValid)
            {
                _roomService.UpdateRoom(vm);
                return RedirectToAction("Index");
            }
            return View(vm);
        }

        public IActionResult Delete(int id)
        {
            _roomService.DeleteRoom(id);
            return RedirectToAction("Index");
        }
    }
}
