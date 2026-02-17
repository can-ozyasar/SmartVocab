using Microsoft.EntityFrameworkCore;
using SmartVocab.Domain.Entities;

namespace SmartVocab.Infrastructure.Persistence
{
    // DbContext, EF Core'un kalbidir. Veritabanı burasıdır.
    public class ApplicationDbContext : DbContext
    {
        // Constructor: Ayarları (Bağlantı adresi vs.) dışarıdan alır.
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
            : base(options)
        {
        }

        // Domain katmanındaki Entity'leri buraya "Tablo" olarak tanıtıyoruz.
        // C# tarafındaki "Users" listesi, veritabanındaki "Users" tablosuna denk gelir.
        public DbSet<User> Users { get; set; }
        public DbSet<Word> Words { get; set; }
        public DbSet<UserWordState> UserWordStates { get; set; }
        public DbSet<UserWordLog> UserWordLogs { get; set; }
        
        // LearningSession'ı şimdilik yorum satırı yapıyorum, entity'yi oluşturunca açarız.
        // public DbSet<LearningSession> LearningSessions { get; set; }

        // Model oluşturulurken çalışacak özel ayarlar (Fluent API)
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Burası çok önemli. Veritabanı şemasını ince ayar yaptığımız yer.
            // Örn: Varsayılan şema 'public' olsun.
            base.OnModelCreating(modelBuilder);
            
            // ÖRNEK AYAR:
            // Word tablosundaki 'Text' alanı zorunlu olsun ve en fazla 100 karakter olsun.
            modelBuilder.Entity<Word>()
                .Property(w => w.Text)
                .IsRequired()
                .HasMaxLength(100);

            // User tablosunda Email alanı EŞSİZ (Unique) olmalı.
            // Aynı mail ile iki kişi kayıt olamaz.
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}