
/*
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Bunu eklemeyi unutma
using TeklifYonetimSistemi.Contexts; // Context'in olduğu yeri ekle
using TeklifYonetimSistemi.Services;

[Authorize]
public class HomeController : Controller
{
    // 1. Veritabanı bağlantısını buraya da tanımlıyoruz (Dependency Injection)
    private readonly VeriTabaniDB _context;

    public HomeController(VeriTabaniDB context)
    {
        _context = context;
        //Sistemin (Program.cs'in) bana gönderdiği o hazır aracı al, benim sınıfımın içindeki _context değişkenine koy. Ben kodun geri kalanında bu değişkeni kullanacağım
    }

    public async Task<IActionResult> Index()
    {
        // 2. Sayıyı burada da hesaplıyoruz
        ViewBag.ToplamUrun = await _context.Products.CountAsync();
        //var urunSayisi = await _context.Products.CountAsync(); //veritabanındaki sadece sayıyı al
        //ViewBag.ToplamUrun = urunSayisi; //ürün sayımızı ekrana iletmek için değişkene atadık

        // İstersen başka istatistikler de ekle
        // ViewBag.ToplamMusteri = await _context.Customers.CountAsync();
        ViewBag.ToplamMusteri = await _context.Customers.CountAsync();
        ViewBag.ToplamProje = await _context.Projects.CountAsync();
        // ÖRNEK 1: Kaç farklı KATEGORİ var?
        // Select: Sadece Kategori sütununu alır.
        // Distinct: Tekrarlayanları eler (Örn: 10 tane "Donanım" varsa 1 sayar).
        /*
        ViewBag.FarkliKategoriSayisi = await _context.Products
                                             .Select(x => x.Kategori)
                                             .Distinct()
                                             .CountAsync();
        
        
        return View();
    }
    public async Task<IActionResult> LogoTest([FromServices] ELogoService logoServis)
    {
        //controller metodu web uygulamada bir route(web uygulamada bir url yolu) olarak çalışır
        //async Task<IActionResult> METOT ASENKRON ÇALIŞIYOR HTTP CEVABI döndürüyor
        //[FromServices] dependency injection'dan al parametre olarak ver
        //Normal parametreler → genellikle URL’den, formdan veya query string’den gelir.
        //[FromServices] parametreleri → Dependency Injection (DI) sisteminden gelir.
        //Yani sen manuel olarak new ELogoService() yapmana gerek yok, ASP.NET Core otomatik olarak hazır bir nesne verir.
        try
        {
            var sonuc = await logoServis.LoginOlVeSessionAlAsync();
            return Content("Gelen Cevap: " + sonuc);
        }
        catch(Exception ex)
        {
            return Content("Hata Oldu Gelen Cevap: " + ex.Message);
        }
    }

}
*/

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeklifYonetimSistemi.Contexts;
using TeklifYonetimSistemi.Models;
using TeklifYonetimSistemi.Models.ViewModels; // ViewModel'i eklemeyi unutma

[Authorize]
public class HomeController : Controller
{
    private readonly VeriTabaniDB _context;
    private readonly UserManager<KullaniciModel> _userManager;

    public HomeController(VeriTabaniDB context,UserManager<KullaniciModel> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);

