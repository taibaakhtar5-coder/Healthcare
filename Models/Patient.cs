using System.ComponentModel.DataAnnotations;

namespace HealthcareCRM.Models;

public class Patient
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    public DateTime DateOfBirth { get; set; }

    [Required]
    public Gender Gender { get; set; }

    [Required, MaxLength(20)]
    public string PatientCode { get; set; } = string.Empty;   // e.g. PAT-00001

    [Required, MaxLength(15)]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    [MaxLength(200)]
    [EmailAddress]
    public string? Email { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(20)]
    public string? BloodGroup { get; set; }

    [MaxLength(500)]
    public string? Allergies { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public bool IsActive   { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation
    public ICollection<MedicalHistory> MedicalHistories { get; set; } = new List<MedicalHistory>();

    // Computed
    public string FullName => $"{FirstName} {LastName}";
    public int Age => CalculateAge();

    private int CalculateAge()
    {
        var today = DateTime.Today;
        var age   = today.Year - DateOfBirth.Year;
        if (DateOfBirth.Date > today.AddYears(-age)) age--;
        return age;
    }
}

public enum Gender
{
    Male,
    Female,
    Other,
    PreferNotToSay
}
