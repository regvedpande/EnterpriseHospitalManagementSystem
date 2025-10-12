using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Hospital.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
        public Gender Gender { get; set; }
        public string Nationality { get; set; }

        public String Address { get; set; }

        public DateTime DOB { get; set; }

        public string Specialist { get; set; }
        public bool IsDoctor { get; set; }
        public string PictureUri { get; set; }

        public string MyProperty { get; set; }
        public Department Department { get; set; }

        public ICollection<Appointment> Appointments { get; set; }
        public ICollection<Payroll> Payrolls { get; set; }
        public ICollection<PatientReport> PatientReports { get; set; }


    }
}

namespace Hospital.Models
{
    public enum Gender
    {
        Male, Female, Other
    }
}