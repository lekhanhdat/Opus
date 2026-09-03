using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.I18N.Core;
using Newtonsoft.Json;
using System;
using System.Text;

namespace AvePoint.RA.Contract.RMWeb.JobMonitor
{
    public class JMArchiverJobProgressDetails : JMMainJobDetails
    {
        private const long OneKB = 1024;
        private const long OneMB = OneKB * 1024;
        private const long OneGB = OneMB * 1024;

        private static readonly object _locker = new object();

        public JobType JobType { get; set; }
        [JsonProperty(PropertyName = "JobStatus")]
        public ProgressStatus ProgressStatus { get; set; } = ProgressStatus.Pending;
        [JsonIgnore]
        public DateTime StartTime { get; set; }
        [JsonIgnore]
        public DateTime FinishTime { get; set; }
        [JsonIgnore]
        public DateTime LastUpdatedTime { get; set; }

        [JsonIgnore]
        public long TotalFiles { get; set; } = 0; // Total files of all lists in this site collection

        [JsonIgnore]
        public long TotalMatchedRuleFilesForExport { get; set; } = 0; // Total files matched with rules for export
        [JsonIgnore]
        public long TotalMatchedRuleFilesForArchive { get; set; } = 0; // Total files matched with rules for archive
        [JsonIgnore]
        public long TotalMatchedRuleFilesForOtherActions { get; set; } = 0; // Total files matched with rules for other actions

        [JsonIgnore]
        public ProcessedItemsInfoDto ProcessedScannedItemsInfo { get; set; } = new ProcessedItemsInfoDto() { Action = ActionTab.Scan };
        [JsonIgnore]
        public ProcessedItemsInfoDto ProcessedExportedItemsInfo { get; set; } = new ProcessedItemsInfoDto() { Action = ActionTab.Export };
        [JsonIgnore]
        public ProcessedItemsInfoDto ProcessedArchivedItemsInfo { get; set; } = new ProcessedItemsInfoDto() { Action = ActionTab.Backup };
        [JsonIgnore]
        public ProcessedItemsInfoDto ProcessedOtherItemsInfo { get; set; } = new ProcessedItemsInfoDto() { Action = ActionTab.Action };

        #region Returned Values
        [JsonProperty(PropertyName = nameof(StartTime))]
        public string StartTimeStr { get; set; }
        [JsonProperty(PropertyName = nameof(FinishTime))]
        public string FinishTimeStr { get; set; }
        [JsonProperty(PropertyName = nameof(LastUpdatedTime))]
        public string LastUpdatedTimeStr { get; set; }
        [JsonProperty(PropertyName = "EstimatedScanFinishedTime")]
        public string EstimatedScanFinishedTimeStr { get; set; }
        [JsonProperty(PropertyName = "EstimatedExportFinishedTime")]
        public string EstimatedExportFinishedTimeStr { get; set; }
        [JsonProperty(PropertyName = "EstimatedArchiveFinishedTime")]
        public string EstimatedArchivedFinishedTimeStr { get; set; }
        [JsonProperty(PropertyName = "EstimatedOtherFinishedTime")]
        public string EstimatedOtherFinishedTimeStr { get; set; }
        #endregion

        #region Scan statistics
        [JsonIgnore]
        public DateTime StartScanTime { get; set; }
        public string ScannedFiles => ProcessedScannedItemsInfo.IsEmpty() ? string.Empty : ConvertToTotalString(ActionTab.Scan);
        [JsonIgnore]
        public DateTime EstimatedScanFinishedTime { get; set; }
        #endregion

        #region Export statistics
        [JsonIgnore]
        public DateTime StartExportTime { get; set; }
        public string ExportedFiles => ProcessedExportedItemsInfo.IsEmpty() ? string.Empty : ConvertToTotalString(ActionTab.Export);
        [JsonIgnore]
        public DateTime EstimatedExportFinishedTime { get; set; }
        #endregion

