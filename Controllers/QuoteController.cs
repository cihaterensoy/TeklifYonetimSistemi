using Microsoft.AspNetCore.Authorization; // Yetkilendirme kütüphanesi
using Microsoft.AspNetCore.Identity;      // Kullanıcı işlemleri kütüphanesi
using Microsoft.AspNetCore.Mvc;           // MVC yapısı kütüphanesi
using TeklifYonetimSistemi.Contexts;
using TeklifYonetimSistemi.Models;                     // UserViewModel modeli kullanmak için 
using Microsoft.AspNetCore.Mvc.Rendering; // SelectList için gerekli
using Microsoft.EntityFrameworkCore;      // Include için gerekli
using System;
using System.Diagnostics.Metrics;
using System.Data;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;
using TeklifYonetimSistemi.Services;
using TeklifYonetimSistemi.Models.ViewModels;

[Authorize]
public class QuoteController : Controller
{
    private readonly VeriTabaniDB _context;
    private readonly UserManager<KullaniciModel> _userManager;
    private readonly Kur _kur;
    private readonly IEmailService _emailService;
    public QuoteController(VeriTabaniDB context, UserManager<KullaniciModel> userManager, Kur kur,IEmailService emailService)
    {
        _context = context;
        _userManager = userManager;
        _kur = kur;
        _emailService = emailService;
    }
   
