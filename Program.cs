using TeklifYonetimSistemi.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TeklifYonetimSistemi.Models;
using TeklifYonetimSistemi.Data; // UserRoles sınıfı için gerekli
using TeklifYonetimSistemi.Services;   // IEmailService ve SmtpEmailService için
using Microsoft.Extensions.DependencyInjection;  // AddScoped için (Genelde zaten ekli)
using static System.Net.WebRequestMethods;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
// Servisleri eklediğimiz alan
//builder.Services.AddScoped<IEmailService, SmtpEmailService>(); //mail gönderme kısmının yapılması için eklendi

// DbContext
builder.Services.AddDbContext<VeriTabaniDB>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity Ayarları (int uyumlu)
builder.Services.AddIdentity<KullaniciModel, IdentityRole<int>>(opt =>
{
    opt.User.RequireUniqueEmail = true;
    opt.Password.RequiredLength = 3; // Test için düşürdüm, istersen artır
    opt.Password.RequireNonAlphanumeric = false;
    opt.Password.RequireLowercase = false;
    opt.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<VeriTabaniDB>()
.AddDefaultTokenProviders();
builder.Services.AddScoped<Kur>();
builder.Services.AddTransient<QuotePdfGenerator>();
//Transient → her kullanımda yeni bir nesne oluşturur.
//Alternatifler: Scoped(HTTP isteği başına bir nesne), Singleton(tüm uygulama boyunca tek nesne).
//builder.Services.AddScoped<MicrosoftEmailService>();
builder.Services.AddScoped<IEmailService, GmailEmailService>();



var app = builder.Build();


// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication(); // Önce kimlik kontrolü
app.UseAuthorization();  // Sonra yetki kontrolü

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Login}/{id?}"); // Login controllerın olduğundan emin ol


app.MapRazorPages();

// --- SEEDING (VERİTABANI DOLDURMA) İŞLEMİ ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
    var userManager = services.GetRequiredService<UserManager<KullaniciModel>>();

    // 1. ROLLERİ OLUŞTUR (Senin 3 rolün)
    // UserRoles sınıfını kullanıyoruz ki hata olmasın
    string[] roles = { UserRoles.Admin, UserRoles.SatisElemani, UserRoles.FirmaKullanicisi };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole<int> { Name = role });
        }
    }

    // 2. ADMIN KULLANICISI
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

    // 3. SATIŞ ELEMANI (Senin kodundaki 'User' yerine)
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

    // 4. FİRMA KULLANICISI (Bu eksikti, ekledik)
    string firmaEmail = "musteri@firma.com";
    if (await userManager.FindByEmailAsync(firmaEmail) == null)
    {
        var firmaUser = new KullaniciModel
        {
            UserName = firmaEmail,
            Email = firmaEmail,
            Isim = "Ayşe",
            Soyisim = "Yılmaz",
            EmailConfirmed = true,
            // Dikkat: Burada CustomerId vermedik çünkü henüz Firma tablosunda veri yok.
            // İleride buraya dummy (sahte) bir CustomerId verebilirsin.
        };
        var result = await userManager.CreateAsync(firmaUser, "Firma123!");
        if (result.Succeeded) await userManager.AddToRoleAsync(firmaUser, UserRoles.FirmaKullanicisi);
    }
}

app.Run();