        var model = new DashboardViewModel
        {
            ToplamUrun = await _context.Products.CountAsync()
           
        };
        if (User.IsInRole("Admin"))
        {
            model.ToplamMusteri = await _context.Customers.CountAsync();
            model.ToplamProje = await _context.Projects.CountAsync();

            model.ToplamCiro = await _context.Quotes.Where(q => q.Durum == QuoteStatus.Onaylandi).SumAsync(q => q.GenelToplam);
            model.YoneticiOnayiBekleyen = await _context.Quotes.CountAsync(q => q.Durum == QuoteStatus.YoneticiOnayBekliyor);

            model.KritikStokSayisi = await _context.Products.CountAsync(p => p.StokTakibiYapilsinMi && p.StokMiktari < 5);

            model.ToplamNetKar = await _context.QuoteItems
                .Where(qi => qi.Quote.Durum == QuoteStatus.Onaylandi)
                .SumAsync(qi => qi.SatirToplami - (qi.Miktar * qi.BirimMaliyet));

            var altiAyOnce = DateTime.UtcNow.AddMonths(-6);
            model.PasifMusteriSayisi = await _context.Customers
        .CountAsync(c => !_context.Projects.Any(p => p.CustomerId == c.Id && p.BaslangicTarihi > altiAyOnce));
            //tüm müşterilere tek tek bak
            //her müşteri için şunu kontrol et -> bu müşterinin 6 aydan daha yeni bir projesi var mı
            //eğer hiç yoksa müşteri pasif

            model.KategoriDagilimi = await _context.QuoteItems
                .Where(qi => qi.Quote.Durum == QuoteStatus.Onaylandi)//sadece onaylanmış tekliflerdeki ürünleri getiriyor
                .GroupBy(qi => qi.Product.Kategori)//aynı kategoride olan ürünleri bir araya toplaa
                .Select(g => new { Kategori = g.Key, Adet = g.Sum(x => x.Miktar) })//bu kategoriye ait ürünler toplam kaç tane satılmış
                .OrderByDescending(x => x.Adet)//kücükten büyüğe sırala
                .Take(5)//en çok satılan 5 kategori seçiliyor
                .ToDictionaryAsync(x => x.Kategori, x => x.Adet);
            //onaylanmış tekliflerde hangi kategori üründen kaç tane satıldığını hesapla en çok satan  ilk 5 kategoriyi sırala ve sözlük olarak döndür
        }

        else if (User.IsInRole("SatisElemani"))
        {
            model.ToplamMusteri = await _context.Customers.CountAsync(c => c.KullaniciId == user.Id);
            model.ToplamProje = await _context.Projects.CountAsync(p => p.ProjeyiOlusturanKullaniciId == user.Id);

            //aksioyn gerektirenler
            model.RevizeGerekenler = await _context.Quotes.CountAsync(q => q.TeklifiOlusturanKullaniciId == user.Id && q.Durum == QuoteStatus.RevizeGerekiyor);

            model.MusteriOnayiBekleyen = await _context.Quotes.CountAsync(q => q.TeklifiOlusturanKullaniciId == user.Id && q.Durum == QuoteStatus.MusteriOnayiBekliyor);

            model.KapananSatislarim = await _context.Quotes
                .CountAsync(q => q.TeklifiOlusturanKullaniciId == user.Id && q.Durum == QuoteStatus.Onaylandi);

        }
        else if (User.IsInRole("FirmaKullanicisi"))
        {
            model.OnayimiBekleyenler = await _context.Quotes
                                                    .Include(q => q.Project)
                                                    .CountAsync(q => q.Project.CustomerId == user.CustomerId && q.Durum == QuoteStatus.MusteriOnayiBekliyor);
            model.AktifProjelerim = await _context.Projects
                .CountAsync(p => p.CustomerId == user.CustomerId && p.ProjeAktifMi);

        }
        if (User.IsInRole("Admin") || User.IsInRole("SatisElemani"))
        {
            var baseQuery = _context.Quotes.AsQueryable();
            if(User.IsInRole("SatisElemani"))
            {
                baseQuery = baseQuery.Where(q => q.TeklifiOlusturanKullaniciId == user.Id);
            }
            int toplamTeklif = await baseQuery.CountAsync();
            int onaylananTeklif = await baseQuery.CountAsync(q => q.Durum == QuoteStatus.Onaylandi);
            model.TeklifBsariOrani = toplamTeklif > 0 ? (onaylananTeklif * 100 / toplamTeklif) : 0;
        }
        if(User.IsInRole("FirmaKullanicisi") && user.CustomerId.HasValue)
        {
            var bagliFirma = await _context.Customers.FirstOrDefaultAsync(c => c.Id == user.CustomerId);
            if(bagliFirma!=null)
            {
                ViewBag.TemsilciId = bagliFirma.KullaniciId.ToString();
            }
        }
        return View(model);

        
    }
}