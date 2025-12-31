using TeklifYonetimSistemi.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TeklifYonetimSistemi.Models;
using TeklifYonetimSistemi.Data; // UserRoles sınıfı için
using TeklifYonetimSistemi.Services;   // IEmailService ve servisler için
using Microsoft.AspNetCore.Localization; // Dil ayarları için
using System.Globalization; // Kültür ayarları için
using TeklifYonetimSistemi.Hubs;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// MVC ve Razor Pages Servisleri
//builder.Services.AddControllersWithViews();

// MVC ve Razor Pages Servisleri
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        // JSON Serileştiriciye döngüsel referansları görmezden gel talimatı verilir.
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });


builder.Services.AddRazorPages();

// Veritabanı Bağlantısı (DbContext)
builder.Services.AddDbContext<VeriTabaniDB>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity (Kullanıcı Yönetimi) Ayarları
builder.Services.AddIdentity<KullaniciModel, IdentityRole<int>>(opt =>
{
    opt.User.RequireUniqueEmail = true;
    opt.Password.RequiredLength = 3; // Test için düşük, canlıda artırılmalı
    opt.Password.RequireNonAlphanumeric = false;
    opt.Password.RequireLowercase = false;
    opt.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<VeriTabaniDB>()
.AddDefaultTokenProviders();

// Senin Özel Servislerin
builder.Services.AddScoped<Kur>();
builder.Services.AddTransient<QuotePdfGenerator>();
builder.Services.AddScoped<IEmailService, GmailEmailService>(); // Veya SmtpEmailService

//e-logo bağlantısı için
//builder.Services.AddHttpClient < TeklifYonetimSistemi.Services.ELogoService>();
//builder.Services asp.net core'da servisleri kaydettiğimiz yer
//AddHttpClient -> ELogoService her ihtiyacı olduğunda sp.net core'a httpClient örneği istiyor
// Bu satır sisteme şunu der: "Biri senden 'IELogoService' isterse, ona 'ELogoService' ver."
builder.Services.AddScoped<TeklifYonetimSistemi.Services.IELogoService, TeklifYonetimSistemi.Services.ELogoService>();

// 1. SignalR Servisini Ekle
builder.Services.AddSignalR();
var app = builder.Build();


// --- KÜLTÜR VE DİL AYARI (₺ Simgesi ve Tarih Formatı İçin Kritik) ---
var trCulture = new CultureInfo("tr-TR");
// Tarih formatını sabitleyelim (Gün.Ay.Yıl)
trCulture.DateTimeFormat.ShortDatePattern = "dd.MM.yyyy";
trCulture.DateTimeFormat.LongTimePattern = "HH:mm:ss";
// Para birimi sembolünü garantiye alalım
trCulture.NumberFormat.CurrencySymbol = "₺";

var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(trCulture),
    SupportedCultures = new List<CultureInfo> { trCulture },
    SupportedUICultures = new List<CultureInfo> { trCulture }
};

// BU SATIR EN BAŞLARDA OLMALI (Authentication'dan önce)
app.UseRequestLocalization(localizationOptions);

// Hata Yönetimi
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

// Statik Dosyalar ve Yönlendirme
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Kimlik Doğrulama ve Yetkilendirme (Sırası Önemli!)
app.UseAuthentication();
app.UseAuthorization();

// Rotalar (Routes)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Login}/{id?}");

app.MapRazorPages();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
        var userManager = services.GetRequiredService<UserManager<KullaniciModel>>();

        // A. Rolleri Oluştur
        string[] roles = { UserRoles.Admin, UserRoles.SatisElemani, UserRoles.FirmaKullanicisi };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<int> { Name = role });
            }
        }

        // B. Admin Kullanıcısı
        string adminEmail = "admin@example.com";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var adminUser = new KullaniciModel
            {
                UserName = adminEmail,
                Email = adminEmail,
                Isim = "Sistem",
                Soyisim = "Admini",
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded) await userManager.AddToRoleAsync(adminUser, UserRoles.Admin);
        }

        // C. Satış Elemanı
        string satisEmail = "satis@example.com";
        if (await userManager.FindByEmailAsync(satisEmail) == null)
        {
            var satisUser = new KullaniciModel
            {
                UserName = satisEmail,
                Email = satisEmail,
                Isim = "Ahmet",
                Soyisim = "Satışçı",
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(satisUser, "Satis123!");
            if (result.Succeeded) await userManager.AddToRoleAsync(satisUser, UserRoles.SatisElemani);
        }

        // D. Firma Kullanıcısı
        string firmaEmail = "musteri@firma.com";
        if (await userManager.FindByEmailAsync(firmaEmail) == null)
        {
            var firmaUser = new KullaniciModel
            {
                UserName = firmaEmail,
                Email = firmaEmail,
                Isim = "Ayşe",
                Soyisim = "Yılmaz",
                EmailConfirmed = true
                // Not: CustomerId sonradan admin panelinden atanabilir.
            };
            var result = await userManager.CreateAsync(firmaUser, "Firma123!");
            if (result.Succeeded) await userManager.AddToRoleAsync(firmaUser, UserRoles.FirmaKullanicisi);
        }
    }
    catch (Exception ex)
    {
        // Seeding sırasında hata olursa konsola yaz (Loglama)
        Console.WriteLine("Veritabanı oluşturulurken bir hata oluştu: " + ex.Message);
    }
}

// 2. SignalR Hub Endpoint'ini Tanımla
app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<ChatHub>("/chatHub"); // '/chatHub' Adresinde yayın yapacak
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});

app.Run();