// Controller: HospitalController.cs
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace HospitalApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HospitalController : ControllerBase
    {
        // In-memory list for demo purposes
        private static readonly List<Hospital> _hospitals = new()
        {
            new Hospital { Id = 1, Name = "City Hospital", Address = "123 Main Street" },
            new Hospital { Id = 2, Name = "General Hospital", Address = "456 High Street" }
        };

        // GET: api/hospital
        [HttpGet]
        public ActionResult<IEnumerable<Hospital>> GetAll()
        {
            return Ok(_hospitals);
        }

        // GET: api/hospital/1
        [HttpGet("{id:int}")]
        public ActionResult<Hospital> GetById(int id)
        {
            var hospital = _hospitals.FirstOrDefault(h => h.Id == id);
            if (hospital == null)
                return NotFound();

            return Ok(hospital);
        }

        // POST: api/hospital
        [HttpPost]
        public ActionResult<Hospital> Create(Hospital hospital)
        {
            hospital.Id = _hospitals.Count == 0 ? 1 : _hospitals.Max(h => h.Id) + 1;
            _hospitals.Add(hospital);

            return CreatedAtAction(nameof(GetById), new { id = hospital.Id }, hospital);
        }

        // PUT: api/hospital/1
        [HttpPut("{id:int}")]
        public IActionResult Update(int id, Hospital updated)
        {
            var hospital = _hospitals.FirstOrDefault(h => h.Id == id);
            if (hospital == null)
                return NotFound();

            hospital.Name = updated.Name;
            hospital.Address = updated.Address;

            return NoContent();
        }

        // DELETE: api/hospital/1
        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var hospital = _hospitals.FirstOrDefault(h => h.Id == id);
            if (hospital == null)
                return NotFound();

            _hospitals.Remove(hospital);
            return NoContent();
        }
    }
}
