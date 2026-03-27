# MedCore HMS — Enterprise Hospital Management System

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com)
[![ASP.NET Core MVC](https://img.shields.io/badge/ASP.NET%20Core%20MVC-8.0-512BD4)](https://learn.microsoft.com/aspnet/core)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-LocalDB-CC2927?logo=microsoftsqlserver)](https://www.microsoft.com/sql-server)
[![Entity Framework Core](https://img.shields.io/badge/EF%20Core-8.0-512BD4)](https://learn.microsoft.com/ef/core)
[![Chart.js](https://img.shields.io/badge/Chart.js-4.4-FF6384?logo=chartdotjs)](https://www.chartjs.org)
[![Bootstrap](https://img.shields.io/badge/Bootstrap-5-7952B3?logo=bootstrap)](https://getbootstrap.com)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

> A **production-ready**, full-stack hospital management platform with 8 role-based portals, real-time analytics dashboards, Chart.js visualisations, document management, and complete CRUD across every clinical and administrative workflow.

---

## Table of Contents

1. [Overview](#overview)
2. [Live Demo & Credentials](#live-demo--credentials)
3. [System Architecture](#system-architecture)
4. [Layer Breakdown](#layer-breakdown)
5. [Database Entity Relationship Diagram](#database-entity-relationship-diagram)
6. [Authentication & Authorization Flow](#authentication--authorization-flow)
7. [Role Portals & Feature Matrix](#role-portals--feature-matrix)
8. [Dashboard Analytics](#dashboard-analytics)
9. [Project Structure](#project-structure)
10. [Technology Stack](#technology-stack)
11. [Prerequisites](#prerequisites)
12. [Quick Start](#quick-start)
13. [Configuration Reference](#configuration-reference)
14. [Security Practices](#security-practices)
15. [Contributing](#contributing)
16. [License & Contact](#license--contact)

---

## Overview

**MedCore HMS** digitises every department of a hospital into a single, cohesive web application. A surgeon, receptionist, pharmacist, lab technician, accountant, nurse, and patient each see a completely different portal — but all data flows through one shared database, ensuring real-time consistency.

**What makes it enterprise-grade:**
- **8 isolated role portals** each with their own sidebar navigation, dashboard, and workflows
- **Real-time Chart.js analytics** — line charts, doughnut charts, bar charts, horizontal bar charts — all driven by live database data
- **Full CRUD** for 15+ entities (Appointments, Bills, Labs, Medicines, Rooms, Payrolls, Suppliers, Insurance, Departments, Contacts, Patient Reports, Prescriptions, Documents, Hospitals, Users)
- **Document management** — patients upload PDFs, images, Word/Excel files; categorised with analytics
- **PDF + Excel exports** for every major data set
- **Twilio SMS** and email notification hooks
- **JWT API layer** alongside cookie-based MVC authentication
- **Serilog** structured logging with rolling file sinks
- **Responsive UI** — custom CSS design system (no Bootstrap utility classes), DM Sans / DM Serif Display typography

---

## Live Demo & Credentials

After running locally the database is seeded with the following accounts:

| Role | Email | Password |
|---|---|---|
| Admin | `admin@hospital.com` | `Admin@123` |
| Doctor | `doctor@hospital.com` | `Doctor@123` |
| Patient | `patient@hospital.com` | `Patient@123` |
| Nurse | `nurse@hospital.com` | `Nurse@123` |
| Lab Technician | `labtech@hospital.com` | `LabTech@123` |
| Pharmacist | `pharmacist@hospital.com` | `Pharma@123` |
| Receptionist | `receptionist@hospital.com` | `Recept@123` |
| Accountant | `accountant@hospital.com` | `Account@123` |

> **Change all passwords before any production deployment.**

---

## System Architecture

```mermaid
graph TB
    subgraph Browser["🌐 Client Browser"]
        UI[MedCore HMS UI\nResponsive HTML/CSS/JS]
    end

    subgraph WebLayer["Hospital.Web — ASP.NET Core MVC"]
        direction TB
        Auth[AuthController\nLogin · Register · Logout]
        Profile[ProfileController\nEdit · ChangePassword]
        Home[HomeController\nLanding · StatusCode]

        subgraph Areas["Role-Based Areas"]
            direction LR
            Admin["🔴 Admin\n15 controllers"]
            Doctor["🟢 Doctor\n3 controllers"]
            Patient["🔵 Patient\n5 controllers"]
            Nurse["🟣 Nurse\n2 controllers"]
            LabTech["🟡 LabTech\n2 controllers"]
            Pharma["🟠 Pharmacist\n3 controllers"]
            Recept["⚪ Receptionist\n2 controllers"]
            Account["🟤 Accountant\n3 controllers"]
        end

        Layout[_Layout.cshtml\nShared sidebar · topbar · alerts · modals]
        Middleware[Middleware Pipeline\nIdentity · RBAC · StatusCodePages · Serilog]
    end

    subgraph Services["Hospital.Services — Business Logic"]
        direction TB
        IS[IApplicationUserService]
        IAS[IAppointmentService]
        IBS[IBillService]
        ILS[ILabService]
        IMS[IMedicineService]
        IPS[IPayrollService]
        IRS[IPatientReportService]
        IDS[IDocumentService]
        IOther[IHospitalInfoService\nIRoomService · IInsuranceService\nIContactService · IDepartmentService\nISupplierService · IDoctorService]
    end

    subgraph Repos["Hospital.Repositories — Data Access"]
        UoW[Unit of Work]
        GR[Generic Repository&lt;T&gt;]
        DB[(SQL Server\nHospitalDB)]
    end

    subgraph Models["Hospital.Models + Hospital.ViewModels"]
        Entities[Domain Entities\n16 models]
        Enums[Enums\nAppointmentStatus · BillStatus\nLabTestStatus · PayrollStatus\nDocumentType · Gender]
        VMs[ViewModels\nDTO layer between\nControllers ↔ Services]
    end

    Browser -->|HTTP/HTTPS| Middleware
    Middleware --> Auth
    Middleware --> Areas
    Areas --> Services
    Services --> Repos
    Repos --> GR
    GR --> DB
    Models -.->|referenced by| Services
    Models -.->|referenced by| Areas
```

---

## Layer Breakdown

```mermaid
flowchart LR
    A["🌐 Presentation\nHospital.Web\nMVC Controllers\nRazor Views\nCSS Design System"] -->|ViewModels| B["⚙️ Application\nHospital.Services\nBusiness Logic\nService Interfaces\nImplementations"]
    B -->|Domain Models| C["🗄️ Data Access\nHospital.Repositories\nUnit of Work\nGeneric Repository\nEF Core DbContext"]
    C --> D[("💾 SQL Server\nHospitalDB")]
    E["📦 Domain\nHospital.Models\nEntities · Enums"] -.->|used by| B
    E -.->|used by| C
    F["🔧 Utilities\nHospital.Utilities\nWebSiteRoles\nImageOperations\nJwtService\nEmailSender\nTwilioSmsService"] -.->|used by| A
    F -.->|used by| B
```

| Layer | Project | Responsibility |
|---|---|---|
| Presentation | `Hospital.Web` | MVC controllers, Razor views, CSS/JS, Areas |
| ViewModels | `Hospital.ViewModels` | DTO layer — shapes data between controllers and services |
| Services | `Hospital.Services` | All business logic, service interfaces, implementations |
| Repositories | `Hospital.Repositories` | EF Core Unit of Work, Generic Repository, DbContext |
| Models | `Hospital.Models` | Domain entities, enums, navigation properties |
| Utilities | `Hospital.Utilities` | Cross-cutting concerns — roles, JWT, image ops, SMS, email |

---

## Database Entity Relationship Diagram

```mermaid
erDiagram
    ApplicationUser {
        string Id PK
        string Name
        string Email
        string Gender
        string Address
        DateTime DOB
        bool IsDoctor
        string Specialist
        string PictureUri
        int DepartmentId FK
        string Role
    }

    Department {
        int Id PK
        string Name
        string Description
    }

    HospitalInfo {
        int Id PK
        string Name
        string Address
        string Phone
        string Email
        string Website
    }

    Appointment {
        int Id PK
        string Number
        string Type
        DateTime AppointmentDate
        AppointmentStatus Status
        string Description
        string Notes
        string DoctorId FK
        string PatientId FK
    }

    PatientReport {
        int Id PK
        string Diagnose
        string Notes
        DateTime CreatedDate
        string DoctorId FK
        string PatientId FK
    }

    PrescribedMedicine {
        int Id PK
        int MedicineId FK
        int PatientReportId FK
        string Dosage
        int Duration
    }

    Bill {
        int Id PK
        int BillNumber
        BillStatus Status
        decimal DoctorCharge
        decimal MedicineCharge
        decimal RoomCharge
        decimal LabCharge
        decimal NursingCharge
        decimal TotalBill
        int NoOfDays
        string PatientId FK
        int InsuranceId FK
    }

    Insurance {
        int Id PK
        string Provider
        string PolicyNumber
        string PatientId FK
    }

    Lab {
        int Id PK
        string LabNumber
        LabTestStatus Status
        string TestType
        string TestCode
        string TestResult
        string PatientId FK
        string DoctorId FK
        string TechnicianId FK
    }

    Medicine {
        int Id PK
        string Name
        string Type
        decimal Cost
        int Stock
        DateTime ExpiryDate
        int SupplierId FK
    }

    Supplier {
        int Id PK
        string Name
        string ContactPerson
        string Phone
        string Email
    }

    Room {
        int Id PK
        string RoomNumber
        string Type
        Status Status
        decimal PricePerDay
        int HospitalInfoId FK
    }

    Payroll {
        int Id PK
        string EmployeeId FK
        decimal BasicSalary
        decimal Allowances
        decimal Deductions
        decimal NetSalary
        PayrollStatus Status
        DateTime PayDate
    }

    PatientDocument {
        int Id PK
        string FileName
        string OriginalFileName
        string ContentType
        long FileSizeBytes
        DocumentType DocumentType
        string Description
        DateTime UploadedDate
        string PatientId FK
        string UploadedById FK
    }

    Contact {
        int Id PK
        string Name
        string Phone
        string Email
        string Role
        int HospitalInfoId FK
    }

    ApplicationUser ||--o{ Appointment : "Doctor has"
    ApplicationUser ||--o{ Appointment : "Patient has"
    ApplicationUser ||--o{ PatientReport : "Doctor writes"
    ApplicationUser ||--o{ PatientReport : "Patient receives"
    ApplicationUser ||--o{ Bill : "Patient billed"
    ApplicationUser ||--o{ Lab : "ordered for Patient"
    ApplicationUser ||--o{ PatientDocument : "uploads"
    ApplicationUser ||--o{ Payroll : "Employee payroll"
    ApplicationUser }o--|| Department : "belongs to"
    PatientReport ||--o{ PrescribedMedicine : "includes"
    PrescribedMedicine }o--|| Medicine : "references"
    Bill }o--o| Insurance : "covered by"
    Room }o--|| HospitalInfo : "located in"
    Contact }o--|| HospitalInfo : "works at"
    Medicine }o--|| Supplier : "supplied by"
```

---

## Authentication & Authorization Flow

```mermaid
sequenceDiagram
    actor User as 👤 User
    participant Browser
    participant Auth as AuthController
    participant Identity as ASP.NET Identity
    participant DB as SQL Server
    participant Area as Role Area

    User->>Browser: Navigate to /Auth/Login
    Browser->>Auth: GET /Auth/Login
    Auth-->>Browser: Login form

    User->>Browser: Submit email + password
    Browser->>Auth: POST /Auth/Login
    Auth->>Identity: SignInManager.PasswordSignInAsync()
    Identity->>DB: Validate credentials
    DB-->>Identity: User record + roles
    Identity-->>Auth: SignInResult.Succeeded

    Auth->>Auth: Determine role → redirect URL
    Note over Auth: Admin → /Admin/Home<br/>Doctor → /Doctor/Home<br/>Patient → /Patient/Home<br/>... etc.

    Auth-->>Browser: 302 Redirect + Auth Cookie

    Browser->>Area: GET /{Role}/Home/Index
    Area->>Area: [Authorize(Roles = "Website_{Role}")]
    Note over Area: Unauthorised roles get<br/>redirected to /Auth/AccessDenied

    Area->>DB: Load dashboard data
    DB-->>Area: KPIs, chart data, recent records
    Area-->>Browser: Rendered dashboard HTML + Chart.js
```

---

## Role Portals & Feature Matrix

```mermaid
graph LR
    HMS(["🏥 MedCore HMS"])

    HMS --> ADM["🔴 Admin"]
    ADM --> ADM1["User Management"]
    ADM --> ADM2["Appointments · Bills · Labs"]
    ADM --> ADM3["Patient Reports · Insurance"]
    ADM --> ADM4["Hospitals · Rooms · Depts"]
    ADM --> ADM5["Medicines · Suppliers"]
    ADM --> ADM6["Payroll · Documents · Contacts"]
    ADM --> ADM7["PDF/Excel Exports"]
    ADM --> ADM8["3 Chart.js Dashboards"]

    HMS --> DOC["🟢 Doctor"]
    DOC --> DOC1["My Appointments — Create/Edit"]
    DOC --> DOC2["Patient Reports — Write/View"]
    DOC --> DOC3["Prescriptions"]
    DOC --> DOC4["My Schedule & Timings"]
    DOC --> DOC5["PDF/Excel Export"]
    DOC --> DOC6["Trend + Status Charts"]

    HMS --> PAT["🔵 Patient"]
    PAT --> PAT1["My Appointments — Book"]
    PAT --> PAT2["My Bills — View"]
    PAT --> PAT3["My Lab Results"]
    PAT --> PAT4["My Medical Reports"]
    PAT --> PAT5["My Documents — Upload"]
    PAT --> PAT6["Document Analytics"]

    HMS --> NRS["🟣 Nurse"]
    NRS --> NRS1["View & Update Appointments"]
    NRS --> NRS2["Appointment Status Charts"]

    HMS --> LAB["🟡 LabTech"]
    LAB --> LAB1["Lab Orders — Create/Edit"]
    LAB --> LAB2["Record Test Results"]
    LAB --> LAB3["Status Doughnut Chart"]

    HMS --> PHA["🟠 Pharmacist"]
    PHA --> PHA1["Medicines Stock — CRUD"]
    PHA --> PHA2["Prescriptions — View/Dispense"]
    PHA --> PHA3["Medicine Type Bar Chart"]

    HMS --> REC["⚪ Receptionist"]
    REC --> REC1["Schedule Appointments"]
    REC --> REC2["All Appointments — View/Edit"]
    REC --> REC3["Status Overview Chart"]

    HMS --> ACC["🟤 Accountant"]
    ACC --> ACC1["Bills — Create/Edit"]
    ACC --> ACC2["Payroll Management"]
    ACC --> ACC3["Revenue Trend Chart"]
    ACC --> ACC4["Financial Summary"]
```

| Feature | Admin | Doctor | Patient | Nurse | LabTech | Pharmacist | Receptionist | Accountant |
|---|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|
| Dashboard with Charts | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Appointments (View) | ✅ | ✅ | ✅ | ✅ | — | — | ✅ | — |
| Appointments (Create) | ✅ | ✅ | ✅ | — | — | — | ✅ | — |
| Appointments (Edit Status) | ✅ | ✅ | — | ✅ | — | — | ✅ | — |
| Patient Reports | ✅ | ✅ | ✅ (read) | — | — | — | — | — |
| Lab Tests | ✅ | — | ✅ (read) | — | ✅ | — | — | — |
| Bills | ✅ | — | ✅ (read) | — | — | — | — | ✅ |
| Payroll | ✅ | — | — | — | — | — | — | ✅ |
| Medicines | ✅ | — | — | — | — | ✅ | — | — |
| Prescriptions | — | ✅ | — | — | — | ✅ | — | — |
| Documents | ✅ | — | ✅ | — | — | — | — | — |
| Hospitals/Rooms | ✅ | — | — | — | — | — | — | — |
| User Management | ✅ | — | — | — | — | — | — | — |
| PDF/Excel Export | ✅ | ✅ | — | — | — | — | — | — |
| Profile & Password | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |

---

## Dashboard Analytics

Every role gets a purpose-built analytics dashboard. Here is the data flow:

```mermaid
flowchart TD
    DB[(SQL Server)] -->|GetAll / GetByPatient\nGetByDoctor / GetByRole| Services

    subgraph Services["Service Layer — real-time queries"]
        IAppt[IAppointmentService]
        IBill[IBillService]
        ILab[ILabService]
        IMed[IMedicineService]
        IPay[IPayrollService]
        IReport[IPatientReportService]
    end

    Services -->|LINQ aggregation\nin-memory| Controllers

    subgraph Controllers["HomeControllers — per role"]
        CA[Admin HomeController\nKPIs · Appointment trend\nBill status · Revenue]
        CD[Doctor HomeController\nToday count · 6-month trend\nStatus doughnut · Upcoming]
        CN[Nurse HomeController\nAppointment status bar chart]
        CL[LabTech HomeController\nTest status doughnut]
        CP[Pharmacist HomeController\nMedicine-type bar chart]
        CR[Receptionist HomeController\nAppointment status doughnut]
        CAc[Accountant HomeController\nRevenue trend · Bill status]
        CPat[Patient HomeController\nPersonal KPIs · Recent activity]
    end

    Controllers -->|JsonSerializer.Serialize\nViewBag JSON arrays| Views

    subgraph Views["Razor Views — Chart.js"]
        direction LR
        LineChart[Line Chart\nTrend over 6 months]
        BarChart[Bar Chart\nStatus or type counts]
        DoughnutChart[Doughnut Chart\nStatus breakdown]
        HBarChart[Horizontal Bar\nComparison]
        KPI[KPI Stat Cards\nStat + Icon + Link]
        Feed[Activity Feed\nRecent records]
        QuickAct[Quick Actions Grid\n8 shortcuts per role]
    end

    Views --> Browser[🌐 Chart.js 4.4 renders\nin the browser]
```

---

## Project Structure

```
EnterpriseHospitalManagementSystem/
│
├── EnterpriseHospitalManagement.sln
│
└── EnterpriseHospitalManagement/
    │
    ├── Hospital.Models/                 # Domain layer
    │   ├── ApplicationUser.cs           # Extended Identity user
    │   ├── Appointment.cs
    │   ├── Bill.cs
    │   ├── Lab.cs
    │   ├── Medicine.cs
    │   ├── PatientDocument.cs
    │   ├── PatientReport.cs
    │   ├── Payroll.cs
    │   ├── Room.cs
    │   ├── Supplier.cs
    │   └── Enums/
    │       ├── AppointmentStatus.cs     # Scheduled|Confirmed|InProgress|Completed|Cancelled|NoShow
    │       ├── BillStatus.cs            # Pending|PartiallyPaid|Paid|Overdue|Cancelled
    │       ├── DocumentType.cs          # LabReport|Prescription|Insurance|XRay|...
    │       ├── LabTestStatus.cs
    │       ├── PayrollStatus.cs
    │       └── Gender.cs
    │
    ├── Hospital.ViewModels/             # DTO layer
    │   ├── AppointmentViewModel.cs
    │   ├── BillViewModel.cs
    │   ├── DocumentViewModel.cs
    │   └── ...
    │
    ├── Hospital.Repositories/           # Data access layer
    │   ├── ApplicationDbContext.cs      # EF Core DbContext + Identity
    │   ├── IGenericRepository.cs
    │   ├── GenericRepository.cs
    │   ├── IUnitOfWork.cs
    │   └── UnitOfWork.cs
    │
    ├── Hospital.Services/               # Business logic layer
    │   ├── Interfaces/
    │   │   ├── IAppointmentService.cs
    │   │   ├── IBillService.cs
    │   │   └── ...                      # Interface per entity
    │   ├── AppointmentService.cs
    │   ├── BillService.cs
    │   └── ...                          # Implementation per entity
    │
    ├── Hospital.Utilities/              # Cross-cutting concerns
    │   ├── WebSiteRoles.cs              # Role string constants
    │   ├── JwtService.cs
    │   ├── ImageOperations.cs
    │   ├── EmailSender.cs
    │   └── TwilioSmsService.cs
    │
    └── Hospital.Web/                    # Presentation layer
        ├── Program.cs                   # DI wiring, middleware, seeding
        ├── appsettings.json             # Dev config (safe placeholders only)
        │
        ├── Areas/
        │   ├── Admin/
        │   │   ├── Controllers/         # 15 controllers
        │   │   └── Views/               # Create/Edit/Index per entity
        │   ├── Doctor/
        │   │   ├── Controllers/         # HomeController + Appointments + Patients + Doctors
        │   │   └── Views/
        │   ├── Patient/
        │   ├── Nurse/
        │   ├── LabTech/
        │   ├── Pharmacist/
        │   ├── Receptionist/
        │   └── Accountant/
        │
        ├── Controllers/
        │   ├── AuthController.cs        # Login/Register/Logout
        │   ├── HomeController.cs        # Landing + error pages
        │   └── ProfileController.cs     # Edit profile + change password
        │
        └── Views/
            ├── Shared/
            │   ├── _Layout.cshtml       # Main layout — topbar, sidebar, alerts, modals
            │   └── _Pagination.cshtml   # Reusable pagination partial
            ├── Auth/                    # Login, Register, AccessDenied
            ├── Home/                    # Landing, Error, StatusCode
            └── Profile/                 # Index, Edit, ChangePassword
```

---

## Technology Stack

| Category | Technology | Version |
|---|---|---|
| Runtime | .NET | 8.0 |
| Web Framework | ASP.NET Core MVC | 8.0 |
| ORM | Entity Framework Core | 8.0 |
| Database | Microsoft SQL Server / LocalDB | 2019+ |
| Authentication | ASP.NET Identity | 8.0 |
| API Auth | JWT Bearer | — |
| CSS Framework | Custom CSS Design System | — |
| JS Charts | Chart.js | 4.4.0 |
| Icons | Font Awesome | 6.4.0 |
| Typography | DM Sans + DM Serif Display | Google Fonts |
| Bootstrap | Bootstrap | 5.x (utilities only) |
| Logging | Serilog | — |
| SMS | Twilio | — |
| File Transfers | SFTP | — |
| Export | iTextSharp (PDF) + EPPlus (Excel) | — |

---

## Prerequisites

| Requirement | Minimum Version |
|---|---|
| .NET SDK | 8.0 |
| SQL Server | 2019 LocalDB (bundled with VS) **or** SQL Server Developer / Express |
| Visual Studio | 2022 (17.8+) **or** VS Code + C# DevKit |
| Git | Any recent version |

---

## Quick Start

### 1. Clone

```bash
git clone https://github.com/your-org/EnterpriseHospitalManagementSystem.git
cd EnterpriseHospitalManagementSystem
```

### 2. Configure connection string

Edit `EnterpriseHospitalManagement/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=HospitalDB;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "Jwt": {
    "Key": "REPLACE_WITH_MIN_32_CHAR_SECRET_KEY_IN_PROD",
    "Issuer": "EnterpriseHospitalManagement",
    "Audience": "EnterpriseHospitalManagement"
  }
}
```

> For SQL Server Express: `Server=.\\SQLEXPRESS;Database=HospitalDB;Trusted_Connection=True;`

### 3. Restore and run

```bash
cd EnterpriseHospitalManagement
dotnet restore
dotnet run --project Hospital.Web
```

The app auto-creates the database and seeds all roles + demo users on first run. Navigate to `https://localhost:7xxx` (port shown in terminal).

### 4. Login

Use any of the seeded credentials from the [table above](#live-demo--credentials).

---

## Configuration Reference

```mermaid
graph LR
    subgraph appsettings.json["appsettings.json (safe defaults — committed)"]
        CS[ConnectionStrings\nDefaultConnection]
        JWT[Jwt\nKey · Issuer · Audience]
        LOG[Logging\nLogLevel defaults]
    end

    subgraph appsettings.Production.json["appsettings.Production.json (🔒 NOT committed)"]
        PCS[Production connection string]
        PJWT[Real JWT secret key]
        TWILIO[Twilio AccountSid\nAuthToken · FromPhone]
        EMAIL[SMTP Host\nUsername · Password]
    end

    subgraph EnvVars["Environment Variables (recommended for prod)"]
        E1["ConnectionStrings__DefaultConnection"]
        E2["Jwt__Key"]
        E3["Twilio__AccountSid"]
        E4["Twilio__AuthToken"]
        E5["Email__SmtpPassword"]
    end
```

| Variable | Purpose | Example |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | Database connection | `Server=prod-sql;Database=HospitalDB;...` |
| `Jwt__Key` | JWT signing secret (min 32 chars) | Use a strong random string |
| `Jwt__Issuer` | JWT issuer claim | `EnterpriseHospitalManagement` |
| `Jwt__Audience` | JWT audience claim | `EnterpriseHospitalManagement` |
| `Twilio__AccountSid` | Twilio Account SID | `ACxxxxxxxxxxxxxxx` |
| `Twilio__AuthToken` | Twilio Auth Token | Never commit |
| `Twilio__FromPhone` | SMS sender number | `+1XXXXXXXXXX` |
| `Email__SmtpHost` | SMTP server | `smtp.gmail.com` |
| `Email__SmtpPort` | SMTP port | `587` |
| `Email__Username` | SMTP username | `your@email.com` |
| `Email__Password` | SMTP password | Never commit |
| `ASPNETCORE_ENVIRONMENT` | Runtime environment | `Development` / `Production` |

---

## Security Practices

```mermaid
flowchart TD
    A[User Request] --> B{Authenticated?}
    B -- No --> C[Redirect to /Auth/Login]
    B -- Yes --> D{Correct Role?}
    D -- No --> E[Redirect to /Auth/AccessDenied\n403 page]
    D -- Yes --> F[Execute Action]

    F --> G{Sensitive Operation?}
    G -- Delete --> H[Delete Confirm Modal\nCSRF Token validated]
    G -- Password Change --> I[Current password required\nSignIn refreshed after change]
    G -- File Upload --> J[Content-type checked\n20MB limit enforced]
    G -- Other --> K[AntiForgery token on\nall POST forms]
```

**Key security measures implemented:**

- **RBAC**: Every controller action decorated with `[Authorize(Roles = "Website_{Role}")]`
- **CSRF protection**: `@Html.AntiForgeryToken()` on every POST form
- **Input validation**: `[Required]`, `[MaxLength]`, `ModelState.IsValid` checks
- **Scoped data access**: Doctors only see their own appointments; patients only see their own records
- **No secrets in source**: `appsettings.Production.json` gitignored; placeholder values committed
- **Status code pages**: Custom 404/403/500 error pages via `UseStatusCodePagesWithReExecute`
- **Password policy**: ASP.NET Identity with minimum length + digit requirement

---

## Contributing

```mermaid
gitGraph
    commit id: "main"
    branch feature/your-feature
    checkout feature/your-feature
    commit id: "Add feature"
    commit id: "Add tests"
    checkout main
    merge feature/your-feature id: "PR merged"
```

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/my-feature`
3. Make changes, ensure `dotnet build` succeeds with 0 errors
4. Commit: `git commit -m "feat: add my feature"`
5. Push: `git push origin feature/my-feature`
6. Open a Pull Request against `main`

**Commit conventions:** `feat:` · `fix:` · `chore:` · `docs:` · `refactor:`

---

## License & Contact

**License:** MIT — see [LICENSE](LICENSE)

**Maintainer:** Regved Patil
- 📍 Kondhali, Nagpur, Maharashtra, India
- ✉️ [regregd@outlook.com](mailto:regregd@outlook.com)
- 🏥 *Built for enterprise hospital deployments across India and beyond*
