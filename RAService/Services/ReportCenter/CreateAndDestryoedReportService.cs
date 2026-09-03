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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Web;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.JPMC;
using AvePoint.RA.Contract.ReportCenter;
using AvePoint.RA.Contract.ReportCenter.Model;
using AvePoint.RA.Contract.RMReport;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.RACommonUtility.Common;
using AvePoint.RA.Service.Services.ReportCenter.Adapter;
using AvePoint.RA.Service.Services.RMReport.AuditHandler;
using AvePoint.RA.SharePoint.Common;
using AvePoint.Wrapper.Common;
using Newtonsoft.Json;

namespace AvePoint.RA.Service.Services.ReportCenter
{
    [Audit]
    public class CreateAndDestryoedReportService : RMServiceBase, ICreateAndDestryoedReportService
    {

        private static readonly RALogger Logger = RALogger.GetInstance(typeof(CreateAndDestryoedReportService));

        private ICreateAndDestryoedReportDao CreateAndDestryoedReportDao => PlatformWindsorManager.GetService<ICreateAndDestryoedReportDao>();

        private IGeneralSettingService  GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService >();

        private IJobQueueService JobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();

        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();
        private IRMSubJobDao SubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();
        private IDownloadDataInfoDao DownloadDataInfoDao => PlatformWindsorManager.GetService<IDownloadDataInfoDao>();
        private IRMKeyValueDao KeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        
        IRMRemoteNodeDao mRemoteNodeDao = null;
        public IRMRemoteNodeDao RemoteNodeDao
        {
            get
            {
                mRemoteNodeDao ??= (IRMRemoteNodeDao)PlatformWindsorManager.GetService(typeof(IRMRemoteNodeDao));
                return mRemoteNodeDao;
            }
        }
        private IAccountDao mAccountDao = null;
        public IAccountDao AccountDao
        {
            get
            {
                mAccountDao ??= (IAccountDao)PlatformWindsorManager.GetService(typeof(IAccountDao));
                return mAccountDao;
            }
        }
        private IRMSecurityTrimmingHelper mSecurityTrimmingHelper = null;
        public IRMSecurityTrimmingHelper SecurityTrimmingHelper
        {
            get
            {
                mSecurityTrimmingHelper ??= (IRMSecurityTrimmingHelper)PlatformWindsorManager.GetService(typeof(IRMSecurityTrimmingHelper));
                return mSecurityTrimmingHelper;
            }
        }
        private IRMScopeRoleAssignmentDao mRMScopeRoleAssignmentDao = null;
        public IRMScopeRoleAssignmentDao RMScopeRoleAssignmentDao
        {
            get
            {
                mRMScopeRoleAssignmentDao ??= (IRMScopeRoleAssignmentDao)PlatformWindsorManager.GetService(typeof(IRMScopeRoleAssignmentDao));
                return mRMScopeRoleAssignmentDao;
            }
        }
        private IUserService mUserService = null;
        public IUserService UserService
        {
            get
            {
                mUserService ??= (IUserService)PlatformWindsorManager.GetService(typeof(IUserService));
                return mUserService;
            }
        }

        public async Task<bool> Create(CreateAndDestryoedReportModel reportInfo)
        {
            try
            {
                reportInfo.CustomStartDate = await GeneralSettingService.ConvertToUTCDateTimeAsync(reportInfo.CustomStartDate);
                reportInfo.CustomEndDate = await GeneralSettingService.ConvertToUTCDateTimeAsync(reportInfo.CustomEndDate);
                var profileModel = CreateAndDestryoedReportAdapter.ConvertToDbModel(reportInfo);
                profileModel.Type = (int)JobType.CreateAndDestroyedReport;
                return await CreateAndDestryoedReportDao.Create(profileModel);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while create create and destryoed report. Error: {e}");
                return false;
            }
        }

        public async Task<CreateAndDestryoedReportModel> Get(int id)
        {
            try
            {
                var profileModel = await CreateAndDestryoedReportDao.Get(id);
                var result = CreateAndDestryoedReportAdapter.ConvertToReportModel(profileModel);
                result.CustomStartDate = await GeneralSettingService.ConvertFromUTCDateTimeAsync(result.CustomStartDate);
                result.CustomEndDate = await GeneralSettingService.ConvertFromUTCDateTimeAsync(result.CustomEndDate);
                return result;
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while get create and destryoed report by: [{id}]. Error: {e}");
                return null;
            }
        }

