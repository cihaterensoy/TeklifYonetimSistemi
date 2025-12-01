using System.ComponentModel.DataAnnotations;

namespace TeklifYonetimSistemi.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class QuoteModel
{
    //bu teklifi hangi kullanıcı verdi tarzında bir ekran oluşturmak istiyorum
    //bu yüzden buradaki değişkenleri değiştir
    [Key]
    public int Id { get; set; }

    [Display(Name = "Teklif No")]
    public string TeklifNo { get; set; } // Örn: T-2025-0105. Controller'da üretilecek.
                                         // -----------------------------

    [Required(ErrorMessage = "Teklif adı boş bırakılamaz!")]
    [Display(Name = "Teklif Başlığı")]
    public string TeklifAdi { get; set; }

    public DateTime TeklifOlusturulmaTarihi { get; set; }
    public DateTime? TeklifSonTarihi { get; set; }

    public QuoteStatus Durum { get; set; } = QuoteStatus.Taslak;

    public string TeklifNotu { get; set; }

    //finansal alanlar
    [Column(TypeName = "decimal(18,2)")]
    public decimal AraToplam { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal GenelToplam { get; set; } = 0;

    // EKLENDİ: Kur Takibi (Snapshot)
    [Column(TypeName = "decimal(18,4)")]
    public decimal DolarKuru { get; set; } = 1.0m;

    [Column(TypeName = "decimal(18,4)")]
    public decimal EuroKuru { get; set; } = 1.0m;

    // KDV ORANININ SNAPSHOT'I
    //[Display(Name = "KDV Oranı (%)")]
    //[Column(TypeName = "decimal(5,2)")] // %20.00, %8.00 gibi küçük bir sayı için 5 hane yeterlidir.
    //public decimal KDVOrani { get; set; } = 20.00m; // Varsayılan KDV oranı

    [Display(Name = "Toplam KDV Tutarı")]
    [Column(TypeName = "decimal(18,4)")]
    public decimal ToplamKDV { get; set; }



    public int Vade { get; set; }


    [ForeignKey("ProjectId")]
    public int ProjectId { get; set; }
    public ProjectModel Project { get; set; }

    public List<QuoteItemModel> QuoteItems { get; set; } = new List<QuoteItemModel>(); //birden fazla item seçebilmemiz için liste şeklinde aldık

    public int? TeklifiOlusturanKullaniciId { get; set; }//teklifin kim tarafından verildiği

}
