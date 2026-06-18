using System.ComponentModel.DataAnnotations;
using HealthcareCRM.Models;

namespace HealthcareCRM.ViewModels;

// ── Search / List ─────────────────────────────────────────────────────────────
public class PatientSearchViewModel
{
    public string? SearchTerm { get; set; }
    public Gender? Gender     { get; set; }
    public string? BloodGroup { get; set; }
    public string  SortBy     { get; set; } = "name";
    public int     Page       { get; set; } = 1;
}

public class PatientListViewModel
{
    public List<Patient>          Patients    { get; set; } = new();
    public PatientSearchViewModel Search      { get; set; } = new();
    public int                    TotalCount  { get; set; }
    public int                    PageSize    { get; set; } = 15;
    public int                    CurrentPage { get; set; } = 1;
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

// ── Create ────────────────────────────────────────────────────────────────────
public class PatientCreateViewModel
{
    [Required, MaxLength(100)]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Date of Birth")]
    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; }

    [Required]
    public Gender Gender { get; set; }

    [MaxLength(20)]
    [Display(Name = "Patient Code (auto-generated if blank)")]
    public string? PatientCode { get; set; }

    [Required, MaxLength(15)]
    [Phone]
    [Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [MaxLength(200)]
    [EmailAddress]
    public string? Email { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(20)]
    [Display(Name = "Blood Group")]
    public string? BloodGroup { get; set; }

    [MaxLength(500)]
    public string? Allergies { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }
}

// ── Edit ──────────────────────────────────────────────────────────────────────
public class PatientEditViewModel : PatientCreateViewModel
{
    public int    Id          { get; set; }
    public string PatientCode { get; set; } = string.Empty;  // not editable
}

// ── Details ───────────────────────────────────────────────────────────────────
public class PatientDetailsViewModel
{
    public Patient                        Patient         { get; set; } = null!;
    public List<MedicalHistory>           MedicalHistory  { get; set; } = new();
    public MedicalHistoryCreateViewModel? NewHistoryForm  { get; set; }
}