        public async Task<bool> Edit(CreateAndDestryoedReportModel reportInfo)
        {
            try
            {
                reportInfo.CustomStartDate = await GeneralSettingService.ConvertToUTCDateTimeAsync(reportInfo.CustomStartDate);
                reportInfo.CustomEndDate = await GeneralSettingService.ConvertToUTCDateTimeAsync(reportInfo.CustomEndDate);
                var profileModel = CreateAndDestryoedReportAdapter.ConvertToDbModel(reportInfo);
                return await CreateAndDestryoedReportDao.Edit(profileModel);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while edit create and destryoed report by: [{reportInfo.Id}]. Error: {e}");
                return false;
            }
        }

        public bool GenerateReportJob(int id)
        {
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                var jqDto = new JobQueueDto()
                {
                    JobType = JobType.CreateAndDestroyedReport,
                    Parameters = id.ToString(),
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName
                };
                var jobId = JobQueueService.AddToDBJobQueue(jqDto);
                return !string.IsNullOrEmpty(jobId);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while generate report job. Error: {e}");
                return false;
            }
        }

        public string RealRunReportJob(int id)
        {
            Logger.Info("Start run disposal report job.");
            var jobId = string.Empty;

            try
            {
                var username = TenantLocalValue.LogonUserEmail;
                jobId = JobMonitorService.CreateJobWithProfileId(JobType.CreateAndDestroyedReport, username, id);
                JobQueueService.HandleMessage(new JobQueueMessage
                {
                    JobId = jobId,
                    JobType = JobType.CreateAndDestroyedReport,
                    CommandLine = $"{JobType.CreateAndDestroyedReport} {jobId} {id}",
                });
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while real run create and destryoed report job. Error: {e}");
            }

            return jobId;
        }

        public bool RunSiteMetricsReportJob()
        {
            try
            {
                List<JPMCTenantConfig> configs = [];
                var jsonConfig = KeyValueDao.GetValueByKey("JPMC_Customization");
                if (jsonConfig != null)
                {
                    try
                    {
                        configs = JsonConvert.DeserializeObject<List<JPMCTenantConfig>>(jsonConfig.Value);
                    }
                    catch (Exception e)
                    {
                        Logger.Warn($"Deserialize object error {e}");
                    }
                }

                if (configs?.Count == 0)
                {
                    return false;
                }

                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                var logonUserId = TenantLocalValue.LogonUserId;
                var jqDto = new JobQueueDto()
                {
                    JobType = JobType.ExportSiteMetrics,
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                    Parameters = logonUserId
                };
                var jobId = JobQueueService.AddToDBJobQueue(jqDto);
                return !string.IsNullOrEmpty(jobId);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while generate report job. Error: {e}");
                return false;
            }
        }

        public async Task<bool> RunSiteMetricsReportJob(SiteMetricsJobParameterDto siteMetricsReportParameters)
        {
            try
            {
                List<JPMCTenantConfig> configs = [];
                var jsonConfig = KeyValueDao.GetValueByKey("JPMC_Customization");
                if (jsonConfig != null)
                {
                    try
                    {
                        configs = JsonConvert.DeserializeObject<List<JPMCTenantConfig>>(jsonConfig.Value);
                    }
                    catch (Exception e)
                    {
                        Logger.Warn($"Deserialize object error {e}");
                    }
                }

                if (configs?.Count == 0)
                {
                    return false;
                }

                var groupId = TenantLocalValue.LogonGroupId;

                if (string.IsNullOrEmpty(TenantLocalValue.LogonUserEmail) || string.IsNullOrEmpty(TenantLocalValue.LogonUserId))
                {
                    var accounts = AccountDao.GetAppAdminAccounts().First();
                    TenantLocalValue.LogonUserEmail = accounts.UserPrincipalName;
                    TenantLocalValue.LogonUserId = accounts.UserId;
                }

                //var jqDto = new JobQueueDto()
                //{
                //    JobType = JobType.ExportSiteMetrics,
                //    //JobRunType = JobRunBy.Control,
                //    TenantGroupId = groupId,
                //    JobRunByUser = loginName,
                //    Parameters = SerializerHelper.SerializeByDataContractSerializer(siteMetricsReportParameters)
                //};
                //var jobId = JobQueueService.AddToDBJobQueue(jqDto);

                var parameters = SerializerHelper.SerializeByDataContractSerializer(siteMetricsReportParameters);
                var jobId = await RealRunGenerateSiteMetricsReportJobAsync(JobType.ExportSiteMetrics, TenantLocalValue.LogonUserEmail, parameters);

                return !string.IsNullOrEmpty(jobId);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while generate report job. Error: {e}");
                return false;
            }
        }

