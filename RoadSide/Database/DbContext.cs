
using IndexedDB.Blazor;
using Microsoft.JSInterop;
using SharedProject1.Models;

namespace RoadSide.Database
{
    public class Roadside : IndexedDb
    {
        #region constructor
        public Roadside(IJSRuntime jSRuntime, string name, int version) : base(jSRuntime, name, version) { }
        #endregion

        #region properties
        public IndexedSet<RoadReportDTO> RoadReports { get; set; }
        #endregion

      
    }
}
