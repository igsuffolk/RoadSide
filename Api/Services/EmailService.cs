using SharedProject1.Models;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Utils;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Api.Services
{
    public interface IEmailService
    {
        Task<EmailMessage> SendEmailAsync(EmailMessage message);
    }

    public class EmailService : IEmailService
    {
        private readonly EmailConfiguration _emailConfig;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;

            _emailConfig = configuration
               .GetSection("EmailConfig")
               .Get<EmailConfiguration>();
        }
        public async Task<EmailMessage> SendEmailAsync(EmailMessage message)
        {
            message.From = _emailConfig.From;

            MimeMessage mailMessage = await CreateEmailMessage(message);

            return await SendAsync(mailMessage);
        }
        private async Task<MimeMessage> CreateEmailMessage(EmailMessage message)
        {
            var emailMessage = new MimeMessage();
            try
            {
                emailMessage.From.Add(new MailboxAddress("", _emailConfig.From));

                emailMessage.To.AddRange(_emailConfig.To.Split(";").ToList().Select(x => new MailboxAddress("", x.Trim())));

                if (message.Cc != null)
                    emailMessage.Cc.AddRange(message.Cc.Select(x => new MailboxAddress("", x.Trim())));

                if (message.Bcc != null)
                    emailMessage.Bcc.AddRange(message.Bcc.Select(x => new MailboxAddress("", x.Trim())));

                emailMessage.Subject = message.Subject;

                BodyBuilder bodyBuilder = new();
                string templateFile = System.IO.File.ReadAllText(Path.Combine(_webHostEnvironment.WebRootPath, "templates", "email.html"));

                //Add Attachments
                if (message.Attachments?.Count > 0)
                {
                    foreach (var file in message.Attachments)
                    {
                        var fileArray = await System.IO.File.ReadAllBytesAsync(Path.Combine(_webHostEnvironment.WebRootPath, "temp", file));

                        bodyBuilder.Attachments.Add(file, fileArray);
                    }
                }

                //Add Embedded Resources eg Images
                if (message.EmbeddedResourcesElementPaths != null && message.EmbeddedResourcesElementPaths.Any())
                {
                    foreach (KeyValuePair<string, string> elementPath in message.EmbeddedResourcesElementPaths)
                    {
                        templateFile = templateFile.Replace(elementPath.Key, elementPath.Value);
                    }
                }

                var mi = bodyBuilder.LinkedResources.Add(Path.Combine(_webHostEnvironment.WebRootPath, "images", "roadside_80.png"));
                mi.ContentId = MimeUtils.GenerateMessageId();
                templateFile = templateFile.Replace("<#LogoImage#>", "<img src='cid:" + mi.ContentId + "'>");

                bodyBuilder.HtmlBody = templateFile;

                emailMessage.Body = bodyBuilder.ToMessageBody();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }
            return emailMessage;
        }
        private async Task<EmailMessage> SendAsync(MimeMessage mailMessage)
        {
            bool sentOk = false;
            using (var client = new SmtpClient())
            {
                try
                {
                    client.ServerCertificateValidationCallback = MySslCertificateValidationCallback;

                    await client.ConnectAsync(_emailConfig.SmtpServer, _emailConfig.Port, _emailConfig.SslConnection);

                    if (!string.IsNullOrEmpty(_emailConfig.UserName) && !string.IsNullOrEmpty(_emailConfig.Password))
                    {
                        await client.AuthenticateAsync(_emailConfig.UserName, _emailConfig.Password);
                    }

                    await client.SendAsync(mailMessage);

                    sentOk = true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message, ex);
                    sentOk = false;
                }
                finally
                {
                    await client.DisconnectAsync(true);
                    client.Dispose();
                }
            }
            EmailMessage emailMessage = new EmailMessage
            {
                Success = sentOk
            };
            return emailMessage;
        }

        static bool MySslCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {

            // If there are no errors, then everything went smoothly.
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            // Note: MailKit will always pass the host name string as the `sender` argument.
            var host = (string)sender;

            if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateNotAvailable) != 0)
            {
                // This means that the remote certificate is unavailable. Notify the user and return false.
                Console.WriteLine("The SSL certificate was not available for {0}", host);
                return false;
            }

            if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateNameMismatch) != 0)
            {
                // This means that the server's SSL certificate did not match the host name that we are trying to connect to.
                var certificate2 = certificate as X509Certificate2;
                var cn = certificate2 != null ? certificate2.GetNameInfo(X509NameType.SimpleName, false) : certificate.Subject;

                Console.WriteLine("The Common Name for the SSL certificate did not match {0}. Instead, it was {1}.", host, cn);
                return false;
            }

            // The only other errors left are chain errors.
            Console.WriteLine("The SSL certificate for the server could not be validated for the following reasons:");

            // The first element's certificate will be the server's SSL certificate (and will match the `certificate` argument)
            // while the last element in the chain will typically either be the Root Certificate Authority's certificate -or- it
            // will be a non-authoritative self-signed certificate that the server admin created. 
            foreach (var element in chain.ChainElements)
            {
                // Each element in the chain will have its own status list. If the status list is empty, it means that the
                // certificate itself did not contain any errors.
                if (element.ChainElementStatus.Length == 0)
                    continue;

                Console.WriteLine("\u2022 {0}", element.Certificate.Subject);
                foreach (var error in element.ChainElementStatus)
                {
                    // `error.StatusInformation` contains a human-readable error string while `error.Status` is the corresponding enum value.
                    Console.WriteLine("\t\u2022 {0}", error.StatusInformation);
                }
            }

            return true;
        }


    }

}

