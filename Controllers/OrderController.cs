using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TeklifYonetimSistemi.Contexts;
using TeklifYonetimSistemi.Models;
using Microsoft.AspNetCore.Identity;
using TeklifYonetimSistemi.Models.ViewModels;
using System;
using System.Reflection;


[Authorize]
public class OrderController : Controller
{
    private readonly VeriTabaniDB _context;
    private readonly UserManager<KullaniciModel> _userManager;//Identity üzerinden kullanıcı yönetimi sağlıyor
    //_context -> entity frameworkun veri tabanına erişimi

    public OrderController(VeriTabaniDB context,UserManager<KullaniciModel> userManager)
    {
        _context = context;
        _userManager = userManager;
        //dependency injection ile veritabanı ve UserManager inject ediliyor
        //bu sayede controller içinde bu nesnelere erişebiliyoruz

    }
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        //IActionResult ASP.NET Core MVC’de bir action methodunun dönüş tipidir.
        //Yani, controller içindeki method çalıştıktan sonra HTTP yanıtı olarak ne döneceğini belirler.
        //Esnek bir yapıdır; birden fazla türde sonuç döndürebilirsin. view, redirectToaction,json vs
        var user = await _userManager.GetUserAsync(User);
        //giriş yapan kullanıcıyı çekiyoruz
        

        var siparislerQuery = _context.Quotes.Include(q => q.Project).ThenInclude(p=>p.Customer).Where(q => q.Durum == QuoteStatus.Onaylandi).AsQueryable(); // Filtrelemeyi zincirlemek için AsQueryable() kullanıyoruz
        /*
         Bu satır, veritabanındaki Quotes tablosundan sadece durumu “Onaylandı” olan teklifleri seçiyor.
        Tekliflere bağlı olan Project ve onun bağlı olduğu Customer verilerini de getiriyor, yani ilişkili tabloları tek sorguda yüklüyor.
        .AsQueryable() kullanıldığı için, bu sorguya sonradan filtreler eklemek mümkün oluyor;
        örneğin, kullanıcı admin değilse sadece kendi firmasının siparişlerini görebilecek şekilde filtre eklenebiliyor.
        Eğer .AsQueryable() olmasaydı, veritabanından tüm veriler önce belleğe çekilir, sonra filtre uygulanırdı ve bu performans kaybına yol açardı.
        Yani AsQueryable(), sorguyu veritabanı üzerinde zincirleme olarak geliştirebilmemizi sağlıyor.
         */
        //eğer kullanıcı admin değilse kendi firmasının siparişlerini görsün
        bool isInternalStaff = await _userManager.IsInRoleAsync(user, "Admin") ||
                       await _userManager.IsInRoleAsync(user, "SatisElemani");

        // 2. KONTROL: Eğer İç Personel Değilse VE CustomerId'si Varsa filtrele
        if (!isInternalStaff && user.CustomerId.HasValue)
        {
            siparislerQuery = siparislerQuery.Where(p => p.Project.CustomerId == user.CustomerId.Value);
            /*
             * Bu kod, siparislerQuery üzerinde ek bir filtre uyguluyor. Yani:
                Sorguya ekleniyor: Project’in CustomerId değeri, giriş yapan kullanıcının CustomerId’sine eşit olan teklifler (siparişler) seçilsin.
                Kısaca: Kullanıcı admin veya satış elemanı değilse, sadece kendi firmasına ait siparişleri görebilsin mantığı.
                .Value kullanımı, CustomerId’nin nullable (yani int?) olduğunu gösteriyor ve gerçek değerini alıyoruz.
                 Özetle: Bu satır rol bazlı filtreyi uygulayan kritik adımdır; kullanıcı sadece kendi firmasının siparişlerini görür.
            */
        }
        var siparisler = await siparislerQuery.OrderByDescending(q => q.TeklifOlusturulmaTarihi).ToListAsync();


        return View(siparisler);

    }
}