
namespace TeklifYonetimSistemi.Models;

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

public class KullaniciModel:IdentityUser<int>//burada veritabanında id'yi int tabanlı tutmasını sağlar
{

		public string Isim { get; set; }
		public string Soyisim { get; set; }

        public int? CustomerId { get; set; }

    /*
        // Navigation Property
        [ForeignKey("CustomerId")]
        public virtual CustomerModel? Customer { get; set; } // ⚠️ DİKKAT: Tipi 'CustomerModel' olmalı
    */
    //bu değişikliği Serileştirme Döngüsü'nden (Circular Reference)'i çözmek için yaptım
    // Navigation Property
    [ForeignKey("CustomerId")]
    [ValidateNever] // 👇 Kritik: Tag Helper hatalarını ve JSON döngülerini engeller
    public CustomerModel? Customer { get; set; } // 'virtual' kaldırıldı
}





