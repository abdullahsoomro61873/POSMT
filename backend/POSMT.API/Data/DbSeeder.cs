using Microsoft.EntityFrameworkCore;
using POSMT.API.Models;

namespace POSMT.API.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, IConfiguration configuration)
    {
        await EnsureUserAsync(db, "Demo Manager", "manager@posmt.local",
        configuration["Seed:ManagerPassword"], 1);
        
        await EnsureUserAsync(db, "Demo Cashier", "cashier@posmt.local",
        configuration["Seed:CashierPassword"], 2);
    }

    private static async Task EnsureUserAsync(AppDbContext db, string name, string email, string? password, int roleId)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        if (await db.Users.AnyAsync(u => u.Email == email))
        {
            return;
        }

        db.Users.Add(new User
        {
            Name = name,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            RoleId = roleId
        });
        await db.SaveChangesAsync();
    }
}
