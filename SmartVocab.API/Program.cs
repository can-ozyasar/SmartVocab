using Microsoft.EntityFrameworkCore;
using SmartVocab.Infrastructure.Persistence;
using SmartVocab.Domain.Interfaces;        
using SmartVocab.Infrastructure.Repositories; 
// --- SERVICES ---

using SmartVocab.Application.Interfaces; 
using SmartVocab.Application.Services;   
using Microsoft.AspNetCore.Authentication.JwtBearer; 
using Microsoft.IdentityModel.Tokens;               
using System.Text;                                  

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IWordService, WordService>();
builder.Services.AddScoped<IStudyService, StudyService>();


// Controller'ları (API uçlarını)  
builder.Services.AddControllers();

// Swagger/OpenAPI dokümantasyonu için gerekli servisler.
// API'mizi test etmek için kullanacağız.
builder.Services.AddEndpointsApiExplorer();


// Swagger'a "Authorize" butonu eklemek için özel ayar
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});



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


// JWT Ayarlarını oku
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(secretKey)
    };
});
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
app.UseAuthentication(); // <-- YENİ EKLENDİ (Kimlik Kontrolü)

app.UseAuthorization(); // Yetkilendirme (İleride JWT ekleyeceğiz).

app.MapControllers(); // Controller rotalarını eşleştir.

// Uygulamayı başlat.
app.Run();