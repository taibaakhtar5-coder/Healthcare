using HealthcareCRM.Models;
using HealthcareCRM.Services;
using HealthcareCRM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HealthcareCRM.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser>  _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<ApplicationRole>  _roleManager;
    private readonly IAuditService                 _audit;
    private readonly ILogger<AccountController>    _logger;

    public AccountController(
        UserManager<ApplicationUser>  userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<ApplicationRole>  roleManager,
        IAuditService                 audit,
        ILogger<AccountController>    logger)
    {
        _userManager   = userManager;
        _signInManager = signInManager;
        _roleManager   = roleManager;
        _audit         = audit;
        _logger        = logger;
    }

    // ── GET /Account/Login ────────────────────────────────────────────────────
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    // ── POST /Account/Login ───────────────────────────────────────────────────
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user is null || !user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            user, model.Password, model.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            await _audit.LogAsync(user.Id, user.UserName!, "Login", "Auth",
                ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString());

            _logger.LogInformation("User {Email} logged in.", model.Email);
            return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : "/");
        }

        if (result.IsLockedOut)
        {
            _logger.LogWarning("User {Email} account locked out.", model.Email);
            return RedirectToAction(nameof(Lockout));
        }

        ModelState.AddModelError(string.Empty, "Invalid email or password.");
        return View(model);
    }

    // ── POST /Account/Logout ──────────────────────────────────────────────────
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var userId   = _userManager.GetUserId(User) ?? "";
        var userName = User.Identity?.Name ?? "";
        await _signInManager.SignOutAsync();
        await _audit.LogAsync(userId, userName, "Logout", "Auth");
        return RedirectToAction(nameof(Login));
    }

    // ── GET /Account/Register  (Admin only) ───────────────────────────────────
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult Register()
    {
        ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).OrderBy(n => n).ToList();
        return View();
    }

    // ── POST /Account/Register ────────────────────────────────────────────────
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).OrderBy(n => n).ToList();

        if (!ModelState.IsValid) return View(model);

        var user = new ApplicationUser
        {
            UserName   = model.Email,
            Email      = model.Email,
            FirstName  = model.FirstName,
            LastName   = model.LastName,
            Department = model.Department,
            IsActive   = true,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, model.Role);
            await _audit.LogAsync(_userManager.GetUserId(User)!, User.Identity!.Name!,
                "UserCreated", "ApplicationUser", user.Id,
                newValues: $"Email={user.Email}, Role={model.Role}");

            TempData["Success"] = $"User {user.FullName} created successfully.";
            return RedirectToAction(nameof(UserList));
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return View(model);
    }

    // ── GET /Account/Lockout ──────────────────────────────────────────────────
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Lockout() => View();

    // ── GET /Account/AccessDenied ─────────────────────────────────────────────
    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied() => View();

    // ── GET /Account/UserList  (Admin only) ───────────────────────────────────
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UserList()
    {
        var users = _userManager.Users.OrderBy(u => u.LastName).ToList();
        var userRoles = new Dictionary<string, IList<string>>();
        foreach (var u in users)
            userRoles[u.Id] = await _userManager.GetRolesAsync(u);

        ViewBag.UserRoles = userRoles;
        return View(users);
    }

    // ── POST /Account/ToggleUserStatus  (Admin only) ─────────────────────────
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleUserStatus(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return NotFound();

        user.IsActive = !user.IsActive;
        await _userManager.UpdateAsync(user);

        await _audit.LogAsync(_userManager.GetUserId(User)!, User.Identity!.Name!,
            user.IsActive ? "UserActivated" : "UserDeactivated", "ApplicationUser", userId);

        TempData["Success"] = $"User status updated.";
        return RedirectToAction(nameof(UserList));
    }

    // ── GET /Account/ChangePassword ───────────────────────────────────────────
    [HttpGet]
    [Authorize]
    public IActionResult ChangePassword() => View();

    // ── POST /Account/ChangePassword ──────────────────────────────────────────
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction(nameof(Login));

        var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (result.Succeeded)
        {
            await _signInManager.RefreshSignInAsync(user);
            TempData["Success"] = "Password changed successfully.";
            return RedirectToAction("Index", "Home");
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return View(model);
    }
}
