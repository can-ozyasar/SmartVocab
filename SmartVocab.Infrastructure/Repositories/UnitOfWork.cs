using SmartVocab.Domain.Interfaces;
using SmartVocab.Infrastructure.Persistence;
using System.Threading.Tasks;
using System.Threading;       

namespace SmartVocab.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> CommitAsync()
        {
            return await _context.SaveChangesAsync();
        }


        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _context.SaveChangesAsync(cancellationToken);
        }
        
        
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}