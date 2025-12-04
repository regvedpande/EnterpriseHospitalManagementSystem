using cloudscribe.Pagination.Models;
using EnterpriseHospitalManagement.Hospital.ViewModels;
using Hospital.Models;
using Hospital.Repositories;
using Hospital.Services.Interfaces;
using Hospital.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hospital.Services
{
    public class ContactService : IContactService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ContactService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public PagedResult<ContactViewModel> GetAll(int pageNumber, int pageSize)
        {
            int totalCount;
            var modelList = _unitOfWork.Repository<Contact>()
                .GetAll(includeProperties: "Hospital")
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            totalCount = _unitOfWork.Repository<Contact>().GetAll().Count();

            var vmList = ConvertModelToViewModelList(modelList);

            return new PagedResult<ContactViewModel>
            {
                Data = vmList,
                TotalItems = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public ContactViewModel GetContactById(int contactId)
        {
            var model = _unitOfWork.Repository<Contact>().GetById(contactId);
            return new ContactViewModel(model);
        }

        public void InsertContact(ContactViewModel contact)
        {
            var model = contact.ConvertViewModel();
            _unitOfWork.Repository<Contact>().Add(model);
            _unitOfWork.Save();
        }

        public void UpdateContact(ContactViewModel contact)
        {
            var model = contact.ConvertViewModel();
            _unitOfWork.Repository<Contact>().Update(model);
            _unitOfWork.Save();
        }

        public void DeleteContact(int id)
        {
            var model = _unitOfWork.Repository<Contact>().GetById(id);
            _unitOfWork.Repository<Contact>().Delete(model);
            _unitOfWork.Save();
        }

        private List<ContactViewModel> ConvertModelToViewModelList(List<Contact> modelList)
        {
            return modelList.Select(c => new ContactViewModel(c)).ToList();
        }
    }
}
