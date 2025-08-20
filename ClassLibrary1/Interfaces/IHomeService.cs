using SharedProject1.Models;

namespace ClassLibrary1.Interfaces
{
    public interface IHomeService
    {
        Task<bool> NewReport(RoadReportDTO model);
    }
}