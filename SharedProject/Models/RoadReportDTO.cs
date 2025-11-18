
using System.ComponentModel.DataAnnotations;

namespace SharedProject.Models
{
    public class RoadReportDTO
    {
        [System.ComponentModel.DataAnnotations.Key]
        public int Id { get; set; }

        [Required]
        [StringLength(500)]
        public string ReportedBy { get; set; } = string.Empty;

        public DateTime ReportDate { get; set; } = DateTime.Now;

        public double? Latitude { get; set; } = 0;

        public double? Longitude { get; set; } = 0;

        [Required]
        [StringLength(2000)]
        public string RoadName {  get; set; } = string.Empty;

        [Required]
        [StringLength(200)]

        public string Description { get; set; }=string.Empty;

        public byte[]? Photo { get; set; }

    }
}
