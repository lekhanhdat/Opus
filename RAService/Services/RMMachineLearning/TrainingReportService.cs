/********************************************************************
 *
 *  PROPRIETARY and CONFIDENTIAL
 *
 *  This file is licensed from, and is a trade secret of:
 *
 *                   AvePoint, Inc.
 *                   525 Washington Blvd, Suite 1400
 *                   Jersey City, NJ 07310
 *                   United States of America
 *                   Telephone: +1-201-793-1111
 *                   WWW: www.avepoint.com
 *
 *  Refer to your License Agreement for restrictions on use,
 *  duplication, or disclosure.
 *
 *  RESTRICTED RIGHTS LEGEND
 *
 *  Use, duplication, or disclosure by the Government is
 *  subject to restrictions as set forth in subdivision
 *  (c)(1)(ii) of the Rights in Technical Data and Computer
 *  Software clause at DFARS 252.227-7013 (Oct. 1988) and
 *  FAR 52.227-19 (C) (June 1987).
 *
 *  Copyright © 2017-2026 AvePoint® Inc. All Rights Reserved. 
 *
 *  Unpublished - All rights reserved under the copyright laws of the United States.
 */
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.MachineLearning;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.RMMachineLearning;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.RMMachineLearning.AuditHandler;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.RMMachineLearning
{
    [Audit]
    public class TrainingReportService : RMServiceBase, ITrainingReportService
    {
        private static readonly RALogger Logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private ITermDao termDao => PlatformWindsorManager.GetService<ITermDao>();
        private static IRMRemoteNodeDao remoteNodeDao => PlatformWindsorManager.GetService<IRMRemoteNodeDao>();
        private static IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();
        private static IGeneralSettingService generalSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();
        private static IJobQueueService jobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();

        private static IDownloadDataInfoDao DownloadDataInfoDao => PlatformWindsorManager.GetService<IDownloadDataInfoDao>();

        private static IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();

        private static IRMSubJobDao SubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();

        private readonly IExplorerDao explorerDao = new ExplorerDao();
        private static Dictionary<string, string> SiteUrlCache = new();
        public async Task<MLTrainingReportResult> QueryAsync(MLTrainingReportQueryParam param)
        {
            try
            {
                param.PageSize = param.PageSize == 0 ? 10 : param.PageSize;
                var dto = new ExplorerQueryV3Dto()
                {
                    QueryOption = new ExplorerQueryOptionV3()
                    {
                        Values = new List<ExplorerSearchOptionV3>()
                    },
                    PagingInfo = new ExplorerPagingInfo
                    {
                        PageIndex = param.PageIndex,
                        PageSize = param.PageSize,
                    }
                };
                dto.QueryOption.Values.Add(new ExplorerSearchOptionV3
                {
                    Column = new ExplorerQueryColumn { Id = QueryCloumnIds.PredictTermId },
                    Value = JsonConvert.SerializeObject(new List<Guid>()),
                });

                if (!string.IsNullOrEmpty(param.SearchValue))
                {
                    dto.QueryOption.Values.Add(new ExplorerSearchOptionV3
                    {
                        Column = new ExplorerQueryColumn { Id = RecordBuildInColumnIds.NameOrTitle },
                        Value = JsonConvert.SerializeObject(param.SearchValue)
                    });
                }

                if (param.Filters != null)
                {
                    foreach (var filter in param.Filters)
                    {
                        if (filter.ColumnValues != null && filter.ColumnValues.Count > 0)
                        {
                            if (filter.Column == TrainingFilterColumn.IntelligentTerm)
                            {
                                dto.QueryOption.Values.Add(new ExplorerSearchOptionV3
                                {
                                    Column = new ExplorerQueryColumn { Id = QueryCloumnIds.PredictTermId },
                                    Value = JsonConvert.SerializeObject(filter.ColumnValues.Select(v => new Guid(v)).ToList()),
                                });
                            }
                            if (filter.Column == TrainingFilterColumn.ReclassifyTerm)
                            {
                                dto.QueryOption.Values.Add(new ExplorerSearchOptionV3
                                {
                                    Column = new ExplorerQueryColumn { Id = QueryCloumnIds.Term },
                                    Value = JsonConvert.SerializeObject(new ExplorerFilterOptionV2()
                                    {
                                        TermIds = filter.ColumnValues.Where(v => v != "-1").Select(v => new Guid(v)).ToList(),
                                        WithOutTerms = filter.ColumnValues.Any(v => v == "-1")
                                    })
                                });
                            }
                            if (filter.Column == TrainingFilterColumn.ApprovalStatus)
                            {
                                dto.QueryOption.Values.Add(new ExplorerSearchOptionV3
                                {
                                    Column = new ExplorerQueryColumn { Id = QueryCloumnIds.MLApprovalStatus },
                                    Value = JsonConvert.SerializeObject(filter.ColumnValues.Select(int.Parse).ToList()),
                                });
                            }
                            if (filter.Column == TrainingFilterColumn.PredictTime)
                            {
                                var timeZoneId = (await generalSettingService.GetGeneralSettingAsync()).TimeZoneId;
                                var timeZone = GeneralSettingConfig.FindSystemTimeZoneById(timeZoneId);

                                var startTime = DateTime.Parse(filter.ColumnValues.FirstOrDefault());
                                var endTime = DateTime.Parse(filter.ColumnValues.Skip(1).FirstOrDefault());

                                var startTimeUTC = TimeZoneInfo.ConvertTimeToUtc(startTime, timeZone);
                                var endTimeUTC = TimeZoneInfo.ConvertTimeToUtc(endTime, timeZone);

                                dto.QueryOption.Values.Add(new ExplorerSearchOptionV3()
                                {
                                    Value = JsonConvert.SerializeObject(new DateInfo
                                    {
                                        Condition = DateCondition.FromTo,
                                        Value1 = startTimeUTC.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                                        Value2 = endTimeUTC.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                                        TimeZoneId = "UTC"
                                    }),
                                    Column = new ExplorerQueryColumn
                                    {
                                        Id = QueryCloumnIds.PredictTime
                                    }
                                });
                            }
                        }
                    }
                }

                var columnName = param.SortBy switch
                {
                    "Name" => CosmosConst.C_LeafName,
                    "PredictTime" => CosmosConst.C_PredictTime,
                    _ => CosmosConst.C_PredictTime
                };
                dto.QueryOption.OrderColumn = new ExplorerQueryOrderColumn() { Column = new ExplorerQueryColumn { Name = columnName }, OrderAsc = param.IsAscending };

                var queryResult = explorerDao.SearchRecordsV3(dto, null);
                var totalCount = explorerDao.QueryCountV3(dto, null);
                var pageResult = new MLTrainingReportResult
                {
                    TotalCount = totalCount,
                    PageIndex = queryResult.Item2,
                    TrainingReports = new List<MLTrainingReportDto>()
                };
                var allTermIds = queryResult.Item1.SelectMany(r => new List<Guid>() { r.TermId, r.PredictTermId }).ToList();
                var allTermDic = (await termDao.FindListAsync(tm => allTermIds.Contains(tm.UniqueId))).ToDictionary(t => t.UniqueId, t => t.Name);
                var gls = generalSettingService.GetGeneralSettingAsync().GetAwaiter().GetResult();
                foreach (var item in queryResult.Item1)
                {
                    var reportItem = new MLTrainingReportDto()
                    {
                        Id = item.Id,
                        FileName = item.LeafName.EndsWith("." + item.ExtensionForFile, StringComparison.OrdinalIgnoreCase) ? item.LeafName : string.Concat(item.LeafName, ".", item.ExtensionForFile),
                        PredictTermName = allTermDic.TryGetValue(item.PredictTermId, out string predictTermName) ? predictTermName : "",
                        PredictTermId = item.PredictTermId,
                        SourceFlag = item.SourceFlag,
                        FullPath = GetRecordFullPath(item),
                        RecordsID = item.RecordsId,
                        Type = item.ExtensionForFile,
                        DateString = generalSettingService.ConvertTiksToDateTime(gls, item.PredictTime, true).SimplifyFormatTime
                    };
                    //if (item.MLClassificationType == (int)RMMLClassificationType.ManualClassified)
                    //{
                    //    reportItem.ChangeTermName = allTermDic.TryGetValue(item.TermId, out string changeTermName) ? changeTermName : "";
                    //    reportItem.ChangeTermId = item.TermId;
                    //}
                    reportItem.ChangeTermName = allTermDic.TryGetValue(item.TermId, out string changeTermName) ? changeTermName : "";
                    reportItem.TermId = item.TermId;
                
                    reportItem.Status = (RMMLApprovalStatus)item.MLApprovalStatus switch
                    {
                        RMMLApprovalStatus.None => I18NEntity.GetString("RM_ML_Report_ApprovalStatus_AutoApply"),
                        RMMLApprovalStatus.WaitingApprove => I18NEntity.GetString("RM_ML_Report_ApprovalStatus_Waiting"),
                        RMMLApprovalStatus.Approved => I18NEntity.GetString("RM_ML_Report_ApprovalStatus_Approved"),
                        RMMLApprovalStatus.Rejected => I18NEntity.GetString("RM_ML_Report_ApprovalStatus_Reclassify"),
                        _ => throw new NotImplementedException(),
                    };

                    pageResult.TrainingReports.Add(reportItem);
                }
                return pageResult;
            }
            catch (Exception ex)
            {
                Logger.Error($"An error occurred while querying training report. Error: {ex}");
                return new MLTrainingReportResult
                {
                    TotalCount = 0,
                    PageIndex = param.PageIndex,
                    TrainingReports = new List<MLTrainingReportDto>()
                };
            }
        }

        public async Task<List<List<string>>> GetIntelligentClassificationFilter()
        {
            var termIds = await explorerDao.DistinctQueryAsync(r => r.PredictTermId.ToString(), r => r.PredictTermId != Guid.Empty);
            var result = await AassembleTermPair(termIds);
            return result;
        }

        public async Task<List<List<string>>> GetReclassificationFilter()
        {
            var termIds = await explorerDao.DistinctQueryAsync(r => r.TermId.ToString(), r => r.PredictTermId != Guid.Empty);
            var result = await AassembleTermPair(termIds);
            result.Add(["-1", I18NEntity.GetString("RM_MachineLearning_ReprotCurrentTermNoTerm")]);
            return result;
        }

        public RAReturnMessage RunExportTrainingReportJob(MLTrainingReportExportParam exportParam)
        {
            var returnMessage = new RAReturnMessage();
            try
            {
                var loginName = TenantLocalValue.LogonUserEmail;
                var exportParamString = SerializerHelper.SerializeByJsonConvert(exportParam);
                var jqDto = new JobQueueDto
                {
                    JobType = JobType.MachineLearningExportReportJob,
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    JobRunByUser = loginName,
                    Parameters = $"{exportParamString}",
                };
                returnMessage.MessageType = RAMessageType.Successful;
                returnMessage.Extension = jobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while run machine learning export report job message. Error: {e}");
                returnMessage.MessageType = RAMessageType.Failed;
                returnMessage.ErrorMessage = e.Message;
            }
            return returnMessage;
        }
        [Audit(Module = AuditModule.MachineLearning, Category = AuditCategory.MachineLearning, Action = AuditAction.MLExportReportJob, BeforeHandler = typeof(MLTermBeforeAuditHandler), AfterHandler = typeof(MLTermAfterAuditHandler))]
        public string RealRunExportJob(string queryDefinitionStr)
        {
            Logger.Info("Start run machine learning export report job.");
            var jobId = string.Empty;

            try
            {
                var username = TenantLocalValue.LogonUserEmail;
                var account = AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail).GetAwaiter().GetResult();
                jobId = JobMonitorService.CreateJob(JobType.MachineLearningExportReportJob, username, account.UserId);
                SubJobDao.UpdateSubJobCount(jobId, 1);
                string subJobId = CreateSubJob(jobId, 0, JobType.MachineLearningExportReportJob, JobStatus.InProgress, 1, queryDefinitionStr);

                DownloadDataInfoDao.Create(new RMDownloadDataInfo()
                {
                    FileDownloadTime = DateTime.UtcNow.Ticks,
                    JobId = jobId,
                    RecordsId = Guid.NewGuid(),
                    JobStatus = (int)DownloadContentJobStatus.Wait,
                    UserId = account.UserId,
                    Name = jobId + ".zip",
                    DownloadType = DownloadContentType.MachineLearningExportReport,
                });

                Logger.Info($"Real run machine learning export report job: [{jobId}]");
                jobQueueService.HandleMessage(new JobQueueMessage
                {
                    JobId = subJobId,
                    JobType = JobType.MachineLearningExportReportJob,
                    CommandLine = $"{JobType.MachineLearningExportReportJob} {subJobId} {jobId}",
                });
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while real run machine learning export report job. Error: {e}");
                if (!string.IsNullOrEmpty(jobId))
                {
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, e.Message);
                }
            }

            return jobId;
        }
        private string CreateSubJob(string jobId, int currentSubjobIndex, JobType jobType, JobStatus jobState, int subJobCount, string jobMessage, string string1 = null)
        {
            string subJobId = string.Format(jobId + "_{0:D3}", currentSubjobIndex);
            var subJob = new RMSubJob()
            {
                Id = subJobId,
                ParentId = jobId,
                StartTime = DateTime.UtcNow.Ticks,
                JobType = (int)jobType,
                Progress = 0,
                Status = (int)jobState,
                Weight = 100d / subJobCount,
                String1 = string1,
                LastUpdateTime = DateTime.UtcNow.Ticks,
                Runable = jobState == JobStatus.InProgress ? RecordsConstants.SubJob_Runnable_Runing : RecordsConstants.SubJob_Runnable_Waiting,
            };
            subJob.JobContext = new RMJobContext() { JobId = subJobId, Content = jobMessage };
            SubJobDao.CreateJob(subJob);
            Logger.Info("Create sub job {0} sucessfull, type {1}, weight {2}, state {3}, string1 {4} ", subJob.Id, subJob.JobType, subJob.Weight, subJob.Status, string1);
            return subJobId;
        }
        private async Task<List<List<string>>> AassembleTermPair(List<string> termIds)
        {
            var result = new List<List<string>>();
            var allTermIds = termIds.Select(t => new Guid(t));
            var allTerms = (await termDao.FindListAsync(tm => allTermIds.Contains(tm.UniqueId))).OrderBy(t => t.Name);

            foreach (var term in allTerms)
            {
                result.Add([term.UniqueId.ToString(), term.Name]);
            }
            return result;
        }

        private static string GetRecordFullPath(Record record)
        {
            try
            {
                var siteUrl = GetSiteUrl(record.AveSiteId);
                var fullPath = WebUtil.MakeFullUrl(siteUrl, record.DirPath);
                if (record.ExtensionForFile == "RM_RDM_RecordDetails_DataType_SPItem")
                {
                    fullPath = WebUtil.GetListItemRealPath(fullPath);
                }
                return fullPath;
            }
            catch (Exception ex)
            {
                Logger.Warn($"An error while get record full path, record id: {record?.Id} message: {ex}");
                return string.Empty;
            }
        }

        private static string GetSiteUrl(string siteId)
        {
            if (!SiteUrlCache.TryGetValue(siteId, out var siteUrl))
            {
                siteUrl = remoteNodeDao.GetRemoteSiteCollectionById(siteId)?.url;
                if (!SiteUrlCache.TryAdd(siteId, siteUrl))
                {
                    Logger.Warn($"An error while add site url, site is:{siteId}");
                }
            }
            return siteUrl;
        }
    }
}
