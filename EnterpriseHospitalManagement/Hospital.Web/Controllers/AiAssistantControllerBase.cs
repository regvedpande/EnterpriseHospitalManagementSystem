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

        protected async Task<IActionResult> RenderAssistantAsync(
            AiAssistantRole role,
            string? prompt = null,
            CancellationToken ct = default)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var vm = await _assistantService.BuildAsync(role, userId, User.Identity?.Name, prompt, ct);
            return View("~/Hospital.Web/Views/Shared/AiAssistant.cshtml", vm);
        }
    }
}