    [HttpGet]
    public async Task<IActionResult> Index(int? projectId,QuoteStatus? status)
    {
        //kurları sayfa oluştuğu gibi alıyorum

        //kullanıcının id'sini alıyoruz
        var user = await _userManager.GetUserAsync(User);



        /*sorgu taslağı oluşturuyoruz direkt veritabanına gitmiyor
         * sadece LINQ-to-Entities sorgu nesnesini oluşturur
         * query template / sorgu taslağı
         * var quotesQuery = _context.Quotes Burası EF Core üzerinden veritabanındaki quotes tablosunu temsil eder
            .Include(q => q.Project) her teklifi çekerken buna bağlı olan project bilgiside gelir
            .ThenInclude(p => p.Customer) projenin bağlı olduğu müşteri bilgiside geliyor
            .AsQueryable (); Bu sorguyu dinamik olarak filtreleyebilmen için hazırlıyor.
        */

        var quotesQuery = _context.Quotes
                                    .Include(q => q.Project)
                                    .ThenInclude(p => p.Customer)
                                    .AsQueryable();
        if (user == null) return RedirectToAction("Login", "Account");
        bool IsAdmin = await _userManager.IsInRoleAsync(user, "Admin");
        /*
         * taslakları adminlerden saklamak istiyorum bu yüzden yapıyı değiştiriyorum
        if (!IsAdmin)
        {
            // DÜZELTME: "user.Customer != null" yerine "user.CustomerId != null" yazdık.
            quotesQuery = quotesQuery.Where(q =>
                q.TeklifiOlusturanKullaniciId == user.Id ||
                (user.CustomerId != null && q.Project.CustomerId == user.CustomerId)
            );
        }
        */
        if (IsAdmin)
        {
            quotesQuery = quotesQuery.Where(q => q.TeklifiOlusturanKullaniciId == user.Id || q.Durum != QuoteStatus.Taslak);
            //Yani admin: Kendi taslaklarını ve kendi dışında olan taslak olmayan teklifleri görebilir.
            //Ama başkasının taslak tekliflerini göremez.
        }
        else
        {
            quotesQuery = quotesQuery.Where(q =>
                q.TeklifiOlusturanKullaniciId == user.Id ||
                (user.CustomerId != null && q.Project.CustomerId == user.CustomerId && q.Durum!=QuoteStatus.Taslak)
            );
        }

        //proje bazlı filtreleme
        if (projectId.HasValue)
        {
            quotesQuery = quotesQuery.Where(q => q.ProjectId == projectId.Value);

            var project = await _context.Projects.FindAsync(projectId.Value);
            if (project != null)
            {
                ViewBag.ProjeAdi = project.ProjeAdi;
            }

        }
        if(status.HasValue)
        {
            quotesQuery = quotesQuery.Where(q => q.Durum == status.Value);
            ViewBag.FiltreDurum = status.Value.ToString();
        }
        var quotes = await quotesQuery.OrderByDescending(q => q.TeklifOlusturulmaTarihi).ToListAsync();
        return View(quotes);

    }
    [HttpGet]
    [Authorize(Roles = "Admin,SatisElemani")]
    public async Task<IActionResult> Create(int projectId)
    {
        

        var project = await _context.Projects.Include(p => p.Customer).FirstOrDefaultAsync(p => p.Id == projectId);
        //.Include(p => p.Customer) Anlamı: Projeleri çekerken, her projenin bağlı olduğu Customer(Müşteri) ilişkisini de sorguya dahil et.
        //nlamı: Projects tablosunda, Id alanı, metoda gelen projectId parametresine eşit olan ilk kaydı (ya da hiç yoksa null değerini) getir.
        if (_context.Quotes.Any(x => x.ProjectId == projectId))
        {
            //quotes tablosunda en az bir tane kayıt var mı ve bu kaydın projectId alanı verilen projectId alanı var mı
            //any() old için şartı sağlayan en az bir kayıt varsa true döner
            TempData["Hata"] = "Bu projeye zaten teklif verilmiş";
            return RedirectToAction("Details", "Project", new { id = projectId });
        }

        if (project == null)
        {
            return NotFound();
        }
        var yeniTeklif = new QuoteModel
        {
            ProjectId = projectId,
            Project=project,//ÇÖZÜM: Navigasyon nesnesini modele atar
            TeklifAdi = project.ProjeAdi + " " +"Teklifi",
            DolarKuru= await _kur.GetUsdRateAsync(),
            EuroKuru=await _kur.GetEurRateAsync(),
            TeklifSonTarihi = DateTime.Now.AddDays(15),
            Vade = 30



        };
        return View(yeniTeklif);
    }
    [HttpPost]
    [Authorize(Roles = "Admin,SatisElemani")]
    public async Task<IActionResult> Create(QuoteModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        // 1. Model Binder'ı atlatma: Project ve Customer nesneleri View'a gitmiyor, manuel olarak kaldırıyoruz.
        ModelState.Remove("Project");
        ModelState.Remove("Project.Customer");
        // ÇÖZÜM: Teklif numarasını controller'da biz üretiyoruz, 
        // bu yüzden validation hatası vermesini engelliyoruz.
        ModelState.Remove("TeklifNo");
        //bunu yazmazsak formdan null geliyor bizim aşağıdaki işlemimizi görmeyecek ve direkt bunu göndermeye çalışacak
        /*
        if (string.IsNullOrEmpty(model.TeklifNotu))
        {
            ModelState.Remove("TeklifNotu");
            model.TeklifNotu = "-"; // Boş kalmasın diye tire koyabilirsin
        }
        */
        if (!ModelState.IsValid)
        {
            
            model.Project = await _context.Projects.Include(p => p.Customer).FirstOrDefaultAsync(p => p.Id == model.ProjectId);
            
            return View(model);
        }
        model.TeklifNo = "TR-" + DateTime.Now.ToString("yyMMdd") + "-" + new Random().Next(1000, 9999);
        model.Durum = QuoteStatus.Taslak;
        model.TeklifOlusturulmaTarihi = DateTime.Now;
        model.TeklifiOlusturanKullaniciId = user.Id;
        await _context.Quotes.AddAsync(model);
        await _context.SaveChangesAsync();
        // 2. İŞ AKIŞI DÜZELTMESİ: Kayıt bitti, kullanıcıyı Satır Ekleme (Sepet) ekranına yönlendir!

        // QuoteController.cs'deki POST Create metodu
        return RedirectToAction("Index", "QuoteItem", new { quoteId = model.Id });

    }
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        //var quotes = await _context.Quotes.Include(p => p.Customer).FirstOrDefaultAsync(q => q.Id == projectId);
        // var quotes= await _context.Quotes.Include(q => q.Project).ThenInclude(p => p.Customer).Include(qi=>qi).FirstOrDefaultAsync(qi => qi.Id == id);
        //var item = await _context.QuoteItems.Include(qi => qi.Quote).FirstOrDefaultAsync(qi => qi.Id == itemId);
        var quote = await _context.Quotes
        .Include(q => q.QuoteItems)            // 1️⃣ Quote tablosuna bağlı tüm QuoteItems’leri getir
        .Include(q => q.Project)               // 2️⃣ Quote tablosuna bağlı Project’i getir
        .ThenInclude(p => p.Customer)          // 3️⃣ O Project’in bağlı olduğu Customer’ı da getir
        .FirstOrDefaultAsync(qi => qi.Id == id); // 4️⃣ Quote.Id’si parametre ile eşleşen ilk kaydı al
        //yani includeların hepsi Quotes tablosunu bağlıyor. thenInclude ise bir öncekini bağlıyor

