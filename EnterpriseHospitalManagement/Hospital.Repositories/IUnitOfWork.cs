using Hospital.Repositories;
using System;
using System.Threading.Tasks;

namespace EnterpriseHospitalManagement.Hospital.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<T> Repository<T>() where T : class;
        void Save();
        Task SaveAsync();
    }
}
