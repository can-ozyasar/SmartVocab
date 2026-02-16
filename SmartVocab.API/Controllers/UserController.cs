using Microsoft.AspNetCore.Mvc;
using SmartVocab.Application.DTOs.User;
using SmartVocab.Application.Interfaces;
using System;
using System.Threading.Tasks;

namespace SmartVocab.API.Controllers
{
    // [ApiController]: Bu sınıfın bir API olduğunu belirtir (View döndürmez, JSON döndürür).
    [ApiController]
    // [Route]: Adres çubuğunda nasıl çağrılacağını belirler. 
    // [controller] yerine sınıfın adı (User) gelir -> "api/user"
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        // Dependency Injection: Servisi istiyoruz.
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // POST api/user/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
        {
            // Validasyon: DTO'daki kurallara (Email, Password length vs.) uyuyor mu?
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // 400 Hata Kodu
            }

            try
            {
                var userId = await _userService.RegisterAsync(dto);
                
                // 200 OK ve oluşturulan ID'yi dönüyoruz.
                return Ok(new { Message = "Kayıt başarılı!", UserId = userId });
            }
            catch (Exception ex)
            {
                // Hata durumunda (Örn: Email zaten var) 400 dönüyoruz.
                return BadRequest(new { Error = ex.Message });
            }
        }


        // POST api/user/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var token = await _userService.LoginAsync(dto);
                
                // Başarılı olursa Token'ı dönüyoruz.
                return Ok(new { Token = token, Message = "Giriş Başarılı" });
            }
            catch (Exception ex)
            {
                // Güvenlik gereği 401 Unauthorized dönmek daha doğrudur.
                return Unauthorized(new { Error = ex.Message });
            }
        }







    }
}
