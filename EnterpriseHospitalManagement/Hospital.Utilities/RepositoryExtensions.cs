using System.Collections.Generic;
using Hospital.Repositories;

namespace Hospital.Utilities
{
    public static class RepositoryExtensions
    {
        public static void Insert<T>(this IGenericRepository<T> repo, T entity)
            where T : class
        {
            repo.Add(entity);
        }

        public static void Save<T>(this IGenericRepository<T> repo)
            where T : class
        {
            repo.SaveChanges();
        }
    }
}
