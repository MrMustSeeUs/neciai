/*
 * File:    Program.cs
 * Purpose: Application entry point and service configuration for NeciAI API.
 *          Configures dependency injection, authentication, authorization,
 *          CORS, Entity Framework, and all middleware in the correct order.
 * Author:  Abraham Macias
 * Date:    June 2026
 * Dependencies: ASP.NET Core 8, Entity Framework Core, JWT Bearer Auth,
 *               ASP.NET Identity, Npgsql, EPPlus, iText7
 */

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NeciAI.API.Data;
using NeciAI.API.Interfaces;
using NeciAI.API.Models;
using NeciAI.API.Services;
using OfficeOpenXml;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────────────────────────
// EPPLUS LICENSE — Required for non-commercial use
// ─────────────────────────────────────────────────────────────
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

// ─────────────────────────────────────────────────────────────
// DATABASE — PostgreSQL via Entity Framework Core
// Reads connection string from appsettings.json
// ─────────────────────────────────────────────────────────────
builder.Services.AddDbContext<NeciAIDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(3)
    )
);

// ─────────────────────────────────────────────────────────────
// ASP.NET IDENTITY — User management and authentication
// Configured with strong password requirements for security
// ─────────────────────────────────────────────────────────────
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password security requirements
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;

    // Lockout settings — lock after 5 failed attempts
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<NeciAIDbContext>()
.AddDefaultTokenProviders();

// ─────────────────────────────────────────────────────────────
// JWT AUTHENTICATION
// Validates every incoming request's JWT token
// ─────────────────────────────────────────────────────────────
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"]
    ?? throw new InvalidOperationException("JWT SecretKey is not configured.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero // No tolerance for expired tokens
    };
});

// ─────────────────────────────────────────────────────────────
// AUTHORIZATION POLICIES
// Role-based access control for Admin, Analyst, Client
// ─────────────────────────────────────────────────────────────
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly",
        policy => policy.RequireRole("Admin"));

    options.AddPolicy("AnalystOrAbove",
        policy => policy.RequireRole("Admin", "Analyst"));

    options.AddPolicy("AllUsers",
        policy => policy.RequireRole("Admin", "Analyst", "Client"));
});

// ─────────────────────────────────────────────────────────────
// CORS — Allow React frontend to call this API
// ─────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("NeciAICorsPolicy", policy =>
    {
        policy.WithOrigins(
            "http://localhost:3000",
            "http://localhost:5173",
            "https://neciai-ai.vercel.app",
            "https://neciai-c2fsnomq9-abraham-macias.vercel.app"
        )
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});

// ─────────────────────────────────────────────────────────────
// DEPENDENCY INJECTION — Register our services
// This is what connects interfaces to their implementations
// ─────────────────────────────────────────────────────────────
builder.Services.AddScoped<IFinancialService, FinancialService>();
builder.Services.AddScoped<IReportService, ReportService>();

// ─────────────────────────────────────────────────────────────
// CONTROLLERS AND API CONFIGURATION
// ─────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ─────────────────────────────────────────────────────────────
// SWAGGER — API documentation with JWT support
// Allows testing endpoints directly from the browser
// ─────────────────────────────────────────────────────────────
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "NeciAI API",
        Version = "v1",
        Description = "Intelligent Financial Data Analysis Platform — " +
                      "D424 Software Engineering Capstone | Abraham Macias",
        Contact = new OpenApiContact
        {
            Name = "Abraham Macias",
            Email = "amaci84@wgu.edu"
        }
    });

    // Add JWT authorization button to Swagger UI
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Enter your JWT token: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ─────────────────────────────────────────────────────────────
// BUILD THE APPLICATION
// ─────────────────────────────────────────────────────────────
var app = builder.Build();

// ─────────────────────────────────────────────────────────────
// MIDDLEWARE PIPELINE — Order matters here!
// Each request passes through these in sequence
// ─────────────────────────────────────────────────────────────

// Show Swagger in development mode
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "NeciAI API v1");
    options.RoutePrefix = "swagger";
});

// Redirect HTTP to HTTPS
app.UseHttpsRedirection();

// Apply CORS policy — must come before auth
app.UseCors("NeciAICorsPolicy");

// Enable authentication — validates JWT tokens
app.UseAuthentication();

// Enable authorization — enforces role policies
app.UseAuthorization();

// Map controller routes
app.MapControllers();

// ─────────────────────────────────────────────────────────────
// AUTO-MIGRATE DATABASE ON STARTUP
// Creates all tables automatically if they don't exist
// ─────────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NeciAIDbContext>();
    var userManager = scope.ServiceProvider
        .GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider
        .GetRequiredService<RoleManager<IdentityRole>>();

    // Apply any pending migrations
    await db.Database.MigrateAsync();

    // Seed default roles if they don't exist
    string[] roles = { "Admin", "Analyst", "Client" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    // Seed default admin user if no users exist
    if (!userManager.Users.Any())
    {
        var adminUser = new ApplicationUser
        {
            UserName = "admin@neciai.app",
            Email = "admin@neciai.app",
            FirstName = "NeciAI",
            LastName = "Administrator",
            UserRole = "Admin",
            IsActive = true,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(
            adminUser, "Admin@NeciAI2026!");

        if (result.Succeeded)
            await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}

app.Run();