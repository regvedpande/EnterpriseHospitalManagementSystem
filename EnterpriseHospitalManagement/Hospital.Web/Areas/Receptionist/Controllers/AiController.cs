using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Hospital.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Areas.Receptionist.Controllers
{
    [Area("Receptionist")]
    [Authorize(Roles = WebSiteRoles.Website_Receptionist)]
    public class AiController : Hospital.Web.Controllers.AiAssistantControllerBase
    {
        public AiController(IAiAssistantService assistantService) : base(assistantService) { }

        [HttpGet]
        public Task<IActionResult> Index(CancellationToken ct) => RenderAssistantAsync(AiAssistantRole.Receptionist, ct: ct);

        [HttpPost]
        public Task<IActionResult> Index(AiAssistantPromptInputModel input, CancellationToken ct) => RenderAssistantAsync(AiAssistantRole.Receptionist, input.Prompt, ct);
    }
}
