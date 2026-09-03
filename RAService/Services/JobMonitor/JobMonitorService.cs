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
using Aspose.Pdf.Operators;
using AvePoint.Api.Contract;
using AvePoint.GCommon.Contract.AveModuleContract;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.Server.Common.Monitor;
using AvePoint.GCommon.Contract.Server.Common.Monitor.Object.Detail;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Server.Job.Object;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.Media.Service;
using AvePoint.RA.Common;
using AvePoint.RA.Common.AzureService;
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Global.Util;
using AvePoint.RA.Common.JobService;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.COP;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Discovery.Model.Configuration.AOSP;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Google.GControlPlatform;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Object.ArchiverMigration;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.Dashboard;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.SignalR;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Discovery.AOSP;
using AvePoint.RA.DB.Dao.Discovery.Impl.AOSP;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Common;
using AvePoint.RA.RACommonUtility.Permission;
using AvePoint.RA.RACommonUtility.Telemetry;
using AvePoint.RA.RADataBroker;
using AvePoint.RA.Service.Security;
using AvePoint.RA.Service.Services;
using AvePoint.RA.Service.Services.Dashboard.Model;
using AvePoint.RA.Service.Services.Discovery.AOSP;
using AvePoint.RA.Service.Services.JobMonitor.AuditHandler;
using AvePoint.RA.Service.Services.JobMonitor.Detail;
using AvePoint.RA.Service.Services.JobMonitor.Summary;
using AvePoint.RA.Service.Services.RMGeneralSetting;
using AvePoint.RA.Service.Services.RMReport;
using AvePoint.RA.Service.Services.RMReport.AuditHandler;
using AvePoint.RA.Service.Services.Schedule;
using AvePoint.RA.Service.Services.SharePoint;
using AvePoint.RA.Service.Services.SignalR;
using AvePoint.RA.Service.SharePointSetting;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.RestoreJob.Restore.Content;
using AvePoint.Records.Core.Utilities.Extensions;
using AvePoint.Wrapper.Common;
using DocAveOnline.WebApi.Contracts;
using DocumentFormat.OpenXml.Presentation;
using Google.Apis.Storage.v1.Data;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.Exchange.WebServices.Data;
using Microsoft365.Common.Extension;
using Newtonsoft.Json;
using Polly.Caching;
using RAExportCommon;
using RATeams;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Xml;
using Util;
using static AvePoint.GCommon.Contract.Server.Common.LogCollector.LogConstants;
using static AvePoint.GCommon.Utility.I18N.EventIds.Configuration;
using static AvePoint.RA.RACommonUtility.Common.CommonUtilityForSpecialTenant;
using ApprovalType = AvePoint.RA.DB.Model.ApprovalType;
using ArgumentCheck = AvePoint.Wrapper.Common.ArgumentCheck;
using JobStatus = AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus;
using SOJobDetailDto = AvePoint.GCommon.Contract.Server.Common.Monitor.Object.Detail.SOJobDetailDto;

namespace AvePoint.RA.Service.JobMonitor
{
    [Audit]
    public class JobMonitorService : RMServiceBase, IJobMonitorService
    {
        private List<JobType> querySubJobTypes = new List<JobType>() { JobType.TeamsArchiverBackup, JobType.SpecifyTeamsArchiverBackup, JobType.TeamsRecordsDisposal };
        public Dictionary<int, IMigrationJobSummaryService> migrationJobSummaryServiceDictionary { get; set; }
        #region Properties

        private RALogger logger = RALogger.GetInstance(typeof(JobMonitorService));

        /// <summary>
        /// job超时时间默认2 hours
        /// </summary>
        private int mTimeoutPeriod = 2 * 60;
        private int mTimeoutPeriodForWaitingJob = 24 * 60 * 5;
        public IJobMonitorDao JMDao => PlatformWindsorManager.GetService<IJobMonitorDao>();
        private IRMSubJobDao SubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();
        private IArchiverJobDao ArhciverJobDao => PlatformWindsorManager.GetService<IArchiverJobDao>();
        private IRMArchiverSettingsService RMArchiverSettingsService => PlatformWindsorManager.GetService<IRMArchiverSettingsService>();
        private IRMArchiverSettingDao ArchiverSettingDao => PlatformWindsorManager.GetService<IRMArchiverSettingDao>();
        private IRMJobExportSettingDao JESDao => PlatformWindsorManager.GetService<IRMJobExportSettingDao>();

        private IJobDetailService JDService => PlatformWindsorManager.GetService<IJobDetailService>();
        private IGeneralSettingService  GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService >();

        private IJobQueueService mJobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();
        private IJobInfoUpdater JobInfoUpdater => PlatformWindsorManager.GetService<IJobInfoUpdater>();
        private readonly static object jobIdLock = new object();
        private IUserService UserService => PlatformWindsorManager.GetService<IUserService>();
        private IRMScopeRoleAssignmentDao RMScopeRoleAssignmentDao => PlatformWindsorManager.GetService<IRMScopeRoleAssignmentDao>();
        private IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();
        private ITenantInfoDao TenantInfoDao => PlatformWindsorManager.GetService<ITenantInfoDao>();
        private IGlobalSettingService GlobalSettingService => PlatformWindsorManager.GetService<IGlobalSettingService>();
        private IRMSecurityTrimmingHelper SecurityTrimmingHelper => PlatformWindsorManager.GetService<IRMSecurityTrimmingHelper>();
        private IRMRunningJobRuleMappingDao RMRunningJobRuleMappingDao => PlatformWindsorManager.GetService<IRMRunningJobRuleMappingDao>();
        private ITenantService TenantService => _tenantService;
        private IDashboardService DashboardService => PlatformWindsorManager.GetService<IDashboardService>();
        private ILicenseHelperService LicenseHelperService => PlatformWindsorManager.GetService<ILicenseHelperService>();
        private static IDownloadDataInfoDao DownloadDataInfoDao => PlatformWindsorManager.GetService<IDownloadDataInfoDao>();
        private IJobQueueService JobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();

        private IRMLocationDao RMLocationDao => PlatformWindsorManager.GetService<IRMLocationDao>();

        private IRMLocalNodeDao LocalNodeDao => PlatformWindsorManager.GetService<IRMLocalNodeDao>();

        private IRMRemoteNodeDao RMRemoteNodeDao => PlatformWindsorManager.GetService<IRMRemoteNodeDao>();

        private readonly IRMMailboxDao MailBoxDao = PlatformWindsorManager.GetService<IRMMailboxDao>();

        private readonly IFSConnectionDao FSConnectionDao = PlatformWindsorManager.GetService<IFSConnectionDao>();
        private IHybridFileSystemWorkerService HybridFileSystemWorkerService => PlatformWindsorManager.GetService<IHybridFileSystemWorkerService>();

        private IRMSettingJobDao mSettingJobDao => PlatformWindsorManager.GetService<IRMSettingJobDao>();
        private ISPSettingTreeService RMSPTreeService => PlatformWindsorManager.GetService<ISPSettingTreeService>();
        private ITeamsSettingTreeService RMTeamsTreeService => PlatformWindsorManager.GetService<ITeamsSettingTreeService>();
        private IRMRemoteGoogleNodeService RemoteGoogleNodeService => PlatformWindsorManager.GetService<IRMRemoteGoogleNodeService>();
        private IScheduleService ScheduleService => PlatformWindsorManager.GetService<IScheduleService>();
        private IRMScheduleDao RMScheduleDao => PlatformWindsorManager.GetService<IRMScheduleDao>();
        private IArchiverSiteMasterIndexDao ArchiverSiteMasterIndexDao => PlatformWindsorManager.GetService<IArchiverSiteMasterIndexDao>();

        private readonly IRMDiscoveryAOSPOptimizationSettingsInfoDao _optimizationSettingsInfoDao = new RMDiscoveryAOSPOptimizationSettingsInfoDao();

        private readonly IRMDiscoveryAOSPSiteOptimizationMappingTableDao _siteOptimizationMappingTableDao = new RMDiscoveryAOSPSiteOptimizationMappingTableDao();

        private IRMJobSizeAndCountStatisticsDao mRMJobSizeAndCountStatisticsDao => PlatformWindsorManager.GetService<IRMJobSizeAndCountStatisticsDao>();
        
        private IGControlPlatformJobService GControlPlatformJobService => PlatformWindsorManager.GetService<IGControlPlatformJobService>();

        private readonly IRMDiscoveryAOSPNodeDao _nodeDao = new RMDiscoveryAOSPNodeDao();
        
        private readonly ITenantService _tenantService = PlatformWindsorManager.GetService<ITenantService>();

        private static DateTime lastGeneratedDate = DateTime.MinValue;

        private readonly string mContainerIdStr = "ContainerId";

        private readonly string mJobTypeStr = "JobType";

        private readonly string mUserNameStr = "UserName";
        
        private readonly List<JobType> _jobTypesAssociateWithGControl =
        [
            JobType.GoogleApplySettings,
            JobType.GoogleRecordsDisposal,
            JobType.GoogleDataSynchronization,
            JobType.TermSynchronization,
            JobType.ExplorerOfflineSearch,
            JobType.GoogleArchiverRestore,
            JobType.GoogleArchiverRetention,
            JobType.GlobalSearchAction,
            JobType.ManualApprovalOrRejectJob,
            JobType.SyncNodesFromAOS,
            JobType.SyncSecurityContainer,
            JobType.Dashboard,
            JobType.ManualApprovalEmailSchedule,
            JobType.MachineLearningReviewReclassify,
            JobType.MachineLearningReviewApprove,
            JobType.ImportTermStructure,
            JobType.DiscoveryGoogleJobV1,
            JobType.DiscoveryGoogleProfileJob,
        ];


        private DAOAPIClientV1 client = null;
        private DAOAPIClientV1 Client
        {
            get
            {
                //RECO-1396 timer check disposal job时需要重新获取Client
                return client = new DAOAPIClientV1();
            }
        }

        //private DAOApiClientV1 mClient = null;
        //private DAOApiClientV1 ClientNew
        //{
        //    get
        //    {
        //        //RECO-1396 timer check disposal job时需要重新获取Client
        //        return mClient = new DAOApiClientV1();
        //    }
        //}

        private const string PARAMTERS = "@{0}";

        // Archive: end-user job types we consider low-value for long-term in primary table
        private static readonly ReadOnlyCollection<int> ArchiveEndUserJobTypes = new ReadOnlyCollection<int>(new List<int>
        {
            (int)JobType.ExportReportDetails,
            (int)JobType.DownloadJobReports,
            (int)JobType.ExportSearchResult,
            (int)JobType.ExplorerOfflineSearch,
            (int)JobType.ArchiverRestore,
            (int)JobType.ArchiverToSpoRestore,
        });

        #endregion

        #region Job Monitor Method
        public System.Threading.Tasks.Task<int> ArchiveJobRecordsAsync(string jobId)
        {
           
            bool enabled;
            AvePoint.RA.Service.JobMonitor.RMJobMonitorArchiverConfig.TryGetBool(
                AvePoint.RA.Service.JobMonitor.RMJobMonitorArchiverConfig.EnabledKey, false, out enabled);
            if (!enabled)
            {
                logger.Info("JobMonitorArchive disabled via RMJobMonitorArchiverConfig; skipping execution.");
                UpdateJobStatus(jobId, JobStatus.Skipped);
                return System.Threading.Tasks.Task.FromResult(0);
            }

            // Parameters with defaults
            int maxRowsPerRun = AvePoint.RA.Service.JobMonitor.RMJobMonitorArchiverConfig.GetInt(
                AvePoint.RA.Service.JobMonitor.RMJobMonitorArchiverConfig.MaxRowsPerRunKey, 20000);
            int olderThanDays = AvePoint.RA.Service.JobMonitor.RMJobMonitorArchiverConfig.GetInt(
                AvePoint.RA.Service.JobMonitor.RMJobMonitorArchiverConfig.OlderThanDaysKey, 100);

            logger.Info($"Start archiving job monitor records. MaxRowsPerRun={maxRowsPerRun}, OlderThanDays={olderThanDays}");

            // Initial progress
            UpdateJobProgress(jobId, 10);

            var batchSize = Math.Max(1, maxRowsPerRun);
            int totalMoved = 0;
            int moved;
            int iterations = 0;
            const int maxIterations2 = 10000;

            do
            {
                using (new CheckJobStopScope()) { }
                iterations++;
                using (var performance = new PerformanceScope("JobMonitorService.ArchiveJobRecordsAsync", $"iterations={iterations}", true))
                {
                    moved = JMDao.ArchiveDataBatch(batchSize, olderThanDays, ArchiveEndUserJobTypes);
                    totalMoved += moved;


                    if (iterations % 10 == 0)
                    {
                        // simple linear progress bump; cap at 95 to leave room for completion
                        var progress = Math.Min(95, 10 + iterations);
                        UpdateJobProgress(jobId, progress);
                    }

                    if (iterations >= maxIterations2)
                    {
                        logger.Warn($"Archive job reached max iterations cap ({maxIterations2}). Stopping early. movedSoFar={totalMoved}");
                        break;
                    }
                }
            }
            while (moved > 0);

            UpdateJobProgress(jobId, 100);
            logger.Info($"Archived {totalMoved} RMJobMonitor rows to RMJobMonitorArchive in total. BatchSize={batchSize}, OlderThanDays={olderThanDays}");

            // Build a concise completion comment with totals and localized job type names
            var jobTypeNames = string.Join(", ", ArchiveEndUserJobTypes.Select(t => GetJobTypeName(t))).Trim();

            // Compose I18N key + arguments so UI/localization layer can render the final text
            string comment;
            if (totalMoved > 0)
            {
                var key = "RM_JS_JM_JobMonitorArchive_Summary"; // expects args: totalMoved, olderThanDays, jobTypeNames
                comment = string.Join(I18NEntity.Separator, new[]
                {
                    key,
                    totalMoved.ToString(),
                    olderThanDays.ToString(),
                    jobTypeNames
                });
            }
            else
            {
                var key = "RM_JS_JM_JobMonitorArchive_Summary_NoRecords"; // expects args: olderThanDays, jobTypeNames
                comment = string.Join(I18NEntity.Separator, new[]
                {
                    key,
                    olderThanDays.ToString(),
                    jobTypeNames
                });
            }

            UpdateJobStatus(jobId, JobStatus.Finished, comment);
            return System.Threading.Tasks.Task.FromResult(totalMoved);
        }


        public async Task<string> GetFilterListAsync(string filterName)
        {
            ParameterExpression param = Expression.Parameter(typeof(RMJobMonitor), "c");
            var selectLambda = Expression4DynamicQuery.GetExpressionBody<RMJobMonitor>(param, filterName);
            try
            {
                var list = JMDao.GetFilterList(selectLambda);
                AddSpecailJobTypeForFilter(list);
                var nameList = new Dictionary<string, string>();

                var result = await GetJobTypeFilterConditionAsync();
                var (needCheckedJobTypeList, needExpectJobTypeList) = (result.Item1, result.Item2);
                foreach (var i in list)
                {
                    if (needCheckedJobTypeList.Count > 0)
                    {
                        if (needCheckedJobTypeList.Contains(i))
                        {
                            nameList.Add(i.ToString(), GetJobTypeName(i));
                        }
                    }
                    else
                    {
                        nameList.Add(i.ToString(), GetJobTypeName(i));
                    }
                }
                nameList = nameList.ExceptBy(needExpectJobTypeList.Select(o => o.ToString()), item => item.Key).ToDictionary(o => o.Key, p => p.Value);

                return JsonConvert.SerializeObject(nameList);
            }
            catch (Exception e)
            {
                logger.Error("Get filter name Error. filter name:{0}, Message:{1}.", filterName, e.ToString());
                return string.Empty;
            }
        }
        private void AddSpecailJobTypeForFilter(List<int> jobTypes)
        {
            jobTypes?.Add((int)JobType.MailBoxBackup);
        }
        public List<BaseJobDto> GetJobsByJobType(JobType jobType)
        {
            var jobs = DatabaseUtility.RetryPolicy.ExecuteAction(() => {
                return JMDao.GetJobsByJobType(jobType);
            });

            List<BaseJobDto> jobInfos = new List<BaseJobDto>();
            foreach (var job in jobs)
            {
                jobInfos.Add(
                    new BaseJobDto()
                    {
                        Id = job.Id,
                        JobType = job.JobType,
                        Status = job.Status,
                        Progress = job.Progress,
                        ScopeId = job.ScopeId,
                        StartTime = job.StartTime
                    });
            }
            return jobInfos;
        }

        public BaseJobDto GetLastestJobByJobType(JobType jobType)
        {
            var jobQueueMessage = mJobQueueService.GetDBJobQueueMessage(TenantLocalValue.LogonGroupId, TenantLocalValue.LogonUserEmail, jobType);
            if (jobQueueMessage.Count != 0)
            {
                return new BaseJobDto()
                {
                    JobType = (int)jobType,
                    Status = (int)JobStatus.Wait,
                    Progress = 0,
                    StartTime = DateTime.UtcNow.Ticks,
                };
            }

            var job = DatabaseUtility.RetryPolicy.ExecuteAction(() => {
                return JMDao.GetLastestJobByJobType(jobType);
            });

            if (job == null)
            {
                return new BaseJobDto()
                {
                    JobType = (int)jobType,
                    Status = (int)JobStatus.None,
                    Progress = 0,
                };
            }

            return new BaseJobDto()
            {
                Id = job.Id,
                JobType = job.JobType,
                Status = job.Status,
                Progress = job.Progress,
                ScopeId = job.ScopeId,
                StartTime = job.StartTime
            };
        }

        private async Task<bool> IsReviewer()
        {
            var endUserPermission = await DashboardService.GetEndUserPermissionAsync();
            return (endUserPermission & (int)DashboardEndUserPermission.ReviewEndUser) == (int)DashboardEndUserPermission.ReviewEndUser;
        }

        private async Task<Tuple<List<int>, List<int>>> GetJobTypeFilterConditionAsync()
        {
            var containsJobTypeList = new List<int>();
            var expectJobTypeList = new List<int>();
            if (!LicenseHelperService.HasOpusILLicense || !LicenseHelperService.HasOpusSOLicense)
            {
                if (LicenseHelperService.HasOpusILLicense)
                {
                    if (await IsReviewer())
                    {
                        containsJobTypeList = containsJobTypeList.Concat(JobTypeConstants.ReviewersJobTypes).ToList();
                    }
                    else
                    {
                        expectJobTypeList = expectJobTypeList.Concat(JobTypeConstants.ArchiverJobTypes).ToList();
                    }
                }
                
                if (LicenseHelperService.HasOpusSOLicense && !LicenseHelperService.HasOpusGoogleLicense)
                {
                    containsJobTypeList = containsJobTypeList.Concat(JobTypeConstants.ArchiverJobTypes.Concat(JobTypeConstants.ArchiverSpecialJobTypes)).ToList();
                    if(TenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, PreviewFeature.ExportIndex))
                    {
                        containsJobTypeList.Add((int)JobType.ExportIndex);
                    }
                    //if (LicenseHelperService.HasOpusGoogleLicense || LicenseHelperService.HasGoogleControlLicense)
                    //{
                    //    containsJobTypeList = containsJobTypeList.Concat(JobTypeConstants.GoogleAllRelatedJobTypes).ToList();
                    //}
                }
            }
            return Tuple.Create(containsJobTypeList, expectJobTypeList);
        }

        public async Task<JMPageResult> GetJobsListAsync(JMPager pager)
        {
            Expression queryExpr = null;
            List<Expression> allExpressionList = new List<Expression>();
            ParameterExpression param = Expression.Parameter(typeof(RMJobMonitor), "c");
            JMPageResult responseResult = new JMPageResult();
            responseResult.Result = new List<JMItemInfo>();
            List<string> timeFrameColumns = new List<string> { "StartTime", "EndTime" };

            int totalCount;
            //用OR合并一个Filter选的多个值的表达式
            foreach (var f in pager.Filters)
            {
                if (timeFrameColumns.Contains(f.ColumnName) && f.ColumnValues != null)
                {
                    var timeZoneId = (await GeneralSettingService.GetGeneralSettingAsync()).TimeZoneId;
                    var timeZone = GeneralSettingConfig.FindSystemTimeZoneById(timeZoneId);

                    var timeFrame = JsonConvert.DeserializeObject<ManualApprovalTimeFrame>(f.ColumnValues.FirstOrDefault());
                    var endTime = new DateTime(timeFrame.EndTime.Year, timeFrame.EndTime.Month, timeFrame.EndTime.Day, 23, 59, 59); //ensure the EndTime is end of the day.

                    var startTimeTicks = TimeZoneInfo.ConvertTimeToUtc(timeFrame.StartTime, timeZone).Ticks;
                    var endTimeTicks = TimeZoneInfo.ConvertTimeToUtc(endTime, timeZone).Ticks;

                    logger.Info($"Select the data with the time frame between [{startTimeTicks} and {endTimeTicks}]");
                    var exp1 = Expression4DynamicQuery.GetGreaterThanOrEqualExpression(typeof(RMJobMonitor), param, f.ColumnName, startTimeTicks);
                    var exp2 = Expression4DynamicQuery.GetLessThanOrEqualExpression(typeof(RMJobMonitor), param, f.ColumnName, endTimeTicks);
                    allExpressionList.AddRange(new List<Expression> { exp1, exp2 });
                    continue;
                }

                if (f.ColumnName.Equals("UserName", StringComparison.OrdinalIgnoreCase))
                {
                    var newValues = new List<string>();

                    f.ColumnValues.ForEach(v =>
                    {
                        //Verify whether the user wanna query with job run by field is "System".
                        if (v.Equals(I18NEntity.GetString("RM_TS_RunSchedule"), StringComparison.OrdinalIgnoreCase))
                        {
                            newValues.AddRange(new List<string> { "RM_TS_RunSchedule", "AvePoint Cloud Records System" });
                        }
                        else
                        {
                            newValues.Add(v);
                        }
                    });

                    var exp = Expression4DynamicQuery.GetInExpression(typeof(RMJobMonitor), param, f.ColumnName, newValues);
                    allExpressionList.Add(exp);
                    continue;
                }

                var exps = f.ColumnValues.Select(c => Expression4DynamicQuery.GetEqualExpression(typeof(RMJobMonitor), param, f.ColumnName, c));
                var filterExpression = exps.Aggregate(Expression.OrElse);
                allExpressionList.Add(filterExpression);
            }
            if (!string.IsNullOrEmpty(pager.SearchValue))
            {
                try
                {
                    var exps = pager.SearcheKeys.Select(searchKey => Expression4DynamicQuery.GetContainsExpression(typeof(RMJobMonitor), param, searchKey, pager.SearchValue));
                    var searchExpression = exps.Aggregate(Expression.OrElse);
                    allExpressionList.Add(searchExpression);
                }
                catch (Exception ex)
                {
                    logger.Warn("{0}", ex.Message.ToString());
                    responseResult.Pager = new JMPager() { TotalNumber = 0, PageSize = 0 };
                    return responseResult;
                }
            }

            //if (((LicenseHelperService.HasOpusILLicense || LicenseHelperService.HasOpusGoogleLicense) && !(await IsOpusILAdminAsync())))
            //{
            //    List<Expression> exps = new();
            //    var endUserPermission = await DashboardService.GetEndUserPermissionAsync();
            //    if ((endUserPermission & (int)DashboardEndUserPermission.ReviewEndUser) == (int)DashboardEndUserPermission.ReviewEndUser)
            //    {
            //        exps.Add(GetReviewersFilerExpression(param));
            //    }
            //    else
            //    {
            //        exps.Add(await GetSecurityGroupFilterExpressionAsync(param));
            //    }

            //    if (exps.Count > 0)
            //    {
            //        allExpressionList.Add(exps.Aggregate(Expression.OrElse));
            //    }
            //}
            //if (!LicenseHelperService.HasOpusILLicense && !LicenseHelperService.HasOpusGoogleLicense && LicenseHelperService.HasOpusSOLicense)
            //{
            //    List<Expression> exps = new();
            //    if (await IsOpusSOAdminAsync())
            //    {
            //        exps.Add(await GetArchiverJobTypeExpression(param));
            //    }
            //    else
            //    {
            //        exps.Add(await GetArchiverJobExpressionAsync(param));
            //    }

            //    if (TenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, PreviewFeature.ExportIndex))
            //    {
            //        var isAdmin = RetryUtility.RetryAlways(
            //            () => UserService.GetApplicationAdminsAsync().GetAwaiter().GetResult()?.Any(u => u.UserId == TenantLocalValue.LogonUserId)
            //            , 3);
            //        if (isAdmin ?? false)
            //        {
            //            exps.Add(Expression4DynamicQuery.GetInExpression(typeof(RMJobMonitor), param, mJobTypeStr,
            //                new List<int> { (int)JobType.ExportIndex }.Cast<object>()));
            //        }
            //    }

            //    if (exps.Count > 0)
            //    {
            //        allExpressionList.Add(exps.Aggregate(Expression.OrElse));
            //    }
            //}
            List<RMJobMonitor> dbResult = new List<RMJobMonitor>();
            if (allExpressionList.Count > 0)
            {
                //将多个Filter和search都用AND合并
                queryExpr = allExpressionList.Aggregate(Expression.AndAlso);
                var lambda = Expression.Lambda<Func<RMJobMonitor, bool>>(queryExpr, param);
                Stopwatch timer = new Stopwatch();
                timer.Start();
                dbResult = JMDao.GetJobs(pager.JumpPage, pager.PageSize, out totalCount, pager.SortBy, (pager.IsSort && !pager.IsDesc), lambda);
                timer.Stop();
                logger.Info("Get Job Monitor Data Take Milliseconds:{0}ms. Lambda is:{1}", timer.ElapsedMilliseconds, lambda.ToString());
            }
            else
            {
                Stopwatch timer = new Stopwatch();
                timer.Start();
                dbResult = JMDao.GetJobs(pager.JumpPage, pager.PageSize, out totalCount, pager.SortBy, (pager.IsSort && !pager.IsDesc), null);
                timer.Stop();
                logger.Info("Get Job Monitor Data Take Milliseconds:{0}ms.", timer.ElapsedMilliseconds);
            }
            responseResult.TotalNumber = totalCount;
            //GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
            foreach (var r in dbResult)
            {
                var job = r;
                responseResult.Result.Add(new JMItemInfo()
                {
                    JobId = job.Id,
                    JobTypeCode = job.JobType,
                    JobType = GetJobTypeName(job.JobType),
                    Status = (JobStatus)job.Status,
                    Progress = job.Progress,
                    StartTime = job.StartTime.ToString(),
                    EndTime = job.EndTime.ToString(),
                    UserName = job.UserName,
                    Joblocation = await GetJobLocationUrl(job),
                });
            }
            return responseResult;
        }

