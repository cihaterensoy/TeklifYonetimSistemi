using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using TeklifYonetimSistemi.Contexts;
using TeklifYonetimSistemi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;


namespace TeklifYonetimSistemi.Hubs
{ 

    [Authorize]
    public class ChatHub : Hub
    {
        public readonly VeriTabaniDB _context;

        public ChatHub(VeriTabaniDB context)
        {
            _context = context;
        }
        public async Task SendMessageAsync(string message,int teklifId)
        {
            // Kullanıcının sunucudan güvenli şekilde alınması (int)
            if (!int.TryParse(Context.UserIdentifier, out int senderUserId))
            {
                await Clients.Caller.SendAsync("ErrorMessage", "Geçersiz kullanıcı ID.");
                return;
            }
            // Teklifi ve Durumunu Çek (AsNoTracking performans içindir, sadece okuma yapacağız)
            var teklif = await _context.Quotes
                                       .AsNoTracking()
                                       .FirstOrDefaultAsync(q => q.Id == teklifId);

            // Teklif yoksa veya SÜREÇ BİTMİŞSE mesaj attırma!
            if (teklif == null ||
                teklif.Durum == QuoteStatus.Onaylandi ||
                teklif.Durum == QuoteStatus.Reddedildi)
            {
                // Kullanıcıya uyarı mesajı gönder
                await Clients.Caller.SendAsync("ReceiveMessage", "SİSTEM", "Bu teklif süreci tamamlandığı için mesaj gönderilemez.");
                return;
            }
            string senderDisplayName=Context.User.Identity?.Name ?? "Bilinmeyen Kullanıcı";

            var teklifMesaj = new TeklifMesaj
            {
                TeklifId = teklifId,
                GonderenUserId = senderUserId,
                MesajMetni = message,
                GonderilmeTarihi = DateTime.UtcNow,
                OkunduMu = false
            };
            _context.TeklifMesajlar.Add(teklifMesaj);
            await _context.SaveChangesAsync();

            // HERKESE BİLDİRİM GÖNDERME (Broadcasting)
            // Sadece o teklif grubundaki kişilere gönder.
            await Clients.Group(teklifId.ToString())
                         .SendAsync("ReceiveMessage", senderDisplayName, message);
        }
        //Gruba Katılma Metodu (Sayfa Açıldığında Tetiklenir)
        public async Task JoinGroup(string teklifId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, teklifId);
        }
        /*
        //arama bildirimi gönderme
        //ara butonuna basıldığında bu metot çalışır
        //hedef temsilciye biri seni arıyor sinyali gönderir

        public async Task YetkiliyiAra(string hedefUserId)
        {
            //arayan kişinin ismini alıyoruz
            string arayanIsim = Context.User.Identity?.Name ?? "Bilinmeyen Müşteri";

            //arayan kişinin kendi id'sini gönderiyoruz, karşı taraf kimi arayacağını bilsin
            string arayanId = Context.UserIdentifier;


            // Hedefteki kullanıcıya (Temsilciye) "AramaGeliyor" sinyali gönder.
            // Bu sinyal frontend'de (JS) bir Modal/Popup açılmasını tetikler.
            await Clients.User(hedefUserId).SendAsync("AramaGeliyor", arayanIsim, arayanId);
        }

        //webrtc sinyalleşme
        //webrtc bağlantısı kurulurken tarayıcların birbirine göndermesi gereken
        //offer answerr ve ice candiate bu metot taşır

        public async Task WebRTCSinyalGonder(String hedefUserId,string sinyalVerisi)
        {
            // Gönderen kişinin ID'sini al
            string gonderenId = Context.UserIdentifier;

            //sinyali ve kimden geldiğini direkt hedef kişiye ilet
            //// Frontend tarafında (JS) bu sinyal alınıp WebRTC motoruna işlenir.
            await Clients.User(hedefUserId).SendAsync("WebRTCSinyalAlindi", gonderenId, sinyalVerisi);
        }
        // ... Diğer metotların altına ekle ...

        public async Task AramayiSonlandir(string hedefUserId)
        {
            // Hedef kullanıcıya "Görüşme Bitti" sinyali gönder
            await Clients.User(hedefUserId).SendAsync("GorusmeSonlandirildi");
        }
        */
    }
}