        #region Archived statistics
        [JsonIgnore]
        public DateTime StartArchivedTime { get; set; }
        public string ArchivedFiles => ProcessedArchivedItemsInfo.IsEmpty() ? string.Empty : ConvertToTotalString(ActionTab.Backup);
        public string ArchivedSize => ProcessedArchivedItemsInfo.IsEmpty() ? string.Empty : FormatSize(ProcessedArchivedItemsInfo.ItemSize + ProcessedArchivedItemsInfo.TotalSize);
        [JsonIgnore]
        public DateTime EstimatedArchivedFinishedTime { get; set; }
        #endregion

        #region Other statistics
        [JsonIgnore]
        public DateTime StartOtherTime { get; set; }
        public string OtherActions => ProcessedOtherItemsInfo.IsEmpty() ? string.Empty : ConvertToTotalString(ActionTab.Action);
        [JsonIgnore]
        public DateTime EstimatedOtherFinishedTime { get; set; }
        #endregion

        #region Increase file count and size
        public void IncreaseScannedFiles(int scannedFiles = 1)
        {
            lock (_locker)
            {
                ProcessedScannedItemsInfo.ItemCount += scannedFiles;
                CalculateEstimatedScanFinishedTime();
            }
        }

        public void IncreaseExportedFiles(long fileSize)
        {
            lock (_locker)
            {
                ProcessedExportedItemsInfo.ItemCount++;
                ProcessedExportedItemsInfo.ItemSize += fileSize;
                CalculateEstimatedExportFinishedTime();
            }
        }

        public void IncreaseArchivedFiles(long fileSize)
        {
            lock (_locker)
            {
                ProcessedArchivedItemsInfo.ItemCount++;
                ProcessedArchivedItemsInfo.ItemSize += fileSize;
                CalculateEstimatedArchivedFinishedTime();
            }
        }

        public void IncreaseOtherActions()
        {
            lock (_locker)
            {
                ProcessedOtherItemsInfo.ItemCount++;
                CalculateEstimatedOtherFinishedTime();
            }
        }

        public void IncreaseOtherItems(ActionTab action, int cacheNodeType, long fileSize)
        {
            lock (_locker)
            {
                var nodeType = GetCacheNodeType(cacheNodeType);
                if (nodeType == CacheNodeType.Item || nodeType == CacheNodeType.ItemVersion || nodeType == CacheNodeType.Attachment
                    || nodeType == CacheNodeType.HSMItem || nodeType == CacheNodeType.HSMItemVersion)
                {
                    return;
                }
                ProcessedItemsInfoDto processedItemsInfo = action switch
                {
                    ActionTab.Scan => this.ProcessedScannedItemsInfo,
                    ActionTab.Export => this.ProcessedExportedItemsInfo,
                    ActionTab.Backup => this.ProcessedArchivedItemsInfo,
                    ActionTab.Action => this.ProcessedOtherItemsInfo,
                    _ => throw new ArgumentOutOfRangeException(nameof(action), $"Not expected action value: {action}"),
                };
                switch (nodeType)
                {
                    case CacheNodeType.SiteCollection:
                        processedItemsInfo.SiteCollectionCount++;
                        break;
                    case CacheNodeType.Web:
                        processedItemsInfo.SiteCount++;
                        break;
                    case CacheNodeType.List:
                        processedItemsInfo.ListCount++;
                        break;
                    case CacheNodeType.Folder:
                        processedItemsInfo.FolderCount++;
                        break;
                    default:
                        processedItemsInfo.ItemCount++;
                        break;
                }
                processedItemsInfo.TotalSize += fileSize;
            }
        }
        #endregion

        #region Calculate estimated finished time
        public void RecalculateCurrentStageEstimatedFinishedTime()
        {
            lock (_locker)
            {
                switch (ProgressStatus)
                {
                    case ProgressStatus.Scan:
                        CalculateEstimatedScanFinishedTime();
                        break;
                    case ProgressStatus.Export:
                        CalculateEstimatedExportFinishedTime();
                        break;
                    case ProgressStatus.Archive:
                        CalculateEstimatedArchivedFinishedTime();
                        break;
                    case ProgressStatus.Others:
                        CalculateEstimatedOtherFinishedTime();
                        break;
                    default:
                        break;
                }
            }
        }

