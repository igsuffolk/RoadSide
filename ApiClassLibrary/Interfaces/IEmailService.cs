using SharedProject.Models.Email;

namespace ApiClassLibrary.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(EmailMessage message);
    }
}