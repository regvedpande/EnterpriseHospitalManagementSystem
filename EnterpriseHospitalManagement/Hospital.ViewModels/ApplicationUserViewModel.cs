using Hospital.Models;
using Hospital.Models.Enums;

namespace Hospital.ViewModels
{
    public class ApplicationUserViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string City { get; set; }
        public Gender Gender { get; set; }
        public bool IsDoctor { get; set; }
        public string Specialist { get; set; }

        public ApplicationUserViewModel()
        {
        }

        public ApplicationUserViewModel(ApplicationUser user)
        {
            Id = user.Id;
            Name = user.Name;
            Email = user.Email;
            UserName = user.UserName;
            City = user.Address;
            Gender = user.Gender;
            IsDoctor = user.IsDoctor;
            Specialist = user.Specialist;
        }
    }
}
