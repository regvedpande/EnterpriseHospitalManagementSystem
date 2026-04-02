# MedCore HMS Demo Deployment Guide

This repo now supports a split deployment:

- Backend: ASP.NET Core 8 API + existing MVC app, containerized for Render
- Frontend: React + Vite portal in `frontend/`, deployable to Vercel

## What the React frontend covers

The React app is built for recruiter/demo use and currently handles:

- JWT login
- Role-aware dashboard
- Role-aware AI assistant
- Live backend connectivity against the .NET API

The original Razor UI is still present in the backend for the rest of the system, so you can keep using both while you migrate more screens later.

## Local developer setup

Backend:

```powershell
cd EnterpriseHospitalManagement
dotnet user-secrets init
dotnet user-secrets set "Nvidia:ApiKey" "YOUR_NVIDIA_KEY"
dotnet run --urls https://localhost:7299
```

Frontend:

```powershell
cd frontend
nvm use 20.19.0
npm install
$env:VITE_DEV_API_TARGET="https://localhost:7299"
npm run dev
```

## Backend deployment on Render

Render will use the root `Dockerfile`.

Recommended backend environment variables:

- `ASPNETCORE_ENVIRONMENT=Production`
- `Database__Provider=Sqlite`
- `ConnectionStrings__SqliteConnection=Data Source=/app/data/medcore-hms.db`
- `Cors__AllowedOrigins=https://YOUR-FRONTEND.vercel.app`
- `Jwt__Key=YOUR_LONG_RANDOM_SECRET`
- `Jwt__Issuer=MedCoreHMS`
- `Jwt__Audience=MedCoreHMS`
- `Nvidia__ApiKey=YOUR_NVIDIA_KEY`

Optional backend variables:

- `Redis__ConnectionString`
- `RabbitMQ__ConnectionString`

Leave those blank for the recruiter demo unless you explicitly want those integrations enabled.

Health check path:

- `/api/Health`

Notes:

- The backend binds to the platform `PORT` automatically through `docker-entrypoint.sh`.
- The SQLite setup is ideal for a demo. On free/ephemeral hosting, redeploys or container replacement can reset data, so do not treat it as production persistence.
- If you need multiple Vercel domains, put them in `Cors__AllowedOrigins` as a comma-separated list.

## Frontend deployment on Vercel

In Vercel:

1. Import the same Git repo.
2. Set the project root directory to `frontend`.
3. Framework preset: Vite.
4. Build command: `npm run build`
5. Output directory: `dist`
6. Node version: `20.19.0` (or any version accepted by `frontend/package.json`)

Frontend environment variable:

- `VITE_API_BASE_URL=https://YOUR-BACKEND.onrender.com/api`

The SPA rewrite is already configured in `frontend/vercel.json`.

## Demo users

Use these seeded accounts after the backend boots:

- `admin@hospital.com / Admin@123`
- `doctor@hospital.com / Doctor@123`
- `receptionist@hospital.com / Receptionist@123`
- `pharmacist@hospital.com / Pharmacist@123`
- `nurse@hospital.com / Nurse@123`
- `labtech@hospital.com / LabTech@123`
- `patient@hospital.com / Patient@123`
- `accountant@hospital.com / Accountant@123`

## Pre-demo checklist

Before a live recruiter demo:

1. Open the Render backend once to wake it up.
2. Check `https://YOUR-BACKEND.onrender.com/api/Health`.
3. Open the Vercel frontend.
4. Login as receptionist or doctor.
5. Run one AI prompt and confirm the provider badge is correct.

## Verified locally

The current repo was verified with:

- `dotnet build EnterpriseHospitalManagement.sln`
- `npm run build` inside `frontend`
- local API checks for `/api/Health`, `/api/ApiAuth/login`, `/api/portal/bootstrap`, and `/api/portal/assistant`
- `docker build -t medcore-hms-api:test .`
- `docker run -p 8088:8080 medcore-hms-api:test`
