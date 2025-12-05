using cloudscribe.Pagination.Models;
using EnterpriseHospitalManagement.Hospital.ViewModels;
using Hospital.Utilities;
using Hospital.ViewModels;

namespace Hospital.Services.Interfaces
{
    public interface IContactService
    {
        PagedResult<ContactViewModel> GetAll(int pageNumber, int pageSize);
        ContactViewModel GetContactById(int contactId);
        void InsertContact(ContactViewModel contact);
        void UpdateContact(ContactViewModel contact);
        void DeleteContact(int id);
    }
}
