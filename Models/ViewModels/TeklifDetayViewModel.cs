namespace TeklifYonetimSistemi.Models.ViewModels
{
    public class TeklifDetayViewModel
    {
        //teklifin tüm detayları
        public QuoteModel Teklif {get;set;}

        public List<TeklifMesaj> GecmisMesajlar { get; set; } = new List<TeklifMesaj>();
        //geçmiş mesajları tutacak liste

        // Mesajı kimin attığını ekranda gösterebilmek için User/DisplayName sözlüğü
        // Key: UserId (int), Value: Display Name (string)
        public Dictionary<int, string> KullaniciAdlari { get; set; } = new Dictionary<int, string>();
    }
}