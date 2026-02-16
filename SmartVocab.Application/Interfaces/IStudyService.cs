using SmartVocab.Application.DTOs.Study;
using System.Threading.Tasks;

namespace SmartVocab.Application.Interfaces
{
    public interface IStudyService
    {
        Task LogInteractionAsync(Guid userId, LogInteractionDto dto);
        Task<IEnumerable<StudyWordDto>> GetNextSessionWordsAsync(Guid userId, int limit = 10);
    }
}