        public void CalculateEstimatedScanFinishedTime()
        {
            if (StartScanTime == DateTime.MinValue || ProcessedScannedItemsInfo.ItemCount == 0)
            {
                EstimatedScanFinishedTime = DateTime.MinValue;
                return;
            }
            var scanSpeed = (double)ProcessedScannedItemsInfo.ItemCount / (DateTime.UtcNow - StartScanTime).TotalSeconds;
            var executionTime = TotalFiles / scanSpeed;
            EstimatedScanFinishedTime = StartScanTime.AddSeconds(executionTime);
        }

        public void CalculateEstimatedExportFinishedTime()
        {
            if (StartExportTime == DateTime.MinValue || ProcessedExportedItemsInfo.ItemCount == 0)
            {
                EstimatedExportFinishedTime = DateTime.MinValue;
                return;
            }
            var exportSpeed = (double)ProcessedExportedItemsInfo.ItemCount / (DateTime.UtcNow - StartExportTime).TotalSeconds;
            var executionTime = TotalMatchedRuleFilesForExport / exportSpeed;
            EstimatedExportFinishedTime = StartExportTime.AddSeconds(executionTime);
        }

        public void CalculateEstimatedArchivedFinishedTime()
        {
            if (StartArchivedTime == DateTime.MinValue || ProcessedArchivedItemsInfo.ItemCount == 0)
            {
                EstimatedArchivedFinishedTime = DateTime.MinValue;
                return;
            }
            var archivedSpeed = (double)ProcessedArchivedItemsInfo.ItemCount / (DateTime.UtcNow - StartArchivedTime).TotalSeconds;
            var executionTime = TotalMatchedRuleFilesForArchive / archivedSpeed;
            EstimatedArchivedFinishedTime = StartArchivedTime.AddSeconds(executionTime);
        }

        public void CalculateEstimatedOtherFinishedTime()
        {
            if (StartOtherTime == DateTime.MinValue || ProcessedOtherItemsInfo.ItemCount == 0)
            {
                EstimatedOtherFinishedTime = DateTime.MinValue;
                return;
            }
            var otherSpeed = (double)ProcessedOtherItemsInfo.ItemCount / (DateTime.UtcNow - StartOtherTime).TotalSeconds;
            var executionTime = TotalMatchedRuleFilesForOtherActions / otherSpeed;
            EstimatedOtherFinishedTime = StartOtherTime.AddSeconds(executionTime);
        }
        #endregion

        private string FormatSize(long bytes)
        {
            if (bytes < OneKB) return $"{bytes} B";
            if (bytes < OneMB) return $"{bytes / (double)OneKB:F3} KB";
            if (bytes < OneGB) return $"{bytes / (double)OneMB:F3} MB";
            return $"{bytes / (double)OneGB:F3} GB";
        }

        private string ConvertToTotalString(ActionTab action)
        {
            ProcessedItemsInfoDto processedItemsInfo = action switch
            {
                ActionTab.Scan => this.ProcessedScannedItemsInfo,
                ActionTab.Export => this.ProcessedExportedItemsInfo,
                ActionTab.Backup => this.ProcessedArchivedItemsInfo,
                ActionTab.Action => this.ProcessedOtherItemsInfo,
                _ => throw new ArgumentOutOfRangeException(nameof(action), $"Not expected action value: {action}"),
            };
            var builder = new StringBuilder();
            long totalProcessedItems = 0;
            switch (action)
            {
                case ActionTab.Scan:
                    totalProcessedItems = TotalFiles + processedItemsInfo.GetAdditionalProcessedItems();
                    break;
                case ActionTab.Export:
                    totalProcessedItems =  TotalMatchedRuleFilesForExport + processedItemsInfo.GetAdditionalProcessedItems();
                    break;
                case ActionTab.Backup:
                    totalProcessedItems = TotalMatchedRuleFilesForArchive + processedItemsInfo.GetAdditionalProcessedItems();
                    break;
                case ActionTab.Action:
                    totalProcessedItems = TotalMatchedRuleFilesForOtherActions + processedItemsInfo.GetAdditionalProcessedItems();
                    break;
            }
            builder.Append($"{processedItemsInfo.GetTotalProcessedItems()} / {totalProcessedItems}");
            return builder.ToString();
        }

