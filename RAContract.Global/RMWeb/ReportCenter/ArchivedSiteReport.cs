using System;

namespace AvePoint.RA.Contract.RMWeb.ReportCenter
{
    public class ArchivedSiteReport : BaseReport
    {
        public string Type { get; set; }
        public string SourceUrl { get; set; }
        public double ArchivedDataSize { get; set; }
        public long ArchivedTime { get; set; }
        public string ArchivedTimeString { get; set; }
    }
}
