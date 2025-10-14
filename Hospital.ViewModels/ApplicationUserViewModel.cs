using Hospital.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hospital.ViewModels
{
    public class ApplicationUserViewModel
    {
        public List<ApplicationUser> Doctors { get; set; } = new List<ApplicationUser>();

        public string Name { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string City { get; set; }
        public string Gender { get; set; }
        public bool IsDoctor { get; set; }
        public string Specialist { get; set; }

        public ApplicationUserViewModel()
        {
        }
        public ApplicationUserViewModel(ApplicationUser user)
        {
            Name = user.Name;
            City = user.Address;
            Gender = user.Gender;
            IsDoctor = user.IsDoctor;
            Specialist = user.Specialist;
            UserName = user.UserName;
            Email = user.Email;
        }

        public ApplicationUser ConvertViewModelToModel(ApplicationUserViewModel user)
        {
            return new ApplicationUser
            {
                Name = user.Name,
                Address = user.City,
                Gender = user.Gender,
                IsDoctor = user.IsDoctor,
                Specialist = user.Specialist
                Email = user.Email,
                UserName = user.UserName
            };
        }

        public List<ApplicationUser> Doctors { get; set; } = new List<ApplicationUser>();
    }
}