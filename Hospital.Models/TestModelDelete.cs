using System;

namespace HospitalMedicineSystem
{
    public class Medicine
    {
        public int MedicineId { get; set; }
        public string Name { get; set; }
        public string Manufacturer { get; set; }
        public DateTime ExpiryDate { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }

        public override string ToString()
        {
            return $"MedicineId: {MedicineId}, Name: {Name}, Manufacturer: {Manufacturer}, ExpiryDate: {ExpiryDate.ToShortDateString()}, Price: ${Price:F2}, Quantity: {Quantity}";
        }
    }
}