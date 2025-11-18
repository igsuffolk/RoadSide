using SharedProject.Models;

namespace ApiClassLibrary.Interfaces
{
    public interface IHomeService
    {
        Task<bool> NewReport(RoadReportDTO model);
    }
}