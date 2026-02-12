using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hospital.Utilities;
using Hospital.ViewModels;
using Hospital.Models;
using Hospital.Repositories;
using Hospital.Services.Interfaces;

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
            var list = _repo.GetAll()?.ToList() ?? new List<Contact>();

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

        public ContactViewModel GetContactById(int contactId)
        {
            var model = _repo.GetById(contactId);
            return model == null ? null : new ContactViewModel(model);
        }

        public void InsertContact(ContactViewModel contactViewModel)
        {
            if (contactViewModel == null)
                return;

            var model = MapViewModelToModel<Contact, ContactViewModel>(contactViewModel);
            _repo.Add(model);
            _repo.Save();
        }

        public void UpdateContact(ContactViewModel contactViewModel)
        {
            if (contactViewModel == null)
                return;

            var id = GetIdFromViewModel(contactViewModel);
            var existing = _repo.GetById(id);
            if (existing == null)
            {
                var newModel = MapViewModelToModel<Contact, ContactViewModel>(contactViewModel);
                _repo.Add(newModel);
            }
            else
            {
                CopyViewModelPropertiesToModel(existing, contactViewModel);
                _repo.Update(existing);
            }

            _repo.Save();
        }

        public void DeleteContact(int contactId)
        {
            var existing = _repo.GetById(contactId);
            if (existing == null)
                return;

            _repo.Delete(existing);
            _repo.Save();
        }

        // -------------------------
        // Reflection helpers (same idea as in DoctorService)
        // -------------------------
        private static TModel MapViewModelToModel<TModel, TViewModel>(TViewModel vm)
            where TModel : new()
        {
            var model = new TModel();
            if (vm == null) return model;

            var vmProps = typeof(TViewModel).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var mProps = typeof(TModel).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite);

            var vmDict = vmProps.ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

            foreach (var mProp in mProps)
            {
                if (!vmDict.TryGetValue(mProp.Name, out var vProp))
                    continue;

                if (!vProp.CanRead)
                    continue;

                var vVal = vProp.GetValue(vm);
                if (vVal == null)
                    continue;

                try
                {
                    if (mProp.PropertyType.IsAssignableFrom(vProp.PropertyType))
                    {
                        mProp.SetValue(model, vVal);
                    }
                    else
                    {
                        var converted = Convert.ChangeType(vVal, Nullable.GetUnderlyingType(mProp.PropertyType) ?? mProp.PropertyType);
                        mProp.SetValue(model, converted);
                    }
                }
                catch
                {
                    // ignore conversion errors
                }
            }

            return model;
        }

        private static void CopyViewModelPropertiesToModel<TModel, TViewModel>(TModel model, TViewModel vm)
        {
            if (model == null || vm == null) return;

            var vmProps = typeof(TViewModel).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var mProps = typeof(TModel).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite);

            var vmDict = vmProps.ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

            foreach (var mProp in mProps)
            {
                if (!vmDict.TryGetValue(mProp.Name, out var vProp))
                    continue;

                if (!vProp.CanRead)
                    continue;

                var vVal = vProp.GetValue(vm);
                if (vVal == null)
                    continue;

                try
                {
                    if (mProp.PropertyType.IsAssignableFrom(vProp.PropertyType))
                    {
                        mProp.SetValue(model, vVal);
                    }
                    else
                    {
                        var converted = Convert.ChangeType(vVal, Nullable.GetUnderlyingType(mProp.PropertyType) ?? mProp.PropertyType);
                        mProp.SetValue(model, converted);
                    }
                }
                catch
                {
                    // swallow
                }
            }
        }

        private static int GetIdFromViewModel<TViewModel>(TViewModel vm)
        {
            if (vm == null) return 0;

            var prop = typeof(TViewModel).GetProperty("Id", BindingFlags.Public | BindingFlags.Instance)
                ?? typeof(TViewModel).GetProperty("ID", BindingFlags.Public | BindingFlags.Instance)
                ?? typeof(TViewModel).GetProperty("ContactId", BindingFlags.Public | BindingFlags.Instance);

            if (prop == null) return 0;

            var val = prop.GetValue(vm);
            try
            {
                return val == null ? 0 : Convert.ToInt32(val);
            }
            catch
            {
                return 0;
            }
        }
    }
}
