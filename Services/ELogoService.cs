/*
using System;
using Microsoft.AspNetCore.Http.HttpResults;
using static System.Net.WebRequestMethods;
using System.Net.Sockets;
using System.Text;
using System.Security;
using System.Linq;
using System.Reflection.Metadata;
using System.Xml.Linq;

namespace TeklifYonetimSistemi.Services
{
	public class ELogoService:IELogoService
	{
		private readonly IConfiguration _config; //Iconfiguration appsetting.json içindeki ELogoSetting'e ulaşmak için kullanılıyor
		private readonly HttpClient _http; //http istekleri göndermek için kullanılıyor.
                                           //HttpClient'ı sınıf içersinde doprudan new ile yaratma,
                                           //çünkü her new yaptığımızda arkada bir socket(TCP bağlantısı) açar. bu hemen kapanmaz ve uzun süre beklemede kalır
                                           //“HttpClient new ile oluşturma” =
                                           //Günler sonra patlayan gizli performans hataları, soket sızıntısı ve bağlantı sorunları demek.
                                           //“IHttpClientFactory kullan” = Performans, güvenlik ve bağlantı yönetimi tamamen otomatik.
                                           //bu cümleyi anlamadım aşağıda basit hali var -> HttpClient: HTTP istekleri göndermek için. Önemli: HttpClient'ı sınıf içinde doğrudan new ile yaratma; DI (örn. IHttpClientFactory) ile singleton/transient yönetimi tercih edilmeli — aksi halde socket sızıntısı/performans sorunları olur.
                                           //kısaca -> HttpClient’ı kendin sürekli yaratma, framework’e yaptır; yoksa bağlantılar sızar ve performans düşer
        public ELogoService(IConfiguration config,HttpClient http)
        {
            //constructor, sınıfın bir örneği oluşturulduğunda çalışan metod -> __init__ gibi sanırım pythondaki
            //IConfiguration config → framework sana otomatik olarak appsettings.json ayarlarını verir.
            //HttpClient http → framework sana yönetilen HttpClient verir(socket sızıntısı yok, performans sorunları yok).
            //Bu sayede _config ve _http değişkenleri kullanıma hazır olur.
            _config = config;
            _http = http;
        }
        public async Task<string?> LoginOlVeSessionAlAsync() //bu metod stringte dönebilir null'da dönebilir http/soap çağrılarında bu şekilde daha mantıklıymış
        {
            
            var url = _config["ELogoSettings:Url"];
            var user = _config["ELogoSettings:UserName"]?.Trim();
            var pass = _config["ELogoSettings:Password"]?.Trim();
            var safePass = SecurityElement.Escape(pass);
            var xml = $@"<soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/' 
                xmlns:tem='http://tempuri.org/' 
                xmlns:efat='http://schemas.datacontract.org/2004/07/eFaturaWebService'>
                <soapenv:Header/>
                <soapenv:Body>
                    <tem:Login>
                        <tem:login>
                            <efat:appStr></efat:appStr>
                            <efat:passWord><![CDATA[{safePass}]]></efat:passWord>
                            <efat:source></efat:source>
                            <efat:userName><![CDATA[{user}]]></efat:userName>
                            <efat:version></efat:version>
                        </tem:login>
                    </tem:Login>
                </soapenv:Body>
            </soapenv:Envelope>";

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            //httpRequestMessage -> http isteği oluşturur
            //httpmethod.post -> soap servisine veri göndereceğimiz için post kullanıyoruz
            //url appsetting'de verdiğimiz url
            request.Headers.Add("SOAPAction", "http://tempuri.org/IPostBoxService/Login");
            //SOAPaction header'ı servisleri hangi metodu çağrıyorum diye bakar.
            //burada login metodunu çağırdığımızı servis sağlayıcıya bildirir
            //eğer yanlış veya eksik olursa servis hata verir 500 ya da method not found
            request.Content = new StringContent(xml, Encoding.UTF8, "text/xml");
            //xml -> gönderdiğimiz içerik
            //xml'in karakter seti utf8
            //text/xml soap 1.1 servisi için content-type, server bunu istiyormuş

            var response = await _http.SendAsync(request);
            //SendAsync http isteğini gönderir cevabı bekler
            //response sunucudan gelen http cevabı (status code + content)
            //return await response.Content.ReadAsStringAsync();
            //response.Content -> sunucudan gelen cevabın içeriği soap xml
            //ReadAsStringAsync() xml'i string olarak alır
            //bu string içinde login işleminden dönen sessionID OLUR bununla cıkış yapacağız sanırım örnek kodlarda öyleydi

            var responseContent = await response.Content.ReadAsStringAsync();

            // Cevabı Parse Et (SessionID'yi Çek)
            try
            {
                // 1. Gelen cevabı XML Nesnesine çeviriyoruz
                var doc = XDocument.Parse(responseContent);

                // 2. "LocalName" kullanarak Namespace ne olursa olsun (tns, ns0, vs.) etiketi buluyoruz
                var loginResult = doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "LoginResult")?.Value;
                var sessionId = doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "sessionID")?.Value;

                if (loginResult?.ToLower() == "true" && !string.IsNullOrEmpty(sessionId))
                {
                    return sessionId; // Temiz SessionID dönüyoruz
                }

                throw new Exception($"Login Başarısız! Servis Cevabı: {responseContent}");
            }
            catch (Exception ex)
            {
                // XML parse edilemezse veya başka hata varsa
                throw new Exception($"Login İşlemi Hatası: {ex.Message} | Ham Cevap: {responseContent}");
            }
        }
        public async Task<bool> MukellefKontrolAsync(string vkn)
        {
            string sessionId = await LoginOlVeSessionAlAsync();
            var url = _config["ELogoSettings:Url"];

            // DİKKAT: Python Spyne kütüphanesi bazen namespace'lere takılabilir.
            // XML'i biraz sadeleştiriyorum:
            var xml = $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" 
                                       xmlns:tem=""http://tempuri.org/"" 
                                       xmlns:arr=""http://schemas.microsoft.com/2003/10/Serialization/Arrays"">
                       <soapenv:Header/>
                       <soapenv:Body>
                          <tem:CheckGibUser>
                             <tem:sessionID>{sessionId}</tem:sessionID>
                             <tem:vknTcknList>
                                <arr:string>{vkn}</arr:string>
                             </tem:vknTcknList>
                          </tem:CheckGibUser>
                       </soapenv:Body>
                    </soapenv:Envelope>";

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("SOAPAction", "http://tempuri.org/IPostBoxService/CheckGibUser");
            request.Content = new StringContent(xml, Encoding.UTF8, "text/xml");

            var response = await _http.SendAsync(request);

            // Hata varsa fırlatsın, sessiz kalmasın!
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();

            if (!string.IsNullOrEmpty(responseContent) && responseContent.Contains(vkn))
            {
                return true;
            }

            return false;

        }
    }
}
*/
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using TeklifYonetimSistemi.Services.ELogoProxy;

