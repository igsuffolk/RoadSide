using ClassLibrary1.DataContext;
using ClassLibrary1.Interfaces;
using Microsoft.AspNetCore.Hosting;
using SharedProject1.Models;
using SharedProject1.Models.Email;

namespace ClassLibrary1.Services
{
    public class HomeService : IHomeService
    {
        private readonly IEmailService _emailService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ReportDbContext _reportDbContext;

        public HomeService(IWebHostEnvironment webHostEnvironment, IEmailService emailService, ReportDbContext reportDbContext)
        {
            _webHostEnvironment = webHostEnvironment;
            _emailService = emailService;
            _reportDbContext = reportDbContext;
        }

        public async Task<bool> NewReport(RoadReportDTO model)
        {
            string fileName = string.Empty;

            EmailMessage message = new()
            {
                To = new List<string> { "iaingrant80@googlemail.com" },
                Subject = "Roadside Report"
            };

            string templateFile = System.IO.File.ReadAllText(Path.Combine(_webHostEnvironment.WebRootPath, "templates", "report.html"));

            templateFile = templateFile.Replace("<#ReportDate#>", model.ReportDate.ToString("dd/MMM/yyyy"));
            templateFile = templateFile.Replace("<#ReportBy#>", model.ReportedBy);
            templateFile = templateFile.Replace("<#RoadName#>", model.RoadName);
            templateFile = templateFile.Replace("<#Description#>", model.Description);
            templateFile = templateFile.Replace("<#Latitude#>", model.Latitude.ToString());
            templateFile = templateFile.Replace("<#Longitude#>", model.Longitude.ToString());

            message.Content = templateFile;

            byte[] photo = null;
            if (model.Photo != null)
            {
                fileName = Guid.NewGuid().ToString() + ".jpg";
                message.Attachments = new List<string> { fileName };

                photo = model.Photo;

                System.IO.File.WriteAllBytes(Path.Combine(_webHostEnvironment.WebRootPath, "ReportPhotos", fileName), photo);
            }

            bool result = await _emailService.SendEmailAsync(message);
            if (result)
            {
                // insert into dnb if email success - otherwise user will resend
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
                _reportDbContext.SaveChanges();

            }
            else
            {
                // send failed remove photo
                if (photo != null)
                {
                    System.IO.File.Delete(Path.Combine(_webHostEnvironment.WebRootPath, "ReportPhotos", fileName));
                }

            }

            return result;
        }


    }
}
