using SharedProject.Models.Email;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Utils;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using ApiClassLibrary.Interfaces;

namespace ApiClassLibrary.Services
{
    /// <summary>
    /// Service responsible for composing and sending email messages using MailKit/MimeKit.
    /// Reads SMTP and message defaults from configuration via the <see cref="EmailConfiguration"/> model.
    /// </summary>
    public class EmailService : IEmailService
    {
        // Holds email configuration loaded from IConfiguration ("EmailConfig" section).
        private readonly EmailConfiguration? _emailConfig = new();
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<EmailService> _logger;

        /// <summary>
        /// Constructs the <see cref="EmailService"/> and loads the EmailConfig section.
        /// </summary>
        /// <param name="configuration">Application configuration containing "EmailConfig".</param>
        /// <param name="logger">Logger for diagnostic messages and errors.</param>
        /// <param name="webHostEnvironment">Provides access to web root paths for file attachments and images.</param>
        public EmailService(IConfiguration configuration, ILogger<EmailService> logger, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;

            // Bind configuration section to strongly-typed model.
            _emailConfig = configuration
               .GetSection("EmailConfig")
               .Get<EmailConfiguration>();
        }

        /// <summary>
        /// Public entry point to send an email. This method sets the From address from configuration,
        /// creates the MIME message and dispatches it using SMTP.
        /// </summary>
        /// <param name="message">The email message details (To, Subject, Content, Attachments, etc.).</param>
        /// <returns>True if the message was sent successfully; otherwise false.</returns>
        public async Task<bool> SendEmailAsync(EmailMessage message)
        {
            // Ensure From is set to the configured default.
            message.From = _emailConfig.From;

            // Build the MimeMessage from the provided EmailMessage.
            MimeMessage mailMessage = await CreateEmailMessage(message);

            // Send the constructed MimeMessage via SMTP.
            return await SendAsync(mailMessage);
        }

        /// <summary>
        /// Constructs a <see cref="MimeMessage"/> from the provided <see cref="EmailMessage"/>.
        /// Handles recipients, CC, BCC, subject, HTML body, embedded resources and attachments.
        /// Errors during construction are logged and an empty MimeMessage is returned.
        /// </summary>
        /// <param name="message">Email message model containing content and attachments.</param>
        /// <returns>A fully constructed <see cref="MimeMessage"/> ready to send.</returns>
        private async Task<MimeMessage> CreateEmailMessage(EmailMessage message)
        {
            var emailMessage = new MimeMessage();
            try
            {
                // Set the From mailbox using configured value.
                emailMessage.From.Add(new MailboxAddress("", _emailConfig.From));

                // Add default To recipients from configuration (semicolon-separated).
                emailMessage.To.AddRange(_emailConfig.To.Split(";").ToList().Select(x => new MailboxAddress("", x.Trim())));

                // Add optional Cc and Bcc recipients from the message.
                if (message.Cc != null)
                    emailMessage.Cc.AddRange(message.Cc.Select(x => new MailboxAddress("", x.Trim())));

                if (message.Bcc != null)
                    emailMessage.Bcc.AddRange(message.Bcc.Select(x => new MailboxAddress("", x.Trim())));

                emailMessage.Subject = message.Subject;

                BodyBuilder bodyBuilder = new();

                // Add attachments from temporary web root folder if provided.
                if (message.Attachments?.Count > 0)
                {
                    foreach (var file in message.Attachments)
                    {
                        // Read attachment bytes from wwwroot/temp/{file}
                        var fileArray = await System.IO.File.ReadAllBytesAsync(Path.Combine(_webHostEnvironment.WebRootPath, "ReportPhotos", file));

                        // Add the attachment with the original file name.
                        bodyBuilder.Attachments.Add(file, fileArray);
                    }
                }

                // Embed a logo image from wwwroot/images and replace placeholder in the content
                // with an inline cid reference so the image displays in HTML email clients.
                var mi = bodyBuilder.LinkedResources.Add(Path.Combine(_webHostEnvironment.WebRootPath, "images", "roadside_80.png"));
                mi.ContentId = MimeUtils.GenerateMessageId();
                message.Content = message.Content.Replace("<#LogoImage#>", "<img src='cid:" + mi.ContentId + "'>");

                // Set the HTML body (use HtmlBody so clients render HTML).
                bodyBuilder.HtmlBody = message.Content;

                // Attach the composed body to the email message.
                emailMessage.Body = bodyBuilder.ToMessageBody();
            }
            catch (Exception ex)
            {
                // Log any errors that occur during message composition.
                _logger.LogError(ex.Message, ex);
            }
            return emailMessage;
        }

