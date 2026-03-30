# MedCore HMS — Enterprise Hospital Management System

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com)
[![ASP.NET Core MVC](https://img.shields.io/badge/ASP.NET%20Core%20MVC-8.0-512BD4)](https://learn.microsoft.com/aspnet/core)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-LocalDB-CC2927?logo=microsoftsqlserver)](https://www.microsoft.com/sql-server)
[![Entity Framework Core](https://img.shields.io/badge/EF%20Core-8.0-512BD4)](https://learn.microsoft.com/ef/core)
[![NVIDIA NIM AI](https://img.shields.io/badge/NVIDIA%20NIM-LLaMA%203.1%2070B-76b900?logo=nvidia)](https://build.nvidia.com)
[![Chart.js](https://img.shields.io/badge/Chart.js-4.4-FF6384?logo=chartdotjs)](https://www.chartjs.org)
[![Bootstrap](https://img.shields.io/badge/Bootstrap-5-7952B3?logo=bootstrap)](https://getbootstrap.com)
[![Redis](https://img.shields.io/badge/Redis-7.x-DC382D?logo=redis)](https://redis.io)
[![RabbitMQ](https://img.shields.io/badge/RabbitMQ-3.x-FF6600?logo=rabbitmq)](https://www.rabbitmq.com)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

> A **production-ready**, full-stack hospital management platform with **8 role-based portals**, **NVIDIA NIM AI assistants** in every portal, real-time analytics dashboards, Chart.js visualisations, document management, and complete CRUD across every clinical and administrative workflow.

---

## Table of Contents

1. [Overview](#overview)
2. [AI Integration — NVIDIA NIM](#ai-integration--nvidia-nim)
3. [Live Demo & Credentials](#live-demo--credentials)
4. [System Architecture](#system-architecture)
5. [Layer Breakdown](#layer-breakdown)
6. [Database Entity Relationship Diagram](#database-entity-relationship-diagram)
7. [Authentication & Authorization Flow](#authentication--authorization-flow)
8. [Role Portals & Feature Matrix](#role-portals--feature-matrix)
9. [Dashboard Analytics](#dashboard-analytics)
10. [Infrastructure & Resilience](#infrastructure--resilience)
11. [Project Structure](#project-structure)
12. [Technology Stack](#technology-stack)
13. [Prerequisites](#prerequisites)
14. [Quick Start](#quick-start)
15. [Configuration Reference](#configuration-reference)
16. [Security Practices](#security-practices)
17. [Contributing](#contributing)
18. [License & Contact](#license--contact)

---

## Overview

**MedCore HMS** digitises every department of a hospital into a single, cohesive web application. A surgeon, receptionist, pharmacist, lab technician, accountant, nurse, and patient each see a completely different portal — but all data flows through one shared database, ensuring real-time consistency.

**What makes it enterprise-grade:**
- **8 isolated role portals** each with their own sidebar navigation, dashboard, and workflows
- **NVIDIA NIM AI Assistants** embedded in every role portal — doctors get diagnosis support, pharmacists get drug interaction checks, nurses get vitals interpretation, and more
- **Real-time Chart.js analytics** — line charts, doughnut charts, bar charts, horizontal bar charts — all driven by live database data
- **Full CRUD** for 15+ entities (Appointments, Bills, Labs, Medicines, Rooms, Payrolls, Suppliers, Insurance, Departments, Contacts, Patient Reports, Prescriptions, Documents, Hospitals, Users)
- **Document management** — patients upload PDFs, images, Word/Excel files; categorised with analytics
- **PDF + Excel exports** for every major data set
- **Redis distributed cache** with in-memory fallback, **RabbitMQ** event bus, **Polly** resilience
- **Twilio SMS** and email notification hooks
- **JWT API layer** alongside cookie-based MVC authentication
- **Serilog** structured logging with rolling file sinks
- **Responsive UI** — custom CSS design system (no Bootstrap utility classes), DM Sans / DM Serif Display typography

---

## AI Integration — NVIDIA NIM

MedCore HMS integrates **NVIDIA NIM** (powered by Meta LLaMA 3.1 70B Instruct) as a clinical intelligence layer across all 8 role portals. Every AI assistant is **role-specific** — it does not give generic responses; it thinks like the professional using it.

### How It Works

```mermaid
sequenceDiagram
    actor User as Role User (Doctor / Nurse / etc.)
    participant View as AI Assistant View
    participant Ctrl as AiAssistantController
    participant Svc as NvidiaAiService
    participant DB as SQL Server (real patient data)
    participant NVIDIA as NVIDIA NIM API\n(LLaMA 3.1 70B)

    User->>View: Fills in symptoms / vitals / drug names
    View->>Ctrl: POST /AiAssistant/{action} (CSRF protected)

    alt Doctor endpoints
        Ctrl->>DB: Fetch patient history + reports + appointments
        DB-->>Ctrl: Real patient context (diagnoses, meds, dates)
    end

    Ctrl->>Svc: Call role-specific method (e.g. GetDiagnosisSuggestionAsync)
    Svc->>Svc: Build system prompt tuned to this role
    Svc->>Svc: Inject patient context into user message
    Svc->>NVIDIA: POST /v1/chat/completions\n{model, messages, temperature:0.4, max_tokens:1200}
    NVIDIA-->>Svc: Structured clinical response
    Svc-->>Ctrl: Response string
    Ctrl-->>View: JSON {result: "..."}
    View->>User: Rendered AI response panel
```

### Role-Specific AI Capabilities

| Portal | AI Assistant | Key Features |
|---|---|---|
| **Doctor** | AI Clinical Assistant | Differential diagnosis from symptoms + patient history, medicine recommendations with dosage, drug interaction check (multi-drug), early symptom warnings, personalised treatment plans, clinical chat |
| **Pharmacist** | AI Pharmacy Assistant | Drug-drug interaction checker (multiple meds), dosage guidance by patient profile, therapeutic substitutes when drugs are unavailable, pharmacy chat |
| **Nurse** | AI Nursing Assistant | Vitals interpretation with alert flags (critical / warning / normal), nursing care plan generation by condition, nursing chat |
| **Lab Technician** | AI Lab Interpreter | Lab result interpretation (test + value + unit + reference range), test panel recommendations for suspected conditions, lab chat |
| **Receptionist** | AI Triage Assistant | Symptom triage with urgency classification (Emergency / Urgent / Routine), appropriate department routing, triage chat |
| **Admin** | Hospital AI Analytics | Real-time hospital stats analysis (patient count, revenue, bills, labs), operational insights and anomaly detection, admin chat |
| **Patient** | Health AI Assistant | Health question answering, document text analysis (upload report → AI explains it), medication questions, general health chat |

### System Prompts Design

Each role has a dedicated system prompt constant in `NvidiaAiService.cs` that gives the LLM a precise professional persona:

```
SysDoctor   → "expert clinical physician... evidence-based, structured differentials"
SysPharmacist → "senior clinical pharmacist... drug safety focused"
SysNurse    → "experienced registered nurse... concise, actionable clinical observations"
SysLabTech  → "expert medical laboratory scientist... explain in clinical terms"
SysRecept   → "experienced hospital triage nurse... urgency classification: Emergency/Urgent/Routine"
SysAdmin    → "senior hospital administrator and healthcare analytics expert"
SysPatient  → "friendly medical assistant... clear, jargon-free, compassionate"
```

### AI Service Architecture

```
Hospital.Web/Infrastructure/AI/
├── IAiService.cs          # Interface — 15 role-specific async methods + ChatAsync
└── NvidiaAiService.cs     # Implementation — NVIDIA NIM OpenAI-compatible REST client
```

**`IAiService` methods:**

```csharp
// Doctor
Task<string> GetDiagnosisSuggestionAsync(symptoms, patientContext)
Task<string> GetMedicineRecommendationAsync(diagnosis, patientContext)
Task<string> GetDrugInteractionCheckAsync(medicineList)
Task<string> GetEarlySymptomAlertAsync(symptoms, patientHistory)
Task<string> GetTreatmentPlanAsync(diagnosis, patientContext)

// Pharmacist
Task<string> GetPharmacistDrugInteractionAsync(medicines)
Task<string> GetDosageGuideAsync(drugName, patientInfo)
Task<string> GetDrugSubstituteAsync(drugName, reason)

// Nurse
Task<string> InterpretVitalsAsync(vitals, patientContext)
Task<string> GetNursingCarePlanAsync(condition)

// Lab Tech
Task<string> InterpretLabResultAsync(testName, value, unit, patientContext)
Task<string> SuggestTestPanelAsync(suspectedCondition)

// Receptionist
Task<string> TriageSymptomsAsync(symptoms)

// Admin
Task<string> AnalyseHospitalStatsAsync(statsJson)

// Universal
Task<string> ChatAsync(IEnumerable<AiMessage> messages)
bool IsConfigured { get; }
```

### AI Setup (Local Development)

```bash
# Store your NVIDIA API key securely (never committed to git)
dotnet user-secrets set "Nvidia:ApiKey" "nvapi-YOUR_KEY_HERE" \
  --project EnterpriseHospitalManagement

# Verify
dotnet user-secrets list --project EnterpriseHospitalManagement
```

The app checks `IsConfigured` before every call and shows a clear "AI not configured" message instead of crashing if the key is missing.

### Production AI Configuration

Set via environment variable — **never** put the key in `appsettings.json`:

```bash
# Linux / Docker
export Nvidia__ApiKey="nvapi-YOUR_KEY_HERE"

# Windows environment variable
set Nvidia__ApiKey=nvapi-YOUR_KEY_HERE
```

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
            Admin["🔴 Admin\n15+ controllers\n+ AI Analytics"]
            Doctor["🟢 Doctor\n4 controllers\n+ AI Clinical"]
            Patient["🔵 Patient\n5 controllers\n+ Health AI"]
            Nurse["🟣 Nurse\n3 controllers\n+ AI Nursing"]
            LabTech["🟡 LabTech\n3 controllers\n+ AI Lab"]
            Pharma["🟠 Pharmacist\n4 controllers\n+ AI Pharmacy"]
            Recept["⚪ Receptionist\n3 controllers\n+ AI Triage"]
            Account["🟤 Accountant\n3 controllers"]
        end

        AI["🤖 AI Layer\nIAiService / NvidiaAiService\nSingleton — 1 HttpClient"]
        Layout[_Layout.cshtml\nShared sidebar · topbar · alerts · modals]
        Middleware[Middleware Pipeline\nIdentity · RBAC · StatusCodePages · Serilog]
    end

    subgraph NvidiaCloud["☁️ NVIDIA NIM Cloud"]
        NIM[LLaMA 3.1 70B Instruct\nOpenAI-compatible API\nhttps://integrate.api.nvidia.com/v1]
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
    Areas -->|patient context| AI
    AI -->|HTTPS + Bearer token| NIM
    NIM -->|JSON response| AI
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
    A["🌐 Presentation\nHospital.Web\nMVC Controllers\nRazor Views\nCSS Design System\n🤖 AI Controllers"] -->|ViewModels| B["⚙️ Application\nHospital.Services\nBusiness Logic\nService Interfaces\nImplementations"]
    B -->|Domain Models| C["🗄️ Data Access\nHospital.Repositories\nUnit of Work\nGeneric Repository\nEF Core DbContext"]
    C --> D[("💾 SQL Server\nHospitalDB")]
    E["📦 Domain\nHospital.Models\nEntities · Enums"] -.->|used by| B
    E -.->|used by| C
    F["🔧 Utilities\nHospital.Utilities\nWebSiteRoles\nImageOperations\nJwtService\nEmailSender\nTwilioSmsService"] -.->|used by| A
    F -.->|used by| B
    G["🤖 AI Infrastructure\nHospital.Web/Infrastructure/AI\nIAiService\nNvidiaAiService"] -.->|injected into| A
```

| Layer | Project | Responsibility |
|---|---|---|
| Presentation | `Hospital.Web` | MVC controllers, Razor views, CSS/JS, Areas |
| **AI Layer** | **`Hospital.Web/Infrastructure/AI`** | **NVIDIA NIM integration — IAiService + NvidiaAiService** |
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
        decimal NetSalary
        decimal HourlySalary
        decimal BonusSalary
        decimal Compensation
        PayrollStatus Status
        DateTime PayPeriodStart
        DateTime PayPeriodEnd
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
    ADM --> ADM9["🤖 Hospital AI Analytics"]

    HMS --> DOC["🟢 Doctor"]
    DOC --> DOC1["My Appointments — Create/Edit"]
    DOC --> DOC2["Patient Reports — Write/View"]
    DOC --> DOC3["Prescriptions"]
    DOC --> DOC4["My Schedule & Timings"]
    DOC --> DOC5["PDF/Excel Export"]
    DOC --> DOC6["Trend + Status Charts"]
    DOC --> DOC7["🤖 AI Clinical Assistant\nDiagnosis · Medicines · Interactions\nEarly Warning · Treatment Plan"]

    HMS --> PAT["🔵 Patient"]
    PAT --> PAT1["My Appointments — Book"]
    PAT --> PAT2["My Bills — View"]
    PAT --> PAT3["My Lab Results"]
    PAT --> PAT4["My Medical Reports"]
    PAT --> PAT5["My Documents — Upload"]
    PAT --> PAT6["Document Analytics"]
    PAT --> PAT7["🤖 Health AI Assistant"]

    HMS --> NRS["🟣 Nurse"]
    NRS --> NRS1["View & Update Appointments"]
    NRS --> NRS2["Appointment Status Charts"]
    NRS --> NRS3["🤖 AI Nursing Assistant\nVitals Interpretation · Care Plans"]

    HMS --> LAB["🟡 LabTech"]
    LAB --> LAB1["Lab Orders — Create/Edit"]
    LAB --> LAB2["Record Test Results"]
    LAB --> LAB3["Status Doughnut Chart"]
    LAB --> LAB4["🤖 AI Lab Interpreter\nResult Interpretation · Test Panels"]

    HMS --> PHA["🟠 Pharmacist"]
    PHA --> PHA1["Medicines Stock — CRUD"]
    PHA --> PHA2["Prescriptions — View/Dispense"]
    PHA --> PHA3["Medicine Type Bar Chart"]
    PHA --> PHA4["🤖 AI Pharmacy Assistant\nInteractions · Dosage · Substitutes"]

    HMS --> REC["⚪ Receptionist"]
    REC --> REC1["Schedule Appointments"]
    REC --> REC2["All Appointments — View/Edit"]
    REC --> REC3["Status Overview Chart"]
    REC --> REC4["🤖 AI Triage Assistant\nSymptom Triage · Urgency Classification"]

    HMS --> ACC["🟤 Accountant"]
    ACC --> ACC1["Bills — Create/Edit"]
    ACC --> ACC2["Payroll Management"]
    ACC --> ACC3["Revenue Trend Chart"]
    ACC --> ACC4["Financial Summary"]
```

| Feature | Admin | Doctor | Patient | Nurse | LabTech | Pharmacist | Receptionist | Accountant |
|---|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|
| Dashboard with Charts | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| **AI Assistant** | **✅** | **✅** | **✅** | **✅** | **✅** | **✅** | **✅** | — |
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

## Infrastructure & Resilience

```mermaid
graph TB
    subgraph AppLayer["ASP.NET Core Application"]
        MVC[MVC Controllers]
        SVC[Service Layer]
        BG[Background Queue\nQueuedHostedService]
        AI[AI Layer\nNvidiaAiService Singleton]
    end

    subgraph Cache["Redis Cache Layer"]
        RC[IDistributedCache\nStackExchange.Redis]
        MC[In-Memory Fallback\n dev / no Redis]
    end

    subgraph MQ["Message Bus"]
        RMQ[RabbitMQ\nTopic Exchange: hms.events]
        CONS[Consumer Hosted Service\nAppointments · Bills · Labs · Users]
        NOOP[No-op Fallback\n not configured]
    end

    subgraph Resilience["Polly Resilience"]
        RETRY[EF Core Retry\n5 retries, exp back-off]
        CB[Circuit Breaker\nHTTP calls, 5 failures / 30s]
    end

    subgraph NvidiaCloud["NVIDIA NIM Cloud"]
        NIM[LLaMA 3.1 70B Instruct\n60s HTTP timeout\ntemperature: 0.4\nmax_tokens: 1200]
    end

    MVC --> SVC
    MVC --> AI
    AI -->|Named HttpClient nvidia\nBearer token auth| NIM
    SVC -->|cache read/write| RC
    RC -.->|unavailable| MC
    SVC -->|enqueue event| BG
    BG -->|publish| RMQ
    RMQ -->|route| CONS
    CONS -->|email notification| SVC
    RMQ -.->|not configured| NOOP
    SVC --> RETRY
    RETRY -->|transient error| CB
```

### How It Works

| Component | Location | Behaviour |
|---|---|---|
| **NVIDIA NIM AI** | `Infrastructure/AI/NvidiaAiService` | Singleton; named `HttpClient("nvidia")` with 60 s timeout. OpenAI-compatible `/v1/chat/completions`. Gracefully returns error string if not configured. |
| **Redis Cache** | `ICacheService` / `RedisCacheService` | Caches appointment, bill & dashboard data (2–5 min TTL). Falls back to in-memory if Redis is unreachable. |
| **Background Queue** | `IBackgroundTaskQueue` / `BackgroundTaskQueue` | Channel-based (capacity 500). All email, SMS and event publishing happens off-request in `QueuedHostedService`. |
| **RabbitMQ Bus** | `IMessageBus` / `RabbitMqMessageBus` | Topic exchange `hms.events` with queues: `hms.appointments`, `hms.billing`, `hms.labs`, `hms.users`. Auto-reconnects. No-ops gracefully when `RabbitMQ:ConnectionString` is empty. |
| **Polly Retry** | `ResiliencePolicies` + EF Core | EF Core SQL provider: 5 retries with exponential back-off on transient SQL errors. Circuit breaker (5 failures → 30 s open) for external HTTP calls. |
| **Lockout** | `AuthController` | 5 failed login attempts → 5-minute account lockout (Identity `LockoutOptions`). |
| **Security Headers** | `Program.cs` middleware | `X-Content-Type-Options`, `X-Frame-Options`, `X-XSS-Protection`, `Referrer-Policy` on every response. |

### Docker Quick-Start (Redis + RabbitMQ)

```bash
# Redis (required for distributed caching)
docker run -d --name redis -p 6379:6379 redis:7

# RabbitMQ with management UI (optional — app degrades gracefully without it)
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
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
    │   ├── DbInitializer.cs             # Seed roles, demo users, demo data
    │   ├── JwtService.cs
    │   ├── ImageOperations.cs
    │   ├── EmailSender.cs
    │   └── TwilioSmsService.cs
    │
    └── Hospital.Web/                    # Presentation layer
        ├── Program.cs                   # DI wiring, middleware, seeding, AI service registration
        ├── appsettings.json             # Dev config (safe placeholders only)
        │
        ├── Infrastructure/
        │   └── AI/                      # 🤖 NVIDIA NIM AI Integration
        │       ├── IAiService.cs        # Interface — 15 role-specific methods + ChatAsync
        │       └── NvidiaAiService.cs   # NVIDIA NIM OpenAI-compatible client
        │
        ├── Areas/
        │   ├── Admin/
        │   │   ├── Controllers/
        │   │   │   ├── HomeController.cs
        │   │   │   ├── AiAssistantController.cs  # 🤖 Hospital stats AI + admin chat
        │   │   │   └── ... (14 more controllers)
        │   │   └── Views/
        │   │       ├── AiAssistant/Index.cshtml  # Stats Analysis + Chat tabs
        │   │       └── ... (per entity views)
        │   │
        │   ├── Doctor/
        │   │   ├── Controllers/
        │   │   │   ├── HomeController.cs
        │   │   │   ├── AiAssistantController.cs  # 🤖 Diagnosis/Medicines/Interactions/Warning/Plan/Chat
        │   │   │   ├── AppointmentsController.cs
        │   │   │   ├── PatientsController.cs
        │   │   │   └── DoctorsController.cs
        │   │   └── Views/
        │   │       └── AiAssistant/Index.cshtml  # 6-tab clinical AI panel
        │   │
        │   ├── Nurse/
        │   │   ├── Controllers/
        │   │   │   ├── AiAssistantController.cs  # 🤖 Vitals + Care plan + Chat
        │   │   │   └── ...
        │   │   └── Views/
        │   │       └── AiAssistant/Index.cshtml
        │   │
        │   ├── LabTech/
        │   │   ├── Controllers/
        │   │   │   ├── AiAssistantController.cs  # 🤖 Lab interpretation + Test panels + Chat
        │   │   │   └── ...
        │   │   └── Views/
        │   │       └── AiAssistant/Index.cshtml
        │   │
        │   ├── Pharmacist/
        │   │   ├── Controllers/
        │   │   │   ├── AiAssistantController.cs  # 🤖 Interactions + Dosage + Substitutes + Chat
        │   │   │   └── ...
        │   │   └── Views/
        │   │       └── AiAssistant/Index.cshtml
        │   │
        │   ├── Receptionist/
        │   │   ├── Controllers/
        │   │   │   ├── AiAssistantController.cs  # 🤖 Symptom triage + urgency + Chat
        │   │   │   └── ...
        │   │   └── Views/
        │   │       └── AiAssistant/Index.cshtml
        │   │
        │   ├── Patient/
        │   │   └── Controllers/
        │   │       └── AiAssistantController.cs  # 🤖 Health Q&A + document analysis + Chat
        │   │
        │   └── Accountant/
        │       └── Controllers/ ...
        │
        ├── Controllers/
        │   ├── AuthController.cs        # Login/Register/Logout
        │   ├── HomeController.cs        # Landing + error pages
        │   └── ProfileController.cs     # Edit profile + change password
        │
        └── Views/
            ├── Shared/
            │   ├── _Layout.cshtml       # Main layout — topbar, sidebar (AI links in every role), alerts, modals
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
| ORM | Entity Framework Core | 8.0 (with retry) |
| Database | Microsoft SQL Server / LocalDB | 2019+ |
| Authentication | ASP.NET Identity | 8.0 |
| API Auth | JWT Bearer | — |
| **AI Engine** | **NVIDIA NIM — LLaMA 3.1 70B Instruct** | **OpenAI-compatible** |
| **Cache** | **Redis (StackExchange.Redis)** | **7.x** |
| **Resilience** | **Polly** | **8.4** |
| **Message Bus** | **RabbitMQ.Client** | **6.8** |
| **Background Queue** | **System.Threading.Channels** | **built-in** |
| CSS Framework | Custom CSS Design System | — |
| JS Charts | Chart.js | 4.4.0 |
| Icons | Font Awesome | 6.4.0 |
| Typography | DM Sans + DM Serif Display | Google Fonts |
| Bootstrap | Bootstrap | 5.x (utilities only) |
| Logging | Serilog | 8.x |
| SMS | Twilio | — |
| File Transfers | SFTP (SSH.NET) | — |
| Export | QuestPDF + CsvHelper + ClosedXML | — |

---

## Prerequisites

| Requirement | Minimum Version |
|---|---|
| .NET SDK | 8.0 |
| SQL Server | 2019 LocalDB (bundled with VS) **or** SQL Server Developer / Express |
| Visual Studio | 2022 (17.8+) **or** VS Code + C# DevKit |
| Git | Any recent version |
| NVIDIA NIM API Key | Free at [build.nvidia.com](https://build.nvidia.com) (optional — AI degrades gracefully without it) |

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

### 3. Add your NVIDIA API key (for AI features)

```bash
cd EnterpriseHospitalManagement
dotnet user-secrets set "Nvidia:ApiKey" "nvapi-YOUR_KEY_HERE"
```

Get a free API key at [build.nvidia.com](https://build.nvidia.com). The app runs fine without it — AI pages will show a "not configured" message.

### 4. Restore and run

```bash
dotnet restore
dotnet run --project Hospital.Web
```

The app auto-creates the database and seeds all roles + demo users + demo clinical data on first run. Navigate to `https://localhost:7xxx` (port shown in terminal).

### 5. Login

Use any of the seeded credentials from the [table above](#live-demo--credentials). The AI Assistant link appears in the sidebar for every role.

---

## Configuration Reference

| Variable | Purpose | Example |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | Database connection | `Server=prod-sql;Database=HospitalDB;...` |
| `Jwt__Key` | JWT signing secret (min 32 chars) | Use a strong random string |
| `Jwt__Issuer` | JWT issuer claim | `MedCoreHMS` |
| `Jwt__Audience` | JWT audience claim | `MedCoreHMS` |
| **`Nvidia__ApiKey`** | **NVIDIA NIM API key for AI features** | **`nvapi-...` — use user-secrets in dev, env var in prod** |
| `Redis__ConnectionString` | Redis for distributed cache | `localhost:6379,abortConnect=false` |
| `RabbitMQ__ConnectionString` | RabbitMQ for event bus | `amqp://guest:guest@localhost:5672/` |
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
    G -- AI Endpoint --> K[CSRF token on every AI POST\nAPI key in user-secrets only]
    G -- Other --> L[AntiForgery token on\nall POST forms]
```

**Key security measures implemented:**

- **RBAC**: Every controller decorated with `[Authorize(Roles = "Website_{Role}")]`; global `AutoValidateAntiforgeryTokenAttribute` filter
- **CSRF protection**: `@Html.AntiForgeryToken()` on every POST form + global antiforgery filter (including all AI endpoints)
- **AI key security**: NVIDIA API key stored only in `dotnet user-secrets` (dev) or environment variable (prod) — never in `appsettings.json` or source control
- **Account lockout**: 5 failed login attempts → 5-minute lock (`LockoutOptions`)
- **Security headers**: `X-Content-Type-Options: nosniff`, `X-Frame-Options: SAMEORIGIN`, `X-XSS-Protection`, `Referrer-Policy` on all responses
- **Cookie hardening**: `HttpOnly=true`, `SameSite=Strict`, `SecurePolicy=SameAsRequest`
- **Input validation**: `[Required]`, `[MaxLength]`, `[EmailAddress]`, `ModelState.IsValid` + antiforgery at every POST
- **Scoped data access**: Doctors/patients filtered by own ID at service layer
- **No secrets in source**: `appsettings.Production.json` gitignored; safe empty placeholders committed
- **Status code pages**: Custom 404/403/500 via `UseStatusCodePagesWithReExecute`
- **DB retry**: EF Core SQL provider with 5-retry exponential back-off on transient failures
- **Password policy**: Identity minimum 6 chars + digit; all passwords hashed (ASP.NET Identity PBKDF2)
- **JWT**: Empty key placeholder — auto-generates random key in dev; always override in production via env var

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
