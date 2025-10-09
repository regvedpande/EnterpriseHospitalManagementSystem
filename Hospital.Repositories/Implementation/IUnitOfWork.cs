using Hospital.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hospital.Repositories.Implementation
{
    public interface IUnitOfWork
    {
        object GenericRepository<T>();
        IGenericRepository<T> Repository<T>() where T : class;
        void Save();
    }
}