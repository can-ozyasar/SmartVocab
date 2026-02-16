using Microsoft.Extensions.Configuration; // AppSettings okumak için
using Microsoft.IdentityModel.Tokens;   // Token imzalamak için
using SmartVocab.Application.DTOs.User;
using SmartVocab.Application.Interfaces;
using SmartVocab.Domain.Entities;
using SmartVocab.Domain.Interfaces;
using System;
using System.IdentityModel.Tokens.Jwt;  // JWT oluşturucu
using System.Security.Claims;           // Token içine veri gömmek için
using System.Text;
using System.Threading.Tasks;
using BCrypt.Net;                       // Şifreleme kütüphanesi

namespace SmartVocab.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IGenericRepository<User> _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration; // appsettings.json'a erişim

        public UserService(IGenericRepository<User> userRepository, IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public async Task<Guid> RegisterAsync(RegisterUserDto dto)
        {
            // 1. Validasyon
            var existingUsers = await _userRepository.FindAsync(u => u.Email == dto.Email);
            if (System.Linq.Enumerable.Any(existingUsers))
            {
                throw new Exception("Bu e-posta adresi zaten kullanılıyor.");
            }

            // 2. GÜVENLİ HASHLEME (BCrypt)
            // Asla şifreyi plain-text saklamayız. BCrypt otomatik "Salt" ekler.
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var newUser = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PasswordHash = passwordHash, // Hashlenmiş hali
                DailyGoalMinutes = dto.DailyGoalMinutes,
                CreatedAt = DateTime.UtcNow,
                NativeLanguage = "Turkish"
            };

            await _userRepository.AddAsync(newUser);
            await _unitOfWork.CommitAsync();

            return newUser.Id;
        }

        public async Task<string> LoginAsync(LoginUserDto dto)
        {
            // 1. Kullanıcıyı Bul
            var users = await _userRepository.FindAsync(u => u.Email == dto.Email);
            var user = System.Linq.Enumerable.FirstOrDefault(users);

            if (user == null)
            {
                throw new Exception("Kullanıcı bulunamadı veya şifre hatalı.");
            }

            // 2. Şifreyi Doğrula
            // Kullanıcının girdiği şifreyi (dto.Password) hashleyip, veritabanındaki hash ile kıyaslar.
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);

            if (!isPasswordValid)
            {
                throw new Exception("Kullanıcı bulunamadı veya şifre hatalı."); // Güvenlik için muğlak hata dönüyoruz.
            }

            // 3. Token Oluştur (JWT Generation)
            return GenerateJwtToken(user);
        }

        // Yardımcı Metod: Token Üretici
        private string GenerateJwtToken(User user)
        {
            // Token'ın içine gömeceğimiz bilgiler (Payload)
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // UserId
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.GivenName, user.FirstName)
            };

            // İmza Anahtarı (appsettings.json'dan)
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(double.Parse(_configuration["JwtSettings:DurationInMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}