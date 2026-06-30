/*
 * File:    ApplicationUser.cs
 * Purpose: Extends ASP.NET Identity's IdentityUser with NeciAI-specific
 *          profile fields. Demonstrates INHERITANCE from both IdentityUser
 *          and the role of ENCAPSULATION through property access control.
 * Author:  Abraham Macias
 * Date:    June 2026
 */

using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace NeciAI.API.Models
{
    /// <summary>
    /// Custom application user that extends ASP.NET Identity's IdentityUser.
    /// Inherits all built-in auth fields (username, email, password hash)
    /// and adds NeciAI-specific profile properties.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        // User's first name — required for personalized dashboard display
        public string FirstName { get; set; } = string.Empty;

        // User's last name
        public string LastName { get; set; } = string.Empty;

        // Role within the organization: Admin, Analyst, or Client
        public string UserRole { get; set; } = "Client";

        // Account creation timestamp
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Tracks last login for security monitoring
        public DateTime? LastLoginAt { get; set; }

        // Whether this account is active — allows disabling without deleting
        public bool IsActive { get; set; } = true;

        // Navigation property — one user can have many financial records
        public virtual ICollection<FinancialRecord> FinancialRecords { get; set; }
            = new List<FinancialRecord>();

        // Navigation property — one user can generate many reports
        public virtual ICollection<Report> Reports { get; set; }
            = new List<Report>();

        /// <summary>
        /// Returns the user's full display name.
        /// Encapsulates the name formatting logic inside the model.
        /// </summary>
        public string GetFullName() => $"{FirstName} {LastName}".Trim();
    }
}