using Microsoft.AspNetCore.Identity;
using TeklifYonetimSistemi.Models; // KullaniciModel burada
using Microsoft.EntityFrameworkCore;

namespace TeklifYonetimSistemi.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider service)
        {
            
            var userManager = service.GetService<UserManager<KullaniciModel>>();
            var roleManager = service.GetService<RoleManager<IdentityRole<int>>>();

            // 1. Rolleri oluştur (Admin, Satis, Firma)
            await CreateRoleAsync(roleManager, UserRoles.Admin);
            await CreateRoleAsync(roleManager, UserRoles.SatisElemani);
            await CreateRoleAsync(roleManager, UserRoles.FirmaKullanicisi);

            // 2. Admin Kullanıcısını oluştur
            var adminEmail = "admin@sirket.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var newAdmin = new KullaniciModel
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    Isim = "Sistem",
                    Soyisim = "Yöneticisi",
                    EmailConfirmed = true
                };

                // Admin kullanıcısını oluştur
                var result = await userManager.CreateAsync(newAdmin, "Admin123!");

                if (result.Succeeded)
                {
                    // Admin rolünü ata
                    await userManager.AddToRoleAsync(newAdmin, UserRoles.Admin);
                }
            }
        }

        private static async Task CreateRoleAsync(RoleManager<IdentityRole<int>> roleManager, string roleName)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                
                await roleManager.CreateAsync(new IdentityRole<int>(roleName));
            }
        }
    }
}