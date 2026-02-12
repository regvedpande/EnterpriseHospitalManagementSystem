// Hospital.ViewModels/ViewModelConversionExtensions.cs
using System;
using System.Linq;
using System.Reflection;
using Hospital.Models;

namespace Hospital.ViewModels
{
    public static class ViewModelConversionExtensions
    {
        /// <summary>
        /// Generic reflection-based mapper: copies properties by name (case-insensitive) from view model to model.
        /// </summary>
        public static TModel ConvertViewModelToModel<TModel, TViewModel>(this TViewModel vm)
            where TModel : new()
        {
            var model = new TModel();
            if (vm == null) return model;

            var vmProps = typeof(TViewModel).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var mProps = typeof(TModel).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanWrite);

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
                        var targetType = Nullable.GetUnderlyingType(mProp.PropertyType) ?? mProp.PropertyType;
                        var converted = Convert.ChangeType(vVal, targetType);
                        mProp.SetValue(model, converted);
                    }
                }
                catch
                {
                    // swallow mapping errors - leave defaults
                }
            }

            return model;
        }

        // Specific extension names the codebase expects (ConvertViewModel)
        public static HospitalInfo ConvertViewModel(this HospitalInfoViewModel vm)
            => vm.ConvertViewModelToModel<HospitalInfo, HospitalInfoViewModel>();

        public static Room ConvertViewModel(this RoomViewModel vm)
            => vm.ConvertViewModelToModel<Room, RoomViewModel>();
    }
}
