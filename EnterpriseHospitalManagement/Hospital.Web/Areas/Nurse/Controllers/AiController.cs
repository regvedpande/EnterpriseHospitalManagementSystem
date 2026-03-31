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
        public IActionResult Index() => RenderAssistant(AiAssistantRole.Nurse);

        [HttpPost]
        public IActionResult Index(AiAssistantPromptInputModel input) => RenderAssistant(AiAssistantRole.Nurse, input.Prompt);
    }
}
