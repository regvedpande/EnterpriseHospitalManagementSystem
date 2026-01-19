
# EnterpriseHospitalManagementSystem

A modular, enterprise-grade Hospital Management System built on **.NET 8** with **Microsoft SQL Server**. Designed for scalability, security, and maintainability using Clean Architecture, CQRS, and EF Core.

---

## Description

This system streamlines hospital operations across patient registration, appointments, billing, pharmacy, lab, and EMR workflows. It supports role-based access, audit trails, and multi-department workflows with robust reporting. The architecture is production-ready—optimized for enterprise deployments, CI/CD, and cloud environments.

---

## Features

- **Patient Management:** Registration, demographics, insurance, EMR, visit history.
- **Appointments & Scheduling:** Doctor calendars, slots, rescheduling, reminders.
- **Billing & Payments:** Invoices, discounts, taxes, payment gateways, refunds.
- **Pharmacy & Inventory:** Stock, purchase orders, dispensing, batch/expiry tracking.
- **Laboratory:** Test orders, results, templates, approvals, HL7-ready structure.
- **Ward & Bed Management:** Admissions, transfers, discharges, occupancy.
- **Role-Based Access Control (RBAC):** Admin, Doctor, Nurse, Pharmacist, LabTech, Accountant.
- **Audit & Compliance:** Change logs, access logs, data retention policies.
- **Reporting & Analytics:** Operational dashboards, financial reports, export to CSV/PDF.
- **Multi-Tenant Ready (Optional):** Tenant isolation via schema or discriminator.

---

## Technologies used

- **Backend:** .NET 8 (ASP.NET Core Minimal APIs/Controllers), EF Core 8, MediatR (CQRS)
- **Database:** Microsoft SQL Server (on-prem/Azure SQL)
- **Auth:** ASP.NET Identity, JWT, OAuth2/OpenID Connect (Azure AD optional)
- **Architecture:** Clean Architecture (API, Application, Domain, Infrastructure)
- **Observability:** Serilog, HealthChecks, OpenAPI/Swagger
- **DevOps:** Docker, Docker Compose, GitHub Actions (CI), EF Core Migrations

---

## Prerequisites

- **.NET 8 SDK**
- **SQL Server** (LocalDB, Developer Edition, or Azure SQL)
- **Node.js** (if using a SPA frontend)
- **Docker** (optional, for containerized setup)
- **Git** (for source control)

---

## Installation

### 1. Clone the repository
```bash
git clone https://github.com/your-org/EnterpriseHospitalManagementSystem.git
cd EnterpriseHospitalManagementSystem
```

### 2. Configure database connection
Update `appsettings.json` in `src/Api`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=HospitalDB;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Issuer": "EnterpriseHMS",
    "Audience": "EnterpriseHMS",
    "Key": "SuperSecretKeyChangeMe"
  }
}
```

### 3. Apply migrations and seed data
```bash
dotnet restore
dotnet build

# From Infrastructure or Api project (where DbContext resides)
dotnet ef database update

# Optional: seed initial roles/users/departments
dotnet run --project src/Api -- --seed
```

### 4. Run the application
```bash
dotnet run --project src/Api
```
Swagger UI will be available at `https://localhost:5001/swagger` (or the configured port).

### 5. Docker (optional)
```bash
docker compose up -d
```
This starts the API and SQL Server containers with preconfigured networking.

---

## Usage

### Authentication
- **JWT-based:** Obtain a token via `/api/auth/login` using seeded admin credentials.
- **Roles:** Admin, Doctor, Nurse, Pharmacist, LabTech, Accountant—each with scoped permissions.

### Common endpoints (sample)
- **Patients:** `GET /api/patients`, `POST /api/patients`
- **Appointments:** `GET /api/appointments`, `POST /api/appointments`
- **Billing:** `POST /api/billing/invoices`, `GET /api/billing/invoices/{id}`
- **Pharmacy:** `GET /api/pharmacy/items`, `POST /api/pharmacy/dispense`
- **Lab:** `POST /api/lab/orders`, `GET /api/lab/results/{orderId}`

### Request/response example
```http
POST /api/patients
Content-Type: application/json
Authorization: Bearer <token>

{
  "firstName": "Regved",
  "lastName": "Patil",
  "dob": "1995-01-01",
  "gender": "Male",
  "phone": "+91-XXXXXXXXXX",
  "address": "Kondhali, MH",
  "insuranceNumber": "INS-12345"
}
```

### Health checks & diagnostics
- **Health:** `/health` (DB, disk, dependencies)
- **Swagger/OpenAPI:** `/swagger`
- **Logs:** Serilog sinks (console/file/Seq)

---

## Architecture overview

- **Domain:** Entities, value objects, domain events, business rules.
- **Application:** Use cases, CQRS handlers (MediatR), DTOs, validators.
- **Infrastructure:** EF Core DbContext, repositories, Identity, external services.
- **API:** Controllers/Minimal APIs, filters, DI wiring, authentication/authorization.

### Key patterns
- **CQRS:** Commands for state changes, queries for reads.
- **EF Core 8:** Migrations, owned types, concurrency tokens, soft deletes.
- **RBAC:** Policy-based authorization with role claims.
- **Validation:** FluentValidation for request models.
- **Auditing:** Interceptors for CreatedBy/UpdatedBy/TimeStamps.

---

⚙️ Configuration
You can configure the application using environment variables or appsettings.{Environment}.json.
Common environment variables
- ASPNETCORE_ENVIRONMENT: Development  Staging  Production
- ConnectionStrings__DefaultConnection: Database connection string
- Jwt__Issuer: Token issuer
- Jwt__Audience: Token audience
- Jwt__Key: Secret key for signing JWTs
- Logging__LogLevel__Default: Minimum log level (Information, Warning, Error)
- Serilog__WriteTo__0__Name: Logging sink (e.g., Console, File, Seq)
- HealthChecks__UI__HealthCheckDatabaseConnectionString: Connection string for health check UI storage
Example: Linux/Windows environment variables
export ASPNETCORE_ENVIRONMENT=Production
export ConnectionStrings__DefaultConnection="Server=sqlserver;Database=HospitalDB;User Id=sa;Password=YourStrong!Passw0rd;"
export Jwt__Issuer="EnterpriseHMS"
export Jwt__Audience="EnterpriseHMS"
export Jwt__Key="SuperSecretKeyChangeMe"

Or in PowerShell:
$env:ASPNETCORE_ENVIRONMENT="Production"
$env:ConnectionStrings__DefaultConnection="Server=sqlserver;Database=HospitalDB;User Id=sa;Password=YourStrong!Passw0rd;"
$env:Jwt__Issuer="EnterpriseHMS"
$env:Jwt__Audience="EnterpriseHMS"
$env:Jwt__Key="SuperSecretKeyChangeMe"


🛠️ Contributing
- Fork the repository
- Create a feature branch (git checkout -b feature/xyz)
- Commit changes (git commit -m "Add xyz feature")
- Push to branch (git push origin feature/xyz)
- Open a Pull Request

📜 License
Distributed under the MIT License. See LICENSE for details.

📧 Contact
Project Maintainer: Regved
- 📍 Location: Kondhali, Nagpur, Maharashtra, India
- ✉️ Email: regregd@outlook.conm

 
