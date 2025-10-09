using Hospital.Repositories.Implementation;
using Hospital.Utilities;
using Hospital.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hospital.Services
{
    public interface IHospitalInfo
    {
        PagedResult<HospitalInfoViewModel> GetAll(int pageNumber, int pageSize);
        HospitalInfoViewModel GetHospitalbyId(int HospitalId);
        HospitalInfoViewModel UpdateHospitalInfo(HospitalInfoViewModel model);
        HospitalInfoViewModel InsertHospitalInfo(HospitalInfoViewModel model);
        HospitalInfoViewModel DeleteHospitalInfo(int id);
    }
}
