using System.Net;
using System.Net.Mail;

namespace TeklifYonetimSistemi.Services
{
    public interface IELogoService
    {
        Task<string> LoginOlVeSessionAlAsync();
        Task<bool> MukellefKontrolAsync(string vkn);
    }
}