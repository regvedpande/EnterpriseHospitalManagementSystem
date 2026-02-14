using System.Linq;
using System.Threading.Tasks;

namespace Hospital.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        IQueryable<T> GetAll();
        T? GetById(int id);

        // Add is commonly used in service code, keep Insert for compatibility
        void Add(T entity);
        void Insert(T entity);

        void Update(T entity);

        // allow deleting by id or by passing the entity
        void Delete(int id);
        void Delete(T entity);

        // synchronous save
        void Save();

        // asynchronous save returns int like EF Core SaveChangesAsync()
        Task<int> SaveAsync();
    }
}
