namespace TeklifYonetimSistemi.Models.ViewModels // Namespace düzeni için öneri
{
    public class KullaniciEkleViewModel
    {
        public string Isim { get; set; }
        public string Soyisim { get; set; }
        public string Email { get; set; }
        public string Sifre { get; set; }
        public string Rol { get; set; }


        // 👇 YENİ EKLENEN ALAN
        // int? (Nullable) olduğu için seçim yapmak zorunda değilsin.
        public int? CustomerId { get; set; }
        //bu sayede kullanıcı firma ile ilişkiliyse hemen ekleyebileceğiz
    }
}