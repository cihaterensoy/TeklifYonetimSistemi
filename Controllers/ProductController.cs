using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TeklifYonetimSistemi.Contexts;
using TeklifYonetimSistemi.Models;

//[Authorize(Roles ="Admin")]
//@if (User.IsInRole("Admin,SatisElemani"))
[Authorize]
public class ProductController:Controller
{
    private readonly VeriTabaniDB _context;

    public ProductController(VeriTabaniDB context)
    {
        _context = context;
    }

    public async Task<IActionResult> Liste()
    {
        //var urunSayisi = await _context.Products.CountAsync(); //veritabanındaki sadece sayıyı al
        //ViewBag.ToplamUrun = urunSayisi; //ürün sayımızı ekrana iletmek için değişkene atadık
        var urunler = await _context.Products.ToListAsync(); // ToListAsync kullanıldı
        return View(urunler);
    }
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        //kategori için dropdown
        ViewBag.Kategoriler = new SelectList(new List<string>()
        {
            "Donanım", "Yazılım", "Hizmet", "Altyapı"
        });
        //Birim listesi
        ViewBag.Birimler = new SelectList(new List<String>()
        {
            "Adet", "Saat", "Gün","Ay","Yıl","Metre", "Lisans", "Kutu"
        });
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(ProductModel model)
    {
        // Eğer Admin "Stok Takibi Yapılsın mı?" kutucuğunu işaretlemediyse (FALSE), 
        // Stok Miktarı ne olursa olsun, veritabanına 0 gönderiyoruz.
        if (model.StokTakibiYapilsinMi == false)
        {
            model.StokMiktari = 0;
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Kategoriler = new SelectList(new List<string>() { "Donanım", "Yazılım", "Hizmet", "Altyapı" });
            ViewBag.Birimler = new SelectList(new List<string>() { "Adet", "Saat", "Gün","Ay","Yıl","Metre", "Lisans", "Kutu" });
            return View(model);
        }
        await _context.Products.AddAsync(model);
        await _context.SaveChangesAsync();

        return RedirectToAction("Liste");
    }
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Pasif(int id)
    {
        var product = await _context.Products.FirstOrDefaultAsync(m => m.Id == id);
        if (product!=null)
        {
            product.UrunAktifMi = false;
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }
          
        return RedirectToAction("Liste");
        //view olsaydı aynı ekran kalıp hata verecekti
        //bu sayede bu ekrana yeniden gidiyor
    }
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Aktif(int id)
    {
        var product = await _context.Products.FirstOrDefaultAsync(m => m.Id == id);
        if (product != null)
        {
            product.UrunAktifMi = true;
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Liste");
        //view olsaydı aynı ekran kalıp hata verecekti
        //bu sayede bu ekrana yeniden gidiyor
    }
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _context.Products.FirstOrDefaultAsync(m => m.Id == id);
        if (product == null) // Sadece ürünün varlığını kontrol et
        {
            return NotFound();
        }
        ViewBag.Kategoriler = new SelectList(new List<string>() { "Donanım", "Yazılım", "Hizmet", "Altyapı" }, product.Kategori);
        ViewBag.Birimler = new SelectList(new List<string>() { "Adet", "Saat", "Gün", "Ay", "Yıl", "Metre", "Lisans", "Kutu" });
        return View(product);
    }
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(ProductModel model)
    {
        //edit'in gönderdiği veripaketi modeldir yani productModel
        if (!ModelState.IsValid)
        {
            //Hata durumunda sayfa geri dönerken Dropdownlar BOŞ dönmemeli!
            ViewBag.Kategoriler = new SelectList(new List<string>() { "Donanım", "Yazılım", "Hizmet", "Altyapı" });
            ViewBag.Birimler = new SelectList(new List<string>() { "Adet", "Saat", "Gün", "Ay", "Yıl", "Metre", "Lisans", "Kutu" });
            return View(model);
        }
        _context.Products.Update(model);
        await _context.SaveChangesAsync();
        return RedirectToAction("Liste");
    }
    [HttpGet]
    public async Task<IActionResult> Details(int id )
    {
        var product = await _context.Products.FirstOrDefaultAsync(m => m.Id == id);
        if(product==null)
        {
            return NotFound();
        }
        return View(product);
    }
}