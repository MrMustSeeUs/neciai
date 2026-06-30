// ============================================================
// Program.cs
// Application entry point, dependency injection registration,
// middleware pipeline configuration, and CORS policy setup.
//
// Author: Abraham Macias
// Stack:  ASP.NET Core 8, Entity Framework Core, ASP.NET Identity, JWT
// ============================================================

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NeciAI.API.Data;
using NeciAI.API.Interfaces;
using NeciAI.API.Models;
using NeciAI.API.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ── DATABASE ────────────────────────────────────────────────
// Register the EF Core DbContext with the Npgsql PostgreSQL provider.
// The connection string is injected at runtime via environment variable
// so credentials never live in source control.
builder.Services.AddDbContext<NeciAIDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── IDENTITY ────────────────────────────────────────────────
// Configure ASP.NET Identity for user management and password hashing.
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit           = true;
    options.Password.RequireLowercase       = true;
    options.Password.RequireUppercase       = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength         = 8;
    options.User.RequireUniqueEmail         = true;
})
.AddEntityFrameworkStores<NeciAIDbContext>()
.AddDefaultTokenProviders();

// ── JWT AUTHENTICATION ───────────────────────────────────────
// Validate incoming Bearer tokens using the symmetric key and
// issuer/audience values defined in configuration.
var jwtKey      = builder.Configuration["JwtSettings:SecretKey"]      ?? throw new InvalidOperationException("JWT secret key is not configured.");
var jwtIssuer   = builder.Configuration["JwtSettings:Issuer"]         ?? "NeciAI.API";
var jwtAudience = builder.Configuration["JwtSettings:Audience"]       ?? "NeciAI.Client";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateIssuer           = true,
        ValidIssuer              = jwtIssuer,
        ValidateAudience         = true,
        ValidAudience            = jwtAudience,
        ValidateLifetime         = true,
        ClockSkew                = TimeSpan.Zero,
    };
});

// ── CORS ─────────────────────────────────────────────────────
// Read allowed origins from configuration so they can be updated
// without modifying source code.
var allowedOrigins = builder.Configuration
    .GetSection("CorsSettings:AllowedOrigins")
    .Get<string[]>()
    ?? new[]
    {
        "http://localhost:3000",
        "http://localhost:5173",
        "https://neciai-ai.vercel.app",
        "https://neciai-c2fsnomq9-abraham-macias.vercel.app",
    };

builder.Services.AddCors(options =>
{
    options.AddPolicy("NeciAICorsPolicy", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());
});

// ── APPLICATION SERVICES ─────────────────────────────────────
// Register domain services against their interfaces for loose coupling.
builder.Services.AddScoped<IFinancialService, FinancialService>();
builder.Services.AddScoped<IReportService,    ReportService>();

// ── MVC & SWAGGER ────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "NeciAI API",
        Version     = "v1",
        Description = "Intelligent Financial Data Analysis Platform",
        Contact     = new OpenApiContact { Name = "Abraham Macias" },
    });

    // Enable the Authorize button in Swagger UI for JWT testing.
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.ApiKey,
        Scheme       = "Bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Enter: Bearer {your JWT token}",
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer",
                }
            },
            Array.Empty<string>()
        }
    });
});

// ── BUILD ────────────────────────────────────────────────────
var app = builder.Build();

// ── SEED DATABASE ────────────────────────────────────────────
// Apply pending migrations and seed the admin user on startup.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await DbSeeder.SeedAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// ── MIDDLEWARE PIPELINE ──────────────────────────────────────
// Swagger is enabled in all environments so the evaluator and
// developers can explore the API without a local build.
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "NeciAI API v1");
    options.RoutePrefix = "swagger";
});

app.UseCors("NeciAICorsPolicy");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
