using Microsoft.AspNetCore.Identity; // Bunu eklemeyi unutma
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TeklifYonetimSistemi.Models;

namespace TeklifYonetimSistemi.Contexts
{
    // DÜZELTME BURADA:
    // Standart IdentityDbContext yerine, tipleri açıkça belirttiğimiz generic yapıyı kullanıyoruz.
    // Sırasıyla: <KullanıcıSınıfı, RolSınıfı, AnahtarTipi>
    public class VeriTabaniDB : IdentityDbContext<KullaniciModel, IdentityRole<int>, int>
    {
        public VeriTabaniDB(DbContextOptions<VeriTabaniDB> options) : base(options) { }

        public DbSet<CustomerModel> Customers { get; set; }
        public DbSet<ProjectModel> Projects { get; set; }
        public DbSet<ProductModel> Products { get; set; }

        public DbSet<QuoteModel> Quotes { get; set; }
        public DbSet<QuoteItemModel> QuoteItems { get; set; }
        public DbSet<TeklifMesaj> TeklifMesajlar { get; set; }

    }
}
/*Bu kod, Identity ile kullanıcı yönetimini destekleyen bir veritabanı bağlamı (DbContext) oluşturuyor.
Projede kullanıcı giriş–kayıt işlemleri, roller, müşteriler, projeler, teklifler gibi tüm veriler bu sınıf üzerinden EF Core tarafından
yönetilecek.*/

