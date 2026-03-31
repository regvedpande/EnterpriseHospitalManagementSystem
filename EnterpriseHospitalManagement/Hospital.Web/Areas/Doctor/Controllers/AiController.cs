using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Hospital.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Areas.Doctor.Controllers
{
    [Area("Doctor")]
    [Authorize(Roles = WebSiteRoles.Website_Doctor)]
    public class AiController : Hospital.Web.Controllers.AiAssistantControllerBase
    {
        public AiController(IAiAssistantService assistantService) : base(assistantService) { }

        [HttpGet]
        public IActionResult Index() => RenderAssistant(AiAssistantRole.Doctor);

        [HttpPost]
        public IActionResult Index(AiAssistantPromptInputModel input) => RenderAssistant(AiAssistantRole.Doctor, input.Prompt);
    }
}
