/*
 * File:    AuthDTOs.cs
 * Purpose: Data Transfer Objects for authentication endpoints.
 *          DTOs define exactly what data is accepted from the client
 *          and returned in responses — keeping models separate from
 *          the API surface and preventing over-posting attacks.
 * Author:  Abraham Macias
 * Date:    June 2026
 */

using System;
using System.ComponentModel.DataAnnotations;

namespace NeciAI.API.DTOs
{
	/// <summary>
	/// Data received when a new user registers.
	/// Validation attributes enforce input rules before
	/// any business logic runs — satisfies validation requirement.
	/// </summary>
	public class RegisterDto
	{
		[Required(ErrorMessage = "First name is required.")]
		[MaxLength(100)]
		public string FirstName { get; set; } = string.Empty;

		[Required(ErrorMessage = "Last name is required.")]
		[MaxLength(100)]
		public string LastName { get; set; } = string.Empty;

		[Required(ErrorMessage = "Email is required.")]
		[EmailAddress(ErrorMessage = "Invalid email format.")]
		public string Email { get; set; } = string.Empty;

		[Required(ErrorMessage = "Password is required.")]
		[MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
		public string Password { get; set; } = string.Empty;

		// Role defaults to Client if not specified
		public string Role { get; set; } = "Client";
	}

	/// <summary>
	/// Data received when a user logs in.
	/// </summary>
	public class LoginDto
	{
		[Required(ErrorMessage = "Email is required.")]
		[EmailAddress]
		public string Email { get; set; } = string.Empty;

		[Required(ErrorMessage = "Password is required.")]
		public string Password { get; set; } = string.Empty;
	}

	/// <summary>
	/// Data returned after successful login or registration.
	/// Contains the JWT token and basic user information.
	/// </summary>
	public class AuthResponseDto
	{
		public string Token { get; set; } = string.Empty;
		public string UserId { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string FullName { get; set; } = string.Empty;
		public string Role { get; set; } = string.Empty;
		public DateTime ExpiresAt { get; set; }
	}

	/// <summary>
	/// Data returned for user profile requests.
	/// </summary>
	public class UserProfileDto
	{
		public string UserId { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string FirstName { get; set; } = string.Empty;
		public string LastName { get; set; } = string.Empty;
		public string Role { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; }
		public DateTime? LastLoginAt { get; set; }
	}
}