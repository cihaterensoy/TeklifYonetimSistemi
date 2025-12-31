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
        
    }
}