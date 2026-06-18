# Healthcare CRM — Track A (ASP.NET Core MVC)
## Week 2 + Week 3

---

## Project Structure

```
HealthcareCRM/
├── Controllers/
│   ├── AccountController.cs     Week 2 — Auth hardening, register, login, lockout, RBAC
│   ├── HomeController.cs        Dashboard with live stats
│   └── PatientController.cs     Week 3 — Full CRUD + search + medical history
│
├── Models/
│   ├── ApplicationUser.cs       Extended IdentityUser
│   ├── ApplicationRole.cs       Extended IdentityRole
│   ├── Patient.cs               Patient entity with computed Age
│   ├── MedicalHistory.cs        Visit/diagnosis records
│   └── AuditLog.cs              Tracks every write operation
│
├── ViewModels/
│   ├── AccountViewModels.cs     Login, Register, ChangePassword, etc.
│   ├── PatientViewModels.cs     Search, List, Create, Edit, Details VMs
│   └── MedicalHistoryViewModels.cs
│
├── Views/
│   ├── Account/
│   │   ├── Login.cshtml         Standalone page, no layout
│   │   ├── Register.cshtml      Admin-only new user form
│   │   ├── UserList.cshtml      Manage staff users
│   │   ├── ChangePassword.cshtml
│   │   ├── AccessDenied.cshtml
│   │   └── Lockout.cshtml
│   ├── Patient/
│   │   ├── Index.cshtml         Search + paginated list
│   │   ├── Create.cshtml        New patient registration
│   │   ├── Edit.cshtml          Edit patient details
│   │   └── Details.cshtml       Profile + full medical history table
│   ├── Home/
│   │   └── Index.cshtml         Dashboard with stats cards
│   └── Shared/
│       └── _Layout.cshtml       Main layout with role-aware nav
│
├── Data/
│   ├── ApplicationDbContext.cs  EF Core context with all configurations
│   └── DbSeeder.cs              Seeds roles + admin/doctor accounts
│
├── Services/
│   ├── IPatientService.cs
│   ├── PatientService.cs        Full business logic
│   ├── IAuditService.cs
│   └── AuditService.cs
│
├── Middleware/
│   └── AuditMiddleware.cs       Logs all POST/PUT/DELETE requests
│
├── Migrations/
│   └── README_MIGRATIONS.cs     Migration instructions
│
├── wwwroot/
│   ├── css/site.css
│   └── js/site.js
│
├── Properties/
│   └── launchSettings.json
├── appsettings.json
├── Program.cs                   DI, Identity config, middleware pipeline
└── HealthcareCRM.csproj
```

---

## Week 2 — Auth Hardening & RBAC

### Roles seeded automatically
| Role          | Can do                                     |
|---------------|--------------------------------------------|
| Admin         | Everything + user management               |
| Doctor        | View/Create/Edit patients + medical history|
| Nurse         | View patients + add medical history        |
| Receptionist  | View/Create/Edit patients (no history add) |
| BillingStaff  | View only                                  |
| ReadOnly      | View only                                  |

### Security features
- Password policy: 8+ chars, digit, uppercase, special character
- Account lockout: 5 failed attempts → 15-minute lockout
- HTTPS-only cookies, HttpOnly, SameSite=Strict
- Anti-forgery token on every POST
- Every action decorated with `[Authorize(Roles = "...")]`
- Audit log for all logins, logouts, user creates, deactivations

---

## Week 3 — Patient Module

### Patient CRUD
- **Create**: Auto-generate `PAT-00001` style codes or custom entry
- **Read**: Details page with full info panel
- **Update**: All fields except patient code
- **Delete**: Soft-delete (sets `IsActive = false`)

### Search
- Full-text search across name, code, phone, email
- Filter by gender and blood group
- Sort by name, DOB, registration date
- Pagination (15 per page)

### Medical History Table
- Per-patient visit log with diagnosis, medications, lab results
- Visit types: Consultation, FollowUp, Emergency, Procedure, LabTest, Vaccination
- Collapsible add-form (Doctor/Nurse/Admin only)
- Delete (Doctor/Admin only)

---

## Getting Started

```bash
# 1. Update connection string in appsettings.json

# 2. Apply migrations
dotnet ef migrations add InitialCreate
dotnet ef database update

# 3. Run
dotnet run
```

### Default credentials
- **Admin**: admin@healthcarecrm.com / `Admin@123!`
- **Doctor**: doctor@healthcarecrm.com / `Doctor@123!`
