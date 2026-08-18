# Donanım ve Zimmet Takip Sistemi

Şirket içi IT varlıklarının; envanter, zimmet, stok, garanti, lisans, periyodik bakım ve teknik destek süreçlerini tek bir sistem üzerinden yönetmek amacıyla geliştirilmiş full-stack web uygulamasıdır.

## Özellikler

- Cihaz envanteri ve durum takibi
- Çalışanlara cihaz zimmetleme ve iade işlemleri
- Aktif zimmet sahibinin ilişkisel olarak takibi
- Stok giriş/çıkış işlemleri ve kritik stok kontrolü
- Garanti ve lisans sürelerinin takibi
- Periyodik bakım planları ve bakım görevleri
- Teknik destek taleplerinin yönetimi
- Admin, IT ve Employee rolleri için yetkilendirme
- Audit Log ile kritik işlemlerin izlenmesi
- Dashboard ve raporlama ekranları

## Teknolojiler

### Frontend
- React
- Vite
- TypeScript
- Ant Design
- Axios
- React Router

### Backend
- ASP.NET Core Web API (.NET 10)
- Entity Framework Core
- JWT Authentication
- Swagger
- xUnit

### Veritabanı
- Microsoft SQL Server

## Uygulama Mimarisi

```text
React UI
   ↓
Axios Services
   ↓
ASP.NET Core Web API
   ↓
Service Layer
   ↓
Entity Framework Core
   ↓
SQL Server
```

Frontend veritabanına doğrudan erişmez. Kullanıcı işlemleri API üzerinden backend'e iletilir. İş kuralları backend tarafında uygulanır ve veriler Entity Framework Core aracılığıyla SQL Server üzerinde yönetilir.

## Roller

### Admin
- Sistem genelindeki operasyonları yönetir
- Kullanıcı yönetimini gerçekleştirir
- Raporlara ve Audit Log kayıtlarına erişebilir

### IT
- Envanter, zimmet, stok, garanti, lisans ve bakım operasyonlarını yürütür
- Teknik destek taleplerini yönetir
- Employee hesabı oluşturabilir
- Raporlara erişebilir

### Employee
- Kendi dashboard'unu görüntüler
- Kendisine zimmetli cihazları görüntüler
- Kendi cihazları için teknik destek talebi oluşturabilir

## Temel İş Kuralları

- Bir cihaz aynı anda yalnızca bir aktif zimmete sahip olabilir.
- İade tamamlanmadan cihaz yeniden zimmetlenemez.
- Zimmet ve iade işlemlerinde cihaz durumu aynı transaction içerisinde güncellenir.
- Stok miktarı negatif olamaz.
- Kullanılan lisans sayısı toplam lisans sayısını aşamaz.
- Teknik destek talebi yalnızca çalışana aktif olarak zimmetli cihaz için açılabilir.
- Kritik işlemler Audit Log üzerinde tutulur.
- Yetkilendirme kontrolleri API tarafında uygulanır.

## Kimlik Doğrulama

Sistem JWT tabanlı authentication kullanır.

Kullanıcılar kullanıcı adı veya e-posta ve parola ile giriş yapabilir. Kullanıcı rolü ve gerekli personel bilgileri JWT üzerinden doğrulanır. Parolalar düz metin olarak saklanmaz.

## Kurulum

### Backend

```powershell
dotnet ef database update --project backend/TakipProgrami.Api.csproj --startup-project backend/TakipProgrami.Api.csproj
dotnet run --project backend/TakipProgrami.Api.csproj
```

Gizli değerler kaynak kod yerine `user-secrets`, environment variable veya uygun bir secret store üzerinden tanımlanmalıdır.

### Frontend

```powershell
cd frontend
npm install
npm run dev
```

## Build ve Test

Backend:

```powershell
dotnet build -c Release
dotnet test
```

Frontend:

```powershell
cd frontend
npm run lint
npm run build
```

Temel test kapsamı; rol ve yetki kontrolleri, tek aktif zimmet kuralı, stok ve lisans sınırları, JWT üzerinden current user doğrulaması ve Employee veri erişim sınırlarını içerir.

