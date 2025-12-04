using System.ComponentModel.DataAnnotations;

namespace TeklifYonetimSistemi.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class TeklifMesaj
{
    [Key]
    public int Id { get; set; }

    public int TeklifId { get; set; }
    [ForeignKey("TeklifId")]
    public QuoteModel Teklif { get; set; }

    public int GonderenUserId { get; set; }

    public string MesajMetni { get; set; }
    public DateTime GonderilmeTarihi { get; set; } = DateTime.UtcNow;

    // İndeksleme: Performans için kritik!
    public bool OkunduMu { get; set; } = false;

}