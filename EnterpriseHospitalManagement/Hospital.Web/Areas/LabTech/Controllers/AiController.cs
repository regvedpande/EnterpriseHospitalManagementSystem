using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Hospital.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Areas.LabTech.Controllers
{
    [Area("LabTech")]
    [Authorize(Roles = WebSiteRoles.Website_LabTech)]
    public class AiController : Hospital.Web.Controllers.AiAssistantControllerBase
    {
        public AiController(IAiAssistantService assistantService) : base(assistantService) { }

        [HttpGet]
        public IActionResult Index() => RenderAssistant(AiAssistantRole.LabTech);

        [HttpPost]
        public IActionResult Index(AiAssistantPromptInputModel input) => RenderAssistant(AiAssistantRole.LabTech, input.Prompt);
    }
}
