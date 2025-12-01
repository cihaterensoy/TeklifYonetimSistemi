using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TeklifYonetimSistemi.Contexts;
using TeklifYonetimSistemi.Models;
using Microsoft.AspNetCore.Identity;
using TeklifYonetimSistemi.Models.ViewModels;

[Authorize (Roles="Admin,SatisElemani")]
public class KullaniciEkleController:Controller
{
    private readonly UserManager<KullaniciModel> _userManager;
    //Usermanager sınıfını benim KullaniciModel.cs dosyamdaki özelliklerle birlikte kullan
    //işlemleri ypaan makine benim özel şablonumla çalışıyor yani
    private readonly RoleManager<IdentityRole<int>> _roleManager;
    /*private => sadece bu controller içinde erişilebilir, dışarıdan erişilmez.
    readonly => oluşturma (constructor) sırasında atanır, daha sonra değiştirilmez. Bu iyi bir pratiktir; böylece yanlışlıkla başka bir yerde bu alanları değiştirmezsin.*/
    //CONSTRUCTOR PARAMETRESİNDEN gelen bu örnekleri bu alanlara atanır

    public readonly VeriTabaniDB _context;
    //firma kullanıcısını ekleyebilmek için firmaları listeleyeceğiz
    public KullaniciEkleController(UserManager<KullaniciModel> userManager, RoleManager<IdentityRole<int>> roleManager, VeriTabaniDB context)
    {
        //adminKullanciController sınıfından bir nesne oluşturulurken çalışan özel bir metoddur
        //KullaniciModel> userManager ASP.Net Identity'nin kullanıcı işlemlerini yapan sınıfının bir örneği. kullanıcı oluşturma şifre işlemleri, rol atama gibi metotları içerir
        //RoleManager<IdentityRole> roleManager -> rollerle ilgili işlemleri yapan bir sınıf. veritabanındaki rolleri okumak, yeni rol oluşturmak için kullanılır
        //ASP.Net core, bir DI container kullanır
        //controller yarratacağı zaman framework constructor'a bakar, ihtiyac duyulan parametrelerin tipine göre uygun nesneleri otomatik algılar
        //framework her HTTP isteği için controller örneğini oluştururken UserManager ve RoleManager örneklerini verir.
        //Bu örneklerin nasıl oluşturulacağı Program.cs / Startup.cs içinde services.AddIdentity<...>() gibi kayıtlarla belirlenir.
        _userManager = userManager;
        //controller içinde uSERmANAGER'I KULLANABİLMEK İÇİN ONU BİR ALAN OLARAK SAKLAMAMIZ GEREKİYOR
        
        _roleManager = roleManager;
        _context = context;
    }
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var kullanicilar = await _context.Users.Include(u => u.Customer).ToListAsync();
        return View(kullanicilar);
    }
    [HttpGet]
    public async Task<IActionResult> Ekle() //asenkron işlem yapmak için kullandık
    {
        var roller = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
        // _roleManager.Roles -> controller içinde Dependency Injection almış RoleManager<IdentityRole> örneğidir
        //.roles -> IQueryable<IdentityRole> döner - yani role kayıtlarının sorgulanabileceği bir veri kaynağı
        //Select(r => r.Name) select ile IdentitiyRole nesnesniin tamamını değil sadece name alanını alıyoruz
        //her bir role için sadece name alanını seçer
        ViewBag.Roller = new SelectList(roller);

        //kullanıcı firma çalışanıysa eklenecek firmaları
        //EKLEME 2: Firmaları Dropdown için hazırla
        // Sadece ID ve İsimlerini çekiyoruz, tüm tabloyu çekmeye gerek yok (Performans).
        var firmalar = await _context.Customers.Select(c => new { c.Id, c.FirmaUnvani }).ToListAsync();
        ViewBag.Firmalar = new SelectList(firmalar, "Id", "FirmaUnvani");
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> Ekle(KullaniciEkleViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new KullaniciModel
            {
                UserName = model.Email,
                Email = model.Email,
                Isim = model.Isim,
                Soyisim = model.Soyisim,
                EmailConfirmed = true,

                // EKLEME 3: Formdan gelen Firma ID'sini kullanıcıya yapıştırıyoruz.
                // Eğer formda seçilmediyse burası 'null' gelir, sorun olmaz.
                CustomerId = model.CustomerId
            };

            // DÜZELTME 1: model.Sifre tamamlandı.
            var result = await _userManager.CreateAsync(user, model.Sifre);

            // DÜZELTME 2: İşlem başarılı mı kontrolü eklendi.
            if (result.Succeeded)
            {
                // Rol seçildiyse ata
                if (!string.IsNullOrEmpty(model.Rol))
                {
                    await _userManager.AddToRoleAsync(user, model.Rol);
                }

                // Başarılıysa Anasayfaya veya Listeye yönlendir
                return RedirectToAction("Index", "Home");
            }

            // Hata varsa (Örn: Şifre çok basit) hataları ekrana bas
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }

        // DÜZELTME 3: Hata durumunda sayfa tekrar yüklenirse Dropdown boş gelmesin.
        // (Bunu yapmazsan hata aldığında dropdown kaybolur)
        var roller = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
        ViewBag.Roller = new SelectList(roller);

        // Hata olursa (Örn: Şifre kısa), Dropdownlar boşalmasın diye tekrar dolduruyoruz
        var firmalarListesi = await _context.Customers.Select(c => new { c.Id, c.FirmaUnvani }).ToListAsync();
        ViewBag.Firmalar = new SelectList(firmalarListesi, "Id", "FirmaUnvani");

        // Modeli geri gönder ki kullanıcının yazdıkları silinmesin
        return View(model);
    }
    
}
