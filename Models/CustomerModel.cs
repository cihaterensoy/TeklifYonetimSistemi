namespace TeklifYonetimSistemi.Models;

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

public class CustomerModel
{
    [Key]
    public int Id { get; set; }
    //Key'i primary key yapmak için kullandık
    //Şirket bilgileri
    public string FirmaUnvani { get; set; }
    //iletişim bilgileri
    public string Email { get; set; }
    public string Telefon { get; set; }
    public string Il { get; set; }
    public string Ilce { get; set; }
    public string Adres { get; set; }
    public string Fax { get; set; }
    //şirket adına görüşülen kişi
    //public string YetkiliIsim { get; set; }
    //public string YetkiliSoyisim { get; set; }

    //Notlar ve özel istekler
    public string FirmaDetay { get; set; }
    
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.Now;

    [Display(Name = "Müşteri Hala Bizle Çalışıyor Mu?")]
    public bool MusteriAktifMi { get; set; } = true;

    //müşteriyi oluşturan kullanıcı bilgisi satış sorumlusu veya admin
    [ForeignKey("Kullanici")]
    public int KullaniciId { get; set; }
    [ValidateNever]
    public KullaniciModel Kullanici { get; set; }


    // ==========================================================
    // 2. YENİ EKLENECEK KISIM (Karşı Firma Yetkilileri)
    // ==========================================================
    /* Açıklama: KullaniciModel'in içine 'CustomerId' eklemiştik ya?
       İşte o ID'ye sahip olan kullanıcıları burada liste olarak göreceksin.
       Yani bu liste "Firma Çalışanları"nı getirecek.
    */
    //Bu sınıftaki FirmaYetkilileri ile KullaniciModel içindeki Customer özelliği birbirinin ters yönlü ilişkisidir
    [InverseProperty("Customer")]
    public virtual ICollection<KullaniciModel> FirmaYetkilileri { get; set; }
    //Entity Framework’te bir sınıfın başka bir sınıfla ilişkisini temsil eden property’lere navigasyon özelliği denir.
    //sınıf firmaModel
    //navigasyon özelliği
    //
    //bağlı olduğu model: KullaniciModel
    //ICollection<T>, birden çok öğeyi saklayan bir koleksiyon demektir
    //virtual anahtar kelimesi EF’in Lazy Loading yapmasını sağlar.
    //Lazy Loading = Veri sadece ihtiyaç olduğunda veritabanından çekilir.


    //e-logo için eklenen detaylar
    [Display(Name ="Vergi No / TCKN")]
    [StringLength(11,ErrorMessage = "Vergi No/TCKN en fazla 11 karakter olabilir.")]
    public string? VergiNo { get; set; }

    [Display(Name = "Vergi Dairesi")]
    public string? VergiDairesi { get; set; }

    [Display(Name = "e-Fatura Mükellefi mi?")]
    public bool EFaturaMukellefiMi { get; set; } = false;

}

/*
IdentityUser miras alındı → Email, Username, PasswordHash, SecurityStamp, vs. hazır.
Kendi alanlarını ekledik (OlusturulmaTarihi, FirmaAciklamasi vb.).
Artık UserManager<ApplicationUser> kullanarak kullanıcı oluşturabilir ve yönetilebilir
*/