        [Audit(Module = AuditModule.ReportCenter, Category = AuditCategory.Explorer, Action = AuditAction.GenerateReport, AfterHandler = typeof(TermUsageOrDueForDisposalAfterAuditHandler))]
        public async Task<string> RealRunGenerateSiteMetricsReportJobAsync(JobType jobType, string jobRunByUser, string parameters)
        {
            var jobId = JobMonitorService.CreateJob(JobType.ExportSiteMetrics, jobRunByUser);
            var availableNode = RABrowserClient.GetAuthorisedRemoteSiteCollectionsByUser();
            List<JPMCTenantConfig> jpmcTenantConfigs = null;
            jpmcTenantConfigs = GetJpmcTenantConfigs(availableNode);

            availableNode = availableNode
                            .Where(sc => sc.NodeType != RemoveNodeType.SkyDrivePro
                                        && jpmcTenantConfigs.Any(c => c.M365TenantId == sc.TenantId)).ToList();
            Logger.Info($"Will process site collection count is {availableNode.Count}");
#if DEBUG
            //availableNode = availableNode.Where(site => site.url.Contains("jpmc_team01") || site.url.Contains("team02")).ToList();
#endif

            if (availableNode.IsNullOrEmpty())
            {
                Logger.Warn("No available sc to run");
                JobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_JM_Report_Skip_NoAvailableSites");
                return jobId;
            }
            //List<JobType> indexJobTypes = JobTypeConstants.ArchiverIndexWriteConflictJobTypes;
            var runningJobs = JobMonitorService.GetRunningJobs(JobType.ExportSiteMetrics);
            runningJobs.Remove(jobId);
            if (runningJobs.Count > 0)
            {
                Logger.Warn($"Current has job running.Running Job Id:{string.Join(",", runningJobs)}.");
                JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");

                //var downCenterInfo = DownloadDataInfoDao.GetDownloadDataInfosByStatus([(int)DownloadContentJobStatus.Wait, (int)DownloadContentJobStatus.InProgress]).FirstOrDefault(item => item.JobId == jobId);
                //if (downCenterInfo != null)
                //{
                //    downCenterInfo.JobStatus = (int)DownloadContentJobStatus.Skipped;
                //    DownloadDataInfoDao.UpdateDownloadInfo(downCenterInfo);
                //}
                return jobId;
            }

            var logonUserId = parameters;
            var libraryFullPath = string.Empty;
            SiteMetricsJobParameterDto siteMetricParams = null;
            try
            {
                siteMetricParams = SerializerHelper.DeserializeByDataContractSerializer<SiteMetricsJobParameterDto>(parameters);
                if (siteMetricParams != null)
                {
                    libraryFullPath = await GetFullPathByPathAndSiteUrl(siteMetricParams.WebUrl, siteMetricParams.LibraryRelativePath);
                    if (string.IsNullOrEmpty(libraryFullPath))
                    {
                        JobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, "StorageOptimization13_SOARSORecordManagerLibraryNotExist");
                        return jobId;
                    }
                    Logger.Info($"This job need to export report to library: {libraryFullPath}");
                    logonUserId = TenantLocalValue.LogonUserId;
                }
            }
            catch (SerializationException se)
            {
                if (!Guid.TryParse(parameters, out _))
                {
                    Logger.Error($"Paramenters is not Guid Id or SiteMetricsJobParameterDto: {parameters}. Error: {se}");
                }
                Logger.Info($"Paramenters is logonUserId.");
            }
            catch (Exception ex)
            {
                Logger.Error($"An error occurred while deserializing parameters. Error: {ex}");
            }

            DownloadDataInfoDao.Create(new RMDownloadDataInfo()
            {
                FileDownloadTime = DateTime.UtcNow.Ticks,
                JobId = jobId,
                RecordsId = Guid.NewGuid(),
                JobStatus = (int)DownloadContentJobStatus.Wait,
                UserId = logonUserId,
                Name = jobId + ".zip",
                DownloadType = DownloadContentType.ExportSiteMetrics
            });

            var timeZoneId = (await GeneralSettingService.GetGeneralSettingAsync()).TimeZoneId;
            var timeZone = GeneralSettingConfig.FindSystemTimeZoneById(timeZoneId);

            var nowUTC = DateTime.UtcNow;
            var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(nowUTC, timeZone);

            var todayStartTimeLocal = new DateTime(nowLocal.Year, nowLocal.Month, nowLocal.Day, 0, 0, 0, DateTimeKind.Unspecified);
            var todayStartTimeUTC = TimeZoneInfo.ConvertTimeToUtc(todayStartTimeLocal, timeZone);

