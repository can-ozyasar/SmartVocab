using SmartVocab.Application.DTOs.Word;
using SmartVocab.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartVocab.Application.Interfaces
{
    public interface IWordService
    {
        Task<Guid> CreateWordAsync(CreateWordDto dto);
        Task<IEnumerable<Word>> GetAllWordsAsync();
        
        Task UpdateWordAsync(UpdateWordDto dto);
        Task DeleteWordAsync(Guid id);
    }
}