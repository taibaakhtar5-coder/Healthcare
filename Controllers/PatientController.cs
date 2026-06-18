using HealthcareCRM.Models;
using HealthcareCRM.Services;
using HealthcareCRM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HealthcareCRM.Controllers;

[Authorize]
public class PatientController : Controller
{
    private readonly IPatientService               _patientService;
    private readonly IAuditService                 _audit;
    private readonly UserManager<ApplicationUser>  _userManager;

    public PatientController(
        IPatientService              patientService,
        IAuditService                audit,
        UserManager<ApplicationUser> userManager)
    {
        _patientService = patientService;
        _audit          = audit;
        _userManager    = userManager;
    }

    // ── GET /Patient  (list + search) ─────────────────────────────────────────
    [HttpGet]
    [Authorize(Roles = "Admin,Doctor,Nurse,Receptionist,ReadOnly")]
    public async Task<IActionResult> Index(PatientSearchViewModel search)
    {
        var vm = await _patientService.GetPatientsAsync(search);
        return View(vm);
    }

    // ── GET /Patient/Details/5 ────────────────────────────────────────────────
    [HttpGet]
    [Authorize(Roles = "Admin,Doctor,Nurse,Receptionist,ReadOnly")]
    public async Task<IActionResult> Details(int id)
    {
        var patient = await _patientService.GetWithHistoryAsync(id);
        if (patient is null) return NotFound();

        var vm = new PatientDetailsViewModel
        {
            Patient        = patient,
            MedicalHistory = patient.MedicalHistories.OrderByDescending(m => m.VisitDate).ToList(),
            NewHistoryForm = new MedicalHistoryCreateViewModel { PatientId = id }
        };

        return View(vm);
    }

    // ── GET /Patient/Create ────────────────────────────────────────────────────
    [HttpGet]
    [Authorize(Roles = "Admin,Doctor,Receptionist")]
    public async Task<IActionResult> Create()
    {
        var vm = new PatientCreateViewModel();
        ViewBag.SuggestedCode = await _patientService.GeneratePatientCodeAsync();
        return View(vm);
    }

    // ── POST /Patient/Create ───────────────────────────────────────────────────
    [HttpPost]
    [Authorize(Roles = "Admin,Doctor,Receptionist")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PatientCreateViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.SuggestedCode = await _patientService.GeneratePatientCodeAsync();
            return View(vm);
        }

        if (!string.IsNullOrWhiteSpace(vm.PatientCode) &&
            await _patientService.PatientCodeExistsAsync(vm.PatientCode))
        {
            ModelState.AddModelError(nameof(vm.PatientCode), "This patient code is already in use.");
            ViewBag.SuggestedCode = await _patientService.GeneratePatientCodeAsync();
            return View(vm);
        }

        var userId  = _userManager.GetUserId(User)!;
        var patient = await _patientService.CreateAsync(vm, userId);

        await _audit.LogAsync(userId, User.Identity!.Name!,
            "PatientCreated", "Patient", patient.Id.ToString(),
            newValues: $"Code={patient.PatientCode}, Name={patient.FullName}");

        TempData["Success"] = $"Patient {patient.FullName} ({patient.PatientCode}) created.";
        return RedirectToAction(nameof(Details), new { id = patient.Id });
    }

    // ── GET /Patient/Edit/5 ────────────────────────────────────────────────────
    [HttpGet]
    [Authorize(Roles = "Admin,Doctor,Receptionist")]
    public async Task<IActionResult> Edit(int id)
    {
        var patient = await _patientService.GetByIdAsync(id);
        if (patient is null) return NotFound();

        var vm = new PatientEditViewModel
        {
            Id          = patient.Id,
            PatientCode = patient.PatientCode,
            FirstName   = patient.FirstName,
            LastName    = patient.LastName,
            DateOfBirth = patient.DateOfBirth,
            Gender      = patient.Gender,
            PhoneNumber = patient.PhoneNumber,
            Email       = patient.Email,
            Address     = patient.Address,
            City        = patient.City,
            BloodGroup  = patient.BloodGroup,
            Allergies   = patient.Allergies,
            Notes       = patient.Notes
        };

        return View(vm);
    }

    // ── POST /Patient/Edit/5 ───────────────────────────────────────────────────
    [HttpPost]
    [Authorize(Roles = "Admin,Doctor,Receptionist")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PatientEditViewModel vm)
    {
        if (id != vm.Id) return BadRequest();

        if (!ModelState.IsValid) return View(vm);

        var userId  = _userManager.GetUserId(User)!;
        var success = await _patientService.UpdateAsync(id, vm, userId);

        if (!success) return NotFound();

        await _audit.LogAsync(userId, User.Identity!.Name!,
            "PatientUpdated", "Patient", id.ToString());

        TempData["Success"] = "Patient record updated successfully.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // ── POST /Patient/Delete/5  (Admin only) ──────────────────────────────────
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var patient = await _patientService.GetByIdAsync(id);
        if (patient is null) return NotFound();

        var patientName = patient.FullName;
        await _patientService.DeleteAsync(id);

        var userId = _userManager.GetUserId(User)!;
        await _audit.LogAsync(userId, User.Identity!.Name!,
            "PatientDeactivated", "Patient", id.ToString(),
            oldValues: $"Name={patientName}");

        TempData["Success"] = $"Patient {patientName} has been deactivated.";
        return RedirectToAction(nameof(Index));
    }

    // ── POST /Patient/AddMedicalHistory ───────────────────────────────────────
    [HttpPost]
    [Authorize(Roles = "Admin,Doctor,Nurse")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddMedicalHistory(MedicalHistoryCreateViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please correct the form errors.";
            return RedirectToAction(nameof(Details), new { id = vm.PatientId });
        }

        var userId = _userManager.GetUserId(User)!;
        var record = await _patientService.AddMedicalHistoryAsync(vm, userId);

        await _audit.LogAsync(userId, User.Identity!.Name!,
            "MedicalHistoryAdded", "MedicalHistory", record.Id.ToString(),
            newValues: $"PatientId={vm.PatientId}, Diagnosis={vm.Diagnosis}");

        TempData["Success"] = "Medical history record added.";
        return RedirectToAction(nameof(Details), new { id = vm.PatientId });
    }

    // ── POST /Patient/DeleteMedicalHistory/5  (Admin/Doctor) ─────────────────
    [HttpPost]
    [Authorize(Roles = "Admin,Doctor")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteMedicalHistory(int historyId, int patientId)
    {
        await _patientService.DeleteMedicalHistoryAsync(historyId);

        var userId = _userManager.GetUserId(User)!;
        await _audit.LogAsync(userId, User.Identity!.Name!,
            "MedicalHistoryDeleted", "MedicalHistory", historyId.ToString());

        TempData["Success"] = "Medical history record deleted.";
        return RedirectToAction(nameof(Details), new { id = patientId });
    }
}
