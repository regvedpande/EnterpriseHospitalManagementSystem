using System;

namespace MyApplication.Models
{
    public class TestModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }

        public TestModel()
        {
            // Default constructor
            CreatedAt = DateTime.UtcNow;
            IsActive = true;
        }

        public void PrintDetails()
        {
            Console.WriteLine($"Id: {Id}, Name: {Name}, CreatedAt: {CreatedAt}, IsActive: {IsActive}");
        }
    }
}