            //var todayEndTimeLocal = new DateTime(nowLocal.Year, nowLocal.Month, nowLocal.Day, 23, 59, 59, DateTimeKind.Unspecified);
            //var todayEndTimeUTC = TimeZoneInfo.ConvertTimeToUtc(todayEndTimeLocal, timeZone);
            var todayEndTimeUTC = nowUTC;

            Logger.Info($"Today time is: {todayStartTimeUTC} - {todayEndTimeUTC}");

            var param = new SiteMetricsJobParameterDto()
            {
                StartTime = todayStartTimeUTC,
                EndTime = todayEndTimeUTC
            };

            if (siteMetricParams != null)
            {
                param.WebUrl = siteMetricParams.WebUrl;
                param.LibraryRelativePath = siteMetricParams.LibraryRelativePath;
                //param.LibraryFullPath = libraryFullPath;
            }
            int subJobCount = availableNode.Count;

            SubJobDao.UpdateSubJobCount(jobId, subJobCount);

            int currentSubjobIndex = 0;
            foreach (var site in availableNode)
            {
                string subJobId = CreateSubJobForSiteMetrics(jobId, param, currentSubjobIndex++, jobType, subJobCount, site, false, site.url, site.TenantId);
            }
            return jobId;
        }

        private async Task<string> GetFullPathByPathAndSiteUrl(string webUrl, string libRelativePath)
        {
            if (string.IsNullOrWhiteSpace(webUrl) || string.IsNullOrWhiteSpace(libRelativePath))
            {
                Logger.Error("webUrl or libRelativePath is null or empty");
                return null;
            }

            webUrl = HttpUtility.UrlDecode(webUrl);
            libRelativePath = HttpUtility.UrlDecode(libRelativePath);
            try
            {
                Logger.Info("Start check location url for ra.");
                Stopwatch watch = new Stopwatch();
                watch.Start();
                RemoteSiteCollection site = RABrowserClient.GetRemoteSiteCollectionByListUrl(webUrl);
                Logger.Info($"Site is null: {site == null}");
                if (site == null)
                {
                    Logger.Error($"Can not find site by webUrl: {webUrl}");
                    return null;
                }
                //checkObject.ContainerId = site.parentId;
                Guid teamsContainerId = Guid.Empty;
                bool isTeamsNode = false;
                if(KeyValueDao.HasUpgradeTeams() && !site.TeamId.IsNullOrEmpty())
                {
                    var (teamsNode, listSiteNode) = RemoteNodeDao.GetTeamsGroupAndChannelsCollectionByTeamsId(site.TeamId);
                    Logger.Info($"teams is null: {teamsNode == null}");
                    if (teamsNode == null) return null;
                    //checkObject.ContainerId = teamsNode.parentId;
                    teamsContainerId = new Guid(teamsNode.parentId);
                    isTeamsNode = true;
                }

                var account = await AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail);
                Logger.Info($"Account is null: {account == null}");
                if (account == null)
                {
                    Logger.Error($"Can not find user by name: {TenantLocalValue.LogonUserEmail}");
                    return null;
                }

                // check permission
                if (!(await IsAdminAsync(account.UserId, site.NodeType)))
                {
                    List<string> userAndGroupUserIds = await UserService.GetUserAndGroupUserIdsAsync(account.UserId);
                    if (!RMScopeRoleAssignmentDao.HavePermissionOnContainerId(isTeamsNode ? teamsContainerId : new Guid(site.parentId), userAndGroupUserIds))
                    {
                        Logger.Info($"Current user doesn't have permission on container. Container Id:{site.parentId}.webUrl:{webUrl}, libRelativePath: {libRelativePath}.");
                        return null;
                    }
                }

                var bposInfo = await PoolUserUtil.GetBPOSInfoAsync(site);
                var mFactory = MultiAppUtil.CreateAveObjectModelFactory(site.url, bposInfo, AveContextKind.ClientObjectModel);
                Logger.Info($"Factory is null: {mFactory == null}");
                var listFullPath = string.Empty;
                using (IAveSite mSite = mFactory.CreateSite(site.url))
                {
                    var (isValid, resultPath) = SPOExportUtility.ValidateWebUrl(mSite, webUrl, libRelativePath, bposInfo, site.id);
                    if (isValid)
                    {
                        listFullPath = resultPath;
                    }
                }
                watch.Stop();
                Logger.Info($"End check location url for webUrl:{webUrl}, libRelativePath: {libRelativePath},Take Milliseconds:{watch.ElapsedMilliseconds} ms.");
                return listFullPath;
            }
            catch (Exception ex)
            {
                Logger.Info($"Failed check location url for webUrl:{webUrl}, libRelativePath: {libRelativePath},error message:{ex.Message}");
            }
            return null;
        }

        private async Task<bool> IsAdminAsync(string userId, RemoveNodeType nodeType)
        {
            bool isAdmin = false;
            if (nodeType == RemoveNodeType.SkyDrivePro)
            {
                isAdmin = await IsOneDriveAdminAsync(userId) || await IsSOOneDriveAdminAsync(userId);
            }
            else
            {
                isAdmin = await IsSPAdminAsync(userId) || await IsSOSPAdminAsync(userId);
            }
            return isAdmin;
        }
        private Task<bool> IsSPAdminAsync(string userId)
        {
            return TenantUtil.RunUnderTenantAsync(
                new TenantContext(TenantLocalValue.LogonGroupId, userId, TenantLocalValue.LogonUserEmail),
                () =>
                {
                    return SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.SPOAdmin);
                });
        }
        private Task<bool> IsOneDriveAdminAsync(string userId)
        {
            return TenantUtil.RunUnderTenantAsync(
                new TenantContext(TenantLocalValue.LogonGroupId, userId, TenantLocalValue.LogonUserEmail),
                () =>
                {
                    return SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.OneDriveAdmin);
                });
        }

        private Task<bool> IsSOSPAdminAsync(string userId)
        {
            return TenantUtil.RunUnderTenantAsync(
                new TenantContext(TenantLocalValue.LogonGroupId, userId, TenantLocalValue.LogonUserEmail),
                () =>
                {
                    return SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMSOPermissionMasks.SPOAdmin);
                });
        }
        private Task<bool> IsSOOneDriveAdminAsync(string userId)
        {
            return TenantUtil.RunUnderTenantAsync(
                new TenantContext(TenantLocalValue.LogonGroupId, userId, TenantLocalValue.LogonUserEmail),
                () =>
                {
                    return SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMSOPermissionMasks.OneDriveAdmin);
                });
        }


        private List<JPMCTenantConfig> GetJpmcTenantConfigs(List<RemoteSiteCollection> availableNode)
        {
            var jsonConfig = KeyValueDao.GetValueByKey("JPMC_Customization");
            List<JPMCTenantConfig> configs = null;
            if (jsonConfig != null)
            {
                configs = JsonConvert.DeserializeObject<List<JPMCTenantConfig>>(jsonConfig.Value);
                configs.ForEach(c =>
                {
                    var remoteSite = availableNode.FirstOrDefault(s => s.url == c.ConfigSiteUrl);
                    if (remoteSite != null)
                    {
                        c.ConfigSite = remoteSite;
                        c.M365TenantId = remoteSite.TenantId;
                    }
                    else
                    {
                        Logger.Warn($"Can not get this site:{c.ConfigSiteUrl}");
                    }
                });
                Logger.Info($"Enable jpmc O365: {string.Join(", ", configs.Select(c => c.ConfigSiteUrl))}");
                return configs;
            }
            return [];
        }

        private string CreateSubJobForSiteMetrics(string jobId, SiteMetricsJobParameterDto param, int currentSubjobIndex, JobType jobType, int subJobCount, RemoteSiteCollection site, bool sendNow, string scope, string o365TenantId)
        {
            string subJobId = string.Format(jobId + "_{0:D3}", currentSubjobIndex);
            var subJob = new RMSubJob() { Id = subJobId, ParentId = jobId, StartTime = DateTime.UtcNow.Ticks, JobType = (int)jobType, Progress = 0, Status = (int)JobStatus.Wait, Weight = 100d / subJobCount, O365TenantId = o365TenantId };
            subJob.Runable = sendNow ? RecordsConstants.SubJob_Runnable_Runing : RecordsConstants.SubJob_Runnable_Waiting;
            subJob.JobContext = new RMJobContext() { JobId = subJobId, Content = SerializerHelper.SerializeByDataContractSerializer(site) , Settings = SerializerHelper.SerializeByDataContractSerializer(param) };
            subJob.String1 = scope;
            SubJobDao.CreateJob(subJob);
            Logger.Info($"Create sub job {subJob.Id} sucessfull, main job: {subJob.ParentId}, type {subJob.JobType}, weight {subJob.Weight}, Path {scope}");
            return subJobId;
        }
    }
}
