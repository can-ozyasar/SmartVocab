using Microsoft.AspNetCore.Authorization; // [Authorize] için gerekli
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims; // ClaimTypes için gerekli
using System.Threading.Tasks;
using SmartVocab.Application.DTOs.Telemetry;
using SmartVocab.Application.Services;

namespace SmartVocab.API.Controllers
{
    [Authorize] // SADECE GİRİŞ YAPMIŞ KULLANICILAR GİREBİLİR
    [ApiController]
    [Route("api/study-session")]
    public class StudySessionController : ControllerBase
    {
        private readonly StudyAiService _aiService;

        public StudySessionController(StudyAiService aiService)
        {
            _aiService = aiService;
        }

        [HttpPost("telemetry")]
        public async Task<IActionResult> SaveTelemetry([FromBody] SessionTelemetryDto telemetry)
        {
            if (telemetry == null || telemetry.Interactions == null || telemetry.Interactions.Count == 0)
                return BadRequest("Geçersiz veya boş telemetri verisi.");

            try
            {
                // 1. JWT Token'ın içinden kullanıcının ID'sini (NameIdentifier) çıkar
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // 2. Eğer ID yoksa veya Guid formatında değilse kapı dışarı et
                if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
                {
                    return Unauthorized("Kullanıcı kimliği (Token) doğrulanamadı.");
                }

                // 3. Gerçek UserId ile Yapay Zeka Servisini çalıştır
                await _aiService.ProcessTelemetryAsync(userId, telemetry);

                return Ok(new { 
                    message = "AI Model başarıyla güncellendi", 
                    processedWords = telemetry.Interactions.Count 
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Telemetri hatası: {ex.Message}");
                return StatusCode(500, $"Sunucu hatası: {ex.Message}");
            }
        }

        // SmartVocab.API/Controllers/StudySessionController.cs içine ekle:

       [HttpGet("daily-blocks")]
// Parametrelere "[FromQuery] bool isVanilla = false" eklendi!
public async Task<IActionResult> GetDailyStudyBlocks([FromQuery] int limit = 20, [FromQuery] bool isVanilla = false)
{
    try
    {
        var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
        {
            return Unauthorized("Kullanıcı kimliği doğrulanamadı.");
        }

        // AI servisinden bloklanmış veriyi çek (Frontend'den gelen isVanilla bilgisini de servise iletiyoruz)
        var blocks = await _aiService.GetTodayStudyBlocksAsync(userId, limit, isVanilla);

        var totalWords = blocks.Sum(b => b.WordCount);

        return Ok(new {
            // Yanıt mesajını moda göre dinamik yaptık
            message = isVanilla ? "Sade (Vanilla) çalışma blokları hazırlandı." : "Günlük AI çalışma blokları hazırlandı.",
            totalWords = totalWords,
            blocks = blocks
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Blok getirme hatası: {ex.Message}");
        return StatusCode(500, $"Sunucu hatası: {ex.Message}");
    }
}


    }
}