using ApiClassLibrary.DataContext;
using ApiClassLibrary.Interfaces;
using Microsoft.AspNetCore.Hosting;
using SharedProject.Models;
using SharedProject.Models.Email;

namespace ApiClassLibrary.Services
{
    /// <summary>
    /// Handles higher-level application operations related to the home/reporting feature.
    /// Responsible for composing report emails, saving uploaded photos to disk and inserting
    /// successful reports into the report database.
    /// </summary>
    public class HomeService : IHomeService
    {
        private readonly IEmailService _emailService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ReportDbContext _reportDbContext;

        /// <summary>
        /// Constructs the service with required dependencies.
        /// </summary>
        /// <param name="webHostEnvironment">Used to resolve web root paths for templates, images and temporary files.</param>
        /// <param name="emailService">Email sending abstraction used to dispatch report emails.</param>
        /// <param name="reportDbContext">EF Core DbContext used to persist successful reports.</param>
        public HomeService(IWebHostEnvironment webHostEnvironment, IEmailService emailService, ReportDbContext reportDbContext)
        {
            _webHostEnvironment = webHostEnvironment;
            _emailService = emailService;
            _reportDbContext = reportDbContext;
        }

        /// <summary>
        /// Creates a report email from the provided DTO, saves an attached photo (if present)
        /// to the web root, sends the email and, only on successful send, persists the report
        /// record to the database. If email sending fails any saved photo is removed so the
        /// file system remains consistent.
        /// </summary>
        /// <param name="model">Report data transfer object containing report fields and optional photo bytes.</param>
        /// <returns>True when the email was sent successfully and the report persisted; false otherwise.</returns>
        public async Task<bool> NewReport(RoadReportDTO model)
        {
            // File name for any saved photo (empty when none).
            string fileName = string.Empty;

            // Compose the email message (To and Subject can be moved to configuration).
            EmailMessage message = new()
            {
                To = new List<string> { "iaingrant80@googlemail.com" },
                Subject = "Roadside Report"
            };

            // Read the HTML template from wwwroot/templates/report.html
            string templateFile = System.IO.File.ReadAllText(Path.Combine(_webHostEnvironment.WebRootPath, "templates", "report.html"));

            // Replace placeholders in the template with values from the DTO.
            templateFile = templateFile.Replace("<#ReportDate#>", model.ReportDate.ToString("dd/MMM/yyyy"));
            templateFile = templateFile.Replace("<#ReportBy#>", model.ReportedBy);
            templateFile = templateFile.Replace("<#RoadName#>", model.RoadName);
            templateFile = templateFile.Replace("<#Description#>", model.Description);
            templateFile = templateFile.Replace("<#Latitude#>", model.Latitude.ToString());
            templateFile = templateFile.Replace("<#Longitude#>", model.Longitude.ToString());

            message.Content = templateFile;

            // If a photo was supplied, generate a unique file name, add it as an attachment
            // and persist the bytes to wwwroot/ReportPhotos so the EmailService can pick it up.
            byte[] photo;
            if (model.Photo != null)
            {
                fileName = Guid.NewGuid().ToString() + ".jpg";
                message.Attachments = new List<string> { fileName };

                photo = model.Photo;

                System.IO.File.WriteAllBytes(Path.Combine(_webHostEnvironment.WebRootPath, "ReportPhotos", fileName), photo);
            }

            // Send the email via the injected email service.
            bool result = await _emailService.SendEmailAsync(message);
            if (result)
            {
                // If the email was sent successfully, create and persist the RoadReport entity.
                RoadReport roadReport = new()
                {
                    ReportDate = model.ReportDate,
                    ReportedBy = model.ReportedBy,
                    RoadName = model.RoadName,
                    Description = model.Description,
                    Latitude = model.Latitude,
                    Longitude = model.Longitude,
                    PhotoFileName = fileName
                };

                _reportDbContext.Add(roadReport);
                // SaveChanges is synchronous here; consider using SaveChangesAsync in async flows.
                _reportDbContext.SaveChanges();
            }
            else
            {
                // On failure to send, remove any saved photo to avoid orphan files.
                if (model.Photo != null)
                {
                    System.IO.File.Delete(Path.Combine(_webHostEnvironment.WebRootPath, "ReportPhotos", fileName));
                }
            }

            return result;
        }
    }
}