namespace TeklifYonetimSistemi.Services
{

    //elogoservice sınıfı e-logunun SOAP servisleri ile iletişim kurmak için
    //uygulama içinde kullanılacak bir yardımcı sınıfıdır
    //IELogoService arayüzünü uygular; bu sayede başka sınıflar tarafından direkt bu dosyadan iletilemez
    //ama IELOGOSERVİCE DOSYASINDAN KOLAYCA implementaasyon veya mocklanabilir(bunu araştır)
    // Bu servis, e-Logo sistemine bağlanıp kullanıcı doğrulama (Login) ve
    // mükellef kontrolü (CheckGibUser) yapmak için tasarlanmıştır.
    // IELogoService arayüzünden türemektedir, böylece:
    // - Başka sınıflarda kolayca kullanılabilir,
    // - Unit testlerde mocklanabilir.

    public class ELogoService:IELogoService
    {
        //IConfiguration nesnesi uygulama ayarlarını yani  appsetting ayarlarını okumak için kullanılır
        //burada elogo servisine ait url, vs okunabilir
        private readonly IConfiguration _config;

        public ELogoService(IConfiguration config)
        {
            // Constructor -> servis oluşturulurken .NET tarafından DI ile verilir.
            // DI (Dependency Injection), modern yazılımın en temel prensiplerinden biridir.
            // Amaç: bağımlılıkların dışarıdan verilmesi ve kodun daha test edilebilir olması.

            _config = config;
        }
        //SOAP servisine bağlanmak için generated proxy sınıfı olan
        // Neden böyle bir metot var?
        // Çünkü SOAP servislerine bağlanmak için "ApplicationClient" adında
        // bir sınıf kullanıyoruz. Bu sınıf otomatik olarak WSDL’den üretilir.
        // Bu client'ı tek bir yerde oluşturmak gelecekte bakım kolaylığı sağlar.

