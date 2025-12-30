using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace HospitalApp.Controllers
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
        public IActionResult GetHospital(int id)
        {
            var hospital = _hospitalService.GetHospitalById(id);
            if (hospital == null)
                return NotFound();

            return Ok(hospital);
        }

        [HttpPost]
        public IActionResult AddHospital(Hospital hospital)
        {
            _hospitalService.AddHospital(hospital);
            return CreatedAtAction(nameof(GetHospital), new { id = hospital.Id }, hospital);
        }
    }

    // Sample model
    public class Hospital
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    // Service interface
    public interface IHospitalService
    {
        Hospital GetHospitalById(int id);
        void AddHospital(Hospital hospital);
    }
}
