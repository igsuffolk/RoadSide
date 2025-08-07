using SharedProject1.Models.Email;

namespace ClassLibrary1.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(EmailMessage message);
    }
}