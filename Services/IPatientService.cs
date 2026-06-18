using HealthcareCRM.Models;
using HealthcareCRM.ViewModels;

namespace HealthcareCRM.Services;

public interface IPatientService
{
    Task<PatientListViewModel>  GetPatientsAsync(PatientSearchViewModel search);
    Task<Patient?>              GetByIdAsync(int id);
    Task<Patient?>              GetWithHistoryAsync(int id);
    Task<Patient>               CreateAsync(PatientCreateViewModel vm, string userId);
    Task<bool>                  UpdateAsync(int id, PatientEditViewModel vm, string userId);
    Task<bool>                  DeleteAsync(int id);
    Task<bool>                  PatientCodeExistsAsync(string code, int? excludeId = null);
    Task<string>                GeneratePatientCodeAsync();

    // Medical History
    Task<MedicalHistory>        AddMedicalHistoryAsync(MedicalHistoryCreateViewModel vm, string userId);
    Task<MedicalHistory?>       GetMedicalHistoryByIdAsync(int id);
    Task<bool>                  DeleteMedicalHistoryAsync(int id);
}