        private ApplicationClient ClientOlustur()//wsdl:service bölümüne bak olusturulan dosyadan
        {
            /*
             * Detay: ApplicationClient senin yazdığın bir sınıf değil; WSDL'den otomatik üretilen bir Proxy Class.

                Mantık: "Ayarlardan URL'i al, bu URL'e bağlanacak bir SOAP istemcisi oluştur ve bana ver" diyor.
             */
            // AppSettings içinden servis URL'si okunur.
            var url = _config["ELogoSettings:Url"];
            // Eğer URL yoksa, uygulama eLogo servisine bağlanamaz.
            if (string.IsNullOrEmpty(url))
                throw new Exception("appsettings.json içerisinde ELogoSettings:Url bulunamadı! Bu ayarı eklemelisiniz.");

            // ApplicationClient SOAP için otomatik üretilmiş olan sınıftır.
            // EndpointConfiguration, istemcinin hangi endpoint tanımını kullanacağını belirler.
            return new ApplicationClient(ApplicationClient.EndpointConfiguration.Application, url);
            //“ELogo servisine bağlanmak için bir müşteri (client) oluştur ve bunu bana ver.”

        }
        public async Task<string> LoginOlVeSessionAlAsync()
        {
            // Kullanıcı adı ve şifreyi appsettings.json'dan alıyoruz.
            // Trim() ile boşluklardan arındırıyoruz; çünkü çoğu kişi yanlışlıkla sonunda boşluk bırakıyor.
            var user = _config["ELogoSettings:UserName"]?.Trim();
            var pass = _config["ELogoSettings:Password"]?.Trim();
            // SOAP client oluşturulur.
            using var client = ClientOlustur();

            //soap servisleri doğrudan loginmodel almaz bunun yerine bir istek zarfı alırmış
            //bunu wdsl dosyası belirliyor belki gerçeğinde farklıdır

            var loginZarfi = new Login
            {
                login = new LoginModel
                {
                    userName = user,
                    passWord = pass,

                    // appStr, version gibi alanlar servis tarafından istenebilir.
                    // Genelde bunlar kimlik doğrulamayı değil, uygulama-türü
                    // bilgisini temsil eder.
                    appStr = "Uygulama",
                    version = "1.0",
                    source=""
                }
            };
            try
            {
                var responseWrapper = await client.LoginAsync(loginZarfi);

                // static responseWrapper.LoginResponse.LoginResult yapısından sonucu çekiyoruz.

                var resultModel = responseWrapper?.LoginResponse?.LoginResult;

                if(resultModel!=null && resultModel.LoginResult==true)
                {
                    return resultModel.sessionID;
                }
                // Başarısızsa açıklayıcı bir hata fırlatıyoruz.
                throw new Exception("Login başarısız! Servis LoginResult=false döndürdü.");
            }
            catch (Exception ex)
            {
                // Hata aldığınızda burası çalışır.
                // Daha detaylı loglama sistemi kullanabilirsiniz.
                throw new Exception($"Login Servis Hatası: {ex.Message}");
            }

        }
        public async Task<bool> MukellefKontrolAsync(string vkn)
        {
            try
            {
                var sessionId = await LoginOlVeSessionAlAsync();

                using var client = ClientOlustur();
                //bu yapıyı düzelteceğim her seferinde login olmak için client oluşturuyorum saçma oluyor

                //checkgibUser doğrudan parameetre beklemez nesne bekler
                var checkRequest = new CheckGibUser
                {
                    sessionID = sessionId,
                    vknTcknList = new string[] { vkn } // Liste şeklinde gönderilmelidir.
                };
                // Async servis çağrısını yapıyoruz.
                var responseWrapper = await client.CheckGibUserAsync(checkRequest);
                //servisin dönüş şekli
                //wrapper response result userlist
                var result = responseWrapper?.CheckGibUserResponse?.CheckGibUserResult;

                // userList boş değilse liste içinde döneriz.
                if (result != null && result.userList != null && result.userList.Length > 0)
                {
                    foreach (var user in result.userList)
                    {
                        // Identifier alanı VKN/TCKN bilgisini taşır.
                        if (user.Identifier == vkn)
                            return true; // Bulduysak true döneriz.
                    }
                }
                return false;
            }
            catch
            {
                // Hata durumunda false döndürmek güvenli bir seçenektir.
                return false;
            }
        }
    }
}