using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Hospital.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = WebSiteRoles.Website_Admin)]
    public class AiController : Hospital.Web.Controllers.AiAssistantControllerBase
    {
        public AiController(IAiAssistantService assistantService) : base(assistantService) { }

        [HttpGet]
        public Task<IActionResult> Index(CancellationToken ct) => RenderAssistantAsync(AiAssistantRole.Admin, ct: ct);

        [HttpPost]
        public Task<IActionResult> Index(AiAssistantPromptInputModel input, CancellationToken ct) => RenderAssistantAsync(AiAssistantRole.Admin, input.Prompt, ct);
    }
}
