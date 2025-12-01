namespace TeklifYonetimSistemi.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
public enum QuoteStatus

{
    [Display(Name = "Taslak")]
    Taslak=0,

    [Display(Name ="Yönetici Onayı Bekliyor")]
    YoneticiOnayBekliyor=1,

    [Display(Name ="Revize Gerekiyor")]
    RevizeGerekiyor = 2,
    // Sen ONAYLADIN. Sadece bu aşamaya gelirse müşteri görebilir/mail gider.
    [Display(Name = "Müşteri Onayı Bekliyor")]
    MusteriOnayiBekliyor = 3,

    // Müşteri onayladı
    [Display(Name = "Onaylandı (Satış)")]
    Onaylandi = 4,

    // Müşteri reddetti veya sen en baştan iptal ettin
    [Display(Name = "İptal / Red")]
    Reddedildi = 5
}