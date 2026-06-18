using System.ComponentModel.DataAnnotations;

namespace HealthcareCRM.Models;

public class MedicalHistory
{
    public int Id { get; set; }

    [Required]
    public int PatientId { get; set; }

    [Required, MaxLength(200)]
    public string Diagnosis { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    public DateTime VisitDate { get; set; }

    [MaxLength(200)]
    public string? TreatingDoctor { get; set; }

    [MaxLength(500)]
    public string? Medications { get; set; }

    [MaxLength(500)]
    public string? LabResults { get; set; }

    [MaxLength(200)]
    public string? Procedure { get; set; }

    public VisitType VisitType { get; set; } = VisitType.Consultation;

    [MaxLength(500)]
    public string? FollowUpNotes { get; set; }

    public DateTime? FollowUpDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy  { get; set; }

    // Navigation
    public Patient Patient { get; set; } = null!;
}

public enum VisitType
{
    Consultation,
    FollowUp,
    Emergency,
    Procedure,
    LabTest,
    Vaccination
}
