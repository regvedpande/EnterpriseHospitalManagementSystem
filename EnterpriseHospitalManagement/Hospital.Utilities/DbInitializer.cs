using Hospital.Models;
using Hospital.Repositories;
using Hospital.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using Hospital.Models.Enums;

namespace Hospital.Utilities
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DbInitializer(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public void Initialize()
        {
            // Apply any pending EF Core migrations (creates the DB if it doesn't exist).
            // Falls back to EnsureCreated if the migration history table is unavailable.
            try
            {
                _db.Database.Migrate();
            }
            catch (Exception)
            {
                try { _db.Database.EnsureCreated(); } catch { }
            }

            // Seed all roles
            var roles = new[]
            {
                WebSiteRoles.Website_Admin,
                WebSiteRoles.Website_Doctor,
                WebSiteRoles.Website_Patient,
                WebSiteRoles.Website_Nurse,
                WebSiteRoles.Website_Pharmacist,
                WebSiteRoles.Website_LabTech,
                WebSiteRoles.Website_Receptionist,
                WebSiteRoles.Website_Accountant
            };

            foreach (var role in roles)
            {
                if (!_roleManager.RoleExistsAsync(role).GetAwaiter().GetResult())
                {
                    _roleManager.CreateAsync(new IdentityRole(role)).GetAwaiter().GetResult();
                }
            }

            // Seed default users for each role
            SeedUser("admin@hospital.com", "Admin@123", "System Admin", WebSiteRoles.Website_Admin, false);
            SeedUser("doctor@hospital.com", "Doctor@123", "Dr. Sarah Johnson", WebSiteRoles.Website_Doctor, true, "Cardiology");
            SeedUser("patient@hospital.com", "Patient@123", "John Smith", WebSiteRoles.Website_Patient, false);
            SeedUser("nurse@hospital.com", "Nurse@123", "Emily Davis", WebSiteRoles.Website_Nurse, false);
            SeedUser("pharmacist@hospital.com", "Pharmacist@123", "Michael Brown", WebSiteRoles.Website_Pharmacist, false);
            SeedUser("labtech@hospital.com", "LabTech@123", "Lisa Wilson", WebSiteRoles.Website_LabTech, false);
            SeedUser("receptionist@hospital.com", "Receptionist@123", "Anna Taylor", WebSiteRoles.Website_Receptionist, false);
            SeedUser("accountant@hospital.com", "Accountant@123", "Robert Martinez", WebSiteRoles.Website_Accountant, false);

            // Seed a default department
            if (!_db.Departments.Any())
            {
                _db.Departments.AddRange(
                    new Department { Name = "General Medicine", Description = "General medical consultations and treatments" },
                    new Department { Name = "Cardiology", Description = "Heart and cardiovascular system" },
                    new Department { Name = "Orthopedics", Description = "Bone and joint disorders" },
                    new Department { Name = "Pediatrics", Description = "Medical care for infants, children, and adolescents" },
                    new Department { Name = "Neurology", Description = "Nervous system disorders" },
                    new Department { Name = "Dermatology", Description = "Skin conditions and treatments" }
                );
                _db.SaveChanges();
            }

            SeedDemoData();
        }

        private void SeedDemoData()
        {
            // ── Hospital ──────────────────────────────────────────────────────
            if (!_db.Hospitals.Any())
            {
                _db.Hospitals.AddRange(
                    new HospitalInfo { Name = "MedCore Central Hospital", Type = "General", City = "New York", PinCode = "10001", Country = "USA", PhoneNumber = "+1-212-555-0100", Address = "1 Hospital Drive, New York, NY", Email = "info@medcorecentral.com", Description = "Leading general hospital serving the greater New York area." },
                    new HospitalInfo { Name = "City Heart Clinic", Type = "Specialty", City = "New York", PinCode = "10002", Country = "USA", PhoneNumber = "+1-212-555-0200", Address = "22 Park Avenue, New York, NY", Email = "heart@cityclinic.com", Description = "Specialist cardiac care centre." }
                );
                _db.SaveChanges();
            }

            // ── Rooms ─────────────────────────────────────────────────────────
            if (!_db.Rooms.Any())
            {
                var hospital = _db.Hospitals.First();
                _db.Rooms.AddRange(
                    new Room { RoomNumber = "101", Type = "General",     HospitalId = hospital.Id, Status = 0 },
                    new Room { RoomNumber = "201", Type = "ICU",          HospitalId = hospital.Id, Status = 1 },
                    new Room { RoomNumber = "301", Type = "Private",      HospitalId = hospital.Id, Status = 0 },
                    new Room { RoomNumber = "401", Type = "Semi-Private", HospitalId = hospital.Id, Status = 0 }
                );
                _db.SaveChanges();
            }

            // ── Medicines ─────────────────────────────────────────────────────
            if (!_db.Medicines.Any())
            {
                _db.Medicines.AddRange(
                    new Medicine { Name = "Amoxicillin 500mg", Type = "Antibiotic",    Cost = 12.50m,  Description = "Broad-spectrum penicillin antibiotic" },
                    new Medicine { Name = "Paracetamol 500mg", Type = "Analgesic",     Cost = 4.00m,   Description = "Pain relief and fever reducer" },
                    new Medicine { Name = "Metformin 500mg",   Type = "Antidiabetic",  Cost = 8.00m,   Description = "First-line medication for type 2 diabetes" },
                    new Medicine { Name = "Lisinopril 10mg",   Type = "Antihypertensive", Cost = 9.75m, Description = "ACE inhibitor for high blood pressure" },
                    new Medicine { Name = "Atorvastatin 20mg", Type = "Lipid-lowering", Cost = 15.20m, Description = "Statin for cholesterol management" },
                    new Medicine { Name = "Omeprazole 20mg",   Type = "Antacid",       Cost = 7.50m,   Description = "Proton pump inhibitor for acid reflux" },
                    new Medicine { Name = "Salbutamol Inhaler",Type = "Bronchodilator", Cost = 22.00m, Description = "Reliever inhaler for asthma" },
                    new Medicine { Name = "Ibuprofen 400mg",   Type = "NSAID",         Cost = 5.50m,   Description = "Anti-inflammatory pain relief" },
                    new Medicine { Name = "Cetirizine 10mg",   Type = "Antihistamine", Cost = 6.00m,   Description = "Allergy relief" },
                    new Medicine { Name = "Amlodipine 5mg",    Type = "Antihypertensive", Cost = 11.00m, Description = "Calcium channel blocker" }
                );
                _db.SaveChanges();
            }

            // Resolve seeded user IDs
            var doctorUser  = _userManager.FindByEmailAsync("doctor@hospital.com").GetAwaiter().GetResult();
            var patientUser = _userManager.FindByEmailAsync("patient@hospital.com").GetAwaiter().GetResult();
            var labUser     = _userManager.FindByEmailAsync("labtech@hospital.com").GetAwaiter().GetResult();
            if (doctorUser == null || patientUser == null) return;

            // ── Appointments ──────────────────────────────────────────────────
            if (!_db.Appointments.Any())
            {
                var now = DateTime.Now;
                _db.Appointments.AddRange(
                    new Appointment { Number = "APT-001", Type = "Consultation",    CreatedDate = now.AddDays(-30), AppointmentDate = now.AddDays(-29), Status = Models.Enums.AppointmentStatus.Completed,  DoctorId = doctorUser.Id, PatientId = patientUser.Id },
                    new Appointment { Number = "APT-002", Type = "Follow-Up",       CreatedDate = now.AddDays(-25), AppointmentDate = now.AddDays(-24), Status = Models.Enums.AppointmentStatus.Completed,  DoctorId = doctorUser.Id, PatientId = patientUser.Id },
                    new Appointment { Number = "APT-003", Type = "Check-Up",        CreatedDate = now.AddDays(-20), AppointmentDate = now.AddDays(-19), Status = Models.Enums.AppointmentStatus.Cancelled,  DoctorId = doctorUser.Id, PatientId = patientUser.Id },
                    new Appointment { Number = "APT-004", Type = "Lab Review",      CreatedDate = now.AddDays(-15), AppointmentDate = now.AddDays(-14), Status = Models.Enums.AppointmentStatus.Completed,  DoctorId = doctorUser.Id, PatientId = patientUser.Id },
                    new Appointment { Number = "APT-005", Type = "Consultation",    CreatedDate = now.AddDays(-10), AppointmentDate = now.AddDays(-9),  Status = Models.Enums.AppointmentStatus.Confirmed,  DoctorId = doctorUser.Id, PatientId = patientUser.Id },
                    new Appointment { Number = "APT-006", Type = "Emergency",       CreatedDate = now.AddDays(-7),  AppointmentDate = now.AddDays(-6),  Status = Models.Enums.AppointmentStatus.Completed,  DoctorId = doctorUser.Id, PatientId = patientUser.Id },
                    new Appointment { Number = "APT-007", Type = "Consultation",    CreatedDate = now.AddDays(-3),  AppointmentDate = now.AddDays(1),   Status = Models.Enums.AppointmentStatus.Scheduled,  DoctorId = doctorUser.Id, PatientId = patientUser.Id },
                    new Appointment { Number = "APT-008", Type = "Follow-Up",       CreatedDate = now.AddDays(-1),  AppointmentDate = now.AddDays(3),   Status = Models.Enums.AppointmentStatus.Scheduled,  DoctorId = doctorUser.Id, PatientId = patientUser.Id }
                );
                _db.SaveChanges();
            }

            // ── Bills ─────────────────────────────────────────────────────────
            if (!_db.Bills.Any())
            {
                var now = DateTime.Now;
                _db.Bills.AddRange(
                    new Bill { BillNumber = 1001, PatientId = patientUser.Id, Status = Models.Enums.BillStatus.Paid,    CreatedDate = now.AddDays(-29), PaidDate = now.AddDays(-28), DoctorCharge = 200, MedicineCharge = 50, RoomCharge = 150, OperationCharge = 0,   NursingCharge = 50,  LabCharge = 80,  Advance = 0,   NoOfDays = 1, TotalBill = 530  },
                    new Bill { BillNumber = 1002, PatientId = patientUser.Id, Status = Models.Enums.BillStatus.Paid,    CreatedDate = now.AddDays(-24), PaidDate = now.AddDays(-23), DoctorCharge = 150, MedicineCharge = 30, RoomCharge = 0,   OperationCharge = 0,   NursingCharge = 0,   LabCharge = 60,  Advance = 0,   NoOfDays = 1, TotalBill = 240  },
                    new Bill { BillNumber = 1003, PatientId = patientUser.Id, Status = Models.Enums.BillStatus.Pending, CreatedDate = now.AddDays(-14), DoctorCharge = 300, MedicineCharge = 80, RoomCharge = 300, OperationCharge = 500, NursingCharge = 100, LabCharge = 120, Advance = 200, NoOfDays = 2, TotalBill = 1200 },
                    new Bill { BillNumber = 1004, PatientId = patientUser.Id, Status = Models.Enums.BillStatus.Paid,    CreatedDate = now.AddMonths(-2), PaidDate = now.AddMonths(-2).AddDays(1), DoctorCharge = 200, MedicineCharge = 40, RoomCharge = 0, OperationCharge = 0, NursingCharge = 0, LabCharge = 50, Advance = 0, NoOfDays = 1, TotalBill = 290 },
                    new Bill { BillNumber = 1005, PatientId = patientUser.Id, Status = Models.Enums.BillStatus.Overdue, CreatedDate = now.AddDays(-45), DoctorCharge = 250, MedicineCharge = 60, RoomCharge = 150, OperationCharge = 0, NursingCharge = 50, LabCharge = 90, Advance = 100, NoOfDays = 1, TotalBill = 500 }
                );
                _db.SaveChanges();
            }

            // ── Lab Orders ────────────────────────────────────────────────────
            if (!_db.Labs.Any())
            {
                var now = DateTime.Now;
                var techId = labUser?.Id;
                _db.Labs.AddRange(
                    new Lab { LabNumber = "LAB-001", PatientId = patientUser.Id, DoctorId = doctorUser.Id, TechnicianId = techId, TestType = "Complete Blood Count",     TestCode = "CBC",  Status = Models.Enums.LabTestStatus.Completed,        CreatedDate = now.AddDays(-28), CompletedDate = now.AddDays(-27), BloodPressure = 120, Temperature = 98, Weight = "75kg", TestResult = "All values within normal range." },
                    new Lab { LabNumber = "LAB-002", PatientId = patientUser.Id, DoctorId = doctorUser.Id, TechnicianId = techId, TestType = "Lipid Panel",              TestCode = "LIP",  Status = Models.Enums.LabTestStatus.Completed,        CreatedDate = now.AddDays(-23), CompletedDate = now.AddDays(-22), BloodPressure = 118, Temperature = 98, Weight = "75kg", TestResult = "LDL slightly elevated at 130 mg/dL." },
                    new Lab { LabNumber = "LAB-003", PatientId = patientUser.Id, DoctorId = doctorUser.Id, TechnicianId = techId, TestType = "Blood Glucose",            TestCode = "GLU",  Status = Models.Enums.LabTestStatus.InProgress,      CreatedDate = now.AddDays(-5),  BloodPressure = 122, Temperature = 99, Weight = "75kg" },
                    new Lab { LabNumber = "LAB-004", PatientId = patientUser.Id, DoctorId = doctorUser.Id,                        TestType = "Urine Analysis",           TestCode = "UAN",  Status = Models.Enums.LabTestStatus.Ordered,          CreatedDate = now.AddDays(-2),  BloodPressure = 120, Temperature = 98, Weight = "75kg" },
                    new Lab { LabNumber = "LAB-005", PatientId = patientUser.Id, DoctorId = doctorUser.Id, TechnicianId = techId, TestType = "ECG",                      TestCode = "ECG",  Status = Models.Enums.LabTestStatus.SampleCollected,  CreatedDate = now.AddDays(-1),  BloodPressure = 125, Temperature = 98, Weight = "75kg" }
                );
                _db.SaveChanges();
            }

            // ── Patient Reports ───────────────────────────────────────────────
            if (!_db.PatientReports.Any())
            {
                var now = DateTime.Now;
                _db.PatientReports.AddRange(
                    new PatientReport { Diagnose = "Hypertension Stage 1",            Notes = "Patient presents with elevated blood pressure. Prescribing Lisinopril 10mg daily. Lifestyle changes advised.",                                          DoctorId = doctorUser.Id, PatientId = patientUser.Id, CreatedDate = now.AddDays(-29) },
                    new PatientReport { Diagnose = "Upper Respiratory Tract Infection", Notes = "Mild viral URTI. Prescribed rest, paracetamol for fever, and increased fluid intake. Antibiotics not indicated at this stage.",                       DoctorId = doctorUser.Id, PatientId = patientUser.Id, CreatedDate = now.AddDays(-24) },
                    new PatientReport { Diagnose = "Dyslipidaemia",                    Notes = "Elevated LDL cholesterol on lipid panel. Initiating Atorvastatin 20mg. Dietary counselling provided. Repeat lipid panel in 3 months.",               DoctorId = doctorUser.Id, PatientId = patientUser.Id, CreatedDate = now.AddDays(-14) },
                    new PatientReport { Diagnose = "Routine Annual Check-Up",          Notes = "Patient in good general health. BMI within normal range. Blood pressure controlled on current medication. Continue Lisinopril 10mg. Next review in 6 months.", DoctorId = doctorUser.Id, PatientId = patientUser.Id, CreatedDate = now.AddDays(-6) }
                );
                _db.SaveChanges();
            }

            // ── Payroll ───────────────────────────────────────────────────────
            if (!_db.Payrolls.Any())
            {
                var now = DateTime.Now;
                var m2 = now.AddMonths(-2); var m1 = now.AddMonths(-1);
                _db.Payrolls.AddRange(
                    new Payroll { EmployeeId = doctorUser.Id, Status = PayrollStatus.Paid,     PayPeriodStart = new DateTime(m2.Year,m2.Month,1), PayPeriodEnd = new DateTime(m2.Year,m2.Month,DateTime.DaysInMonth(m2.Year,m2.Month)), NetSalary = 9000, HourlySalary = 50, BonusSalary = 500, Compensation = 1000, AccountNumber = "ACC-DOC-001" },
                    new Payroll { EmployeeId = doctorUser.Id, Status = PayrollStatus.Approved, PayPeriodStart = new DateTime(m1.Year,m1.Month,1), PayPeriodEnd = new DateTime(m1.Year,m1.Month,DateTime.DaysInMonth(m1.Year,m1.Month)), NetSalary = 9000, HourlySalary = 50, BonusSalary = 500, Compensation = 1000, AccountNumber = "ACC-DOC-001" },
                    new Payroll { EmployeeId = doctorUser.Id, Status = PayrollStatus.Draft,    PayPeriodStart = new DateTime(now.Year,now.Month,1), PayPeriodEnd = new DateTime(now.Year,now.Month,DateTime.DaysInMonth(now.Year,now.Month)), NetSalary = 9000, HourlySalary = 50, BonusSalary = 500, Compensation = 1000, AccountNumber = "ACC-DOC-001" }
                );
                _db.SaveChanges();
            }
        }

        private void SeedUser(string email, string password, string name, string role, bool isDoctor, string? specialist = null)
        {
            if (_userManager.FindByEmailAsync(email).GetAwaiter().GetResult() == null)
            {
                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    Name = name,
                    Gender = Models.Enums.Gender.Other,
                    Address = "Hospital HQ",
                    DOB = new DateTime(1990, 1, 1),
                    IsDoctor = isDoctor,
                    Specialist = specialist,
                    EmailConfirmed = true
                };

                var result = _userManager.CreateAsync(user, password).GetAwaiter().GetResult();
                if (result.Succeeded)
                {
                    _userManager.AddToRoleAsync(user, role).GetAwaiter().GetResult();
                }
            }
        }
    }
}