        /// <summary>
        /// Connects to the SMTP server and sends the provided <see cref="MimeMessage"/>.
        /// Uses configuration values for server, port, SSL and optional credentials.
        /// </summary>
        /// <param name="mailMessage">The MIME message to send.</param>
        /// <returns>True if send succeeded; otherwise false.</returns>
        private async Task<bool> SendAsync(MimeMessage mailMessage)
        {
            bool sentOk = false;
            using (var client = new SmtpClient())
            {
                try
                {
                    // Use custom certificate validation callback to provide detailed logging and control.
                    client.ServerCertificateValidationCallback = MySslCertificateValidationCallback;

                    // Connect to SMTP server using configured host/port/ssl settings.
                    await client.ConnectAsync(_emailConfig.SmtpServer, _emailConfig.Port, _emailConfig.SslConnection);

                    // Authenticate only when credentials are present.
                    if (!string.IsNullOrEmpty(_emailConfig.UserName) && !string.IsNullOrEmpty(_emailConfig.Password))
                    {
                        await client.AuthenticateAsync(_emailConfig.UserName, _emailConfig.Password);
                    }

                    // Send the email.
                    await client.SendAsync(mailMessage);

                    sentOk = true;
                }
                catch (Exception ex)
                {
                    // Log errors during connect/auth/send operations.
                    _logger.LogError(ex.Message, ex);
                    sentOk = false;
                }
                finally
                {
                    // Gracefully disconnect and dispose the client.
                    await client.DisconnectAsync(true);
                    client.Dispose();
                }
            }

            return sentOk;
        }

        /// <summary>
        /// Custom SSL certificate validation callback used by MailKit's <see cref="SmtpClient"/>.
        /// Provides verbose console output for certificate chain errors and allows fine-grained
        /// handling of name mismatches or missing certificates.
        /// </summary>
        /// <remarks>
        /// Returning true accepts the certificate; returning false causes the TLS handshake to fail.
        /// This implementation returns true for valid certificates and logs details for chain errors
        /// while still returning true at the end — adapt this behavior if stricter validation is required.
        /// </remarks>
        static bool MySslCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {

            // If there are no errors, then everything went smoothly.
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            // Note: MailKit will always pass the host name string as the `sender` argument.
            var host = (string)sender;

            if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateNotAvailable) != 0)
            {
                // The remote certificate was unavailable.
                Console.WriteLine("The SSL certificate was not available for {0}", host);
                return false;
            }

            if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateNameMismatch) != 0)
            {
                // Server certificate common name did not match the host we connected to.
                var certificate2 = certificate as X509Certificate2;
                var cn = certificate2 != null ? certificate2.GetNameInfo(X509NameType.SimpleName, false) : certificate.Subject;

                Console.WriteLine("The Common Name for the SSL certificate did not match {0}. Instead, it was {1}.", host, cn);
                return false;
            }

            // The remaining errors are chain errors; print details for diagnostics.
            Console.WriteLine("The SSL certificate for the server could not be validated for the following reasons:");

            // The first element's certificate will be the server's SSL certificate.
            foreach (var element in chain.ChainElements)
            {
                // If no status entries, skip.
                if (element.ChainElementStatus.Length == 0)
                    continue;

                Console.WriteLine("\u2022 {0}", element.Certificate.Subject);
                foreach (var error in element.ChainElementStatus)
                {
                    // `error.StatusInformation` contains a human-readable description of the issue.
                    Console.WriteLine("\t\u2022 {0}", error.StatusInformation);
                }
            }

            // NOTE: This returns true even when chain errors are present. Keep or change this behavior
            // depending on whether you want to accept untrusted/self-signed certificates.
            return true;
        }


    }

}