        //Veritabanına git, ID'si benim verdiğim id olan Teklifi bul.
        //Ama bana sadece teklifi getirme; yanına bu teklifin Ürün Kalemlerini, bağlı olduğu Projesini ve o projenin Müşterisini de ekleyerek (doldurarak) getir.
        //Include: Ana tablonun doğrudan bağlı olduğu veriyi çeker. (Oğul/Kız)
        //ThenInclude: Az önce çektiğin verinin içindeki veriyi çeker. (Torun)
        if (quote == null)
        {
            return NotFound();
        }
        //teklife ait mesajları çekiyoruz
        var gecmisMesajlar = await _context.TeklifMesajlar
            .Where(m => m.TeklifId == id)
            .OrderBy(m => m.GonderilmeTarihi)
            .ToListAsync();
        //Mesajı atan kullanıcıların ID'lerini toplama (tekrar edenleri filtreleyerek)
        var gonderenUserIds = gecmisMesajlar.Select(m => m.GonderenUserId) //mesajların gönderen userıd alanını alıyoruz
            .Distinct()//Aynı kullanıcıdan birden fazla mesaj gelmiş olabilir. Distinct() ile tekrar eden ID’leri kaldırıyoruz. Örnek: [3, 5, 7]
            .ToList();//Sonucu List<int> tipinde bir listeye çeviriyoruz.

