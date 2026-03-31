using System.Security.Claims;
using Hospital.Services.Interfaces;
using Hospital.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Controllers
{
    public abstract class AiAssistantControllerBase : Controller
    {
        private readonly IAiAssistantService _assistantService;

        protected AiAssistantControllerBase(IAiAssistantService assistantService)
        {
            _assistantService = assistantService;
        }

        protected IActionResult RenderAssistant(AiAssistantRole role, string? prompt = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var vm = _assistantService.Build(role, userId, User.Identity?.Name, prompt);
            return View("~/Hospital.Web/Views/Shared/AiAssistant.cshtml", vm);
        }
    }
}
