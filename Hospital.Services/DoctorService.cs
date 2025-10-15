using Hospital.Models;
using Hospital.Repositories.Implementation;
using Hospital.Services;
using Hospital.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

public class DoctorService : IDoctorService
{
    private IUnitOfWork _unitOfWork;

    public DoctorService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public void AddTiming(TimingViewModel timing)
    {
        throw new NotImplementedException();
    }

    public void AddTiming(TimingViewModel timing)
    {
        var model = new TimingViewModel().ConvertViewModel(timimg);
        _unitOfWork.GenericRepository<Timing>().Add(model);
        _unitOfWork.Save();

    }

    public void DeleteTiming(int TimingId)
    {
        var model = _unitOfWork.GenericRepository<Timing>().GetById(TimingId);
        _unitOfWork.GenericRepository<Timing>().Delete(model);
        _unitOfWork.Save();
    }

    public PagedResult<TimingViewModel> GetAll(int pageNumber, int pageSize)
    {
        var vm = new TimingViewModel();
        int totalCount;
        List<TimingViewModel> vmList = new List<TimingViewModel>();
        try
        {
            int ExcludeRecords = (pageSize * pageNumber) - pageSize;

            var modelList = _unitOfWork.GenericRepository<Timing>()
                .GetAll()
                .Skip(ExcludeRecords)
                .Take(pageSize)
                .ToList();

            totalCount = _unitOfWork.GenericRepository<Timing>().GetAll().ToList().Count;

            vmList = ConvertModelToViewModelList(modelList);
        }
        catch (Exception ex)
        {
            // Handle exception
        }

        return new PagedResult<TimingViewModel>
        {
            Data = vmList,
            TotalItems = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    // Note: The `GetAll()` method without parameters and other methods like `GetTimingById`, `UpdateTiming` are not implemented in the image.
    // You may need to implement them according to your requirements.
    public IEnumerable<TimingViewModel> GetAll()
    {
        throw new NotImplementedException();
    }

    public TimingViewModel GetTimingById(int TimingId)
    {
        throw new NotImplementedException();
    }

    public void UpdateTiming(TimingViewModel timing)
    {
        throw new NotImplementedException();
    }

    public void UpdateTiming(TimingViewModel timing)
    {
        throw new NotImplementedException();
    }

    // Helper method to convert a list of Timing models to a list of TimingViewModel
    private List<TimingViewModel> ConvertModelToViewModelList(List<Timing> modelList)
    {
        // Implement conversion logic here
        return modelList.Select(model => new TimingViewModel(model)).ToList();
    }

    cloudscribe.Pagination.Models.PagedResult<TimingViewModel> IDoctorService.GetAll(int pageNumber, int pageSize)
    {
        throw new NotImplementedException();
    }

    IEnumerable<TimingViewModel> IDoctorService.GetAll()
    {
        throw new NotImplementedException();
    }

    TimingViewModel IDoctorService.GetTimingById(int TimingId)
    {
        throw new NotImplementedException();
    }
}
