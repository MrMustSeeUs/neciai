/*
 * File:    AuthController.cs
 * Purpose: Handles user registration, login, and profile endpoints.
 *          Issues JWT tokens on successful authentication.
 *          Implements industry-appropriate security features including
 *          password hashing, account lockout, and JWT expiration.
 * Author:  Abraham Macias
 * Date:    June 2026
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NeciAI.API.DTOs;
using NeciAI.API.Models;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NeciAI.API.Controllers
{
    /// <summary>
    /// Handles all authentication operations for NeciAI.
    /// Routes are prefixed with /api/auth
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        // Private fields — ENCAPSULATION of dependencies
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        /// <summary>
        /// POST /api/auth/register
        /// Creates a new user account with the specified role.
        /// Returns a JWT token on success.
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            // ModelState validation — catches DTO validation failures
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check if email already exists
            var existing = await _userManager.FindByEmailAsync(dto.Email);
            if (existing != null)
                return Conflict(new { message = "Email already registered." });

            // Create the new user
            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                UserRole = dto.Role,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                return BadRequest(new
                {
                    message = "Registration failed.",
                    errors = result.Errors.Select(e => e.Description)
                });

            // Assign role — defaults to Client
            var role = dto.Role is "Admin" or "Analyst" or "Client"
                ? dto.Role : "Client";
            await _userManager.AddToRoleAsync(user, role);

            // Generate and return JWT token
            var token = await GenerateJwtTokenAsync(user);
            return Ok(token);
        }

        /// <summary>
        /// POST /api/auth/login
        /// Authenticates a user and returns a JWT token.
        /// Records the login timestamp for security monitoring.
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Find user by email
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || !user.IsActive)
                return Unauthorized(new { message = "Invalid credentials." });

            // Attempt sign in — handles lockout automatically
            var result = await _signInManager.CheckPasswordSignInAsync(
                user, dto.Password, lockoutOnFailure: true);

            if (result.IsLockedOut)
                return StatusCode(423,
                    new { message = "Account locked. Try again in 15 minutes." });

            if (!result.Succeeded)
                return Unauthorized(new { message = "Invalid credentials." });

            // Update last login timestamp
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            var token = await GenerateJwtTokenAsync(user);
            return Ok(token);
        }

        /// <summary>
        /// GET /api/auth/profile
        /// Returns the authenticated user's profile information.
        /// Requires a valid JWT token.
        /// </summary>
        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);

            if (user == null)
                return NotFound(new { message = "User not found." });

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new UserProfileDto
            {
                UserId = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = roles.FirstOrDefault() ?? "Client",
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            });
        }

        /// <summary>
        /// Generates a signed JWT token for the authenticated user.
        /// Token contains user ID, email, and role claims.
        /// Private method — encapsulated within the controller.
        /// </summary>
        private async Task<AuthResponseDto> GenerateJwtTokenAsync(
            ApplicationUser user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"]!;
            var expirationHours = int.Parse(
                jwtSettings["ExpirationHours"] ?? "24");

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "Client";

            // Build claims — data embedded in the token
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.GivenName, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti,
                    Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256);
            var expiration = DateTime.UtcNow.AddHours(expirationHours);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: expiration,
                signingCredentials: credentials
            );

            return new AuthResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                UserId = user.Id,
                Email = user.Email!,
                FullName = user.GetFullName(),
                Role = role,
                ExpiresAt = expiration
            };
        }
    }
}