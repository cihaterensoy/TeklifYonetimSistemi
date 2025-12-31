using Microsoft.AspNetCore.Authorization; // Yetkilendirme kütüphanesi
using Microsoft.AspNetCore.Identity;      // Kullanıcı işlemleri kütüphanesi
using Microsoft.AspNetCore.Mvc;           // MVC yapısı kütüphanesi
using TeklifYonetimSistemi.Contexts;
using TeklifYonetimSistemi.Models;                     // UserViewModel modeli kullanmak için
using Microsoft.AspNetCore.Mvc.Rendering; // SelectList için gerekli
using Microsoft.EntityFrameworkCore;      // Include için gerekli

[Authorize]
public class ProjectController : Controller
{
    private readonly VeriTabaniDB _context;
    private readonly UserManager<KullaniciModel> _userManager; //projeyi kimin oluşturduğunu tutmak için kullanıyoruz

    public ProjectController(VeriTabaniDB context, UserManager<KullaniciModel> userManager)
    {
        
        _context = context;
        _userManager = userManager;
    }
    //aşağıdaki index metodunu buraya taşıdım
    [HttpGet]
    public async Task<IActionResult> Index(int? customerId)
    {
        // Sorguyu hazırlıyoruz (henüz veritabanına gitmedi)  
        //var projectsQuery = await _context.Projects.Include(p => p.Customer).ToListAsync();
        //İlk satırda sorguyu çalıştırıp listeye dönüştürüyorsun → filtre bellekte yapılıyor → ToListAsync patlıyor.
        //bu kod hatalı hemen çalışıp liste döndürmeye çalışıyor aşağıda doğrusu var
        
        //İlk satırda sorguyu hazırlıyorum → filtre EF Core içinde kalıyor → ToListAsync ile düzgün çalışıyor.
        //şu an sadece sorgu taslağı oluşturuyoruz

        var user = await _userManager.GetUserAsync(User); //giriş yapan kullanıcının id'sini görüyoruz
        //controller.User sondaki User
        //eğer kullanıcı null ise giriş yapmamış bu yüzden login sayfasına yönlenidiriyoruz
        if (user == null) return RedirectToAction("Login", "Account");

        bool isAdmin = await _userManager.IsInRoleAsync(user,"Admin"); //kullanıcı admin mi kontrol ediyoruz

        //var projectsQuery = _context.Projects.Include(p => p.Customer).AsQueryable();
        //projede herhangi bir teklif var mı bunu incelemek için bu yapıyı değiştiriyorum



        var projectsQuery = _context.Projects.Include(p => p.Customer).Include(p=>p.Quotes).AsQueryable();

        if (!isAdmin)
        {
            projectsQuery = projectsQuery.Where(p => p.ProjeyiOlusturanKullaniciId == user.Id || (user.CustomerId != null) && p.CustomerId == user.CustomerId);
            //kendi oluşturduğu projeyi veya kendi firmasına ait projeleri görür
            //böylece kullanıcı diğer firmaların veya başkasının projelerini göremez
            //admin değilse sadece kendi oluşturdu
        }
        
        if (customerId.HasValue)
        {
            projectsQuery = projectsQuery.Where(p => p.CustomerId == customerId);

            var customer = await _context.Customers.FindAsync(customerId);
            if (customer !=null)
            {
                ViewBag.MusteriAdi = customer.FirmaUnvani;
            }
        }

        var projects = await projectsQuery.ToListAsync();

         //Include(p => p.Customer) listede müşterinin adını göremmizi sağlar yoksa sadece Id görünür
        return View(projects);
    }
    //kullanıcı sayfayı açtığında burası çalışacak
    [Authorize(Roles = "Admin,SatisElemani")]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        // Müşteri seçimi için veritabanından listeyi çekip "ViewBag" içersine koyuyoruz
        // "Id": Arka planda tutulacak değer
        // "FirmaUnvani": Ekranda görünecek yazı
        ViewBag.Musteriler = new SelectList(await _context.Customers.ToListAsync(), "Id", "FirmaUnvani");
        return View();
    }
    [Authorize(Roles = "Admin,SatisElemani")]
    [HttpPost]
    public async Task<IActionResult> Create(ProjectModel model)
    {
        var user = await _userManager.GetUserAsync(User);//aktif olan kullancının id'sini aldım
        if (!ModelState.IsValid)
        {
            // Hata varsa (örneğin proje adı boşsa) sayfayı tekrar göster.
            // Ama Dropdown boş kalmasın diye listeyi tekrar gönderiyoruz.
            ViewBag.Musteriler = new SelectList(_context.Customers.ToList(), "Id", "FirmaUnvani");
            return View(model);
        }

        //model.Durum = "Devam Ediyor"; //varsayılan olarak atanıyor
        model.ProjeyiOlusturanKullaniciId = user.Id;
        await _context.Projects.AddAsync(model);
        await _context.SaveChangesAsync();
        //return View("Index")
        // View("Index") HATALIDIR. RedirectToAction kullanılır.
        // Çünkü View("Index") dersen, Index sayfası bir Liste beklerken sen ona Model gönderirsin, sistem çöker.
        return RedirectToAction("Index");


    }
    /* Tek Index metodu ile hem listelemeyi hem de filtrelemeyi kullanacağım
     * 
    [HttpGet]//kullanıcı tarayıcıdan linke tıklandığında url'ye bastığında çalışır
    public async Task<IActionResult> Index(int? customerId)
    {
        var projectsQuery = _context.Projects.Include(p => p.Customer).AsQueryable();
        //projeler tablosundan
        //.Include(p => p.Customer) -> project ile ilşkili customer nesnesini al böylece View içinde project customer firmaUnvanini kullanırken ek sorgu calışmaz
        //.AsQueryable(); -> sorguyu henüz çalıştırmıyoruz, sadece LINQ sorgu ağacını oluşturuyoruz
        //bu sayede sonradan where orderBy gibi eklemeleri zincirleyebiliriz bu satır aslında

        if(customerId.HasValue)
        {
            projectsQuery = projectsQuery.Where(p => p.CustomerId == customerId);
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer != null) ViewBag.MusteriAdi = customer.FirmaUnvani;
            //customerId parametresi var mı diye bakar
            //projectsQuery.Where(p => p.CustomerId == customerId) → LINQ filtre eklenir; yalnızca CustomerId alanı eşit olan projeler seçilecek.
            //customerId nullable; EF Core bunu düzgün parametreye çevirir
            //var customer = await _context.Customers.FindAsync(customerId);
            //findasync veritabanından müşteriyi bulmaya çalışır
        }
        var projects = await projectsQuery.ToListAsync();
        return View(projects);
    }
    */
    [HttpGet]
    [Authorize(Roles = "Admin,SatisElemani")]
    public async Task<IActionResult> Edit(int id)
    {
        var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);

        if (project == null)
        {
            return NotFound();
        }

        // Edit sayfası açılırken Dropdown'ın dolu gelmesi için bu satır ŞART:
        ViewBag.Musteriler = new SelectList(await _context.Customers.ToListAsync(), "Id", "FirmaUnvani");

        return View(project);
    }
    [HttpPost]
    [Authorize(Roles = "Admin,SatisElemani")]
    public async Task<IActionResult> Edit(ProjectModel model)
    {
        //bunu kullanıyoruz cünkü projeyi oluşturan kişinin id'si yok olmasın
        //asnotracking-> ef core cektiği veriyi normalde context tarafından izliyor
        //tracking değişikleri otomatik takip eder, update v eya savechanges sırasında degisikleri kaydeder
        //bu yapı ise sadece oku değişiklik yapma demektir
        //sadece veriyi gösterecek veya kontrol edeceksek tracking gereksiz
        //listeleme ve güvenlik kontrolü için iyi bir şeydir
        //kullanıcdan gelen veriyi veritabanıyla karşılaştırır
        //Tracking olursa yanlışlıkla değişiklikler context tarafından takip edilebilir.
        //Biz burada sadece orijinal projeyi kontrol etmek istiyoruz, değiştirmeyeceğiz.
        var existingProject = await _context.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == model.Id);
        if (existingProject == null) return NotFound();
        if (!ModelState.IsValid)
         {

            ViewBag.Musteriler = new SelectList(_context.Customers.ToList(), "Id", "FirmaUnvani");
            return View(model);
        }
        model.ProjeyiOlusturanKullaniciId = existingProject.ProjeyiOlusturanKullaniciId; //projeyi editleyenin id'si gitmesin diye önceden astrackingle çektiğimiz veriyi kullanıyoruz
        _context.Projects.Update(model);
        await _context.SaveChangesAsync();
        return RedirectToAction("Index");
    }
    [HttpPost]
    [Authorize(Roles = "Admin,SatisElemani")]
    public async Task <IActionResult> Delete (int id)
    {
        var project = await _context.Projects.FirstOrDefaultAsync(p=>p.Id==id);
        if (project != null)
        {
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("Index");       
    }
}