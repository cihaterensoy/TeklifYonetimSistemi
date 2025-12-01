namespace TeklifYonetimSistemi.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


public class ProductModel
{
    [Key]
    public int Id { get; set; }

    //ürün kimliği
    [Required(ErrorMessage = "Ürün adı boş bırakılamaz.")]
    [Display(Name = "Ürün / Hizmet Adı")]
    public string UrunAdi { get; set; }

    [Required(ErrorMessage = "Kategori boş bırakalamaz")]
    [Display(Name = "Kategori")]
    public string Kategori { get; set; }

    [Display(Name = "Stok Takibi Var mı?")]
    public bool StokTakibiYapilsinMi { get; set; } = true;

    [Display(Name = "Stok Miktarı")]
    public int StokMiktari { get; set; } = 0;

    [Required(ErrorMessage = "Birim Boş Bırakılamaz")]
    [Display(Name = "Birim")]
    public string Birim { get; set; }

    //detaylar
    [Required(ErrorMessage = "Teknik Açıklama")]
    public string Aciklama { get; set; }

    [Display(Name = "Ürün Aktif Olarak Kullanılıyor mu?")]
    public bool UrunAktifMi { get; set; } = true;

    //fiyatlandırma


    //[Required(ErrorMessage = "Birim Fiyat")]
    [Display(Name = "Varsayılan Alış Maliyeti")]
    [Column(TypeName = "decimal(18,2)")]//decimalin kaç basamak tutacağını belirler 18 basamaklı basamaklı virgülden sonraki kuruştur
    public decimal AlisFiyati { get; set; } = 0;

    //müşteriye ilk söylenen liste fiyatı
    [Display(Name = "Liste Satış Fiyatı")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal SatisFiyatListesi { get; set; }

    [Display(Name = "Varsayılan Kar Marjı (%)")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal VarsayilanKarMarji { get; set; } = 20;


    [Display(Name = "KDV Oranı (%)")]
    public int KDVOrani { get; set; } = 20;
    // YENİ: Para Birimi (USD, EUR, TL)
    [Display(Name = "Para Birimi")]
    [StringLength(5)]
    public string ParaBirimi { get; set; } = "TL";





    //buradan sonrası tamamıyla yapay zeka destekli bir şekilde yazıldı
}


