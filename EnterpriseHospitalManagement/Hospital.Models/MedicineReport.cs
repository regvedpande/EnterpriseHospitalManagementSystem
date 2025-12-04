namespace Hospital.Models
{
    public class MedicineReport
    {
        public int Id { get; set; }
        public int SupplierId { get; set; }
        public Supplier Supplier { get; set; }
        public int MedicineId { get; set; }
        public Medicine Medicine { get; set; }
        public string Company { get; set; }
        public int Quantity { get; set; }
        public DateTime ProductionDate { get; set; }
        public DateTime ExpireDate { get; set; }
        public string Country { get; set; }
    }
}
