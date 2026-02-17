using System;
using System.Threading.Tasks;

namespace SmartVocab.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        // Değişiklikleri veritabanına yazar (SaveChanges)
        Task<int> CommitAsync();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}