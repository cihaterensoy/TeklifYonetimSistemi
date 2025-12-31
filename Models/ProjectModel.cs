using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;


namespace TeklifYonetimSistemi.Models
{
    public class ProjectModel
    {
        public int Id { get; set; }

        //proje bilgileri
        [Required(ErrorMessage = "Proje adı boş bırakılamaz!")]
        public string ProjeAdi { get; set; }

        public DateTime BaslangicTarihi { get; set; }
        public DateTime? BitisTarihi { get; set; }





        //public ProjectStatus Durum { get; set; } = ProjectStatus.Yeni; //projenin durumunu gireceğiz devam ediyor,bitti,iptal edildi
        [Display(Name = "Proje Aktif mi?")]
        public bool ProjeAktifMi { get; set; } = true;
        //iletişim bilgileri
        public string Email { get; set; }
        public string Telefon { get; set; }
        public string Il { get; set; }
        public string Ilce { get; set; }
        public string Adres { get; set; }

        [ForeignKey("CustomerId")]
        public int CustomerId { get; set; }
        [ValidateNever]// "Bunu doğrulama, görmezden gel" demektir.
        public CustomerModel Customer { get; set; }

        //proje notu
        public string ProjeNotu { get; set; }
        public int? ProjeyiOlusturanKullaniciId { get; set; }


        //bunu ekliyoruz çünkü projemize bağlı teklifleri değerlendirebilelim
        //list şeklinde ekleme sebebimiz revize vs yaptığımızda tek bir teklif bağlı olmasın ama tek teklif gösterebilelim
        [ValidateNever]//görmezden gel boşsa
        public List<QuoteModel> Quotes { get; set; }
        //başlangıçta null olsa bile proje oluştururken problem çıkarmaz çünkü List bunu kabul ediyor
        //mesela project.Quotes.Any() yaparsak  listenin boş olup olmadığının kontrolü yapılabilir

    }
}
