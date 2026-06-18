using HealthcareCRM.Models;
using Microsoft.AspNetCore.Identity;

namespace HealthcareCRM.Data;

public static class DbSeeder
{
    public static readonly string[] Roles =
    {
        "Admin",
        "Doctor",
        "Nurse",
        "Receptionist",
        "BillingStaff",
        "ReadOnly"
    };

    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var config      = services.GetRequiredService<IConfiguration>();

        // ── Roles ──────────────────────────────────────────────────────────
        foreach (var roleName in Roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new ApplicationRole
                {
                    Name        = roleName,
                    Description = $"System role: {roleName}"
                });
            }
        }

        // ── Default Admin ──────────────────────────────────────────────────
        var adminEmail    = config["AppSettings:DefaultAdminEmail"]    ?? "admin@healthcarecrm.com";
        var adminPassword = config["AppSettings:DefaultAdminPassword"] ?? "Admin@123!";

        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
        if (existingAdmin is null)
        {
            var admin = new ApplicationUser
            {
                UserName   = adminEmail,
                Email      = adminEmail,
                FirstName  = "System",
                LastName   = "Admin",
                Department = "IT",
                IsActive   = true,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, adminPassword);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, "Admin");
        }

        // ── Demo Doctor ────────────────────────────────────────────────────
        var doctorEmail = "doctor@healthcarecrm.com";
        if (await userManager.FindByEmailAsync(doctorEmail) is null)
        {
            var doctor = new ApplicationUser
            {
                UserName  = doctorEmail,
                Email     = doctorEmail,
                FirstName = "Demo",
                LastName  = "Doctor",
                Department = "General Medicine",
                IsActive  = true,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(doctor, "Doctor@123!");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(doctor, "Doctor");
        }
    }
}
