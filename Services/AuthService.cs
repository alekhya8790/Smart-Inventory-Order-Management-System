using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartIOMS.Data;
using SmartIOMS.DTOs.Auth;
using SmartIOMS.Models;

namespace SmartIOMS.Services
{
    public class AuthService
    {
        private readonly SmartIOMSDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(
            SmartIOMSDbContext context,
            IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }


        // ==========================================
        // REGISTER
        // ==========================================

        public async Task<User> RegisterAsync(RegisterRequest request)
        {
            bool emailExists = await _context.Users
                .AnyAsync(x => x.Email == request.Email);

            if (emailExists)
            {
                throw new Exception("Email already registered.");
            }

            string passwordHash =
                BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                PasswordHash = passwordHash,
                Role = "Customer",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            return user;
        }


        // ==========================================
        // LOGIN
        // ==========================================

        public async Task<AuthResponse?> LoginAsync(LoginRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x =>
                    x.Email == request.Email);

            if (user == null)
            {
                return null;
            }

            bool passwordValid =
                BCrypt.Net.BCrypt.Verify(
                    request.Password,
                    user.PasswordHash);

            if (!passwordValid)
            {
                return null;
            }

            if (!user.IsActive)
            {
                return null;
            }

            string token = GenerateJwtToken(user);

            int expiryMinutes =
                _configuration.GetValue<int>("Jwt:ExpiryMinutes");

            return new AuthResponse
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes)
            };
        }


        // ==========================================
        // JWT TOKEN GENERATION
        // ==========================================

        private string GenerateJwtToken(User user)
        {
            var jwtKey = _configuration["Jwt:Key"];

            if (string.IsNullOrWhiteSpace(jwtKey))
            {
                throw new InvalidOperationException(
                    "JWT key is not configured.");
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);


            var claims = new[]
            {
                new Claim(
                    JwtRegisteredClaimNames.Sub,
                    user.UserId.ToString()),

                new Claim(
                    ClaimTypes.Name,
                    user.FullName),

                new Claim(
                    ClaimTypes.Email,
                    user.Email),

                new Claim(
                    ClaimTypes.Role,
                    user.Role)
            };


            int expiryMinutes =
                _configuration.GetValue<int>("Jwt:ExpiryMinutes");

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }
    }
}