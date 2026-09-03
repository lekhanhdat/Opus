using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvePoint.RA.DB.Model
{
    public class RMJobProgress : BaseModel
    {
        [Key]
        public string SubJobID { get; set; } = null!;

        public int JobType { get; set; }

        [Index]
        public int Status { get; set; }

        [Index]
        public int ProgressStatus { get; set; }

        [Index]
        [MaxLength(1024)]
        public string Scope { get; set; } = string.Empty;

        public string Comment { get; set; } = string.Empty;

        public bool IsSavedJobDetails { get; set; } = false;

        public long Successful { get; set; } = 0;

        public long Failed { get; set; } = 0;

        public long Skipped { get; set; } = 0;

        [Index]
        public long StartTime { get; set; } = 0;

        public long FinishTime { get; set; } = 0;

        public long LastUpdatedTime { get; set; } = 0;

        public long TotalFiles { get; set; } = 0;

        public long TotalMatchedRuleFilesForExport { get; set; } = 0;

        public long TotalMatchedRuleFilesForArchive { get; set; } = 0;

        public long TotalMatchedRuleFilesForOtherActions { get; set; } = 0;

        public string ProcessedItemsInfos { get; set; } = string.Empty;

        public long StartScanTime { get; set; } = 0;

        public long EstimatedScanFinishedTime { get; set; } = 0;

        public long StartExportTime { get; set; } = 0;

        public long EstimatedExportFinishedTime { get; set; } = 0;

        public long StartArchivedTime { get; set; } = 0;

        public long EstimatedArchivedFinishedTime { get; set; } = 0;

        public long StartOtherTime { get; set; } = 0;

        public long TotalOtherActions { get; set; } = 0;

        public long EstimatedOtherFinishedTime { get; set; } = 0;
    }
}
