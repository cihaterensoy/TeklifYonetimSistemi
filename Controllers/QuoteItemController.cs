using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TeklifYonetimSistemi.Contexts;
using TeklifYonetimSistemi.Models;

[Authorize(Roles = "Admin,SatisElemani")]
public class QuoteItemController : Controller
{
    private readonly VeriTabaniDB _context;
    public QuoteItemController(VeriTabaniDB context)
    {
        _context = context;
    }
    [HttpGet]
    public async Task<IActionResult> Index(int quoteId)
    {
        // a. teklifi, bağlı olduğu proje/müşteriyi ve ona bağlı olan satırları/ürünleri getir
        var quote = await _context.Quotes
                                  .Include(q => q.Project)
                                  .ThenInclude(p => p.Customer)
                                  .Include(q => q.QuoteItems)
                                  .ThenInclude(qi => qi.Product)
                                  .FirstOrDefaultAsync(q => q.Id == quoteId);


        //Her teklifle beraber bağlı olduğu Project tablosu da çekilsin anlamına gelir.
        //Her projenin bağlı olduğu Customer da getirilsin.
        //Teklife ait olan ürün/hizmet satırlarını (QuoteItems) da yükler.
        //Id değeri verilen quoteId olan tek kayıt alınır
        /*
         Teklifin kendisi
        Teklifin bağlı olduğu proje
        Projenin müşterisi
        Teklif satırları
        Satırların ürün bilgileri
         */

        if (quote == null) return NotFound();
        // B. Ürün/Hizmet Dropdown Listesini Hazırla (Sadece aktif olan ürünleri göster)
        // Hibrit senaryoya uygun olarak, Ürün Adı ve Fiyatı Dropdown'da gösteriyoruz.
        /*
        var urunler = await _context.Products.Where(p => p.UrunAktifMi == true)//aktif ürünlerden
                                              .Select(p => new
                                              {
                                                  Id = p.Id,
                                                  AdVeFiyat = p.UrunAdi + "( " + p.Birim + " - " + p.SatisFiyatListesi.ToString("C2") + ")",
                                                  // Anonim nesne projeksiyonu
                                                  BirimFiyat = p.SatisFiyatListesi,
                                                  Birim = p.Birim
                                              })//LINQ(sorgu dili) içinde bir projeksiyon işlemidir
                                                //bu sayede anonim nesne oluşturuyoruz
                                              .ToListAsync();
        */
        var urunListesi = await _context.Products
                                    .Where(p => p.UrunAktifMi == true)
                                    .OrderBy(p => p.UrunAdi)
                                    .ToListAsync();
        //dropdown listesinde görünen ürün listesinin kodunu yazdık
        /*
         * JS ile ürün seçtiğinde
            Alış maliyeti
            Kâr marjı
            KDV oranı
            Liste fiyatı
         */
        ViewBag.UrunListesi = urunListesi;
        //bu değişkenleri view tarafına gönderiyoruz, razor sayfasında selectList kullanılabilir
        //C2 noktadan sonra iki sayı gösterilmesini sağlar yani kuruşları
        return View(quote);
        //view model olarak quote gönderilir. görüntüleme sayfası index.cshtml @model quotemODEL SEKLŞNDE TANIMLANMIŞ OLUR VE QUOTE.QUOTEıTEMS DÖNGÜSÜYLE SATIRLAR LİSTELENİR

    }
    [HttpPost]
    public async Task<IActionResult> AddItem(int quoteId,int productId,int quantity, decimal alisMaliyeti,decimal karMarji,decimal isk1,decimal isk2,decimal isk3,string dovizKuru)
    {
        if (!string.IsNullOrEmpty(dovizKuru)) dovizKuru = dovizKuru.Replace(",", ".");
        decimal finalDovizKuru = 0;
        decimal.TryParse(dovizKuru, NumberStyles.Any, CultureInfo.InvariantCulture, out finalDovizKuru);
        var urun = await _context.Products.FindAsync(productId);
        var teklif = await _context.Quotes.Include(q => q.QuoteItems).FirstOrDefaultAsync(q => q.Id == quoteId);

        if(urun==null || teklif==null || quantity<=0)
        {
            TempData["HataMesaji"] = "Lütfen geçerli değerler giriniz.";
            return RedirectToAction("Index", new { quoteId = quoteId });
        }
        if (finalDovizKuru <= 0) finalDovizKuru = 1;
        // --- KRİTİK NOKTA BURASI ---
        // Formdan gelen Dövizli Fiyatı, Kur ile çarpıp TL'ye çeviriyoruz.
        // Eğer ürün zaten TL ise formdan kur "1" geleceği için sonuç değişmez.
        decimal tlBazliMaliyet = alisMaliyeti * finalDovizKuru;

        // Artık hesaplamalara "tlBazliMaliyet" üzerinden devam ediyoruz.
        decimal netMaliyet = tlBazliMaliyet;
        //formdan gelen alış maliyeti üzerinden tedarikçi iskontolarını düşürüyoruz
        //eğer kullanıcı iskontolu net fiyatı girdiyse bunlar 0 gelsede problem değil
        //decimal netMaliyet = alisMaliyeti;
        if (isk1 > 0) netMaliyet = netMaliyet * (1 - (isk1 / 100));
        if (isk2 > 0) netMaliyet = netMaliyet * (1 - (isk2 / 100));
        if (isk3 > 0) netMaliyet = netMaliyet * (1 - (isk3 / 100));

        decimal birimSatisFiyati = netMaliyet * (1 + (karMarji / 100));
        //karmarjini burada urun tablosundan bilerek almadık, ekrana otomatik gelmesini sağlayacağız sadece burada hiç değiştirmeye gerek yok
        //çünkü indirim dahi yapamayız

        int kdvOrani = urun.KDVOrani; 
        decimal birimKdvTutari = birimSatisFiyati * (kdvOrani / 100m);

        //kdv dahil birim fiyat
        decimal birimGenelFiyat = birimSatisFiyati + birimKdvTutari;

        //satır toplamı
        //quantity miktr
        decimal satirKdvHaricToplam = birimSatisFiyati * quantity;
        decimal satirKdvTutari = birimKdvTutari * quantity;
        decimal satirGenelToplam = birimGenelFiyat * quantity;

        //veritabanı kayıt yani snapshot
        //burada yaptığım şey her şey değişse bile müşteriye nasıl bir teklif verdim bunu bilmek
        var newItem = new QuoteItemModel
        {
            QuoteId = quoteId,
            ProductId=productId,
            //ürün bilgeleri ve adı değişse bile her şey aynı kalacak
            UrunAdi=urun.UrunAdi,
            Birim=urun.Birim,
            Miktar = quantity,

            //finansal veriler
            Iskonto1=isk1,Iskonto2=isk2,Iskonto3=isk3,
            BirimMaliyet=netMaliyet,//hesaplanan net maliyet
            KarMarji=karMarji,

            //sonuclar
            BirimSatisFiyati=birimSatisFiyati,
            KDVOrani=kdvOrani,
            KDVTutari=satirKdvTutari,

            //toplamlar
            SatirToplami=satirKdvHaricToplam,
            SatirGenelToplam=satirGenelToplam // burada kdv'de dahil edilmiş
        };
        _context.QuoteItems.Add(newItem);
        await _context.SaveChangesAsync();

        //teklif toplamlarını güncellemek için yazılmış fonksioynu çağrıyorum
        await RecalculateQuoteTotals(quoteId);

        TempData["BasariMesaji"] = "Ürün başarıyla eklendi.";
        return RedirectToAction("Index", new { quoteId = quoteId });



    }
    [HttpPost]
    public async Task<IActionResult>RemoveItem(int itemId)
    {
        var item = await _context.QuoteItems.Include(qi => qi.Quote).FirstOrDefaultAsync(qi => qi.Id == itemId);
        //veritabanından silinecek Quoteıtem satırını çekiyor
        //include(qi=>qi.Quote) ile satırın bağlı olduğu teklif nesnesini de yğklüyor
        //first... item bulunamazsa null döner
        
        if (item == null) return NotFound();
        int quoteId = item.QuoteId;

        _context.QuoteItems.Remove(item);
        await _context.SaveChangesAsync();

        await RecalculateQuoteTotals(quoteId);
        //burada silinen satır göze alınarak yeniden hesaplama yapılır
        TempData["BasariMesaji"] = "Satır silindi.";
        return RedirectToAction("Index", new { quoteId = quoteId });
        //kullanıcı teklife ait listeleme sayfasına yönlendirilir. bu sayfa artık silinmiş satır olmadan gösterilir
    }
    [HttpPost]
    public async Task<IActionResult> UpdateKDV(int itemId, int yeniKdvOrani)
    {
        var item = await _context.QuoteItems.FirstOrDefaultAsync(qi => qi.Id == itemId);
        if (item == null) return NotFound();

        // 1. Yeni oranı ata
        item.KDVOrani = yeniKdvOrani;

        // 2. Matrahı hesapla (Birim Fiyat * Miktar)
        decimal matrah = item.BirimSatisFiyati * item.Miktar;

        // 3. Yeni Vergi Tutarını Hesapla
        decimal yeniVergiTutari = matrah * (yeniKdvOrani / 100m);

        // 4. Verileri Güncelle (Math.Round finansal işlemlerde önemlidir)
        item.KDVTutari = Math.Round(yeniVergiTutari, 2);
        item.SatirToplami = Math.Round(matrah, 2); // KDV Hariç Toplam
        item.SatirGenelToplam = item.SatirToplami + item.KDVTutari; // KDV Dahil Toplam

        _context.QuoteItems.Update(item);
        await _context.SaveChangesAsync();

        await RecalculateQuoteTotals(item.QuoteId);

        TempData["BasariMesaji"] = "KDV oranı ve tutarlar güncellendi.";
        return RedirectToAction("Index", new { quoteId = item.QuoteId });
    }


    private async Task RecalculateQuoteTotals(int quoteId)
    {
        var teklif = await _context.Quotes.FindAsync(quoteId);
        var satirlar = await _context.QuoteItems.Where(x => x.QuoteId == quoteId).ToListAsync();

        if (teklif != null)
        {
            // Ara Toplam (KDV Hariç Ürün Bedelleri)
            teklif.AraToplam = satirlar.Sum(x => x.SatirToplami);

            // Toplam KDV (Sadece KDV Tutarlarının Toplamı)
            teklif.ToplamKDV = satirlar.Sum(x => x.KDVTutari);

            // Genel Toplam
            teklif.GenelToplam = satirlar.Sum(x => x.SatirGenelToplam);

            _context.Quotes.Update(teklif);
            await _context.SaveChangesAsync();
        }
    }
}
