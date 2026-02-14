using Hospital.Models;
using Hospital.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Utilities
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ApplicationDbContext _context;

        public DbInitializer(ApplicationDbContext context)
        {
            _context = context;
        }

        public void Initialize()
        {
            // Safe for first run, no migrations required
            _context.Database.EnsureCreated();
        }
    }
}