        var kullanicilar = await _context.Users
            .Where(u => gonderenUserIds.Contains(u.Id)) //gonderenUserIds listesindeki ID’lere sahip kullanıcıları filtreler. Yani sadece mesaj gönderen kullanıcılar alınır.
            .ToDictionaryAsync(u => u.Id, u => u.UserName);
        var viewModel = new TeklifDetayViewModel
        {
            Teklif = quote,                  // Teklif bilgisi
            GecmisMesajlar = gecmisMesajlar, // Mesajlar
            KullaniciAdlari = kullanicilar   // ID → Kullanıcı adı eşlemesi
        };
        return View(viewModel);
    }
    [HttpPost]
    [Authorize(Roles = "Admin,SatisElemani")]
    public async Task<IActionResult> SubmitForApproval(int id)
    {
        //var quote = await _context.Quotes.FirstOrDefaultAsync(q => q.Id == id);
        //Veritabanında ilgili id değerine sahip Teklif kaydını bulur.
        //bu sadece teklifi getirdiği için hata verdirdi içindeki ürünleri alamıyorduk
        var quote = await _context.Quotes.Include(q=>q.QuoteItems).FirstOrDefaultAsync(q => q.Id == id);
        //şu anda içindeki ürünleride ekliyor veritabanı çağrısına
        /*
         * 
         * Include, Entity Framework (EF Core) içinde ilişkili tabloları (navigation properties) sorguya dahil etmek için kullanılan bir metottur.
         * Normalde EF Core, performans için lazy loading yapmaz; yani ilişkili tablolar otomatik olarak gelmez.
         * Include kullanmazsan sadece Quotes tablosundaki verileri alırsın, QuoteItems gelmez (null olur veya boş görünür).
         * Bu kod, veritabanındaki Quotes tablosundan Id değeri verilen kaydı bulup getirir.
         * Ayrıca Include(q => q.QuoteItems) ifadesi sayesinde, bu kayda bağlı olan QuoteItems listesini de aynı anda yükler.
         */
        if (quote == null)
        {
            return NotFound();
        }
        if (quote.Durum != QuoteStatus.Taslak && quote.Durum != QuoteStatus.RevizeGerekiyor)
        {
            TempData["HataMesaji"] = "Teklif sadece taslak veya Revize durumundayken onaya gönderilebilir";
            return RedirectToAction("Index");
        }
        if (quote.QuoteItems.Count == 0)
        {
            TempData["HataMesaji"] = "Teklifi onaya göndermeden önce en az bir ürün/hizmet satırı eklemelisiniz!";
            return RedirectToAction("Index", "QuoteItem", new { quoteId = id });// Ürün ekleme sayfasına yönlendir.
        }
        if (quote.AraToplam <= 0)
        {
            TempData["HataMesaji"] = "Teklifin Ara Toplamı sıfır veya eksi olamaz. Lütfen satırları kontrol edin.";
            return RedirectToAction("Index", "QuoteItem", new { quoteId = id });
        }

        quote.Durum = QuoteStatus.YoneticiOnayBekliyor;
        await _context.SaveChangesAsync();
        TempData["BasariMesaji"] = "Teklif başarıyla yönetici onayına gönderildi.";
        return RedirectToAction("Index");
        //Kayıt tamamlandıktan sonra listeye geri dönülür.
        //Bu metot bittiğinde Tarayıcıya "yeni bir sayfaya git" komutu gönderilir Kullanıcı Index action’ının sayfasına yönlendirilir
    }
    [HttpPost]
    [Authorize(Roles = "Admin")] // Sadece Admin rolündekiler onaylayabilir.
    public async Task<IActionResult> Approve(int id)
    {
        //ya hepsini değiştir ya da hiç değiştirme
        //veritabanında bir işlemi bloğu başlatıyor. bu blok içindeki değişiklikleri ya tamamen uygulaancak ya da hiç uygulanmayacak eğer bir sorun olursa tüm değişiklikler geri alanacak
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var quote = await _context.Quotes
               .Include(q => q.QuoteItems)
               .ThenInclude(qi => qi.Product)
               .FirstOrDefaultAsync(q => q.Id == id);
            //quoteItems teklifin kalemleri
            //product her kalemin detaylı bilgisi
            //verilen id ile eşlesen teklifi getirir
            if (quote == null) return NotFound();


            if (quote.Durum != QuoteStatus.YoneticiOnayBekliyor)
            {
                TempData["HataMesaji"] = "Teklif sadece 'Onay Bekliyor' durumundayken onaylanabilir";
                return RedirectToAction("Index");

            }

            foreach (var item in quote.QuoteItems)
            {
                if (item.Product != null && item.Product.StokTakibiYapilsinMi)
                {
                    if (item.Miktar > item.Product.StokMiktari)
                    {
                        TempData["HataMesaji"] = $"Yetersiz Stok! {item.UrunAdi} için elde {item.Product.StokMiktari} var, teklifte {item.Miktar} isteniyor.";
                        return RedirectToAction("Index");

                    }
                }
            }
            foreach (var item in quote.QuoteItems)
            {
                if (item.Product != null && item.Product.StokTakibiYapilsinMi)
                {
                    item.Product.StokMiktari -= item.Miktar;
                }
            }

            quote.Durum = QuoteStatus.MusteriOnayiBekliyor;
            await _context.SaveChangesAsync();
            TempData["BasariMesaji"] = $"Teklif ({quote.TeklifNo}) başarıyla ONAYLANDI, müşteriye gönderildi ve stoklar rezerve edildi.";
            return RedirectToAction("Index");
        }
        catch(Exception ex)
        {   
            await transaction.RollbackAsync();
            //başlattığımız transasction'ı geri al ve bu işlem sırasında yapılan tüm değişiklikleri iptal et
            TempData["HataMesaji"] = "Bir hata oluştu: " + ex.Message;
            return RedirectToAction("Index");
        }
    }
    [HttpPost]
    [Authorize(Roles = "Admin")] // Sadece Admin rolündekiler onaylayabilir.
    public async Task<IActionResult> Revize(int id)

    {
        var quote = await _context.Quotes.FirstOrDefaultAsync(q => q.Id == id);
        if (quote == null) return NotFound();

        if (quote.Durum != QuoteStatus.YoneticiOnayBekliyor)
        {
            TempData["HataMesaji"] = "Teklif sadece 'Onay Bekliyor' durumundayken onaylanabilir";
            return RedirectToAction("Index");

        }
        quote.Durum = QuoteStatus.RevizeGerekiyor;
        await _context.SaveChangesAsync();
        TempData["HataMesaji"] = "Teklif revize edilmesi için personele geri gönderildi.";
        return RedirectToAction("Index");
    }
    [HttpPost]
    [Authorize(Roles = "Admin,SatisElemani")]
    public async Task<IActionResult> RevizeTamamlandi(QuoteModel model)
    {
        //model binder'dan kaldırma bu sayede customer nesneleri view'a gitmiyor
        ModelState.Remove("Project");
        ModelState.Remove("Project.Customer");

        if (!ModelState.IsValid)
        {
            TempData["HataMesaji"] = "Litfen teklif başlığını ve geçerlilik tarihi alanlarını kontrol edin.";
            return RedirectToAction("Details", new { id = model.Id });
        }

        var existingQuote = await _context.Quotes.FirstOrDefaultAsync(q => q.Id == model.Id);
        if (existingQuote == null) return NotFound();

        existingQuote.TeklifAdi = model.TeklifAdi;
        existingQuote.TeklifSonTarihi = model.TeklifSonTarihi;
        existingQuote.Vade = model.Vade;
        existingQuote.TeklifNotu = model.TeklifNotu;

        existingQuote.Durum = QuoteStatus.YoneticiOnayBekliyor;

        existingQuote.ReviseReasonNote = null; //düzenlemeyi yaptığımız için temizliyoruz içeriğini
        _context.Quotes.Update(existingQuote);
        await _context.SaveChangesAsync();
        TempData["BasariMesaji"] = $"Teklif ({existingQuote.TeklifNo}) revize edilerek tekrar yönetici onayına gönderildi.";

        // Teklif öğelerinin düzenlendiği sayfaya yönlendiriyoruz (QuoteItem/Index)
        return RedirectToAction("Index", "QuoteItem", new { quoteId = model.Id });
    }
    [HttpPost]
    [Authorize(Roles = "Admin")] // Sadece Admin rolündekiler reddeebilir.
    public async Task<IActionResult> YoneticiReject(int id)
    {
        var quote = await _context.Quotes.FirstOrDefaultAsync(q => q.Id == id);
        if (quote == null) return NotFound();
        if (quote.Durum != QuoteStatus.YoneticiOnayBekliyor)
        {
            TempData["HataMesaji"] = "Teklif sadece 'Onay Bekliyor' durumundayken reddedebilir";
            return RedirectToAction("Index");

        }
        quote.Durum = QuoteStatus.Reddedildi;
        await _context.SaveChangesAsync();
        TempData["BasariMesaji"] = $"Teklif ({quote.TeklifNo}) yönetici tarafından REDDEDİLDİ.";
        return RedirectToAction("Index");
    }
    [HttpPost]
    [Authorize(Roles = "Admin,FirmaKullanicisi")]
    public async Task<IActionResult> MusteriOnay(int id, [FromServices] QuotePdfGenerator pdfGenerator)
    {
        //bu sınıf çağrıdılığında [fromservices] yüzünden, sistem otomatik olarak servis container'dan QuotePdfGenerator'ı alır
        var quote = await _context.Quotes
                                    .Include(q => q.Project)
                                    .ThenInclude(p => p.Customer) // Project içindeki Customer'ı da yükle
                                    .Include(q => q.QuoteItems) // Eğer PDF’de ürünler gösterilecekse
                                    .FirstOrDefaultAsync(q => q.Id == id);
        if (quote == null) return NotFound();

        if (quote.Durum !=QuoteStatus.MusteriOnayiBekliyor)
        {
            TempData["HataMesaji"] = "Yönetici Onaylamadan Sizin Bir Yetkiniz Bulunmamaktadır";
            return RedirectToAction("Index");
        }
        quote.Durum = QuoteStatus.Onaylandi;
        await _context.SaveChangesAsync();
        //pdf oluşturma kod
        var pdfBytes = pdfGenerator.GenerateQuotePdf(quote);
        //
        //mail gönderme//
        /*
         //sürekli mail atmasın diye kapattım
        await _emailService.SendEmailWithAttachmentAsync(
            toEmail: quote.Project.Customer.Email,
            subject: $"{quote.TeklifAdi} Teklif Onayı",
            body: "Kabul ettiğiniz teklifin pdf'i ekte bulunmaktadır",
            attachmentBytes: pdfBytes,
            attachmentName: $"Teklif_{quote.TeklifNo}.pdf"
        );
        */

        TempData["BasariMesaji"] = $"Teklif ({quote.TeklifNo}) Müşteri tarafından Kabul Edildi.";
        //return RedirectToAction("Index"); pdfi indirecek bü yüzden yorum yaptım
        return File(pdfBytes, "application/pdf", $"Teklif_{quote.TeklifNo}.pdf");
    }
    [HttpPost]
    [Authorize(Roles = "Admin,FirmaKullanicisi")]// Sadece Admin rolündekiler reddeebilir.
    public async Task<IActionResult> MusteriReject(int id)
    {
        //Teklif ve ona bağlı ürünleri veritabanından çekiyoruz
        var quote = await _context.Quotes.Include(q=>q.QuoteItems).ThenInclude(qi=>qi.Product).FirstOrDefaultAsync(q => q.Id == id);
        if (quote == null) return NotFound();
        if (quote.Durum != QuoteStatus.MusteriOnayiBekliyor)
        {
            TempData["HataMesaji"] = "Teklif sadece 'Onay Bekliyor' durumundayken reddedebilir";
            //
            //tempdata ve viewbag arasındaki fark nedir veya fark var mı bunu araştır
            //
            return RedirectToAction("Index");

        }
        //transaction kullanarak işlemleri bir bütün olarak ele almayı sağlıyoruz
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            foreach (var item in quote.QuoteItems)
            {
                if (item.Product != null && item.Product.StokTakibiYapilsinMi)
                {
                    item.Product.StokMiktari += item.Miktar;
                }
            }
            quote.Durum = QuoteStatus.Reddedildi;
            await _context.SaveChangesAsync();
            TempData["BasariMesaji"] = $"Teklif ({quote.TeklifNo}) müşteri tarafından REDDEDİLDİ.";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            //bir hata oldu değişiklikleri geri al
            await transaction.RollbackAsync();
            TempData["HataMesaji"] = "İşlem sırasında bir hata oluştu ve tüm değişiklikler geri alındı. Detay: " + ex.Message;
            return RedirectToAction("Index"); // Ana sayfaya dön
        }
    }

    //buradaki delete sadece hatalı girilen verileri silmek için kullanılıyor. uygulamanın son halinde bunu kaldırsan daha iyi olur

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        //var quote = await _context.Quotes.FirstOrDefaultAsync(q => q.Id == id);
        var quote = await _context.Quotes
                              .Include(q => q.QuoteItems)
                              .FirstOrDefaultAsync(q => q.Id == id);
        /*
        if (quote != null)
        {
            _context.Quotes.Remove(quote);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("Index");
        */
        if (quote != null)
        {
            // 2. Önce Teklifin içindeki ürün satırlarını siliyoruz (Garanti temizlik)
            if (quote.QuoteItems != null && quote.QuoteItems.Any())
            {
                _context.QuoteItems.RemoveRange(quote.QuoteItems);
            }

            // 3. Artık içi boşalan Teklifi silebiliriz
            _context.Quotes.Remove(quote);

            // 4. Değişiklikleri veritabanına işle (Async kullanıyoruz)
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Index");
    }
    


}
