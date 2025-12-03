using System.ComponentModel.DataAnnotations;

namespace TeklifYonetimSistemi.Models.ViewModels
{
    public class MusteriKayitViewModel
    {
        public string FirmaUnvani { get; set; }
        public string Email { get; set; }
        public string Telefon { get; set; }
        public string Il { get; set; }
        public string Ilce { get; set; }
        public string Adres { get; set; }
        public string? Fax { get; set; }
        public string? FirmaDetay { get; set; }
        [Display(Name = "Vergi No / TCKN")]
        [StringLength(11, ErrorMessage = "Vergi No en fazla 11 karakter olabilir.")]
        public string? VergiNo { get; set; }

        [Display(Name = "Vergi Dairesi")]
        public string? VergiDairesi { get; set; }

        // Bu alan formdan gelmeyebilir (Controller hesaplayacak) ama modelde durması sorun değil.
        public bool EFaturaMukellefiMi { get; set; } = false;

        public string? YetkiliIsim { get; set; }
        public string? YetkiliSoyisim { get; set; }
        public string? YetkiliEmail { get; set; }
        public string? YetkiliSifre { get; set; }

        public int? SecilenVarolanKullaniciId { get; set; }


    }
}


//burası sadece formdan bilgi taşımak için bulunuyor. herhangi bir veritabanı kaydı yok yani
//from verilerini gecici olarak saklaamk ve işlemek için kullanılır
//form post edilir -> controller musterikayitViewmodel parametresi alır
//conroller modeldeki alanları gerekirse customer model vey akullanıcı modele aktarır
//viewmodel kendi başına db'de bir tabloya sahip dğeildir sadecce geçici taşıma aracıdır
