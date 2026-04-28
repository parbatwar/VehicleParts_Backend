using Microsoft.AspNetCore.Identity;
using VehicleParts.Domain.Models;

namespace VehicleParts.Infrastructure.Persistence.Seed;

public static class AdminSeeder
{
    public static async Task SeedAsync(
        UserManager<User> userManager,
        RoleManager<Role> roleManager)
    {
        var adminEmail = "admin@vehicleparts.com";

        // Check if admin already exists
        if (await userManager.FindByEmailAsync(adminEmail) != null)
            return; // already seeded, do nothing

        var admin = new User
        {
            FirstName = "System",
            LastName = "Admin",
            Email = adminEmail,
            UserName = adminEmail,
            NormalizedEmail = adminEmail.ToUpper(),
            NormalizedUserName = adminEmail.ToUpper(),
        };

        var result = await userManager.CreateAsync(admin, "Admin@1234");

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
}