using System;
using Hospital.Models;

namespace Hospital.ViewModels
{
    public class RoomViewModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }        // label/display name
        public string? Type { get; set; }        // e.g., ICU, General
        public string? Status { get; set; }      // e.g., Available/Occupied
        public int? RoomNumber { get; set; }     // numeric room number if model has it
        public decimal? Price { get; set; }      // optional

        public RoomViewModel() { }

        // Defensive mapping constructor using reflection.
        public RoomViewModel(object? model)
        {
            if (model == null) return;

            // Id
            var idObj = GetPropertySafely(model, "Id") ?? GetPropertySafely(model, "RoomId");
            if (idObj != null && int.TryParse(Convert.ToString(idObj), out var idVal))
            {
                Id = idVal;
            }

            Name = FirstNonNullString(model, "Name", "RoomName", "Title");
            Type = FirstNonNullString(model, "Type", "RoomType");
            Status = FirstNonNullString(model, "Status", "Availability", "State");

            // RoomNumber (could be int or string — try both)
            var rn = GetPropertySafely(model, "RoomNumber") ?? GetPropertySafely(model, "Number");
            if (rn != null && int.TryParse(Convert.ToString(rn), out var roomNum))
            {
                RoomNumber = roomNum;
            }

            // Price attempts
            var priceObj = GetPropertySafely(model, "Price") ?? GetPropertySafely(model, "Cost") ?? GetPropertySafely(model, "Rate");
            if (priceObj != null)
            {
                try
                {
                    Price = Convert.ToDecimal(priceObj);
                }
                catch
                {
                    Price = null;
                }
            }
        }

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

        private static object? GetPropertySafely(object obj, string propName)
        {
            var pi = obj.GetType().GetProperty(propName);
            if (pi == null) return null;
            return pi.GetValue(obj);
        }
    }
}
