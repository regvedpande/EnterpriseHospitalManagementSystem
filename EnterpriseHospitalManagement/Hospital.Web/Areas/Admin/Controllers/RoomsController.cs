using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hospital.ViewModels;

namespace Hospital.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = WebSiteRoles.Website_Admin)]
    public class RoomsController : Controller
    {
        private readonly IRoomService _service;
        public RoomsController(IRoomService service) => _service = service;

        public IActionResult Index(int pageNumber = 1, int pageSize = 10)
        {
            var all = _service.GetAllRooms();
            var paged = new PagedResult<RoomViewModel>
            {
                Data       = all.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList(),
                TotalCount = all.Count(),
                PageNumber = pageNumber,
                PageSize   = pageSize
            };
            return View(paged);
        }

        public IActionResult Create() => View(new RoomViewModel());

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Create(RoomViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            _service.AddRoom(vm);
            TempData["success"] = "Room created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            var room = _service.GetRoomById(id);
            if (room == null) return NotFound();
            return View(room);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Edit(RoomViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            _service.UpdateRoom(vm);
            TempData["success"] = "Room updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            _service.DeleteRoom(id);
            TempData["success"] = "Room deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
