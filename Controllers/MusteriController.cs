using Microsoft.AspNetCore.Authorization; // Yetkilendirme kütüphanesi
using Microsoft.AspNetCore.Identity;      // Kullanıcı işlemleri kütüphanesi
using Microsoft.AspNetCore.Mvc;           // MVC yapısı kütüphanesi
using Microsoft.EntityFrameworkCore;
using TeklifYonetimSistemi.Contexts;
using TeklifYonetimSistemi.Models;                     // UserViewModel modeli kullanmak için
using System.Security.Claims; // Claims yöntemi için
using TeklifYonetimSistemi.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using TeklifYonetimSistemi.Data;
using TeklifYonetimSistemi.Services;

[Authorize(Roles = "Admin,SatisElemani")]
public class MusteriController : Controller
{
    private readonly VeriTabaniDB _context;
    private readonly UserManager<KullaniciModel> _userManager;
    private readonly IELogoService _eLogoService;
    public MusteriController(VeriTabaniDB context,UserManager<KullaniciModel> userManager,IELogoService eLogoService)
    {
        _context = context;
        _userManager = userManager;
        //_context veri tabanı bağlantısı
        //VeritabaniDB DbContext sınıfıdır ve içinde DbSet<Customer> bulunur
        //Bu sayede müşteri kayıtlarını veritabanına ekleyebilir listeleyebilir ve güncelleyebiliriz
        _eLogoService = eLogoService;

    }
    // LİSTELEME
    public async Task<IActionResult> ana() // async yaptık
    {
        // ToList yerine ToListAsync kullandık (Performans için)
        //var musteriler = await _context.Customers.Include(x=>x.Kullanici).ToListAsync();
        //kullanıcının id'sini alıyoruz
        var user = await _userManager.GetUserAsync(User);
        var sorgu = _context.Customers.Include(x => x.Kullanici).AsQueryable();
        if (!User.IsInRole("Admin"))
        {
            sorgu = sorgu.Where(c => c.KullaniciId == user.Id);
        }
        var musteriler = await sorgu.ToListAsync();
        return View(musteriler);
        //context.Customers.ToList(); veritabanındaki tüm müşterileri alır ve liste olarak customers değişkenini atar
        //return view(customers) -> listeyi index.cshtml view'ına gönderir
    }
    /*
    //Detay Görüntüleme
    public IActionResult Details(int id)
    {
        var musteri = _context.Customers.FirstOrDefault(m => m.Id == id);
        //_context.Musteriler.FirstOrDefault(m => m.Id == id) → Veritabanında bu id’ye sahip müşteriyi bulur.
        if(musteri==null)
        {
            return NotFound();
            //if (musteri == null) return NotFound(); → Eğer müşteri bulunamazsa HTTP 404 döndürür.
        }
        return View(musteri);
        //return View(musteri) → Bulunan müşteri bilgilerini Details.cshtml view’ine gönderir.
    }
    */
    //bir firmaya bağlı birden fazla firma yetkilisi eklemek için değişiklik yapıyorum
    public async Task<IActionResult> Details(int id)
    {
        var musteri = await _context.Customers
            .Include(c => c.FirmaYetkilileri)//İlişkili kullanıcıları da yüklüyoruz.
            .FirstOrDefaultAsync(m => m.Id == id);
        if (musteri == null)
        {
            return NotFound();
            //if (musteri == null) return NotFound(); → Eğer müşteri bulunamazsa HTTP 404 döndürür.
        }
        return View(musteri);
    }
    //formu göndermek için kullanılır
    //Kullanıcı bir müşteri eklemek istediğinde form acılır
    /*
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }
    
    [HttpPost]
    public async Task<IActionResult> Create(CustomerModel model)
    {
        // ModelState kontrolünden ÖNCE, kullanıcının girmesi gerekmeyen (sistemin atayacağı)
        // alanları 'Model hataları' listesinden siliyoruz.
        // Yoksa "Kullanici alanı boş" diye hata verir.
        ModelState.Remove("Kullanici");
        ModelState.Remove("KullaniciId");
        if (!ModelState.IsValid)
        {
            return View(model);
        };
        model.OlusturulmaTarihi = DateTime.Now;
        var user = await _userManager.GetUserAsync(User); // Giriş yapan kullanıcıyı bul
        model.KullaniciId = user.Id; // Onun ID'sini modele ata
        await _context.Customers.AddAsync(model);
        await _context.SaveChangesAsync();
        return RedirectToAction("ana");
    }
    */
    [HttpGet]
    public async Task <IActionResult> Create()
    {
        var tumYetkililer = await _userManager.GetUsersInRoleAsync(UserRoles.FirmaKullanicisi);//firma kullanıcısını user tipi olan çekiyor
        //sadece firması olmayan kullanıcıları gösteriyor
        var bostakiYetkililer = tumYetkililer.Where(u => u.CustomerId == null).Select(u => new
        {
            Id = u.Id,
            Gorunum = $"{u.Isim} {u.Soyisim} ({u.Email})"
        }).ToList();
        ViewBag.BostakiYetkililer = new SelectList(bostakiYetkililer, "Id", "Gorunum");
        return View(new MusteriKayitViewModel());
    }
    [HttpPost]
    public async Task<IActionResult> Create(MusteriKayitViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        // ENTEGRASYON BAŞLANGICI 
        bool eFaturaMukellefi = false;
        // Eğer vergi no girildiyse servise sor
        if (!string.IsNullOrEmpty(model.VergiNo) && model.VergiNo.Length >= 10)
        {
            eFaturaMukellefi = await _eLogoService.MukellefKontrolAsync(model.VergiNo);
        }
        var yeniFirma = new CustomerModel
        {
            FirmaUnvani = model.FirmaUnvani,
            VergiNo = model.VergiNo,
            VergiDairesi = model.VergiDairesi,
            EFaturaMukellefiMi = eFaturaMukellefi, // Sonucu kaydet
            Email =model.Email,
            Telefon=model.Telefon,
            Il=model.Il,
            Ilce=model.Ilce,
            Adres = model.Adres,
            Fax = model.Fax ?? "-",
            FirmaDetay = model.FirmaDetay ?? "-",
            OlusturulmaTarihi = DateTime.Now,
            MusteriAktifMi=true,
            KullaniciId=int.Parse(_userManager.GetUserId(User))
        };

        _context.Customers.Add(yeniFirma);
        await _context.SaveChangesAsync(); //id oluştu

        if (model.SecilenVarolanKullaniciId!=null)
        {
            var mevcutUser = await _userManager.FindByIdAsync(model.SecilenVarolanKullaniciId.ToString());
            if (mevcutUser != null)
            {
                mevcutUser.CustomerId = yeniFirma.Id;
                await _userManager.UpdateAsync(mevcutUser);
            }
        }
        else if (!string.IsNullOrEmpty(model.YetkiliEmail) && !string.IsNullOrEmpty(model.YetkiliSifre))
        {
            var yeniYetkili = new KullaniciModel
            {
                UserName = model.YetkiliEmail,
                Email = model.YetkiliEmail,
                Isim = model.YetkiliIsim ?? "Yetkili",
                Soyisim=model.YetkiliSoyisim ?? "Personel",
                EmailConfirmed=true,
                CustomerId=yeniFirma.Id//FİRMAYI YETKİLİYE BAĞLADIK

            };
            var result = await _userManager.CreateAsync(yeniYetkili, model.YetkiliSifre);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(yeniYetkili, UserRoles.FirmaKullanicisi);
            }

        }
        return RedirectToAction("ana");
    }



    //!ModelState.IsValid form eksik değilse devam etmez
    //await _context.Customers.AddAsync(model); müşteri oluşturma tarihini otomatik atar
    //await _context.Customers.AddAsync(model); müşteriyi veritabanına ekler
    //await _context.SaveChangesAsync(); ekleme işlemi kaydedilir
    //ekeleme başarılıysa müşteri listesine yönlendirir
    //Düzenleme
    [HttpGet]
    // Metot imzası async ve Task<IActionResult> olarak değiştirildi
    public async Task<IActionResult> Edit(int id)
    {
        // Asenkron metot kullanıldı
        var musteri = await _context.Customers.FirstOrDefaultAsync(m => m.Id == id);

        if (musteri == null)
        {
            return NotFound();
        }
        return View(musteri);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]//CSRF saldırılarına karşı koruma sağlar. Formda _antiForgeryToken gizli alanının olup olmadığını kontrol eder.
    public async Task<IActionResult> Edit(CustomerModel model)
    {
        if (model.Id == 0)
        {
            return BadRequest("Müşteri kimliği (Id) eksik.");
            //fromda gönderilen modelin id değeri 0 ise işlem durduruluyor
        }

        // ModelState.Remove işlemleri burada da faydalı olacaktır.
        ModelState.Remove("Kullanici");
        ModelState.Remove("KullaniciId");
        ModelState.Remove("OlusturulmaTarihi");
        ModelState.Remove("FirmaYetkilileri");
        ModelState.Remove("SilindiMi");
        //modelstate,form verisinin doğrulama durumunu tutar
        //bazı alanlar formdan gelmiyor veya güncellenmesi gerekmiyor, bu alanları remove ile modelstate'den cıkartmamız modelstate.IsValid kontrolünde bu alanların eksikliğniin hata yaratmasını engeller

        if (!ModelState.IsValid)
        {
            return View(model);
            //modelstate doğrulaması başarısızsa form tekrar  gösterilir ve kullanıcı hatalrı görebilir
        }

        try
        {
            // 1. EF Core'a, formdan gelen bu nesnenin (model) zaten var olduğunu söyle.
            // EF Core, bu nesneyi "Modified" (Değiştirilmiş) durumuna getirir.
            _context.Customers.Attach(model);
            //attach metodu ef core'a bu nesnenin zaten var olduğunu güncelleneceğini söyler
            //burada henüz değişiklik ypaılmıyor, sadece izlenen nesne olarak işaretleniyor

            // 2. KRİTİK ADIM: Sadece formdan gelen alanları güncellenmek üzere işaretle.
            // Bu, KullaniciId'nin ve OlusturulmaTarihi'nin güncellenmesini engeller ve orijinal değerini korumasını sağlar.

            // Kullanmadığınız, ancak veritabanının NOT NULL beklediği alanları
            // hariç tutmak için isChecked: false kullanıyoruz.

            _context.Entry(model).Property(e => e.FirmaUnvani).IsModified = true;
            _context.Entry(model).Property(e => e.Email).IsModified = true;
            _context.Entry(model).Property(e => e.Telefon).IsModified = true;
            _context.Entry(model).Property(e => e.Il).IsModified = true;
            _context.Entry(model).Property(e => e.Ilce).IsModified = true;
            _context.Entry(model).Property(e => e.Adres).IsModified = true;
            _context.Entry(model).Property(e => e.Fax).IsModified = true;
            _context.Entry(model).Property(e => e.FirmaDetay).IsModified = true;
            _context.Entry(model).Property(e => e.MusteriAktifMi).IsModified = true;
            //true olanlar update edilecek
            //Bu şekilde, kullanıcı formdan sadece bazı alanları göndermiş olsa bile, yalnızca bu alanlar veritabanında değişir

            // KORUNMASI GEREKEN ALANLARI GÜNCELLEMEYE DAHİL ETME:
            _context.Entry(model).Property(e => e.KullaniciId).IsModified = false;
            //Explicit Update'te, KullaniciId'yi manuel olarak IsModified = false yaparak, EF Core'un bu sıfır değeri veritabanına göndermesini engelleriz.
            _context.Entry(model).Property(e => e.OlusturulmaTarihi).IsModified = false;
            //KullaniciId ve OlusturulmaTarihi güncellenmez, eski değerleri korunur.

            // Not: FirmaYetkilileri (ICollection) ve Kullanici (Navigation Property) Attach ile otomatik olarak yönetilir.

            // 3. Değişiklikleri kaydet. Sadece IsModified = true olan alanlar güncellenir.
            await _context.SaveChangesAsync();

            return RedirectToAction("ana");
        }
        catch (Exception ex)
        {
            // Hata yakalama: Hatanın tam nedeni hala iç hatada (InnerException) gizli.
            // Bu mesajı alıyorsanız, iç hatayı incelemek zorunludur.
            var innerExMessage = ex.InnerException?.Message ?? ex.Message;

            ModelState.AddModelError("", "Güncelleme hatası. Lütfen teknik destek ile iletişime geçin. Detay: " + innerExMessage);

            return View(model);
        }
    }
    // bu delete sınıfını düzelt, soft delete atacağız
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        // 1. Müşteriyi (Firmayı) ve ona bağlı Kullanıcıları (FirmaYetkilileri) getiriyoruz.
        // Include kullanmazsak bağlı kullanıcılar gelmez, silmeye çalışırken yine hata alırız.
        var musteri = await _context.Customers
                                    .Include(c => c.FirmaYetkilileri)
                                    .FirstOrDefaultAsync(m => m.Id == id);

        if (musteri != null)
        {
            // 2. Önce bu firmaya bağlı tüm kullanıcıları siliyoruz (Hard Delete)
            if (musteri.FirmaYetkilileri != null && musteri.FirmaYetkilileri.Any())
            {
                // _context.Users tablosundan bu kişileri kaldırıyoruz.
                _context.Users.RemoveRange(musteri.FirmaYetkilileri);
            }

            // 3. Kullanıcılar gittiğine göre artık firmayı da silebiliriz.
            _context.Customers.Remove(musteri);

            // 4. Tüm bu işlemleri tek seferde veritabanına uyguluyoruz.
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("ana");
    }

}
