using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TeklifYonetimSistemi.Models;

public class LoginController : Controller
{
    private readonly SignInManager<KullaniciModel> _signInManager;
    private readonly UserManager<KullaniciModel> _userManager;
    public LoginController(SignInManager<KullaniciModel> signInManager,UserManager<KullaniciModel> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if(!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            model.Email,//Login sayfasındaki formdan gelen email bilgisidir.
            model.Password,
            isPersistent: false,//tarayıcı kapansa bile oturum acık kalsın mı
            lockoutOnFailure: false//başarısız girişlerde kullancııyı kilitler
            );
        if(result.Succeeded)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                // Artık senin eklediğin alanlara erişebilirsin
                string isim = user.Isim;       
                string soyisim = user.Soyisim; 

                // Örnek: View'a bu ismi göndermek istersen:
                ViewBag.AdSoyad = $"{isim} {soyisim}";
            }
            return RedirectToAction("Index", "Home");
        }
        ModelState.AddModelError("", "Email veya şifre yanlış!");
        return View(model);//giriş başarılı olmazsa buraya yönlendiriyor
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login", "Login");
    }
}