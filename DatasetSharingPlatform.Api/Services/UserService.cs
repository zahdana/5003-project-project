using DatasetSharingPlatform.Api.Models;
using DatasetSharingPlatform.Api.Data;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DatasetSharingPlatform.Api.Services
{
    public class UserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        // 用户注册
        public async Task<AuthResult> RegisterAsync(string username, string password)
        {
            var existingUser = await _context.Users
                .Where(u => u.Username == username)
                .FirstOrDefaultAsync();

            if (existingUser != null)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = "用户名已存在"
                };
            }

            var passwordHash = HashPassword(password);

            var user = new User
            {
                Username = username,
                PasswordHash = passwordHash,
                IsAdmin = false, // 普通用户默认不是管理员
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new AuthResult
            {
                Success = true,
                Message = "注册成功"
            };
        }

        // 用户登录
        public async Task<AuthResult> LoginAsync(string username, string password)
        {
            var user = await _context.Users
                .Where(u => u.Username == username)
                .FirstOrDefaultAsync();

            if (user == null || !VerifyPassword(password, user.PasswordHash))
            {
                return new AuthResult
                {
                    Success = false,
                    Message = "用户名或密码错误"
                };
            }

            var token = GenerateJwtToken(user);

            return new AuthResult
            {
                Success = true,
                Token = token,
                IsAdmin = user.IsAdmin
            };
        }

        // 密码哈希化
        private string HashPassword(string password)
        {
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            string hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8
            ));

            // 将 salt 和哈希值合并并以冒号分隔进行存储
            return Convert.ToBase64String(salt) + ":" + hashedPassword;
        }

        // 验证密码
        private bool VerifyPassword(string password, string storedHash)
        {
            // 分离 salt 和哈希值
            var parts = storedHash.Split(':');
            if (parts.Length != 2)
            {
                return false;  // 格式错误
            }

            byte[] salt = Convert.FromBase64String(parts[0]);
            string storedPasswordHash = parts[1];

            // 使用存储的盐来验证密码
            string hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8
            ));

            return storedPasswordHash == hashedPassword;
        }

        // 生成 JWT Token
        private readonly IConfiguration _configuration;  // 注入配置文件读取器

        public UserService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // 生成 JWT Token
        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");

            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim("userId", user.Id.ToString()), // 自定义 Claim
            new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User")
             };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
