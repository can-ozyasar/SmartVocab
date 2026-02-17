using Microsoft.AspNetCore.Authorization; // <-- GÜVENLİK İÇİN ŞART
using Microsoft.AspNetCore.Mvc;
using SmartVocab.Application.DTOs.Word;
using SmartVocab.Application.Interfaces;
using System;
using System.Threading.Tasks;

namespace SmartVocab.API.Controllers
{
    [Authorize] // <-- DİKKAT: Bu satır, "Sadece Token'ı olanlar girebilir" demektir.
    [ApiController]
    [Route("api/[controller]")]
    public class WordController : ControllerBase
    {
        private readonly IWordService _wordService;

        public WordController(IWordService wordService)
        {
            _wordService = wordService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateWordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var wordId = await _wordService.CreateWordAsync(dto);
                return Ok(new { Message = "Kelime eklendi.", WordId = wordId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var words = await _wordService.GetAllWordsAsync();
            return Ok(words);
        }
    }
}