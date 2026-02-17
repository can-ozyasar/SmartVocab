using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartVocab.Application.DTOs.Study;
using SmartVocab.Application.Interfaces;
using System;
using System.Linq;
using System.Security.Claims; // Token'dan UserID okumak için
using System.Threading.Tasks;

namespace SmartVocab.API.Controllers
{
    [Authorize] // Sadece giriş yapmış kullanıcılar!
    [ApiController]
    [Route("api/[controller]")]
    public class StudyController : ControllerBase
    {
        private readonly IStudyService _studyService;

        public StudyController(IStudyService studyService)
        {
            _studyService = studyService;
        }

        // POST api/study/log
        // Kullanıcı bir kelimeye cevap verdiğinde bu endpoint çağrılır.
        [HttpPost("log")]
        public async Task<IActionResult> LogInteraction([FromBody] LogInteractionDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Token'ın içinden "UserId" bilgisini çekiyoruz.
                // Kullanıcı ID göndermez, Token'dan biz anlarız (Güvenlik).
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userIdString))
                    return Unauthorized("Kullanıcı kimliği doğrulanamadı.");

                var userId = Guid.Parse(userIdString);

                // Servise gönder (Hem logla hem algoritmayı çalıştır)
                await _studyService.LogInteractionAsync(userId, dto);

                return Ok(new { Message = "Etkileşim kaydedildi ve algoritma güncellendi." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        // GET api/study/session
        // Kullanıcı "Çalışmaya Başla" dediğinde burası çağrılır.
        [HttpGet("session")]
        public async Task<IActionResult> StartSession([FromQuery] int limit = 10)
        {
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdString)) return Unauthorized();
                var userId = Guid.Parse(userIdString);

                var words = await _studyService.GetNextSessionWordsAsync(userId, limit);
                
                // Eğer liste boşsa, kullanıcının çalışacak hiçbir şeyi kalmamış demektir.
                if (!System.Linq.Enumerable.Any(words))
                {
                    return Ok(new { Message = "Harika! Bugünlük çalışacak tüm kelimeleri bitirdin veya sistemde yeni kelime kalmadı.", IsFinished = true });
                }

                return Ok(words);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}