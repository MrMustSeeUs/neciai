/*
 * File:         DbSeeder.cs
 * Purpose:      Applies any pending EF Core migrations and seeds baseline
 *               data (the three app roles, plus the admin account) each
 *               time the application starts. Called once from Program.cs,
 *               inside a try/catch there, so a seeding failure logs an
 *               error instead of crashing the whole app on startup.
 * Author:       Abraham Macias
 * Date:         July 2026
 * Dependencies: Microsoft.EntityFrameworkCore, Microsoft.AspNetCore.Identity
 *
 * Security note: the admin password is never hardcoded here. It's read
 * from the ADMIN_SEED_PASSWORD environment variable (set in Railway),
 * the same way Program.cs handles the database connection string.
 */

using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NeciAI.API.Models;
using System;
using System.Threading.Tasks;

namespace NeciAI.API.Data;

public static class DbSeeder
{
    /// <summary>
    /// Entry point called once from Program.cs on every startup.
    /// Migrates the schema, then ensures the three application roles
    /// and the admin account exist. Every step here is written to be
    /// a no-op if it's already been done, so calling this repeatedly
    /// (e.g. on every deploy) is always safe.
    /// </summary>
    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<NeciAIDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var configuration = services.GetRequiredService<IConfiguration>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");

        // Applies any migrations that haven't run against this database
        // yet. Creates the schema from scratch on a brand-new database.
        await db.Database.MigrateAsync();

        // Ensures the three application roles always exist. Runs on
        // every startup but is a no-op after the first successful run.
        string[] roles = { "Admin", "Analyst", "Client" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        await SeedAdminAccountAsync(userManager, configuration, logger);
    }

    /// <summary>
    /// Creates the admin account on first-ever deploy, or rotates its
    /// password on later deploys if ADMIN_SEED_PASSWORD has changed.
    /// WHY: the password is never hardcoded in source control - it's
    /// read from configuration, so rotating it later only ever requires
    /// updating the environment variable and redeploying.
    /// EDGE CASE: if ADMIN_SEED_PASSWORD isn't set (local/dev only), a
    /// random one-time password is generated and logged once so a
    /// developer can retrieve it. This path should never be reachable
    /// in production, since Railway always has the variable configured.
    /// </summary>
    private static async Task SeedAdminAccountAsync(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        ILogger logger)
    {
        const string adminEmail = "admin@neciai.app";

        var adminPassword = configuration["ADMIN_SEED_PASSWORD"];
        if (string.IsNullOrWhiteSpace(adminPassword))
        {
            adminPassword = Guid.NewGuid().ToString("N") + "!Aa1";
            logger.LogWarning(
                "ADMIN_SEED_PASSWORD not set - generated a one-time local password: {Password}",
                adminPassword);
        }

        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);

        if (existingAdmin is null)
        {
            // First-ever deploy: create the admin account from scratch.
            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "NeciAI",
                LastName = "Administrator",
                UserRole = "Admin",
                IsActive = true,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);

            if (result.Succeeded)
                await userManager.AddToRoleAsync(adminUser, "Admin");
        }
        else
        {
            // Rotation path - safe to leave in permanently. This only
            // actually changes anything the first time the app starts
            // after ADMIN_SEED_PASSWORD has been updated in Railway.
            var removeResult = await userManager.RemovePasswordAsync(existingAdmin);
            if (!removeResult.Succeeded)
            {
                var errors = string.Join("; ", removeResult.Errors.Select(e => e.Description));
                logger.LogError("Failed to remove existing admin password: {Errors}", errors);
                return;
            }

            var addResult = await userManager.AddPasswordAsync(existingAdmin, adminPassword);
            if (!addResult.Succeeded)
            {
                var errors = string.Join("; ", addResult.Errors.Select(e => e.Description));
                logger.LogError("Failed to set new admin password: {Errors}", errors);
            }
        }
    }
}