using SmartVocab.Application.DTOs.Word;
using SmartVocab.Application.Interfaces;
using SmartVocab.Domain.Entities;
using SmartVocab.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq; // Any() ve FirstOrDefault() için
using System.Threading.Tasks;

namespace SmartVocab.Application.Services
{
    public class WordService : IWordService
    {
        private readonly IGenericRepository<Word> _wordRepository;
        private readonly IUnitOfWork _unitOfWork;

        public WordService(IGenericRepository<Word> wordRepository, IUnitOfWork unitOfWork)
        {
            _wordRepository = wordRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> CreateWordAsync(CreateWordDto dto)
        {
            // 1. Kural: Aynı kelime daha önce eklenmiş mi?
            // Veritabanından kelimeyi küçük harfe çevirip arıyoruz (Case Insensitive).
            var existingWords = await _wordRepository.FindAsync(w => w.Text.ToLower() == dto.Text.ToLower());
            
            if (existingWords.Any())
            {
                throw new Exception($"'{dto.Text}' kelimesi zaten sistemde mevcut.");
            }

            // 2. Mapping (DTO -> Entity)
            var newWord = new Word
            {
                Text = dto.Text,
                Meaning = dto.Meaning,
                ExampleSentence = dto.ExampleSentence,
                Type = dto.Type,
                Level = dto.Level,
                CreatedAt = DateTime.UtcNow,
                // Varsayılan değerler
                FrequencyIndex = 0.5, 
                PronunciationUrl = ""
            };

            // 3. Kayıt
            await _wordRepository.AddAsync(newWord);
            await _unitOfWork.CommitAsync();

            return newWord.Id;
        }

        public async Task<IEnumerable<Word>> GetAllWordsAsync()
        {
            return await _wordRepository.GetAllAsync();
        }




        public async Task UpdateWordAsync(UpdateWordDto dto)
{
    var word = await _wordRepository.GetByIdAsync(dto.Id);
    if (word == null) throw new Exception("Kelime bulunamadı.");

    // Sadece kelime sahibi güncelleyebilir (Güvenlik kontrolü controller'da userId ile yapılmalı ama burada da repository seviyesinde filter var mı emin olalım. Şimdilik ID yeterli.)
    
    word.Text = dto.Text;
    word.Meaning = dto.Meaning;
    word.ExampleSentence = dto.ExampleSentence;
    word.Type = dto.Type;
    word.Level = dto.Level;

    _wordRepository.Update(word);
    await _unitOfWork.CommitAsync();
}

public async Task DeleteWordAsync(Guid id)
{
    var word = await _wordRepository.GetByIdAsync(id);
    if (word == null) throw new Exception("Kelime bulunamadı.");

    _wordRepository.Delete(word);
    await _unitOfWork.CommitAsync();
}
    }
}