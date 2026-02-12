using System;
using Hospital.Models;

namespace Hospital.ViewModels
{
    public class HospitalInfoViewModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? PinCode { get; set; }
        public string? Country { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Type { get; set; }

        public HospitalInfoViewModel() { }

        // Defensive mapping constructor using reflection so missing/different model property names don't break compilation.
        public HospitalInfoViewModel(object? model)
        {
            if (model == null) return;

            // Id
            var idObj = GetPropertySafely(model, "Id") ?? GetPropertySafely(model, "HospitalId");
            if (idObj != null && int.TryParse(Convert.ToString(idObj), out var idVal))
            {
                Id = idVal;
            }

            // Strings: try common names; Convert.ToString handles int -> string safely.
            Name = FirstNonNullString(model, "Name", "HospitalName", "Title");
            Description = FirstNonNullString(model, "Description", "Desc");
            Address = FirstNonNullString(model, "Address", "Location", "Addr");
            City = FirstNonNullString(model, "City", "Town");
            PinCode = Convert.ToString(GetPropertySafely(model, "PinCode") ?? GetPropertySafely(model, "PostalCode") ?? GetPropertySafely(model, "ZipCode"));
            Country = FirstNonNullString(model, "Country");
            Phone = FirstNonNullString(model, "Phone", "PhoneNumber", "Contact");
            Email = FirstNonNullString(model, "Email", "EmailAddress", "ContactEmail");
            Type = FirstNonNullString(model, "Type", "HospitalType");
        }

        // Helper: try multiple possible property names and return first non-null string.
        private static string? FirstNonNullString(object model, params string[] propNames)
        {
            foreach (var name in propNames)
            {
                var val = GetPropertySafely(model, name);
                if (val != null)
                {
                    return Convert.ToString(val);
                }
            }
            return null;
        }

        // Reflection helper: returns property value or null (no compile-time dependency on model's exact type)
        private static object? GetPropertySafely(object obj, string propName)
        {
            var pi = obj.GetType().GetProperty(propName);
            if (pi == null) return null;
            return pi.GetValue(obj);
        }
    }
}
