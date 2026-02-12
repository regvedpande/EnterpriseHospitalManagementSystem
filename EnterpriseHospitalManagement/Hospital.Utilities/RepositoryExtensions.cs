// Hospital.Repositories/RepositoryExtensions.cs
using System;
using System.Reflection;

namespace Hospital.Repositories
{
    public static class RepositoryExtensions
    {
        /// <summary>
        /// Provide a Save extension that will call Save() or SaveChanges() on the concrete repository if present.
        /// This overload matches repositories of the common pattern where T is a reference type.
        /// </summary>
        public static void Save<T>(this IGenericRepository<T> repo) where T : class
        {
            if (repo == null) return;

            var type = repo.GetType();

            // check for Save()
            var mi = type.GetMethod("Save", BindingFlags.Public | BindingFlags.Instance);
            if (mi != null)
            {
                mi.Invoke(repo, null);
                return;
            }

            // check for SaveChanges()
            mi = type.GetMethod("SaveChanges", BindingFlags.Public | BindingFlags.Instance);
            if (mi != null)
            {
                mi.Invoke(repo, null);
                return;
            }

            // no-op if neither method exists (some repo patterns persist immediately)
        }

        /// <summary>
        /// Legacy name alternative — some code used SaveChanges() directly; provide that too.
        /// </summary>
        public static void SaveChanges<T>(this IGenericRepository<T> repo) where T : class
        {
            Save(repo);
        }

        /// <summary>
        /// AddOrUpdate helper that will try Update, then Add if Update fails.
        /// Keeps same constraint to match IGenericRepository<T>.
        /// </summary>
        public static void AddOrUpdate<T>(this IGenericRepository<T> repo, T entity) where T : class
        {
            if (repo == null || entity == null) return;

            var type = repo.GetType();
            var updateMi = type.GetMethod("Update", BindingFlags.Public | BindingFlags.Instance);
            if (updateMi != null)
            {
                try
                {
                    updateMi.Invoke(repo, new object[] { entity });
                    return;
                }
                catch
                {
                    // fall through to add
                }
            }

            var addMi = type.GetMethod("Add", BindingFlags.Public | BindingFlags.Instance)
                     ?? type.GetMethod("Insert", BindingFlags.Public | BindingFlags.Instance);

            if (addMi != null)
            {
                addMi.Invoke(repo, new object[] { entity });
            }
        }
    }
}
