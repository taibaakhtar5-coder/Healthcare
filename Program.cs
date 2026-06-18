using HealthcareCRM.Data;
using HealthcareCRM.Middleware;   // ← add this
using HealthcareCRM.Models;
using HealthcareCRM.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ─── Database ────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ─── Identity ────────────────────────────────────────────────────────────────
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    // Password policy
    options.Password.RequireDigit           = true;
    options.Password.RequiredLength         = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase       = true;
    options.Password.RequireLowercase       = true;

    // Lockout policy
    options.Lockout.DefaultLockoutTimeSpan  = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers      = true;

    // User settings
    options.User.RequireUniqueEmail         = true;
    options.SignIn.RequireConfirmedEmail     = false; // set true in production
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ─── Cookie / Session ────────────────────────────────────────────────────────
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath          = "/Account/Login";
    options.LogoutPath         = "/Account/Logout";
    options.AccessDeniedPath   = "/Account/AccessDenied";
    options.ExpireTimeSpan     = TimeSpan.FromHours(8);
    options.SlidingExpiration  = true;
    options.Cookie.HttpOnly    = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite    = SameSiteMode.Strict;
});

// ─── Session ─────────────────────────────────────────────────────────────────
builder.Services.AddSession(options =>
{
    options.IdleTimeout        = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly    = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddDistributedMemoryCache();

// ─── Application services ────────────────────────────────────────────────────
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IAuditService, AuditService>();

// ─── MVC ─────────────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews(options =>
{
    // Global anti-forgery filter
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
});

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
});

var app = builder.Build();

// ─── Seed roles & default admin ──────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    await DbSeeder.SeedAsync(scope.ServiceProvider);
}

// ─── Middleware pipeline ──────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Custom audit middleware
app.UseMiddleware<AuditMiddleware>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
