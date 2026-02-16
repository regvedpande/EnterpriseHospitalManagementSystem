using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HospitalAPI.Controllers
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetHospitalById(int id)
        {
            var hospital = await _hospitalService.GetHospitalByIdAsync(id);
            if (hospital == null)
            {
                return NotFound();
            }
            return Ok(hospital);
        }

        // Other methods like AddHospital, DeleteHospital, etc.
    }
}