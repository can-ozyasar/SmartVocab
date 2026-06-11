# SmartVocab

Kelime öğrenme süreçlerini desteklemek için .NET backend ve web arayüzünden oluşan akıllı sözlük uygulaması.

## Öne Çıkanlar
- Clean Architecture benzeri katmanlı .NET çözüm yapısı
- API, domain, application, infrastructure ve test projeleri
- Frontend için ayrı UI modülü
- Kullanılan ana teknolojiler: C#, Node.js, npm, React, Vite, .NET, HTML, CSS

## Kurulum
```bash
npm install
dotnet restore "SmartVocab.sln"
```

## Çalıştırma
```bash
cd SmartVocab.UI && npm run dev
dotnet build "SmartVocab.sln"
```

## Önemli Dosyalar
- `SmartVocab.API/SmartVocab.API.csproj`
- `SmartVocab.Application/SmartVocab.Application.csproj`
- `SmartVocab.Domain/SmartVocab.Domain.csproj`
- `SmartVocab.Infrastructure/SmartVocab.Infrastructure.csproj`
- `SmartVocab.Tests/SmartVocab.Tests.csproj`
- `SmartVocab.UI/index.html`
- `SmartVocab.UI/package.json`
- `SmartVocab.sln`
- `package.json`

## Proje Yapısı
- `SmartVocab.Application` - 25 dosya
- `SmartVocab.UI` - 24 dosya
- `SmartVocab.API` - 11 dosya
- `SmartVocab.Domain` - 9 dosya
- `SmartVocab.Infrastructure` - 7 dosya
- `SmartVocab.Tests` - 2 dosya
- `.config` - 1 dosya
- `SmartVocab.sln` - 1 dosya

## Geliştirme Notları
- README içeriği, repodaki mevcut dosya yapısı ve proje açıklamasına göre düzenlenmiştir.
- Yeni modül, veri seti veya servis eklendiğinde kurulum/çalıştırma bölümlerini güncelleyin.
- Frontend projelerinde sürüm uyumu için `package-lock.json`/`pnpm-lock.yaml` gibi lock dosyalarını koruyun.
- .NET projelerinde solution yapısı değişirse `dotnet restore` ve `dotnet build` adımlarını yeniden doğrulayın.

## Lisans
Bu repoda açık bir lisans dosyası yoksa tüm haklar varsayılan olarak proje sahibine aittir. Paylaşım veya kullanım koşulları için repo sahibine danışın.
