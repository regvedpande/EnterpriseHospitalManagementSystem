using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Hospital.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Web.Areas.Patient.Controllers
{
    [Area("Patient")]
    [Authorize(Roles = WebSiteRoles.Website_Patient)]
    public class AiController : Hospital.Web.Controllers.AiAssistantControllerBase
    {
        public AiController(IAiAssistantService assistantService) : base(assistantService) { }

        [HttpGet]
        public IActionResult Index() => RenderAssistant(AiAssistantRole.Patient);

        [HttpPost]
        public IActionResult Index(AiAssistantPromptInputModel input) => RenderAssistant(AiAssistantRole.Patient, input.Prompt);
    }
}
