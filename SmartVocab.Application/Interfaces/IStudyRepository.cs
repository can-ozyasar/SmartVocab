using System;
using System.Threading.Tasks;
using SmartVocab.Domain.Entities;
using System.Collections.Generic;
namespace SmartVocab.Application.Interfaces
{
    public interface IStudyRepository
    {
        Task<UserWordState> GetUserWordStateAsync(Guid userId, Guid wordId);
        Task AddUserWordStateAsync(UserWordState state);
        Task SaveChangesAsync();
        Task<List<UserWordState>> GetDueWordsAsync(Guid userId, int limit);
    }
}