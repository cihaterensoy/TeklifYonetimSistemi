using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TeklifYonetimSistemi.Models;

public class RegisterController: Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    //Kullanıcı ekleme, şifre doğrulama, email kontrolü gibi işlemler için UserManager’ı kullanacağım” demektir
    public RegisterController(UserManager<IdentityUser>userManager)
    {
        _userManager = userManager;
        //ASP.Net Core'un Dependency Injection sistemi Sayesinde Usermanager servisini controller içine otomatik olarak enjekte eder
    }
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if(!ModelState.IsValid)
        {
            return View(model);
            //eğer kurallara göre değilse tekrar aynı sayfayı gösteriyor
        };
        var user = new IdentityUser
        {
            UserName = model.Email,
            Email = model.Email,

        };
        var result = await _userManager.CreateAsync(user, model.Password);

        if(result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, "User");

            return Redirect("Login/Login");
        }
        foreach (var error in result.Errors)
            ModelState.AddModelError("", error.Description);
        return View(model);
    }
}