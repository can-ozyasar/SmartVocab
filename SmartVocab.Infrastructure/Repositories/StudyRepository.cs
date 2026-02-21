using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartVocab.Application.Interfaces;
using SmartVocab.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using SmartVocab.Infrastructure.Persistence; // ApplicationDbContext'in olduğu namespace'i kontrol et

namespace SmartVocab.Infrastructure.Repositories
{
    public class StudyRepository : IStudyRepository
    {
        private readonly ApplicationDbContext _context;

        public StudyRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserWordState> GetUserWordStateAsync(Guid userId, Guid wordId)
        {
            return await _context.UserWordStates
                .FirstOrDefaultAsync(w => w.UserId == userId && w.WordId == wordId);
        }

        public async Task AddUserWordStateAsync(UserWordState state)
        {
            await _context.UserWordStates.AddAsync(state);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<List<UserWordState>> GetDueWordsAsync(Guid userId, int limit)
        {
        return await _context.UserWordStates
            .Include(uws => uws.Word) // DİKKAT: Kelimenin Text ve Meaning bilgisini de çekiyoruz (JOIN)
            .Where(uws => uws.UserId == userId && uws.NextReviewDate <= DateTime.UtcNow)
            .OrderBy(uws => uws.NextReviewDate) // En çok gecikenleri en üste al
            .Take(limit) // Günlük limit (örn: 20 kelime)
            .ToListAsync();
        }
    }
}