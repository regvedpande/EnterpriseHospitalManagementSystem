using Hospital.Models;
using Hospital.Repositories;
using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Hospital.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace Hospital.Services
{
    public class ApplicationUserService : IApplicationUserService
    {
        private readonly IGenericRepository<ApplicationUser> _repo;

        public ApplicationUserService(IGenericRepository<ApplicationUser> repo)
        {
            _repo = repo;
        }

        public PagedResult<ApplicationUserViewModel> GetAll(int pageNumber, int pageSize)
        {
            var query = _repo.GetAll().ToList();

            return BuildPagedResult(query, pageNumber, pageSize);
        }

        public PagedResult<ApplicationUserViewModel> GetAllDoctors(int pageNumber, int pageSize)
        {
            var query = _repo.GetAll()
                .Where(x => x.Role == "Doctor")
                .ToList();

            return BuildPagedResult(query, pageNumber, pageSize);
        }

        public PagedResult<ApplicationUserViewModel> GetAllPatients(int pageNumber, int pageSize)
        {
            var query = _repo.GetAll()
                .Where(x => x.Role == "Patient")
                .ToList();

            return BuildPagedResult(query, pageNumber, pageSize);
        }

        public PagedResult<ApplicationUserViewModel> SearchDoctors(int pageNumber, int pageSize, string search)
        {
            var query = _repo.GetAll()
                .Where(x => x.Role == "Doctor" && x.Name.Contains(search))
                .ToList();

            return BuildPagedResult(query, pageNumber, pageSize);
        }

        private static PagedResult<ApplicationUserViewModel> BuildPagedResult(
            List<ApplicationUser> source,
            int pageNumber,
            int pageSize)
        {
            var data = source
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ApplicationUserViewModel(x))
                .ToList();

            return new PagedResult<ApplicationUserViewModel>
            {
                Data = data,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = source.Count
            };
        }
    }
}