        private CacheNodeType GetCacheNodeType(int cacheNodeType)
        {
            CacheNodeType nodeType = CacheNodeType.Item;
            if (cacheNodeType == (int)CacheNodeType.Exception)
            {
                nodeType = CacheNodeType.Exception;
            }
            else if (cacheNodeType == (int)CacheNodeType.HSMItem)
            {
                nodeType = CacheNodeType.HSMItem;
            }
            else if (cacheNodeType == (int)CacheNodeType.HSMItemVersion)
            {
                nodeType = CacheNodeType.HSMItemVersion;
            }
            else if (cacheNodeType == (int)CacheNodeType.ArchiveBy365Item)
            {
                nodeType = CacheNodeType.ArchiveBy365Item;
            }
            else if (cacheNodeType > (int)CacheNodeType.ItemVersion)
            {
                nodeType = CacheNodeType.Attachment;
            }
            else if (cacheNodeType > (int)CacheNodeType.Item)
            {
                nodeType = CacheNodeType.ItemVersion;
            }
            else if (cacheNodeType == (int)CacheNodeType.Item)
            {
                nodeType = CacheNodeType.Item;
            }
            else if (cacheNodeType > (int)CacheNodeType.List)
            {
                nodeType = CacheNodeType.Folder;
            }
            else if (cacheNodeType == (int)CacheNodeType.List)
            {
                nodeType = CacheNodeType.List;
            }
            else if (cacheNodeType >= (int)CacheNodeType.Web)
            {
                if (cacheNodeType == (int)CacheNodeType.APP)
                {
                    nodeType = CacheNodeType.APP;
                }
                else
                {
                    nodeType = CacheNodeType.Web;
                }
            }
            else if (cacheNodeType == (int)CacheNodeType.SiteCollection)
            {
                nodeType = CacheNodeType.SiteCollection;
            }
            return nodeType;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine("JMArchiverJobProgressDetails {");
            builder.AppendLine($"ProgressStatus: {ProgressStatus},");
            builder.AppendLine($"TotalMatchedRuleFilesForExport: {TotalMatchedRuleFilesForExport},");
            builder.AppendLine($"TotalMatchedRuleFilesForArchive: {TotalMatchedRuleFilesForArchive},");
            builder.AppendLine($"TotalMatchedRuleFilesForOtherActions: {TotalMatchedRuleFilesForOtherActions},");
            builder.AppendLine($"ProcessedScannedItemsInfo: {ProcessedScannedItemsInfo},");
            builder.AppendLine($"ProcessedExportedItemsInfo: {ProcessedExportedItemsInfo},");
            builder.AppendLine($"ProcessedArchivedItemsInfo: {ProcessedArchivedItemsInfo},");
            builder.AppendLine($"ProcessedOtherItemsInfo: {ProcessedOtherItemsInfo},");
            builder.Append('}');
            return builder.ToString();
        }
    }

    public record ProcessedItemsInfoDto
    {
        public ActionTab Action { get; set; }
        public int SiteCollectionCount { get; set; } = 0;
        public int SiteCount { get; set; } = 0;
        public int ListCount { get; set; } = 0;
        public int FolderCount { get; set; } = 0;
        public long TotalSize { get; set; } = 0; // Total size of all items, in bytes, excluding the size of the items/item versions level

        public int ItemCount { get; set; } = 0;
        public long ItemSize { get; set; } = 0;

        public long GetTotalProcessedItems()
        {
            return GetAdditionalProcessedItems() + ItemCount;
        }

        public long GetAdditionalProcessedItems()
        {
            return SiteCollectionCount + SiteCount + ListCount + FolderCount;
        }

        public bool IsEmpty()
        {
            return SiteCollectionCount == 0 && SiteCount == 0 && ListCount == 0 && FolderCount == 0 && ItemCount == 0;
        }

        public override string ToString()
        {
            return $"{{ Action: {Action}, SiteCollectionCount: {SiteCollectionCount}, SiteCount: {SiteCount}, ListCount: {ListCount}, FolderCount: {FolderCount}, TotalSize: {TotalSize}, ItemCount: {ItemCount}, ItemSize: {ItemSize} }}";
        }
    }
}
