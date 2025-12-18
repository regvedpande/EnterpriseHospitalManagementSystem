using System;
using System.Collections.Generic;

namespace HospitalManagement.Models
{
    public class Hospital
    {
        // Unique identifier for each hospital
        public int Id { get; set; }
        
        // Name of the hospital
        public string Name { get; set; }
        
        // Address of the hospital
        public string Address { get; set; }
        
        // Contact number of the hospital
        public string PhoneNumber { get; set; }
        
        // Number of available beds in the hospital
        public int NumberOfBeds { get; set; }
        
        // Departments available in the hospital 
        public List<string> Departments { get; set; }
        
        // Date when the hospital was added
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Status of the hospital (e.g., Active/Inactive)
        public bool IsActive { get; set; } = true;

        // Constructors
        public Hospital()
        {
            Departments = new List<string>();
        }

        public Hospital(int id, string name, string address, string phoneNumber, int numberOfBeds, List<string> departments)
        {
            Id = id;
            Name = name;
            Address = address;
            PhoneNumber = phoneNumber;
            NumberOfBeds = numberOfBeds;
            Departments = departments ?? new List<string>();
            CreatedAt = DateTime.Now;
            IsActive = true;
        }
    }
}