using AvePoint.GCommon.Contract.Compliance.eDiscovery.Object;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.ManualApproval;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.Myhub.Model;
using AvePoint.RA.Contract.Myhub.Model.QueryRequest.Views;
using AvePoint.RA.Contract.MyHub;
using AvePoint.RA.Contract.RMPublicAPI.JPMC;
using AvePoint.RA.Contract.RMPublicAPI.JPMC.Model;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Service.Services.ManualApproval;
using AvePoint.Records.Core.Utilities.Extensions;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.RMPublicAPI
{
    public class RetirveDataServices : IRetriveDataServices
    {
        RALogger logger = new RALogger(MethodBase.GetCurrentMethod().DeclaringType);

        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();
        private IJobMonitorDao JobMonitorDao => PlatformWindsorManager.GetService<IJobMonitorDao>();
        private IRMMyhubServices RMMyhubServices => PlatformWindsorManager.GetService<IRMMyhubServices>();
        private IRMManualApprovalService ManualApprovalService => PlatformWindsorManager.GetService<IRMManualApprovalService>();

        private readonly List<JobType> _jobTypesCanRetrive = new List<JobType>
        { 
            JobType.FSDataSynchronization,
            JobType.FSDisposal,
            JobType.FSDisposalByClassCode,
            JobType.FSDisposalSchedule,
            JobType.FSDataSynchronizationSchedule,
            JobType.ApplyClassCode,
            JobType.FSMyHubDashboard,
            JobType.DownloadRCCReport,
        };

        private readonly List<JobStatus> _jobStatusCanRetrive = new List<JobStatus>
        {
            JobStatus.Finished,
            JobStatus.Failed,
            JobStatus.FinishWithException,
            JobStatus.Skipped,
            JobStatus.Stopped
        };
        private IFSMyHubDashboardDao FSMyHubDashboardDao => PlatformWindsorManager.GetService<IFSMyHubDashboardDao>();

        private IExplorerDao _explorerDao;
        private IExplorerDao ExplorerDao
        {
            get
            {
                if (_explorerDao == null)
                {
                    _explorerDao = new ExplorerDao();
                }
                return _explorerDao;
            }
        }

        private const int PageSize = 15;
        public async Task<JobReportResult> GetJobReportAsync(JobReportParam param)
        {
            var filter = BuildFilter(param);

            var (entities, totalCount) = await JobMonitorDao.GetJobReportsAsync(
                filter,
                param.Page,
                param.PageSize);
            if(entities == null || !entities.Any())
            {
                logger.Warn($"No job records found for the given filter: {JsonConvert.SerializeObject(param)}");
                return null;
            }
            return new JobReportResult
            {
                TotalCount = totalCount,
                Items = entities.Select(x => new JobReportItem
                {
                    JobId = x.Id,
                    JobType = (JobType)x.JobType,
                    Status = (JobStatus)x.Status,
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    Duration = (long)TimeSpan.FromTicks(x.EndTime - x.StartTime).TotalSeconds
                }).ToList()
            };
        }

        public async Task<string> GetJobDetails(JMDetailsQuery queryModel)
        {
            var job = JobMonitorDao.GetJobById(queryModel.JobID);
            if (job == null)
            {
                logger.Warn($"Job not found for JobID: {queryModel.JobID}");
                return null;
            }
            if (_jobTypesCanRetrive.Contains((JobType)queryModel.JobType))
            {
                queryModel.StatusFilters = Array.Empty<JobDetailsStatus>();
                var result = await JobMonitorService.GetJobDetailsAsync(queryModel);

                try
                {
                    var jObject = JsonConvert.DeserializeObject<JObject>(result);
                    if (jObject != null && jObject["Details"] is JArray detailsArray)
                    {
                        foreach (var detail in detailsArray)
                        {
                            if (detail["FinishTime"] != null && detail["FinishTime"].Type == JTokenType.String)
                            {
                                var originalFinishTime = detail["FinishTime"].Value<string>();

                                try
                                {
                                    var match = Regex.Match(originalFinishTime, @"^(.+?)\s*\(UTC\s*([+-]\d{2}:\d{2})?\s*\)$");

                                    if (match.Success)
                                    {
                                        string dateTimePart = match.Groups[1].Value.Trim();
                                        string offsetPart = match.Groups[2].Success ? match.Groups[2].Value : "+00:00";

                                        string cleanString = $"{dateTimePart} {offsetPart}";

                                        string[] expectedFormats = { "M-d-yyyy HH:mm:ss zzz", "MM-dd-yyyy HH:mm:ss zzz", "yyyy-MM-dd HH:mm:ss zzz" };

                                        if (DateTimeOffset.TryParseExact(cleanString, expectedFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTimeOffset parsedDateTimeOffset))
                                        {
                                            long ticks = parsedDateTimeOffset.UtcDateTime.Ticks;
                                            detail["FinishTime"] = ticks;
                                        }
                                        else
                                        {
                                            logger.Warn($"Failed to parse normalized FinishTime: {cleanString}. Keeping original value.");
                                        }
                                    }
                                    else
                                    {
                                        logger.Warn($"Format not matched for FinishTime: {originalFinishTime}. Keeping original value.");
                                    }
                                }
                                catch (Exception innerEx)
                                {
                                    logger.Warn($"Error processing FinishTime: {innerEx.Message}. Keeping original value: {originalFinishTime}");
                                }
                            }
                        }
                    }
                    return JsonConvert.SerializeObject(jObject);
                }
                catch (Exception ex)
                {
                    logger.Warn($"Failed to convert FinishTime format: {ex.Message}. Returning original result.");
                    return result;
                }
            }
            return null;
        }


        public async Task<FSMetadata> GetFSMetadataAsync(FSMetadataParam param)
        {
            logger.Info($"Getting FS metadata for path: {param.FullPath}");
            var nodeData = await FSMyHubDashboardDao.GetByFullPathAsync(param.FullPath);

            if (nodeData == null || string.IsNullOrEmpty(nodeData.MetaData))
            {
                return null;
            }
            var fsMetaData = JsonConvert.DeserializeObject<FSDashboard>(nodeData.MetaData);
            return new FSMetadata
            {
                TotalSizeActive = fsMetaData.Storage.Size,
                FileActiveCount = fsMetaData.Storage.FileCount,
                FileDestroyedCount = fsMetaData.FileStatusSummary.Destroyed,
                FolderActiveCount = fsMetaData.FolderStatusSummary.Active,
                FolderDestroyedCount = fsMetaData.FolderStatusSummary.Destroyed,
                FileAllCount = fsMetaData.FileStatusSummary.Total,
                FolderAllCount = fsMetaData.FolderStatusSummary.Total
            };
        }

        public async Task<FSFileCount> GetFSFileCountByCategory(FSMetadataByCategoryParam param)
        {
            logger.Info($"Getting FS metadata for category: {param.Category}");

            var nodeData = await FSMyHubDashboardDao.GetByFullPathAsync(param.FullPath);

            if (nodeData == null || string.IsNullOrEmpty(nodeData.MetaData))
            {
                return null;
            }
            var metaData = JsonConvert.DeserializeObject<FSDashboard>(nodeData.MetaData);
            IEnumerable<RecordStats> records = metaData.LineChartData;
            IEnumerable<DestroyedStats> destroyedRecords = metaData.DestroyedStats ?? [];
            if (param.Category is FSMetadataCategory.Created
                            or FSMetadataCategory.Modified
                            or FSMetadataCategory.Accessed)
            {
                long start = long.Parse(new DateTime(param.StartTime).ToString("yyyyMMddHH"));
                long end = long.Parse(new DateTime(param.EndTime).ToString("yyyyMMddHH"));
                records = records.Where(x => x.Date >= start && x.Date <= end);
            }
            else if (param.Category == FSMetadataCategory.Destroyed)
            {
                long start = long.Parse(new DateTime(param.StartTime).ToString("yyyyMMdd"));
                long end = long.Parse(new DateTime(param.EndTime).ToString("yyyyMMdd"));

                destroyedRecords = destroyedRecords.Where(x => x.Date >= start && x.Date <= end);
            }
            long fileCount = param.Category switch
            {
                FSMetadataCategory.Created => records.Sum(x => x.Created),
                FSMetadataCategory.Modified => records.Sum(x => x.Modified),
                FSMetadataCategory.Accessed => records.Sum(x => x.Accessed),
                FSMetadataCategory.ClassCode => metaData.ClassCodes.FirstOrDefault(x => x.ClassCodeName == param.ClassCode)?.Usage ?? 0,
                FSMetadataCategory.Destroyed => destroyedRecords.Sum(x => x.Destroyed),
                FSMetadataCategory.All => records.Sum(x => x.Created + x.Modified + x.Accessed),
                _ => metaData.Storage.FileCount
            };
            return new FSFileCount
            {
                FileCount = fileCount
            };
        }

        public async Task<RecordItemPagingResult> GetRecordItemInformation(RecordItemQueryDefinition queryModel)
        {
            var connectionRecordId = queryModel.FullPathConnection.ToLowerInvariant().ToMd5();
            logger.Info($"Getting folder information for connectionGroupId: {queryModel.ConnectionGroupId}, connectionId: {queryModel.ConnectionId}");
            Expression<Func<Record, bool>> predicate = r =>
                    r.Id != connectionRecordId &&
                    r.SourceFlag == (int)SourceFlag.FileSystem &&
                    r.L2PartitionKey == queryModel.ConnectionId.ToString() &&
                    r.NodeType == queryModel.Level &&
                    r.RecordStatus == (int)RMRecordStatus.Active;
            var queryResult = ExplorerDao.QueryByPage(
                    predicate,
                    r => r.LeafName,
                    queryModel.IsDesc,
                    queryModel.PageSize,
                    queryModel.ContinuationToken);

            var totalCount = ExplorerDao.QueryCount(predicate);
            var records = queryResult.Item1;
            var continuationToken = queryResult.Item2;
            string Normalize(string path)
            {
                return path.TrimEnd('\\');
            }
            var recordItems = records.Select(r => new RecordItem
            {
                NodeId = r.NodeId,               
                FullPath = $"{Normalize(r.DirPath)}\\{r.LeafName}",           
                ConnectionId = queryModel.ConnectionId,
                ConnectionGroupId = queryModel.ConnectionGroupId,
                ParentId = r.ParentId,           
                PartitionKeyId = new Guid(r.L2PartitionKey), 
                Level = r.NodeType
            }).ToList();

            return new RecordItemPagingResult
            {
                Items = recordItems,
                ContinuationToken = continuationToken,
                Count = totalCount
            };
        }

        public async Task<RecordItemPagingResult> GetPendingDisposalItem(RecordItemQueryDefinition queryModel)
        {
            var queryDefinition = new ManualApprovalQueryDefinition() 
            {
                Continuation = string.IsNullOrEmpty(queryModel.ContinuationToken) ? null : queryModel.ContinuationToken,
                PageSize = queryModel.PageSize,
                IsJpmc = true,
                NeedCalculationCount = true,
                PartitionKeyId = queryModel.ConnectionId.ToString(),
                IsEnableFolderView = false,
            };
            queryDefinition.Filters.AddRange(new[] {
                new ManualApprovalFilterDefinition
                {
                    FilterOption = ManualApprovalFilterOptions.Source,
                    Value = JsonConvert.SerializeObject(new List<int> { (int)SourceFlag.FileSystem })
                },
                new ManualApprovalFilterDefinition
                {
                    FilterOption = ManualApprovalFilterOptions.JpmcConnectionId,
                    Value = queryModel.ConnectionId.ToString()
                },
                new ManualApprovalFilterDefinition
                {
                    FilterOption = ManualApprovalFilterOptions.ApprovalStatus,
                    Value = JsonConvert.SerializeObject(new List<SOApproveDBStatus> { SOApproveDBStatus.WaitingApprove })
                },
                new ManualApprovalFilterDefinition
                {
                    FilterOption = ManualApprovalFilterOptions.Workspace,
                    Value = JsonConvert.SerializeObject(new
                    {
                        WorkspaceIds = new List<string>(),
                        WorkspacePaths = new List<string> { queryModel.FullPathConnection }, 
                        ContentSource = (int)SourceFlag.FileSystem
                    })
                }
            }
            );
            var result = await ManualApprovalService.UnderReviewFolderViewQueryAsync(queryDefinition, string.Empty, false);
            return new RecordItemPagingResult
            {
                Items = result.Items.Select(r => new RecordItem
                {
                    NodeId = r.NodeId,
                    FullPath = r.FullPath,
                    ConnectionId = queryModel.ConnectionId,
                    ConnectionGroupId = queryModel.ConnectionGroupId,
                    PartitionKeyId = queryModel.ConnectionId,
                    Level = r.NodeType
                }).ToList(),
                ContinuationToken = result.Continuation,
                Count = result.Count
            };
        }

        private Expression<Func<RMJobMonitor, bool>> BuildFilter(JobReportParam param)
        {
            return x =>
                (!param.StartTime.HasValue || x.StartTime >= param.StartTime.Value)
                && (!param.EndTime.HasValue || x.EndTime <= param.EndTime.Value)
                && (!param.JobType.HasValue || x.JobType == (int)param.JobType.Value)
                && (!param.Status.HasValue || x.Status == (int)param.Status.Value)
                && _jobTypesCanRetrive.Contains((JobType)x.JobType);
               // && _jobStatusCanRetrive.Contains((JobStatus)x.Status);
        }

    }
}
