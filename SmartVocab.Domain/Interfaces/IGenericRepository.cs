using SmartVocab.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SmartVocab.Domain.Interfaces
{
    // <T> demek: Bu repo her türlü class (User, Word) ile çalışabilir.
    // where T : BaseEntity demek: Ama sadece bizim BaseEntity'den türeyenlerle çalışır.
    public interface IGenericRepository<T> where T : BaseEntity
    {
        // Veri Okuma İşleri
        Task<T> GetByIdAsync(Guid id);
        Task<IEnumerable<T>> GetAllAsync();
        
        // Şuna benzer sorgular için: "Get(x => x.Email == 'test@test.com')"
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        // Veri Yazma İşleri
        Task AddAsync(T entity);
        void Update(T entity); // Update genelde async olmaz, sadece State değişir.
        void Delete(T entity);
    }
}