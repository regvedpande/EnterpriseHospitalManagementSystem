using System.Linq;
using System.Threading.Tasks;

namespace Hospital.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        IQueryable<T> GetAll();
        T? GetById(int id);
        void Add(T entity);
        void Insert(T entity);
        void Update(T entity);
        void Delete(int id);
        void Delete(T entity);
        void Save();
        Task<int> SaveAsync();
    }
}