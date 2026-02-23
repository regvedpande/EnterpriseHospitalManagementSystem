using System.Collections.Generic;
using System.Linq;
using Hospital.Models;
using Hospital.Repositories;
using Hospital.Services.Interfaces;
using Hospital.Utilities;
using Hospital.ViewModels;

namespace Hospital.Services
{
    public class ContactService : IContactService
    {
        private readonly IGenericRepository<Contact> _repo;

        public ContactService(IGenericRepository<Contact> repo)
        {
            _repo = repo;
        }

        public PagedResult<ContactViewModel> GetAll(int pageNumber, int pageSize)
        {
            var list = _repo.GetAll().ToList();
            return new PagedResult<ContactViewModel>
            {
                Data = list
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(c => new ContactViewModel(c))
                    .ToList(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = list.Count
            };
        }

        public ContactViewModel? GetContactById(int contactId)
        {
            var model = _repo.GetById(contactId);
            return model == null ? null : new ContactViewModel(model);
        }

        public void InsertContact(ContactViewModel vm)
        {
            if (vm == null) return;
            var model = vm.ToModel();
            model.Id = 0; // ensure insert
            _repo.Add(model);
            _repo.Save();
        }

        public void UpdateContact(ContactViewModel vm)
        {
            if (vm == null) return;
            var existing = _repo.GetById(vm.Id);
            if (existing == null)
            {
                InsertContact(vm);
                return;
            }
            existing.Email = vm.Email;
            existing.Phone = vm.Phone;
            existing.HospitalId = vm.HospitalInfoId;
            _repo.Update(existing);
            _repo.Save();
        }

        public void DeleteContact(int contactId)
        {
            var existing = _repo.GetById(contactId);
            if (existing == null) return;
            _repo.Delete(existing);
            _repo.Save();
        }
    }
}