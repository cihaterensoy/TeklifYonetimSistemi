# Teklif Yönetim Sistemi

Teklif Yönetim Sistemi, işletmelerin müşteri ilişkilerini, projelerini, ürün portföylerini ve teklif süreçlerini uçtan uca yönetebilmeleri için geliştirilmiş, web tabanlı bir kurumsal yönetim (ERP/CRM) çözümüdür.

## 🚀 Öne Çıkan Özellikler

* **Teklif ve Sipariş Yönetimi:** Profesyonel teklifler oluşturma, bunları onay süreçlerine sokma ve onaylanan teklifleri siparişe dönüştürme özelliği.
* **PDF Raporlama:** Hazırlanan teklifleri otomatik olarak profesyonel PDF dökümanlarına dönüştürebilme.
* **Müşteri ve Proje Takibi:** Müşteri veritabanı oluşturma ve bu müşterilere bağlı projelerin detaylı takibi.
* **E-Logo Entegrasyonu:** E-Logo servisleri üzerinden entegrasyon desteği.
* **Gerçek Zamanlı İletişim:** SignalR tabanlı ChatHub aracılığıyla sistem içi anlık mesajlaşma.
* **Gelişmiş Dashboard:** Teklif durumlarını, satış istatistiklerini ve bekleyen işleri özetleyen görsel kontrol paneli.
* **E-Posta Bildirimleri:** Gmail SMTP entegrasyonu ile tekliflerin ve güncellemelerin e-posta yoluyla iletilmesi.
* **Rol Tabanlı Yetkilendirme:** Kullanıcıların yetki seviyelerine göre (Admin, Personel vb.) sisteme erişimi.

## 🛠️ Kullanılan Teknolojiler

* **Framework:** .NET Core / ASP.NET Core MVC
* **Veritabanı:** Entity Framework Core (Code First yaklaşımı)
* **Frontend:** HTML5, CSS3, JavaScript (Bootstrap & jQuery)
* **İletişim:** SignalR (Anlık bildirimler ve chat)
* **Entegrasyon:** SOAP/WCF servisleri (E-Logo için)

## 📂 Proje Yapısı

```text
TeklifYonetimSistemi/
├── Contexts/           # Veritabanı bağlantı ve context sınıfları
├── Controllers/        # İş mantığının yönetildiği kontrolcüler
├── Hubs/               # SignalR hub yapılandırmaları
├── Models/             # Veri modelleri ve ViewModeller
├── Services/           # PDF üretimi, E-Logo ve E-posta servisleri
├── Views/              # Arayüz (Razor View) dosyaları
├── wwwroot/            # Statik dosyalar (CSS, JS, Resimler)
└── Program.cs          # Uygulama başlangıç ve servis kayıtları
```

## ⚙️ Kurulum ve Yapılandırma

1.  **Gereksinimler:**
    * .NET SDK (projenin hedeflediği sürüm)
    * SQL Server
2.  **Veritabanı Ayarları:** `appsettings.json` dosyasındaki `ConnectionStrings` bölümünü kendi yerel veritabanı bilgilerinizle güncelleyin.
3.  **Bağımlılıkları Yükleme:**
    ```bash
    dotnet restore
    ```
4.  **Veritabanı Oluşturma:** Paket Yöneticisi Konsolu'nu veya terminali kullanarak migration işlemleri yapın.
    ```bash
    dotnet ef database update
    ```
5.  **Uygulamayı Çalıştırma:**
    ```bash
    dotnet run
    ```

## 🔐 Güvenlik ve Kimlik Doğrulama

Sistem, kullanıcı kayıt ve giriş süreçlerini yönetmek için güvenli bir kimlik doğrulama altyapısı kullanır. Kullanıcı rolleri aracılığıyla hangi personelin hangi verilere erişebileceği yönetici tarafından belirlenir.

---
**Geliştirici:** Cihat Erensoy
