namespace Hospital.Services.Interfaces
{
    public interface IReportService
    {
        byte[] GenerateHospitalsPdf();
        byte[] GenerateHospitalsExcel();
        byte[] GenerateDoctorsPdf();
        byte[] GenerateDoctorsExcel();
        byte[] GenerateRoomsPdf();
        byte[] GenerateRoomsExcel();
    }
}
