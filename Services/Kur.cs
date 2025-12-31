using System.Net.Http; //http istekleri yapmak için
using System.Xml.Linq; // xml verileri ni LINQ ile okumak için TCMB XML formatında kur verilir sunuyormuş bu yüzden bunu kullandık

public class Kur
{
    private readonly HttpClient _httpClient = new HttpClient();
    public async Task<decimal> GetUsdRateAsync()
    {
        var url = "https://www.tcmb.gov.tr/kurlar/today.xml";
        var xml = await _httpClient.GetStringAsync(url); //string şeklinde aldık urldeki veriyi
        var doc = XDocument.Parse(xml);//string haldeki XML'i Xdocument şekline çevirdik. bu sayede LINQ ile sorgulanabilir hale geliyor
        var usd = doc.Descendants("Currency")//XML currency etiketini alıyor. yani isimlerin yazdığı yer
                        .First(c => c.Attribute("Kod").Value == "USD")
                        .Element("ForexSelling").Value;
        return decimal.Parse(usd.Replace(".", ","));
        //xmldeki nokta ile ayrılmış ondalıkları virgüle çevirdik
    }

    public async Task<decimal> GetEurRateAsync()
    {
        var url = "https://www.tcmb.gov.tr/kurlar/today.xml";

        var xml = await _httpClient.GetStringAsync(url);

        var doc = XDocument.Parse(xml);

        var eur = doc.Descendants("Currency")
                     .First(c => c.Attribute("Kod").Value == "EUR")
                     .Element("ForexSelling").Value;

        return decimal.Parse(eur.Replace(".", ","));
    }

}