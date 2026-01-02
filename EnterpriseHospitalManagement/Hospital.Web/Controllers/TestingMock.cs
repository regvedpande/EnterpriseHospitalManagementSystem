using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace HospitalAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HospitalController : ControllerBase
    {
        // Mock data for simplicity
        private static List<Hospital> Hospitals = new List<Hospital>
        {
            new Hospital { Id = 1, Name = "City Hospital", Address = "123 Main St", Phone = "123-456-7890" },
            new Hospital { Id = 2, Name = "Green Valley Hospital", Address = "456 Elm St", Phone = "987-654-3210" }
        };

        // GET: api/hospital
        [HttpGet]
        public IActionResult GetHospitals()
        {
            return Ok(Hospitals);
        }

        // GET: api/hospital/{id}
        [HttpGet("{id}")]
        public IActionResult GetHospital(int id)
        {
            var hospital = Hospitals.Find(h => h.Id == id);
            if (hospital == null)
            {
                return NotFound($"Hospital with ID {id} not found.");
            }

            return Ok(hospital);
        }

        // POST: api/hospital
        [HttpPost]
        public IActionResult AddHospital([FromBody] Hospital newHospital)
        {
            if (newHospital == null)
            {
                return BadRequest("Hospital data is invalid.");
            }

            newHospital.Id = Hospitals.Count + 1;
            Hospitals.Add(newHospital);
            return CreatedAtAction(nameof(GetHospital), new { id = newHospital.Id }, newHospital);
        }

        // PUT: api/hospital/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateHospital(int id, [FromBody] Hospital updatedHospital)
        {
            var hospital = Hospitals.Find(h => h.Id == id);
            if (hospital == null)
            {
                return NotFound($"Hospital with ID {id} not found.");
            }

            hospital.Name = updatedHospital.Name;
            hospital.Address = updatedHospital.Address;
            hospital.Phone = updatedHospital.Phone;

            return NoContent();
        }

        // DELETE: api/hospital/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteHospital(int id)
        {
            var hospital = Hospitals.Find(h => h.Id == id);
            if (hospital == null)
            {
                return NotFound($"Hospital with ID {id} not found.");
            }

            Hospitals.Remove(hospital);
            return NoContent();
        }

        // Model for the hospital
        public class Hospital
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Address { get; set; }
            public string Phone { get; set; }
        }
    }
}