using AvePoint.RA.Contract.Myhub.Model.QueryRequest.Views;
using AvePoint.RA.Contract.RMWeb.Dashboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Myhub.Model
{
    public class FSDashboard
    {
        public StorageStats Storage { get; set; } = new StorageStats();
        public List<FileTypeStats> FileTypes { get; set; } = [];
        public List<CreatorStats> Creators { get; set; } = [];
        public List<ClassCodeStats> ClassCodes { get; set; } = [];
        public long ClassCodesTotal { get; set; }
        public List<RecordStats> LineChartData { get; set; } = [];
        public StatusSummary FolderStatusSummary { get; set; }
        public StatusSummary FileStatusSummary { get; set; }
        public List<DestroyedStats> DestroyedStats { get; set; }
    }

    public class StorageStats
    {
        public long TotalSize { get; set; }
        public long Size { get; set; }
        public long FileCount { get; set; }
    }

    public class FileTypeStats
    {
        public string Ext { get; set; }
        public long FileCount { get; set; }
        public long FileSize { get; set; }
    }

    public class CreatorStats
    {
        public string Creator { get; set; }
        public long FileCount { get; set; }
    }

    public class ClassCodeStats
    {
        public string ClassCodeId { get; set; }
        public string ClassCodeName { get; set; }
        public long Usage { get; set; }
    }

    public class RecordStats
    {
        public long Date { get; set; }
        public long Created { get; set; }
        public long Modified { get; set; }
        public long Accessed { get; set; }
    }

    public class DestroyedStats
    {
        public long Date { get; set; }
        public long Destroyed { get; set; }
    }

    /// <summary>
    /// Aggregated record counts by RMRecordStatus, scoped to either folders or files.
    /// Total = all records under the node regardless of status.
    /// </summary>
    public class StatusSummary
    {
        public long Active { get; set; }
        public long Destroyed { get; set; }
        public long Total { get; set; }
    }

    public class FSDashboardInformation
    {
        public StorageStats Storage { get; set; } = new StorageStats();
        public List<FileTypeStats> FileTypes { get; set; } = [];
        public List<CreatorStats> Creators { get; set; } = [];
        public List<ClassCodeStats> ClassCodes { get; set; } = [];
        public long ClassCodesTotal { get; set; }
        public List<LineChartData> LineChartDatas { get; set; }
    }

    public class LineChartData
    {
        public string Date { get; set; }
        public long Created { get; set; }
        public long Modified { get; set; }
        public long Accessed { get; set; }
    }
}
