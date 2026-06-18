using System.ComponentModel.DataAnnotations;
using HealthcareCRM.Models;

namespace HealthcareCRM.ViewModels;

public class MedicalHistoryCreateViewModel
{
    [Required]
    public int PatientId { get; set; }

    [Required, MaxLength(200)]
    public string Diagnosis { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    [Display(Name = "Visit Date")]
    [DataType(DataType.Date)]
    public DateTime VisitDate { get; set; } = DateTime.Today;

    [MaxLength(200)]
    [Display(Name = "Treating Doctor")]
    public string? TreatingDoctor { get; set; }

    [MaxLength(500)]
    public string? Medications { get; set; }

    [MaxLength(500)]
    [Display(Name = "Lab Results")]
    public string? LabResults { get; set; }

    [MaxLength(200)]
    public string? Procedure { get; set; }

    [Display(Name = "Visit Type")]
    public VisitType VisitType { get; set; } = VisitType.Consultation;

    [MaxLength(500)]
    [Display(Name = "Follow-up Notes")]
    public string? FollowUpNotes { get; set; }

    [Display(Name = "Follow-up Date")]
    [DataType(DataType.Date)]
    public DateTime? FollowUpDate { get; set; }
}
