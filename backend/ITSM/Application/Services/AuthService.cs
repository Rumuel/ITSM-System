using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            var userName = request.UserName.Trim();
            var email = request.Email.Trim();
            var name = request.FullName.Trim();

            var existingUser = await _userManager.FindByNameAsync(userName);
            if (existingUser != null)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Username already exists"
                };
            }

            var existingEmail = await _userManager.FindByEmailAsync(email);
            if (existingEmail != null)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Email already exists"
                };
            }

            var user = new ApplicationUser
            {
                UserName = userName,
                Email = email,
                Name = name,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = string.Join(", ", result.Errors.Select(e => e.Description))
                };
            }

            return new AuthResponse
            {
                Success = true,
                Message = "Registration successful",
                User = MapToDto(user)
            };
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.UserName.Trim());
            if (user == null)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Invalid username or password"
                };
            }

            if (!user.IsActive)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "User account is inactive"
                };
            }

            var result = await _signInManager.PasswordSignInAsync(user, request.Password, false, false);
            if (!result.Succeeded)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Invalid username or password"
                };
            }

            var token = GenerateJwtToken(user);
            return new AuthResponse
            {
                Success = true,
                Message = "Login successful",
                Token = token,
                User = MapToDto(user)
            };
        }

        public async Task<UserDto?> GetCurrentUserAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            return user == null ? null : MapToDto(user);
        }

        private string GenerateJwtToken(ApplicationUser user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? string.Empty));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpiresInMinutes"] ?? "60")),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static UserDto MapToDto(ApplicationUser user)
        {
            return new UserDto
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FullName = user.Name,
                IsActive = user.IsActive
            };
        }
    }
}
