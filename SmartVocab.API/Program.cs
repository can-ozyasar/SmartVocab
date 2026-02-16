using Microsoft.EntityFrameworkCore;
using SmartVocab.Infrastructure.Persistence;
using SmartVocab.Domain.Interfaces;        // <-- BUNU EKLE (Interface'ler için)
using SmartVocab.Infrastructure.Repositories; // <-- BUNU EKLE (Class'lar için)
// --- SERVICES ---

using SmartVocab.Application.Interfaces; 
using SmartVocab.Application.Services;   


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<IUserService, UserService>();

// ==================================================================
// 1. SERVICES (Dependency Injection Container)
// Burası, uygulamamızın kullanacağı alet çantasını hazırladığımız yerdir.
// ==================================================================

// Controller'ları (API uçlarını) sisteme ekle.
builder.Services.AddControllers();

// Swagger/OpenAPI dokümantasyonu için gerekli servisler.
// API'mizi test etmek için kullanacağız.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- VERİTABANI BAĞLANTISI ---
// appsettings.json dosyasından bağlantı cümlesini (Connection String) okuyoruz.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// PostgreSQL sürücüsünü (Npgsql) kullanarak DbContext'i ekliyoruz.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));


// --- REPOSITORY & UNIT OF WORK ---
// Scoped: Her HTTP isteği için yeni bir tane oluşturulur.
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();



// ==================================================================
// 2. BUILD (Uygulamayı İnşa Et)
// ==================================================================
var app = builder.Build();

// ==================================================================
// 3. MIDDLEWARE PIPELINE (HTTP İstek Hattı)
// Gelen isteğin nasıl işleneceğini sırayla belirtiyoruz.
// ==================================================================

// Sadece geliştirme ortamındaysak Swagger'ı aç.
// Prodüksiyonda güvenlik açığı olmaması için kapatılır.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection(); // HTTP isteklerini HTTPS'e zorla (Güvenlik).

app.UseAuthorization(); // Yetkilendirme (İleride JWT ekleyeceğiz).

app.MapControllers(); // Controller rotalarını eşleştir.

// Uygulamayı başlat.
app.Run();