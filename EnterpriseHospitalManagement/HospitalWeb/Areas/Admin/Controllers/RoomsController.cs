using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Hospital.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = WebSiteRoles.Website_Admin)]
    public class RoomsController : Controller
    {
        private readonly IRoomService _roomService;
        public RoomsController(IRoomService roomService) => _roomService = roomService;

        public IActionResult Index(int pageNumber = 1, int pageSize = 10)
            => View(_roomService.GetAll(pageNumber, pageSize));

        [HttpGet]
        public IActionResult Create() => View(new RoomViewModel());

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Create(RoomViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            _roomService.InsertRoom(vm);
            TempData["success"] = "Room created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var vm = _roomService.GetRoomById(id);
            if (vm == null) return NotFound();
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Edit(RoomViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            _roomService.UpdateRoom(vm);
            TempData["success"] = "Room updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            _roomService.DeleteRoom(id);
            TempData["success"] = "Room deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
