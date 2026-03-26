# MedCore HMS — Enterprise Hospital Management System

<div align="center">

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-MVC-blue?style=for-the-badge&logo=dotnet)
![EF Core](https://img.shields.io/badge/EF_Core-8.0-orange?style=for-the-badge)
![SQL Server](https://img.shields.io/badge/SQL_Server-LocalDB-CC2927?style=for-the-badge&logo=microsoftsqlserver)
![Bootstrap](https://img.shields.io/badge/Bootstrap-5-7952B3?style=for-the-badge&logo=bootstrap)
![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)

**A production-ready, role-based hospital management platform built for enterprise healthcare.**

[Features](#-features) · [Architecture](#-architecture) · [Roles](#-role-based-access-control) · [Quick Start](#-quick-start) · [API](#-rest-api)

</div>

---

## Overview

MedCore HMS is a full-stack enterprise web application that unifies hospital operations — from patient intake and clinical workflows to pharmacy, lab management, billing, and HR payroll — into a single, secure, role-aware platform.

Built on **ASP.NET Core 8 MVC** with **Entity Framework Core** and **ASP.NET Identity**, it supports **8 distinct roles**, a **JWT-secured REST API**, PDF/Excel reporting, and a modern responsive UI inspired by SaaS dashboards.

---

## ✨ Features

| Domain | Capabilities |
|---|---|
| **Clinical** | Appointment scheduling, patient reports, diagnoses, prescription writing |
| **Laboratory** | Lab test ordering, technician assignment, result entry, status tracking |
| **Pharmacy** | Medicine inventory, prescription fulfilment, expiry management |
| **Billing & Finance** | Patient billing with insurance integration, payroll processing |
| **Facility** | Multi-hospital branches, room management, department structure |
| **HR** | Staff management, role assignment, payroll (Draft → Approved → Paid) |
| **Security** | 8-role RBAC, JWT API auth, cookie session auth, CSRF protection |
| **Reporting** | PDF & Excel exports (hospitals, rooms, doctor schedules) |

---

## 🏗️ Architecture

### System Overview

```mermaid
graph TB
    subgraph Client["Client Layer"]
        B[Browser / MVC Views]
        API[REST API Client]
    end

    subgraph Web["ASP.NET Core 8 Web App"]
        direction TB
        MVC["MVC Controllers\n8 Role Areas"]
        APIC["API Controllers\nJWT Auth"]
        MW["Middleware\nSerilog · Cookie Auth · JWT Bearer"]
    end

    subgraph Services["Service Layer"]
        AS[AppointmentService]
        BS[BillService]
        LS[LabService]
        PS[PatientReportService]
        MS[MedicineService]
        RS["ReportService\nPDF · Excel"]
        US[UserService]
        PAY[PayrollService]
    end

    subgraph Data["Data Layer"]
        UOW[Unit of Work]
        REPO["Generic Repository&lt;T&gt;"]
        DB[("SQL Server\nLocalDB")]
    end

    subgraph Identity["ASP.NET Identity"]
        UM[UserManager]
        RM[RoleManager]
        SM[SignInManager]
    end

    B --> MVC
    API --> APIC
    MVC --> MW
    APIC --> MW
    MW --> Services
    Services --> UOW
    UOW --> REPO
    REPO --> DB
    MVC --> Identity
    APIC --> Identity
```

### Database Schema

```mermaid
erDiagram
    ApplicationUser {
        string Id PK
        string Name
        string Gender
        string Specialist
        bool IsDoctor
        int DepartmentId FK
    }
    Appointment {
        int Id PK
        string DoctorId FK
        string PatientId FK
        datetime AppointmentDate
        string Status
        string Type
    }
    PatientReport {
        int Id PK
        string DoctorId FK
        string PatientId FK
        string Diagnose
        datetime CreatedDate
    }
    PrescribedMedicine {
        int Id PK
        int PatientReportId FK
        int MedicineId FK
        string Dosage
        string Duration
    }
    Bill {
        int Id PK
        string PatientId FK
        int InsuranceId FK
        string Status
        decimal TotalBill
        datetime CreatedDate
    }
    Lab {
        int Id PK
        string PatientId FK
        string DoctorId FK
        string TechnicianId FK
        string TestType
        string Status
        string TestResult
    }
    Payroll {
        int Id PK
        string EmployeeId FK
        string Status
        decimal NetSalary
        datetime PayPeriodStart
        datetime PayPeriodEnd
    }
    Medicine {
        int Id PK
        string Name
        string Type
        decimal Cost
    }
    HospitalInfo {
        int Id PK
        string Name
        string City
        string Country
        string PhoneNumber
        string Email
    }
    Room {
        int Id PK
        int HospitalId FK
        string RoomNumber
        string Type
        int Status
    }
    Insurance {
        int Id PK
        string PatientId FK
        string PolicyNumber
        decimal CoverageAmount
    }
    Department {
        int Id PK
        string Name
        string Description
    }
    Supplier {
        int Id PK
        string Company
        string Email
        string Phone
    }

    ApplicationUser ||--o{ Appointment : "as doctor"
    ApplicationUser ||--o{ Appointment : "as patient"
    ApplicationUser ||--o{ PatientReport : "writes"
    ApplicationUser ||--o{ PatientReport : "receives"
    ApplicationUser ||--o{ Bill : "billed"
    ApplicationUser ||--o{ Lab : "tested"
    ApplicationUser ||--o{ Lab : "processes"
    ApplicationUser ||--o{ Payroll : "paid"
    PatientReport ||--o{ PrescribedMedicine : "contains"
    Medicine ||--o{ PrescribedMedicine : "prescribed in"
    Bill ||--o| Insurance : "covered by"
    HospitalInfo ||--o{ Room : "has"
    ApplicationUser }o--|| Department : "belongs to"
```

### Request Lifecycle

```mermaid
sequenceDiagram
    participant U as User (Browser)
    participant MW as Middleware
    participant C as Controller
    participant S as Service
    participant R as Repository
    participant DB as SQL Server

    U->>MW: HTTP Request
    MW->>MW: Authenticate (Cookie / JWT)
    MW->>MW: Authorize (Role check)
    MW->>C: Route to Area Controller
    C->>S: Call Service Method
    S->>R: Query via Repository
    R->>DB: EF Core SQL Query
    DB-->>R: Result Set
    R-->>S: Domain Models
    S-->>C: ViewModels (PagedResult)
    C-->>U: Razor View / JSON Response
```

---

## 🔐 Role-Based Access Control

Eight roles, each with a dedicated area, sidebar navigation, and tailored dashboard:

```mermaid
graph LR
    subgraph Roles["8 Distinct Roles"]
        ADM["🛡️ Admin"]
        DOC["🩺 Doctor"]
        NUR["💉 Nurse"]
        PAT["🏥 Patient"]
        LAB["🔬 Lab Tech"]
        PHA["💊 Pharmacist"]
        REC["📋 Receptionist"]
        ACC["💰 Accountant"]
    end

    subgraph AdminScope["Admin — Full Control"]
        A1["Hospitals · Rooms · Departments"]
        A2["Appointments · Labs · Reports"]
        A3["Medicines · Suppliers · Bills"]
        A4["Payrolls · Insurance · Users"]
    end

    subgraph ClinicalScope["Clinical Staff"]
        D1["Doctor: Appointments · Patient Reports · Timings"]
        N1["Nurse: Appointment Monitoring"]
        L1["Lab Tech: Test Orders · Results"]
        P1["Pharmacist: Inventory · Prescriptions"]
    end

    subgraph PatientScope["Patient Portal"]
        PA1["Book Appointments"]
        PA2["View Bills · Lab Results"]
        PA3["Medical Reports"]
    end

    subgraph FinanceScope["Operations"]
        AC1["Accountant: Bills · Payrolls"]
        RE1["Receptionist: Scheduling"]
    end

    ADM --> AdminScope
    DOC --> D1
    NUR --> N1
    LAB --> L1
    PHA --> P1
    PAT --> PatientScope
    ACC --> AC1
    REC --> RE1
```

### Permissions Matrix

| Feature | Admin | Doctor | Nurse | Patient | Lab Tech | Pharmacist | Receptionist | Accountant |
|---|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|
| Hospitals CRUD | ✅ | | | | | | | |
| Rooms CRUD | ✅ | | | | | | | |
| Departments CRUD | ✅ | | | | | | | |
| Appointments | ✅ All | View/Edit | Edit Status | Create/View | | | Create/Edit | |
| Patient Reports | ✅ View | ✅ Full CRUD | | View own | | | | |
| Prescriptions | ✅ | ✅ Write | | | | ✅ View | | |
| Lab Tests | ✅ | Order | | View own | ✅ Process | | | |
| Medicines | ✅ | | | | | ✅ Full | | |
| Bills | ✅ | | | View own | | | | ✅ Full |
| Payrolls | ✅ | | | | | | | ✅ Full |
| Insurance | ✅ | | | | | | | |
| User Management | ✅ | | | | | | | |
| PDF/Excel Export | ✅ | ✅ | | | | | | |
| REST API | ✅ | ✅ | | | | | | |

---

## 📊 Clinical Workflows

### Appointment Lifecycle

```mermaid
stateDiagram-v2
    [*] --> Scheduled : Receptionist or Patient books
    Scheduled --> Completed : Doctor or Nurse marks done
    Scheduled --> Cancelled : Any authorised user
    Scheduled --> Rescheduled : Receptionist reschedules
    Rescheduled --> Scheduled : New slot confirmed
    Completed --> [*]
    Cancelled --> [*]
```

### Bill Lifecycle

```mermaid
stateDiagram-v2
    [*] --> Pending : Accountant creates bill
    Pending --> Paid : Payment received
    Pending --> Overdue : Deadline passed
    Pending --> Cancelled : Bill voided
    Overdue --> Paid : Late payment received
    Paid --> [*]
    Cancelled --> [*]
```

### Payroll Workflow

```mermaid
stateDiagram-v2
    [*] --> Draft : Accountant creates
    Draft --> Pending : Submitted for review
    Pending --> Approved : Admin approves
    Pending --> Rejected : Admin rejects
    Approved --> Paid : Payment processed
    Rejected --> Draft : Revised and resubmitted
    Paid --> [*]
```

### Lab Test Flow

```mermaid
sequenceDiagram
    participant D as Doctor
    participant L as Lab Tech
    participant P as Patient

    D->>Lab System: Order test (type, patient)
    Lab System-->>L: Test appears as Ordered
    L->>Lab System: Start processing → In Progress
    L->>Lab System: Enter results → Completed
    Lab System-->>P: Results visible in Patient Portal
    Lab System-->>D: Results linked to Patient Report
```

---

## 🛠️ Tech Stack

```mermaid
graph TD
    subgraph Frontend["Frontend"]
        R["Razor Views MVC"]
        B5["Bootstrap 5"]
        FA["Font Awesome 6"]
        CSS["Custom CSS Design System\nDM Sans · DM Serif Display"]
    end

    subgraph Backend["Backend"]
        ASPNET["ASP.NET Core 8"]
        EF["Entity Framework Core 8"]
        ID["ASP.NET Identity"]
        JWT["JWT Bearer Auth"]
        SL["Serilog Logging\nConsole + Rolling File"]
    end

    subgraph Libraries["Libraries"]
        QP["QuestPDF — PDF Reports"]
        CX["ClosedXML — Excel Reports"]
        MK["MailKit — SMTP Email"]
        SSH["SSH.NET — SFTP"]
        CSV["CsvHelper — CSV Export"]
    end

    subgraph Storage["Storage"]
        MSSQL["SQL Server / LocalDB"]
        FS["File System Logs"]
    end

    Frontend --> Backend
    Backend --> Libraries
    Backend --> Storage
```

---

## 🚀 Quick Start

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- SQL Server or SQL Server LocalDB _(included with Visual Studio)_

### 1. Clone
```bash
git clone https://github.com/YOUR_USERNAME/EnterpriseHospitalManagementSystem.git
cd EnterpriseHospitalManagementSystem
```

### 2. Configure
```bash
cd EnterpriseHospitalManagement
cp appsettings.example.json appsettings.json
# Open appsettings.json and set your connection string and JWT secret
```

### 3. Run
```bash
dotnet run
# App starts at https://localhost:5001
```

> The database is **created and seeded automatically** on first run — no manual migrations needed.

### 4. Login with Demo Accounts

The login page has a **click-to-fill credentials panel** — just click any role to auto-fill.

| Role | Email | Password |
|---|---|---|
| 🛡️ Admin | admin@hospital.com | Admin@123 |
| 🩺 Doctor | doctor@hospital.com | Doctor@123 |
| 💉 Nurse | nurse@hospital.com | Nurse@123 |
| 🏥 Patient | patient@hospital.com | Patient@123 |
| 🔬 Lab Tech | labtech@hospital.com | LabTech@123 |
| 💊 Pharmacist | pharmacist@hospital.com | Pharmacist@123 |
| 📋 Receptionist | receptionist@hospital.com | Receptionist@123 |
| 💰 Accountant | accountant@hospital.com | Accountant@123 |

---

## 🌐 REST API

JWT-authenticated endpoints for system integrations and mobile clients.

### Get a Token
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "admin@hospital.com",
  "password": "Admin@123"
}
```
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "expiration": "2024-01-01T13:00:00Z"
}
```

### Authenticated Request
```http
GET /api/...
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

Token validity: **60 minutes**

---

## 📁 Project Structure

```
EnterpriseHospitalManagementSystem/
├── EnterpriseHospitalManagement/          # Single deployable ASP.NET Core project
│   ├── Hospital.Models/                   # EF Core domain models + enums
│   ├── Hospital.ViewModels/               # DTOs and form view models
│   ├── Hospital.Repositories/             # Generic repository + Unit of Work
│   ├── Hospital.Services/                 # Business logic layer + interfaces
│   ├── Hospital.Utilities/                # DbInitializer, WebSiteRoles, JWT helper
│   ├── Hospital.Web/
│   │   ├── Areas/
│   │   │   ├── Admin/                     # Full system admin
│   │   │   ├── Doctor/                    # Clinical workflows
│   │   │   ├── Patient/                   # Patient self-service portal
│   │   │   ├── Nurse/                     # Appointment monitoring
│   │   │   ├── LabTech/                   # Lab test processing
│   │   │   ├── Pharmacist/                # Pharmacy management
│   │   │   ├── Receptionist/              # Front-desk scheduling
│   │   │   └── Accountant/                # Finance and payroll
│   │   └── Views/
│   │       ├── Auth/                      # Login + Register
│   │       ├── Home/                      # Public landing page
│   │       └── Shared/_Layout.cshtml      # Master layout (8 role sidebars)
│   ├── Migrations/                        # EF Core migration history
│   ├── wwwroot/css/site.css               # Design system (variables, components)
│   ├── Program.cs                         # DI, middleware, routing config
│   └── appsettings.example.json           # Config template (safe to commit)
└── README.md
```

---

## 🔒 Security

| Concern | Implementation |
|---|---|
| Authentication | Cookie (browser) + JWT Bearer (API) |
| Session length | 8-hour sliding cookie |
| API tokens | 60-minute JWT, HS256 |
| Authorisation | `[Authorize(Roles = "...")]` on every controller |
| CSRF | Anti-forgery tokens on all POST forms |
| Input validation | Model-level `[Required]`, `[EmailAddress]`, `[Range]` |
| Secrets | `appsettings.json` gitignored — use `appsettings.example.json` |

---

## 📤 Reporting

| Report | Formats | Who |
|---|---|---|
| Hospitals list | PDF · Excel | Admin |
| Rooms list | PDF · Excel | Admin |
| Doctor schedules | PDF · Excel | Doctor |

Generated server-side via **QuestPDF** (PDF) and **ClosedXML** (Excel), streamed as file downloads — no client-side dependencies.

---

## ⚙️ Configuration Reference

```jsonc
// Copy appsettings.example.json → appsettings.json
{
  "ConnectionStrings": {
    // LocalDB (default) or full SQL Server
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=HospitalDB;Trusted_Connection=True"
  },
  "Jwt": {
    // Minimum 32 characters — use a random secret in production
    "Key": "YOUR_SECRET_KEY_MIN_32_CHARS",
    "Issuer": "EnterpriseHospitalManagement",
    "Audience": "EnterpriseHospitalManagement"
  }
}
```

---

## License

MIT — free to use, modify, and distribute.

---

<div align="center">
Built with ❤️ using ASP.NET Core 8 · Entity Framework Core · Bootstrap 5
</div>
