using HealthcareCRM.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HealthcareCRM.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Patient>        Patients         { get; set; }
    public DbSet<MedicalHistory> MedicalHistories { get; set; }
    public DbSet<AuditLog>       AuditLogs        { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ── Patient ────────────────────────────────────────────────────────
        builder.Entity<Patient>(e =>
        {
            e.HasKey(p => p.Id);
            e.HasIndex(p => p.PatientCode).IsUnique();
            e.HasIndex(p => p.PhoneNumber);
            e.HasIndex(p => new { p.LastName, p.FirstName });

            e.Property(p => p.PatientCode).IsRequired().HasMaxLength(20);
            e.Property(p => p.FirstName).IsRequired().HasMaxLength(100);
            e.Property(p => p.LastName).IsRequired().HasMaxLength(100);
            e.Property(p => p.Gender).HasConversion<string>();
        });

        // ── MedicalHistory ─────────────────────────────────────────────────
        builder.Entity<MedicalHistory>(e =>
        {
            e.HasKey(m => m.Id);
            e.HasIndex(m => m.PatientId);
            e.HasIndex(m => m.VisitDate);

            e.Property(m => m.VisitType).HasConversion<string>();

            e.HasOne(m => m.Patient)
             .WithMany(p => p.MedicalHistories)
             .HasForeignKey(m => m.PatientId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── AuditLog ───────────────────────────────────────────────────────
        builder.Entity<AuditLog>(e =>
        {
            e.HasKey(a => a.Id);
            e.HasIndex(a => a.UserId);
            e.HasIndex(a => a.Timestamp);

            e.HasOne(a => a.User)
             .WithMany(u => u.AuditLogs)
             .HasForeignKey(a => a.UserId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Rename Identity tables (optional, cleaner DB) ──────────────────
        builder.Entity<ApplicationUser>().ToTable("Users");
        builder.Entity<ApplicationRole>().ToTable("Roles");
    }
}
