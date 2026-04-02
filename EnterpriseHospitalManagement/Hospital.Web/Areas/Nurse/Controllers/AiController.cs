using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Hospital.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Areas.Nurse.Controllers
{
    [Area("Nurse")]
    [Authorize(Roles = WebSiteRoles.Website_Nurse)]
    public class AiController : Hospital.Web.Controllers.AiAssistantControllerBase
    {
        public AiController(IAiAssistantService assistantService) : base(assistantService) { }

        [HttpGet]
        public Task<IActionResult> Index(CancellationToken ct) => RenderAssistantAsync(AiAssistantRole.Nurse, ct: ct);

        [HttpPost]
        public Task<IActionResult> Index(AiAssistantPromptInputModel input, CancellationToken ct) => RenderAssistantAsync(AiAssistantRole.Nurse, input.Prompt, ct);
    }
}
