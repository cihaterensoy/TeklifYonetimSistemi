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
        */
        
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