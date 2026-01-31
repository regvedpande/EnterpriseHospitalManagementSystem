using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace HospitalApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HospitalController : ControllerBase
    {
        // In-memory list for demo purposes
        private static readonly List<Patient> Patients = new List<Patient>
        {
            new Patient { Id = 1, Name = "John Doe", Age = 45, Diagnosis = "Flu" },
            new Patient { Id = 2, Name = "Jane Smith", Age = 30, Diagnosis = "Asthma" }
        };

        // GET: api/hospital/patients
        [HttpGet("patients")]
        public ActionResult<IEnumerable<Patient>> GetPatients()
        {
            return Ok(Patients);
        }

        // GET: api/hospital/patients/1
        [HttpGet("patients/{id}")]
        public ActionResult<Patient> GetPatientById(int id)
        {
            var patient = Patients.Find(p => p.Id == id);
            if (patient == null)
                return NotFound($"Patient with ID {id} not found.");
            return Ok(patient);
        }

        // POST: api/hospital/patients
        [HttpPost("patients")]
        public ActionResult<Patient> AddPatient([FromBody] Patient newPatient)
        {
            newPatient.Id = Patients.Count + 1;
            Patients.Add(newPatient);
            return CreatedAtAction(nameof(GetPatientById), new { id = newPatient.Id }, newPatient);
        }

        // PUT: api/hospital/patients/1
        [HttpPut("patients/{id}")]
        public ActionResult UpdatePatient(int id, [FromBody] Patient updatedPatient)
        {
            var patient = Patients.Find(p => p.Id == id);
            if (patient == null)
                return NotFound($"Patient with ID {id} not found.");

            patient.Name = updatedPatient.Name;
            patient.Age = updatedPatient.Age;
            patient.Diagnosis = updatedPatient.Diagnosis;

            return NoContent();
        }

        // DELETE: api/hospital/patients/1
        [HttpDelete("patients/{id}")]
        public ActionResult DeletePatient(int id)
        {
            var patient = Patients.Find(p => p.Id == id);
            if (patient == null)
                return NotFound($"Patient with ID {id} not found.");

            Patients.Remove(patient);
            return Ok($"Patient with ID {id} deleted.");
        }
    }

    // Simple Patient model
    public class Patient
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string Diagnosis { get; set; }
    }
}
