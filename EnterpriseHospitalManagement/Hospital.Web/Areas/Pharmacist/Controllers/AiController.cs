using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Hospital.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Areas.Pharmacist.Controllers
{
    [Area("Pharmacist")]
    [Authorize(Roles = WebSiteRoles.Website_Pharmacist)]
    public class AiController : Hospital.Web.Controllers.AiAssistantControllerBase
    {
        public AiController(IAiAssistantService assistantService) : base(assistantService) { }

        [HttpGet]
        public Task<IActionResult> Index(CancellationToken ct) => RenderAssistantAsync(AiAssistantRole.Pharmacist, ct: ct);

        [HttpPost]
        public Task<IActionResult> Index(AiAssistantPromptInputModel input, CancellationToken ct) => RenderAssistantAsync(AiAssistantRole.Pharmacist, input.Prompt, ct);
    }
}
