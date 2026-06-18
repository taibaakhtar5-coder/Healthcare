using HealthcareCRM.Data;
using HealthcareCRM.Models;
using HealthcareCRM.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace HealthcareCRM.Services;

public class PatientService : IPatientService
{
    private readonly ApplicationDbContext _db;

    public PatientService(ApplicationDbContext db)
    {
        _db = db;
    }

    // ── List / Search ──────────────────────────────────────────────────────────
    public async Task<PatientListViewModel> GetPatientsAsync(PatientSearchViewModel search)
    {
        var query = _db.Patients.Where(p => p.IsActive).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search.SearchTerm))
        {
            var term = search.SearchTerm.Trim().ToLower();
            query = query.Where(p =>
                p.FirstName.ToLower().Contains(term)  ||
                p.LastName.ToLower().Contains(term)   ||
                p.PatientCode.ToLower().Contains(term)||
                p.PhoneNumber.Contains(term)          ||
                (p.Email != null && p.Email.ToLower().Contains(term)));
        }

        if (search.Gender.HasValue)
            query = query.Where(p => p.Gender == search.Gender.Value);

        if (!string.IsNullOrWhiteSpace(search.BloodGroup))
            query = query.Where(p => p.BloodGroup == search.BloodGroup);

        var totalCount = await query.CountAsync();

        query = search.SortBy switch
        {
            "name_desc"    => query.OrderByDescending(p => p.LastName),
            "dob"          => query.OrderBy(p => p.DateOfBirth),
            "dob_desc"     => query.OrderByDescending(p => p.DateOfBirth),
            "created"      => query.OrderBy(p => p.CreatedAt),
            "created_desc" => query.OrderByDescending(p => p.CreatedAt),
            _              => query.OrderBy(p => p.LastName)
        };

        int page     = search.Page < 1 ? 1 : search.Page;
        int pageSize = 15;

        var patients = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PatientListViewModel
        {
            Patients    = patients,
            Search      = search,
            TotalCount  = totalCount,
            PageSize    = pageSize,
            CurrentPage = page
        };
    }

    // ── Get by ID ──────────────────────────────────────────────────────────────
    public async Task<Patient?> GetByIdAsync(int id)
        => await _db.Patients.FindAsync(id);

    public async Task<Patient?> GetWithHistoryAsync(int id)
        => await _db.Patients
            .Include(p => p.MedicalHistories.OrderByDescending(m => m.VisitDate))
            .FirstOrDefaultAsync(p => p.Id == id);

    // ── Create ─────────────────────────────────────────────────────────────────
    public async Task<Patient> CreateAsync(PatientCreateViewModel vm, string userId)
    {
        var patient = new Patient
        {
            FirstName   = vm.FirstName,
            LastName    = vm.LastName,
            DateOfBirth = vm.DateOfBirth,
            Gender      = vm.Gender,
            PatientCode = string.IsNullOrWhiteSpace(vm.PatientCode)
                            ? await GeneratePatientCodeAsync()
                            : vm.PatientCode,
            PhoneNumber = vm.PhoneNumber,
            Email       = vm.Email,
            Address     = vm.Address,
            City        = vm.City,
            BloodGroup  = vm.BloodGroup,
            Allergies   = vm.Allergies,
            Notes       = vm.Notes,
            IsActive    = true,
            CreatedAt   = DateTime.UtcNow,
            CreatedBy   = userId
        };

        _db.Patients.Add(patient);
        await _db.SaveChangesAsync();
        return patient;
    }

    // ── Update ─────────────────────────────────────────────────────────────────
    public async Task<bool> UpdateAsync(int id, PatientEditViewModel vm, string userId)
    {
        var patient = await _db.Patients.FindAsync(id);
        if (patient is null) return false;

        patient.FirstName   = vm.FirstName;
        patient.LastName    = vm.LastName;
        patient.DateOfBirth = vm.DateOfBirth;
        patient.Gender      = vm.Gender;
        patient.PhoneNumber = vm.PhoneNumber;
        patient.Email       = vm.Email;
        patient.Address     = vm.Address;
        patient.City        = vm.City;
        patient.BloodGroup  = vm.BloodGroup;
        patient.Allergies   = vm.Allergies;
        patient.Notes       = vm.Notes;
        patient.UpdatedAt   = DateTime.UtcNow;
        patient.UpdatedBy   = userId;

        await _db.SaveChangesAsync();
        return true;
    }

    // ── Soft Delete ────────────────────────────────────────────────────────────
    public async Task<bool> DeleteAsync(int id)
    {
        var patient = await _db.Patients.FindAsync(id);
        if (patient is null) return false;

        patient.IsActive  = false;
        patient.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    // ── Code helpers ───────────────────────────────────────────────────────────
    public async Task<bool> PatientCodeExistsAsync(string code, int? excludeId = null)
    {
        var q = _db.Patients.Where(p => p.PatientCode == code);
        if (excludeId.HasValue) q = q.Where(p => p.Id != excludeId.Value);
        return await q.AnyAsync();
    }

    public async Task<string> GeneratePatientCodeAsync()
    {
        var last = await _db.Patients
            .OrderByDescending(p => p.Id)
            .Select(p => p.PatientCode)
            .FirstOrDefaultAsync();

        int next = 1;
        if (last is not null && last.StartsWith("PAT-") &&
            int.TryParse(last[4..], out int n))
            next = n + 1;

        return $"PAT-{next:D5}";
    }

    // ── Medical History ────────────────────────────────────────────────────────
    public async Task<MedicalHistory> AddMedicalHistoryAsync(MedicalHistoryCreateViewModel vm, string userId)
    {
        var history = new MedicalHistory
        {
            PatientId      = vm.PatientId,
            Diagnosis      = vm.Diagnosis,
            Description    = vm.Description,
            VisitDate      = vm.VisitDate,
            TreatingDoctor = vm.TreatingDoctor,
            Medications    = vm.Medications,
            LabResults     = vm.LabResults,
            Procedure      = vm.Procedure,
            VisitType      = vm.VisitType,
            FollowUpNotes  = vm.FollowUpNotes,
            FollowUpDate   = vm.FollowUpDate,
            CreatedAt      = DateTime.UtcNow,
            CreatedBy      = userId
        };

        _db.MedicalHistories.Add(history);
        await _db.SaveChangesAsync();
        return history;
    }

    public async Task<MedicalHistory?> GetMedicalHistoryByIdAsync(int id)
        => await _db.MedicalHistories.Include(m => m.Patient).FirstOrDefaultAsync(m => m.Id == id);

    public async Task<bool> DeleteMedicalHistoryAsync(int id)
    {
        var record = await _db.MedicalHistories.FindAsync(id);
        if (record is null) return false;
        _db.MedicalHistories.Remove(record);
        await _db.SaveChangesAsync();
        return true;
    }
}
