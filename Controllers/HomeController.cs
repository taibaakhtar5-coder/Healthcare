using System.Diagnostics;
using HealthcareCRM.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthcareCRM.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ApplicationDbContext       _db;
    private readonly ILogger<HomeController>    _logger;

    public HomeController(ApplicationDbContext db, ILogger<HomeController> logger)
    {
        _db     = db;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var totalPatients  = await _db.Patients.CountAsync(p => p.IsActive);
        var totalHistories = await _db.MedicalHistories.CountAsync();
        var todayVisits    = await _db.MedicalHistories
            .CountAsync(m => m.VisitDate.Date == DateTime.Today);
        var recentPatients = await _db.Patients
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .ToListAsync();

        ViewBag.TotalPatients  = totalPatients;
        ViewBag.TotalHistories = totalHistories;
        ViewBag.TodayVisits    = todayVisits;
        ViewBag.RecentPatients = recentPatients;

        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
