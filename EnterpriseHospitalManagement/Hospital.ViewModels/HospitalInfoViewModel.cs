using Hospital.Models;

namespace Hospital.ViewModels
{
    public class HospitalInfoViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string City { get; set; }

        public HospitalInfoViewModel() { }

        public HospitalInfoViewModel(HospitalInfo model)
        {
            Id = model.Id;
            Name = model.Name;
            City = model.City;
        }

        public HospitalInfo ToModel()
        {
            return new HospitalInfo
            {
                Id = Id,
                Name = Name,
                City = City
            };
        }
    }
}
