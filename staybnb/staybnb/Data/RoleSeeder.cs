using Microsoft.AspNetCore.Identity;
using staybnb.Models;

namespace staybnb.Data
{
    public class RoleSeeder
    {
        public static readonly string[] Roles = { "SuperAdmin", "Admin", "Host", "Guest" };

        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var config = services.GetRequiredService<IConfiguration>();

            foreach (var role in Roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            var email = config["SuperAdmin:Email"] ?? "superadmin@staybnb.com";
            var password = config["SuperAdmin:Password"] ?? "SuperAdmin@123";

            var superAdmin = await userManager.FindByEmailAsync(email);
            if (superAdmin == null)
            {
                superAdmin = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = config["SuperAdmin:FirstName"] ?? "Super",
                    LastName = config["SuperAdmin:LastName"] ?? "Admin",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.Now
                };

                var result = await userManager.CreateAsync(superAdmin, password);
                if (!result.Succeeded)
                    throw new Exception("Failed to seed SuperAdmin: " +
                        string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            if (!await userManager.IsInRoleAsync(superAdmin, "SuperAdmin"))
                await userManager.AddToRoleAsync(superAdmin, "SuperAdmin");
        }
    }
}
