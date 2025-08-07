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

        public HomeService(IWebHostEnvironment webHostEnvironment, IEmailService emailService)
        {
            _webHostEnvironment = webHostEnvironment;
            _emailService = emailService;
        }

        public async Task<bool> NewReport(RoadReport model)
        {
            string fileName = Guid.NewGuid().ToString() + ".jpg";

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

            if (model.Photo != null)
            {
                message.Attachments = new List<string> { fileName };

                byte[] photo = model.Photo;

                System.IO.File.WriteAllBytes(Path.Combine(_webHostEnvironment.WebRootPath, "temp", fileName), photo);
            }

            bool result = await _emailService.SendEmailAsync(message);

            return result;
        }


    }
}
