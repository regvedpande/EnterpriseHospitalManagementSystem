using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace HospitalApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HospitalController : ControllerBase
    {
        private readonly IHospitalService _hospitalService;

        public HospitalController(IHospitalService hospitalService)
        {
            _hospitalService = hospitalService;
        }

        [HttpGet]
        public IActionResult GetAllHospitals()
        {
            var hospitals = _hospitalService.GetHospitals();
            return Ok(hospitals);
        }
    }

    public interface IHospitalService
    {
        IEnumerable<string> GetHospitals();
    }
}
