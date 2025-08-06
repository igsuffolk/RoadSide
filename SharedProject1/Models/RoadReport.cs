
namespace SharedProject1.Models
{
    public class RoadReport
    {
        [System.ComponentModel.DataAnnotations.Key]
        public int Id { get; set; }
        public string ReportedBy { get; set; } = string.Empty;
        public DateTime ReportDate { get; set; } = DateTime.Now;
        public double? Latitude { get; set; } = 0;
        public double? Longitude { get; set; } = 0;
        public string RoadName {  get; set; } = string.Empty;
        public string Description { get; set; }=string.Empty;

        public byte[]? Photo { get; set; }

    }
}
