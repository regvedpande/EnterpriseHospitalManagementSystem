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
    public class DoctorService : IDoctorService
    {
        private readonly IGenericRepository<Timing> _repo;

        public DoctorService(IGenericRepository<Timing> repo)
        {
            _repo = repo;
        }

        public PagedResult<TimingViewModel> GetAllTimings(int pageNumber, int pageSize)
        {
            var list = _repo.GetAll()?.ToList() ?? new List<Timing>();

            return new PagedResult<TimingViewModel>
            {
                Data = list
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new TimingViewModel(x))
                    .ToList(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = list.Count
            };
        }

        public TimingViewModel GetTimingById(int timingId)
        {
            var model = _repo.GetById(timingId);
            return model == null ? null : new TimingViewModel(model);
        }

        public void AddTiming(TimingViewModel timingViewModel)
        {
            if (timingViewModel == null)
                return;

            var model = MapViewModelToModel<Timing, TimingViewModel>(timingViewModel);
            _repo.Add(model);
            _repo.Save();
        }

        public void UpdateTiming(TimingViewModel timingViewModel)
        {
            if (timingViewModel == null)
                return;

            // If repo expects the entity instance to update, fetch it first and copy properties.
            var existing = _repo.GetById(GetIdFromViewModel(timingViewModel));
            if (existing == null)
            {
                // If not found, just add as new
                var newModel = MapViewModelToModel<Timing, TimingViewModel>(timingViewModel);
                _repo.Add(newModel);
            }
            else
            {
                CopyViewModelPropertiesToModel(existing, timingViewModel);
                _repo.Update(existing);
            }

            _repo.Save();
        }

        public void DeleteTiming(int timingId)
        {
            var existing = _repo.GetById(timingId);
            if (existing == null)
                return;

            _repo.Delete(existing);
            _repo.Save();
        }

        // -------------------------
        // Helpers: reflection-based copier to avoid compile-time property name assumptions
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
                        // try to convert
                        var converted = Convert.ChangeType(vVal, Nullable.GetUnderlyingType(mProp.PropertyType) ?? mProp.PropertyType);
                        mProp.SetValue(model, converted);
                    }
                }
                catch
                {
                    // ignore any conversion errors — leave default value
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
                    // swallow conversion exceptions
                }
            }
        }

        // Attempt to read an "Id" or "ID" property from the view model (common conventions).
        private static int GetIdFromViewModel<TViewModel>(TViewModel vm)
        {
            if (vm == null) return 0;

            var prop = typeof(TViewModel).GetProperty("Id", BindingFlags.Public | BindingFlags.Instance)
                ?? typeof(TViewModel).GetProperty("ID", BindingFlags.Public | BindingFlags.Instance)
                ?? typeof(TViewModel).GetProperty("TimingId", BindingFlags.Public | BindingFlags.Instance)
                ?? typeof(TViewModel).GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);

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