        public async Task<string> GetJobsDataAsync(JMPager pager)
        {
            Expression queryExpr = null;
            List<Expression> allExpressionList = new List<Expression>();
            ParameterExpression param = Expression.Parameter(typeof(RMJobMonitor), "c");
            JMPageResult responseResult = new JMPageResult();
            responseResult.Result = new List<JMItemInfo>();
            List<string> timeFrameColumns = new List<string> { "StartTime", "EndTime" };

            int totalCount;
            //用OR合并一个Filter选的多个值的表达式
            foreach (var f in pager.Filters)
            {
                if (timeFrameColumns.Contains(f.ColumnName) && f.ColumnValues != null)
                {
                    var timeZoneId = (await GeneralSettingService.GetGeneralSettingAsync()).TimeZoneId;
                    var timeZone = GeneralSettingConfig.FindSystemTimeZoneById(timeZoneId);

                    var timeFrame = JsonConvert.DeserializeObject<ManualApprovalTimeFrame>(f.ColumnValues.FirstOrDefault());
                    var endTime = new DateTime(timeFrame.EndTime.Year, timeFrame.EndTime.Month, timeFrame.EndTime.Day, 23, 59, 59); //ensure the EndTime is end of the day.

                    var startTimeTicks = TimeZoneInfo.ConvertTimeToUtc(timeFrame.StartTime, timeZone).Ticks;
                    var endTimeTicks = TimeZoneInfo.ConvertTimeToUtc(endTime, timeZone).Ticks;

                    logger.Info($"Select the data with the time frame between [{startTimeTicks} and {endTimeTicks}]");
                    var exp1 = Expression4DynamicQuery.GetGreaterThanOrEqualExpression(typeof(RMJobMonitor), param, f.ColumnName, startTimeTicks);
                    var exp2 = Expression4DynamicQuery.GetLessThanOrEqualExpression(typeof(RMJobMonitor), param, f.ColumnName, endTimeTicks);
                    allExpressionList.AddRange(new List<Expression> { exp1, exp2 });
                    continue;
                }

                if (f.ColumnName.Equals("UserName", StringComparison.OrdinalIgnoreCase))
                {
                    var newValues = new List<string>();

                    f.ColumnValues.ForEach(v =>
                    {
                        //Verify whether the user wanna query with job run by field is "System".
                        if (v.Equals(I18NEntity.GetString("RM_TS_RunSchedule"), StringComparison.OrdinalIgnoreCase))
                        {
                            newValues.AddRange(new List<string> { "RM_TS_RunSchedule", "AvePoint Cloud Records System" });
                        }
                        else
                        {
                            newValues.Add(v);
                        }
                    });

                    var exp = Expression4DynamicQuery.GetInExpression(typeof(RMJobMonitor), param, f.ColumnName, newValues);
                    allExpressionList.Add(exp);
                    continue;
                }

                var exps = f.ColumnValues.Select(c => Expression4DynamicQuery.GetEqualExpression(typeof(RMJobMonitor), param, f.ColumnName, c));
                var filterExpression = exps.Aggregate(Expression.OrElse);
                allExpressionList.Add(filterExpression);
            }
            if (!string.IsNullOrEmpty(pager.SearchValue))
            {
                try
                {
                    var exps = pager.SearcheKeys.Select(searchKey => Expression4DynamicQuery.GetContainsExpression(typeof(RMJobMonitor), param, searchKey, pager.SearchValue));
                    var searchExpression = exps.Aggregate(Expression.OrElse);
                    allExpressionList.Add(searchExpression);
                }
                catch (Exception ex)
                {
                    logger.Warn("{0}", ex.Message.ToString());
                    responseResult.Pager = new JMPager() { TotalNumber = 0, PageSize = 0 };
                    return JsonConvert.SerializeObject(responseResult);
                }
            }

            if (((LicenseHelperService.HasOpusILLicense || LicenseHelperService.HasOpusGoogleLicense) && !(await IsOpusILAdminAsync())))
            {
                List<Expression> exps = new();
                var endUserPermission = await DashboardService.GetEndUserPermissionAsync();
                if ((endUserPermission & (int)DashboardEndUserPermission.ReviewEndUser) == (int)DashboardEndUserPermission.ReviewEndUser)
                {
                    exps.Add(await GetReviewersFilerExpressionAsync(param));
                }
                else
                {
                    exps.Add(await GetSecurityGroupFilterExpressionAsync(param));
                }

                if (exps.Count > 0)
                {
                    allExpressionList.Add(exps.Aggregate(Expression.OrElse));
                }
            }
            if (!LicenseHelperService.HasOpusILLicense && !LicenseHelperService.HasOpusGoogleLicense && LicenseHelperService.HasOpusSOLicense)
            {
                List<Expression> exps = new();
                if (await IsOpusSOAdminAsync())
                {
                    exps.Add(await GetArchiverJobTypeExpression(param));
                }
                else
                {
                    exps.Add(await GetArchiverJobExpressionAsync(param));
                }

                if (TenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, PreviewFeature.ExportIndex))
                {
                    var isAdmin = RetryUtility.RetryAlways(
                        () => UserService.GetApplicationAdminsAsync().GetAwaiter().GetResult()?.Any(u => u.UserId == TenantLocalValue.LogonUserId)
                        ,3);
                    if(isAdmin ?? false)
                    {
                        exps.Add( Expression4DynamicQuery.GetInExpression(typeof(RMJobMonitor), param, mJobTypeStr,
                            new List<int> { (int)JobType.ExportIndex }.Cast<object>()));
                    }
                }

                if (exps.Count > 0)
                { 
                    allExpressionList.Add(exps.Aggregate(Expression.OrElse));
                }
            }
            // Exclude internal COP download job report jobs directly at query time so counts remain accurate.
            allExpressionList.Add(
                Expression.Not(
                    Expression4DynamicQuery.GetEqualExpression(
                        typeof(RMJobMonitor), param, mJobTypeStr, (int)JobType.DownloadJobReportsForCOP)));
            List<RMJobMonitor> dbResult = new List<RMJobMonitor>();
            if (allExpressionList.Count > 0)
            {
                //将多个Filter和search都用AND合并
                queryExpr = allExpressionList.Aggregate(Expression.AndAlso);
                var lambda = Expression.Lambda<Func<RMJobMonitor, bool>>(queryExpr, param);
                Stopwatch timer = new Stopwatch();
                timer.Start();
                dbResult = JMDao.GetJobs(pager.JumpPage, pager.PageSize, out totalCount, pager.SortBy, (pager.IsSort && !pager.IsDesc), lambda);
                timer.Stop();
                logger.Info("Get Job Monitor Data Take Milliseconds:{0}ms. Lambda is:{1}", timer.ElapsedMilliseconds, lambda.ToString());
            }
            else
            {
                Stopwatch timer = new Stopwatch();
                timer.Start();
                dbResult = JMDao.GetJobs(pager.JumpPage, pager.PageSize, out totalCount, pager.SortBy, (pager.IsSort && !pager.IsDesc),null);
                timer.Stop();
                logger.Info("Get Job Monitor Data Take Milliseconds:{0}ms.", timer.ElapsedMilliseconds);
            }
            responseResult.TotalNumber = totalCount;
            GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
            foreach (var r in dbResult)
            {
                var job = r;
                responseResult.Result.Add(new JMItemInfo()
                {
                    JobId = job.Id,
                    JobTypeCode = job.JobType,
                    JobType = GetJobTypeName(job.JobType),
                    Status = (JobStatus)job.Status,
                    Progress = job.Progress,
                    StartTime = job.StartTime == 0 ? "" : GeneralSettingService.ConvertTiksToDateTime(gls, job.StartTime, true).SimplifyFormatTime,
                    EndTime = job.EndTime == 0 ? I18NEntity.GetString("RM_JS_JM_EndTimePending") : GeneralSettingService.ConvertTiksToDateTime(gls, job.EndTime, true).SimplifyFormatTime,
                    UserName = job.UserName,
                    Joblocation = await GetJobLocationUrl(job),
                    JobPriority = job.JobPriority,
                    JobVersion = job.JobVersion,
                }) ;
            }
            return JsonConvert.SerializeObject(responseResult);
        }

        public async Task<bool> HasRunningFSSyncDataJobAsync(string connectionId)
        {
            var jobType = (int)JobType.FSMyHubDashboard;
            var extensionKeyword = $"\"connectionId\":\"{connectionId}\"";

            return await JMDao.AnyJobAsync(j =>
                j.JobType == jobType
                && (j.Status == (int)JobStatus.Wait
                    || j.Status == (int)JobStatus.InProgress
                    || j.Status == (int)JobStatus.Stopping
                    || j.Status == (int)JobStatus.Pending)
                && (string.IsNullOrEmpty(connectionId)
                    || (!string.IsNullOrEmpty(j.Extension) && j.Extension.Contains(extensionKeyword))));
        }

        private async Task<string> GetJobLocationUrl(RMJobMonitor job) 
        {
            try
            {
                string finallyPath = string.Empty;
                switch (job.JobType)
                {   //enforce rule action 
                    case (int)JobType.RecordsDisposal:
                    case (int)JobType.OneDriveRecordsDisposal:
                    case (int)JobType.EXORecordsDisposal:
                    case (int)JobType.TeamsRecordsDisposal:
                        finallyPath = job.ScopeId;
                        break;
                    case (int)JobType.PhysicalRecordsDisposal:
                        finallyPath = RMLocationDao.GetLocationById(int.Parse(job.ScopeId)).Name;
                        break;
                    case (int)JobType.FSDisposal:
                    case (int)JobType.FSDisposalSchedule:
                    case (int)JobType.FSDisposalByClassCode:
                        finallyPath = job.Extension;
                        break;
                    case (int)JobType.SPOnPremEnforceRuleAction:
                    case (int)JobType.SPOnPremEnforceRuleActionSchedule:
                        finallyPath = job.Extension;
                        break;
                    case (int)JobType.BoxRecordsDisposal:
                    case (int)JobType.GoogleRecordsDisposal:
                        finallyPath = job.Extension;
                        break;
                    //apply setting 
                    case (int)JobType.ApplySharePointSettings:
                    case (int)JobType.ApplyTeamsSettings:
                        var fullPath = string.IsNullOrEmpty(job.ScopeId) ? RMRemoteNodeDao.GetUrlById(job.ContainerId) : RMRemoteNodeDao.GetUrlById(job.ScopeId);
                        var scFullPath = string.IsNullOrEmpty(fullPath) && !string.IsNullOrEmpty(job.Extension) ? SerializerHelper.DeserializeByJsonConvert<JobExtension>(job.Extension).soSCProgress.fullPath : string.Empty;
                        if (!string.IsNullOrEmpty(fullPath))
                        {
                            scFullPath = fullPath;
                        }
                        finallyPath = scFullPath;
                        break;
                    case (int)JobType.EXOApplySetting:
                    case (int)JobType.EXOApplySettingSchedule:
                        finallyPath = MailBoxDao.GetEmailById(job.ContainerId).Email;

                        break;
                    case (int)JobType.SPOnPremApplySetting:
                    case (int)JobType.SPOnPremApplySettingSchedule:
                        var containerPath = !string.IsNullOrEmpty(job.Extension) ? SerializerHelper.DeserializeByJsonConvert<JobExtension>(job.Extension)?.soSCProgress?.fullPath : string.Empty;
                        if (string.IsNullOrEmpty(containerPath))
                        {
                            var localNodeOnpreSetting = LocalNodeDao.GetById(job.ContainerId);
                            containerPath = localNodeOnpreSetting?.NodeLevel == 2 ? localNodeOnpreSetting?.Name : localNodeOnpreSetting?.Url;
                        }
                        finallyPath = containerPath;
                        break;
                    //so job
                    case (int)JobType.RMArchiverBackup:
                    case (int)JobType.RMEndUserArchiverBackup:
                    case (int)JobType.SpecifySitesArchiverBackup:
                    case (int)JobType.TeamsArchiverBackup:
                    case (int)JobType.SpecifyTeamsArchiverBackup:
                        finallyPath = job.ScopeId;
                        break;
                    //so pre scan
                    case (int)JobType.SOPreScan:
                    case (int)JobType.TeamsPreScan:
                        finallyPath = DefaultSecurityContainerNameHelper.GetI18NName(job.ScopeId);
                        break;
                    case (int)JobType.ArchiverRestore:
                    case (int)JobType.StubOopRestore:
                    case (int)JobType.TeamsArchiverRestore:
                    case (int)JobType.TeamsOutPlaceRestore:
                    case (int)JobType.MailBoxArchiverRestore:
                    case (int)JobType.AOSPRestore:
                    case (int)JobType.GoogleArchiverRestore:
                    case (int)JobType.ArchiverToSpoRestore:
                    case (int)JobType.StubArchiverRestore:
                    case (int)JobType.M365InPlaceArchiverRestore:
                        finallyPath = job.ScopeId;
                        break ;
                    case (int)JobType.ArchiverOutPlaceRestore:
                        finallyPath = I18NEntity.GetString(job.ScopeId);
                        break;
                    case (int)JobType.FSArchiverRestore:
                        if(Guid.TryParse(job.ScopeId, out Guid connectionId))
                        {
                            finallyPath = FSConnectionDao.GetConnectionById(connectionId)?.Name;
                        }
                        break;
                    default:
                        finallyPath = string.Empty;
                        break;
                }
                if (finallyPath == "Default Office 365 Group Sites Group")
                {
                    finallyPath = I18NEntity.GetString("RM_SPS_DefaultGroupTeamSiteContainer");
                }
                if (finallyPath == "Default_ SharePoint Sites_ Group")
                {
                    finallyPath = I18NEntity.GetString("RM_SPS_DefaultSharePointSitesGroup");
                }
                if (finallyPath == "Default OneDrive for Business Group")
                {
                    finallyPath = I18NEntity.GetString("RM_SPS_DefaultOneDriveforBusinessGroup");
                }
                if (finallyPath == "Default Private Channel Sites Container")
                {
                    finallyPath = I18NEntity.GetString("RM_SPS_DefaultPrivateChannelSitesContainer");
                }
                if (finallyPath == "Default_ Mailbox_ Group")
                {
                    finallyPath = I18NEntity.GetString("RM_EXO_Default_Container");
                }
                if (finallyPath == "Default_ GoogleUser_ Group")
                {
                    finallyPath = I18NEntity.GetString("RM_GoogleUser_Default_Container");
                }
                if (finallyPath == "Default_ Google_ SharedDrive_ Group")
                {
                    finallyPath = I18NEntity.GetString("RM_GoogleSharedDrive_Default_Container");
                }
                if (finallyPath == JobType.RecordsDisposal.ToString() || finallyPath == "RM_SP_Virtual_Container")
                {
                    finallyPath = I18NEntity.GetString("RM_SP_Virtual_Container");
                }
                if (finallyPath == JobType.OneDriveRecordsDisposal.ToString() || finallyPath == "RM_OD_Virtual_Container")
                {
                    finallyPath = I18NEntity.GetString("RM_OD_Virtual_Container");
                }
                if (finallyPath == JobType.EXORecordsDisposal.ToString() || finallyPath == "RM_EXO_Virtual_Container")
                {
                    finallyPath = I18NEntity.GetString("RM_EXO_Virtual_Container");
                }
                if (finallyPath == JobType.BoxRecordsDisposal.ToString() || finallyPath == "RM_BOX_Virtual_Container")
                {
                    finallyPath = I18NEntity.GetString("RM_BOX_Virtual_Container");
                }
                if (finallyPath == JobType.FSDisposal.ToString() || finallyPath == JobType.FSDisposalByClassCode.ToString() || finallyPath == "RM_FS_Virtual_Container")
                {
                    finallyPath = I18NEntity.GetString("RM_FS_Virtual_Container");
                }
                if (finallyPath == JobType.TeamsRecordsDisposal.ToString() || finallyPath == "RM_Teams_Virtual_Container")
                {
                    finallyPath = I18NEntity.GetString("RM_Teams_Virtual_Container");
                }
                return finallyPath;
            }
            catch (Exception e)
            {
                logger.Info($"GetJobLocationUrl Error Exception -> {e}");
                return string.Empty;
            }               
        
        }


        private Task<bool> IsOpusILAdminAsync()
        {
            return SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.JobMonitorAdmin);
        }

        private Task<bool> IsOpusSOAdminAsync()
        {
            return SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMSOPermissionMasks.JobMonitorAdmin);
        }

        /// <summary>
        /// 该方法用于在job monitor中过滤非super admin用户可以看到的job
        ///1.对于report job，先获取当前user有权限的report job type,使用job的contenterid=userid和job type过滤
        ///2.对于非report job，先查询出user有权限的container集合，再使用container id过滤
        ///3.如果user有physical admin权限，可以看到以下几种physical job的report
        ///4.如果user有fs admin权限，可以看到以下几种fs job的report
        ///5.对于download report job,container id为运行job的user id
        ///6.其余job，例如schedule job，term sync job等contenterid为空，只有super admin可以看到
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        private async Task<Expression> GetSecurityGroupFilterExpressionAsync(ParameterExpression param)
        {
            bool isPhysicalAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.PhysicalAdmin);
            bool isFSAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.FSAdmin);
            bool isSPOnPremAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.SPOnPremEnduser);
            bool isAzureFileShareAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.AzureFSAdmin);
            bool isBoxAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.BoxAdmin);
            bool isGoogleAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.GoogleAdmin);
            bool isRestoreCenterUser = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMSOPermissionMasks.RestoreCenterFullControl);
            var isTeamsAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.TeamsEndUser);
            var containers = await GetContainerIdCollectionAsync();
            if (isPhysicalAdmin)
            {
                if (!containers.Contains(Guid.Empty.ToString()))
                {
                    containers.Add(Guid.Empty.ToString());
                }
            }
            var exps = containers.Select(c => Expression4DynamicQuery.GetEqualExpression(typeof(RMJobMonitor), param, mContainerIdStr, c));           
          
            if (isFSAdmin)
            {
                var jobtypeExps = JobTypeConstants.FSJobTypes.Select(c => Expression4DynamicQuery.GetEqualExpression(typeof(RMJobMonitor), param, mJobTypeStr, c));
                exps = exps.Concat(jobtypeExps);
            }

            if (isPhysicalAdmin)
            {
                var physicalJobtypeExps = JobTypeConstants.PhysicalJobTypes.Select(c => Expression4DynamicQuery.GetEqualExpression(typeof(RMJobMonitor), param, mJobTypeStr, c));
                exps = exps.Concat(physicalJobtypeExps);
            }
            if (isSPOnPremAdmin)
            {
                var onPremJobtypeExps = JobTypeConstants.SPOnPremJobTypes.Select(c => Expression4DynamicQuery.GetEqualExpression(typeof(RMJobMonitor), param, mJobTypeStr, c));
                exps = exps.Concat(onPremJobtypeExps);
            }

            if (isAzureFileShareAdmin)
            {
                var azureFileShareTypeExps = JobTypeConstants.AzureFileShareJobTypes.Select(c => Expression4DynamicQuery.GetEqualExpression(typeof(RMJobMonitor), param, mJobTypeStr, c));
                exps = exps.Concat(azureFileShareTypeExps);
            }

            if (isBoxAdmin)
            {
                var boxTypeExps = JobTypeConstants.BoxJobTypes.Select(c => Expression4DynamicQuery.GetEqualExpression(typeof(RMJobMonitor), param, mJobTypeStr, c));
                exps = exps.Concat(boxTypeExps);
            }

            if (isGoogleAdmin)
            {
                var googleTypeExps = JobTypeConstants.GoogleJobTypes.Select(c => Expression4DynamicQuery.GetEqualExpression(typeof(RMJobMonitor), param, mJobTypeStr, c));
                exps = exps.Concat(googleTypeExps);
            }

            if (isTeamsAdmin)
            {
                var teamsTypeExps = JobTypeConstants.SOTeamsReportTypes.Select(c => Expression4DynamicQuery.GetEqualExpression(typeof(RMJobMonitor), param, mJobTypeStr, c));
                exps = exps.Concat(teamsTypeExps);
            }
            var reportJobTypes = await GetReportJobTypesAsync();
            ////Like report, you can also see your own job
            //reportJobTypes.Add((int)JobType.PhysicalLoanBox);
            //reportJobTypes.Add((int)JobType.PhysicalReturnBox);
            if (reportJobTypes.Count > 0)
            {
                var typeExpression = Expression4DynamicQuery.GetInExpression(typeof(RMJobMonitor), param, mJobTypeStr , reportJobTypes.Cast<object>());
                var userIdExpression = Expression4DynamicQuery.GetEqualExpression(typeof(RMJobMonitor), param, mContainerIdStr, TenantLocalValue.LogonUserId);
                List<Expression> tempList = new List<Expression>()  
                {
                    typeExpression,
                    userIdExpression
                };
                exps = exps.Append(tempList.Aggregate(Expression.AndAlso));
            }

            if (isRestoreCenterUser)
            {
                var restoreCenterTypeExps = new[] { (int)JobType.ArchiverRestore, (int)JobType.ArchiverOutPlaceRestore, (int)JobType.ExportRestoreCenterSeachResult, (int)JobType.FSArchiverRestore, (int)JobType.TeamsArchiverRestore, (int)JobType.TeamsOutPlaceRestore, (int)JobType.MailBoxArchiverRestore, (int)JobType.ArchiverToSpoRestore };
                var typeExpression = Expression4DynamicQuery.GetInExpression(typeof(RMJobMonitor), param, mJobTypeStr, restoreCenterTypeExps.Cast<object>());
                var userNameExpression = Expression4DynamicQuery.GetEqualExpression(typeof(RMJobMonitor), param, mUserNameStr, TenantLocalValue.LogonUserEmail);
                List<Expression> tempList = new List<Expression>()  
                {
                    typeExpression,
                    userNameExpression
                };
                exps = exps.Append(tempList.Aggregate(Expression.AndAlso));
            }

            var filterExpression = exps.Aggregate(Expression.OrElse);
            return filterExpression;
        }

        private async Task<Expression> GetArchiverJobTypeExpression(ParameterExpression param)
        {
            var reportJobTypes = await GetSOReportJobTypesAsync();
            return Expression4DynamicQuery.GetInExpression(typeof(RMJobMonitor), param, mJobTypeStr, 
                JobTypeConstants.ArchiverJobTypes.Concat(
                    JobTypeConstants.ArchiverSpecialJobTypes.Concat(reportJobTypes).Concat(JobTypeConstants.SOArchivedSiteReportTypes)
                    ).Cast<object>());
        }

        private async Task<Expression> GetArchiverJobExpressionAsync(ParameterExpression param)
        {
            var containers = await GetContainerIdCollectionAsync();
            var exps = containers.Select(c => Expression4DynamicQuery.GetEqualExpression(typeof(RMJobMonitor), param, mContainerIdStr, c));
            var reportJobTypes = await GetSOReportJobTypesAsync();
            var typeExpression =  Expression4DynamicQuery.GetInExpression(typeof(RMJobMonitor), param, mJobTypeStr, 
                JobTypeConstants.ArchiverJobTypes.Concat(
                    JobTypeConstants.ArchiverSpecialJobTypes.Concat(reportJobTypes).Concat(JobTypeConstants.SOArchivedSiteReportTypes)
                    ).Cast<object>());
            var userIdExpression = Expression4DynamicQuery.GetEqualExpression(typeof(RMJobMonitor), param, mContainerIdStr, TenantLocalValue.LogonUserId);
            List<Expression> tempList = new List<Expression>()
            {
                typeExpression,
                userIdExpression
            };
            exps = exps.Append(tempList.Aggregate(Expression.AndAlso));
            var isRestoreCenterFullPermission = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMSOPermissionMasks.RestoreCenterFullControl);
            if (isRestoreCenterFullPermission)
            {
                var restoreTypeExpression = JobTypeConstants.RestoreJobTypes.Select(c => Expression4DynamicQuery.GetEqualExpression(typeof(RMJobMonitor), param, mJobTypeStr, c));
                var typeExps = restoreTypeExpression.Aggregate(Expression.OrElse);
                var userNameExpression = Expression4DynamicQuery.GetEqualExpression(typeof(RMJobMonitor), param, mUserNameStr, TenantLocalValue.LogonUserEmail);
                List<Expression> tempExps = new List<Expression>
                    {
                        typeExps,
                        userNameExpression
                    };
                exps = exps.Append(tempExps.Aggregate(Expression.AndAlso));
            }
            var filterExpression = exps.Aggregate(Expression.OrElse);
            return filterExpression;
        }

        private async Task<Expression> GetReviewersFilerExpressionAsync(ParameterExpression param)
        {
            IEnumerable<Expression> exps = new List<Expression>();
            if (LicenseHelperService.HasOpusSOLicense)
            {
                var isRestoreFullPermission = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMSOPermissionMasks.RestoreCenterFullControl);
                if (isRestoreFullPermission)
                {
                    var restoreTypeExpression = JobTypeConstants.RestoreJobTypes.Select(c => Expression4DynamicQuery.GetEqualExpression(typeof(RMJobMonitor), param, mJobTypeStr, c));
                    var typeExps = restoreTypeExpression.Aggregate(Expression.OrElse);
                    var userNameExpression = Expression4DynamicQuery.GetEqualExpression(typeof(RMJobMonitor), param, mUserNameStr, TenantLocalValue.LogonUserEmail);
                    List<Expression> tempExps = new List<Expression>
                    {
                        typeExps,
                        userNameExpression
                    };
                    exps = exps.Append(tempExps.Aggregate(Expression.AndAlso));
                }
            }
            var reportExps = JobTypeConstants.ReviewersJobTypes.Select(c => Expression4DynamicQuery.GetEqualExpression(typeof(RMJobMonitor), param, mJobTypeStr, c));
            var typeExpression = reportExps.Aggregate(Expression.OrElse);
            var userIdExpression = Expression4DynamicQuery.GetEqualExpression(typeof(RMJobMonitor), param, mContainerIdStr, TenantLocalValue.LogonUserId);
            List<Expression> tempList = new List<Expression>()
                {
                    typeExpression,
                    userIdExpression
                };
            exps = exps.Append(tempList.Aggregate(Expression.AndAlso));

            var filterExpression = exps.Aggregate(Expression.OrElse);
            return filterExpression;
        }

        private async Task<List<int>> GetReportJobTypesAsync()
        {
            List<int> types = new List<int>();
            var isSPAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.SPOEnduser);
            var isEXOAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.EXOEnduser);
            var isPhysicalAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.PhysicalAdmin);
            var isFSAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.FSAdmin);
            var isOneDriveAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.OneDriveEnduser);
            var isSPOnPremAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.SPOnPremEnduser);
            var isAFAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.AzureFSAdmin);
            var isBoxAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.BoxAdmin);
            var isGoogleAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.GoogleAdmin);
            var isTeamsAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.TeamsEndUser);

            if (isSPAdmin)
            {
                types.AddRange(JobTypeConstants.SPReportTypes);
            }
            if (isEXOAdmin)
            {
                types.AddRange(JobTypeConstants.EXOReportTypes);
            }
            if (isPhysicalAdmin)
            {
                types.AddRange(JobTypeConstants.PhysicalReportTypes);
            }
            if (isFSAdmin)
            {
                types.AddRange(JobTypeConstants.FSReportTypes);
            }
            if (isOneDriveAdmin)
            {
                types.AddRange(JobTypeConstants.OneDriveReportTypes);
            }
            if (isSPOnPremAdmin)
            {
                types.AddRange(JobTypeConstants.SPOnPremReportTypes);
            }
            if (isBoxAdmin)
            {
                types.AddRange(JobTypeConstants.BoxReportTypes);
            }
            if(isGoogleAdmin)
            {
                types.AddRange(JobTypeConstants.GoogleReportTypes);
            }
            if(isTeamsAdmin)
            {
                types.AddRange(JobTypeConstants.TeamsReportTypes);
            }
            types.AddRange(JobTypeConstants.SpecialJobTypes);
            return types;
        }

        private async Task<List<int>> GetSOReportJobTypesAsync()
        {
            List<int> types = new List<int>();
            var isSOSPAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMSOPermissionMasks.SPOEnduser);
            var isSOOneDriveAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMSOPermissionMasks.OneDriveEnduser);
            var isSOTeamsAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMSOPermissionMasks.TeamsEndUser);

            if (isSOSPAdmin)
            {
                types.AddRange(JobTypeConstants.SOSPReportTypes);
            }
            
            if (isSOOneDriveAdmin)
            {
                types.AddRange(JobTypeConstants.SOOneDriveReportTypes);
            }

            if(isSOTeamsAdmin)
            {
                types.AddRange(JobTypeConstants.SOTeamsReportTypes);
            }

            types.AddRange(JobTypeConstants.SpecialJobTypes);
            return types;
        }

        private async Task<List<string>> GetContainerIdCollectionAsync()
        {
            var collection = new List<string>();
            List<string> userAndGroupUserIds = await UserService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
            var containerGruops = (await RMScopeRoleAssignmentDao.GetAllContainersByUsersAsync(userAndGroupUserIds)).Values;
            foreach (var containers in containerGruops)
            {
                foreach (var container in containers)
                {
                    collection.Add(container.ToString());
                }
            }
            return collection;
        }
       
        public async Task<JMJobSummary> GetDisposalJobSummaryAsync(string jobid)
        {
            var summary = new JMJobSummary();
            try
            {
                var archiverJob = ArhciverJobDao.GetJobByID(jobid);
                var timeZoneId = (await GeneralSettingService.GetGeneralSettingAsync()).TimeZoneId;
                var jobSummaryInfo = Client.JobSummary(ConvertToArchiverJobDto(archiverJob, timeZoneId));
                //var jobSummaryInfo = Client.GetJobSummary(ConvertToArchiverJobDto(archiverJob));
                var rmDisposalSummary = new RMJobSummaryInfos();
                rmDisposalSummary.SummaryItem = new List<RMJobSummaryItem>();
                summary.DisposalSummary = rmDisposalSummary;
                GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();

                foreach (var item in jobSummaryInfo.SummaryItem)
                {
                    var rmItem = new RMJobSummaryItem();
                    rmDisposalSummary.SummaryItem.Add(rmItem);
                    rmItem.Title = HandleI18n(item.Title);
                    rmItem.SummaryRow = new List<RMJobSummaryRow>();
                    foreach (var row in item.SummaryRow)
                    {
                        var rowValue = row.Value;
                        if (row.Key == "Start Time" || row.Key == "Finish Time" || row.Key == "開始時刻" || row.Key == "終了時刻")
                        {
                            rowValue = rowValue.Equals("0") ? I18NEntity.GetString("RM_JS_JM_EndTimePending") : GeneralSettingService.ConvertTiksToDateTime(gls, long.Parse(row.Value), true).SimplifyFormatTime;
                        }
                        if (row.Key == "Scope" || row.Key == "範囲")
                        {
                            rowValue = DefaultSecurityContainerNameHelper.GetI18NName(row.Value);
                        }
                        rmItem.SummaryRow.Add(new RMJobSummaryRow() { Key = row.Key, Value = rowValue });
                    }
                }
                summary.JobId = archiverJob.Id;
                summary.JobType = (JobType)archiverJob.JobType;
            }
            catch (Exception e)
            {
                summary = null;
                logger.Error("Failed to get disposal job summary.JobId:[{0}] ERROR:{1}", jobid, e.ToString());
            }
            return summary;
        }

        #region JobTitle国际化处理
        /// <summary>
        /// JobTitle国际化处理
        /// </summary>
        /// <param name="jobTitle"></param>
        /// <returns></returns>
        private string HandleI18n(string jobTitle)
        {
            switch (Convert.ToString(jobTitle).ToLower())
            {
                case "deletion statistics":
                    {
                        jobTitle = I18NEntity.GetString("RM_JM_Summary_Title_DataDisposal");
                        break;
                    }
                case "record declaration statistics":
                    {
                        jobTitle = I18NEntity.GetString("RM_JM_Summary_Title_DataMove");
                        break;
                    }
                default:
                    break;
            }
            return jobTitle;
        }
        #endregion




        public async Task<JMJobSummary> GetDisposalJobSummaryAsync(SOJob soJob)
        {
            var summary = new JMJobSummary();
            try
            {
                var jobSummaryInfo = Client.JobSummary(soJob);
                //var jobSummaryInfo = Client.GetJobSummary(ConvertToArchiverJobDto(archiverJob));
                var rmDisposalSummary = new RMJobSummaryInfos();
                rmDisposalSummary.SummaryItem = new List<RMJobSummaryItem>();
                summary.DisposalSummary = rmDisposalSummary;
                GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
                foreach (var item in jobSummaryInfo.SummaryItem)
                {
                    var rmItem = new RMJobSummaryItem();
                    rmDisposalSummary.SummaryItem.Add(rmItem);
                    rmItem.Title = item.Title;
                    rmItem.SummaryRow = new List<RMJobSummaryRow>();
                    foreach (var row in item.SummaryRow)
                    {
                        var rowValue = row.Value;
                        if (row.Key == "Start Time" || row.Key == "Finish Time" || row.Key == "開始時刻" || row.Key == "終了時刻")
                        {
                            rowValue = GeneralSettingService.ConvertTiksToDateTime(gls, long.Parse(row.Value), true).SimplifyFormatTime;
                        }
                        if (row.Key == "Comment")
                        {
                            summary.Comment += row.Value;
                        }
                        rmItem.SummaryRow.Add(new RMJobSummaryRow() { Key = row.Key, Value = rowValue });
                    }
                }
                summary.JobId = soJob.Id;
                summary.JobType = (JobType)soJob.Type;
            }
            catch (Exception e)
            {
                summary = null;
                logger.Error("Failed to get disposal job summary.JobId:[{0}] ERROR:{1}", soJob.Id, e.ToString());
            }
            return summary;
        }

        public static SOJob ConvertToArchiverJobDto(RMArchiverJob archiverJob, string timeZoneId)
        {
            return new SOJob()
            {
                Id = archiverJob.Id,
                Type = archiverJob.JobType,
                Category = archiverJob.JobCategory,
                PlanId = archiverJob.PlanId,
                State = archiverJob.StatusFromDAOL,
                StartTime = archiverJob.StartTime,
                FinishTime = archiverJob.EndTime,
                UserName = archiverJob.UserName,
                Scope = archiverJob.Scope,
                TimeZoneId = timeZoneId,
                Tags = (long)GCommon.Contract.Server.ControlPanel.Object.JobTags.RemoteFarm
            };
        }

        public static JobStatus ConvertToRAStatus(int status)
        {
            switch ((AvePoint.Common.JobState)status)
            {
                case AvePoint.Common.JobState.None:
                    return JobStatus.None;
                case AvePoint.Common.JobState.Waiting:
                    return JobStatus.Wait;
                case AvePoint.Common.JobState.InProgress:
                    return JobStatus.InProgress;
                case AvePoint.Common.JobState.Started:
                    return JobStatus.InProgress;
                case AvePoint.Common.JobState.Finished:
                    return JobStatus.Finished;
                case AvePoint.Common.JobState.Failed:
                    return JobStatus.Failed;
                case AvePoint.Common.JobState.Stopped:
                    return JobStatus.Stopped;
                case AvePoint.Common.JobState.Paused:
                    return JobStatus.None;
                case AvePoint.Common.JobState.Skiped:
                    return JobStatus.Skipped;
                case AvePoint.Common.JobState.FinishedException:
                    return JobStatus.FinishWithException;
                case AvePoint.Common.JobState.Pending:
                    return JobStatus.None;
                case AvePoint.Common.JobState.Stopping:
                    return JobStatus.Stopping;
                case AvePoint.Common.JobState.Pausing:
                    return JobStatus.None;
                case AvePoint.Common.JobState.WaitingInServiceBus:
                    return JobStatus.None;
                case AvePoint.Common.JobState.WaitingReSendToServiceBus:
                    return JobStatus.None;
                case AvePoint.Common.JobState.Delay:
                    return JobStatus.None;
                default:
                    return JobStatus.None;
            }
        }

        [Audit(Module = AuditModule.JobMonitor, Category = AuditCategory.JobMonitor, Action = AuditAction.DeleteJobs, AfterHandler = typeof(JobMonitorServiceAuditHandler))]
        public async Task<int> DeleteJobsAsync(List<string> idArray)
        {
            await DeleteJobReportsAsync(idArray);
            return JMDao.DeleteJobs(idArray);
        }

        public async Task<int> DeleteJobsForAgentAsync(List<string> idArray)
        {
            await DeleteJobReportsAsync(idArray);
            return JMDao.DeleteJobs(idArray);
        }

        public Task<int> DeleteJobByTypes(List<JobType> jobTypes)
        {
            return JMDao.DeleteJobByJobTypes(jobTypes);
        }

        public async System.Threading.Tasks.Task DeleteOldOfflineSearchJobAsync(string scopeId, string exceptId)
        {
            List<RMJobMonitor> jobs = JMDao.GetPermittedFinalJobByScopeId((int)JobType.ExplorerOfflineSearch, scopeId, TenantLocalValue.LogonUserId); // JMDao.FindList(a => a.JobType == (int)JobType.ExplorerOfflineSearch && a.ScopeId == scopeId && a.Status != 0 && a.Status != 1).OrderByDescending(a => a.StartTime).ToList();
            if(jobs.Count > 3)
            {
                int keepCount = 0;
                List<string> needDelete = new List<string>();
                foreach (var item in jobs)
                {
                    if (keepCount < 3 && (item.Status == (int)JobStatus.Finished || item.Status == (int)JobStatus.FinishWithException))
                    {
                        keepCount++;
                        continue;
                    }
                    else if(item.Id != exceptId)
                    { 
                        needDelete.Add(item.Id);
                    }
                }
                if(needDelete.Count > 0)
                {
                    logger.Info("try to remove older offline search jobs {0}", string.Join(";", needDelete));
                    await this.DeleteJobsAsync(needDelete);
                }
            }
        }
        [Audit(Module = AuditModule.JobMonitor, Category = AuditCategory.JobMonitor, Action = AuditAction.StopJobs, AfterHandler = typeof(JobMonitorServiceAuditHandler))]
        public int StopJobs(List<string> idArray)
        {
            int result = JMDao.StopJobs(idArray);

            string tenantId = TenantLocalValue.LogonGroupId;
            System.Threading.Tasks.Task.Run(() => SendStopSignalToAgentsAsync(idArray, tenantId));

            return result;
        }

        private async System.Threading.Tasks.Task SendStopSignalToAgentsAsync(List<string> jobIds, string tenantId)
        {
            foreach (var jobId in jobIds)
            {
                try
                {
                    logger.Info("Starting to send stop signal for Main JobId: {0}", jobId);
                    var agentSubJobs = SubJobDao.GetAllSubJobByMainJobId(jobId)
                        .Where(s => !string.IsNullOrEmpty(s.AgentId)
                                 && (s.Status == (int)JobStatus.Stopping || s.Status == (int)JobStatus.InProgress))
                        .ToList();

                    if (!agentSubJobs.Any())
                    {
                        logger.Info("No active sub-jobs found for Main JobId: {0}. Skipping.", jobId);
                        continue;
                    }

                    foreach (var subJob in agentSubJobs)
                    {
                        logger.Info("Starting to send stop signal for SubJob JobId: {0}", subJob.Id);
                        //var workerService = HybridFileSystemWorkerService;
                        //if (workerService == null)
                        //{
                        //    logger.Warn("HybridFileSystemWorkerService is not available. Skipping stop signal for job: {0}", jobId);
                        //    continue;
                        //}
                        await HybridFileSystemWorkerService.StopAgentJobAsync(subJob.Id, tenantId, subJob.AgentId);
                    }
                }
                catch (Exception e)
                {
                    logger.Warn("Failed to send stop signal for job id: {0}. Error: {1}", jobId, e.ToString());
                }
            }
        }

        private string GetJobTypeName(int jobtype)
        {
            //枚举与词条的Key一致
            if (jobtype == (int)JobType.MailBoxBackup)
            {
                return I18NEntity.GetString("RM_JS_JM_JobType_" + JobType.TeamsArchiverBackup.ToString());
            }
            if (JobTypeConstants.ArchivedSiteReportJobTypes.Contains(jobtype))
            {
                return I18NEntity.GetString("RM_JS_JM_JobType_" + JobType.ArchivedSiteReport.ToString());
            }
            return I18NEntity.GetString("RM_JS_JM_JobType_" + ((JobType)jobtype).ToString());
        }

        public async Task<JMItemInfo> GetJobAsync(string id)
        {
            JMItemInfo JSJobInfo = new JMItemInfo();
            try
            {
                RMJobMonitor dbJob = JMDao.GetJob(id); 
                GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
                JSJobInfo = new JMItemInfo()
                {
                    JobId = dbJob.Id,
                    JobType = GetJobTypeName(dbJob.JobType),
                    JobTypeCode = dbJob.JobType,
                    ProfileId = dbJob.ProfileId.HasValue ? dbJob.ProfileId.Value : 0,
                    Status = (JobStatus)dbJob.Status,
                    Progress = dbJob.Progress,
                    StartTime = dbJob.StartTime == 0 ? "" : GeneralSettingService.ConvertTiksToDateTime(gls, dbJob.StartTime, true).SimplifyFormatTime,
                    EndTime = dbJob.EndTime == 0 ? I18NEntity.GetString("RM_JS_JM_EndTimePending") : GeneralSettingService.ConvertTiksToDateTime(gls, dbJob.EndTime, true).SimplifyFormatTime,
                    UserName = dbJob.UserName,
                    Comment = dbJob.Comment,
                    MigrationJobStatus = (int)(Enum.TryParse<ArchiverMigrationJobStatus>(dbJob.AdditionalInformation, out var migrateStatus) ? migrateStatus : ArchiverMigrationJobStatus.None),
                    SubJobCount = dbJob.SubJobCount,
                    JobVersion = dbJob.JobVersion,
                };
            }
            catch (Exception ex)
            {
                logger.Warn("This Job maybe deleted by other user. \t Job Guid:{0}. \n Error:{1}", id, ex.ToString());
            }
            return JSJobInfo;
        }

        //There are preformance issues.
        //The speed is very slow when dealing with large amounts of data, and memory overflow problems may occur. A temporary solution will be released in June.
        //And it will be optimized in the next release.
        public Task<AOSPJMItemInfo> GetAOSPJobAsync(string id)
        {
            return GetAOSPJobAsync(id, null);
        }

        public async Task<AOSPJMItemInfo> GetAOSPJobAsync(string id, Guid o365TenantId)
        {
            return await GetAOSPJobAsync(id, (Guid?)o365TenantId);
        }

        private async Task<AOSPJMItemInfo> GetAOSPJobAsync(string id, Guid? o365TenantId)
        {
            AOSPJMItemInfo JSJobInfo = new AOSPJMItemInfo();
            try
            {
                RMJobMonitor dbJob = JMDao.GetSpecialJob(id);
                logger.Info($"GetAOSPJobAsync: JobId={id}, JobType={dbJob.JobType}, Status={dbJob.Status}, ProfileId={dbJob.ProfileId}, UserName={dbJob.UserName}");
                GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
                JSJobInfo = new AOSPJMItemInfo()
                {
                    JobId = dbJob.Id,
                    JobType = GetJobTypeName(dbJob.JobType),
                    JobTypeCode = dbJob.JobType,
                    ProfileId = dbJob.ProfileId.HasValue ? dbJob.ProfileId.Value : 0,
                    Status = (JobStatus)dbJob.Status,
                    Progress = dbJob.Progress,
                    StartTime = dbJob.StartTime.ToString(),
                    EndTime = dbJob.StartTime.ToString(),
                    UserName = dbJob.UserName,
                    Comment = dbJob.Comment,
                    MigrationJobStatus = (int)(Enum.TryParse<ArchiverMigrationJobStatus>(dbJob.AdditionalInformation, out var migrateStatus) ? migrateStatus : ArchiverMigrationJobStatus.None),
                };

                if (o365TenantId.HasValue && (JSJobInfo.Status == JobStatus.Finished || JSJobInfo.Status == JobStatus.FinishWithException || JSJobInfo.Status == JobStatus.Failed))
                {
                    var settingInfo = await _optimizationSettingsInfoDao.GetSettingInfoByJobIdAsync(dbJob.Id, o365TenantId.Value);
                    var settingMapping = await _siteOptimizationMappingTableDao.GetAllMappingInfoBySettingIdsAsync(o365TenantId.Value, new List<Guid>() { settingInfo.SettingId });
                    var siteInfoes = await _nodeDao.GetSiteInfosBySiteIds(o365TenantId.Value, settingMapping.Select(item => item.NodeId));
                    var jobSiteStatus = new List<AOSPJobSiteStatus>();
                    var archiveJobInfoes = ArchiverSiteMasterIndexDao.GetSiteMastersInfoByMainJobId(dbJob.Id);
                    var deleteJobInfoes = await mRMJobSizeAndCountStatisticsDao.GetJobStatisticsByMainJobIdAsync(JobType.DiscoveryAOSPOptimization, dbJob.Id);
                    var subJobSiteIdsInDB = SubJobDao.GetAllSubJobSiteIdsByParentId(dbJob.Id);
                    foreach (var siteInfo in siteInfoes)
                    {
                        try
                        {
                            if (subJobSiteIdsInDB.Any(keyValue => new Guid(keyValue.Value) == siteInfo.SiteId))
                            {
                                var subJobId = subJobSiteIdsInDB.First(keyValue => new Guid(keyValue.Value) == siteInfo.SiteId).Key;
                                var (status, comment) = await SubJobDao.GetSubJobStatusWithCommentAsync(subJobId);
                                jobSiteStatus.Add(new AOSPJobSiteStatus()
                                {
                                    SiteUrl = siteInfo.Url,
                                    SiteStatus = status == JobStatus.None ? JobStatus.Failed : status,
                                    Comment = comment
                                });
                            }
                            else
                            {
                                var archiverSite = archiveJobInfoes.Where(s => new Guid(s.SiteId) == siteInfo.SiteId).FirstOrDefault();
                                var deleteSite = deleteJobInfoes.Where(s => new Guid(s.SiteId) == siteInfo.SiteId).FirstOrDefault();
                                if (archiverSite != null || deleteSite != null)
                                {
                                    jobSiteStatus.Add(new AOSPJobSiteStatus()
                                    {
                                        SiteUrl = siteInfo.Url,
                                        SiteStatus = JobStatus.Finished,
                                    });
                                }
                                else
                                {
                                    jobSiteStatus.Add(new AOSPJobSiteStatus()
                                    {
                                        SiteUrl = siteInfo.Url,
                                        SiteStatus = JobStatus.Skipped,
                                        Comment = "RM_AOSP_Discovery_Site_Skipped",
                                    });
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Error($"Get aosp job info failed, site id: {siteInfo.SiteId}, error: {e}");
                            jobSiteStatus.Add(new AOSPJobSiteStatus()
                            {
                                SiteUrl = siteInfo.Url,
                                SiteStatus = JobStatus.Failed,
                                Comment = "RM_AOSP_Discovery_Site_Failed",
                            });
                        }
                    }
                    JSJobInfo.jobSiteStatuses = jobSiteStatus;
                }
            }
            catch (Exception ex)
            {
                logger.Warn("This Job maybe deleted by other user. \t Job Guid:{0}. \n Error:{1}", id, ex.ToString());
            }
            logger.Info($"GetAOSPJobAsync: JobId={id}, JobType={JSJobInfo.JobTypeCode}, Status={JSJobInfo.Status}, ProfileId={JSJobInfo.ProfileId}, UserName={JSJobInfo.UserName}");
            return JSJobInfo;
        }

        public async Task<JMItemInfo> GetJobForRecenterAsync(string id)
        {
            JMItemInfo JSJobInfo = new JMItemInfo();
            try
            {
                RMJobMonitor dbJob = JMDao.GetJob(id);
                GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
                var runJobUser = string.Empty;
                if (!string.IsNullOrEmpty(dbJob.AdditionalInformation))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(dbJob.AdditionalInformation);
                    runJobUser = doc.DocumentElement.HasAttribute("ReCenterRunJobUser") ? doc.DocumentElement.GetAttribute("ReCenterRunJobUser"): dbJob.UserName;
                }
                else
                {
                    runJobUser = dbJob.UserName;
                }

                JSJobInfo = new JMItemInfo()
                {
                    JobId = dbJob.Id,
                    JobType = GetJobTypeName(dbJob.JobType),
                    JobTypeCode = dbJob.JobType,
                    ProfileId = dbJob.ProfileId.HasValue ? dbJob.ProfileId.Value : 0,
                    Status = (JobStatus)dbJob.Status,
                    Progress = dbJob.Progress,
                    StartTime = dbJob.StartTime == 0 ? "" : dbJob.StartTime.ToString(),
                    EndTime = dbJob.EndTime == 0 ? I18NEntity.GetString("RM_JS_JM_EndTimePending") : dbJob.EndTime.ToString(),
                    UserName = runJobUser,
                };
            }
            catch (Exception ex)
            {
                logger.Warn("This Job maybe deleted by other user. \t Job Guid:{0}. \n Error:{1}", id, ex.ToString());
            }
            return JSJobInfo;
        }
        public JobStatus GetJobStatus(string id)
        {
            JobStatus status = JobStatus.None;
            try
            {
                RMJobMonitor dbJob = JMDao.GetJob(id);
                if (dbJob != null)
                {
                    status = (JobStatus)dbJob.Status;
                }
            }
            catch (Exception ex)
            {
                logger.Warn("This Job maybe deleted by other user. \t Job Guid:{0}. \n Error:{1}", id, ex.ToString());
            }
            return status;
        }
        public JobType GetJobType(string id)
        {
            JobType jobType = JobType.None;
            try
            {
                RMJobMonitor dbJob = JMDao.GetJob(id);
                if (dbJob != null)
                {
                    jobType = (JobType)dbJob.JobType;
                }
            }
            catch (Exception ex)
            {
                logger.Warn("This Job maybe deleted by other user. \t Job Guid:{0}. \n Error:{1}", id, ex.ToString());
            }
            return jobType;
        }

        public async Task<List<JMItemInfo>> GetJobsAsync(List<string> idArray)
        {
            if (idArray == null)
            {
                throw new Exception("delete job id array is null");
            }
            List<JMItemInfo> JSJobInfos = new List<JMItemInfo>();
            JMItemInfo JSJobInfo = new JMItemInfo();
            try
            {
                List<RMJobMonitor> dbJobs = JMDao.GetJobs(idArray);
                GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
                foreach (var dbJob in dbJobs)
                {
                    JSJobInfo = new JMItemInfo()
                    {
                        JobId = dbJob.Id,
                        JobType = GetJobTypeName(dbJob.JobType),
                        JobTypeCode = dbJob.JobType,
                        ProfileId = dbJob.ProfileId.HasValue ? dbJob.ProfileId.Value : 0,
                        Status = (JobStatus)dbJob.Status,
                        Progress = dbJob.Progress,
                        StartTime = dbJob.StartTime == 0 ? "" : GeneralSettingService.ConvertTiksToDateTime(gls, dbJob.StartTime, true).SimplifyFormatTime,
                        EndTime = dbJob.EndTime == 0 ? I18NEntity.GetString("RM_JS_JM_EndTimePending") : GeneralSettingService.ConvertTiksToDateTime(gls, dbJob.EndTime, true).SimplifyFormatTime,
                        AdditionalInformation = dbJob.AdditionalInformation,
                        SubJobCount = dbJob.SubJobCount,
                        JobVersion = dbJob.JobVersion,
                    };
                    JSJobInfos.Add(JSJobInfo);
                }
            }
            catch (Exception ex)
            {
                logger.Warn("This Job maybe deleted by other user. \t Job Ids:{0}. \n Error:{1}", string.Join(",", idArray), ex.ToString());
            }
            return JSJobInfos;
        }
        public async Task<List<JMItemInfo>> GetJobsForRecenterAsync(List<string> idArray)
        {
            if (idArray == null)
            {
                throw new Exception("delete job id array is null");
            }
            List<JMItemInfo> JSJobInfos = new List<JMItemInfo>();
            JMItemInfo JSJobInfo = new JMItemInfo();
            try
            {
                List<RMJobMonitor> dbJobs = await JMDao.GetJobsAsync(idArray);
                GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
                foreach (var dbJob in dbJobs)
                {
                    JSJobInfo = new JMItemInfo()
                    {
                        JobId = dbJob.Id,
                        JobType = GetJobTypeName(dbJob.JobType),
                        JobTypeCode = dbJob.JobType,
                        ProfileId = dbJob.ProfileId.HasValue ? dbJob.ProfileId.Value : 0,
                        Status = (JobStatus)dbJob.Status,
                        Progress = dbJob.Progress,
                        StartTime = dbJob.StartTime == 0 ? "" : dbJob.StartTime.ToString(),
                        EndTime = dbJob.EndTime == 0 ? I18NEntity.GetString("RM_JS_JM_EndTimePending") : dbJob.EndTime.ToString(),
                        NodeType=dbJob.NodeType
                    };
                    JSJobInfos.Add(JSJobInfo);
                }
            }
            catch (Exception ex)
            {
                logger.Warn("This Job maybe deleted by other user. \t Job Ids:{0}. \n Error:{1}", string.Join(",", idArray), ex.ToString());
            }
            return JSJobInfos;
        }

        /// <summary>
        /// 判断job或者子job是否超时,  直接依赖LastModifyTime  
        /// </summary>
        public void CheckAndDisposeTimeoutJob()
        {
            try
            {
                var watingJobTimeout = GetActualWatingJobTimeout();
                var runningJobScopeIds = GetRunningJobsScopeId(JobType.DisposalActivityManagement);
                List<string> idList = JMDao.GetTimeOutJobIds(mTimeoutPeriod, watingJobTimeout);
                List<string> inQueueJobIds = GetInQueueJobsIds(runningJobScopeIds);
                List<string> realTimeoutIds = idList.Where(id => !inQueueJobIds.Contains(id)).ToList();
                if (!realTimeoutIds.IsNullOrEmpty())
                {
                    logger.Info("Get time out job id {0}", string.Join(",", realTimeoutIds.ToArray()));
                    foreach (string jobId in realTimeoutIds)
                    {
                        if (JobServiceUtility.IsSubJob(jobId))
                        {
                            logger.Warn($"Sub Job {jobId} time out, update state to failed.");
                            JobInfoUpdater.UpdateJobState(jobId, (int)JobStatus.Failed, "RM_JM_Comment_Timeout");
                        }
                        else
                        {
                            logger.Warn($"Job {jobId} time out, update state to failed.");
                            JMDao.UpdateJob(jobId, JobStatus.Failed, "RM_JM_Comment_Timeout", true);
                            try
                            {
                                RMRunningJobRuleMappingDao.RemoveJobRuleMappings(TenantLocalValue.LogonGroupId, jobId);
                            }
                            catch (Exception e)
                            {
                                logger.Warn($"Error occurred while removing running job rule mappings. JobId:{jobId} Error:{e.ToString()}"); 
                            }
                        }
                    }
                }
                else
                {
                    logger.Debug("Check time out, no job is timed out.");
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
            }
            //RMDBContextManager.DisposeCurrentDBContext();
            //List<BaseJobDto> allNeedCheckJobs = GetRunningAndWaitingJobs();
            //foreach (var tempJob in allNeedCheckJobs)
            //{
            //    if (CheckJobTimeout(tempJob))
            //    {
            //        SetJobTimeout(tempJob);
            //    }
            //}
        }

      /*  private void SetJobTimeout(BaseJobDto jobDto)
        {
            try
            {
                logger.Debug("Set job timeout.JobId[{0}]", jobDto.Id);
                JMDao.UpdateJob(jobDto.Id, JobStatus.Failed, "RM_JM_Comment_Timeout", true);
            }
            catch (Exception e)
            {
                logger.Error("Failed to set job timeout.ERROR:{0}", e.ToString());
            }
        }*/

        private int GetActualWatingJobTimeout()
        {
            var watingJobTimeoutConfig = TenantService.GetTimeoutPeriodForWaitingJob();
            var result = watingJobTimeoutConfig > 0 ? watingJobTimeoutConfig * 24 * 60 : mTimeoutPeriodForWaitingJob;
            logger.Info($"The timeout period for the waiting job is {result / (24 * 60)} days, TenantId is {TenantLocalValue.LogonGroupId}");
            return result;
        }

        #endregion

        #region Job Monitor Details Method
        public async Task<string> GetJobDetailsAsync(JMDetailsQuery queryModel, bool isGettingMainJobDetails = false)
        {
            JMDetailsResult result = new JMDetailsResult() { Success = true };
            try
            {
                queryModel.Validate();
                JobType jobType = (JobType)queryModel.JobType;
                if (!TenantService.IsNewOpusTenant())
                {
                    if (jobType == JobType.ArchiverScan || jobType == JobType.ArchiverBackup
                        || jobType == JobType.MigrationArchiverScan || jobType == JobType.MigrationArchiverBackup
                        || jobType == JobType.ExchangeArchiverScan || jobType == JobType.ExchangeArchiverBackup)
                    {
                        return await GetDisposalJobDetailsAsync(queryModel);
                    }
                    if (jobType == JobType.PhysicalDisposal)
                    {
                        return await GetPhysicalDisposalJobDetailsAsync(queryModel);
                    }
                }
                //sharepoint setting job 都使用global的worker
                if (jobType == JobType.SharePointInheritSetting || jobType == JobType.SharePointCustomSetting || jobType == JobType.SharePointScheduleSetting || jobType == JobType.ApplySharePointSettings)
                {
                    jobType = JobType.SharePointGlobalSetting;
                }
                if (jobType == JobType.PhysicalTermSynchronization)
                {
                    jobType = JobType.TermSynchronization;
                }
                if (jobType == JobType.EXOEnforceRetention || jobType == JobType.OneDriveEnforceRetention || jobType == JobType.TeamsEnforceRetention)
                {
                    jobType = JobType.EnforceRetention;
                }
                int totalCount = 0;
                StringBuilder condition = new StringBuilder();
                StringBuilder statusCondition = new StringBuilder();
                StringBuilder searchCondition = new StringBuilder();
                StringBuilder actionTabCondition = new StringBuilder();
                StringBuilder archiverActionCondition = new StringBuilder();
                Dictionary<string, object> addValues = new Dictionary<string, object>();

                List<StringBuilder> conditionList = new List<StringBuilder>();
                bool hasStatusCondition = !isGettingMainJobDetails ? queryModel.StatusFilters.Length > 0 : (queryModel.SubJobStatusFilters is not null && queryModel.SubJobStatusFilters.Length > 0);
                bool hasSearchCondition = (!string.IsNullOrEmpty(queryModel.SearchValue)) && queryModel.SearcheKeys.Length > 0;
                bool hasActionTabCondition = queryModel.ActionTabFilters != null && queryModel.ActionTabFilters.Length > 0;
                bool hasArchiverActionCondition = queryModel.ArchiverActionFilters != null && queryModel.ArchiverActionFilters.Length > 0;
                bool isFristCondition = true;
                int i = 0;
                if (hasStatusCondition)
                {
                    if (!isGettingMainJobDetails)
                    {
                        foreach (JobDetailsStatus filter in queryModel.StatusFilters)
                        {
                            var sKey = string.Format(PARAMTERS, "Status" + i);
                            if (isFristCondition)
                            {
                                //statusCondition.AppendFormat("Status='{0}' ", (int)filter);
                                statusCondition.AppendFormat("Status={0} ", sKey);
                                addValues.Add(sKey, (int)filter);
                                isFristCondition = false;
                            }
                            else
                            {
                                //statusCondition.AppendFormat("OR Status='{0}' ", (int)filter);
                                statusCondition.AppendFormat("OR Status={0} ", sKey);
                                addValues.Add(sKey, (int)filter);
                            }
                            i++;
                        }
                    }
                    else
                    {
                        foreach (var filter in queryModel.SubJobStatusFilters)
                        {
                            var sKey = string.Format(PARAMTERS, "Status" + i);
                            if (isFristCondition)
                            {
                                //statusCondition.AppendFormat("Status='{0}' ", (int)filter);
                                statusCondition.AppendFormat("Status={0} ", sKey);
                                addValues.Add(sKey, (int)filter);
                                isFristCondition = false;
                            }
                            else
                            {
                                //statusCondition.AppendFormat("OR Status='{0}' ", (int)filter);
                                statusCondition.AppendFormat("OR Status={0} ", sKey);
                                addValues.Add(sKey, (int)filter);
                            }
                            i++;
                        }
                    }
                    conditionList.Add(statusCondition);
                }
                isFristCondition = true;
                if (hasSearchCondition)
                {
                    foreach (var searchKey in queryModel.SearcheKeys)
                    {
                        String safeKey = SecurityUtils.SanitizeSQLParameterName(searchKey);
                        var sKey = string.Format(PARAMTERS, safeKey + i);
                        var transferedValue = queryModel.SearchValue.TransferSpecialCharacter();
                        var sValue = string.Format("%" + transferedValue + "%");
                        if (isFristCondition)
                        {
                            //searchCondition.AppendFormat("{0} LIKE '%{1}%' ", searchKey, queryModel.SearchValue);
                            searchCondition.AppendFormat("[{0}] LIKE {1} ", safeKey, sKey);
                            addValues.Add(sKey, sValue);
                            isFristCondition = false;
                        }
                        else
                        {
                            //searchCondition.AppendFormat("OR {0} LIKE '%{1}%' ", searchKey, queryModel.SearchValue);
                            searchCondition.AppendFormat("OR [{0}] LIKE {1} ", safeKey, sKey);
                            addValues.Add(sKey, sValue);
                        }
                        i++;
                    }
                    conditionList.Add(searchCondition.Append("ESCAPE '\\'"));
                }
                isFristCondition = true;
                if (hasActionTabCondition)
                {
                    foreach (var filter in queryModel.ActionTabFilters)
                    {
                        var sKey = string.Format(PARAMTERS, "ActionTab" + i);
                        if (isFristCondition)
                        {
                            //statusCondition.AppendFormat("Status='{0}' ", (int)filter);
                            actionTabCondition.AppendFormat("ActionTab={0} ", sKey);
                            addValues.Add(sKey, (int)filter);
                            isFristCondition = false;
                        }
                        else
                        {
                            //statusCondition.AppendFormat("OR Status='{0}' ", (int)filter);
                            actionTabCondition.AppendFormat("OR ActionTab={0} ", sKey);
                            addValues.Add(sKey, (int)filter);
                        }
                        i++;
                    }
                    conditionList.Add(actionTabCondition);
                }

                isFristCondition = true;
                if (hasArchiverActionCondition)
                {
                    foreach (var filter in queryModel.ArchiverActionFilters)
                    {
                        var sKey = string.Format(PARAMTERS, "Action" + i);
                        if (isFristCondition)
                        {
                            archiverActionCondition.AppendFormat("Action={0} ", sKey);
                            addValues.Add(sKey, filter);
                            isFristCondition = false;
                        }
                        else
                        {
                            archiverActionCondition.AppendFormat("OR Action={0} ", sKey);
                            addValues.Add(sKey, filter);
                        }
                        i++;
                    }
                    conditionList.Add(archiverActionCondition);
                }

                if (conditionList.Count <= 1)
                {
                    condition.Append(statusCondition);
                    condition.Append(searchCondition);
                    condition.Append(actionTabCondition);
                    condition.Append(archiverActionCondition);
                }
                else
                {
                    bool isFirst = true;
                    foreach (var confition in conditionList)
                    {
                        if (isFirst)
                        {
                            condition.Append($"({confition})");
                            isFirst = false;
                        }
                        else
                        {
                            condition.Append($" And ({confition})");
                        }
                    }
                }
                var jobDto = new BaseJobDto()
                {
                    Id = queryModel.JobID,
                    JobType = (int)jobType,
                    AddValues = addValues
                };

                if (jobType == JobType.ArchiverScan || jobType == JobType.ArchiverBackup
                    || jobType == JobType.MigrationArchiverScan || jobType == JobType.MigrationArchiverBackup
                    || jobType == JobType.ExchangeArchiverScan || jobType == JobType.ExchangeArchiverBackup 
                    || jobType == JobType.PhysicalDisposal)
                {
                    var archiverJob = ArhciverJobDao.GetJobByID(queryModel.JobID);
                    var tenantGroupEmail = TenantInfoDao.GetTenantInfo(TenantLocalValue.LogonGroupId)?.RegisterEmail;
                    jobDto = new BaseJobDto()
                    {
                        Id = queryModel.JobID,
                        JobType = archiverJob.JobType,
                        PlanId = archiverJob.PlanId,
                        Category = archiverJob.JobCategory,
                        TenantGroupEmail = tenantGroupEmail
                    };
                }

                if (jobType == JobType.MigrationArchiverRestore 
                    || jobType == JobType.MigrationArchiverRetention 
                    || (jobType == JobType.MigrationArchiverFileLevelRetention || queryModel.JobID.StartsWith("DD"))
                    || jobType == JobType.ArchiverDeduplication)
                {
                    var job = JMDao.GetJob(queryModel.JobID);
                    ArchiverMigratedJobExtension jobExtension = new ArchiverMigratedJobExtension();
                    try
                    {
                        jobExtension = SerializerHelper.DeserializeByJsonConvert<ArchiverMigratedJobExtension>(job.AdditionalInformation);
                    }
                    catch (Exception e)
                    {
                        logger.Warn($"Deserialize ArchiverMigratedJobExtension Error {e}");
                    }
                    var tenantGroupEmail = TenantInfoDao.GetTenantInfo(TenantLocalValue.LogonGroupId)?.RegisterEmail;
                    jobDto = new BaseJobDto()
                    {
                        Id = queryModel.JobID,
                        JobType = job.JobType,
                        PlanId = jobExtension.PlanId,
                        Category = jobExtension.JobCategory,
                        TenantGroupEmail = tenantGroupEmail
                    };
                }
                jobDto.IsMainJob = isGettingMainJobDetails;
                // Show job details when the job is running
                if (JobServiceUtility.NewJobDetailsJobs.Contains(jobDto.JobType))
                {
                    if (JobServiceUtility.IsSubJob(queryModel.JobID))
                    {
                        var subJob =  SubJobDao.GetSubJob(queryModel.JobID);
                        if (subJob is not null)
                        {
                            var job = JMDao.GetJob(subJob.ParentId);
                            jobDto.NeedQueryFromUploadLocation = job.JobVersion == JobVersion.UnMerged && !JobServiceUtility.IsFinalState(subJob.Status);
                        }
                    }
                    else
                    {
                        var job =  JMDao.GetJob(queryModel.JobID);
                        jobDto.NeedQueryFromUploadLocation = job.JobVersion == JobVersion.UnMerged && !JobServiceUtility.IsFinalState(job.Status);
                    }
                }

                var data = JDService.GetData(queryModel.PageSize, queryModel.CurrentPage, ref totalCount, condition.ToString(), jobDto);
                ReBuildTheDetails(data);
                result.Details = data;

                result.TotalNumber = totalCount;
            }
            catch (Exception e)
            {
                logger.Error("Get Job Details Error. {0}", e.ToString());
                result.Success = false;
            }
            return JsonConvert.SerializeObject(result);
        }
        private void ReBuildTheDetails(IEnumerable<JMJobDetails> datas)
        {
            if (datas == null)
            {
                return;
            }

            foreach (var detail in datas)
            {
                if (detail == null)
                {
                    continue;
                }

                if (detail.Status == JobDetailsStatus.ContainerFailed)
                {
                    detail.Status = JobDetailsStatus.Skipped;
                }
            }
        }
        public async Task<JMAOSPDetailsResult> GetAOSPJobDetailsAsync(JMDetailsQuery queryModel)
        {
            JMDetailsResult result = new JMDetailsResult() { Success = true };
            try
            {
                queryModel.Validate();
                var jobType = (JobType)queryModel.JobType;
                //sharepoint setting job 都使用global的worker

                int totalCount = 0;
                var condition = new StringBuilder();
                var addValues = new Dictionary<string, object>();
                int i = 0;

                if (jobType == JobType.DiscoveryAOSPOptimization)
                {
                    var itemKey = string.Format(PARAMTERS, "Level" + i++);
                    var versionKey = string.Format(PARAMTERS, "Level" + i++);
                    condition.AppendFormat("(Level={0} or Level={1})", itemKey, versionKey);
                    addValues.Add(itemKey, "RM_JS_Rule_ObjectLevel_Item");
                    addValues.Add(versionKey, "RM_JS_Rule_ObjectLevel_ItemVersion");

                    var backupTabKey = string.Format(PARAMTERS, "ActionTab" + i++);
                    var deleteTabKey = string.Format(PARAMTERS, "ActionTab" + i++);
                    condition.AppendFormat(" And (ActionTab={0} or ActionTab={1})", backupTabKey, deleteTabKey);
                    addValues.Add(backupTabKey, ActionTab.Backup);
                    addValues.Add(deleteTabKey, ActionTab.Action);
                }
                else
                {
                    var sKey = string.Format(PARAMTERS, "Level" + i++);
                    condition.AppendFormat("Level={0} ", sKey);
                    addValues.Add(sKey, "RM_JS_Rule_ObjectLevel_Item");
                }

                var jobDto = new BaseJobDto()
                {
                    Id = queryModel.JobID,
                    JobType = (int)jobType,
                    AddValues = addValues
                };

                var data = JDService.GetData(queryModel.PageSize, queryModel.CurrentPage, ref totalCount, condition.ToString(), jobDto);

                result.Details = data;

                result.TotalNumber = totalCount;

                var resultStr = JsonConvert.SerializeObject(result);
                return JsonConvert.DeserializeObject<JMAOSPDetailsResult>(resultStr);
            }
            catch (Exception e)
            {
                logger.Error("Get Job Details Error. {0}", e.ToString());
                result.Success = false;
            }
            return null;
        }

        public HSMArchvierJobDetailsResult GetHSMJobFailedDetails(JMDetailsQuery queryModel)
        {
            JMDetailsResult result = new JMDetailsResult() { Success = true };
            try
            {
                queryModel.Validate();
                var jobType = JobType.ArchiverByHSMXml;

                int totalCount = 0;
                var condition = new StringBuilder();
                var addValues = new Dictionary<string, object>();
                int i = 0;

                var itemKey = string.Format(PARAMTERS, "Level" + i++);
                var versionKey = string.Format(PARAMTERS, "Level" + i++);
                condition.AppendFormat("(Level={0} or Level={1})", itemKey, versionKey);
                addValues.Add(itemKey, "RM_JS_Rule_ObjectLevel_Item");
                addValues.Add(versionKey, "RM_JS_Rule_ObjectLevel_ItemVersion");

                var backupTabKey = string.Format(PARAMTERS, "ActionTab" + i++);
                condition.AppendFormat(" And ActionTab={0}", backupTabKey);
                addValues.Add(backupTabKey, ActionTab.Backup);

                var statusKey = string.Format(PARAMTERS, "Status" + i++);
                condition.AppendFormat(" And Status={0}", statusKey);
                addValues.Add(statusKey, (int)JobDetailsStatus.Failed);

                var jobDto = new BaseJobDto()
                {
                    Id = queryModel.JobID,
                    JobType = (int)jobType,
                    AddValues = addValues
                };

                var data = JDService.GetData(queryModel.PageSize, queryModel.CurrentPage, ref totalCount, condition.ToString(), jobDto);

                result.Details = data;

                result.TotalNumber = totalCount;

                var resultStr = JsonConvert.SerializeObject(result);
                return JsonConvert.DeserializeObject<HSMArchvierJobDetailsResult>(resultStr);
            }
            catch (Exception e)
            {
                logger.Error("Get Job Details Error. {0}", e.ToString());
                result.Success = false;
            }
            return null;
        }

        private async Task<string> GetPhysicalDisposalJobDetailsAsync(JMDetailsQuery queryModel)
        {
            JMDetailsResult jdr = new JMDetailsResult();
            try
            {
                var archiverJob = ArhciverJobDao.GetJobByID(queryModel.JobID);
                var statesFilter = queryModel.StatusFilters.Length == 0 ? new int[] { 0, 1, 2 } : queryModel.StatusFilters.Select(f => (int)f).ToArray();
                var transferedVal = queryModel.SearchValue.TransferSpecialCharacter();
                var result = Client.JobDetails(new ArchiverJobDto() { Id = archiverJob.Id, JobType = archiverJob.JobType, JobCategory = archiverJob.JobCategory, PlanId = archiverJob.PlanId },
                    new List<string> { queryModel.JobID }, transferedVal, (queryModel.CurrentPage - 1) * queryModel.PageSize, queryModel.PageSize, statesFilter, queryModel.EntityTypeFilters);
                List<JMPhysicalDisposalJobDetails> list = new List<JMPhysicalDisposalJobDetails>();
                GeneralSettingModel timeSetting = await GeneralSettingService.GetGeneralSettingAsync();
                foreach (var job in result.Values)
                {
                    var soJob = job as SOJobDetailDto;
                    var entityType = (JobReportDetailEntityType)soJob.EntityType;
                    switch (entityType)
                    {
                        case JobReportDetailEntityType.ArchiveDeletion://Deletion 
                        case JobReportDetailEntityType.RecordManager://Record Declaration
                            list.Add(new JMPhysicalDisposalJobDetails()
                            {
                                ObjectName = soJob.MediaHost,
                                ItemType = I18NEntity.GetString(soJob.Type),
                                FullPath = JobReportUtility.ReplaceRootLocationName(soJob.SrcURL),
                                DestinationPath = JobReportUtility.ReplaceRootLocationName(soJob.DestURL),
                                //Status = (JobDetailsStatus)Enum.Parse(typeof(JobDetailsStatus), soJob.Status),
                                StatusStr = soJob.Status,
                                ActionType = I18NEntity.GetString(soJob.DataOperation),
                                Comment = I18NEntity.GetString(soJob.Comment),
                            });
                            break;
                        default:
                            break;
                    }
                }
                jdr.Details = list;
                jdr.TotalNumber = result.TotalLength;
            }
            catch (Exception e)
            {
                logger.Error("get disposal job details error: {0}", e.ToString());
                jdr.IsDeleted = true;
                jdr.Success = false;
            }
            return JsonConvert.SerializeObject(jdr);
        }

        private async Task<string> GetDisposalJobDetailsAsync(JMDetailsQuery queryModel)
        {
            JMDetailsResult jdr = new JMDetailsResult();
            try
            {
                var archiverJob = ArhciverJobDao.GetJobByID(queryModel.JobID);
                var statesFilter = queryModel.StatusFilters.Length == 0 ? new int[] { 0, 1, 2 } : queryModel.StatusFilters.Select(f => (int)f).ToArray();
                var transferedVal = queryModel.SearchValue.TransferSpecialCharacter();
                var result = Client.JobDetails(new ArchiverJobDto() { Id = archiverJob.Id, JobType = archiverJob.JobType, JobCategory = archiverJob.JobCategory, PlanId = archiverJob.PlanId },
                    new List<string> { queryModel.JobID }, transferedVal, (queryModel.CurrentPage - 1) * queryModel.PageSize, queryModel.PageSize, statesFilter, queryModel.EntityTypeFilters);
                List<JMDisposalJobDetails> list = new List<JMDisposalJobDetails>();
                GeneralSettingModel timeSetting = await GeneralSettingService.GetGeneralSettingAsync();
                foreach (var job in result.Values)
                {
                    var soJob = job as SOJobDetailDto;
                    var entityType = (JobReportDetailEntityType)soJob.EntityType;
                    if (soJob.Status == "Failed")
                    {
                        soJob.Status =  I18NEntity.GetString("RM_JS_JMD_Status_Failed") ;
                    }
                    switch (entityType)
                    {
                        case JobReportDetailEntityType.Export://Export
                            list.Add(new JMDisposalJobDetails()
                            {
                                DetailsTab = I18NEntity.GetString("RM_JS_JM_EntityType_Export"),
                                Type = string.IsNullOrEmpty(soJob.Type) ? I18NEntity.GetString("RM_Archiver_JobDetailExceptionLevel") : soJob.Type,
                                SourceURL = JobReportUtility.ReplaceRootLocationName(soJob.SrcURL),//related
                                Size = soJob.Size,
                                StatusStr = soJob.Status,
                                Action = I18NEntity.GetString(soJob.DataOperation),
                                FinishTime = GeneralSettingService.ConvertTiksToDateTime(timeSetting, soJob.Date, true).FormaTime,
                                Comment = I18NEntity.GetString(soJob.Comment),
                            });
                            break;
                        case JobReportDetailEntityType.NormalInfo://Backup
                            list.Add(new JMDisposalJobDetails()
                            {
                                DetailsTab = I18NEntity.GetString("RM_JS_JM_EntityType_Backup"),
                                Type = string.IsNullOrEmpty(soJob.Type)? I18NEntity.GetString("RM_Archiver_JobDetailExceptionLevel"): soJob.Type,
                                SourceURL = JobReportUtility.ReplaceRootLocationName(soJob.SrcURL),//related
                                Size = soJob.Size,
                                StatusStr = soJob.Status,
                                FinishTime = GeneralSettingService.ConvertTiksToDateTime(timeSetting, soJob.Date, true).SimplifyFormatTime,
                                RuleName = soJob.RuleName,
                                Action = I18NEntity.GetString(soJob.DataOperation),
                                Comment = I18NEntity.GetString(soJob.Comment),
                            });
                            break;
                        case JobReportDetailEntityType.ArchiveDeletion://Deletion 
                            list.Add(new JMDisposalJobDetails()
                            {
                                DetailsTab = I18NEntity.GetString("RM_JS_JMD_Grid_Action"),
                                Type = string.IsNullOrEmpty(soJob.Type) ? I18NEntity.GetString("RM_Archiver_JobDetailExceptionLevel") : soJob.Type,
                                SourceURL = JobReportUtility.ReplaceRootLocationName(soJob.SrcURL),//related
                                Size = soJob.Size,
                                StatusStr = soJob.Status,
                                FinishTime = GeneralSettingService.ConvertTiksToDateTime(timeSetting, soJob.Date, true).SimplifyFormatTime,
                                Action = I18NEntity.GetString(soJob.DataOperation),
                                Comment = I18NEntity.GetString(soJob.Comment),
                            });
                            break;
                        case JobReportDetailEntityType.RecordManager://Record Declaration
                            list.Add(new JMDisposalJobDetails()
                            {
                                DetailsTab = I18NEntity.GetString("RM_JS_JM_EntityType_RecordDeclaration"),
                                Type = soJob.Type,
                                SourceURL = JobReportUtility.ReplaceRootLocationName(soJob.SrcURL),
                                DestinationURL = JobReportUtility.ReplaceRootLocationName(soJob.DestURL),
                                Size = soJob.Size,
                                StatusStr = soJob.Status,
                                Action = I18NEntity.GetString(soJob.DataOperation),
                                FinishTime = GeneralSettingService.ConvertTiksToDateTime(timeSetting, soJob.Date, true).SimplifyFormatTime,
                                Comment = I18NEntity.GetString(soJob.Comment),
                            });
                            break;
                        default:
                            break;
                    }
                }
                jdr.Details = list;
                jdr.TotalNumber = result.TotalLength;
            }
            catch (Exception e)
            {
                logger.Error("get disposal job details error: {0}", e.ToString());
                jdr.IsDeleted = true;
                jdr.Success = false;
            }
            return JsonConvert.SerializeObject(jdr);
        }
        public string GetTermSelection(string jobId)
        {
            int totalCount = 0;
            return JsonConvert.SerializeObject(JDService.GetDataForTermSelection(-1, 1, ref totalCount, "", new BaseJobDto()
            {
                Id = jobId,
                JobType = (int)JobType.BCSTermUsageReport
            }));
        }

        public async Task<JMDetailsResult> GetJobProgress(JMProgressDetailsQuery queryModel)
        {
            JMDetailsResult result = new() { Success = true };
            try
            {
                queryModel.Validate();
                JobType jobType = (JobType)queryModel.JobType;
                if (!TenantService.IsNewOpusTenant())
                {
                    result.Success = false;
                    return result;
                }
                var dbJob = JMDao.GetJob(queryModel.JobID);
                if (dbJob is null)
                {
                    result.Success = false;
                    return result;
                }
                int totalCount = 0;
                StringBuilder statusCondition = new();
                StringBuilder searchCondition = new();
                Dictionary<string, object> addValues = new Dictionary<string, object>();

                List<StringBuilder> conditionList = [];
                bool hasStatusCondition = queryModel.StatusFilter is not null && queryModel.StatusFilter.Length > 0;
                bool hasSearchCondition = (!string.IsNullOrEmpty(queryModel.SearchValue)) && queryModel.SearchKeys is not null && queryModel.SearchKeys.Length > 0;
                bool isFirstCondition = true;
                int i = 0;
                if (hasStatusCondition)
                {
                    foreach (ProgressStatus filter in queryModel.StatusFilter)
                    {
                        var sKey = string.Format(PARAMTERS, "ProgressStatus" + i);
                        if (isFirstCondition)
                        {
                            statusCondition.AppendFormat("ProgressStatus={0} ", sKey);
                            addValues.Add(sKey, (int)filter);
                            isFirstCondition = false;
                        }
                        else
                        {
                            statusCondition.AppendFormat("OR ProgressStatus={0} ", sKey);
                            addValues.Add(sKey, (int)filter);
                        }
                        i++;
                    }
                    conditionList.Add(statusCondition);
                }
                isFirstCondition = true;
                if (hasSearchCondition)
                {
                    foreach (var searchKey in queryModel.SearchKeys)
                    {
                        var safeKey = SecurityUtils.SanitizeSQLParameterName(searchKey);
                        var sKey = string.Format(PARAMTERS, safeKey + i);
                        var transferedValue = queryModel.SearchValue.TransferSpecialCharacter();
                        var sValue = string.Format("%" + transferedValue + "%");
                        if (isFirstCondition)
                        {
                            searchCondition.AppendFormat("[{0}] LIKE {1} ", safeKey, sKey);
                            addValues.Add(sKey, sValue);
                            isFirstCondition = false;
                        }
                        else
                        {
                            searchCondition.AppendFormat("OR [{0}] LIKE {1} ", safeKey, sKey);
                            addValues.Add(sKey, sValue);
                        }
                        i++;
                    }
                    conditionList.Add(searchCondition.Append("ESCAPE '\\'"));
                }

                StringBuilder condition = new StringBuilder();
                if (conditionList.Count <= 1)
                {
                    condition.Append(statusCondition);
                    condition.Append(searchCondition);
                }
                else
                {
                    bool isFirst = true;
                    foreach (var confition in conditionList)
                    {
                        if (isFirst)
                        {
                            condition.Append($"({confition})");
                            isFirst = false;
                        }
                        else
                        {
                            condition.Append($" And ({confition})");
                        }
                    }
                }
                var jobDto = new BaseJobDto()
                {
                    Id = queryModel.JobID,
                    JobType = (int)jobType,
                    AddValues = addValues,
                    IsGettingProgress = true,
                    NeedQueryFromUploadLocation = !JobServiceUtility.IsFinalState(dbJob.Status),
                };

                var data = JDService.GetData(queryModel.PageSize, queryModel.PageNumber, ref totalCount, condition.ToString(), jobDto);
                ReBuildTheDetails(data);
                result.Details = data;

                result.TotalNumber = totalCount;
                result.JobProgressDetails = await BuildProgressDetailJobAsync(queryModel.JobID, result.TotalNumber);

            }
            catch (Exception e)
            {
                logger.Error("Get Job Details Error. {0}", e.ToString());
                result.Success = false;
            }
            return result;
        }

        #endregion

        #region Create And UpDate Job Method

        public string CreateJob(JobType jobType)
        {
            return CreateJob(jobType, "");
        }

        public string CreateJob(JobType jobType, string jobRunBy, string containerId = null,string scopedId = null, string fullPath = null)  
        {
            var id = GenerateJobId(jobType);
            if(_jobTypesAssociateWithGControl.Contains(jobType) && _tenantService.HasInitGControlPlatForm().Result)
            {
                var gControlJobId = GControlPlatformJobService.CreatePlatformJob(id, fullPath, jobType, jobRunBy).GetAwaiter().GetResult();
                logger.Info($"Created GControl job for job type: {jobType}, job id: {id}, gControl job id: {gControlJobId}");
                return JMDao.CreateJobWithGControlJobId(id, gControlJobId.ToString(), jobType, jobRunBy, containerId, scopedId, fullPath);
            }
            return JMDao.CreateJob(id, jobType, jobRunBy, containerId, scopedId, fullPath);
        }

        public async Task<string> CreateDiscoveryJobAsync(string jobRunBy, Guid mainJobId, Guid discoveryJobId)
        {
            var id = GenerateJobId(JobType.DiscoveryJob);
            await JMDao.CreateDiscoveryJobAsync(id, jobRunBy, mainJobId, discoveryJobId, JobType.DiscoveryJob);
            return id;
        }

        public async Task<string> CreateDiscoveryJobNextVersionAsync(string jobRunBy, Guid mainJobId, JobType jobType)
        {
            var id = GenerateJobId(jobType);
            if (_jobTypesAssociateWithGControl.Contains(jobType) && _tenantService.HasInitGControlPlatForm().Result)
            {
                var gControlJobId = GControlPlatformJobService.CreatePlatformJob(id, null, jobType, jobRunBy).GetAwaiter().GetResult();
                logger.Info($"Created GControl job for job type: {jobType}, job id: {id}, gControl job id: {gControlJobId}");
                return await JMDao.CreateDiscoveryJobWithGControlJobId(id, gControlJobId.ToString(), jobRunBy, mainJobId, Guid.Empty, jobType);
            }
            await JMDao.CreateDiscoveryJobAsync(id, jobRunBy, mainJobId, Guid.Empty, jobType);
            return id;
        }

        public async Task<string> CreateDiscoveryRetryJobAsync(string jobRunBy, Guid mainJobId, Guid discoveryJobId)
        {
            var id = GenerateJobId(JobType.DiscoveryReCalculate);
            await JMDao.CreateDiscoveryJobAsync(id, jobRunBy, mainJobId, discoveryJobId, JobType.DiscoveryReCalculate);
            return id;
        }

        public string CreateJobWithJobId(string jobId, JobType jobType, string jobRunBy)
        {
            return JMDao.CreateJob(jobId, jobType, jobRunBy);
        }

        public string CreateJobWithScopeId(JobType jobType, string jobRunBy, string scopeId, string containerId = null,string fullPath = null,string jobConflictExtension = null)
        {
            var id = GenerateJobId(jobType);
            if(_jobTypesAssociateWithGControl.Contains(jobType) && _tenantService.HasInitGControlPlatForm().Result)
            {
                var gControlJobId = GControlPlatformJobService.CreatePlatformJob(id, fullPath, jobType, jobRunBy).GetAwaiter().GetResult();
                logger.Info($"Created GControl job for job type: {jobType}, job id: {id}, gControl job id: {gControlJobId}");
                return JMDao.CreateJobWithScopeIdAndWithGControlJobId(id, gControlJobId.ToString(), jobType, jobRunBy, scopeId, containerId, JobStatus.Wait, null, fullPath , jobConflictExtension);;
            }
            return JMDao.CreateJobWithScopeId(id, jobType, jobRunBy, scopeId, containerId, JobStatus.Wait, null, fullPath , jobConflictExtension);
        }        
        
        public string CreateJobWithScopeId(string jobId, JobType jobType, string jobRunBy, string scopeId, string containerId = null,string fullPath = null,string jobConflictExtension = null)
        {
            return JMDao.CreateJobWithScopeId(jobId, jobType, jobRunBy, scopeId, containerId, JobStatus.Wait, null, fullPath , jobConflictExtension);
        }

        public string CreateJobWithScopeIdForTeams(JobType jobType, string jobRunBy, string scopeId, string additionalInformation , string containerId = null, string fullPath = null, string jobConflictExtension = null)
        {
            var id = GenerateJobId(jobType);
            return JMDao.CreateJobWithScopeIdForTeams(id, jobType, jobRunBy, scopeId, additionalInformation, containerId, JobStatus.Wait, null, fullPath, jobConflictExtension);
        }

        public string CreateJobWithScopeIdAndJobId(string jobId, JobType jobType, string jobRunBy, string scopeId, string containerId = null,string fullPath = null, string jobConflictExtension = null)
        {
            return JMDao.CreateJobWithScopeId(jobId, jobType, jobRunBy, scopeId, containerId, JobStatus.Wait, null, fullPath, jobConflictExtension);
        }

        public string CreateJobWithScopeId(JobType jobType, JobStatus jobStatus, string jobRunBy, string scopeId, string containerId = null, string failedReason = null)
        {
            var id = GenerateJobId(jobType);
            return JMDao.CreateJobWithScopeId(id, jobType, jobRunBy, scopeId, containerId, jobStatus, failedReason);
        }

        public string CreateJobWithProfileId(JobType jobType, string jobRunBy, int profileId, string userId = null, int subJobCount = 0)
        {
            var id = GenerateJobId(jobType);
            return JMDao.CreateJobWithProfileId(id, jobType, jobRunBy, profileId, userId, subJobCount);
        }
        public string CreateJobWithScopeIdForRecenter(JobType jobType, string jobRunBy, string scopeId, string jobid,int nodeType,string realRunJobUser,string containerId = null)
        {
            return JMDao.CreateJobWithScopeIdForRecenter(jobid, jobType, jobRunBy, scopeId, nodeType, realRunJobUser, containerId);
        }
        public int GetJobProgress(string id)
        {
            return JMDao.GetJobProgress(id);
        }
        public bool UpdateJobProgress(string id, int progress)
        {
            return JMDao.UpdateJob(id, progress);
        }
        public bool UpdateSubJobProgress(string id, int progress)
        {
            return SubJobDao.UpdateJob(id, progress);
        }

        public Task<bool> UpdateJobWithoutProgressChangeAsync(string id)
        {
            return JMDao.UpdateJobWithoutProgressChangeAsync(id);
        }
        public bool UpdateMigrationJobStatus(string id,JobStatus status,string message, ArchiverMigrationJobStatus migrationJobStatus)
        {
            string additionalInformation = Enum.GetName(typeof(ArchiverMigrationJobStatus), migrationJobStatus);
            var res = JMDao.UpdateMigrationJob(id, status, message, additionalInformation);
            if (res && JobServiceUtility.IsFinalState((int)status))
            {
                TelemetryContext.SendToQueue(TelemetryModule.JobMonitor, TelemetryEventType.RunJob, new List<object> { id });
                TelemetryContext.FlushAsync().GetAwaiter().GetResult();
            }
            return res;
        }

        public bool UpdateMigrationJobAdditionalInformation(string id, ArchiverMigrationJobStatus migrationJobStatus)
        {
            string additionalInformation = Enum.GetName(typeof(ArchiverMigrationJobStatus), migrationJobStatus);
            var res = JMDao.UpdateJobAdditionalInformation(id,additionalInformation);
            return res;
        }
        public bool UpdateJobExtension(string id, ArchiveJobMonitorExtension extension)
        {
            var extensionString = SerializerHelper.SerializeByDataContractSerializer(extension);
            return JMDao.UpdateJobExtension(id, extensionString);
        }

        public bool UpdateJobExtensionById(string id, string extension)
        {
            return JMDao.UpdateJobExtensionById(id, extension);
        }
        public bool UpdateJobStatus(string id, JobStatus status, string message)
        {
            var res = JMDao.UpdateJob(id, status, message);
            if (res && JobServiceUtility.IsFinalState((int)status))
            {
                TelemetryContext.SendToQueue(TelemetryModule.JobMonitor, TelemetryEventType.RunJob, new List<object> { id });
                TelemetryContext.FlushAsync().GetAwaiter().GetResult();
            }
            return res;
        }

        public bool AtomicityUpdateJobExtension(string jobId, string oldJobExtension, string newJobExtension)
        {
            try
            {
                return JMDao.AtomicityUpdateJobExtension(jobId, oldJobExtension, newJobExtension);
            }
            catch (Exception ex)
            {
                logger.Error($"fail update job extension,error message:{ex.Message}, error:{ex}");
                throw;
            }
        }

        

        public bool SetSumSCCountOfJobExtension(int sumCount, string jobId)
        {
            try
            {
                RMJobMonitor job = null;
                string newJobExtensionJson = null;
                do
                {
                    job = JMDao.GetJob(jobId);
                    string extensionJson = job.Extension;
                    JobExtension newJobExtension = null;
                    if (string.IsNullOrWhiteSpace(extensionJson))
                    {
                        newJobExtension = new JobExtension() 
                        {
                            soSCProgress = new SOSCProgress(),
                        };
                    }
                    else
                    {
                        newJobExtension = SerializerHelper.DeserializeByJsonConvert<JobExtension>(extensionJson);
                    }

                    if (newJobExtension.SOProgressFileAndSCCount == null)
                    {
                        newJobExtension.SOProgressFileAndSCCount = new SOProgressFileAndSCCount()
                        {
                            AllSCCount = sumCount, 
                            ProgressedFileCountArr = new int[sumCount],
                            ProgressedSCCountArr = new int[sumCount],
                            IsNewJob =  true,
                        };
                    }
                    else
                    {
                        var existing = newJobExtension.SOProgressFileAndSCCount;
                        if (existing.ProgressedFileCountArr == null || existing.ProgressedFileCountArr.Length != sumCount)
                        {
                            var progressedFileCountArr = existing.ProgressedFileCountArr ?? Array.Empty<int>();
                            Array.Resize(ref progressedFileCountArr, sumCount);
                            existing.ProgressedFileCountArr = progressedFileCountArr;
                        }
                        if (existing.ProgressedSCCountArr == null || existing.ProgressedSCCountArr.Length != sumCount)
                        {
                            var progressedScCountArr = existing.ProgressedSCCountArr ?? Array.Empty<int>();
                            Array.Resize(ref progressedScCountArr, sumCount);
                            existing.ProgressedSCCountArr = progressedScCountArr;
                        }
                        existing.AllSCCount = sumCount;
                        existing.IsNewJob = true;
                    }

                    newJobExtensionJson = SerializerHelper.SerializeByJsonConvert(newJobExtension);

                } while (!JMDao.AtomicityUpdateJobExtension(jobId, job.Extension, newJobExtensionJson));
                return true;
            }
            catch (Exception ex)
            {
                logger.Error($"fail set sc count,error message:{ex.Message}, error:{ex}");
                return false;
            }
        }

        public bool UpdateJobStatus(string id, JobStatus status)
        {
            var res = JMDao.UpdateJob(id, status);
            if (res && JobServiceUtility.IsFinalState((int)status))
            {
                TelemetryContext.SendToQueue(TelemetryModule.JobMonitor, TelemetryEventType.RunJob, new List<object> { id });
                TelemetryContext.FlushAsync().GetAwaiter().GetResult();
            }
            return res;
        }

        public string GetJobIdByJobTypeExceptCurrent(JobType jobType, string currentId)
        {
            return GetJobIdByJobTypeExceptCurrent(jobType, currentId, "");
        }

        public string GetJobIdByJobTypeExceptCurrent(JobType jobType, string currentId, string scopeId)
        {
            RMJobMonitor job = null;
            if (string.IsNullOrEmpty(scopeId))
            {
                job = JMDao.Find(c => c.JobType == (int)jobType && (c.Status == (int)JobStatus.Wait || c.Status == (int)JobStatus.InProgress) && c.Id != currentId);
            }
            else
            {
                job = JMDao.Find(c => (c.JobType == (int)jobType && (c.Status == (int)JobStatus.Wait || c.Status == (int)JobStatus.InProgress) && c.Id != currentId) && c.ScopeId == scopeId);
            }
            return job == null ? "" : job.Id;
        }

        public (string, long) GetLastFinishedJob(JobType jobType)
        {
            List<BaseJobDto> jobInfos = new List<BaseJobDto>();
            RMJobMonitor job = JMDao.GetLastFinishedJob(jobType);
            return job == null ? (null, 0) : (job.Id, job.EndTime);
        }

        public List<string> GetRunningJobs(JobType jobType)
        {
            return JMDao.GetRunningJobs(jobType);
        }

        public List<BaseJobDto> GetRunningJobs(List<JobType> jobTypes, string scopeId)
        {
            List<BaseJobDto> jobInfos = new List<BaseJobDto>();
            var jobs = JMDao.GetRunningJobs(jobTypes, scopeId);
            foreach (var job in jobs)
            {
                jobInfos.Add(
                    new BaseJobDto()
                    {
                        Id = job.Id,
                        JobType = job.JobType,
                        Status = job.Status,
                        Progress = job.Progress,
                        ScopeId = job.ScopeId,
                    });
            }
            return jobInfos;
        }

        public List<BaseJobDto> GetRunningJobsBatch(List<JobType> jobTypes, List<string> scopeIds)
        {
            List<BaseJobDto> jobInfos = new List<BaseJobDto>();
            var jobs = JMDao.GetRunningJobsBatch(jobTypes, scopeIds);
            foreach (var job in jobs)
            {
                jobInfos.Add(
                    new BaseJobDto()
                    {
                        Id = job.Id,
                        JobType = job.JobType,
                        Status = job.Status,
                        Progress = job.Progress,
                        ScopeId = job.ScopeId,
                    });
            }
            return jobInfos;
        }

        public List<BaseJobDto> GetRunningJobs(List<JobType> jobTypes)
        {
            List<BaseJobDto> jobInfos = new List<BaseJobDto>();
            var jobs = JMDao.GetRunningJobs(jobTypes);
            foreach (var job in jobs)
            {
                jobInfos.Add(
                    new BaseJobDto()
                    {
                        Id = job.Id,
                        JobType = job.JobType,
                        Status = job.Status,
                        Progress = job.Progress,
                        ScopeId = job.ScopeId,
                    });
            }
            return jobInfos;
        }

        public List<string> GetRunningJobsScopeId(JobType jobType)
        {
            return JMDao.GetRunningJobsScopeId(jobType);
        }

        public int GetRunningJobsCount(JobType jobType)
        {
            var jobs = JMDao.GetRunningJobs(jobType);
            return jobs == null ? 0 : jobs.Count();
        }

        public int GetRunningJobsCount(List<JobType> jobTypes)
        {
            var jobs = JMDao.GetRunningJobs(jobTypes);
            return jobs == null ? 0 : jobs.Count();
        }

        /// <summary>
        /// get profile ids which profile is running job
        /// </summary>
        /// <param name="profileIds">profile ids</param>
        /// <returns>the profile ids of running jobs</returns>
        public List<int?> GetRunningJobsByProfileIds(List<int> profileIds)
        {
            List<int> results = new List<int>();
            var jobs = JMDao.GetRunningJobsByProfileIds(profileIds);
            return jobs.Select(jm => jm.ProfileId).Distinct().ToList<int?>();
        }

        //public List<string> GetRunningJobs(JobType jobType, string scopeId)
        //{
        //    return JMDao.GetRunningJobs(jobType, scopeId);
        //}

        public string GenerateJobId(JobType jobType)
        {
            string jobId = "";
            lock (jobIdLock)
            {
                try
                {
                    DateTime now = DateTime.Now;
                    while ((now - lastGeneratedDate) < TimeSpan.FromSeconds(1))
                    {
                        Thread.Sleep(1000);
                        now = DateTime.Now;
                    }
                    lastGeneratedDate = now;
                    var prefix = string.Empty;
                    switch (jobType)
                    {
                        case JobType.JobMonitorArchive:
                            prefix = "JMA";
                            break;
                        case JobType.TermSynchronization:
                            prefix = "TS";
                            break;
                        case JobType.SPOnPremTermSynchronization:
                        case JobType.SPOnPremTermSynchronizationSchedule:
                            prefix = "SLTS";
                            break;
                        case JobType.ImportTermStructure:
                        case JobType.ImportGoogleTermStructure:
                            prefix = "TI";
                            break;
                        case JobType.ImportSCMapping:
                            prefix = "ISM";
                            break;
                        case JobType.ExportSCMapping:
                            prefix = "ESM";
                            break;
                        case JobType.ImportSCWhitelist:
                            prefix = "ISW";
                            break;
                        case JobType.ExportSCWhitelist:
                            prefix = "ESW";
                            break;
                        case JobType.ImportSCBlacklist:
                            prefix = "ISB";
                            break;
                        case JobType.ExportSCBlacklist:
                            prefix = "ESB";
                            break;
                        case JobType.ExportTermStructure:
                            prefix = "TE";
                            break;
                        case JobType.DiscoveryExportExcludeSCList:
                            prefix = "DEESCL";
                            break;
                        case JobType.DiscoveryImportExcludeSCList:
                            prefix = "DIESCL";
                            break;
                        case JobType.ExportHoldRecords:
                            prefix = "EHR";
                            break;
                        case JobType.ImportHoldRecords:
                            prefix = "IHR";
                            break;
                        case JobType.ImportWorkspaceHold:
                            prefix = "IWH";
                            break;
                        case JobType.ItemsFilesDueDisposal:
                        case JobType.BCSTermUsageReport:
                        case JobType.FSBCSTermUsageReport:
                        case JobType.EXOTermUsageReport:
                        case JobType.PhysicalTermUsageReport:
                        case JobType.CreateAndDestroyedFileReport:
                        case JobType.RestoreReport:
                        case JobType.EXOCreateAndDestroyedFileReport:
                        case JobType.PhysicalCreateAndDestroyedFileReport:
                        case JobType.AvailableSpaceReport:
                        case JobType.EXOItemsFilesDueDisposalReport:
                        case JobType.PhysicalItemsFilesDueDisposalReport:
                        case JobType.FSItemsFilesDueDisposal:
                        case JobType.FSCreateAndDestroyedFileReport:
                        case JobType.OneDriveItemsFilesDueDisposalReport:
                        case JobType.OneDriveTermUsageReport:
                        case JobType.OneDriveCreateAndDestroyedFileReport:
                        case JobType.OneDriverRestoreReport:
                        case JobType.SPOnPremItemsFilesDueDisposal:
                        case JobType.SPOnPremCreateAndDestroyedFileReport:
                        case JobType.SPOnPremBCSTermUsageReport:
                        case JobType.SPOnPremRetiredTermReport:
                        case JobType.SPOnPremOrphanedTermReport:
                        case JobType.OneDriveActionAuditReport:
                        case JobType.SPOActionAuditReport:
                        case JobType.BoxItemsFilesDueDisposalReport:
                        case JobType.BoxBCSTermUsageReport:
                        case JobType.BoxCreateAndDestroyedFileReport:
                        case JobType.GoogleCreateAndDestroyedFileReport:
                        case JobType.GoogleItemsFilesDueDisposalReport:
                        case JobType.GoogleBCSTermUsageReport:
                        case JobType.GoogleRestoreReport:
                        case JobType.TeamsRestoreReport:
                        case JobType.TeamsItemsFilesDueDisposalReport:
                        case JobType.TeamsCreateAndDestroyedFileReport:
                        case JobType.TeamsBCSTermUsageReport:
                        case JobType.TeamsOrphanedTermUsageReport:
                        case JobType.TeamsRetiredTermUsageReport:
                        case JobType.TeamsActionAuditReport:
                            prefix = "RC";
                            break;
                        case JobType.SharePointGlobalSetting:
                        case JobType.SharePointCustomSetting:
                        case JobType.SharePointInheritSetting:
                        case JobType.SharePointScheduleSetting:
                        case JobType.ApplySharePointSettings:
                        case JobType.SPOnPremApplySetting:
                        case JobType.SPOnPremApplySettingSchedule:
                            prefix = "SS";
                            break;                          
                        case JobType.TeamsScheduleSetting:
                        case JobType.ApplyTeamsSettings:
                            prefix = "TAS";
                            break;
                        case JobType.PhysicalFolderSynchronization:
                            prefix = "FS";
                            break;
                        case JobType.PhysicalTermSynchronization:
                            prefix = "PS";
                            break;
                        case JobType.UpdateLocation:
                            prefix = "PU";
                            break;
                        case JobType.ImportPhysicalRecords:
                            prefix = "PI";
                            break;
                        case JobType.TrimRecordsDeletion:
                            prefix = "PID";
                            break;
                        case JobType.ImportRecordsRelated:
                            prefix = "PI";
                            break;
                        case JobType.ManualApproval:
                            prefix = "MA";
                            break;
                        case JobType.ArchiverFullTextIndex:
                            prefix = "AFTI";
                            break;
                        case JobType.DeleteRestoredData:
                            prefix = "DRD";
                            break;
                        case JobType.DiscoveryJobV2:
                        case JobType.DiscoveryJobV3:
                        case JobType.DiscoveryJobV4:
                        case JobType.DiscoveryJobV5:
                            prefix = "DCT";
                            break;
                        case JobType.DiscoveryAOSPJob:
                            prefix = "DACT";
                            break;
                        case JobType.DiscoveryGoogleJobV1:
                            prefix = "DGJ";
                            break;
                        case JobType.SFDiscoveryJob:
                            prefix = "SFD";
                            break;
                        case JobType.DiscoveryProfileJob:
                            prefix = "DCP";
                            break;
                        case JobType.DiscoveryGoogleProfileJob:
                            prefix = "DGP";
                            break;
                        case JobType.DiscoveryExportO365Profile:
                            prefix = "DOPE";
                            break;
                        case JobType.DiscoveryExportRowDataJob:
                            prefix = "DRD";
                            break;
                        case JobType.DiscoveryExportDuplicationReport:
                            prefix = "DDR";
                            break;
                        case JobType.DiscoveryAnalysisFileSystemV1:
                            prefix = "DAFS";
                            break;
                        case JobType.ManualApprovalOrRejectJob:
                            prefix = "BMA";
                            break;
                        case JobType.ManualFolderViewActions:
                            prefix = "MFA";
                            break;
                        case JobType.DisposalActivityManagement:                     
                            prefix = "DA";
                            break;
                        case JobType.UniqueIDSettingFullSchedule:
                        case JobType.TeamsUniqueIDSettingFullSchedule:
                            prefix = "UF";
                            break;
                        case JobType.UniqueIDSettingIncrementalSchedule:
                        case JobType.TeamsUniqueIDSettingIncrementalSchedule:
                            prefix = "UI";
                            break;
                        case JobType.CollectionDataFull:
                            prefix = "CF";
                            break;
                        case JobType.CollectionDataIncremental:
                            prefix = "CI";
                            break;
                        case JobType.ManualApprovalTimer:
                            prefix = "MA";//暂定。带MA删除后，前缀可改成MA
                            break;
                        case JobType.SharePointOnlineDeletionSyncUpgrade:
                            prefix = "DSU";
                            break;
                        case JobType.SendEmailJob:
                            prefix = "SEJ";
                            break;
                        case JobType.ManualFileSystemUpgrade:
                            prefix = "MFS";
                            break;
                        case JobType.DiscoveryJob:
                            prefix = "Discovery";
                            break;
                        case JobType.DiscoveryReCalculate:
                            prefix = "DRC";
                            break;
                        case JobType.DiscoveryOptimizationCalculate:
                            prefix = "DOC";
                            break;
                        case JobType.DiscoveryAOSPOptimizationCalculate:
                            prefix = "DAOC";
                            break;
                        case JobType.CosmosDBDirtyDataDeleteUpgrade:
                            prefix = "DDDElU";
                            break;
                        case JobType.EnforceRetention:
                        case JobType.OldEnforceRetention:
                        case JobType.EXOEnforceRetention:
                        case JobType.OneDriveEnforceRetention:
                        case JobType.TeamsEnforceRetention:
                            prefix = "ER";
                            break;
                        case JobType.DataSynchronisation:
                        case JobType.SPDataSynchronisationSchedule:
                        case JobType.EXODataSynchronisation:
                        case JobType.EXODataSynchronisationSchedule:
                        case JobType.FSDataSynchronization:
                        case JobType.FSDataSynchronizationSchedule:
                        case JobType.SPOnPremDataSync:
                        case JobType.SPOnPremDataSyncSchedule:
                        case JobType.OneDriveDataSynchronisation:
                        case JobType.OneDriveDataSynchronisationSchedule:
                        case JobType.AzureFileShareDataSynchronisation:
                        case JobType.AzureFileShareDataSynchronisationSchedule:
                        case JobType.TeamsDataSynchronisation:
                        case JobType.TeamsDataSynchronisationSchedule:
                            prefix = "DS";
                            break;
                        case JobType.RecordsExplorerMove:
                            prefix = "RM";
                            break;
                        case JobType.EXOApplySetting:
                        case JobType.EXOApplySettingSchedule:
                            prefix = "ES";
                            break;
                        case JobType.PhysicalDisposal:
                        case JobType.PhysicalRecordsDisposal:
                            prefix = "PD";
                            break;
                        case JobType.FSDisposal:
                        case JobType.FSDisposalSchedule:
                            prefix = "FD";
                            break;
                        case JobType.FSDisposalByClassCode:
                            prefix = "FDCC";
                            break;
                        case JobType.ImportFSSetting:
                            prefix = "IFS";
                            break;
                        case JobType.PhysicalExplorerTimer:
                            prefix = "PT";
                            break;
                        case JobType.ConnectorTimer:
                            prefix = "CT";
                            break;
                        case JobType.ImportSPSetting:
                            prefix = "IS";
                            break;
                        case JobType.PhysicalExportBarcode:
                            prefix = "EB";
                            break;
                        case JobType.ActionOnly:
                            prefix = "RD";
                            break;
                        case JobType.ApplyClassCode:
                            prefix = "ACC";
                            break;
                        case JobType.PhysicalSetPermission:
                            prefix = "PSP";
                            break;
                        case JobType.FSDashBoard:
                        case JobType.FSMyHubDashboard:
                            prefix = "FDB";
                            break;
                        case JobType.SPOnPremDashBoard:
                            prefix = "SODB";
                            break;
                        case JobType.Dashboard:
                            prefix = "DB";
                            break;
                        case JobType.TenantUpgrade:
                            prefix = "TU";
                            break;
                        case JobType.ManualApprovalEmailSchedule:
                            prefix = "MAES";
                            break;
                        case JobType.FSFolderChangeTerm:
                            prefix = "FSR";
                            break;
                        case JobType.FSFolderManageHold:
                            prefix = "RH";
                            break;
                        case JobType.SyncSecurityContainer:
                            prefix = "PS";
                            break;
                        case JobType.GlobalSearchAction:
                            prefix = "GSA";
                            break;
                        case JobType.ExplorerOfflineSearch:
                            prefix = "OS";
                            break;
                        case JobType.SyncNodesFromAOS:
                            prefix = "SRN";
                            break;
                        case JobType.SPOnPremScanLocalNodes:
                            prefix = "SLN";
                            break;
                        case JobType.SPOnPremEnforceRuleAction:
                        case JobType.SPOnPremEnforceRuleActionSchedule:
                            prefix = "SLER";
                            break;
                        case JobType.SPOnPremUniqueIDSettingFullSchedule:
                            prefix = "SLUF";
                            break;
                        case JobType.SPOnPremUniqueIDSettingIncrementalSchedule:
                            prefix = "SLUI";
                            break;
                        case JobType.ExportSearchResult:
                            prefix = "ESR";
                            break;
                        case JobType.PhysicalReturnBox:
                            prefix = "RPB";
                            break;
                        case JobType.PhysicalLoanBox:
                            prefix = "LPB";
                            break;
                        case JobType.SwitchSecurityProfile:
                            prefix = "SSP";
                            break;
                        case JobType.EXORecordsDisposal:
                            prefix = "EEA";
                            break;
                        case JobType.TeamsArchiverBackup:
                        case JobType.RMArchiverBackup:
                        case JobType.RMEndUserArchiverBackup:
                        case JobType.SpecifySitesArchiverBackup:
                        case JobType.SpecifyTeamsArchiverBackup:
                            prefix = "SO";
                            break;
                        case JobType.ApprovalProcessArchive:
                            prefix = "APA";
                            break;
                        case JobType.DeleteInvalidRecords:
                            prefix = "DIVR";
                            break;
                        case JobType.RecordsDisposal:
                            prefix = "SEA";
                            break;
                        case JobType.OneDriveRecordsDisposal:
                            prefix = "OEA";
                            break; 
                        case JobType.ManualHistoriesUpgrade:
                            prefix = "MHU";
                            break;
                        case JobType.PhysicalLoanPick:
                            prefix = "PLLR";
                            break;
                        case JobType.ManualExportHistoryDatasJob:
                            prefix = "MHE";
                            break;
                        case JobType.PhysicalDestructionPick:
                            prefix = "PLD";
                            break;
                        case JobType.PhysicalLoanPickExportJob:
                            prefix = "EPLL";
                            break;
                        case JobType.PhysicalDestructionPickExportJob:
                            prefix = "EPLD";
                            break;
                        case JobType.PhysicalMovePickExportJob:
                            prefix = "EPLM";
                            break;
                        case JobType.PhysicalMoveDataJob:
                            prefix = "MPB";
                            break;
                        case JobType.ExportReportDetails:
                            prefix = "RPE";
                            break;
                        case JobType.ExportFSSetting:
                            prefix = "ESFS";
                            break;
                        case JobType.DownloadRCCReport:
                            prefix = "RCCFS";
                            break;
                        case JobType.ExportSPSetting:
                            prefix = "ESSP";
                            break;
                        case JobType.BaseArchiveJobIdMultiRestore:
                            prefix = "MRRS";
                            break;
                        case JobType.ManualExportRecordsForReviewDatasJob:
                            prefix = "MAE";
                            break;
                        case JobType.ManualImportUnderReviewDatasJob:
                            prefix = "MAI";
                            break;
                        case JobType.MachineLearningTraining:
                            prefix = "MALT";
                            break;
                        case JobType.MachineLearningAnalyse:
                            prefix = "MALA";
                            break;
                        case JobType.MachineLearningReviewApprove:
                            prefix = "MAAP";
                            break;
                        case JobType.MachineLearningReviewReclassify:
                            prefix = "MARE";
                            break;
                        case JobType.ArchiverRestore:
                        case JobType.StubOopRestore:
                        case JobType.AOSPRestore:
                            prefix = "RS";
                            break;
                        case JobType.ArchiverToSpoRestore:
                            prefix = "TSRS";
                            break;
                        case JobType.SimulateRestore:
                            prefix = "SRS";
                            break;
                        case JobType.ArchiverOutPlaceRestore:
                            prefix = "ORS";
                            break;
                        case JobType.ArchiverMoveIndex:
                            prefix = "AMI";
                            break;
                        case JobType.ArchiverRetention:
                            prefix = "ARP";
                            break;
                        case JobType.ArchiverFullMoveRetention:
                            prefix = "AFM";
                            break;
                        case JobType.ArchiverRetentionSimulate:
                            prefix = "ARPS";
                            break;
                        case JobType.ArchiverRetentionSimulateMain:
                            prefix = "ARPSM";
                            break;
                        case JobType.VeoMerge:
                            prefix = "VM";
                            break;
                        case JobType.ArchiverExport:
                            prefix = "ASE";
                            break;
                        case JobType.MoveDataTier:
                            prefix = "MDT";
                            break;
                        case JobType.SOPreScan:
                            prefix = "SAN";
                            break;
                        case JobType.DiscoveryPreScan:
                            prefix = "DAN";
                            break;
                        case JobType.CloudArchiverMigration:
                            prefix = "CAM";
                            break;
                        case JobType.DiscoverOptimization:
                            prefix = "DSO";
                            break;
                        case JobType.CleanUpDuplicateDatas:
                            prefix = "CUD";
                            break;
                        case JobType.ArchiverByHSMXml:
                            prefix = "ABH";
                            break;
                        case JobType.DiscoveryAOSPOptimization:
                            prefix = "DASO";
                            break;
                        case JobType.PhysicalBulkInsertExport:
                            prefix = "PBI";
                            break;
                        case JobType.PhysicalBulkEditExport:
                            prefix = "PBE";
                            break;
                        case JobType.ExportToLocation:
                        case JobType.DownloadJobReports:
                            prefix = "DJR";
                            break;
                        case JobType.DownloadJobReportsForCOP:
                            prefix = "DJRC";
                            break;
                        case JobType.RebuildStub:
                            prefix = "ARS";
                            break;
                        case JobType.RebuildIndex:
                            prefix = "ARI";
                            break;
                        case JobType.RebuildEncryptKeyValue:
                            prefix = "AREKV";
                            break;
                        case JobType.RebuildSOJobReport:
                            prefix = "ARSR";
                            break;
                        case JobType.BuildRunningJobReport:
                            prefix = "ABRR";
                            break;
                        case JobType.RebuildDeDupForWPPMigration:
                            prefix = "ARDM";
                            break;
                        case JobType.MachineLearningExportReportJob:
                            prefix = "MLER";
                            break;
                        case JobType.BoxDataSynchronisation:
                        case JobType.BoxDataSynchronisationSchedule:
                            prefix = "BS";
                            break;
                        case JobType.BoxRecordsDisposal:
                            prefix = "BEA";
                            break;
                        case JobType.AdjustStorageSize:
                            prefix = "ADS";
                            break;
                        case JobType.ExportSiteMetrics:
                            prefix = "MR";
                            break;
                        case JobType.ExportIndex:
                            prefix = "EI";
                            break;
                        case JobType.GoogleApplySettings:
                            prefix = "GA";
                            break;
                        case JobType.GoogleRecordsDisposal:
                            prefix = "GEA";
                            break;
                        case JobType.GoogleDataSynchronization:
                            prefix = "GS";
                            break;
                        case JobType.ArchiverDeduplication:
                            prefix = "ADD";
                            break;
                        case JobType.ArchiverDeduplicationReport:
                            prefix = "ADR";
                            break;
                        case JobType.DeleteOrphanDatas:
                            prefix = "DOD";
                            break;                        
                        case JobType.PhysicalTemplateImport:
                            prefix = "PTI";
                            break;
                        case JobType.MigrationArchiverFileLevelRetention:
                            prefix = "FRT";
                            break;
                        case JobType.ExportRestoreCenterSeachResult:
                            prefix = "EASR";
                            break;
                        case JobType.FSArchiverRestore:
                            prefix = "FRS";
                            break;
                        case JobType.GoogleArchiverRestore:
                            prefix = "GRS";
                            break;
                        case JobType.FSRetain:
                            prefix = "FARP";
                            break;
                        case JobType.FSRetainSimulate:
                            prefix = "FARPS";
                            break;
                        //case JobType.MigrationArchiverDeduplication:
                        //    prefix = "DD";
                        //    break;
                        case JobType.ConvertStub:
                            prefix = "CVS";
                            break;
                        case JobType.TeamsRecordsDisposal:
                            prefix = "TEA";
                            break;
                        case JobType.TeamsArchiverRestore:
                        case JobType.MailBoxArchiverRestore:
                            prefix = "TRS";
                            break;
                        case JobType.TeamsOutPlaceRestore:
                            prefix = "OTRS";
                            break;
                        case JobType.ExportTeamsSetting:
                            prefix = "EST";
                            break;
                        case JobType.ImportTeamsSetting:
                            prefix = "IST";
                            break;
                        case JobType.TeamsArchiverRetention:
                            prefix = "TAR";
                            break;
                        case JobType.EXOArchiverRetention:
                            prefix = "EAR";
                            break;
                        case JobType.DiscoveryFileSystemV1:
                            prefix = "DFS";
                            break;
                        case JobType.PhysicalReturnHistoryExport:
                            prefix = "RLHE";
                            break;
                        case JobType.TeamsChannelSettingConflictCheck:
                            prefix = "TSC";
                            break;
                        case JobType.ConflictSettingDetailExport:
                            prefix = "CSDE";
                            break;
                        case JobType.TeamsNodeSettingUpgrade:
                            prefix = "TSU";
                            break;
                        case JobType.TeamsPreScan:
                            prefix = "TAN";
                            break;
                        case JobType.TeamsDataUpgrade:
                            prefix = "TDU";
                            break;
                        case JobType.GoogleArchiverRetention:
                            prefix = "GAR";
                            break;
                        case JobType.ExportDecryptIndexDB:
                            prefix = "EDI";
                            break;
                        case JobType.MultiSiteCollectionRestore:
                            prefix = "SSCR";
                            break;
                        case JobType.ExportTeamsSOSetting:
                            prefix = "ETSOS";
                            break;
                        case JobType.ExportSPSOSetting:
                            prefix = "ESPSOS";
                            break;
                        case JobType.DeclaredRecordsMigration:
                            prefix = "DRM";
                            break;
                        case JobType.StubDisposal:
                            prefix = "SD";
                            break;
                        case JobType.MigrateDataCosmosDbForJPMC:
                            prefix = "MD";
                            break;
                        case JobType.DeleteArchivedSiteCollection:
                            prefix = "DASC";
                            break;
                        case JobType.MultiGeoMainDCSyncCommonData:
                            prefix = "MDCSC";
                            break;
                        case JobType.MultiGeoOtherDCSyncCommonData:
                            prefix = "ODCSC";
                            break;
                        case JobType.SharePointSiteMetricsReport:
                            prefix = "SPSR";
                            break;
                        case JobType.DispatchedJob:
                            prefix = "DSPJ";
                            break;
                        case JobType.APStorageCostEvaluation:
                            prefix = "ASCE";
                            break;
                        case JobType.DiscoveryDalJob:
                            prefix = "IO";
                            break;
                        case JobType.PreviewRestore:
                            prefix = "PRS";
                            break;
                        case JobType.StubArchiverRestore:
                            prefix = "SARS";
                            break;
                        case JobType.M365InPlaceArchiverRestore:
                            prefix = "IPARS";
                            break;
                        default:
                            break;
                    }
                    /* Fortify Issue Type: Insecure Randomness 
                    * Sink Details: this position 
                    * Ignore Reason: random用于生成jobid 不涉及安全问题 
                    */
                    jobId = prefix + DateTime.Now.ToString("yyyyMMddHHmmss") + GenerateRandomNumber(6);
                }
                catch (Exception ex)
                {
                    logger.Warn("Generating job ID failed: " + ex.ToString());
                }
            }
            return jobId;
        }

        private static string GenerateRandomNumber(int count)
        {
            Random ran = new Random((int)DateTime.Now.Ticks);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                sb.Append(ran.Next(0, 9)).ToString();
            }
            return sb.ToString();
        }

        #endregion

        #region Profiles Method
        public async Task<(List<KeyValuePair<string, string>>,bool)> GetJobByProfileIdAsync(int profileId ,bool onlyFinishedJob = false)
        {
            bool hasRanJob = false;
            List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();
            var jobs = new List<RMJobMonitor>();
            var unFilterJobs = JMDao.GetJobsByProfileId(profileId);
            if (onlyFinishedJob)
            {
                foreach(var job in unFilterJobs)
                {
                    if(job.Status == (int)JobStatus.FinishWithException || job.Status == (int)JobStatus.Finished)
                    {
                        jobs.Add(job);
                    }
                    jobs = jobs.OrderByDescending(job => job.StartTime).ToList();
                }
            }
            else
            {
                jobs = unFilterJobs;
            }
            if (jobs.Count > 0)
            {
                hasRanJob = true;
            }
            else
            {
                hasRanJob = false;
            }
            GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
            foreach (var r in jobs)
            {
                //过滤waiting 和 in progress的job
                if (r.Status != (int)JobStatus.InProgress && r.Status != (int)JobStatus.Wait)
                {
                    result.Add(new KeyValuePair<string, string>(GeneralSettingService.ConvertTiksToDateTime(gls, r.StartTime, true).SimplifyFormatTime, r.Id));
                }
            }
            return (result,hasRanJob);
        }
        private List<string> GetJobIdsProfileIds(List<int> profileIds)
        {
            List<string> result = new List<string>();
            var jobs = JMDao.GetJobsByProfileIds(profileIds);
            foreach (var r in jobs)
            {
                result.Add(r.Id);
            }
            return result;
        }

        public string GetProfileNameById(int id)
        {
            return JMDao.GetProfileNameById(id);
        }

        public async Task<int> DeleteJobsByProfileIdsAsync(List<int> proflieIds)
        {
            return await DeleteJobsAsync(GetJobIdsProfileIds(proflieIds));
        }

        public async Task<int> DeleteJobReportsByProfileIdsAsync(List<int> profileIds)
        {
            return await DeleteJobReportsAsync(GetJobIdsProfileIds(profileIds));
        }

        #endregion

        #region Tool Method
        private Tuple<List<string>, Dictionary<string, Dictionary<string, List<string>>>> BuildRunnignInfo(RMJobMonitor job, Dictionary<string, List<string>> info)
        {
            Tuple<List<string>, Dictionary<string, Dictionary<string, List<string>>>> resultTuple = new Tuple<List<string>, Dictionary<string, Dictionary<string, List<string>>>>(new List<string>(), new Dictionary<string, Dictionary<string, List<string>>>());

            if (job.AdditionalInformation == null)
            {
                Guid guid = Guid.NewGuid();
                logger.Info($"Current checked job not set additional information is null, for not exception,will use guid instead for check job conflict,job id:{job.Id}, Guid:{guid}");
                resultTuple.Item2.Add(guid.ToString(), info);
            }
            else
            {
                resultTuple.Item2.Add(job.AdditionalInformation, info);
            }
            resultTuple.Item1.AddRange(info.Keys);
            resultTuple.Item1.AddRange(info.Values.SelectMany(v => v));
            return resultTuple;
        }

        private Tuple<List<string>, Dictionary<string, Dictionary<string, List<string>>>> MergeRunningInfo(params Tuple<List<string>, Dictionary<string, Dictionary<string, List<string>>>>[] sources)
        {
            var item1 = new List<string>();
            var item2 = new Dictionary<string, Dictionary<string, List<string>>>();

            if (sources.IsNotNullOrEmpty())
            {
                foreach (var source in sources)
                {
                    if (source == null) continue;

                    if (source.Item1.IsNotNullOrEmpty())
                    {
                        item1.AddRange(source.Item1);
                    }

                    if (source.Item2.IsNotNullOrEmpty())
                    {
                        foreach (var kvp in source.Item2)
                        {
                            if (!item2.ContainsKey(kvp.Key))
                            {
                                item2[kvp.Key] = new Dictionary<string, List<string>>();
                            }

                            if (kvp.Value.IsNotNullOrEmpty())
                            {
                                foreach (var subKvp in kvp.Value)
                                {
                                    if (!item2[kvp.Key].ContainsKey(subKvp.Key))
                                    {
                                        item2[kvp.Key][subKvp.Key] = new List<string>();
                                    }

                                    if (subKvp.Value.IsNotNullOrEmpty())
                                    {
                                        item2[kvp.Key][subKvp.Key].AddRange(subKvp.Value);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            item1 = item1.Distinct().ToList();
            foreach (var key in item2.Keys)
            {
                foreach (var subKey in item2[key].Keys)
                {
                    item2[key][subKey] = item2[key][subKey].Distinct().ToList();
                }
            }

            return new Tuple<List<string>, Dictionary<string, Dictionary<string, List<string>>>>(item1, item2);
        }

        #endregion

        #region JobReport
        private async Task<int> DeleteJobReportsAsync(List<string> idArray)
        {
            List<BaseJobDto> jobInfos = new List<BaseJobDto>();
            var jobs = await GetJobsAsync(idArray);
            foreach (var job in jobs)
            {
                jobInfos.Add(new BaseJobDto() { Id = job.JobId, JobType = job.JobTypeCode });
            }
            return DeleteJobReportFiles(jobInfos);
        }
        public List<BaseJobDto> GetJobDtoByProfileIds(List<int> ids)
        {
            List<BaseJobDto> jobInfos = new List<BaseJobDto>();
            var jobs = JMDao.GetJobs(GetJobIdsProfileIds(ids));
            foreach (var job in jobs)
            {
                jobInfos.Add(new BaseJobDto() { Id = job.Id, JobType = job.JobType });
            }
            return jobInfos;
        }

        private int DeleteJobReportFiles(List<BaseJobDto> jobInfos)
        {
            string expandedName = ".rpt";
            var successCount = 0;

            foreach (var jobInfo in jobInfos)
            {
                if(jobInfo.JobType == (int)JobType.ExplorerOfflineSearch)
                {
                    if (DeleteSearchResult(jobInfo.Id))
                    {
                        successCount++;
                    }
                    continue;
                }
                string rptPath = JobReportUtility.GetJobReportTempPath(jobInfo, expandedName);
                try
                {
                    File.Delete(rptPath);
                    successCount++;
                }
                catch (Exception e)
                {
                    logger.Warn("delete temp folder file {0} error, message:{1}", rptPath, e.ToString());
                }
                try
                {
                    var uri = JobReportUtility.GetJobReportUri(jobInfo.Id, jobInfo.JobType, ".rpt");
                    logger.Debug("delete file uri is:{0}", uri);
                    RAStorageUtil.DeleteReportBlob(uri);
                }
                catch (Exception e)
                {
                    logger.Warn("delete blob file {0} error, message:{1}", rptPath, e.ToString());
                }
            }
            return successCount;
        }
        private bool DeleteSearchResult(string jobId)
        {
            string expandedName = ".db";
            bool sucess = false;
            string rptPath = JobReportUtility.GetSearchResultFilePath("SearchResult_" + jobId + expandedName);
            try
            {
                File.Delete(rptPath);
                sucess = true;
            }
            catch (Exception e)
            {
                logger.Warn("delete search result file {0} error, message:{1}", rptPath, e.ToString());
            }
            try
            {
                var uri = JobReportUtility.GetSearchResultBlobPath("SearchResult_" + jobId + expandedName + ".db");
                logger.Debug("delete file uri is:{0}", uri);
                RAStorageUtil.DeleteReportBlob(uri);
            }
            catch (Exception e)
            {
                logger.Warn("delete search result {0} error, message:{1}", rptPath, e.ToString());
            }
            return sucess;
        }
        #endregion

        #region Disposal Job
        public async Task<string> GetJobsDataForDisposalAsync(string recoJobId)
        {
            var result = new DisposalJMItemResult();
            result.IsDeleted = false;
            var resultList = new List<DisposalJMItemInfo>();
            result.Items = resultList;
            var archiverJobs = ArhciverJobDao.GetJobByRECOJobID(recoJobId);
            GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
            foreach (var job in archiverJobs)
            {
                resultList.Add(new DisposalJMItemInfo
                {
                    Order = job.Order,
                    JobId = job.Id,
                    JobTypeCode = job.JobType,
                    JobType = GetJobTypeName(job.JobType),
                    Status = ConvertToRAStatus(job.StatusFromDAOL),
                    Progress = (int)job.Progress,
                    StartTime = job.StartTime == 0 ? "" : GeneralSettingService.ConvertTiksToDateTime(gls, job.StartTime, true).SimplifyFormatTime,
                    EndTime = job.EndTime == 0 ? I18NEntity.GetString("RM_JS_JM_EndTimePending") : GeneralSettingService.ConvertTiksToDateTime(gls, job.EndTime, true).SimplifyFormatTime,
                    UserName = job.UserName
                });
            }
            return JsonConvert.SerializeObject(result);
        }

        public string GetJobValidateKey(string id)
        {
            return JMDao.GetJob(id).ScopeId;
        }

        public string GetJobExtension(string id)
        {
            var job = JMDao.GetJob(id);
            return job.Extension;
        }

        public async Task<List<JMItemInfo>> GetEndedJobByScopeIdAsync(string scopeId, int[] status, int[] securityGroupId)
        {
            List<JMItemInfo> result = new List<JMItemInfo>(); 
            List<RMJobMonitor> jms = JMDao.GetPermittedJobByScopeId((int)JobType.ExplorerOfflineSearch, scopeId, securityGroupId, status);
            GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
            foreach (var dbJob in jms)
            {
                var JSJobInfo = new JMItemInfo()
                {
                    JobId = dbJob.Id,
                    JobType = GetJobTypeName(dbJob.JobType),
                    JobTypeCode = dbJob.JobType,
                    ProfileId = dbJob.ProfileId.HasValue ? dbJob.ProfileId.Value : 0,
                    Status = (JobStatus)dbJob.Status,
                    Progress = dbJob.Progress,
                    StartTime = dbJob.StartTime == 0 ? "" : GeneralSettingService.ConvertTiksToDateTime(gls, dbJob.StartTime, true).SimplifyFormatTime,
                    EndTime = dbJob.EndTime == 0 ? I18NEntity.GetString("RM_JS_JM_EndTimePending") : GeneralSettingService.ConvertTiksToDateTime(gls, dbJob.EndTime, true).SimplifyFormatTime,
                };
                result.Add(JSJobInfo);
            }
            return result;
        }

        public async Task<List<JMItemInfo>> GetEndedJobByScopeIdAsync(string scopeId, int[] status, string userId)
        {
            List<JMItemInfo> result = new List<JMItemInfo>();
            //List<RMJobMonitor> jms = JMDao.FindList(a => a.ScopeId == scopeId && (a.Status == (int)JobStatus.Finished || a.Status == (int)JobStatus.FinishWithException)).OrderByDescending(c => c.StartTime).ToList();
            List<RMJobMonitor> jms = JMDao.GetPermittedJobByScopeId((int)JobType.ExplorerOfflineSearch, scopeId, userId, status);
            GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
            foreach (var dbJob in jms)
            {
                var JSJobInfo = new JMItemInfo()
                {
                    JobId = dbJob.Id,
                    JobType = GetJobTypeName(dbJob.JobType),
                    JobTypeCode = dbJob.JobType,
                    ProfileId = dbJob.ProfileId.HasValue ? dbJob.ProfileId.Value : 0,
                    Status = (JobStatus)dbJob.Status,
                    Progress = dbJob.Progress,
                    StartTime = dbJob.StartTime == 0 ? "" : GeneralSettingService.ConvertTiksToDateTime(gls, dbJob.StartTime, true).SimplifyFormatTime,
                    EndTime = dbJob.EndTime == 0 ? I18NEntity.GetString("RM_JS_JM_EndTimePending") : GeneralSettingService.ConvertTiksToDateTime(gls, dbJob.EndTime, true).SimplifyFormatTime,
                };
                result.Add(JSJobInfo);
            }
            return result;
        }

        public void UpdateJob(string id, int progress, int status, long endTime, string comment = null)
        {
            JMDao.UpdateJob(id, progress, status, endTime, comment);
        }
        public bool UpdateSubJobStatus(string id, JobStatus status, string message)
        {
            return SubJobDao.UpdateJob(id, status, message);
            //throw new NotImplementedException();
        }

        public string GetJobFakeidByKey(string key)
        {
            return JMDao.GetJobFakeidByKey(key);
        }

        public List<SOJob> ValidateJobs(List<string> keys)
        {

            return Client.GetJobByRevIMKey(keys);
        }

        public List<SOJob> GetJobByRECOID(string recoJobId)
        {
            var rstJobs = new List<SOJob>();
            var jobs = ArhciverJobDao.GetJobByRECOJobID(recoJobId);
            foreach (var job in jobs)
            {
                rstJobs.Add(new SOJob()
                {
                    Id = job.Id,
                    Type = job.JobType,
                    PlanId = job.PlanId,
                    Progress = job.Progress,
                    Scope = job.Scope,
                    StartTime = job.StartTime,
                    FinishTime = job.EndTime,
                    State = job.StatusFromDAOL,
                    Category = job.JobCategory,
                    UserName = job.UserName,
                    Tags = (long)GCommon.Contract.Server.ControlPanel.Object.JobTags.RemoteFarm,
                });
            }
            return rstJobs;
        }

        public List<SOJob> GetSOJobsByIds(List<string> jobIds)
        {
            return Client.GetSOJobsByIds(jobIds);
        }

        public void UpdateArchiverJob(SOJob soJob, string recoJobId)
        {
            var order = 0;
            switch ((JobTypes)soJob.Type)
            {
                case JobTypes.ArchiverScan:
                case JobTypes.ExchangeArchiverScan:
                case JobTypes.PhysicalRecords:
                    order = 1;
                    break;
                case JobTypes.ArchiverBackup:
                case JobTypes.ExchangeArchiverBackup:
                    order = 2;
                    break;
            }
            ArhciverJobDao.UpdateJob(new RMArchiverJob()
            {
                Order = order,
                Id = soJob.Id,
                JobType = soJob.Type,
                PlanId = soJob.PlanId,
                Progress = (int)soJob.Progress,
                Scope = soJob.Scope,
                StartTime = soJob.StartTime,
                EndTime = soJob.FinishTime,
                StatusFromDAOL = soJob.State,
                JobCategory = soJob.Category,
                UserName = soJob.UserName,
                RECOJobId = recoJobId,
            });
        }

        public List<string> GetRunningUniqueIDSettingJob()
        {
            return JMDao.GetUniqueIDSettingJobs();
        }

        public List<string> GetRunningSPOnPremUniqueIDSettingJob()
        {
            return JMDao.GetSPOnPremUniqueIDSettingJobs();
        }

        public List<string> GetTeamsRunningUniqueIDSettingJob()
        {
            return JMDao.GetTeamsUniqueIDSettingJobs();
        }

        public List<string> GetRunningSyncSecurityContainerJob()
        {
            return JMDao.GetRunningSyncSecurityContainerJob();
        }

        public List<string> GetCollectionDataSettingJobs()
        {
            return JMDao.GetCollectionDataSettingJobs();
        }


        [Audit(Module = AuditModule.JobMonitor, Category = AuditCategory.JobMonitor, Action = AuditAction.RunDownloadJobDetailsJob, AfterHandler = typeof(JobMonitorServiceAuditHandler))]
        public async Task<string> RunExportDisposalJobAsync(string exportJobId, string jobRunByUser)
        {
            string jobId = "-1";
            try
            {
                var account = await AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail);
                jobId = CreateJob(JobType.ExportToLocation, jobRunByUser, account.UserId);
                mJobQueueService.HandleMessage(new JobQueueMessage()
                {
                    JobId = jobId,
                    JobType = JobType.ExportToLocation,
                    CommandLine = string.Format("{0} {1} {2}", JobType.ExportToLocation, jobId, exportJobId),
                });
            }
            catch (Exception ex)
            {
                logger.Error("run download report job error:{0}", ex.ToString());
            }
            return jobId;
        }
        #endregion

        public async Task<JMJobSummary> GetJobSummaryAsync(string id)
        {
            var job = JMDao.GetJob(id);
            if (job != null)
            {
                var comment = string.Empty;
                try
                {
                    comment = I18NEntity.GetStringWithSeparator(job.Comment);
                }
                catch (Exception)
                {
                    comment = I18NEntity.GetString(job.Comment);
                }
                if (((JobStatus)job.Status == JobStatus.Failed || (JobStatus)job.Status == JobStatus.FinishWithException) && string.IsNullOrEmpty(comment))
                {
                    var failedSubJob = SubJobDao.Find(s => s.ParentId == id && s.Status == (int)JobStatus.Failed);
                    if (failedSubJob != null)
                    {
                        comment = I18NEntity.GetString(failedSubJob.Comment);
                    }
                }
                GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();

                var summary = new JMJobSummary()
                {
                    JobType = (JobType)job.JobType,
                    JobId = job.Id,
                    ProfileName = job.ProfileId.HasValue ? JMDao.GetProfileNameById(job.ProfileId.Value) : "",
                    StartTime = job.StartTime == 0 ? "" : GeneralSettingService.ConvertTiksToDateTime(gls, job.StartTime, true).SimplifyFormatTime,
                    EndTime = job.EndTime == 0 ? I18NEntity.GetString("RM_JS_JM_EndTimePending") : GeneralSettingService.ConvertTiksToDateTime(gls, job.EndTime, true).SimplifyFormatTime,
                    JobRunBy = job.UserName,
                    Status = (JobStatus)job.Status,
                    Comment = comment
                };

                FillProgressFileAndSCCountInfo(job, summary);

                if (summary.JobType == JobType.EXORecordsDisposal || summary.JobType == JobType.PhysicalRecordsDisposal ||  
                    summary.JobType == JobType.FSDisposal || job.JobType == (int)JobType.FSDisposalSchedule || job.JobType == (int)JobType.FSDisposalByClassCode ||
                    summary.JobType == JobType.SPOnPremEnforceRuleAction || summary.JobType == JobType.SPOnPremEnforceRuleActionSchedule ||
                    summary.JobType == JobType.BoxRecordsDisposal ||
                    summary.JobType == JobType.ApplySharePointSettings || summary.JobType == JobType.EXOApplySetting ||
                    summary.JobType == JobType.EXOApplySettingSchedule || summary.JobType == JobType.SPOnPremApplySetting ||
                    summary.JobType == JobType.SPOnPremApplySettingSchedule || summary.JobType == JobType.SOPreScan || 
                    summary.JobType == JobType.ApplyTeamsSettings || summary.JobType == JobType.TeamsScheduleSetting ||
                    summary.JobType == JobType.TeamsPreScan
                    )
                {
                    summary.Scope = GetJobLocationUrl(job).GetAwaiter().GetResult();
                }
                else if(summary.JobType == JobType.FSArchiverRestore)
                {
                    if (Guid.TryParse(job.ScopeId, out Guid connectionId))
                    {
                        summary.Scope = FSConnectionDao.GetConnectionById(connectionId)?.Name;
                    }
                }
                if (I18NEntity.HasKey(summary.Scope))
                {
                    summary.Scope = I18NEntity.GetString(summary.Scope);
                }
                try
                {
                    if (job != null && (job.JobType == (int)JobType.RMArchiverBackup ||
                                job.JobType == (int)JobType.TeamsArchiverBackup ||
                                job.JobType == (int)JobType.TeamsRecordsDisposal ||
                                job.JobType == (int)JobType.RecordsDisposal ||
                                job.JobType == (int)JobType.OneDriveRecordsDisposal))
                    {
                        if (!string.IsNullOrWhiteSpace(job.Extension))
                        {
                            var jobExtension = SerializerHelper.DeserializeByJsonConvert<JobExtension>(job.Extension);
                            summary.IsNewJob = jobExtension?.SOProgressFileAndSCCount.IsNewJob ?? false;
                            logger.Info($"GetJobSummaryAsync: JobExtension.IsNewJob: {jobExtension?.SOProgressFileAndSCCount.IsNewJob}");
                        }
                    }
                       
                }
                catch (Exception ex)
                {
                    logger.Warn($"GetJobSummaryAsync: Failed to read TotalArchivedSize from extension. {ex.Message}");
                }
                try
                {
                    if (summary.JobType == JobType.TeamsPreScan ||
                        summary.JobType == JobType.DiscoveryPreScan ||
                        summary.JobType == JobType.SOPreScan) 
                    {
                        var jobDto = new BaseJobDto()
                        {
                            Id = job.Id,
                            JobType = job.JobType
                        };
                        var details = JDService.GetDataForSOSummaryDetails("", jobDto);
                        if (details is JMSOSummaryDetails valueDetailSummary)
                        {
                            var scanDetail = valueDetailSummary.ActionStatistics.Where(x => x.ActionTab == (int)ActionTab.Scan).FirstOrDefault();
                            summary.EstimatedOptimizeDataSize = scanDetail.SizeStr;
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Warn($"Cannot get Scan summary");
                }
                return summary;
            }
            else
            {
                return null;
            }
        }

        private void FillProgressFileAndSCCountInfo(RMJobMonitor job, JMJobSummary summary)
        {
            JobExtension jobExtension = null;
            if (!string.IsNullOrWhiteSpace(job.Extension) &&
                !(job.JobType == (int)JobType.FSDisposal
                || job.JobType == (int)JobType.FSDisposalByClassCode
                || job.JobType == (int)JobType.FSDisposalSchedule
                || job.JobType == (int)JobType.SPOnPremEnforceRuleAction
                || job.JobType == (int)JobType.SPOnPremEnforceRuleActionSchedule
                || job.JobType == (int)JobType.BoxRecordsDisposal)
                )
            {
                try
                {
                    jobExtension = SerializerHelper.DeserializeByJsonConvert<JobExtension>(job.Extension);
                }
                catch (Exception e)
                {
                    logger.Info($"Current job cant DeserializeByJsonConvert");
                }
            }
            if (summary.JobType == JobType.SOPreScan || summary.JobType == JobType.DiscoveryPreScan || summary.JobType == JobType.RMArchiverBackup || summary.JobType == JobType.RMEndUserArchiverBackup || summary.JobType == JobType.SpecifySitesArchiverBackup || summary.JobType == JobType.ArchiverRestore || summary.JobType == JobType.RecordsDisposal || summary.JobType == JobType.OneDriveRecordsDisposal || summary.JobType == JobType.ArchiverOutPlaceRestore || summary.JobType == JobType.DiscoverOptimization || summary.JobType == JobType.DiscoveryAOSPOptimization || summary.JobType == JobType.StubOopRestore || summary.JobType == JobType.AOSPRestore || summary.JobType == JobType.BoxRecordsDisposal || summary.JobType == JobType.ApprovalProcessArchive
                || summary.JobType == JobType.TeamsArchiverBackup || summary.JobType == JobType.SpecifyTeamsArchiverBackup ||summary.JobType == JobType.TeamsRecordsDisposal || summary.JobType == JobType.TeamsArchiverRestore || summary.JobType == JobType.TeamsOutPlaceRestore || summary.JobType == JobType.MailBoxArchiverRestore || summary.JobType == JobType.TeamsPreScan || summary.JobType == JobType.GoogleArchiverRestore
                || summary.JobType == JobType.EXORecordsDisposal || summary.JobType == JobType.ArchiverByHSMXml || summary.JobType == JobType.ArchiverToSpoRestore
                || summary.JobType == JobType.StubArchiverRestore || summary.JobType == JobType.M365InPlaceArchiverRestore)
            {
                if (jobExtension?.SOProgressFileAndSCCount == null)
                {
                    summary.ProgressFileCountStr =  null;
                    summary.ProgressSCStr = string.Empty;
                }
                else if (jobExtension.SOProgressFileAndSCCount.ProgressedFileCountArr != null && jobExtension.SOProgressFileAndSCCount.ProgressedSCCountArr != null)
                {
                    summary.ProgressFileCountStr = jobExtension.SOProgressFileAndSCCount.ProgressedFileCountArr.Sum().ToString();
                    summary.ProgressSCStr = $"{jobExtension.SOProgressFileAndSCCount.ProgressedSCCountArr.Sum().ToString()}/{jobExtension.SOProgressFileAndSCCount.AllSCCount}";
                }
                else
                {// old data
                    summary.ProgressFileCountStr = jobExtension.SOProgressFileAndSCCount.ProgressedFileCount.ToString();
                    summary.ProgressSCStr = $"{jobExtension.SOProgressFileAndSCCount.ProgressedSCCount}/{jobExtension.SOProgressFileAndSCCount.AllSCCount}";
                }
                summary.Scope = DefaultSecurityContainerNameHelper.GetI18NName(job.ScopeId);
            }
        }

        public async Task<JMJobDetails> GetSOJobSummaryDetailsAsync(string jobId)
        {
            var job = JMDao.GetJob(JobServiceUtility.IsSubJob(jobId) ? jobId.Split('_').First() : jobId);
            if (job != null)
            {
                int totalCount = 0;
                var jobDto = new BaseJobDto()
                {
                    Id = jobId,
                    JobType = job.JobType
                };
                try
                {
                    if(!string.IsNullOrEmpty(job.AdditionalInformation) && job.JobType == (int)JobType.ArchiverDeduplication && job.Id.StartsWith("DD"))
                    {
                        var jobExtension = SerializerHelper.DeserializeByJsonConvert<ArchiverMigratedJobExtension>(job.AdditionalInformation);
                        jobDto.PlanId = jobExtension.PlanId;
                        jobDto.Category = jobExtension.JobCategory;
                    }
                }
                catch (Exception e)
                {
                    logger.Warn($"Deserialize ArchiverMigratedJobExtension Error {e}");
                }

                jobDto.TenantGroupEmail = TenantInfoDao.GetTenantInfo(TenantLocalValue.LogonGroupId)?.RegisterEmail;
                var details = JDService.GetDataForSOSummaryDetails("", jobDto);
                return details;
            }
            else
            {
                return null;
            }
        }

        public async Task<JMJobDetails> GetRestoreJobSummaryDetailsAsync(string jobId)
        {
            var job = JMDao.GetJob(jobId);
            if (job != null)
            {
                int totalCount = 0;
                var jobDto = new BaseJobDto()
                {
                    Id = jobId,
                    JobType = job.JobType
                };
                try
                {
                    if (!string.IsNullOrEmpty(job.AdditionalInformation) && job.JobType == (int)JobType.ArchiverDeduplication && job.Id.StartsWith("DD"))
                    {
                        var jobExtension = SerializerHelper.DeserializeByJsonConvert<ArchiverMigratedJobExtension>(job.AdditionalInformation);
                        jobDto.PlanId = jobExtension.PlanId;
                        jobDto.Category = jobExtension.JobCategory;
                    }
                }
                catch (Exception e)
                {
                    logger.Warn($"Deserialize ArchiverMigratedJobExtension Error {e}");
                }

                jobDto.TenantGroupEmail = TenantInfoDao.GetTenantInfo(TenantLocalValue.LogonGroupId)?.RegisterEmail;
                var details = JDService.GetDataForRestoreSummaryDetails("", jobDto);
                return details;
            }
            else
            {
                return null;
            }
        }


        public async Task<JMJobSetting> GetJobSettingAsync(string jobId, int jobType)
        {
            if (string.IsNullOrEmpty(jobId))
            {
                logger.Warn("Cannot get job setting infoes, JobId is null or empty.");
                return null;
            }

            switch (jobType)
            {
                case (int)JobType.ApplySharePointSettings:              
                    var jobSetting = mSettingJobDao.GetRMSettingJob(i => i.Id == jobId && i.JobType == jobType);
                    if (jobSetting != null && jobSetting.JobInfos.Any())
                    {
                        string GetYesNoI18NEntity(bool value) {
                            return value == true ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No");
                        }

                        void ModifyValueByKey(List<KeyValuePair<string, object>> keyValuePairs, string keyToModify, object valueToModify)
                        {
                            int index = keyValuePairs.FindIndex(pair => pair.Key == keyToModify);
                            if (index != -1)
                            {
                                keyValuePairs[index] = new KeyValuePair<string, object>(keyToModify, valueToModify);
                            }
                        }

                        void RemoveByKeys(List<KeyValuePair<string, object>> keyValuePairs, params string[] keys)
                        {
                            keyValuePairs.RemoveAll(kp => keys.Contains(kp.Key));
                        }

                        void ChangeKeyPosition(List<KeyValuePair<string, object>> keyValuePairs, string keyNeedToChange, string targetKey, bool isAfter = false)
                        {
                            var pairNeedToChange = keyValuePairs.FirstOrDefault(p => p.Key == keyNeedToChange);
                            if(!string.IsNullOrEmpty(pairNeedToChange.Key))
                            {
                                keyValuePairs.Remove(pairNeedToChange);
                                var targetIndex = keyValuePairs.FindIndex(item => item.Key == targetKey);
                                if (isAfter)
                                {
                                    targetIndex += 1;
                                }
                                keyValuePairs.Insert(targetIndex, pairNeedToChange);
                            }
                        }

                        var sortedSettings = new List<RMSharePointSetting>();
                        
                        var jobInfos = (SerializerHelper.DeserializeByDataContractSerializer<List<RMSharePointSetting>>(jobSetting.JobInfos))
                                       .OrderBy(i => SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(i.NodeInfo).Level)
                                       .ToList();

                        if (jobInfos.Any(i => i.ScopeId == i.SiteGroupId)) //check if the jobinfos include SiteContainer or not.
                        {
                            var containerSetting = jobInfos.FirstOrDefault(i => i.ScopeId == i.SiteGroupId);
                            sortedSettings = jobInfos.Skip(1).OrderBy(i => i.FullPath).ToList();
                            sortedSettings.Insert(0,containerSetting);
                        }
                        else
                        {
                            sortedSettings = jobInfos.OrderBy(i => i.FullPath).ToList();
                        }

                        var settings = sortedSettings.Select(info =>
                        {
                            RMSPTreeNode node = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(info.NodeInfo);

                            var listKeyPair = new Dictionary<string, object>()
                                {
                                    { "RM_DAM_Scope", node.Level == (int)NodeLevel.WebApplication ? node.Name : !string.IsNullOrEmpty(node.Title) ? $"({node.Title}){node.FullPath}" : node.FullPath },
                                    { "RM_JS_SPS_EnableRecordsManagement", GetYesNoI18NEntity(info.EnableRecordManagement == 1) },
                                    { "RM_JS_SPS_EnableDataSync",GetYesNoI18NEntity(info.IsSyncData) },
                                    { "RM_JS_SPS_SupportLockedSite",GetYesNoI18NEntity(node.SupportLockedSite) },
                                    { "RM_JS_SPS_EnableLifecycleManagementForSharePointLists", GetYesNoI18NEntity(node.EnableLifecycleManagementForSharePointLists ?? true) },
                                    { "RM_JS_SPS_EditKey_ClassificationColumnName", info.ColumnName },
                                    { "RM_JS_SPS_EditKey_ColumnNameDescription", info.Description },
                                    { "RM_JS_SPS_HiddenColumn", GetYesNoI18NEntity(info.ColumnHidden ?? false) },
                                    { "RM_JS_SPS_DisplayColumnRequired", GetYesNoI18NEntity(info.ColumnRequired ?? false) },
                                    { "RM_JS_SPS_EditKey_ShowUniqueID", GetYesNoI18NEntity(info.IsShowUniqueId ?? false) },
                                    { "RM_JS_SPS_EditKey_KeepSPDefaultValue", GetYesNoI18NEntity(false) },
                                    { "RM_SP_SettingRelatedRecords", GetYesNoI18NEntity(info.EnableRelatedRecords)} ,
                                    { "RM_JS_SPS_EditKey_TermScope", node.TermScopeFullPath },
                                    { "RM_JS_SPS_EditKey_TermDisplayForm", info.IsDisplyaTermPath ? I18NEntity.GetString("RM_SPS_Auditor_DisplayTerm_EntirePath") : I18NEntity.GetString("RM_SPS_Auditor_DisplayTerm_TermLabel") },
                                    { "RM_SPS_AutoClassification_DeployTermMethod", "" },
                                    { "RM_SPS_AutoClassification_DefaultConditionTitle", "" },
                                    { "RM_MachineLearning_IntelligenceMA", GetYesNoI18NEntity(node.AIReviewers.Any())},
                                    { "RM_MachineLearning_IntelligenceReviewers", node.AIReviewers.Select(rw => rw.DisplayName)},
                                    { "RM_MA_Setting_Email_Notification", GetYesNoI18NEntity(info.AISendEMail) },
                                    { "RM_BCM_Audit_NameConflictOption", "" },
                                    { "RM_JS_SPS_EditKey_DefaultValue", !string.IsNullOrEmpty(node.DefaultTermFullPath) ? node.DefaultTermFullPath : node.DefaultTermName },
                                    { "RM_SPS_Auto_RunFullJob", GetYesNoI18NEntity(info.RunAutoFullJob) },
                                    { "RM_JS_SPS_EditKey_Action", GetYesNoI18NEntity(false) },
                                    { "RM_JS_SPS_IncludeDSetAndFolder", GetYesNoI18NEntity(false) },
                                    { "RM_JS_SPS_IncludeDeclaredRecords", GetYesNoI18NEntity(info.IncludeDeclaredRecords) },
                                    { "RM_JS_SPS_EditTitle_ContainerLevelTermSetting", !string.IsNullOrEmpty(node.ContainerTermFullPath) ? node.ContainerTermFullPath : node.TermNameOfContainer },
                                    { "RM_JS_SPS_EditKey_DescriptionOfContainer", info.DescriptionOfContainer },
                                    { "RM_JS_SPS_EditKey_EnableInheritParentTerm", GetYesNoI18NEntity(info.IsInheritParentTerm) },                                                                             
                            }.ToList();

                            if (info.EnableRecordManagement != 1)
                            {
                                return new Dictionary<string, object>
                                {
                                    { "RM_DAM_Scope", node.Level == (int)NodeLevel.WebApplication ? node.Name : !string.IsNullOrEmpty(node.Title) ? $"({node.Title}){node.FullPath}" : node.FullPath },
                                    { "RM_JS_SPS_EnableRecordsManagement", GetYesNoI18NEntity(false) },
                                };
                            }

                            if (node.Level != (int)NodeLevel.WebApplication && node.Level != (int)NodeLevel.SiteCollection)
                            {
                                if (node.Level >= (int)NodeLevel.List)
                                {
                                    if(node.Level >= (int)NodeLevel.Folder)
                                    {
                                        RemoveByKeys(listKeyPair,
                                        "RM_JS_SPS_EditKey_TermDisplayForm",
                                        "RM_JS_SPS_EditTitle_ContainerLevelTermSetting",
                                        "RM_JS_SPS_EditKey_DescriptionOfContainer",
                                        "RM_JS_SPS_EditKey_EnableInheritParentTerm");
                                    }

                                    RemoveByKeys(listKeyPair, "RM_SP_SettingRelatedRecords");
                                }

                                RemoveByKeys(listKeyPair, "RM_JS_SPS_EnableDataSync", "RM_JS_SPS_EnableLifecycleManagementForSharePointLists");
                                RemoveByKeys(listKeyPair, "RM_JS_SPS_SupportLockedSite");
                            }
                            
                            if(node.Level == (int)NodeLevel.SiteCollection || node.Level == (int)NodeLevel.Site)
                            {
                                ChangeKeyPosition(listKeyPair, "RM_SP_SettingRelatedRecords", "RM_JS_SPS_EditTitle_ContainerLevelTermSetting");
                            }

                            if ((info.IsKeepSharePointDefaultValue && info.SetTermForEmptyDefaultValue) || (node.IsKeepSharePointDefaultValue && node.SetTermForEmptyDefaultValue))
                            {
                                ModifyValueByKey(listKeyPair, "RM_JS_SPS_EditKey_KeepSPDefaultValue", $"{GetYesNoI18NEntity(true)}; {I18NEntity.GetString("RM_SPS_NoSetTermForEmptyDefaultValue_Title")}");
                            }
                            else if (info.IsKeepSharePointDefaultValue || node.IsKeepSharePointDefaultValue)
                            {
                                ModifyValueByKey(listKeyPair, "RM_JS_SPS_EditKey_KeepSPDefaultValue", GetYesNoI18NEntity(true));
                            }

                            switch (info.DeployTermMethod)
                            {
                                case (int)DeployTermMethod.UseDefaultTerm:
                                    ModifyValueByKey(listKeyPair, "RM_SPS_AutoClassification_DeployTermMethod", I18NEntity.GetString("RM_JS_SPS_AutoClassification_UseDefault"));
                                    RemoveByKeys(listKeyPair, "RM_SPS_Auto_RunFullJob");
                                    if (info.NeedCheckDefaultValue)
                                    {
                                        if (info.ApplyExistType == (int)ApplyExistingTermType.OverWrite)
                                        {
                                            ModifyValueByKey(listKeyPair, "RM_JS_SPS_EditKey_Action", $"{GetYesNoI18NEntity(true)}; {I18NEntity.GetString("RM_JS_SPS_AutoClassification_ApplyOverwirteTerm")}");
                                        }
                                        else if (info.ApplyExistType == (int)ApplyExistingTermType.SkipAndKeep)
                                        {
                                            ModifyValueByKey(listKeyPair, "RM_JS_SPS_EditKey_Action", $"{GetYesNoI18NEntity(true)}; {I18NEntity.GetString("RM_JS_SPS_AutoClassification_ApplySkipTerm")}");
                                        }
                                    }

                                    if (!info.NeedCheckDefaultValue || !info.IncludeDeclaredRecords)
                                    {
                                        RemoveByKeys(listKeyPair, "RM_JS_SPS_IncludeDeclaredRecords");
                                    }
                                    else
                                    {
                                        var targetIndex = listKeyPair.FindIndex(p => p.Key == "RM_JS_SPS_EditKey_Action");
                                        ChangeKeyPosition(listKeyPair, "RM_JS_SPS_IncludeDeclaredRecords", "RM_JS_SPS_EditKey_Action", true);
                                    }

                                    break;

                                case (int)DeployTermMethod.NoDefaultTerm:
                                    ModifyValueByKey(listKeyPair, "RM_SPS_AutoClassification_DeployTermMethod", I18NEntity.GetString("RM_JS_SPS_AutoClassification_NoDefaultValue"));
                                    RemoveByKeys(listKeyPair,
                                    "RM_SPS_Auto_RunFullJob",
                                    "RM_JS_SPS_EditKey_DefaultValue",
                                    "RM_JS_SPS_EditKey_Action",
                                    "RM_JS_SPS_IncludeDSetAndFolder",     
                                    "RM_JS_SPS_IncludeDeclaredRecords");
                                    break;

                                case (int)DeployTermMethod.UseAutoClassification:
                                    RemoveByKeys(listKeyPair,
                                    "RM_SPS_AutoClassification_DeployTermMethod",
                                    "RM_JS_SPS_EditKey_DefaultValue",
                                    "RM_JS_SPS_EditKey_Action");

                                    var autoDefaultCondition = "";
                                    var autoClassification = SerializerHelper.DeserializeByDataContractSerializer<List<ClassificationRule>>(info.AutoClassificationRules).FirstOrDefault();
                                    ArgumentCheck.CheckNotNull(autoClassification);
                                    if (info.AITermUseType == ArtificialIntelligenceTermUseType.AutoDefault)
                                    {
                                        autoDefaultCondition = I18NEntity.GetString("RM_MachineLearning_DeployTermMethodIntelligence");
                                    }
                                    else if (info.AITermUseType == ArtificialIntelligenceTermUseType.None)
                                    {
                                        autoDefaultCondition = (bool)autoClassification?.NoDefaultTerm ? I18NEntity.GetString("RM_JS_SPS_AutoClassification_NoDefaultValue") : I18NEntity.GetString("RM_JS_SPS_AutoClassification_UseDefault");

                                        if (!autoClassification.NoDefaultTerm)
                                        {
                                            var _targetIndex = listKeyPair.FindIndex(item => item.Key == "RM_SPS_AutoClassification_DefaultConditionTitle");
                                            listKeyPair.Insert(_targetIndex + 1, new KeyValuePair<string, object>("RM_JS_SPS_EditKey_DefaultValue",autoClassification.TermName));
                                        }

                                        RemoveByKeys(listKeyPair,
                                        "RM_MachineLearning_IntelligenceMA",
                                        "RM_MachineLearning_IntelligenceReviewers",
                                        "RM_MA_Setting_Email_Notification");
                                    }

                                    ModifyValueByKey(listKeyPair, "RM_SPS_AutoClassification_DefaultConditionTitle", autoDefaultCondition);

                                    break;

                                case (int)DeployTermMethod.UseIntelligenceClassification:
                                    ModifyValueByKey(listKeyPair, "RM_SPS_AutoClassification_DeployTermMethod", I18NEntity.GetString("RM_MachineLearning_DeployTermMethodIntelligence"));
                                    RemoveByKeys(listKeyPair,
                                    "RM_JS_SPS_EditKey_DefaultValue",
                                    "RM_JS_SPS_EditKey_Action",
                                    "RM_JS_SPS_IncludeDSetAndFolder",
                                    "RM_SPS_AutoClassification_DefaultConditionTitle");

                                    if (!node.AIReviewers.Any())
                                    {
                                        RemoveByKeys(listKeyPair, "RM_MA_Setting_Email_Notification", "RM_MachineLearning_IntelligenceReviewers");
                                    }

                                    break;

                                default: break;
                            }

                            if (info.DeployTermMethod != (int)DeployTermMethod.UseIntelligenceClassification
                            && (info.DeployTermMethod != (int)DeployTermMethod.UseAutoClassification && info.AITermUseType != ArtificialIntelligenceTermUseType.AutoDefault))
                            {
                                RemoveByKeys(listKeyPair,
                                "RM_MachineLearning_IntelligenceMA",
                                "RM_MachineLearning_IntelligenceReviewers",
                                "RM_MA_Setting_Email_Notification",
                                "RM_BCM_Audit_NameConflictOption",
                                "RM_SPS_AutoClassification_DefaultConditionTitle");
                            }
                            else
                            {
                                if (info.AutoJobOption == (int)AutoJobOption.SkipAndKeep)
                                {
                                    ModifyValueByKey(listKeyPair, "RM_BCM_Audit_NameConflictOption", I18NEntity.GetString("RM_JS_SPS_AutoClassification_SkipOverrideOption_Skip"));
                                }
                                else if (info.AutoJobOption == (int)AutoJobOption.Override)
                                {
                                    ModifyValueByKey(listKeyPair, "RM_BCM_Audit_NameConflictOption", I18NEntity.GetString("RM_JS_SPS_AutoClassification_SkipOverrideOption_Override"));
                                }

                                if (!node.AIReviewers.Any())
                                {
                                    RemoveByKeys(listKeyPair, "RM_MA_Setting_Email_Notification", "RM_MachineLearning_IntelligenceReviewers");
                                }
                            }

                            if (info.IsUsingExistColumnName)
                            {
                                string applyTermSetting = info.SetDocLevelTermForExistColumn ? "RM_JS_SPS_UseTermSettingsDefinedInRecords" : "RM_JS_SPS_UseTermSettingsDefinedInSP";
                                ModifyValueByKey(listKeyPair, "RM_JS_SPS_EditKey_ClassificationColumnName", $"{node.ExistColumnName}{string.Format(I18NEntity.GetString("RM_JS_SPS_ExistingColumn"), I18NEntity.GetString(applyTermSetting))}");

                                if (!info.SetDocLevelTermForExistColumn)
                                {
                                    RemoveByKeys(listKeyPair,
                                      "RM_JS_SPS_EditKey_ColumnNameDescription",
                                      "RM_JS_SPS_HiddenColumn",
                                      "RM_JS_SPS_DisplayColumnRequired",
                                      "RM_JS_SPS_EditKey_TermScope",
                                      "RM_JS_SPS_EditKey_TermDisplayForm",
                                      "RM_JS_SPS_EditKey_DefaultValue",
                                      "RM_SPS_AutoClassification_DeployTermMethod",
                                      "RM_MachineLearning_IntelligenceMA",
                                      "RM_MachineLearning_IntelligenceReviewers",
                                      "RM_MA_Setting_Email_Notification",
                                      "RM_BCM_Audit_NameConflictOption",
                                      "RM_SPS_AutoClassification_DefaultConditionTitle",
                                      "RM_SPS_Auto_RunFullJob",
                                      "RM_JS_SPS_EditKey_Action",
                                      "RM_JS_SPS_IncludeDSetAndFolder",
                                      "RM_JS_SPS_IncludeDeclaredRecords");
                                      
                                    if (node.Level != (int)NodeLevel.WebApplication)
                                    {
                                        RemoveByKeys(listKeyPair, "RM_SP_SettingRelatedRecords");
                                    }
                                }
                                else
                                {
                                    RemoveByKeys(listKeyPair, "RM_JS_SPS_EditKey_ColumnNameDescription", "RM_JS_SPS_DisplayColumnRequired", "RM_JS_SPS_HiddenColumn");
                                }   
                            }

                            if ((bool)info.ApplyTermIncludeFolder && info.DeployTermMethod != (int)DeployTermMethod.UseAutoClassification)
                            {
                                if (info.ApplyExistType == (int)ApplyExistingTermType.OverWrite)
                                {
                                    ModifyValueByKey(listKeyPair, "RM_JS_SPS_IncludeDSetAndFolder", $"{GetYesNoI18NEntity(true)}; {I18NEntity.GetString("RM_JS_SPS_AutoClassification_ApplySetsOverwirteTerm")}");
                                }
                                else if (info.ApplyExistType == (int)ApplyExistingTermType.SkipAndKeep)
                                {
                                    ModifyValueByKey(listKeyPair, "RM_JS_SPS_IncludeDSetAndFolder", $"{GetYesNoI18NEntity(true)}; {I18NEntity.GetString("RM_JS_SPS_AutoClassification_ApplySetsSkipTerm")}");
                                }
                            }
                            else if((bool)info.ApplyTermIncludeFolder)
                            {
                                ModifyValueByKey(listKeyPair, "RM_JS_SPS_IncludeDSetAndFolder", $"{GetYesNoI18NEntity(true)}");
                            }

                            if (!info.isEnableClassification)
                            {
                                ModifyValueByKey(listKeyPair, "RM_JS_SPS_EditTitle_ContainerLevelTermSetting", I18NEntity.GetString("RM_CP_Agent_Column_Status_Disabled"));
                                RemoveByKeys(listKeyPair, "RM_JS_SPS_EditKey_DescriptionOfContainer", "RM_JS_SPS_EditKey_EnableInheritParentTerm");
                            }
                          
                            return listKeyPair.ToDictionary();

                        }).ToList();

                        return new JMJobSetting
                        {
                            JobId = jobSetting.Id,
                            JobType = jobSetting.JobType,
                            Settings = JsonConvert.SerializeObject(settings),
                        };
                    }
                    return null;
                case (int)JobType.ApplyTeamsSettings:
                    var jobTeamsSetting = mSettingJobDao.GetRMSettingJob(i => i.Id == jobId && i.JobType == jobType);
                    if (jobTeamsSetting != null && jobTeamsSetting.JobInfos.Any())
                    {
                        string GetYesNoI18NEntity(bool value)
                        {
                            return value == true ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No");
                        }

                        void ModifyValueByKey(List<KeyValuePair<string, object>> keyValuePairs, string keyToModify, object valueToModify)
                        {
                            int index = keyValuePairs.FindIndex(pair => pair.Key == keyToModify);
                            if (index != -1)
                            {
                                keyValuePairs[index] = new KeyValuePair<string, object>(keyToModify, valueToModify);
                            }
                        }

                        void RemoveByKeys(List<KeyValuePair<string, object>> keyValuePairs, params string[] keys)
                        {
                            keyValuePairs.RemoveAll(kp => keys.Contains(kp.Key));
                        }

                        void ChangeKeyPosition(List<KeyValuePair<string, object>> keyValuePairs, string keyNeedToChange, string targetKey, bool isAfter = false)
                        {
                            var pairNeedToChange = keyValuePairs.FirstOrDefault(p => p.Key == keyNeedToChange);
                            if (!string.IsNullOrEmpty(pairNeedToChange.Key))
                            {
                                keyValuePairs.Remove(pairNeedToChange);
                                var targetIndex = keyValuePairs.FindIndex(item => item.Key == targetKey);
                                if (isAfter)
                                {
                                    targetIndex += 1;
                                }
                                keyValuePairs.Insert(targetIndex, pairNeedToChange);
                            }
                        }

                        var sortedSettings = new List<RMTeamsSetting>();

                        var jobInfos = (SerializerHelper.DeserializeByDataContractSerializer<List<RMTeamsSetting>>(jobTeamsSetting.JobInfos))
                                       .OrderBy(i => SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(i.NodeInfo).Level)
                                       .ToList();

                        if (jobInfos.Any(i => i.ScopeId == i.TeamsGroupId)) //check if the jobinfos include SiteContainer or not.
                        {
                            var containerSetting = jobInfos.FirstOrDefault(i => i.ScopeId == i.TeamsGroupId);
                            sortedSettings = jobInfos.Skip(1).OrderBy(i => i.FullPath).ToList();
                            sortedSettings.Insert(0, containerSetting);
                        }
                        else
                        {
                            sortedSettings = jobInfos.OrderBy(i => i.FullPath).ToList();
                        }

                        var settings = sortedSettings.Select(info =>
                        {
                            RMSPTreeNode node = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(info.NodeInfo);

                            var listKeyPair = new Dictionary<string, object>()
                                {
                                    { "RM_DAM_Scope", node.Level == (int)NodeLevel.WebApplication ? node.Name : !string.IsNullOrEmpty(node.Title) ? $"({node.Title}){node.FullPath}" : node.FullPath },
                                    { "RM_JS_SPS_EnableRecordsManagement", GetYesNoI18NEntity(info.EnableRecordManagement == 1) },
                                    { "RM_JS_SPS_EnableDataSync",GetYesNoI18NEntity(info.IsSyncData) },
                                    { "RM_JS_SPS_SupportLockedSite",GetYesNoI18NEntity(node.SupportLockedSite) },
                                    { "RM_JS_SPS_EnableLifecycleManagementForSharePointLists", GetYesNoI18NEntity(node.EnableLifecycleManagementForSharePointLists ?? true) },
                                    { "RM_JS_SPS_EditKey_ClassificationColumnName", info.ColumnName },
                                    { "RM_JS_SPS_EditKey_ColumnNameDescription", info.Description },
                                    { "RM_JS_SPS_Teams_HiddenColumn", GetYesNoI18NEntity(info.ColumnHidden ?? false) },
                                    { "RM_JS_SPS_DisplayColumnRequired", GetYesNoI18NEntity(info.ColumnRequired ?? false) },
                                    { "RM_JS_SPS_Teams_EditKey_ShowUniqueID", GetYesNoI18NEntity(info.IsShowUniqueId ?? false) },
                                    { "RM_JS_SPS_Teams_EditKey_KeepSPDefaultValue", GetYesNoI18NEntity(false) },
                                    { "RM_SP_SettingRelatedRecords", GetYesNoI18NEntity(info.EnableRelatedRecords)} ,
                                    { "RM_JS_SPS_EditKey_TermScope", node.TermScopeFullPath },
                                    { "RM_JS_SPS_EditKey_TermDisplayForm", info.IsDisplyaTermPath ? I18NEntity.GetString("RM_SPS_Auditor_DisplayTerm_EntirePath") : I18NEntity.GetString("RM_SPS_Auditor_DisplayTerm_TermLabel") },
                                    { "RM_SPS_AutoClassification_DeployTermMethod", "" },
                                    { "RM_SPS_AutoClassification_DefaultConditionTitle", "" },
                                    { "RM_MachineLearning_IntelligenceMA", GetYesNoI18NEntity(node.AIReviewers.Any())},
                                    { "RM_MachineLearning_IntelligenceReviewers", node.AIReviewers.Select(rw => rw.DisplayName)},
                                    { "RM_MA_Setting_Email_Notification", GetYesNoI18NEntity(info.AISendEMail) },
                                    { "RM_BCM_Audit_NameConflictOption", "" },
                                    { "RM_JS_SPS_EditKey_DefaultValue", !string.IsNullOrEmpty(node.DefaultTermFullPath) ? node.DefaultTermFullPath : node.DefaultTermName },
                                    { "RM_SPS_Auto_RunFullJob", GetYesNoI18NEntity(info.RunAutoFullJob) },
                                    { "RM_JS_SPS_EditKey_Action", GetYesNoI18NEntity(false) },
                                    { "RM_JS_SPS_IncludeDSetAndFolder", GetYesNoI18NEntity(false) },
                                    { "RM_JS_SPS_IncludeDeclaredRecords", GetYesNoI18NEntity(info.IncludeDeclaredRecords) },
                                    { "RM_JS_SPS_EditTitle_ContainerLevelTermSetting", !string.IsNullOrEmpty(node.ContainerTermFullPath) ? node.ContainerTermFullPath : node.TermNameOfContainer },
                                    { "RM_JS_SPS_EditKey_DescriptionOfContainer", info.DescriptionOfContainer },
                                    { "RM_JS_SPS_EditKey_EnableInheritParentTerm", GetYesNoI18NEntity(info.IsInheritParentTerm) },
                            }.ToList();

                            if (info.EnableRecordManagement != 1)
                            {
                                return new Dictionary<string, object>
                                {
                                    { "RM_DAM_Scope", node.Level == (int)NodeLevel.WebApplication ? node.Name : !string.IsNullOrEmpty(node.Title) ? $"({node.Title}){node.FullPath}" : node.FullPath },
                                    { "RM_JS_SPS_EnableRecordsManagement", GetYesNoI18NEntity(false) },
                                };
                            }

                            if (node.Level != (int)NodeLevel.WebApplication 
                            && node.Level != (int)NodeLevel.Office365GroupEntire 
                            && node.Level != (int)NodeLevel.SiteCollection)
                            {
                                if (node.Level >= (int)NodeLevel.List)
                                {
                                    if (node.Level >= (int)NodeLevel.Folder)
                                    {
                                        RemoveByKeys(listKeyPair,
                                        "RM_JS_SPS_EditKey_TermDisplayForm",
                                        "RM_JS_SPS_EditTitle_ContainerLevelTermSetting",
                                        "RM_JS_SPS_EditKey_DescriptionOfContainer",
                                        "RM_JS_SPS_EditKey_EnableInheritParentTerm");
                                    }

                                    RemoveByKeys(listKeyPair, "RM_SP_SettingRelatedRecords");
                                }

                                RemoveByKeys(listKeyPair, "RM_JS_SPS_EnableDataSync", "RM_JS_SPS_EnableLifecycleManagementForSharePointLists");
                                RemoveByKeys(listKeyPair, "RM_JS_SPS_SupportLockedSite");
                            }

                            if (node.Level == (int)NodeLevel.SiteCollection || node.Level == (int)NodeLevel.Site)
                            {
                                ChangeKeyPosition(listKeyPair, "RM_SP_SettingRelatedRecords", "RM_JS_SPS_EditTitle_ContainerLevelTermSetting");
                            }

                            if ((info.IsKeepSharePointDefaultValue && info.SetTermForEmptyDefaultValue) || (node.IsKeepSharePointDefaultValue && node.SetTermForEmptyDefaultValue))
                            {
                                ModifyValueByKey(listKeyPair, "RM_JS_SPS_Teams_EditKey_KeepSPDefaultValue", $"{GetYesNoI18NEntity(true)}; {I18NEntity.GetString("RM_SPS_Teams_NoSetTermForEmptyDefaultValue_Title")}");
                            }
                            else if (info.IsKeepSharePointDefaultValue || node.IsKeepSharePointDefaultValue)
                            {
                                ModifyValueByKey(listKeyPair, "RM_JS_SPS_Teams_EditKey_KeepSPDefaultValue", GetYesNoI18NEntity(true));
                            }

                            switch (info.DeployTermMethod)
                            {
                                case (int)DeployTermMethod.UseDefaultTerm:
                                    ModifyValueByKey(listKeyPair, "RM_SPS_AutoClassification_DeployTermMethod", I18NEntity.GetString("RM_JS_SPS_AutoClassification_UseDefault"));
                                    RemoveByKeys(listKeyPair, "RM_SPS_Auto_RunFullJob");
                                    if (info.NeedCheckDefaultValue)
                                    {
                                        if (info.ApplyExistType == (int)ApplyExistingTermType.OverWrite)
                                        {
                                            ModifyValueByKey(listKeyPair, "RM_JS_SPS_EditKey_Action", $"{GetYesNoI18NEntity(true)}; {I18NEntity.GetString("RM_JS_SPS_AutoClassification_ApplyOverwirteTerm")}");
                                        }
                                        else if (info.ApplyExistType == (int)ApplyExistingTermType.SkipAndKeep)
                                        {
                                            ModifyValueByKey(listKeyPair, "RM_JS_SPS_EditKey_Action", $"{GetYesNoI18NEntity(true)}; {I18NEntity.GetString("RM_JS_SPS_AutoClassification_ApplySkipTerm")}");
                                        }
                                    }

                                    if (!info.NeedCheckDefaultValue || !info.IncludeDeclaredRecords)
                                    {
                                        RemoveByKeys(listKeyPair, "RM_JS_SPS_IncludeDeclaredRecords");
                                    }
                                    else
                                    {
                                        var targetIndex = listKeyPair.FindIndex(p => p.Key == "RM_JS_SPS_EditKey_Action");
                                        ChangeKeyPosition(listKeyPair, "RM_JS_SPS_IncludeDeclaredRecords", "RM_JS_SPS_EditKey_Action", true);
                                    }

                                    break;

                                case (int)DeployTermMethod.NoDefaultTerm:
                                    ModifyValueByKey(listKeyPair, "RM_SPS_AutoClassification_DeployTermMethod", I18NEntity.GetString("RM_JS_SPS_AutoClassification_NoDefaultValue"));
                                    RemoveByKeys(listKeyPair,
                                    "RM_SPS_Auto_RunFullJob",
                                    "RM_JS_SPS_EditKey_DefaultValue",
                                    "RM_JS_SPS_EditKey_Action",
                                    "RM_JS_SPS_IncludeDSetAndFolder",
                                    "RM_JS_SPS_IncludeDeclaredRecords");
                                    break;

                                case (int)DeployTermMethod.UseAutoClassification:
                                    RemoveByKeys(listKeyPair,
                                    "RM_SPS_AutoClassification_DeployTermMethod",
                                    "RM_JS_SPS_EditKey_DefaultValue",
                                    "RM_JS_SPS_EditKey_Action");

                                    var autoDefaultCondition = "";
                                    var autoClassification = SerializerHelper.DeserializeByDataContractSerializer<List<ClassificationRule>>(info.AutoClassificationRules).FirstOrDefault();
                                    ArgumentCheck.CheckNotNull(autoClassification);
                                    if (info.AITermUseType == ArtificialIntelligenceTermUseType.AutoDefault)
                                    {
                                        autoDefaultCondition = I18NEntity.GetString("RM_MachineLearning_DeployTermMethodIntelligence");
                                    }
                                    else if (info.AITermUseType == ArtificialIntelligenceTermUseType.None)
                                    {
                                        autoDefaultCondition = (bool)autoClassification?.NoDefaultTerm ? I18NEntity.GetString("RM_JS_SPS_AutoClassification_NoDefaultValue") : I18NEntity.GetString("RM_JS_SPS_AutoClassification_UseDefault");

                                        if (!autoClassification.NoDefaultTerm)
                                        {
                                            var _targetIndex = listKeyPair.FindIndex(item => item.Key == "RM_SPS_AutoClassification_DefaultConditionTitle");
                                            listKeyPair.Insert(_targetIndex + 1, new KeyValuePair<string, object>("RM_JS_SPS_EditKey_DefaultValue", autoClassification.TermName));
                                        }

                                        RemoveByKeys(listKeyPair,
                                        "RM_MachineLearning_IntelligenceMA",
                                        "RM_MachineLearning_IntelligenceReviewers",
                                        "RM_MA_Setting_Email_Notification");
                                    }

                                    ModifyValueByKey(listKeyPair, "RM_SPS_AutoClassification_DefaultConditionTitle", autoDefaultCondition);

                                    break;

                                case (int)DeployTermMethod.UseIntelligenceClassification:
                                    ModifyValueByKey(listKeyPair, "RM_SPS_AutoClassification_DeployTermMethod", I18NEntity.GetString("RM_MachineLearning_DeployTermMethodIntelligence"));
                                    RemoveByKeys(listKeyPair,
                                    "RM_JS_SPS_EditKey_DefaultValue",
                                    "RM_JS_SPS_EditKey_Action",
                                    "RM_JS_SPS_IncludeDSetAndFolder",
                                    "RM_SPS_AutoClassification_DefaultConditionTitle");

                                    if (!node.AIReviewers.Any())
                                    {
                                        RemoveByKeys(listKeyPair, "RM_MA_Setting_Email_Notification", "RM_MachineLearning_IntelligenceReviewers");
                                    }

                                    break;

                                default: break;
                            }

                            if (info.DeployTermMethod != (int)DeployTermMethod.UseIntelligenceClassification
                            && (info.DeployTermMethod != (int)DeployTermMethod.UseAutoClassification && info.AITermUseType != ArtificialIntelligenceTermUseType.AutoDefault))
                            {
                                RemoveByKeys(listKeyPair,
                                "RM_MachineLearning_IntelligenceMA",
                                "RM_MachineLearning_IntelligenceReviewers",
                                "RM_MA_Setting_Email_Notification",
                                "RM_BCM_Audit_NameConflictOption",
                                "RM_SPS_AutoClassification_DefaultConditionTitle");
                            }
                            else
                            {
                                if (info.AutoJobOption == (int)AutoJobOption.SkipAndKeep)
                                {
                                    ModifyValueByKey(listKeyPair, "RM_BCM_Audit_NameConflictOption", I18NEntity.GetString("RM_JS_SPS_AutoClassification_SkipOverrideOption_Skip"));
                                }
                                else if (info.AutoJobOption == (int)AutoJobOption.Override)
                                {
                                    ModifyValueByKey(listKeyPair, "RM_BCM_Audit_NameConflictOption", I18NEntity.GetString("RM_JS_SPS_AutoClassification_SkipOverrideOption_Override"));
                                }

                                if (!node.AIReviewers.Any())
                                {
                                    RemoveByKeys(listKeyPair, "RM_MA_Setting_Email_Notification", "RM_MachineLearning_IntelligenceReviewers");
                                }
                            }

                            if (info.IsUsingExistColumnName)
                            {
                                string applyTermSetting = info.SetDocLevelTermForExistColumn ? "RM_JS_SPS_UseTermSettingsDefinedInRecords" : "RM_JS_SPS_UseTermSettingsDefinedInSP";
                                ModifyValueByKey(listKeyPair, "RM_JS_SPS_EditKey_ClassificationColumnName", $"{node.ExistColumnName}{string.Format(I18NEntity.GetString("RM_JS_SPS_ExistingColumn"), I18NEntity.GetString(applyTermSetting))}");

                                if (!info.SetDocLevelTermForExistColumn)
                                {
                                    RemoveByKeys(listKeyPair,
                                      "RM_JS_SPS_EditKey_ColumnNameDescription",
                                      "RM_JS_SPS_Teams_HiddenColumn",
                                      "RM_JS_SPS_DisplayColumnRequired",
                                      "RM_JS_SPS_EditKey_TermScope",
                                      "RM_JS_SPS_EditKey_TermDisplayForm",
                                      "RM_JS_SPS_EditKey_DefaultValue",
                                      "RM_SPS_AutoClassification_DeployTermMethod",
                                      "RM_MachineLearning_IntelligenceMA",
                                      "RM_MachineLearning_IntelligenceReviewers",
                                      "RM_MA_Setting_Email_Notification",
                                      "RM_BCM_Audit_NameConflictOption",
                                      "RM_SPS_AutoClassification_DefaultConditionTitle",
                                      "RM_SPS_Auto_RunFullJob",
                                      "RM_JS_SPS_EditKey_Action",
                                      "RM_JS_SPS_IncludeDSetAndFolder",
                                      "RM_JS_SPS_IncludeDeclaredRecords");

                                    if (node.Level != (int)NodeLevel.WebApplication)
                                    {
                                        RemoveByKeys(listKeyPair, "RM_SP_SettingRelatedRecords");
                                    }
                                }
                                else
                                {
                                    RemoveByKeys(listKeyPair, "RM_JS_SPS_EditKey_ColumnNameDescription", "RM_JS_SPS_DisplayColumnRequired", "RM_JS_SPS_HiddenColumn");
                                }
                            }

                            if ((bool)info.ApplyTermIncludeFolder && info.DeployTermMethod != (int)DeployTermMethod.UseAutoClassification)
                            {
                                if (info.ApplyExistType == (int)ApplyExistingTermType.OverWrite)
                                {
                                    ModifyValueByKey(listKeyPair, "RM_JS_SPS_IncludeDSetAndFolder", $"{GetYesNoI18NEntity(true)}; {I18NEntity.GetString("RM_JS_SPS_AutoClassification_ApplySetsOverwirteTerm")}");
                                }
                                else if (info.ApplyExistType == (int)ApplyExistingTermType.SkipAndKeep)
                                {
                                    ModifyValueByKey(listKeyPair, "RM_JS_SPS_IncludeDSetAndFolder", $"{GetYesNoI18NEntity(true)}; {I18NEntity.GetString("RM_JS_SPS_AutoClassification_ApplySetsSkipTerm")}");
                                }
                            }
                            else if ((bool)info.ApplyTermIncludeFolder)
                            {
                                ModifyValueByKey(listKeyPair, "RM_JS_SPS_IncludeDSetAndFolder", $"{GetYesNoI18NEntity(true)}");
                            }

                            if (!info.isEnableClassification)
                            {
                                ModifyValueByKey(listKeyPair, "RM_JS_SPS_EditTitle_ContainerLevelTermSetting", I18NEntity.GetString("RM_CP_Agent_Column_Status_Disabled"));
                                RemoveByKeys(listKeyPair, "RM_JS_SPS_EditKey_DescriptionOfContainer", "RM_JS_SPS_EditKey_EnableInheritParentTerm");
                            }

                            return listKeyPair.ToDictionary();

                        }).ToList();

                        return new JMJobSetting
                        {
                            JobId = jobTeamsSetting.Id,
                            JobType = jobTeamsSetting.JobType,
                            Settings = JsonConvert.SerializeObject(settings),
                        };
                    }
                    return null;
                default: 
                    logger.Error($"Cannot get job setting infoes cause job type [{jobType}] is not supported.");
                    return null;
            }
        }

        public async Task<JMJobSummary> GetDAOJobSummaryDetailsAsync(string jobId, int type)
        {
            JMJobSummary summary = null;
            try
            {
                if (migrationJobSummaryServiceDictionary.ContainsKey(type))
                {
                    GeneralSettingModel gsm = await GeneralSettingService.GetGeneralSettingAsync();
                    (JMJobSummary, SOJob) result = migrationJobSummaryServiceDictionary[type].GetSummaryBasicInfo(jobId, gsm);
                    summary = result.Item1;
                    SOJob jobInfo = result.Item2;
                    
                    summary.DisposalSummary = migrationJobSummaryServiceDictionary[type].GetJobSummaryInfo(jobInfo, gsm);
                }
            }
            catch (Exception e)
            {
                logger.Warn($"GetDAOJobSummaryDetailsAsync error: {e}");
            }
            return summary;
        }

        public int DelJobReportFiles(List<BaseJobDto> jobInfos)
        {
            string expandedName = ".rpt";
            var successCount = 0;
            foreach (var jobInfo in jobInfos)
            {
                string rptPath = JobReportUtility.GetJobReportPath(jobInfo, expandedName);
                try
                {
                    File.Delete(rptPath);
                    successCount++;
                }
                catch (Exception e)
                {
                    logger.Warn("delete file {0} error, message:{1}", rptPath, e.ToString());
                }
            }
            return successCount;
        }

        public List<string> GetRunningSharePointSettingJob()
        {
            return JMDao.GetSharePointSettingJobs();
        }

        public List<string> GetRunningTeamsSettingJob()
        {
            return JMDao.GetTeamsSettingJobs();
        }

        public List<string> GetRunningEXOApplySettingJob()
        {
            return JMDao.GetRunningEXOApplySettingJob();
        }

        public List<string> GetRunningSharePointOnPremiseSettingJob()
        {
            return JMDao.GetSharePointOnPremiseSettingJobs();
        }

        private List<string> GetInQueueJobsIds(List<string> keys)
        {
            var inQueueJobScopeIds = Client.GetJobInQueue(keys);
            return JMDao.GetJobIdsByScopeId(inQueueJobScopeIds);
        }

        public bool CheckHasRunningManualJob()
        {
            return JMDao.CheckHasRunningManualJob();
        }

        public bool CheckCurrentUserHasRunningJob(string containerId, string jobId)
        {
            return JMDao.CheckCurrentUserHasRunningJob(containerId,jobId);
        }

        public bool CheckStoppedJobByDiscoveryJobId(Guid mainJobId)
        {
            return JMDao.CheckStoppedJobByDiscoveryJobId(mainJobId).GetAwaiter().GetResult();
        }

        #region Job Export Setting


        public async Task<string> GetExportSettingsAsync(bool loadExportLocation)
        {
            if (loadExportLocation)
            {
                JobExportSettingResult rst = new JobExportSettingResult();
                rst.AllExportLocation = new List<Contract.RMWeb.CP.ExportLocation>();
                var allExportLocations = await Client.GetAllExportLocationAsync();
                if (TenantService.IsNewOpusTenant())
                {
                    foreach (var item in allExportLocations)
                    {
                        if ((item.Type == (int)StorageType.AzureBlob && !item.Id.Equals(RecordsConstants.AVEPOINT_DEFAULT_STORAGEID, StringComparison.OrdinalIgnoreCase) && !item.IsSystemStorage) || item.Type == (int)StorageType.SFTP)
                        {
                            rst.AllExportLocation.Add(new Contract.RMWeb.CP.ExportLocation() { ID = item.Id, Name = item.Name });
                        }
                    }
                }
                else
                {
                    rst.AllExportLocation.AddRange(allExportLocations.ConvertAll(item => new Contract.RMWeb.CP.ExportLocation() { ID = item.Id, Name = item.Name }));
                }

                var dto = new JobExportSettingDto();
                var setting = JESDao.GetExportSetting();
                if (setting == null)
                {
                    dto.ExportSetting = 0;
                    dto.ExportLocationId = Guid.Empty;
                }
                else
                {
                    dto.ExportSetting = setting.ExportSetting;
                    dto.ExportLocationId = setting.ExportLocationId;
                    dto.LocationName = setting.LocationName;
                }
                rst.JobExportSetting = dto;
                return JsonConvert.SerializeObject(rst);
            }
            else
            {
                var setting = JESDao.GetExportSetting();
                if (setting == null)
                {
                    return 0.ToString();
                }
                else
                {
                    return setting.ExportSetting.ToString();
                }
            }
        }

        [Audit(Module = AuditModule.JobMonitor, Category = AuditCategory.DownloadSettings, Action = AuditAction.ConfigDownloadSettings, BeforeHandler = typeof(JobMonitorServiceBeforeAuditHandler), AfterHandler = typeof(JobMonitorServiceAuditHandler))]
        public async Task<RAReturnMessage> SaveExportSettingsAsync(JobExportSettingDto setting)
        {
            RAReturnMessage returnMessage = new RAReturnMessage()
            {
                MessageType = RAMessageType.Successful
            };
            try
            {
                var exportLocationDic = await GlobalSettingService.GetExportLocationTypesAsync();
                if (exportLocationDic.ContainsKey(setting.ExportLocationId) && exportLocationDic[setting.ExportLocationId] == 1)
                {
                    returnMessage.MessageType = RAMessageType.Failed;
                    returnMessage.ErrorMessage = I18NEntity.GetString("RM_JS_CP_GSS_FTPExportLocationNotSupported");
                    return returnMessage;
                }
                var db = new RMJobExportSetting();
                db.ExportLocationId = setting.ExportLocationId;
                db.ExportSetting = setting.ExportSetting;
                db.LocationName = setting.LocationName;
                await JESDao.CreateOrSaveExportSettingAsync(db);
            }
            catch (Exception e)
            {
                returnMessage.MessageType = RAMessageType.Failed;
                returnMessage.ErrorMessage = e.Message;
                logger.Error("save export settings error:{0}", e.ToString());
            }
            return returnMessage;
        }

        public void StartExportJob(string exportJobId)
        {
            string id = string.Empty;
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.ExportToLocation,
                    Parameters = exportJobId,
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName
                };
                id = mJobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while StartExportJob,ERROR:{0}", ex.ToString());
            }

        }


        #endregion

        #region Sub Job Operation
        public List<RMSubJobDto> GetRunnableSubJob()
        {
            List<RMSubJobDto> dtos = new List<RMSubJobDto>();
            List<RMSubJob> subJobs = SubJobDao.GetRunableSubJobList();
            subJobs.ForEach(a => dtos.Add(ConvertUtil.ConvertSubJob2Dto(a)));
            return dtos;
        }

        public string GetJobContextSettingByJobId(string jobId)
        {
            return SubJobDao.GetJobContextSettingByJobId(jobId);
        }

        public List<string> GetRunningMoveJobByDestUrl(string destUrl)
        {
            return SubJobDao.GetRunningMoveSubJobByDest(destUrl, false);
        }
        public bool UpdateRunable(string id, int runnable, bool updateState = false)
        {
            return SubJobDao.UpdateRunable(id, runnable, updateState);
        }

        public List<string> GetRunningSetPermissionJob(string exceptJobId)
        {
            return SubJobDao.GetRunningSetPermissionJobIds(exceptJobId);
        }

        public Dictionary<JobType, int> GetRunningAndRunnableSubJobCount()
        {
            return SubJobDao.GetRunningAndRunnableSubJobCount();
        }
        public void ChangeRunnableWiating2CanRun(JobType jobType, int count)
        {
            SubJobDao.ChangeRunnableWiating2CanRun(jobType, count);
        }
        public async Task<List<SubJobsResult>> GetSubJobsAsync(COPSubJobRequest request)
        {

            var pageIndex = request.PageIndex;
            var pageSize = request.PageSize;
            var subjobs = SubJobDao.QueryAllSubJobs(request);
            var response = subjobs.Select(s => new SubJobsResult
            {
                JobType = GetJobTypeName(s.JobType),
                SubJobId = s.Id,
                StartTime = s.StartTime == 0 ? "" : s.StartTime.ToString(),
                EndTime = s.EndTime == 0 ? I18NEntity.GetString("RM_JS_JM_EndTimePending") : s.EndTime.ToString(),
                Status = (JobStatus)s.Status,
                Duration = (s.EndTime > 0 && s.StartTime > 0) ? TimeSpan.FromTicks(s.EndTime - s.StartTime).ToString(@"hh\:mm\:ss") : null,
                Progress = s.Progress
            }).ToList();
            return response;
        }
        #endregion

        #region archiver
        public List<string> GetRunningArchiverJobsScopes(List<JobType> types)
        {
            return SubJobDao.GetRunningArchiverJobsScopes(types);
        }

        public bool HasRunningArchiverJobOnScope(List<JobType> types, string scope)
        {
            return JMDao.HasRunningArchiverJobOnScope(types, scope);
        }

        public List<string> FilterRunnableSOJobSitesInContainerForImportedSites(string containerId, List<string> siteUrls)
        {
            if (string.IsNullOrWhiteSpace(containerId) || siteUrls == null || siteUrls.Count == 0)
            {
                return new List<string>();
            }

            var filterSiteCollections = NormalizeSiteCollectionFilters(siteUrls);
            if (filterSiteCollections.Count == 0)
            {
                return new List<string>();
            }

            var runnableSites = new HashSet<string>(filterSiteCollections, StringComparer.OrdinalIgnoreCase);

            try
            {
                var runningJobs = JMDao.HasRunningArchiverJob(JobTypeConstants.NeedCheckInSiteCollectinMethod);
                foreach (var job in runningJobs)
                {
                    if (!string.Equals(job.ContainerId, containerId, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var matchedScopes = SubJobDao.GetSubJobScopesByMainJobId(job.Id, filterSiteCollections.ToArray());
                    foreach (var scope in matchedScopes)
                    {
                        if (runnableSites.Contains(scope))
                        {
                            runnableSites.Remove(scope);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("HasRunningSOJobSiteInContainer error, containerId:{0}, siteUrls:{1}, error:{2}", containerId, string.Join(",", filterSiteCollections), ex);
            }

            logger.Info("HasRunningSOJobSiteInContainer runnable sites, containerId:{0}, runnableSites:{1}", containerId, string.Join(",", runnableSites));
            return runnableSites.ToList();
        }

        public List<string> GetRunningArchiverJobOnScope(List<JobType> types, string scope)
        {
            return JMDao.GetRunningArchiverJobOnScope(types, scope);
        }

        public List<string> GetRunningArchiverJobSiteUrl(IEnumerable<JobType> types, IEnumerable<string> siteCollectionUrls, bool includeTeamsExtra = false)
        {
            List<string> conflictScopes = new List<string>();
            List<string> filterSiteCollections = NormalizeSiteCollectionFilters(siteCollectionUrls);
            try
            {
                IEnumerable<RMJobMonitor> runningJobs = JMDao.HasRunningArchiverJob(JobTypeConstants.NeedCheckInSiteCollectinMethod.Intersect(types).ToList());

                foreach (var job in runningJobs)
                {
                    try
                    {
                        List<string> jobScopes = ResolveArchiverScopesForJob(job, filterSiteCollections);
                        conflictScopes.AddRange(jobScopes);
                    }
                    catch (Exception ex)
                    {
                        logger.Error("GetRunningArchiverJobSiteUrlSlim resolve job scopes error,job id:{0},error:{1}", job.Id, ex);
                    }
                }

                if (includeTeamsExtra && TeamsPermissionHelper.HasUpgradeTeamsFeature())
                {
                    var filterDic = BuildTeamsFilterBySiteUrls(filterSiteCollections);
                    var runningTeams = GetRunningTeamsArchiverJobSiteUrl(
                        JobTypeConstants.NeedCheckInTeamMethod.Intersect(types).ToList()
                        , true, filterDic);
                    conflictScopes.AddRange(runningTeams.SelectMany(team => team.Value));
                }
            }
            catch (Exception ex)
            {
                logger.Error("GetRunningArchiverJobSiteUrlSlim error:{0}", ex);
            }

            return conflictScopes.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        public List<string> GetRunningTeamsArchiverJobSiteUrl(IEnumerable<JobType> types, IEnumerable<string> siteCollectionUrls)
        {
            List<string> conflictScopes = new List<string>();
            List<string> filterSiteCollections = NormalizeSiteCollectionFilters(siteCollectionUrls);
            try
            {
                IEnumerable<RMJobMonitor> runningJobs = JMDao.HasRunningArchiverJob(types.ToList());

                foreach (var job in runningJobs)
                {
                    try
                    {
                        List<string> jobScopes = ResolveArchiverScopesForJob(job, filterSiteCollections);
                        conflictScopes.AddRange(jobScopes);
                    }
                    catch (Exception ex)
                    {
                        logger.Error("GetRunningArchiverJobSiteUrlSlim resolve job scopes error,job id:{0},error:{1}", job.Id, ex);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("GetRunningArchiverJobSiteUrlSlim error:{0}", ex);
            }

            return conflictScopes.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        public HashSet<string> GetRunningArchiverJobs()
        {
            return JMDao.HasRunningArchiverJob(JobTypeConstants.ArchiveSiteConflictType).Select(job => job.Id).ToHashSet();
        }

        private List<string> ResolveArchiverScopesForJob(RMJobMonitor job, List<string> filterSiteCollections)
        {
            List<string> scopes = new List<string>();
            
            if (string.IsNullOrEmpty(job.JobConflictExtension))
            {
                logger.Error($"job conflict is null:{job.Id}  {job.JobType}");
                return scopes;
            }
            ArchiveJobMonitorExtension jobExtension = SerializerHelper.DeserializeByDataContractSerializer<ArchiveJobMonitorExtension>(job.JobConflictExtension);
            bool hasCacheSiteUrls = jobExtension.SiteUrls != null && jobExtension.SiteUrls.Any();
            if (hasCacheSiteUrls)
            {
                jobExtension.SiteUrls = jobExtension.SiteUrls
                    .Where(sc => filterSiteCollections.Any(file => file.Equals(sc, StringComparison.OrdinalIgnoreCase))).ToList();
            }

            if (job.JobType == (int)JobType.ConvertStub)
            {
                scopes.AddRange(FormatScope(GetConvertStubArchiverScopesByExtension(job, jobExtension, filterSiteCollections)));
            }
            else if (job.JobType == (int)JobType.RMEndUserArchiverBackup)
            {
                Dictionary<string, List<string>> scAndFileMapping = jobExtension.SiteUrls.ToDictionary(url => url, url => new List<string>());
                foreach (string fileUrl in jobExtension.ProcessNodeUrls)
                {
                    foreach (string sc in scAndFileMapping.Keys.OrderDescending())
                    {
                        if (RuleSPTreeUtil.IsPrefixWithSlash(sc, fileUrl))
                        {
                            scAndFileMapping[sc].Add(fileUrl);
                        }
                    }
                }
                scAndFileMapping = scAndFileMapping
                    .Where(kv => filterSiteCollections.Any(sc => sc.Equals(kv.Key, StringComparison.OrdinalIgnoreCase)))
                    .ToDictionary(kv => kv.Key, kv => kv.Value);
                scopes.AddRange(FormatScope(scAndFileMapping.SelectMany(kv => kv.Value)));
            }
            else if (hasCacheSiteUrls)
            {
                bool isUnderSiteCollectionRunJob = 
                    jobExtension.SiteUrls.Count == 1 
                    && !string.IsNullOrWhiteSpace(jobExtension.SiteUrls.First())
                    && RuleSPTreeUtil.IsPrefixWithSlash(jobExtension.SiteUrls.First(), job.ScopeId ?? string.Empty);
                if (isUnderSiteCollectionRunJob)
                {
                    scopes.AddRange(FormatScope([job.ScopeId]));
                }
                else
                {
                    scopes.AddRange(FormatScope(jobExtension.SiteUrls));
                }
            }
            else
            {
                var subJobScopes = SubJobDao.GetSubJobScopesByMainJobId(job.Id, filterSiteCollections.ToArray());
                scopes.AddRange(FormatScope(subJobScopes));
            }

            return scopes;
        }

        private List<string> GetConvertStubArchiverScopesByExtension(RMJobMonitor job, ArchiveJobMonitorExtension jobExtension, List<string> filterSiteCollections)
        {
            if (jobExtension == null)
            {
                return new List<string>();
            }

            if (jobExtension.IsGroupLevelArchive)
            {
                var remoteSites = RMRemoteNodeDao.GetRemoteSiteCollectionBySiteUrls(filterSiteCollections, [jobExtension.GroupNode.SPObjectId]);
                return remoteSites.Where(site => site != null).Select(site => site.url).ToList();
            }
            else
            {
                return jobExtension?.SiteUrls ?? new ();
            }
        }

        private List<string> NormalizeSiteCollectionFilters(IEnumerable<string> siteCollectionUrls)
        {
            if (siteCollectionUrls == null)
            {
                return new List<string>();
            }

            return siteCollectionUrls
                .Where(url => !string.IsNullOrWhiteSpace(url))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private List<string> FormatScope(IEnumerable<string> scopes)
        {
            if (scopes == null)
            {
                return new List<string>();
            }

            return scopes.Where(url => !string.IsNullOrWhiteSpace(url)).ToList();
        }

        private Dictionary<string, List<string>> BuildTeamsFilterBySiteUrls(List<string> siteCollectionUrls)
        {
            Dictionary<string, List<string>> res = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            if (siteCollectionUrls.IsNullOrEmpty())
            {
                return res;
            }

            Dictionary<RemoteSiteCollection, List<RemoteSiteCollection>> teams = RMRemoteNodeDao.GetTeamsGroupAndChannelsCollectionBySiteUrls(siteCollectionUrls);
            foreach (var team in teams)
            {
                string teamName = team.Key.Name;
                if (!res.ContainsKey(teamName))
                {
                    res[teamName] = new List<string>();
                }
                res[teamName].AddRange(team.Value.Select(sc => sc.url));
            }

            return res;
        }

        public async Task<List<string>> GetRunningDriveNodeIds(List<JobType> types)
        {
            List<string> runningDriveIds = [];
            try
            {
                var runningJobs = JMDao.HasRunningArchiverJob(types);
                if (runningJobs.IsNotNullOrEmpty())
                {
                    logger.Info("GetRunningDriveNodeIds runningJobs count:{0}", runningJobs.Count);
                    foreach (var job in runningJobs)
                    {
                        logger.Info("GetRunningDriveNodeIds runningSoJob id:{0}", job.Id);
                        if (!string.IsNullOrEmpty(job.JobConflictExtension))
                        {
                            var jobExtension = SerializerHelper.DeserializeByDataContractSerializer<ArchiveGoogleJobMonitorExtension>(job.JobConflictExtension);
                            if (jobExtension.IsDriveContainer)
                            {
                                if(jobExtension.TreeMode == TreeMode.LifeGDrive)
                                {
                                    logger.Info($"GetRunningDriveNodeIds this job is container level ,need get drives by container browser,LifeGDrive mode");
                                    List<RMGoogleTreeNode> drives = await RemoteGoogleNodeService.BrowserRMTreeAsync(jobExtension.ContainerNode);
                                    List<string> breakTreeNodes = [];
                                    if (drives.IsNullOrEmpty())
                                    {
                                        return runningDriveIds;
                                    }
                                    var parentId = ScheduleService.GetProfileId(jobExtension.ContainerNode) + "|";

                                    var treeNodes = RMScheduleDao.GetDisposalBreakNodes(parentId);
                                    foreach (var item in treeNodes)
                                    {

                                        var node = JsonConvert.DeserializeObject<RMGoogleTreeNode>(item);
                                        if (node.Level is (int)GCommon.Contract.Tree.Object.NodeLevel.GoogleMyDriveContainer  or (int)GCommon.Contract.Tree.Object.NodeLevel.GoogleSharedDriveContainer)
                                        {
                                            continue;
                                        }
                                        breakTreeNodes.Add(node.ObjectId);
                                    }
                                    foreach (var drive in drives)
                                    {
                                        if (!breakTreeNodes.Contains(drive.ObjectId))
                                        {
                                            runningDriveIds.Add(drive.ObjectId);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (jobExtension.DriveIds != null)
                                {
                                    runningDriveIds.AddRange(jobExtension.DriveIds);
                                }
                            }
                        }
                        else
                        {
                            logger.Info($"GetRunningDriveNodeIds job.Extension is null,job id:{job.Id}");
                        }
                        
                        logger.Info($"this running job scope id is:{job.ScopeId}");
                    }
                }
                return runningDriveIds;
            }
            catch(Exception e)
            {
                logger.Error("GetRunningDriveNodeIds error:{0}", e.ToString());
                return runningDriveIds;
            }
        }

        private bool ShouldSkipCheckCurrentJobRunningTeams(RMJobMonitor job, string skipJobId = "")
        {
            if (job.Id.Equals(skipJobId))
            {
                logger.Info("GetRunningArchiverJobSiteUrl skip current job id:{0}", job.Id);
                return true;
            }
            if (string.IsNullOrEmpty(job.JobConflictExtension))
            {
                logger.Info("GetRunningArchiverJobSiteUrl job.JobConflictExtension is null ,job id:{0}", job.Id);
                return true;
            }
            return false;
        }
        
        public Dictionary<string, List<string>> GetRunningTeamsArchiverJobSiteUrl(List<JobType> types, bool needLoadSiteUrl, Dictionary<string, List<string>> filterTeamAncSCDic, string skipCurrentJobId = "")
        {
            Dictionary<string, List<string>> resultTuple = new Dictionary<string, List<string>>();
            filterTeamAncSCDic = filterTeamAncSCDic ?? new Dictionary<string, List<string>>();
            try
            {
                var runningSoJob = JMDao.HasRunningArchiverJob(types).Where(job => !ShouldSkipCheckCurrentJobRunningTeams(job, skipCurrentJobId)).ToList();
                logger.Info("GetRunningArchiverJobSiteUrl runningSoJob count:{0}", runningSoJob.Count);
                if (runningSoJob.IsNullOrEmpty())
                {
                    return resultTuple;
                }

                foreach (RMJobMonitor job in runningSoJob)
                {
                    ArchiveJobMonitorExtension jobExtension = SerializerHelper.DeserializeByDataContractSerializer<ArchiveJobMonitorExtension>(job.JobConflictExtension);
                    Dictionary<string, List<string>> temp = new Dictionary<string, List<string>>();
                    try
                    {
                        switch (jobExtension.ConflictNodeLevel)
                        {
                            case ConflictNodeLevel.Group:
                                temp = GetGroupRunningTeamsArchiverJobSiteUrl(job, jobExtension, needLoadSiteUrl, filterTeamAncSCDic);
                                break;
                            case ConflictNodeLevel.Teams:
                                temp = GetTeamsRunningTeamsArchiverJobSiteUrl(job, jobExtension, needLoadSiteUrl, filterTeamAncSCDic);
                                break;
                            case ConflictNodeLevel.ArchiverImportTeams:
                            case ConflictNodeLevel.TeamsApprovalProcessJob:
                                temp = GetArchiverImportTeamsGroupRunningTeamsArchiverJobSiteUrl(job, jobExtension, needLoadSiteUrl, filterTeamAncSCDic);
                                break;
                            case ConflictNodeLevel.ArchiverTeamsRetention:
                                temp = GetTeamsRetentionRunningTeamsArchiverJobSiteUrl(job, jobExtension, needLoadSiteUrl, filterTeamAncSCDic);
                                break;
                            case ConflictNodeLevel.SiteCollection:
                            default:
                                temp = GetSiteCollectionGroupRunningTeamsArchiverJobSiteUrl(job, jobExtension, needLoadSiteUrl, filterTeamAncSCDic);
                                break;
                        }
                        resultTuple = MergeResult(resultTuple, temp);
                    }
                    catch(Exception e)
                    {
                        logger.Error($"Fail load job conflict info for job :{job.Id},e:{e}");
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("HasRunningArchiverJobOnSiteUrl error:{0}", e.ToString());
            }
            return resultTuple;
        }

        private Dictionary<string, List<string>> MergeResult(Dictionary<string, List<string>> dic1, Dictionary<string, List<string>> dic2)
        {
            foreach(var item in dic2)
            {
                if (!dic1.ContainsKey(item.Key))
                {
                    dic1[item.Key] = item.Value;
                }
                else
                {
                    dic1[item.Key].AddRange(item.Value);
                    dic1[item.Key] = dic1[item.Key].Distinct().ToList();
                }
            }
            return dic1;
        }

        private Dictionary<string, List<string>> GetGroupRunningTeamsArchiverJobSiteUrl(RMJobMonitor job, ArchiveJobMonitorExtension jobExtension, bool needLoadSCUrl, Dictionary<string, List<string>> filterTeamAncSCDic)
        {
            Dictionary<string, List<string>> res = new Dictionary<string, List<string>>();
            if (jobExtension.IsGroupLevelArchive)
            {
                // meaning is sharepoint data source run job
                return res;
            }

            if (JobType.ConvertStub != (JobType)job.JobType)
            {
                var scopes = SubJobDao.GetSubJobScopesByMainJobId(job.Id, filterTeamAncSCDic.Keys.ToArray());
                res = scopes.ToDictionary(scope => scope, scope => new List<string>());
            }
            else if (jobExtension.treeMode == TreeMode.SO)
            {
                logger.Info($"GetRunningArchiverJobSiteUrl this job is group level ,need get teams by group browser");
                List<RemoteSiteCollection> teams = RMTeamsTreeService.GetTeamsUnderContainer(jobExtension.GroupNode.SPObjectId, filterTeamAncSCDic.Keys.ToList(), job.JobType != (int)JobType.ConvertStub).GetAwaiter().GetResult();
                res = teams.ToDictionary(t => t.Name, t => new List<string>());
            }
            else if (jobExtension.treeMode == TreeMode.LifeTeams)
            {
                logger.Info($"GetRunningArchiverJobSiteUrl this job is group level ,need get teams by group browser,lifeTeams mode");
                List<RemoteSiteCollection> teams = RMTeamsTreeService.GetTeamsUnderContainer(jobExtension.GroupNode.SPObjectId, filterTeamAncSCDic.Keys.ToList()).GetAwaiter().GetResult();
                res = teams.ToDictionary(t => t.Name, t => new List<string>());
            }

            if (res.Keys.IsNullOrEmpty())
            {
                return res;
            }

            if (jobExtension.treeMode == TreeMode.LifeTeams)
            {
                List<string> breakNodeInfo = GetAllLCBreakNodeInfo(jobExtension.GroupNode);
                res = res.Where(res => !breakNodeInfo.Contains(res.Key)).ToDictionary();
                if (needLoadSCUrl)
                {
                    LCLoadSCInfo(res, jobExtension, job, breakNodeInfo, filterTeamAncSCDic);
                }
            }
            else
            {
                if (needLoadSCUrl)
                {
                    LoadSCInfo(res, jobExtension, job, filterTeamAncSCDic);
                }
            }

            if(JobType.ConvertStub == (JobType)job.JobType)
            {
                res = res.ToDictionary(kv => Guid.NewGuid().ToString(), kv => kv.Value);
            }
            return res;
        }

        private List<string> GetAllLCBreakNodeInfo(RMSPTreeNode parent)
        {
            var parentId = ScheduleService.GetProfileId(parent) + "|";

            var treeNodes = RMScheduleDao.GetDisposalBreakNodes(parentId);
            List<string> mBreakTreeNode = new List<string>();
            foreach (var item in treeNodes)
            {

                var node = JsonConvert.DeserializeObject<RMSPTreeNode>(item);
                if (node.Level == (int)NodeLevel.WebApplication)
                {
                    continue;
                }
                mBreakTreeNode.Add(node.FullPath);
            }
            return mBreakTreeNode;
        }

        private void LoadSCInfo(Dictionary<string, List<string>> res, ArchiveJobMonitorExtension jobExtension, RMJobMonitor job, Dictionary<string, List<string>> filterTeamAncSCDic)
        {
            Dictionary<RemoteSiteCollection, List<RemoteSiteCollection>> sites = new();
            List<string> specialSearchUrls = filterTeamAncSCDic.Where(map => res.ContainsKey(map.Key)).SelectMany(map => map.Value).Distinct().ToList();
            sites = RMRemoteNodeDao.GetTeamsGroupAndChannelsCollectionBySiteUrls(specialSearchUrls);

            List<RMArchiverSetting> breakSCSettings = new List<RMArchiverSetting>();
            if (jobExtension.treeMode == TreeMode.SO && job.JobType != (int)JobType.ConvertStub)
            {
                breakSCSettings = ArchiverSettingDao.LoadArchiverSettingsUnderTeamsIds(sites.Keys.Select(key => new Guid(key.TeamId)));
            }
            foreach (var team in sites)
            {
                if (!res.ContainsKey(team.Key.Name))
                {
                    continue;
                }
                Func<RemoteSiteCollection, bool> predicate = sc => !breakSCSettings.Any(scSetting => scSetting.SPObjectId.ToString() == sc.ObjectId);
                res[team.Key.Name] = team.Value.Where(predicate).Select(sc => sc.url).ToList();
            }
        }

        private void LCLoadSCInfo(Dictionary<string, List<string>> res, ArchiveJobMonitorExtension jobExtension, RMJobMonitor job, List<string> breakInherNodes, Dictionary<string, List<string>> filterTeamAncSCDic)
        {
            Dictionary<RemoteSiteCollection, List<RemoteSiteCollection>> sites = new ();
            List<string> specialSearchUrls = filterTeamAncSCDic.Where(map => res.ContainsKey(map.Key)).SelectMany(map => map.Value).Distinct().ToList();
            sites = RMRemoteNodeDao.GetTeamsGroupAndChannelsCollectionBySiteUrls(specialSearchUrls);

            foreach (var team in sites)
            {
                if (!res.ContainsKey(team.Key.Name)) 
                {
                    continue;
                }
                Func<RemoteSiteCollection, bool> predicate = sc => !breakInherNodes.Any(nodePath => nodePath.Equals(sc.url, StringComparison.OrdinalIgnoreCase));
                res[team.Key.Name] = team.Value.Where(predicate).Select(sc => sc.url).ToList();
            }
        }

        private Dictionary<string, List<string>> GetTeamsRunningTeamsArchiverJobSiteUrl(RMJobMonitor job, ArchiveJobMonitorExtension jobExtension, bool needLoadSCUrl, Dictionary<string, List<string>> filterTeamAncSCDic)
        {
            Dictionary<string, List<string>> res = new Dictionary<string, List<string>>();
            if (JobType.ConvertStub != (JobType)job.JobType)
            {
                var scopes = SubJobDao.GetSubJobScopesByMainJobId(job.Id, filterTeamAncSCDic.Keys.ToArray());
                res = scopes.ToDictionary(scope => scope, scope => new List<string>());
            }
            else if (jobExtension.treeMode == TreeMode.SO)
            {
                res[jobExtension.GroupNode.FullPath] = new List<string>();
            }
            else if (jobExtension.treeMode == TreeMode.LifeTeams)
            {
                res[jobExtension.GroupNode.FullPath] = new List<string>();
            }

            if (res.Keys.IsNullOrEmpty())
            {
                return res;
            }

            if (needLoadSCUrl)
            {
                if (jobExtension.treeMode == TreeMode.LifeTeams)
                {
                    List<string> breakNodeInfo = GetAllLCBreakNodeInfo(jobExtension.GroupNode);
                    LCLoadSCInfo(res, jobExtension, job, breakNodeInfo, filterTeamAncSCDic);
                }
                else
                {
                    LoadSCInfo(res, jobExtension, job, filterTeamAncSCDic);
                }
            }

            if (JobType.ConvertStub == (JobType)job.JobType)
            {
                res = res.ToDictionary(kv => Guid.NewGuid().ToString(), kv => kv.Value);
            }

            return res;
        }

        private Dictionary<string, List<string>> GetTeamsRetentionRunningTeamsArchiverJobSiteUrl(RMJobMonitor job, ArchiveJobMonitorExtension jobExtension, bool needLoadSCUrl, Dictionary<string, List<string>> filterTeamAncSCDic)
        {
            Dictionary<string, List<string>> res = new Dictionary<string, List<string>>();
            List<string> scopes = new List<string>();

            if (jobExtension.teamsUrls != null && jobExtension.teamsUrls.Count > 0)
            {
                logger.Info($"GetRunningArchiverJobTeamsAddress this job is ArchiverTeamsRetention. Addresses: [{string.Join(", \n", jobExtension.teamsUrls)}]");
                res = jobExtension.teamsUrls.ToDictionary(team => team, team => new List<string>());
            }

            return res;
        }

        private Dictionary<string, List<string>> GetSiteCollectionGroupRunningTeamsArchiverJobSiteUrl(RMJobMonitor job, ArchiveJobMonitorExtension jobExtension, bool needLoadSCUrl, Dictionary<string, List<string>> filterTeamAncSCDic)
        {
            Dictionary<string, List<string>> res = new Dictionary<string, List<string>>();

            if(job.JobType == (int)JobType.RMEndUserArchiverBackup)
            {
                List<string> needCheckSCS = filterTeamAncSCDic.Values.SelectMany(scs => scs ?? new List<string>()).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                Dictionary<string, List<string>> scAndFileMapping = jobExtension.SiteUrls.ToDictionary(url => url, url => new List<string>());
                foreach (string fileUrl in jobExtension.ProcessNodeUrls)
                {
                    foreach (string sc in scAndFileMapping.Keys.OrderDescending())
                    {
                        if (RuleSPTreeUtil.IsPrefixWithSlash(sc, fileUrl))
                        {
                            scAndFileMapping[sc].Add(fileUrl);
                        }
                    }
                }
                scAndFileMapping = scAndFileMapping
                    .Where(kv => needCheckSCS.Any(sc => sc.Equals(kv.Key, StringComparison.OrdinalIgnoreCase)))
                    .ToDictionary(kv => kv.Key, kv => kv.Value);
                res[Guid.NewGuid().ToString()] = scAndFileMapping.SelectMany(sc => sc.Value).ToList();
            }
            else if (jobExtension.SiteUrls != null)
            {
                bool runJobUnderSiteCollection = jobExtension.SiteUrls.Count == 1 && RuleSPTreeUtil.IsPrefixWithSlash(jobExtension.SiteUrls.First(), job.ScopeId);
                List<string> needCheckSCS = filterTeamAncSCDic.Values.SelectMany(scs => scs ?? new List<string>()).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                jobExtension.SiteUrls = jobExtension.SiteUrls
                    .Where(sc => needCheckSCS.Any(needCheckSC => needCheckSC.Equals(sc, StringComparison.OrdinalIgnoreCase))).ToList();
                if(runJobUnderSiteCollection && jobExtension.SiteUrls.Any())
                {
                    res[Guid.NewGuid().ToString()] = [job.ScopeId];
                }
                else
                {
                    res[Guid.NewGuid().ToString()] = jobExtension.SiteUrls;
                }                
            }

            return res;
        }

        private Dictionary<string, List<string>> GetArchiverImportTeamsGroupRunningTeamsArchiverJobSiteUrl(RMJobMonitor job, ArchiveJobMonitorExtension jobExtension, bool needLoadSCUrl, Dictionary<string, List<string>> filterTeamAncSCDic)
        {
            Dictionary<string, List<string>> res = new Dictionary<string, List<string>>();
            if (jobExtension.teamsUrls != null && jobExtension.teamsUrls.Count > 0)
            {
                if (filterTeamAncSCDic.Keys.ToArray().Any())
                {
                    Func<string, bool> predicate = teamName => filterTeamAncSCDic.Keys.Any(key => string.Equals(key, teamName, StringComparison.OrdinalIgnoreCase));
                    res = jobExtension.teamsUrls.Where(predicate).ToDictionary(t => t, t => new List<string>());
                }
                else
                {
                    res = jobExtension.teamsUrls.ToDictionary(t => t, t => new List<string>());
                }
            }
            if (needLoadSCUrl && !res.Keys.IsNullOrEmpty())
            {
                LoadSCInfo(res, jobExtension, job, filterTeamAncSCDic);
            }
            return res;
        }

        public bool HasStoppingArchiverJobOnScope(List<JobType> types, string scope)
        {
            return JMDao.HasStoppingArchiverJobOnScope(types, scope);
        }
        #endregion
        #region

        public async Task<ArchiverExportJobDetailInfo> RecenterJobDetailsAsync(string jobId)
        {
            try
            {
                ArchiverExportJobDetailInfo result = new ArchiverExportJobDetailInfo() { Details=new List<ArchiverExportJobDetail>()};
                RA.Contract.JobMonitor.JobType tempJobType;
                if (jobId.StartsWith("RS"))
                {
                    tempJobType = RA.Contract.JobMonitor.JobType.ArchiverRestore;
                }
                else if (jobId.StartsWith("TRS"))
                {
                    tempJobType = RA.Contract.JobMonitor.JobType.TeamsArchiverRestore;
                }
                else if (jobId.StartsWith("OTRS"))
                {
                    tempJobType = RA.Contract.JobMonitor.JobType.TeamsOutPlaceRestore;
                }
                else if (jobId.StartsWith("TSRS"))
                {
                    tempJobType = RA.Contract.JobMonitor.JobType.ArchiverToSpoRestore;
                }
                else
                {
                    tempJobType = RA.Contract.JobMonitor.JobType.ArchiverOutPlaceRestore;
                }
                JMDetailsQuery queryModel = new JMDetailsQuery() { JobID = jobId, JobType = (int)tempJobType, PageSize = -1, CurrentPage = 1, StatusFilters = new JobDetailsStatus[0], ActionTabFilters = new ActionTab[0] };
                JMDetailsResultForApi tempResult = JsonConvert.DeserializeObject<JMDetailsResultForApi>(await GetJobDetailsAsync(queryModel));
                if (tempResult==null || tempResult.Details == null)
                {
                    return result;
                }
                RestoreSettingAndTree jobExtension = null;
                try
                {
                    var job = JMDao.GetJob(jobId);
                    if (!string.IsNullOrWhiteSpace(job.Extension))
                    {
                        jobExtension = SerializerHelper.DeserializeByJsonConvert<RestoreSettingAndTree>(job.Extension);
                    }
                }
                catch (Exception e)
                {
                    logger.Error("Get jobExtension Failed, Error. {0}", e.ToString());
                }
                foreach (var temp in tempResult.Details)
                {
                    if (string.IsNullOrEmpty(result.SiteCollectionURL) && temp.Level == "Site Collection")
                    {
                        result.SiteCollectionURL = temp.SourceLocation;
                    }                    
                    if (temp.Level == "Item")
                    {
                        var tempDetail = ConvertToExportjobDetail(temp);
                        if (tempDetail != null && tempJobType == RA.Contract.JobMonitor.JobType.ArchiverOutPlaceRestore)
                        {
                            tempDetail.DestinationPath = string.Empty;
                        }
                        if (jobExtension != null && jobExtension.Setting.RestoreOption == RestoreOption.Append)
                        {
                            tempDetail.FullPath = temp.DestinationUrl;
                        }
                        result.Details.Add(tempDetail);
                    }                   
                }
                return result;
            }
            catch (Exception e)
            {
                var errorMessage = $"Get recenter job details failed.{e}";
                logger.Error(errorMessage);
                return new ArchiverExportJobDetailInfo() { ErrorCode = ErrorCode.UnExpectedException, ErrorMessage = errorMessage };
            }
        }
        private ArchiverExportJobDetail ConvertToExportjobDetail(JMRestoreActionJobDetailes detail)
        {
            ArchiverExportJobDetail result = new ArchiverExportJobDetail();
            string tempPath = string.Empty;
            tempPath = detail.Path.Substring(detail.Path.LastIndexOfAny(new char[] { '/', '\\' }) + 1);
            int colenIndex = tempPath.IndexOf(':');
            if (colenIndex > 0)
            {
                tempPath = tempPath.Substring(0, tempPath.IndexOf(':'));
            }
            string tempName = detail.SourceLocation.Substring(detail.SourceLocation.LastIndexOfAny(new char[] { '/', '\\' }) + 1);
            int versionindex = tempName.LastIndexOf(':') + 1;
            result.FullPath = TrimPath(detail.Path); 
            result.Status = (DocAveOnline.WebApi.Contracts.JobReportDetailStatus)(int)detail.Status;
            result.Name = tempName.Contains(":")? tempName.Substring(0, tempName.LastIndexOf(':')): tempName;
            result.DestinationPath = TrimPath(detail.SourceLocation);
            if (string.IsNullOrEmpty(result.FullPath))
            {
                result.FullPath = result.DestinationPath;
                result.DestinationPath = string.Empty;
            }
            if (versionindex > 0)
            {
                result.Version = tempName.Substring(versionindex, tempName.Length - versionindex);
            }
            return result;
        }
        private string TrimPath(string oldPath)
        {
            var targetChar = ':';
            string newPath = oldPath;
            if (oldPath.Count(c => c == targetChar) > 1)
            {
                newPath = oldPath.Substring(0, oldPath.LastIndexOf(targetChar));
            }
            return newPath;
        }
        #endregion

        public async System.Threading.Tasks.Task ClearOldArchiverJobsAsync()
        {
            var results = await JMDao.ClearOldArchiverJobsAsync();
            logger.Info($"{results} old archiver main jobs deleted.");

            results = await ArhciverJobDao.ClearOldArchiverJobsAsync();
            logger.Info($"{results} old archiver jobs deleted.");
        }

        public System.Threading.Tasks.Task BulkMigrateJobsAsync(IEnumerable<ArchiverMigrationJobDto> jobs)
        {
            return JMDao.BulkMigrateJobsAsync(jobs);
        }

        public System.Threading.Tasks.Task BulkMigrateArchiverJobs(IEnumerable<ArchiverMigrationJobDto> jobs)
        {
            return ArhciverJobDao.BulkMigrateJobsAsync(jobs);
        }

        public Task<int> UpdateMigratedJobsInfoAsync()
        {
            return ArhciverJobDao.UpdateMigratedJobsInfoAsync();
        }

        public Task<int> DeleteMigratedArchiverJobsAsync()
        {
            return ArhciverJobDao.DeleteMigratedArchiverJobsAsync();
        }

        public Task<int> DeleteMigratedMainJobsAsync()
        {
            return ArhciverJobDao.DeleteMigratedMainJobsAsync();
        }

        public string GetMigrationJobReportExcelBlobName(string jobId)
        {
            var tenantId = TenantLocalValue.LogonGroupId;
            return $"MigrationJobReport/{tenantId}/{jobId}.zip";
        }

        public async System.Threading.Tasks.Task UploadMigrationJobReportToStorageBlob(string jobId)
        {
            try
            {
                var job = await GetJobAsync(jobId);
                var baseJobDto = new BaseJobDto();
                baseJobDto.Id = jobId;
                baseJobDto.JobType = job.JobTypeCode;
                IJobMonitorDetailDownloadWorker jobMonitorDetailDownloadWorker = null;
                jobMonitorDetailDownloadWorker = (IJobMonitorDetailDownloadWorker)PlatformWindsorManager.GetService(typeof(IJobMonitorDetailDownloadWorker));
                string baseFolder = JobReportUtility.GetDownloadJobMonitorDetailTempleFolder(Guid.NewGuid().ToString());
                logger.Info("base folder: {0}", baseFolder);
                await jobMonitorDetailDownloadWorker.GenerateSingleAsync(baseFolder, baseJobDto);
                var zipPath = baseFolder + JobMonitorConstants.ZIP;
                ZipUtil.ZipFolder(baseFolder, zipPath, Encoding.UTF8);

                var sharedStorage = CommonUtilityForSpecialTenant.GetStorageConnectionStringFromConfigFile(StorageStringType.SharedStorage);
                var sharedStorageContainer = RMGlobalConfiguration.StorageConfig[Contract.Configurations.RMStorageSettingKey.SHARED_STORAGE_CONTAINER_NAME];
                if (!string.IsNullOrEmpty(sharedStorage) && !string.IsNullOrEmpty(sharedStorageContainer) && File.Exists(zipPath))
                {
                    var blobName = GetMigrationJobReportExcelBlobName(jobId);
                    AzureUtil.UploadStorageBlob(sharedStorage, sharedStorageContainer, blobName, zipPath);
                    logger.Info($"finish to upload blob name:{blobName}");
                }
                if(File.Exists(zipPath))
                {
                    File.Delete(zipPath);
                }
            }
            catch (Exception ex)
            {
                logger.Error("job failed, error:{0}", ex.ToString());
                throw;
            }
        }


        public async Task<string> RealRunDownloadJobReportJobForCOP(string param)
        {
            logger.Info("Start RealRunDownloadJobReportJobForCOP");
            var jobId = string.Empty;
            try
            {
                var username = TenantLocalValue.LogonUserEmail;
                var account = await AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail);
                var userId = Guid.Empty.ToString();
                if (account == null)
                {
                    logger.Info($"COP Account is null, email:{TenantLocalValue.LogonUserEmail}, will use application admin");

                    var admin = RetryUtility.RetryAlways(
                    () => UserService.GetApplicationAdminsAsync().GetAwaiter().GetResult()?.FirstOrDefault(),
                    3
                );

                    if (admin != null)
                    {
                        logger.Info($"Application admin is: {admin.UserId}");
                        TenantLocalValue.LogonUserId = admin.UserId;
                        TenantLocalValue.LogonUserEmail = admin.UserPrincipalName;
                        username = admin.UserPrincipalName;
                        userId = TenantLocalValue.LogonUserId;
                    }
                    else
                    {
                        throw new Exception("Can't get application admin user.");
                    }
                }
                else
                {
                    userId = account.UserId;
                }
                jobId = CreateJob(JobType.DownloadJobReportsForCOP, username, userId);
                DownloadDataInfoDao.Create(new RMDownloadDataInfo()
                {
                    FileDownloadTime = DateTime.UtcNow.Ticks,
                    JobId = jobId,
                    RecordsId = Guid.NewGuid(),
                    JobStatus = (int)DownloadContentJobStatus.Wait,
                    UserId = userId,
                    Name = jobId + ".zip",
                    // Use dedicated COP type so we can hide these internal jobs from end-user job monitor
                    DownloadType = DownloadContentType.JobReportContentForCOP,
                    ExtendString1 = string.Join("; ", RA.Common.Global.Utils.SerializerHelper.DeserializeByDataContractSerializer<List<string>>(param)),
                });
                JobQueueService.HandleMessage(new JobQueueMessage
                {
                    JobId = jobId,
                    JobType = JobType.DownloadJobReportsForCOP,
                    CommandLine = $"{JobType.DownloadJobReportsForCOP} {jobId}",
                    Extension = param,
                });

            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while real run download job report job. Error: {e}");
                if (!string.IsNullOrEmpty(jobId))
                {
                    UpdateJobStatus(jobId, JobStatus.Failed, e.Message);
                }
            }

            return jobId;
        }

        [Audit(Module = AuditModule.JobMonitor, Category = AuditCategory.JobMonitor, Action = AuditAction.RunDownloadJobDetailsJob, AfterHandler = typeof(JobMonitorServiceAuditHandler))]
        public async Task<string> RealRunDownloadJobReportJob(string param)
        {
            logger.Info("Start run download job report job");
            var jobId = string.Empty;
            try
            {
                var username = TenantLocalValue.LogonUserEmail;
                var account = await AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail);
                jobId = CreateJob(JobType.DownloadJobReports, username, account.UserId);
                DownloadDataInfoDao.Create(new RMDownloadDataInfo()
                {
                    FileDownloadTime = DateTime.UtcNow.Ticks,
                    JobId = jobId,
                    RecordsId = Guid.NewGuid(),
                    JobStatus = (int)DownloadContentJobStatus.Wait,
                    UserId = account.UserId,
                    Name = jobId + ".zip",
                    DownloadType = DownloadContentType.JobReportContent,
                    ExtendString1 = string.Join("; ", RA.Common.Global.Utils.SerializerHelper.DeserializeByDataContractSerializer<List<string>>(param)),
                });
                JobQueueService.HandleMessage(new JobQueueMessage
                {
                    JobId = jobId,
                    JobType = JobType.DownloadJobReports,
                    CommandLine = $"{JobType.DownloadJobReports} {jobId}",
                    Extension = param,
                });

            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while real run download job report job. Error: {e}");
                if (!string.IsNullOrEmpty(jobId))
                {
                    UpdateJobStatus(jobId, JobStatus.Failed, e.Message);
                }
            }

            return jobId;
        }

        [Audit(Module = AuditModule.JobMonitor, Category = AuditCategory.JobMonitor, Action = AuditAction.UpdateJobMonitorPriority, AfterHandler = typeof(JobMonitorServiceAuditHandler), BeforeHandler = typeof(JobMonitorServiceBeforeAuditHandler))]
        public async Task<bool> UpdateJobPriorityAsync(List<string> jobIds, JobPriority jobPriority)
        {
            try
            {
                return await JMDao.UpdateJobPriorityAsync(jobIds, jobPriority);
            }
            catch (Exception ex)
            {
                logger.Error("UpdateJobPriorityAsync error:{0}", ex.ToString());
                return false;
            }
        }

        public Task<bool> UpdateJobVersionAsync(string jobId, JobVersion jobVersion)
        {
            return JMDao.UpdateJobVersion(jobId, jobVersion);
        }

        public HSMArchiverJobInfo GetHSMArchiverJobInfo(string location)
        {
            var lastestJob = JMDao.GetLastestJobByLocation(location);
            var result = new HSMArchiverJobInfo()
            {
                HasHSMArchiverJob = false,
                SubJobInfos = new List<HSMArchiverSubJobInfo>()
            };

            if (lastestJob == null)
            {
                return result;
            }

            result.HasHSMArchiverJob = true;
            result.JobId = lastestJob.Id;
            result.JobStatus = lastestJob.Status;
            result.LastUpdateTime = lastestJob.LastUpdateTime;
            var subJobs = SubJobDao.GetArchiverSubJobsByParentId(lastestJob.Id);
            result.SubJobInfos = subJobs;
            return result;
        }

        private async Task<JMProgressStatisticDetails> BuildProgressDetailJobAsync(string mainJobId, int totalNumber)
        {
            var job = JMDao.GetJob(mainJobId);
            GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
            if (job == null)
            {
                return null;
            }
            string processedSites = string.Empty;
            string processedFiles = string.Empty;
            FillProcessedSitesAndFiles(job, totalNumber, ref processedSites, ref processedFiles, out int totalSCCount, out int processFiledCount, out int processedSCCount);

            // Aggregate archived size and ETA from RMJobProgresses
            long totalArchivedSize = 0;
            long estimatedFinishTimeTicks = 0;
            bool isNewJob = false;
            try
            {
                if (!string.IsNullOrWhiteSpace(job.Extension))
                {
                    var jobExtension = SerializerHelper.DeserializeByJsonConvert<JobExtension>(job.Extension);
                    totalArchivedSize = jobExtension?.SOProgressFileAndSCCount.TotalArchivedSize ?? 0;
                    estimatedFinishTimeTicks = jobExtension?.SOProgressFileAndSCCount.EstimatedFinishTimeTicks ?? 0;    
                    isNewJob = jobExtension?.SOProgressFileAndSCCount?.IsNewJob ?? false;
                    logger.Warn($"BuildProgressDetailJobAsync: JobExtension.IsNewJob: {jobExtension?.SOProgressFileAndSCCount?.IsNewJob}");
                }
            }
            catch (Exception ex)
            {
                logger.Warn($"BuildProgressDetailJobAsync: Failed to read TotalArchivedSize from extension. {ex.Message}");
                totalArchivedSize = 0;
                estimatedFinishTimeTicks = 0;
            }

            return new JMProgressStatisticDetails
            {
                IsNewJob = isNewJob,
                ProcessedSites = processedSites,
                ProcessedFiles = processedFiles,
                ProcessedSize = FormatBytes(totalArchivedSize),
                EstimatedFinishTime = estimatedFinishTimeTicks > 0
                        ? GeneralSettingService.ConvertTiksToDateTime(gls, estimatedFinishTimeTicks, true).SimplifyFormatTime
                        : string.Empty,
                LastUpdateTime = job.LastUpdateTime > 0
                        ? GeneralSettingService.ConvertTiksToDateTime(gls, job.LastUpdateTime, true).SimplifyFormatTime
                        : string.Empty,
            };
        }

        private void FillProcessedSitesAndFiles(RMJobMonitor job, int totalSubJobNumber, ref string processedSites, ref string processedFiles, out int allSCCount, out int processedFilesCount, out int processedSCCount)
        {
            processedSCCount = 0;
            processedFilesCount = 0;
            allSCCount = 0;
            if (string.IsNullOrWhiteSpace(job?.Extension))
            {
                return;
            }

            try
            {
                var jobExtension = SerializerHelper.DeserializeByJsonConvert<JobExtension>(job.Extension);

                if (jobExtension?.SOProgressFileAndSCCount != null)
                {
                    allSCCount = jobExtension.SOProgressFileAndSCCount.AllSCCount;
                    var scCount = jobExtension.SOProgressFileAndSCCount;
                    processedSites = $"{scCount.ProgressedSCCount}/{allSCCount}";
                    processedFiles = scCount.ProgressedFileCount.ToString();
                    processedSCCount = scCount.ProgressedSCCount;
                    processedFilesCount = scCount.ProgressedFileCount;
                }
            }
            catch (Exception ex)
            {
                logger.Warn($"Cannot parse extension from job mornitor. jobId: {job.Id}");
            }
        }    

        private string FormatBytes(long bytes)
        {
            if(bytes > 0)
            {
                const long kb = 1024, mb = kb * 1024, gb = mb * 1024;
                if (bytes < kb) return $"{bytes} B";
                if (bytes < mb) return $"{bytes / (double)kb:F3} KB";
                if (bytes < gb) return $"{bytes / (double)mb:F3} MB";
                return $"{bytes / (double)gb:F3} GB";
            }
            return string.Empty;
        }

        public JobMonitorStatisticsDto GetJobMonitorStatisDto(string mainJobId)
        {
            JobMonitorStatisticsDto rs = new JobMonitorStatisticsDto();
            try
            {
                var job = JMDao.GetJob(mainJobId);
                rs.StartTime = job.StartTime;
                rs.FinishTime = job.EndTime;
                rs.JobType = job.JobType;
            }
            catch (Exception ex)
            {
                logger.Error($"GetJobMonitorStatisDto error:{ex.ToString()}");
                return null;
            }
            return rs;
        }
    }
}
