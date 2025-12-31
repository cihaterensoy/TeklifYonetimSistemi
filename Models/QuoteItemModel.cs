using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace TeklifYonetimSistemi.Models
{
    public class QuoteItemModel
    {
        [Key]
        public int Id { get; set; }


        [ForeignKey("QuoteId")]
        public int QuoteId { get; set; }
        public QuoteModel Quote { get; set; }

        [ForeignKey("ProductId")]
        public int ProductId { get; set; }
        public ProductModel Product { get; set; }


        //snapshot verileri
        public string UrunAdi { get; set; }
        public string Birim { get; set; }
        public int Miktar { get; set; }

       
        //[Column(TypeName = "decimal(18,2)")]//decimalin kaç basamak tutacağını belirler 18 basamaklı basamaklı virgülden sonraki kuruştur
        //public decimal BirimFiyat { get; set; }

        //maliyet hesaplama
        //ürünün katalogdaki brüt fiyatı
        [Display(Name = "Brüt Liste Fiyatı")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal BrutFiyat { get; set; }
        //tedarikciden alınan zincirleme indirimler
        [Display(Name = "Iskonto 1 (%)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Iskonto1 { get; set; } = 0;

        [Display(Name = "Iskonto 2 (%)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Iskonto2 { get; set; } = 0;

        [Display(Name = "Iskonto 3 (%)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Iskonto3 { get; set; } = 0;

        //indirimler düştükten sonra bize geliş maliyeti
        [Display(Name = "Birim")]
        [Column(TypeName = "decimal(18,2)")]//decimalin kaç basamak tutacağını belirler 18 basamaklı basamaklı virgülden sonraki kuruştur
        public decimal BirimMaliyet{ get; set; }

        // --- SATIŞ VE KÂR ---

        // Maliyetin üzerine koyduğumuz kâr
        [Display(Name = "Bayi Kârı (%)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal KarMarji { get; set; } = 0;

        // Müşterinin göreceği son fiyat ('BirimFiyat'  oluyor)
        [Display(Name = "Birim Satış Fiyatı")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal BirimSatisFiyati { get; set; }

        // miktar * birimsatisFiyati
        [Column(TypeName = "decimal(18,2)")]
        public decimal SatirToplami { get; set; }

        //kdv ile ilgili satırlar
        [Display(Name = "KDV Oranı (%)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal KDVOrani { get; set; }

        [Display(Name = "KDV Tutarı")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal KDVTutari { get; set; }

        [Display(Name = "Satır Genel Toplam")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SatirGenelToplam { get; set; }

    }
}