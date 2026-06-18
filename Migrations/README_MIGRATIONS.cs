// ──────────────────────────────────────────────────────────────────────────────
// MIGRATION INSTRUCTIONS
// ──────────────────────────────────────────────────────────────────────────────
//
// Run these commands from the project root in the Package Manager Console
// or your terminal:
//
//   dotnet ef migrations add InitialCreate
//   dotnet ef database update
//
// Or in Package Manager Console:
//   Add-Migration InitialCreate
//   Update-Database
//
// The migration will create the following tables:
//   Users            — ApplicationUser (extends IdentityUser)
//   Roles            — ApplicationRole (extends IdentityRole)
//   Patients         — Patient records with unique PatientCode
//   MedicalHistories — Visit/diagnosis history per patient
//   AuditLogs        — All write operations logged
//   + Standard Identity tables (UserRoles, UserClaims, etc.)
//
// Default seed data (created automatically on startup):
//   Admin:  admin@healthcarecrm.com  / Admin@123!
//   Doctor: doctor@healthcarecrm.com / Doctor@123!
// ──────────────────────────────────────────────────────────────────────────────
