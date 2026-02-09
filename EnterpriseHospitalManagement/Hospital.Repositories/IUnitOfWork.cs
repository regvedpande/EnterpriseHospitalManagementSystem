// IUnitOfWork.cs
using System;
using System.Threading.Tasks;

namespace Hospital.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<T> Repository<T>() where T : class;
        void Save();
        Task SaveAsync();
    }
}
