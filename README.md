# Property Manager Platform

A mobile-friendly full-stack web application tailored for independent property managers. It centralises lease management, rent collection, tenant communication, and financial reporting with streamlined workflows and ready-to-deploy container images.

## Key Features
- **Tenant & Unit CRM** – Add tenants, manage detailed rental unit records, and keep emergency contacts in one place.
- **Contract Lifecycle** – Create and update rental contracts with automatic payment scheduling.
- **Rent Collection** – Track upcoming and historical rent payments, record receipts, apply late fees, and generate branded PDF receipts.
- **Automated Alerts** – Background worker flags overdue payments and surfaces actionable late-payment alerts.
- **Insightful Dashboard** – Charts for rent collection trends, occupancy, and headline KPIs using live data.
- **Reporting & Exports** – One-click PDF and Excel exports for payments and performance summaries.
- **Communication Ready** – Email integration hook for delivering monthly rent receipts (configurable SMTP credentials).

## Project Structure
```
property-manager-app/
├── backend/
│   └── PropertyManager.Api/       # ASP.NET Core 9 Web API
├── client/                        # Responsive React (Vite + TS) front-end
├── docker-compose.yml             # Orchestrates API, UI, and Postgres
└── README.md
```

### Backend Highlights (`backend/PropertyManager.Api`)
- ASP.NET Core 9 with Entity Framework Core (PostgreSQL provider).
- Domain entities for tenants, rental units, contracts, payments, alerts, and documents.
- Services for PDF receipts (QuestPDF), Excel exports (EPPlus), and SMTP email (MailKit).
- Background hosted service that scans for overdue rent and raises alerts.
- Swagger/OpenAPI enabled in development, Serilog console logging, and automatic migrations on start-up.

### Frontend Highlights (`client`)
- React 18 + TypeScript + React Query for data fetching/caching.
- Responsive layout with reusable components, toast notifications, and mobile navigation.
- Charts built with Chart.js/React-ChartJS-2, Excel/PDF download helpers, and inline forms/modals.
- Environment driven API base path (`VITE_API_BASE_URL`, default `/api`).

## Prerequisites
- .NET SDK 9.0+
- Node.js 20+
- npm
- Docker (optional, for containerised deployment)
- PostgreSQL 16+ (local development) or Dockerised Postgres via compose

> **Note:** Package restores require internet access. If your environment is network-restricted, restore NuGet and npm dependencies manually where connectivity is available, then copy the artefacts into this workspace.

## Local Development
### 1. Backend
```bash
cd backend/PropertyManager.Api
# Restore packages (requires internet)
dotnet restore
# Apply EF Core migrations
dotnet ef database update
# Run the API
dotnet run
```
The API listens on `http://localhost:8080` (configure via `ASPNETCORE_URLS`). Update `appsettings.Development.json` with your local Postgres connection string if required.

### 2. Frontend
```bash
cd client
npm install
npm run dev
```
The Vite dev server runs on `http://localhost:5173` and proxies `/api` calls to the backend.

## Containerised Deployment
A production-ready stack is provided via Docker Compose.
```bash
# Build and start all services
docker compose up --build -d
```
Services exposed:
- `client` (React SPA served by nginx) → http://localhost:5173
- `api` (ASP.NET Core) → http://localhost:8080
- `postgres` (database) → internal only, persisted via `postgres_data` volume

Environment variables:
- `ConnectionStrings__DefaultConnection` configures the API database.
- `Email__*` settings (SMTP host, credentials, sender) enable outbound receipt delivery.

## Database Schema (Core Tables)
- `Tenants` – tenant directory with contact metadata.
- `RentalUnits` – inventory of properties with availability status.
- `RentalContracts` – lease agreements linking tenants to units, payment schedules, deposits, notes.
- `RentPayments` – scheduled invoices, payment records, status, receipt metadata.
- `PaymentAlerts` – overdue rent notifications raised by the background worker.
- `DocumentLogs` – persisted PDFs/artefacts for receipts and reports.

## Automated Tasks
- `LatePaymentAlertWorker` (`Background/LatePaymentAlertWorker.cs`) runs every 6 hours, updating `RentPayments` status to `Late` and creating `PaymentAlerts` for overdue invoices.
- API start-up (`Program.cs`) ensures migrations are applied automatically (`db.Database.Migrate()`).

## Extending the Solution
- Add authentication/authorization (e.g., Identity, JWT) for multi-user scenarios.
- Integrate SMS notifications by implementing `IEmailService` equivalent for SMS gateways.
- Enhance reporting with budgeting/expense modules and additional exports.
- Integrate object storage (Azure Blob/S3) for long-term receipt archival in lieu of database BLOBs.

## Testing & Quality
- Backend: add unit/integration tests using xUnit + WebApplicationFactory to validate services and controllers.
- Frontend: add React Testing Library tests for form workflows and API hooks.
- Linting: `npm run lint` validates TypeScript/React patterns.

## Troubleshooting
- **RESTORE failures:** Confirm network access to NuGet/npm registries. Use a local cache or configure offline feeds as needed.
- **Database connection errors:** Update connection strings or ensure Postgres credentials match (`docker-compose.yml` defaults).
- **CORS issues:** Adjust `Cors:AllowedOrigins` in `appsettings.*.json` to match your deployment hostnames.

---
Built with ❤️ to give independent property managers enterprise-grade tooling without the overhead.
