using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Bunu eklemeyi unutma
using TeklifYonetimSistemi.Contexts; // Context'in olduğu yeri ekle

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
}