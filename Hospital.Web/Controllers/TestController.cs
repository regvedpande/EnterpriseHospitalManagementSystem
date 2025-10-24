using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using HospitalAPI.Models;
using HospitalAPI.Services;

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

        // GET: api/hospital
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Hospital>>> GetHospitals()
        {
            var hospitals = await _hospitalService.GetAllHospitalsAsync();
            return Ok(hospitals);
        }

        // GET: api/hospital/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Hospital>> GetHospital(int id)
        {
            var hospital = await _hospitalService.GetHospitalByIdAsync(id);
            if (hospital == null)
                return NotFound();

            return Ok(hospital);
        }

        // POST: api/hospital
        [HttpPost]
        public async Task<ActionResult<Hospital>> CreateHospital(Hospital hospital)
        {
            var createdHospital = await _hospitalService.CreateHospitalAsync(hospital);
            return CreatedAtAction(nameof(GetHospital), new { id = createdHospital.Id }, createdHospital);
        }

        // PUT: api/hospital/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateHospital(int id, Hospital hospital)
        {
            if (id != hospital.Id)
                return BadRequest("Hospital ID mismatch");

            var updated = await _hospitalService.UpdateHospitalAsync(hospital);
            if (!updated)
                return NotFound();

            return NoContent();
        }

        // DELETE: api/hospital/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHospital(int id)
        {
            var deleted = await _hospitalService.DeleteHospitalAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}
