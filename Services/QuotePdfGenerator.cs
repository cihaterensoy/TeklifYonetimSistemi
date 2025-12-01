using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TeklifYonetimSistemi.Models;

namespace TeklifYonetimSistemi.Services
{
    public class QuotePdfGenerator
    {
        public QuotePdfGenerator()
        {
            // Lisans ayarı: Community kullanıyoruz
            QuestPDF.Settings.License = LicenseType.Community;
        }
        public byte[] GenerateQuotePdf(QuoteModel quote)
        {
            using var stream = new MemoryStream();
            //veriyi disk yerine reamde saklayan bir akış sınıfıdır memoryStream
            //yani dosyayı fiziksel olarak kaydetmeden bellekte tutup işleyebiliriz
            //questpdf ile pdf oluştururken memoryStream'e yazıyoruz sonra bu byteları tarayıcıya gönderiyoruz

            //using var ne işe yarıyor
            //stream işi bittiğinde otomatik olarak bellek Dispose yani serbest bırakılır eskiden try finally ile yapılıyormuş, stream.Dispose(); yazılması gerekiyormuş

            Document.Create(container => //QuestPdf'de pdf oluşturmayı başlatır
            {
                container.Page(page => //page sayfa özelliklerini ayarlamak için kullanılır
                {
                    page.Margin(50);             // Sayfa kenar boşlukları
                    page.Size(PageSizes.A4);     // Sayfa boyutu A4

                    // Header - Üst bilgi alanı
                    page.Header().Element(ComposeHeader);

                    // Content - Ana içerik
                    page.Content().Element(c => ComposeContent(c, quote));

                    // Footer - Alt bilgi (sayfa numarası)
                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.CurrentPageNumber(); // Mevcut sayfa numarası
                        text.Span(" / ");
                        text.TotalPages(); // Toplam sayfa sayısı
                    });

                    void ComposeHeader(IContainer container)
                    {
                        container.Row(row =>
                        {
                            // Sol tarafta şirket logosu/başlığı
                            row.RelativeItem().Column(column =>
                            {
                                column.Item().Text("TEKLİF BELGESİ")
                                    .FontSize(24)
                                    .Bold()
                                    .FontColor(Colors.Blue.Darken3);

                                column.Item().Text($"Tarih: {DateTime.Now:dd.MM.yyyy}")
                                    .FontSize(10)
                                    .FontColor(Colors.Grey.Medium);
                            });

                            // Sağ tarafta teklif numarası
                            row.ConstantItem(200).AlignRight().Column(column =>
                            {
                                column.Item().PaddingBottom(5).Text($"Teklif No: {quote.TeklifNo}")
                                    .FontSize(14)
                                    .Bold()
                                    .FontColor(Colors.Red.Darken2);

                                column.Item().Text("Geçerlilik: 30 Gün")
                                    .FontSize(9)
                                    .FontColor(Colors.Grey.Medium);
                            });
                        });
                    }

                    void ComposeContent(IContainer container, QuoteModel quoteData)
                    {
                        container.Column(column =>
                        {
                            // Müşteri ve Proje Bilgileri Bölümü
                            column.Item().PaddingVertical(10).Background(Colors.Grey.Lighten3).Padding(10).Column(c =>
                            {
                                c.Item().Text("MÜŞTERİ BİLGİLERİ").Bold().FontSize(12).FontColor(Colors.Blue.Darken3);
                                c.Item().Text($"Firma: {quoteData.Project.Customer.FirmaUnvani}").FontSize(10);
                                c.Item().Text($"Proje: {quoteData.Project.ProjeAdi}").FontSize(10);
                            });

                            // Kur Bilgileri Bölümü
                            column.Item().PaddingVertical(10).Row(row =>
                            {
                                row.RelativeItem().Background(Colors.Green.Lighten5).Padding(10).Column(c =>
                                {
                                    c.Item().Text("DÖVİZ KURLARI").Bold().FontSize(11).FontColor(Colors.Green.Darken3);
                                    c.Item().Text($"USD: {quoteData.DolarKuru} ₺").FontSize(10);
                                    c.Item().Text($"EUR: {quoteData.EuroKuru} ₺").FontSize(10);
                                });

                                row.ConstantItem(20); // Boşluk

                                row.RelativeItem().Background(Colors.Orange.Lighten5).Padding(10).AlignRight().Column(c =>
                                {
                                    c.Item().Text("TEKLİF DURUMU").Bold().FontSize(11).FontColor(Colors.Orange.Darken3);
                                    c.Item().Text("Aktif").FontSize(10); // Durum bilgisi eklenebilir
                                });
                            });

                            // Teklif Kalemleri Tablosu
                            column.Item().PaddingVertical(10).Element(e => ComposeQuoteItemsTable(e, quoteData));

                            // Finansal Özet Bölümü
                            column.Item().PaddingVertical(10).Element(e => ComposeFinancialSummary(e, quoteData));
                        });
                    }

                    void ComposeQuoteItemsTable(IContainer container, QuoteModel quoteData)
                    {
                        container.Column(column =>
                        {
                            column.Item().Text("TEKLİF KALEMLERİ").Bold().FontSize(14).FontColor(Colors.Blue.Darken3);

                            // Tablo başlıkları
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(30);  // Sıra No
                                    columns.RelativeColumn(2);   // Ürün Adı
                                    columns.ConstantColumn(80);  // Miktar
                                    columns.ConstantColumn(100); // Birim Fiyat
                                    columns.ConstantColumn(120); // Toplam
                                });

                                // Tablo header
                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("No").FontColor(Colors.White).Bold();
                                    header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Ürün/Hizmet").FontColor(Colors.White).Bold();
                                    header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Miktar").FontColor(Colors.White).Bold();
                                    header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Birim Fiyat").FontColor(Colors.White).Bold();
                                    header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Toplam (₺)").FontColor(Colors.White).Bold();
                                });

                                // Tablo içeriği
                                int itemNo = 1;
                                foreach (var item in quoteData.QuoteItems)
                                {
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(itemNo.ToString());
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(item.UrunAdi);
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(item.Miktar.ToString());
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{item.BirimSatisFiyati:C}");
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{item.SatirToplami:C}");
                                    itemNo++;
                                }
                            });
                        });
                    }

                    void ComposeFinancialSummary(IContainer container, QuoteModel quoteData)
                    {
                        container.Background(Colors.Grey.Lighten3).Padding(15).Column(column =>
                        {
                            column.Item().AlignRight().Text("FİNANSAL ÖZET").Bold().FontSize(16).FontColor(Colors.Blue.Darken3);

                            column.Item().PaddingVertical(5).AlignRight().Row(row =>
                            {
                                row.RelativeItem().Text("Ara Toplam:");
                                row.ConstantItem(150).Text($"{quoteData.AraToplam:C}").Bold();
                            });

                            column.Item().PaddingVertical(5).AlignRight().Row(row =>
                            {
                                //row.RelativeItem().Text($"KDV (%{quoteData.KDVOrani}):");
                                //row.ConstantItem(150).Text($"{quoteData.KDVTutari:C}").Bold();
                            });

                            column.Item().PaddingVertical(10).AlignRight().Row(row =>
                            {
                                row.RelativeItem().Text("GENEL TOPLAM:").Bold().FontSize(12);
                                row.ConstantItem(150).Text($"{quoteData.GenelToplam:C}").Bold().FontSize(14).FontColor(Colors.Green.Darken3);
                            });
                        });
                    }
                });
            }).GeneratePdf(stream);
            //MemoryStream → PDF'i bellekten byte array olarak elde ederiz, böylece tarayıcıya gönderebiliriz.
            //return stream.ToArray() → Controller'a PDF'i byte[] olarak döndürür.
            return stream.ToArray(); // PDF byte array olarak döner
        }
    }
}