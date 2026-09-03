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
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.CommonFilter;
using ReportCenterObject = AvePoint.GCommon.Contract.ReportCenter.Object;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.RMRuleManagement;
using AvePoint.RA.Common.Threads;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.CodeView;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.DocAve.SOArchiver;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Object.Base;
using AvePoint.RA.Contract.RMReport;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.Dashboard;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Permission;
using AvePoint.RA.RADataBroker;
using AvePoint.RA.RAExchange.Report;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.Service.Services.AccountManager;
using AvePoint.RA.Service.Services.RMReport.AuditHandler;
using AvePoint.RA.Service.Services.Dashboard;
using AvePoint.Records.Core.Utilities.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using RACloudFS.Report;
using AvePoint.RA.Contract.RMWeb.SignalR;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.Service.SharePointSetting;
using AvePoint.RA.Service.Services.JobQueue;
using System.Threading.Tasks;
using AvePoint.RA.Service.Services.JobMonitor.Util;
using AvePoint.RA.Service.Services.ControlPanel;
using AvePoint.RA.Contract.Box;
using AvePoint.GCommon.Contract.CommonFilter.Rules;
using AvePoint.RA.Contract;
using ZXing;
using AvePoint.RA.Contract.ReportCenter;
using RAGoogle.Report;
using AvePoint.RA.DB.Dao.GoogleSyncNodeDao.Contract;
using AvePoint.GCommon.Contract.Tree;
using AvePoint.RA.Common.SystemSetting;
using Microsoft.Graph.Drives.Item.Items.Item.Children;
using AvePoint.RA.Contract.Schedule;

namespace AvePoint.RA.Service.Services.RMReport
{
    [Audit]
    public class RMReportService : RMServiceBase, IRMReportService
    {
        #region Interface
        #region private property

        private RALogger logger = RALogger.GetInstance(typeof(RMReportService));
        public Dictionary<int, AbstractReportWorker> baeReportWorkerDictionary { set; get; }
        private readonly static RASimpleLocker _simpleLocker = new RASimpleLocker();
        private string _datetimeFormat;

        private readonly object locker = new object();
        private List<BaseReport> reportsWaiting = new List<BaseReport>();
        private BaseJobDto baseJobDto;
        private int sendStatus = 0;//0 init;  send +1 ; finish -1
        private bool finalUpdate = false;
        private bool IsDynamicSizeDisplay = false;
        #endregion
        private IRMSecurityTrimmingHelper SecurityTrimmingHelper => PlatformWindsorManager.GetService<IRMSecurityTrimmingHelper>();
        private ITermGroupDao TermGroupDao => PlatformWindsorManager.GetService<ITermGroupDao>();
        private ITermSetDao TermSetDAO => PlatformWindsorManager.GetService<ITermSetDao>();
        private ITermDao TermDao => PlatformWindsorManager.GetService<ITermDao>();
        private IProfileDao profileDAO => PlatformWindsorManager.GetService<IProfileDao>();
        private IReportCenterDao ReportCenterDao => PlatformWindsorManager.GetService<IReportCenterDao>();
        private IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();

        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();
        private IRMArchiveSiteInfoDao ArchiveSiteInfoDao => PlatformWindsorManager.GetService<IRMArchiveSiteInfoDao>();
        private ITeamsSettingDao TeamsSettingDao => PlatformWindsorManager.GetService<ITeamsSettingDao>();

        private async Task GenerateArchivedSiteReportAsync(string jobId, JobType jobType)
        {
            try
            {
                var sites = await ArchiveSiteInfoDao.GetArchiverSitesByPagerAsync(1, int.MaxValue);
                var reports = sites.Select(site => new ArchivedSiteReport
                {
                    ObjectLevel = 0,
                    TitleOrName = site.SiteUrl,
                    Url = site.SiteUrl,
                    Type = jobType.ToString(),
                    SourceUrl = site.SiteUrl,
                    ArchivedDataSize = site.ArchivedSize,
                    CreatedTime = DateTime.UtcNow.Ticks,
                    LastModifiedTime = DateTime.UtcNow.Ticks,
                    ArchivedTime = DateTime.UtcNow.Ticks
                }).Cast<BaseReport>();
                var jobInfo = new BaseJobDto { Id = jobId, JobType = (int)jobType };
                SyncReportJobDatas(reports, jobInfo);
                JobMonitorService.UpdateJobStatus(jobId, JobStatus.Finished);
                await StartScheduledArchivedSiteExportAsync(jobId);
            }
            catch (Exception ex)
            {
                logger.Error("Failed to generate Archived Sites report. JobId:{0}, Error:{1}", jobId, ex);
                JobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, ex.Message);
            }
        }

        private async Task StartScheduledArchivedSiteExportAsync(string jobId)
        {
            var job = await JobMonitorService.GetJobAsync(jobId);
            if (job == null || job.ProfileId <= 0 || job.Status != JobStatus.Finished)
            {
                return;
            }

            var profile = await GetProfileByIdAsync(job.ProfileId.ToString());
            if (profile == null || string.IsNullOrWhiteSpace(profile.ScheduleId))
            {
                return;
            }

            var exportModel = new ExportReportCommonModel
            {
                ReportJobType = ((int)profile.Type).ToString(),
                ReportJobId = jobId,
                ProfileName = profile.ProfileName,
                ProfileId = profile.Id.ToString(),
            };
            RunExportReportJob(SerializerHelper.SerializeByJsonConvert(exportModel));
            logger.Info("Started scheduled Archived Sites report export. JobId:{0}, ProfileId:{1}, JobType:{2}", jobId, profile.Id, profile.Type);
        }

        public ISecurityGroupManagementService SecurityGroupManagementService
        {
            get
            {
                return (ISecurityGroupManagementService)PlatformWindsorManager.GetService(typeof(ISecurityGroupManagementService));
            }
        }

        public Task<bool> UpdateProfileScheduleIdAsync(int profileId, string scheduleId)
        {
            try
            {
                var profile = profileDAO.GetProfileById(profileId);
                if (profile == null)
                {
                    return Task.FromResult(false);
                }

                profile.ScheduleId = scheduleId;
                return Task.FromResult(profileDAO.EditProfile(profile));
            }
            catch (Exception ex)
            {
                logger.Error("Failed to update report profile schedule. ProfileId={0}, Error={1}", profileId, ex);
                return Task.FromResult(false);
            }
        }

        private IGeneralSettingService mGeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();
        //public IGlobalStorageSettingDao mGlobalStorageSettingDao => PlatformWindsorManager.GetService<IGlobalStorageSettingDao>();
        private IRuleManagerService RuleService => PlatformWindsorManager.GetService<IRuleManagerService>();

        private ITermRuleAssociationDao TermRuleInfos => PlatformWindsorManager.GetService<ITermRuleAssociationDao>();

        private ISharePointSettingDao SPSettingDao => PlatformWindsorManager.GetService<ISharePointSettingDao>();

        private IOneDriveSettingDao OneDriveSettingDao => PlatformWindsorManager.GetService<IOneDriveSettingDao>();
        private IRMGoogleSettingDao GoogleDriveSettingDao => PlatformWindsorManager.GetService<IRMGoogleSettingDao>();
        private IRMGoogleRemoteNodeDao GoogleNodeDao => PlatformWindsorManager.GetService<IRMGoogleRemoteNodeDao>();
        private ISPSettingTreeService RMSPTreeService => PlatformWindsorManager.GetService<ISPSettingTreeService>();
        private ITeamsSettingTreeService RMTeamsTreeService => PlatformWindsorManager.GetService<ITeamsSettingTreeService>();

        private IRMFileSystemSettingsService RMFileSystemSettingsService => PlatformWindsorManager.GetService<IRMFileSystemSettingsService>();

        private IJobQueueService mJobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();
        private IRMSubJobDao SubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();
        public IJobMonitorDao JobMonitorDao => PlatformWindsorManager.GetService<IJobMonitorDao>();
        protected IRMRemoteNodeDao RMRemoteNodeDao => PlatformWindsorManager.GetService<IRMRemoteNodeDao>();
        public IHybridFileSystemWorkerService HybridFileSystemWorkerService { set; get; } = PlatformWindsorManager.GetService<IHybridFileSystemWorkerService>();
        protected IRMMailboxDao MailBoxDao => PlatformWindsorManager.GetService<IRMMailboxDao>();

        private static IJobQueueService JobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();

        private static IDownloadDataInfoDao DownloadDataInfoDao => PlatformWindsorManager.GetService<IDownloadDataInfoDao>();

        private static ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        private readonly IScheduleService ScheduleService = PlatformWindsorManager.GetService<IScheduleService>();
        #endregion

        #region public method

        [RACodeReview("Allen Yin")]
        [Audit(Module = AuditModule.ReportCenter, Category = AuditCategory.ReportCenter, Action = AuditAction.CreateProfile, BeforeHandler = typeof(TermUsageOrDueForDisposalBeforeAuditHandler), AfterHandler = typeof(TermUsageOrDueForDisposalAfterAuditHandler))]
        public async Task<RAReturnMessage> BuildProfileAsync(RMProfileDto profile)
        {
            RAReturnMessage returnMessage = new RAReturnMessage();

            if (profileDAO.CheckProfileNameExist(profile))
            {
                returnMessage.MessageType = RAMessageType.Failed;
                returnMessage.FaildType = RAFailedType.NameExisting;
                returnMessage.ErrorMessage = I18NEntity.GetString("RM_JS_RC_ProfileNameExist");
                return returnMessage;
                //throw new Exception(I18NEntity.GetString("RM_JS_RC_ProfileNameExist"));
            }
            if (JobTypeConstants.ContentDueReportJobTypes.Contains((int)profile.Type)
                || JobTypeConstants.ArchivedSiteReportJobTypes.Contains((int)profile.Type)
                || profile.Type == JobType.ItemsFilesDueDisposal || profile.Type == JobType.EXOItemsFilesDueDisposalReport
                || profile.Type == JobType.PhysicalItemsFilesDueDisposalReport || profile.Type == JobType.FSItemsFilesDueDisposal
                || profile.Type == JobType.OneDriveItemsFilesDueDisposalReport || profile.Type == JobType.SPOnPremItemsFilesDueDisposal
                || profile.Type == JobType.BoxItemsFilesDueDisposalReport || profile.Type == JobType.GoogleItemsFilesDueDisposalReport || profile.Type == JobType.TeamsItemsFilesDueDisposalReport)
            {
                    var dateTimeObj = JsonConvert.DeserializeObject<DisplayDateTime>(profile.Extension1);
                    if (dateTimeObj != null && !string.IsNullOrWhiteSpace(dateTimeObj.StartTime))
                    {
                        dateTimeObj.StartTime = await mGeneralSettingService.ConvertToUTCDateTimeAsync(dateTimeObj.StartTime);
                        profile.Extension1 = JsonConvert.SerializeObject(dateTimeObj);
                    }
            }
            if (profile.Type == JobType.CreateAndDestroyedFileReport)
            {
                //
            }
            //这的逻辑已经挪到controller
            //profile.Extension2 = BuildSPTreeXMLStr(profile.Extension2);
            profile.Id = profileDAO.SaveProfile(this.ConvertProfileToDBModel(profile));
            returnMessage.MessageType = RAMessageType.Successful;
            returnMessage.Extsion1 = profile;
            return returnMessage;
        }

        public async Task<RAReturnMessage> BuildJobNotificationProfileAsync(JobNotificationDto jobNotificationInfo)
        {
            var returnMessage = new RAReturnMessage();

            try
            {
                jobNotificationInfo.ProfileCreatedTime = DateTime.UtcNow.Ticks.ToString();
                var profile = new RMProfileDto
                {
                    ProfileName = jobNotificationInfo.ProfileName,
                    Description = jobNotificationInfo.ProfileDes,
                    Type = JobType.JobNotification,
                    Extension1 = SerializerHelper.SerializeByDataContractSerializer(jobNotificationInfo)
                };

                if (profileDAO.CheckProfileNameExist(profile))
                {
                    returnMessage.MessageType = RAMessageType.Failed;
                    returnMessage.FaildType = RAFailedType.NameExisting;
                    returnMessage.ErrorMessage = I18NEntity.GetString("RM_JS_RC_ProfileNameExist");
                    return returnMessage;
                    //throw new Exception(I18NEntity.GetString("RM_JS_RC_ProfileNameExist"));
                }

                profileDAO.SaveProfile(this.ConvertProfileToDBModel(profile));
                returnMessage.MessageType = RAMessageType.Successful;
                returnMessage.Extsion1 = profile;
                await CreateJobNotificationSchedule();
                return returnMessage;
            }
            catch (Exception e)
            {
                logger.Error($"Create job notification profile failed, error: {e}");
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = e.Message,
                };
            }
        }

        public async Task<RAReturnMessage> EditJobNotificationProfileAsync(JobNotificationDto jobNotificationInfo)
        {
            try
            {
                var dbResult = await GetProfileByIdAsync(jobNotificationInfo.ProfileId.ToString());
                var dbProfile = SerializerHelper.DeserializeByDataContractSerializer<JobNotificationDto>(dbResult.Extension1);
                jobNotificationInfo.ProfileCreatedTime = dbProfile.ProfileCreatedTime;

                var profile = new RMProfileDto
                {
                    Id = jobNotificationInfo.ProfileId,
                    ProfileName = jobNotificationInfo.ProfileName,
                    Description = jobNotificationInfo.ProfileDes,
                    Type = JobType.JobNotification,
                    Extension1 = SerializerHelper.SerializeByDataContractSerializer(jobNotificationInfo)
                };

                return await EidtProfileAsync(profile);
            }
            catch (Exception e)
            {
                logger.Error($"Edit job notification profile failed, error: {e}");
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = e.Message,
                };
            }
        }

        private async Task CreateJobNotificationSchedule()
        {
            var jobNotificationSchedule = await ScheduleService.GetScheduleByTypeServiceAsync(ScheduleType.JobNotificationSchedule);
            if (jobNotificationSchedule != null && jobNotificationSchedule.Count > 0)
            {
                return;
            }
            var generalSetting = mGeneralSettingService.GetGeneralSettingAsync();
            var info = new ScheduleInfo
            {
                Id = Guid.NewGuid().ToString()
            };

            var utcNow = DateTime.UtcNow;
            var globalTimeZoneId = (await generalSetting).TimeZoneId;
            TimeZoneInfo localZone = GeneralSettingConfig.FindSystemTimeZoneById(globalTimeZoneId);
            var localNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, localZone);
            localNow = localNow.AddDays(1);

            var startTime = new DateTime(localNow.Year, localNow.Month, localNow.Day, 0, 0, 0);
            info.StartTime = startTime.ToString();
            info.EndTime = startTime.ToString();
            info.EndType = 0;
            info.Interval = 1;
            info.IntervalType = IntervalType.Daily;
            info.JobCategory = ScheduleType.JobNotificationSchedule;
            info.OccurrencesTotal = 1;
            info.TimeZoneId = (await generalSetting).TimeZoneId;
            await ScheduleService.CreateScheduleServiceAsync(info);
        }

        [Audit(Module = AuditModule.ReportCenter, Category = AuditCategory.ReportCenter, Action = AuditAction.EditProfile, BeforeHandler = typeof(TermUsageOrDueForDisposalBeforeAuditHandler), AfterHandler = typeof(TermUsageOrDueForDisposalAfterAuditHandler))]
        public async Task<RAReturnMessage> EidtProfileAsync(RMProfileDto profile)
        {
            RAReturnMessage returnMessage = new RAReturnMessage();

            if (!profile.ProfileName.Equals(profileDAO.GetProfileById(profile.Id).Name) && profileDAO.CheckProfileNameExist(profile))
            {
                returnMessage.MessageType = RAMessageType.Failed;
                returnMessage.FaildType = RAFailedType.NameExisting;
                returnMessage.ErrorMessage = I18NEntity.GetString("RM_JS_RC_ProfileNameExist");
                return returnMessage;
            }
            if (JobTypeConstants.ContentDueReportJobTypes.Contains((int)profile.Type)
                || JobTypeConstants.ArchivedSiteReportJobTypes.Contains((int)profile.Type)
                || profile.Type == JobType.ItemsFilesDueDisposal || profile.Type == JobType.EXOItemsFilesDueDisposalReport
                || profile.Type == JobType.PhysicalItemsFilesDueDisposalReport || profile.Type == JobType.FSItemsFilesDueDisposal
                || profile.Type == JobType.OneDriveItemsFilesDueDisposalReport || profile.Type == JobType.SPOnPremItemsFilesDueDisposal
                || profile.Type == JobType.BoxItemsFilesDueDisposalReport || profile.Type == JobType.GoogleItemsFilesDueDisposalReport || profile.Type == JobType.TeamsItemsFilesDueDisposalReport)
            {
                    var dateTimeObj = JsonConvert.DeserializeObject<DisplayDateTime>(profile.Extension1);
                    if (dateTimeObj != null && !string.IsNullOrWhiteSpace(dateTimeObj.StartTime))
                    {
                        dateTimeObj.StartTime = await mGeneralSettingService.ConvertToUTCDateTimeAsync(dateTimeObj.StartTime);
                        dateTimeObj.IsDayLightSaving = false;
                        dateTimeObj.TimeZoneId = null;
                        profile.Extension1 = JsonConvert.SerializeObject(dateTimeObj);
                    }
            }
            //这的逻辑已经挪到controller里
            //profile.Extension2 = BuildSPTreeXMLStr(profile.Extension2);
            profileDAO.EditProfile(this.ConvertProfileToDBModel(profile));
            returnMessage.MessageType = RAMessageType.Successful;
            return returnMessage;
        }

        [RACodeReview("Allen Yin")]
        [Audit(Module = AuditModule.ReportCenter, Category = AuditCategory.ReportCenter, Action = AuditAction.DeleteProfile, BeforeHandler = typeof(TermUsageOrDueForDisposalBeforeAuditHandler), AfterHandler = typeof(TermUsageOrDueForDisposalAfterAuditHandler))]
        public async Task<(bool, List<string>)> DeleteProfilesAsync(DelProfileInfo dpi)
        {
            bool result = false;
            List<string> runningJobProfileNames = null;
            var profileIds = dpi.ProfileNames.Keys.ToList();
            if (dpi.DeleteJobs)
            {
                List<int?> RunningJobProfileIds = JobMonitorService.GetRunningJobsByProfileIds(profileIds);
                if (RunningJobProfileIds.Count > 0)
                {
                    runningJobProfileNames = new List<string>();
                    foreach (var item in dpi.ProfileNames)
                    {
                        if (RunningJobProfileIds.Contains(item.Key) && !runningJobProfileNames.Contains(item.Value))
                        {
                            runningJobProfileNames.Add(item.Value);
                        }
                    }
                    result = false;
                }
                else
                {
                    List<BaseJobDto> jobs = JobMonitorService.GetJobDtoByProfileIds(profileIds);
                    result = await profileDAO.RealDeleteProfilesAndJobsAsync(profileIds);
                    if (result)
                    {
                        JobMonitorService.DelJobReportFiles(jobs);
                    }
                }
            }
            else
            {
                try
                {
                    profileDAO.DeleteProfiles(profileIds);
                    result = true;
                }
                catch
                {
                    result = false;
                }
            }
            return (result, runningJobProfileNames);
        }

        [Audit(Module = AuditModule.ReportCenter, Category = AuditCategory.ReportCenter, Action = AuditAction.DeleteJobNotificationProfile, BeforeHandler = typeof(TermUsageOrDueForDisposalBeforeAuditHandler), AfterHandler = typeof(TermUsageOrDueForDisposalAfterAuditHandler))]
        public void DeleteJobNotificationProfile(List<int> profileIds)
        {
            try
            {
                profileDAO.DeleteProfiles(profileIds);
            }
            catch(Exception e)
            {
                logger.Error($"Delete profiles failed, error: {e}");
                throw;
            }
        }

        public async Task<string[][]> GenerateReportForJobAsync(int reportJobType, string[][] datas, int newJobType, IEnumerable<BaseReport> reportDetailList, bool isCreateHeader)
        {
            try
            {
                switch (reportJobType)
                {
                    case (int)JobType.BCSTermUsageReport:
                        if (isCreateHeader)
                        {
                            datas = AssembleBCSTermUsageReportHeaderTittle(datas);
                        }
                        return await ConvertBCSTermUsageReportToArrayAsync(reportDetailList, datas);
                    case (int)JobType.ItemsFilesDueDisposal:
                        reportJobType = newJobType;
                        var IsSPReport = reportJobType == (int)JobType.ItemsFilesDueDisposal
                            || reportJobType == (int)JobType.OneDriveItemsFilesDueDisposalReport
                            || reportJobType == (int)JobType.SPOnPremItemsFilesDueDisposal;
                        if (isCreateHeader)
                        {
                            datas = AssembleDueDisposalReportHeaderTittle(datas, IsSPReport);
                        }
                        return await ConvertDueDisposalReportToArrayAsync(reportDetailList, datas, IsSPReport);
                    case (int)JobType.AvailableSpaceReport:
                        if (isCreateHeader)
                        {
                            datas = AssembleAvailableSpaceReportHeaderTittle(datas);
                        }
                        return ConvertAvailableSpaceReportToArray(reportDetailList, datas);
                    //前台写死的值，无论哪个Source传的都是SP的,因此此处不需要添加其他Source
                    case (int)JobType.CreateAndDestroyedFileReport:
                        if (isCreateHeader)
                        {
                            datas = AssembleDueTimeFrameReportHeaderTittle(datas);
                        }
                        return ConvertDueTimeFrameReportToArray(reportDetailList, datas);
                    case (int)JobType.SPOActionAuditReport:
                    case (int)JobType.OneDriveActionAuditReport:
                    case (int)JobType.TeamsActionAuditReport:
                        if (isCreateHeader)
                        {
                            datas = AssembleClientAuditReportHeaderTittle(datas);
                        }
                        return await ConvertClientAuditReportToArrayAsync(reportDetailList, datas);
                    case (int)JobType.GenerateRestoreReport:
                        if (isCreateHeader)
                        {
                            datas = AssembleRestoreReportHeaderTittle(datas);
                        }
                        return ConvertRestoreReportToArray(reportDetailList, datas);
                    case (int)JobType.ArchivedSiteReport:
                    case (int)JobType.OneDriveArchivedSiteReport:
                    case (int)JobType.TeamsArchivedSiteReport:
                    case (int)JobType.GoogleArchivedSiteReport:
                        if (isCreateHeader)
                        {
                            datas = AssembleArchivedSiteReportHeaderTittle(datas);
                        }
                        return await ConvertArchivedSiteReportToArrayAsync(reportDetailList, datas);
                };
            }
            catch (Exception e)
            {
                logger.Error($"Generate report for export job failed {e}");
                throw;
            }

            return datas;
        }

        private string[][] AssembleArchivedSiteReportHeaderTittle(string[][] datas)
        {
            datas[0] = new[]
            {
                I18NEntity.GetString("StorageOptimization.Service_390AC5E1-D1D4-44F6-973F-F037414DC1EF"),
                I18NEntity.GetString("StorageOptimization.Service_684E2AE1-DC1D-47DF-AD8F-025251ABF811"),
                I18NEntity.GetString("StorageOptimization.Service_33C9D42B-9834-440F-A5EB-F5A393DEBC9E"),
                I18NEntity.GetString("StorageOptimization.Service_84F15AC4-BDBF-4F4D-A036-B63EBA03C404"),
                I18NEntity.GetString("StorageOptimization.Service_86D5507D-A47C-46F8-8D85-C7CBD183B23F"),
                I18NEntity.GetString("StorageOptimization.Service_1D64CD2C-D447-4C0D-813C-20925D93E1C3"),
            };
            return datas;
        }

        private async Task<string[][]> ConvertArchivedSiteReportToArrayAsync(IEnumerable<BaseReport> reportDetails, string[][] datas)
        {
            var generalSetting = await mGeneralSettingService.GetGeneralSettingAsync();
            var rowIndex = 1;
            foreach (var report in reportDetails.Cast<ArchivedSiteReport>())
            {
                datas[rowIndex++] = new[]
                {
                    report.Type,
                    report.SourceUrl,
                    report.ArchivedDataSize.ToString(),
                    ConverTicksToString(report.CreatedTime, report.SPWebTimeZoneName, generalSetting),
                    ConverTicksToString(report.LastModifiedTime, report.SPWebTimeZoneName, generalSetting),
                    ConverTicksToString(report.ArchivedTime, report.SPWebTimeZoneName, generalSetting),
                };
            }
            return datas;
        }



        [Audit(Module = AuditModule.ReportCenter, Category = AuditCategory.ReportCenter, Action = AuditAction.ExportReport, AfterHandler = typeof(TermUsageOrDueForDisposalAfterAuditHandler))]
        public async Task<bool> GenerateReportAsync(BaseJobDto baseJobDto, bool IsOrphanedTermReport = false, bool isRetiredTermReport = false)
        {
            string[][] datas = null;
            int countOfOneSheet = 65535;
            int sheetTotalCount = 0;
            int jobReportTotalCount = GetReportJobDatas(null, baseJobDto);
            IEnumerable<BaseReport> reportDetailList = null;
            string reportFilePath = JobReportUtility.GetDownloadReportDetailTempleFolder(baseJobDto, ".xlsx");
            if (!Directory.Exists(JobReportUtility.GetDownloadReportDetailTempleFolder(baseJobDto)))
            {
                Directory.CreateDirectory(JobReportUtility.GetDownloadReportDetailTempleFolder(baseJobDto));
            }
            try
            {
                if (jobReportTotalCount > 0)
                {
                    sheetTotalCount = jobReportTotalCount % countOfOneSheet == 0 ? jobReportTotalCount / countOfOneSheet : jobReportTotalCount / countOfOneSheet + 1;
                    ResetDateTimeFormat();
                    for (int index = 1; index < sheetTotalCount + 1; index++)
                    {
                        (reportDetailList, jobReportTotalCount) = await GetReportJobDatasAsync(countOfOneSheet, index, null, baseJobDto);
                        datas = new string[reportDetailList.Count() + 1][];
                        if (baseJobDto.JobType == (int)JobType.BCSTermUsageReport)
                        {
                            datas = await ConvertBCSTermUsageReportToArrayAsync(reportDetailList, AssembleBCSTermUsageReportHeaderTittle(datas));
                        }
                        else if (baseJobDto.JobType == (int)JobType.ItemsFilesDueDisposal)
                        {
                            var jobType = (JobType)JobMonitorDao.GetJob(baseJobDto.Id)?.JobType;
                            var IsSPReport = jobType == JobType.ItemsFilesDueDisposal || jobType == JobType.OneDriveItemsFilesDueDisposalReport || jobType == JobType.SPOnPremItemsFilesDueDisposal;
                            datas = await ConvertDueDisposalReportToArrayAsync(reportDetailList, AssembleDueDisposalReportHeaderTittle(datas, IsSPReport), IsSPReport);
                        }
                        else if (baseJobDto.JobType == (int)JobType.AvailableSpaceReport)
                        {
                            datas = ConvertAvailableSpaceReportToArray(reportDetailList, AssembleAvailableSpaceReportHeaderTittle(datas));
                        }
                        //前台写死的值，无论哪个Source传的都是SP的,因此此处不需要添加其他Source
                        else if (baseJobDto.JobType == (int)JobType.CreateAndDestroyedFileReport)
                        {
                            datas = ConvertDueTimeFrameReportToArray(reportDetailList, AssembleDueTimeFrameReportHeaderTittle(datas));
                        }
                        else if (baseJobDto.JobType == (int)JobType.SPOActionAuditReport || baseJobDto.JobType == (int)JobType.OneDriveActionAuditReport
                            || baseJobDto.JobType == (int)JobType.TeamsActionAuditReport)
                        {
                            datas = await ConvertClientAuditReportToArrayAsync(reportDetailList, AssembleClientAuditReportHeaderTittle(datas));
                        }
                        if (index == 1)
                        {
                            ReportUtil.CreateExcel(reportFilePath, "Sheet", datas);
                        }
                        else
                        {
                            ReportUtil.InsertWorksheet(reportFilePath, "Sheet" + index, datas);
                        }
                    }
                }
                else
                {
                    datas = new string[1][];
                    datas[0] = new string[] { I18NEntity.GetString("RM_RC_DueDisposal_DownLoadReport_NoData") };
                    ReportUtil.CreateExcel(reportFilePath, "Sheet", datas);
                }
                ZipUtil.ZipFolder(JobReportUtility.GetDownloadReportDetailTempleFolder(baseJobDto), JobReportUtility.GetDownloadReportDetailTempleFolder(baseJobDto) + ".zip", Encoding.UTF8);
                return true;
            }
            catch (Exception e)
            {
                logger.Debug("generate Report Erro Info:{0},{1}", e.Message, e.StackTrace);
                return false;
            }
        }

        public async Task<List<RMProfileDto>> GetJobNotificationProfiles()
        {
            var profiles = profileDAO.GetJobNotificationProfiles();
            var results = await Task.WhenAll(profiles.Select(ConvertToProfileDtoAsync));
            return [.. results];
        }
        public async Task<bool> GenarateReportSchedule(string scheduleId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(scheduleId))
                {
                    logger.Warn("ScheduleId is empty when generating scheduled report.");
                    return false;
                }

                var profile = profileDAO.GetProfileByScheduleId(scheduleId);
                if (profile == null)
                {
                    logger.Error($"Profile not found for scheduleId: {scheduleId}");
                    return false;
                }

                if (JobTypeConstants.ArchivedSiteReportJobTypes.Contains(profile.Type))
                {
                    return StartArchivedSiteReportSchedule(profile);
                }

                var result = StartReportJobSchedule((JobType)profile.Type, profile.Id, false);
                if (string.IsNullOrWhiteSpace(result))
                {
                    logger.Warn($"Failed to start report job for profile {profile.Id}, schedule {scheduleId}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to generate report schedule. ScheduleId: {scheduleId}", ex);
                return false;
            }
        }

        private bool StartArchivedSiteReportSchedule(RMProfile profile)
        {
            var jobId = StartArchivedSiteReport(profile, JobRunBy.Schedule);
            return jobId != null && jobId.Length > 0;
        }

        public string StartArchivedSiteReportJob(int profileId)
        {
            try
            {
                var profile = profileDAO.GetProfileById(profileId);
                return profile == null ? string.Empty : StartArchivedSiteReport(profile, JobRunBy.Control);
            }
            catch (Exception ex)
            {
                logger.Error("Failed to start Archived Sites report. ProfileId:{0}, Error:{1}", profileId, ex);
                return string.Empty;
            }
        }

        private string StartArchivedSiteReport(RMProfile profile, JobRunBy jobRunType)
        {
            if (!profile.ObjectLevel.HasValue
                || (profile.ObjectLevel.Value != (int)ReportType.AllItem
                    && profile.ObjectLevel.Value != (int)ReportType.AllSubSite))
            {
                logger.Warn($"Invalid Archived Sites object level. ProfileId:{profile.Id}");
                return string.Empty;
            }

            var jobQueue = new JobQueueDto
            {
                JobType = (JobType)profile.Type,
                JobRunType = jobRunType,
                TenantGroupId = TenantLocalValue.LogonGroupId,
                JobRunByUser = TenantLocalValue.LogonUserEmail,
                Parameters = string.Format("{0} {1} {2} {3} {4}", profile.Id, false, true, false, false)
            };

            return mJobQueueService.AddToDBJobQueue(jobQueue);
        }

        private List<ArchiverSiteSizeInfo> GetArchivedSiteExportInfos(string extension2)
        {
            var treeJson = extension2.TrimStart().StartsWith("[")
                ? extension2
                : RuleSPTreeUtil.ConvertXmlStrToSPTreeJsonStr(extension2);
            var nodes = SerializerHelper.DeserializeByJsonConvert<List<RMSPTreeNode>>(treeJson);
            return nodes
                .Where(node => node.CheckNumber != 0 && !string.IsNullOrWhiteSpace(node.FullPath))
                .Select(node => new ArchiverSiteSizeInfo { SiteId = node.Id, SiteUrl = node.FullPath })
                .ToList();
        }

        private static bool IsValidExportDestination(string extension3)
        {
            if (string.IsNullOrWhiteSpace(extension3))
            {
                return true;
            }

            try
            {
                var destination = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(extension3);
                return destination != null && !string.IsNullOrWhiteSpace(destination.FullPath);
            }
            catch (Exception)
            {
                return false;
            }
        }
        private static IEnumerable<RMSPTreeNode> GetSelectedArchivedSiteNodes(RMSPTreeNode node)
        {
            if (node == null)
            {
                return Enumerable.Empty<RMSPTreeNode>();
            }

            var children = node.Children ?? new List<RMSPTreeNode>();
            var selectedChildren = children.SelectMany(GetSelectedArchivedSiteNodes);
            return node.CheckNumber != 0 && !string.IsNullOrWhiteSpace(node.FullPath)
                ? new[] { node }.Concat(selectedChildren)
                : selectedChildren;
        }

        public async Task<RMProfileDto> GetProfileByIdAsync(string Id)
        {
            RMProfileDto dto = await ConvertToProfileDtoAsync(profileDAO.GetProfileById(int.Parse(Id)));
            switch (dto.Type)
            {
                case JobType.ItemsFilesDueDisposal:
                case JobType.EXOItemsFilesDueDisposalReport:
                case JobType.PhysicalItemsFilesDueDisposalReport:
                case JobType.FSItemsFilesDueDisposal:
                case JobType.OneDriveItemsFilesDueDisposalReport:
                case JobType.SPOnPremItemsFilesDueDisposal:
                case JobType.BoxItemsFilesDueDisposalReport:
                case JobType.GoogleItemsFilesDueDisposalReport:
                case JobType.TeamsItemsFilesDueDisposalReport:
                    var dateTimeObj = JsonConvert.DeserializeObject<DisplayDateTime>(dto.Extension1);
                    dateTimeObj.StartTime = await mGeneralSettingService.ConvertFromUTCDateTimeAsync(dateTimeObj.StartTime);
                    dateTimeObj.IsDayLightSaving = false;
                    dateTimeObj.TimeZoneId = null;
                    dto.Extension1 = JsonConvert.SerializeObject(dateTimeObj);
                    break;
                case JobType.BCSTermUsageReport:
                case JobType.FSBCSTermUsageReport:
                case JobType.EXOTermUsageReport:
                case JobType.PhysicalTermUsageReport:
                case JobType.OneDriveTermUsageReport:
                case JobType.SPOnPremBCSTermUsageReport:
                case JobType.BoxBCSTermUsageReport:
                case JobType.GoogleBCSTermUsageReport:
                case JobType.TeamsBCSTermUsageReport:
                    //dto.Extension1 = ConvertToJson(this.BuildRMTermSetTree(dto.Extension1));
                    dto.Extension1 = ConvertToJson(this.GetTermTree(dto.Extension1));
                    break;
                case JobType.CreateAndDestroyedFileReport:
                case JobType.EXOCreateAndDestroyedFileReport:
                case JobType.PhysicalCreateAndDestroyedFileReport:
                case JobType.FSCreateAndDestroyedFileReport:
                case JobType.OneDriveCreateAndDestroyedFileReport:
                case JobType.SPOnPremCreateAndDestroyedFileReport:
                case JobType.RestoreReport:
                case JobType.OneDriverRestoreReport:
                case JobType.TeamsRestoreReport:
                case JobType.BoxCreateAndDestroyedFileReport:
                case JobType.GoogleCreateAndDestroyedFileReport:
                case JobType.GoogleRestoreReport:
                case JobType.TeamsCreateAndDestroyedFileReport:
                    if (dto.RangeType == TimeRangeType.Custom)
                    {
                        string[] timeArray = dto.Extension1.Split(',');
                        string starttimeStr = timeArray[0] + "}";
                        string endtimeStr = "{" + timeArray[1];
                        endtimeStr = endtimeStr.Replace("EndTime", "StartTime");
                        var sttime = JsonConvert.DeserializeObject<DisplayDateTime>(starttimeStr);
                        var endtime = JsonConvert.DeserializeObject<DisplayDateTime>(endtimeStr);
                        dto.StartTime = DateTime.Parse(sttime.StartTime);
                        dto.EndTime = DateTime.Parse(endtime.StartTime);
                    }
                    else
                    {
                        DateTime startTime = DateTime.UtcNow;
                        DateTime endTime = startTime;
                        var globalTimeZone = GeneralSettingConfig.FindSystemTimeZoneById((await mGeneralSettingService.GetGeneralSettingAsync()).TimeZoneId);
                        GetRangeDate(ref startTime, ref endTime, dto.RangeType, globalTimeZone);
                        dto.StartTime = startTime;
                        dto.EndTime = endTime;
                    }
                    break;
                default:
                    break;
            }
            return dto;
        }

        private void GetRangeDate(ref DateTime start, ref DateTime end, TimeRangeType tangeType, TimeZoneInfo userTimeZone)
        {
            //对于One_Month 这种range,时间范围从月初开始. e.g 当前3月13日，onemonth是3月1日 to now
            DateTime now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, userTimeZone);
            DateTime tmp = now;
            if (tangeType != TimeRangeType.Custom)
            {
                end = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0).AddDays(1);
                tmp = new DateTime(end.Year, end.Month, 1, 0, 0, 0);
            }
            switch (tangeType)
            {
                case TimeRangeType.CurrentWeek:
                    start = end.AddDays(-(((int)now.DayOfWeek + 6) % 7) -1);
                    break;
                case TimeRangeType.CurrentMonth:
                    start = tmp;
                    break;
                case TimeRangeType.Last3Month:
                    start = tmp.AddMonths(-2);
                    break;
                case TimeRangeType.Last6Month:
                    start = tmp.AddMonths(-5);
                    break;
                //case TimeRangeType.Custom:
                //    start = new DateTime(start.Value.Year, start.Value.Month, start.Value.Day, 0, 0, 0);
                //    end = new DateTime(end.Value.Year, end.Value.Month, end.Value.Day, 23, 59, 59);
                //    break;
                default:
                    start = end.AddDays(-6).AddHours(-23).AddMinutes(-59).AddSeconds(-59);
                    break;
            }
        }

        public RMProfileDto GetProfileByIdForReportJob(string Id)
        {
            RMProfile profile = profileDAO.GetProfileById(int.Parse(Id));
            RMProfileDto profileDto = new RMProfileDto()
            {
                Id = profile.Id,
                ProfileName = profile.Name,
                Description = profile.Description,
                Type = (JobType)profile.Type,
                Extension1 = profile.Extension1,
                Extension2 = profile.Extension2,
                Extension3 = profile.Extension3,
                ObjectLevel = profile.ObjectLevel,
                ScheduleId = profile.ScheduleId
            };
            return profileDto;
        }

        public DateTime GetUtcTimePoint(string ext1)
        {
            var dateTimeObj = JsonConvert.DeserializeObject<DisplayDateTime>(ext1);
            DateTime utcDt = DateTime.Parse(dateTimeObj.StartTime);
            utcDt = DateTime.SpecifyKind(utcDt, DateTimeKind.Utc);
            logger.Info($"TimePoint:{utcDt.ToString(JSDateTimeFormat.DEFAULT_TIME_FORMAT)}");
            return utcDt;
        }

        public async Task<string> GetJobMessageForFSAsync(string jobId)
        {
            try
            {
                logger.Debug("Start to get job message. Job Id:" + jobId);
                var subJob = SubJobDao.GetSubJob(jobId, true);
                var mainJob = JobMonitorDao.GetJob(subJob.ParentId);

                BaseJobDto jobDto = new BaseJobDto()
                {
                    Id = mainJob.Id,
                    JobType = mainJob.JobType
                };
                var settings = JobMonitorService.GetJobContextSettingByJobId(jobId);
                var nodes = SerializerHelper.DeserializeByDataContractSerializer<List<FSTreeNodeDto>>(settings);
                var node = nodes.FirstOrDefault();  //Connection Level
                AvePoint.RA.Contract.Global.Object.FSJobMessage jobMsg = new AvePoint.RA.Contract.Global.Object.FSJobMessage();
                jobMsg.Job = jobDto;
                jobMsg.JobId = jobId;
                jobMsg.FSTreeNodes = nodes.Select(a => RMDtoConverter.ConvertFSTreeNode2GlobalDto(a)).ToList();  // new List<Contract.Global.Object.FSTreeNodeDto>() { RMDtoConverter.ConvertFSTreeNode2GlobalDto(node)};

                await RMFileSystemSettingsService.AssembleCacheDataForDisposalAsync(new Guid(node?.ParentId), jobMsg);
                var generalSetting = await mGeneralSettingService.GetGeneralSettingAsync();
                if (generalSetting != null)
                {
                    jobMsg.GeneralSettingModel = SerializerHelper.SerializeByDataContractSerializer(generalSetting);
                    jobMsg.TimeFormat = DateTimeUtil.GetAllStaticTimeZones().Where(x => x.Id == RA.Common.SystemSetting.GeneralSettingConfig.GetTimeZoneInforById(generalSetting.TimeZoneId).Id).FirstOrDefault()?.DisplayName;
                }
                if (mainJob.JobType == (int)JobType.FSCreateAndDestroyedFileReport)
                {
                    RMProfileDto dto = await GetProfileByIdAsync(mainJob.ProfileId.ToString());
                    jobMsg.StartTime = DateTimeUtil.ConvertTimeToUtcDate(dto.StartTime, generalSetting.TimeZoneId, generalSetting.DayLight);
                    jobMsg.EndTime = DateTimeUtil.ConvertTimeToUtcDate(dto.EndTime, generalSetting.TimeZoneId, generalSetting.DayLight);
                }
                bool isCosmosBulkOperationEnabled = RMKeyValueDao.IsCosmosBulkOperationEnabled();
                if (isCosmosBulkOperationEnabled)
                {
                    jobMsg.BulkImportEnabled = true;
                    var bulkSize = RMKeyValueDao.GetCosmosBulkInsertOperationBufferSize();
                    if (bulkSize == default(int))
                    {
                        bulkSize = DB.Explorer.Bulk.CosmosBulkOperator.DefualtBufferSize;
                    }
                    logger.Info($"Cosmos bulk operation enabled, bulk size: {bulkSize}");
                    jobMsg.BulkSize = bulkSize;
                }
                return SerializerHelper.SerializeByDataContractSerializer(jobMsg);
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while getting job message. JobId:{0} Error:{1}", jobId, e.ToString());
                return string.Empty;
            }
            finally
            {
                logger.Debug("Get job message finished. Job Id: " + jobId);
            }
        }

        public string StartReportJob(JobType jobType, int profileId, bool IsOrphanedTermReport, bool isRetiredTermReport = false)
        {
            string id = string.Empty;
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = jobType,
                    Parameters = string.Format("{0} {1} {2} {3} {4}", profileId, IsOrphanedTermReport, true, false, isRetiredTermReport),
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName
                };
                id = mJobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while StartReportJob,ERROR:{0}", ex.ToString());
            }

            return id;

        }
        public string StartReportJobSchedule(JobType jobType, int profileId, bool IsOrphanedTermReport, bool isRetiredTermReport = false)
        {
            string id = string.Empty;
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = jobType,
                    Parameters = string.Format("{0} {1} {2} {3} {4}", profileId, IsOrphanedTermReport, true, false, isRetiredTermReport),
                    JobRunType = JobRunBy.Schedule,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName
                };
                logger.Info($"Scheduled report job started.TypeJob:{jqDto.JobRunType}");

                id = mJobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while StartReportJob,ERROR:{0}", ex.ToString());
            }

            return id;

        }
        private string CreateSubJob(string jobId, int currentSubjobIndex, JobType jobType, int subJobCount, List<FSTreeNodeDto> tempList, bool sendNow, string fullPath)
        {
            string subJobId = string.Format(jobId + "_{0:D3}", currentSubjobIndex);
            var subJob = new RMSubJob() { Id = subJobId, ParentId = jobId, StartTime = DateTime.UtcNow.Ticks, JobType = (int)jobType, Progress = 0, Status = (int)JobStatus.Wait, Weight = 100d / subJobCount, String1 = fullPath };
            subJob.Runable = sendNow ? RecordsConstants.SubJob_Runnable_Runing : RecordsConstants.SubJob_Runnable_Waiting;
            if (tempList != null)
            {
                subJob.JobContext = new RMJobContext() { JobId = subJobId, Settings = SerializerHelper.SerializeByDataContractSerializer(tempList) };
            }
            //if (message != null)
            //{
            //    subJob.JobContext.Content = SerializerHelper.SerializeByDataContractSerializer(message);
            //}
            SubJobDao.CreateJob(subJob);
            logger.Info("Create sub job {0} sucessfull, type {1}, weight {2} ", subJob.Id, subJob.JobType, subJob.Weight);
            return subJobId;
        }
        public async Task<string> RunFSReportJobInAgentAsync(JobType jobType, RMProfileDto profile, string jobRunByUser, string userId)
        {
            int subJobCountInConfigFile = RMKeyValueDao.GetSubJobCountFromDB((int)jobType);
            RMFSTreeNode FSTreeNode = SerializerHelper.DeserializeByJsonSerializer<RMFSTreeNode>(RuleSPTreeUtil.BuildFSTreeJsonStr(profile.Extension2));
            FSReportManager fSReport = new FSReportManager(profile.Id.ToString(), jobType);
            var allconnections = await fSReport.GetSelectedConnectionsAsync();
            int subJobCount = allconnections.Count;
            logger.Info("Runnable connections count {0}", subJobCount);

            string jobId = JobMonitorService.CreateJobWithProfileId(jobType, jobRunByUser, profile.Id, userId, subJobCount);

            var groupIds = FSTreeNode.Children.Select(item => item.Id).ToList();
            //var parallelSubJobCount = subJobCountInConfigFile * HybridFileSystemWorkerService.GetAgentCount();
            var parallelSubJobCount = subJobCountInConfigFile * await HybridFileSystemWorkerService.GetAgentCountByGroupsAsync(groupIds);
            if (parallelSubJobCount == 0)
            {
                logger.Error("No available agent server. Set main job failed.");
                JobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_SS_FSNoAvailableAgent");
                return jobId;
            }
            var tempList = new List<FSTreeNodeDto>();
            int currentSubjobIndex = 0;
            foreach (FSTreeNodeDto site in allconnections)
            {
                if (jobType == JobType.FSItemsFilesDueDisposal)
                {
                    site.TimeStamp = GetUtcTimePoint(profile.Extension1).Ticks;
                }
                tempList.Add(site);
                string subJobId = CreateSubJob(jobId, currentSubjobIndex, jobType, subJobCount, tempList, currentSubjobIndex < parallelSubJobCount, site.FullPath);
                if (currentSubjobIndex < parallelSubJobCount)  //一次只发两个子job, 后续在JobInfoUpdater中触发
                {
                    HybridFileSystemWorkerService.StartJobWithConnectionGroupId(new Hybrid.Contract.RecordsJobArgs()
                    {
                        JobId = subJobId,
                        JobType = AvePoint.Hybrid.Contract.JobType.FSContentDueReport,
                        TenantId = TenantLocalValue.LogonGroupId
                    }, new Guid(site.ParentId));
                }
                tempList.Clear();
                currentSubjobIndex++;
            }
            return jobId;
        }
        public async Task<string> RunFSCreationReportJobInAgentAsync(JobType jobType, RMProfileDto profile, string jobRunByUser, string userId)
        {
            int subJobCountInConfigFile = RMKeyValueDao.GetSubJobCountFromDB((int)jobType);
            RMFSTreeNode FSTreeNode = SerializerHelper.DeserializeByJsonSerializer<RMFSTreeNode>(RuleSPTreeUtil.BuildFSTreeJsonStr(profile.Extension2));
            var groupIds = FSTreeNode.Children.Select(item => item.Id).ToList();
            int subJobCount = profile.IsCreated && profile.IsDestoryed ? groupIds.Count + 1 : groupIds.Count;
            logger.Info("subjob count {0}", subJobCount);

            string jobId = JobMonitorService.CreateJobWithProfileId(jobType, jobRunByUser, profile.Id, userId, subJobCount);

            int currentSubjobIndex = 0;
            if (profile.IsCreated)
            {
                logger.Info("Create sub job for creation report");
                var parallelSubJobCount = subJobCountInConfigFile * await HybridFileSystemWorkerService.GetAgentCountByGroupsAsync(groupIds);
                if (parallelSubJobCount == 0)
                {
                    logger.Error("No available agent server. Set main job failed.");
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_SS_FSNoAvailableAgent");
                    return jobId;
                }
                FSReportManager fSReport = new FSReportManager(profile.Id.ToString(), jobType);
                var allconnections = await fSReport.GetSelectedConnectionsAsync();
                foreach (var item in groupIds)
                {
                    List<FSTreeNodeDto> tempList = allconnections.Where(a => a.ParentId == item.ToString()).ToList();
                    string subJobId = CreateSubJob(jobId, currentSubjobIndex, jobType, subJobCount, tempList, currentSubjobIndex < parallelSubJobCount, null);
                    logger.Info($"Create sub job {subJobId} for creation report");

                    if (currentSubjobIndex < parallelSubJobCount)  //一次只发两个子job, 后续在JobInfoUpdater中触发
                    {
                        HybridFileSystemWorkerService.StartJobWithConnectionGroupId(new Hybrid.Contract.RecordsJobArgs()
                        {
                            JobId = subJobId,
                            JobType = Hybrid.Contract.JobType.FSCreationAndDestructionReport,
                            TenantId = TenantLocalValue.LogonGroupId
                        }, item);
                    }
                    currentSubjobIndex++;
                }
            }
            if (profile.IsDestoryed)
            {
                profile.IsCreated = false;  //job role不处理Created数据
                string subJobId = CreateSubJob(jobId, currentSubjobIndex, jobType, subJobCount, null, true, null);
                logger.Info($"Create sub job {subJobId} for destrction report");
                JobQueueMessage message = new JobQueueMessage()
                {
                    JobId = subJobId,
                    JobType = jobType,
                    CommandLine = string.Format("{0} {1} {2} {3} {4} {5} {6} {7}", jobType, subJobId, profile.Id.ToString(), profile.IsCreated, profile.IsDestoryed, profile.StartTime.ToString("yyyy/MM/dd"), profile.EndTime.ToString("yyyy/MM/dd"), (await mGeneralSettingService.GetGeneralSettingAsync()).TimeZoneId.Replace(" ", "_")),
                };
                mJobQueueService.HandleMessage(message);
            }

            return jobId;
        }
        [Audit(Module = AuditModule.ReportCenter, Category = AuditCategory.ReportCenter, Action = AuditAction.GenerateReport, AfterHandler = typeof(TermUsageOrDueForDisposalAfterAuditHandler))]
        public async Task<string> RealRunReportJobAsync(JobType jobType, string jobRunByUser, int profileId, bool IsOrphanedTermReport, bool isRetiredTermReport = false)
        {
            //int subJobCountInConfigFile = RMGlobalConfiguration.AppConfig.GetSubJobCount(new Guid(TenantLocalValue.LogonGroupId), (int)jobType);
            int subJobCountInConfigFile = RMKeyValueDao.GetSubJobCountFromDB((int)jobType);
            var account = await AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail);
            RMProfileDto dto = await GetProfileByIdAsync(profileId.ToString());
            string jobId = null;
            if (JobTypeConstants.ArchivedSiteReportJobTypes.Contains((int)jobType))
            {
                jobId = JobMonitorService.CreateJobWithProfileId(jobType, jobRunByUser, profileId, account.UserId);
                await GenerateArchivedSiteReportAsync(jobId, jobType);
                return jobId;
            }
            int ClassLevel = RMFileSystemSettingsService.GetClassificationLevel();
            if (ClassLevel == (int)NodeLevel.FSFolder && (jobType == JobType.FSItemsFilesDueDisposal || jobType == JobType.FSCreateAndDestroyedFileReport))
            {
                ClassLevel = RMFileSystemSettingsService.GetClassificationLevel();
                if (ClassLevel == (int)NodeLevel.FSFolder)
                {
                    if (jobType == JobType.FSItemsFilesDueDisposal)
                    {
                        logger.Info("Folder level FSItemsFilesDueDisposal start job in agent.");
                        return await RunFSReportJobInAgentAsync(jobType, dto, jobRunByUser, account.UserId);
                    }
                    if (jobType == JobType.FSCreateAndDestroyedFileReport)
                    {
                        logger.Info("Folder level FSCreateAndDestroyedFileReport start job in agent.");
                        return await RunFSCreationReportJobInAgentAsync(jobType, dto, jobRunByUser, account.UserId);
                    }
                }
            }
            else
            {
                jobId = JobMonitorService.CreateJobWithProfileId(jobType, jobRunByUser, profileId, account.UserId);
            }

            if (jobType == JobType.FSBCSTermUsageReport || jobType == JobType.FSOrphanedTermReport || jobType == JobType.FSRetiredTermReport || jobType == JobType.SPOActionAuditReport || jobType == JobType.OneDriveActionAuditReport || jobType == JobType.TeamsActionAuditReport
                || jobType == JobType.BoxBCSTermUsageReport || jobType == JobType.BoxOrphanedTermUsageReport || jobType == JobType.BoxRetiredTermUsageReport)
            {
                return await RunReportJobInMainJobAsync(jobId, jobType, jobRunByUser, profileId, IsOrphanedTermReport, isRetiredTermReport);
            }
            #region Physical Term Usage Report Job
            if (jobType == JobType.PhysicalTermUsageReport || jobType == JobType.PhysicalOrphanedTermUsageReport || jobType == JobType.PhysicalRetiredTermUsageReport || jobType == JobType.PhysicalCreateAndDestroyedFileReport
                || jobType == JobType.FSCreateAndDestroyedFileReport || jobType == JobType.SPOnPremCreateAndDestroyedFileReport || jobType == JobType.BoxCreateAndDestroyedFileReport
                || jobType == JobType.SPOnPremBCSTermUsageReport || jobType == JobType.SPOnPremRetiredTermReport || jobType == JobType.SPOnPremOrphanedTermReport)
            {
                return await RunReportJobInMainJobAsync(jobId, jobType, jobRunByUser, profileId, IsOrphanedTermReport, isRetiredTermReport);
            }
            #endregion
            #region Physical Content Due Report Job
            if (jobType == JobType.PhysicalItemsFilesDueDisposalReport)
            {
                return await RunReportJobInMainJobAsync(jobId, jobType, jobRunByUser, profileId, IsOrphanedTermReport, isRetiredTermReport);
            }
            if (jobType == JobType.FSItemsFilesDueDisposal || jobType == JobType.SPOnPremItemsFilesDueDisposal
                || jobType == JobType.BoxItemsFilesDueDisposalReport)
            {
                return await RunReportJobInMainJobAsync(jobId, jobType, jobRunByUser, profileId, IsOrphanedTermReport, isRetiredTermReport);
            }

            if (jobType == JobType.GoogleCreateAndDestroyedFileReport
                || jobType == JobType.GoogleBCSTermUsageReport
                || jobType == JobType.GoogleOrphanedTermUsageReport
                || jobType == JobType.GoogleRetiredTermUsageReport
                || jobType == JobType.GoogleItemsFilesDueDisposalReport
                || jobType == JobType.GoogleRestoreReport)
            {
                return await RunGoogleReportAsync(jobType, dto, jobId, profileId, IsOrphanedTermReport, isRetiredTermReport);
            }
            #endregion
            if (jobType == JobType.OneDriverRestoreReport || jobType == JobType.RestoreReport || jobType == JobType.TeamsRestoreReport || jobType == JobType.GoogleRestoreReport)
            {
                List<JobType> restoreRportTyps = new List<JobType>() { JobType.OneDriverRestoreReport, JobType.RestoreReport , JobType.TeamsRestoreReport, JobType.GoogleRestoreReport };
                List<BaseJobDto> runningRestoreReports = JobMonitorService.GetRunningJobs(restoreRportTyps);
                if (runningRestoreReports.Count > 1)
                {
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                    logger.Warn("Restore Report Job Only Can Serial. Skip.");
                    return jobId;
                }
            }

            if (jobType == JobType.OneDriveItemsFilesDueDisposalReport
                || jobType == JobType.OneDriveTermUsageReport
                || jobType == JobType.OneDriveItemsFilesDueDisposalReport
                || jobType == JobType.OneDriveItemsFilesDueDisposalReport
                || jobType == JobType.OneDriverRestoreReport
                || jobType == JobType.OneDriveCreateAndDestroyedFileReport)
            {
                return await RunOneDriveReportJobAsync(jobId, jobType, jobRunByUser, profileId, IsOrphanedTermReport, dto, subJobCountInConfigFile, isRetiredTermReport);
            }

            List<JobType> teamsReportJobType = new List<JobType>
            {
                JobType.TeamsRestoreReport,
                JobType.TeamsItemsFilesDueDisposalReport,
                JobType.TeamsBCSTermUsageReport,
                JobType.TeamsCreateAndDestroyedFileReport,
                JobType.TeamsOrphanedTermUsageReport,
                JobType.TeamsRetiredTermUsageReport,
            };

            if (teamsReportJobType.Contains(jobType))
            {
                return await RunTeamsReportJobAsync(jobId, jobType, jobRunByUser, profileId, IsOrphanedTermReport, dto, subJobCountInConfigFile, isRetiredTermReport);
            }

            //Exchange Online Report Job
            if (jobType >= JobType.EXOItemsFilesDueDisposalReport)
            {
                #region Exchange Online
                List<RMEXOTreeNode> availableSites = AssembleRunableMessageBox(dto);

                //TreeNode Empty, Job空跑Skipped.
                if (availableSites.Count == 0)
                {
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_JM_Report_Skip_NoAvailableMailBox");
                    logger.Warn("No mail box for this job. Skip.");
                    return jobId;
                }
                List<RMEXOTreeNode> tempList = new List<RMEXOTreeNode>();
                int subJobCount = availableSites.Count % RMGlobalConfiguration.AppConfig.NodeCountInSubJob == 0 ? availableSites.Count / RMGlobalConfiguration.AppConfig.NodeCountInSubJob : availableSites.Count / RMGlobalConfiguration.AppConfig.NodeCountInSubJob + 1;
                SubJobDao.UpdateSubJobCount(jobId, subJobCount);

                string timeZoneId = (await mGeneralSettingService.GetGeneralSettingAsync()).TimeZoneId.Replace(" ", "_");

                int currentSubjobIndex = 0;
                foreach (RMEXOTreeNode site in availableSites)
                {
                    tempList.Add(site);
                    if (tempList.Count == RMGlobalConfiguration.AppConfig.NodeCountInSubJob)
                    {
                        string subJobId = this.GenerateSubJobId(jobId, currentSubjobIndex);
                        var cmdPrex = string.Empty;
                        if (jobType == JobType.EXOCreateAndDestroyedFileReport || jobType == JobType.PhysicalCreateAndDestroyedFileReport || jobType == JobType.FSCreateAndDestroyedFileReport || jobType == JobType.BoxCreateAndDestroyedFileReport || jobType == JobType.GoogleCreateAndDestroyedFileReport)
                        {
                            cmdPrex = string.Format("{0} {1} {2} {3} {4} {5}", profileId.ToString(), dto.IsCreated, dto.IsDestoryed, dto.StartTime.ToString("yyyy/MM/dd"), dto.EndTime.ToString("yyyy/MM/dd"), timeZoneId);
                        }
                        else
                        {
                            cmdPrex = string.Format("{0} {1} {2}", profileId.ToString(), IsOrphanedTermReport, isRetiredTermReport);
                        }
                        string cmdLine = "{0} {1} " + cmdPrex;
                        var commandLine = string.Format(cmdLine, jobType, subJobId);
                        CreateSubJob(subJobId, jobId, jobType, subJobCount, tempList, cmdLine, currentSubjobIndex < subJobCountInConfigFile);
                        if (currentSubjobIndex < subJobCountInConfigFile)  //一次只发两个子job, 后续在JobInfoUpdater中触发
                        {
                            JobQueueMessage message = new JobQueueMessage()
                            {
                                JobId = subJobId,
                                JobType = jobType,
                                CommandLine = commandLine
                            };
                            mJobQueueService.HandleMessage(message);
                        }
                        tempList.Clear();
                        currentSubjobIndex++;
                    }
                }
                if (tempList.Count > 0)
                {
                    string subJobId = this.GenerateSubJobId(jobId, currentSubjobIndex);
                    var cmdPrex = string.Empty;
                    if (jobType == JobType.EXOCreateAndDestroyedFileReport)
                    {
                        cmdPrex = string.Format("{0} {1} {2} {3} {4} {5}", profileId.ToString(), dto.IsCreated, dto.IsDestoryed, dto.StartTime.ToString("yyyy/MM/dd"), dto.EndTime.ToString("yyyy/MM/dd"), timeZoneId);
                    }
                    else
                    {
                        cmdPrex = string.Format("{0} {1} {2}", profileId.ToString(), IsOrphanedTermReport, isRetiredTermReport);
                    }
                    string cmdLine = "{0} {1} " + cmdPrex;
                    var commandLine = string.Format(cmdLine, jobType, subJobId);
                    CreateSubJob(subJobId, jobId, jobType, subJobCount, tempList, cmdLine, currentSubjobIndex < subJobCountInConfigFile);
                    if (currentSubjobIndex < subJobCountInConfigFile)  //一次只发两个子job, 后续在JobInfoUpdater中触发
                    {
                        JobQueueMessage message = new JobQueueMessage()
                        {
                            JobId = subJobId,
                            JobType = jobType,
                            CommandLine = commandLine
                        };
                        mJobQueueService.HandleMessage(message);
                    }
                    tempList.Clear();
                }
                #endregion
            }
            //SharePoint Online Report Job
            else
            {
                #region SharePoint Online
                if (jobType == JobType.AvailableSpaceReport)
                {
                    return await RunReportJobInMainJobAsync(jobId, jobType, jobRunByUser, profileId, IsOrphanedTermReport, isRetiredTermReport);
                }

                List<RMSPTreeNode> availableSites = await AssembleRunableSitesAsync(dto, RMBrowseTreeNodeSourceType.SharepointOnline,
                    jobType != JobType.RestoreReport && jobType != JobType.OneDriverRestoreReport);

                //TreeNode Empty, Job空跑Skipped.
                if (availableSites.Count == 0)
                {
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_JM_Report_Skip_NoAvailableSites");
                    logger.Warn("No sites available for this job. Skip.");
                    return jobId;
                }

                List<string> groupIds = availableSites.Select(s => s.GetGroupNode().Id).ToList();
                if (!SPSettingDao.ChickGroupSettingExist(groupIds) && !JobReportUtility.CheckInSOReportTypes((int)jobType))
                {
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_JM_Report_Skip_ContainerNoConfigSetting");
                    logger.Warn("The Container Settings of the selected site are not configured");
                    return jobId;
                }

                List<JobType> runInOneSubJobList = new List<JobType>() { JobType.RestoreReport, JobType.OneDriverRestoreReport };

                List<RMSPTreeNode> tempList = new List<RMSPTreeNode>();
                int subJobCount = runInOneSubJobList.Contains(jobType) ? 1 :
                    availableSites.Count % RMGlobalConfiguration.AppConfig.NodeCountInSubJob == 0 ? availableSites.Count / RMGlobalConfiguration.AppConfig.NodeCountInSubJob : availableSites.Count / RMGlobalConfiguration.AppConfig.NodeCountInSubJob + 1;
                SubJobDao.UpdateSubJobCount(jobId, subJobCount);

                string timeZoneId = (await mGeneralSettingService.GetGeneralSettingAsync()).TimeZoneId.Replace(" ", "_");

                int currentSubjobIndex = 0;
                logger.Info($"Try to create sub jobs for main job : {jobId}");
                foreach (RMSPTreeNode site in availableSites)
                {
                    tempList.Add(site);
                    if (tempList.Count == RMGlobalConfiguration.AppConfig.NodeCountInSubJob && !jobType.In(runInOneSubJobList))
                    {
                        string subJobId = this.GenerateSubJobId(jobId, currentSubjobIndex);
                        var cmdPrex = string.Empty;
                        if (jobType == JobType.CreateAndDestroyedFileReport)
                        {
                            cmdPrex = string.Format("{0} {1} {2} {3} {4} {5}", profileId.ToString(), dto.IsCreated, dto.IsDestoryed, dto.StartTime.ToString("yyyy/MM/dd"), dto.EndTime.ToString("yyyy/MM/dd"), timeZoneId);
                        }
                        else if (jobType == JobType.RestoreReport)
                        {
                            cmdPrex = string.Format("{0} {1} {2} {3}", profileId.ToString(), dto.StartTime.ToString("yyyy/MM/dd"), dto.EndTime.ToString("yyyy/MM/dd"), timeZoneId);
                        }
                        else
                        {
                            cmdPrex = string.Format("{0} {1} {2}", profileId.ToString(), IsOrphanedTermReport, isRetiredTermReport); ;
                        }
                        string cmdLine = "{0} {1} " + cmdPrex;
                        var commandLine = string.Format(cmdLine, jobType, subJobId);
                        CreateSubJob(subJobId, jobId, jobType, subJobCount, tempList, cmdLine, currentSubjobIndex < subJobCountInConfigFile);
                        logger.Info($"subJobId : {subJobId}, jobType : {jobType}, subJobCount : {subJobCount}, currentSubjobIndex: {currentSubjobIndex}, subJobCountInConfigFile: {subJobCountInConfigFile}, send now : {currentSubjobIndex < subJobCountInConfigFile}");

                        if (currentSubjobIndex < subJobCountInConfigFile)  //一次只发两个子job, 后续在JobInfoUpdater中触发
                        {
                            JobQueueMessage message = new JobQueueMessage()
                            {
                                JobId = subJobId,
                                JobType = jobType,
                                CommandLine = commandLine
                            };
                            mJobQueueService.HandleMessage(message);
                        }
                        tempList.Clear();
                        currentSubjobIndex++;
                    }
                }
                if (tempList.Count > 0)
                {
                    string subJobId = this.GenerateSubJobId(jobId, currentSubjobIndex);
                    var cmdPrex = string.Empty;
                    if (jobType == JobType.CreateAndDestroyedFileReport)
                    {
                        cmdPrex = string.Format("{0} {1} {2} {3} {4} {5}", profileId.ToString(), dto.IsCreated, dto.IsDestoryed, dto.StartTime.ToString("yyyy/MM/dd"), dto.EndTime.ToString("yyyy/MM/dd"), timeZoneId);
                    }
                    else if (jobType == JobType.RestoreReport)
                    {
                        cmdPrex = string.Format("{0} {1} {2} {3}", profileId.ToString(), dto.StartTime.ToString("yyyy/MM/dd"), dto.EndTime.ToString("yyyy/MM/dd"), timeZoneId);
                    }
                    else
                    {
                        cmdPrex = string.Format("{0} {1} {2}", profileId.ToString(), IsOrphanedTermReport, isRetiredTermReport); ;
                    }
                    string cmdLine = "{0} {1} " + cmdPrex;
                    var commandLine = string.Format(cmdLine, jobType, subJobId);
                    CreateSubJob(subJobId, jobId, jobType, subJobCount, tempList, cmdLine, currentSubjobIndex < subJobCountInConfigFile);
                    logger.Info($"subJobId : {subJobId}, jobType : {jobType}, subJobCount : {subJobCount}, currentSubjobIndex: {currentSubjobIndex}, subJobCountInConfigFile: {subJobCountInConfigFile}, sendnow : {currentSubjobIndex < subJobCountInConfigFile}");
                    if (currentSubjobIndex < subJobCountInConfigFile)  //一次只发两个子job, 后续在JobInfoUpdater中触发
                    {
                        JobQueueMessage message = new JobQueueMessage()
                        {
                            JobId = subJobId,
                            JobType = jobType,
                            CommandLine = commandLine
                        };
                        mJobQueueService.HandleMessage(message);
                    }
                    tempList.Clear();
                }

                logger.Info($"End of creating sub jobs for main job : {jobId}");
                #endregion
            }
            return jobId;
        }
        private string CreateSubJob<T>(string subJobId, string parentId, JobType jobType, int subJobCount, List<T> tempList, string cmdLine, bool sendNow) where T : RMBaseTreeNode<T>
        {
            using (PerformanceScope scope = new PerformanceScope($"CreateSubJob:[{subJobId}]"))
            {
                var tempSPList = new List<RMSPTreeNode>();
                var tempEXOList = new List<RMEXOTreeNode>();
                foreach (var item in tempList)
                {
                    if (item is RMSPTreeNode)
                    {
                        tempSPList.Add(item as RMSPTreeNode);
                    }
                    if (item is RMEXOTreeNode)
                    {
                        tempEXOList.Add(item as RMEXOTreeNode);
                    }
                }
                var subJob = new RMSubJob()
                {
                    Id = subJobId,
                    ParentId = parentId,
                    StartTime = DateTime.UtcNow.Ticks,
                    JobType = (int)jobType,
                    Progress = 0,
                    Status = (int)JobStatus.Wait,
                    Weight = 100d / subJobCount,
                    String1 = cmdLine
                };
                subJob.Runable = sendNow ? RecordsConstants.SubJob_Runnable_Runing : RecordsConstants.SubJob_Runnable_Waiting;
                List<JobType> notNeedContextJobList = new List<JobType>() { JobType.RestoreReport, JobType.OneDriverRestoreReport, JobType.TeamsRestoreReport, JobType.GoogleRestoreReport};
                if (!jobType.In(notNeedContextJobList))
                {
                    if (tempSPList.Count > 0)
                    {
                        subJob.JobContext = new RMJobContext() { JobId = subJobId, Settings = SerializerHelper.SerializeByDataContractSerializer(tempSPList) };
                    }
                    else
                    {
                        subJob.JobContext = new RMJobContext() { JobId = subJobId, Settings = SerializerHelper.SerializeByDataContractSerializer(tempEXOList) };
                    }
                }
                SubJobDao.CreateJob(subJob);
                logger.Info("Create sub job {0} sucessfull, type {1}, weight {2}, Runable {3}", subJob.Id, subJob.JobType, subJob.Weight, subJob.Runable);
                return subJobId;
            }
        }

        private string CreateSubJob(string subJobId, string parentId, JobType jobType, int subJobCount, List<GoogleDriveTreeNodeDto> tempList, string cmdLine, bool sendNow)
        {
            using (PerformanceScope scope = new PerformanceScope($"CreateSubJob:[{subJobId}]"))
            {
                var subJob = new RMSubJob()
                {
                    Id = subJobId,
                    ParentId = parentId,
                    StartTime = DateTime.UtcNow.Ticks,
                    JobType = (int)jobType,
                    Progress = 0,
                    Status = (int)JobStatus.Wait,
                    Weight = 100d / subJobCount,
                    String1 = cmdLine
                };
                subJob.Runable = sendNow ? RecordsConstants.SubJob_Runnable_Runing : RecordsConstants.SubJob_Runnable_Waiting;
                subJob.JobContext = new RMJobContext() { JobId = subJobId, Settings = SerializerHelper.SerializeByDataContractSerializer(tempList) };
                SubJobDao.CreateJob(subJob);
                logger.Info("Create sub job {0} sucessfull, type {1}, weight {2}, Runable {3}", subJob.Id, subJob.JobType, subJob.Weight, subJob.Runable);
                return subJobId;
            }
        }

        private string GenerateSubJobId(string jobId, int currentSubjobIndex)
        {
            return string.Format(jobId + "_{0:D3}", currentSubjobIndex);
        }
        private async Task<string> RunReportJobInMainJobAsync(string jobId, JobType jobType, string jobRunByUser, int profileId, bool IsOrphanedTermReport, bool isRetiredTermReport = false)
        {
            //string jobId = string.Empty;
            //jobId = JobMonitorService.CreateJobWithProfileId(jobType, jobRunByUser, profileId);
            JobQueueMessage message = null;
            if (jobType == JobType.CreateAndDestroyedFileReport
                || jobType == JobType.PhysicalCreateAndDestroyedFileReport
                || jobType == JobType.FSCreateAndDestroyedFileReport
                || jobType == JobType.OneDriveCreateAndDestroyedFileReport
                || jobType == JobType.SPOnPremCreateAndDestroyedFileReport
                || jobType == JobType.BoxCreateAndDestroyedFileReport
                || jobType == JobType.GoogleCreateAndDestroyedFileReport
                || jobType == JobType.TeamsCreateAndDestroyedFileReport)


            {
                RMProfileDto dto = await GetProfileByIdAsync(profileId.ToString());
                message = new JobQueueMessage()
                {
                    JobId = jobId,
                    JobType = jobType,
                    CommandLine = string.Format("{0} {1} {2} {3} {4} {5} {6} {7}", jobType, jobId, profileId.ToString(), dto.IsCreated, dto.IsDestoryed, dto.StartTime.ToString("yyyy/MM/dd"), dto.EndTime.ToString("yyyy/MM/dd"), (await mGeneralSettingService.GetGeneralSettingAsync()).TimeZoneId.Replace(" ", "_")),
                };
            }
            else
            {
                message = new JobQueueMessage()
                {
                    JobId = jobId,
                    JobType = jobType,
                    CommandLine = string.Format("{0} {1} {2} {3} {4}", jobType, jobId, profileId.ToString(), IsOrphanedTermReport, isRetiredTermReport),
                };
            }
            mJobQueueService.HandleMessage(message);
            return jobId;
        }

        public async Task<string> RunGoogleReportAsync(JobType jobType, RMProfileDto dto, string jobId, int profileId, bool IsOrphanedTermReport, bool isRetiredTermReport = false)
        {
            try
            {
                var treeNode = new RMGoogleTreeNode();
                if (jobType == JobType.GoogleBCSTermUsageReport)
                {
                    treeNode = SerializerHelper.DeserializeByDataContractSerializer<RMGoogleTreeNode>(dto.Extension2);
                }
                else
                {
                    treeNode = SerializerHelper.DeserializeByJsonSerializer<RMGoogleTreeNode>(RuleSPTreeUtil.BuildGoogleTreeJsonStr(dto.Extension2));
                }

                // get all need run nodes
                var allAvailableDrives =  GoogleTreeScopeUtil.AssembleAllTreeNodeForGoogleAsync(treeNode).Result;
                var enableNullClassificationContainerIds = GoogleDriveSettingDao.GetAllSettings().Where(s => s.ContainerId == s.ScopeId && s.IsNullClassificationSetting).Select(s => s.ContainerId.ToString()).Distinct().ToList();
                Dictionary<string, string> googleDriveRuleSettingContainers = new();
                List<GoogleDriveTreeNodeDto> availableDrives = new();

                if (enableNullClassificationContainerIds.IsNotNullOrEmpty() && jobType != JobType.GoogleRestoreReport)
                {
                    foreach (var selectedNode in allAvailableDrives)
                    {
                        var containerId = selectedNode.ContainerId;
                        if (enableNullClassificationContainerIds.Contains(containerId))
                        {
                            logger.Info("GGdrive container enable null classification, drive:{0}", selectedNode.ID);
                            if (!googleDriveRuleSettingContainers.ContainsKey(containerId))
                            {
                                googleDriveRuleSettingContainers.Add(containerId, "");
                            }
                            continue;
                        }
                        availableDrives.Add(selectedNode);
                    }
                }
                else
                {
                    availableDrives = allAvailableDrives;
                }
                if (availableDrives.Count == 0)
                {
                    if (googleDriveRuleSettingContainers.IsNotNullOrEmpty())
                    {
                        var containerNames = GoogleNodeDao.GetContainerNames(googleDriveRuleSettingContainers.Keys.ToList());
                        JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, $"RM_EXO_GroupIsRuleSettingAndSkipApplySetting{I18NEntity.Separator}{string.Join(',', containerNames)}");
                        logger.Warn($"GGdrive container enable null classification. Skip run job. Container name:{string.Join(',', googleDriveRuleSettingContainers.Values)}");
                    }
                    else
                    {
                        JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_JM_Report_Skip_NoAvailableDrives");
                        logger.Warn("No drives available for this job. Skip.");
                    }
                    return jobId;
                }

                List<GoogleDriveTreeNodeDto> tempList = new();
                List<JobType> runInOneSubJobList = new List<JobType>() { JobType.GoogleRestoreReport };

                int subJobCount = runInOneSubJobList.Contains(jobType) ? 1 :
                    availableDrives.Count % RMGlobalConfiguration.AppConfig.NodeCountInSubJob == 0 ? availableDrives.Count / RMGlobalConfiguration.AppConfig.NodeCountInSubJob : availableDrives.Count / RMGlobalConfiguration.AppConfig.NodeCountInSubJob + 1;
                SubJobDao.UpdateSubJobCount(jobId, subJobCount);

                int currentSubjobIndex = 0;
                int subJobCountInConfigFile = RMKeyValueDao.GetSubJobCountFromDB((int)jobType);
                string timeZoneId = (await mGeneralSettingService.GetGeneralSettingAsync()).TimeZoneId.Replace(" ", "_");

                foreach (GoogleDriveTreeNodeDto drive in availableDrives)
                {
                    tempList.Add(drive);
                    if (tempList.Count == RMGlobalConfiguration.AppConfig.NodeCountInSubJob && !jobType.In(runInOneSubJobList))
                    {
                        string subJobId = this.GenerateSubJobId(jobId, currentSubjobIndex);
                        var tempCommandLine = string.Empty;
                        if (jobType == JobType.GoogleCreateAndDestroyedFileReport)
                        {
                            tempCommandLine = string.Format("{0} {1} {2} {3} {4} {5}", profileId.ToString(), dto.IsCreated, dto.IsDestoryed, dto.StartTime.ToString("yyyy/MM/dd"), dto.EndTime.ToString("yyyy/MM/dd"), (await mGeneralSettingService.GetGeneralSettingAsync()).TimeZoneId.Replace(" ", "_"));
                        }
                        else
                        {
                            tempCommandLine = string.Format("{0} {1} {2}", profileId.ToString(), IsOrphanedTermReport, isRetiredTermReport);
                        }
                        var commandLineForSubJob = "{0} {1} " + tempCommandLine;
                        var commandLine = string.Format(commandLineForSubJob, jobType, subJobId);
                        CreateSubJob(subJobId, jobId, jobType, subJobCount, tempList, commandLineForSubJob, currentSubjobIndex < subJobCountInConfigFile);
                        if (currentSubjobIndex < subJobCountInConfigFile)
                        {
                            JobQueueMessage message = new JobQueueMessage()
                            {
                                JobId = subJobId,
                                JobType = jobType,
                                CommandLine = commandLine
                            };
                            mJobQueueService.HandleMessage(message);
                        }
                        tempList.Clear();
                        currentSubjobIndex++;
                    }
                }
                if (tempList.Count > 0)
                {
                    string subJobId = this.GenerateSubJobId(jobId, currentSubjobIndex);
                    var tempCommandLine = string.Empty;
                    if (jobType == JobType.GoogleCreateAndDestroyedFileReport)
                    {
                        tempCommandLine = string.Format("{0} {1} {2} {3} {4} {5}", profileId.ToString(), dto.IsCreated, dto.IsDestoryed, dto.StartTime.ToString("yyyy/MM/dd"), dto.EndTime.ToString("yyyy/MM/dd"), (await mGeneralSettingService.GetGeneralSettingAsync()).TimeZoneId.Replace(" ", "_"));
                    }
                    else if (jobType == JobType.GoogleRestoreReport)
                    {
                        tempCommandLine = string.Format("{0} {1} {2} {3}", profileId.ToString(), dto.StartTime.ToString("yyyy/MM/dd"), dto.EndTime.ToString("yyyy/MM/dd"), timeZoneId);
                    }
                    else
                    {
                        tempCommandLine = string.Format("{0} {1} {2}", profileId.ToString(), IsOrphanedTermReport, isRetiredTermReport);
                    }
                    var commandLineForSubJob = "{0} {1} " + tempCommandLine;
                    var commandLine = string.Format(commandLineForSubJob, jobType, subJobId);
                    CreateSubJob(subJobId, jobId, jobType, subJobCount, tempList, commandLineForSubJob, currentSubjobIndex < subJobCountInConfigFile);
                    if (currentSubjobIndex < subJobCountInConfigFile)
                    {
                        JobQueueMessage message = new()
                        {
                            JobId = subJobId,
                            JobType = jobType,
                            CommandLine = commandLine
                        };
                        mJobQueueService.HandleMessage(message);
                    }
                    tempList.Clear();
                }

            }
            catch (Exception e)
            {
                logger.Error($"Google Report error: {e}");
                throw;
            }
            return jobId;
        }

        private async Task<string> RunOneDriveReportJobAsync(string jobId, JobType jobType, string jobRunByUser, int profileId, bool IsOrphanedTermReport, RMProfileDto dto, int subJobCountInConfigFile, bool isRetiredTermReport = false)
        {
            if (jobType == JobType.AvailableSpaceReport)
            {
                return await RunReportJobInMainJobAsync(jobId, jobType, jobRunByUser, profileId, IsOrphanedTermReport, isRetiredTermReport);
            }

            List<RMSPTreeNode> allAvailableSites = await AssembleRunableSitesAsync(dto, RMBrowseTreeNodeSourceType.SkyDrivePro, jobType != JobType.RestoreReport && jobType != JobType.OneDriverRestoreReport);
            var enableNullClassificationGroupIds = OneDriveSettingDao.LoadAllSetting().Where(s => s.SiteGroupId == s.ScopeId && s.IsNullClassificationSetting).Select(s => s.SiteGroupId.ToString()).ToList();
            Dictionary<string, string> OneDriveRuleSettingContainers = new Dictionary<string, string>();
            List<RMSPTreeNode> availableSites = new List<RMSPTreeNode>();
            if(jobType != JobType.RestoreReport && jobType != JobType.OneDriverRestoreReport)
            {
                allAvailableSites = allAvailableSites.Where(site => site.IsOrphenOneDrive != true).ToList();
            }

            if (jobType != JobType.RestoreReport && jobType != JobType.OneDriverRestoreReport
                && enableNullClassificationGroupIds != null && enableNullClassificationGroupIds.Count > 0)
            {
                foreach (var selectedNode in allAvailableSites)
                {
                    var groupNode = selectedNode.GetGroupNode();
                    if (enableNullClassificationGroupIds.Contains(groupNode.SPObjectId))
                    {
                        logger.Info("Onedrive group enable null classification, site:{0}", selectedNode.Name);
                        if (!OneDriveRuleSettingContainers.ContainsKey(groupNode.SPObjectId))
                        {
                            OneDriveRuleSettingContainers.Add(groupNode.SPObjectId, GetSPContainerName(groupNode));
                        }
                        continue;
                    }
                    availableSites.Add(selectedNode);
                }
            }
            else
            {
                availableSites = allAvailableSites;
            }
            //TreeNode Empty, Job空跑Skipped.
            if (availableSites.Count == 0)
            {
                if (OneDriveRuleSettingContainers != null && OneDriveRuleSettingContainers.Count > 0)
                {
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Finished, $"RM_EXO_GroupIsRuleSettingAndSkipApplySetting{I18NEntity.Separator}{string.Join(',', OneDriveRuleSettingContainers.Values)}");
                    logger.Warn($"Onedrive group enable null classification. Skip run job. Group name:{string.Join(',', OneDriveRuleSettingContainers.Values)}");
                }
                else
                {
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_JM_Report_Skip_NoAvailableSites");
                    logger.Warn("No sites available for this job. Skip.");
                }
                return jobId;
            }

            List<RMSPTreeNode> tempList = new List<RMSPTreeNode>();
            List<JobType> runInOneSubJobList = new List<JobType>() { JobType.OneDriverRestoreReport };

            int subJobCount = runInOneSubJobList.Contains(jobType) ? 1 :
                availableSites.Count % RMGlobalConfiguration.AppConfig.NodeCountInSubJob == 0 ? availableSites.Count / RMGlobalConfiguration.AppConfig.NodeCountInSubJob : availableSites.Count / RMGlobalConfiguration.AppConfig.NodeCountInSubJob + 1;

            SubJobDao.UpdateSubJobCount(jobId, subJobCount);

            string timeZoneId = (await mGeneralSettingService.GetGeneralSettingAsync()).TimeZoneId.Replace(" ", "_");

            int currentSubjobIndex = 0;
            foreach (RMSPTreeNode site in availableSites)
            {
                tempList.Add(site);
                if (tempList.Count == RMGlobalConfiguration.AppConfig.NodeCountInSubJob && !jobType.In(runInOneSubJobList))
                {
                    string subJobId = this.GenerateSubJobId(jobId, currentSubjobIndex);
                    var cmdPrex = string.Empty;
                    if (jobType == JobType.CreateAndDestroyedFileReport
                        || jobType == JobType.OneDriveCreateAndDestroyedFileReport)
                    {
                        cmdPrex = string.Format("{0} {1} {2} {3} {4} {5}", profileId.ToString(), dto.IsCreated, dto.IsDestoryed, dto.StartTime.ToString("yyyy/MM/dd"), dto.EndTime.ToString("yyyy/MM/dd"), timeZoneId);
                    }
                    else if (jobType == JobType.OneDriverRestoreReport)
                    {
                        cmdPrex = string.Format("{0} {1} {2} {3}", profileId.ToString(), dto.StartTime.ToString("yyyy/MM/dd"), dto.EndTime.ToString("yyyy/MM/dd"), timeZoneId);
                    }
                    else
                    {
                        cmdPrex = string.Format("{0} {1} {2}", profileId.ToString(), IsOrphanedTermReport, isRetiredTermReport); ;
                    }
                    string cmdLine = "{0} {1} " + cmdPrex;
                    var commandLine = string.Format(cmdLine, jobType, subJobId);
                    CreateSubJob(subJobId, jobId, jobType, subJobCount, tempList, cmdLine, currentSubjobIndex < subJobCountInConfigFile);
                    if (currentSubjobIndex < subJobCountInConfigFile)  //一次只发两个子job, 后续在JobInfoUpdater中触发
                    {
                        JobQueueMessage message = new JobQueueMessage()
                        {
                            JobId = subJobId,
                            JobType = jobType,
                            CommandLine = commandLine
                        };
                        mJobQueueService.HandleMessage(message);
                    }
                    tempList.Clear();
                    currentSubjobIndex++;
                }
            }
            if (tempList.Count > 0)
            {
                string subJobId = this.GenerateSubJobId(jobId, currentSubjobIndex);
                var cmdPrex = string.Empty;
                if (jobType == JobType.CreateAndDestroyedFileReport
                    || jobType == JobType.OneDriveCreateAndDestroyedFileReport)
                {
                    cmdPrex = string.Format("{0} {1} {2} {3} {4} {5}", profileId.ToString(), dto.IsCreated, dto.IsDestoryed, dto.StartTime.ToString("yyyy/MM/dd"), dto.EndTime.ToString("yyyy/MM/dd"), timeZoneId);
                }
                else if (jobType == JobType.OneDriverRestoreReport)
                {
                    cmdPrex = string.Format("{0} {1} {2} {3}", profileId.ToString(), dto.StartTime.ToString("yyyy/MM/dd"), dto.EndTime.ToString("yyyy/MM/dd"), timeZoneId);
                }
                else
                {
                    cmdPrex = string.Format("{0} {1} {2}", profileId.ToString(), IsOrphanedTermReport, isRetiredTermReport); ;
                }
                string cmdLine = "{0} {1} " + cmdPrex;
                var commandLine = string.Format(cmdLine, jobType, subJobId);
                CreateSubJob(subJobId, jobId, jobType, subJobCount, tempList, cmdLine, currentSubjobIndex < subJobCountInConfigFile);
                if (currentSubjobIndex < subJobCountInConfigFile)  //一次只发两个子job, 后续在JobInfoUpdater中触发
                {
                    JobQueueMessage message = new JobQueueMessage()
                    {
                        JobId = subJobId,
                        JobType = jobType,
                        CommandLine = commandLine
                    };
                    mJobQueueService.HandleMessage(message);
                }
                tempList.Clear();
            }
            return jobId;
        }

        private async Task<string> RunTeamsReportJobAsync(string jobId, JobType jobType, string jobRunByUser, int profileId, bool IsOrphanedTermReport, RMProfileDto dto, int subJobCountInConfigFile, bool isRetiredTermReport = false)
        {

            List<RMSPTreeNode> availableSites = await AssembleRunableSitesAsync(dto, RMBrowseTreeNodeSourceType.Teams);

            if (availableSites.Count == 0)
            {
                JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_JM_Report_Skip_NoAvailableSites");
                logger.Warn("No sites available for this job. Skip.");
                return jobId;
            }

            List<string> groupIds = availableSites.Select(s => s.GetGroupNode().Id).ToList();
            if (!TeamsSettingDao.CheckGroupSettingExist(groupIds) && !JobReportUtility.CheckInSOReportTypes((int)jobType))
            {
                JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_JM_Report_Skip_ContainerNoConfigSetting");
                logger.Warn("The Container Settings of the selected site are not configured");
               return jobId;
            }

            List<RMSPTreeNode> tempList = new List<RMSPTreeNode>();

            List<JobType> runInOneSubJobList = new List<JobType>() { JobType.TeamsRestoreReport };
            int subJobCount = runInOneSubJobList.Contains(jobType) ? 1 :
                availableSites.Count % RMGlobalConfiguration.AppConfig.NodeCountInSubJob == 0 ? availableSites.Count / RMGlobalConfiguration.AppConfig.NodeCountInSubJob : availableSites.Count / RMGlobalConfiguration.AppConfig.NodeCountInSubJob + 1;

            SubJobDao.UpdateSubJobCount(jobId, subJobCount);

            string timeZoneId = (await mGeneralSettingService.GetGeneralSettingAsync()).TimeZoneId.Replace(" ", "_");


            int currentSubjobIndex = 0;
            logger.Info($"Try to create sub jobs for main job : {jobId}");
            foreach (RMSPTreeNode site in availableSites)
            {
                tempList.Add(site);
                if (tempList.Count == RMGlobalConfiguration.AppConfig.NodeCountInSubJob && !jobType.In(runInOneSubJobList))
                {
                    string subJobId = this.GenerateSubJobId(jobId, currentSubjobIndex);
                    var cmdPrex = string.Empty;
                    if (jobType == JobType.TeamsCreateAndDestroyedFileReport)
                    {
                        cmdPrex = string.Format("{0} {1} {2} {3} {4} {5}", profileId.ToString(), dto.IsCreated, dto.IsDestoryed, dto.StartTime.ToString("yyyy/MM/dd"), dto.EndTime.ToString("yyyy/MM/dd"), timeZoneId);
                    }
                    else if (jobType == JobType.TeamsRestoreReport)
                    {
                        cmdPrex = string.Format("{0} {1} {2} {3}", profileId.ToString(), dto.StartTime.ToString("yyyy/MM/dd"), dto.EndTime.ToString("yyyy/MM/dd"), timeZoneId);
                    }
                    else
                    {
                        cmdPrex = string.Format("{0} {1} {2}", profileId.ToString(), IsOrphanedTermReport, isRetiredTermReport); ;
                    }
                    string cmdLine = "{0} {1} " + cmdPrex;
                    var commandLine = string.Format(cmdLine, jobType, subJobId);
                    CreateSubJob(subJobId, jobId, jobType, subJobCount, tempList, cmdLine, currentSubjobIndex < subJobCountInConfigFile);
                    logger.Info($"subJobId : {subJobId}, jobType : {jobType}, subJobCount : {subJobCount}, currentSubjobIndex: {currentSubjobIndex}, subJobCountInConfigFile: {subJobCountInConfigFile}, send now : {currentSubjobIndex < subJobCountInConfigFile}");

                    if (currentSubjobIndex < subJobCountInConfigFile)  //一次只发两个子job, 后续在JobInfoUpdater中触发
                    {
                        JobQueueMessage message = new JobQueueMessage()
                        {
                            JobId = subJobId,
                            JobType = jobType,
                            CommandLine = commandLine
                        };
                        mJobQueueService.HandleMessage(message);
                    }
                    tempList.Clear();
                    currentSubjobIndex++;
                }
            }
            if (tempList.Count > 0)
            {
                string subJobId = this.GenerateSubJobId(jobId, currentSubjobIndex);
                var cmdPrex = string.Empty;
                if (jobType == JobType.TeamsCreateAndDestroyedFileReport)
                {
                    cmdPrex = string.Format("{0} {1} {2} {3} {4} {5}", profileId.ToString(), dto.IsCreated, dto.IsDestoryed, dto.StartTime.ToString("yyyy/MM/dd"), dto.EndTime.ToString("yyyy/MM/dd"), timeZoneId);
                }
                else if (jobType == JobType.TeamsRestoreReport)
                {
                    cmdPrex = string.Format("{0} {1} {2} {3}", profileId.ToString(), dto.StartTime.ToString("yyyy/MM/dd"), dto.EndTime.ToString("yyyy/MM/dd"), timeZoneId);
                }
                else
                {
                    cmdPrex = string.Format("{0} {1} {2}", profileId.ToString(), IsOrphanedTermReport, isRetiredTermReport); ;
                }
                string cmdLine = "{0} {1} " + cmdPrex;
                var commandLine = string.Format(cmdLine, jobType, subJobId);
                CreateSubJob(subJobId, jobId, jobType, subJobCount, tempList, cmdLine, currentSubjobIndex < subJobCountInConfigFile);
                logger.Info($"subJobId : {subJobId}, jobType : {jobType}, subJobCount : {subJobCount}, currentSubjobIndex: {currentSubjobIndex}, subJobCountInConfigFile: {subJobCountInConfigFile}, sendnow : {currentSubjobIndex < subJobCountInConfigFile}");
                if (currentSubjobIndex < subJobCountInConfigFile)  //一次只发两个子job, 后续在JobInfoUpdater中触发
                {
                    JobQueueMessage message = new JobQueueMessage()
                    {
                        JobId = subJobId,
                        JobType = jobType,
                        CommandLine = commandLine
                    };
                    mJobQueueService.HandleMessage(message);
                }
                tempList.Clear();
            }

            logger.Info($"End of creating sub jobs for main job : {jobId}");
            return jobId;
        }


        private string GetSPContainerName(RMSPTreeNode selectedNode)
        {
            if (selectedNode.Level == (int)NodeLevel.WebApplication)
            {
                return selectedNode.Name;
            }
            else
            {
                return GetSPContainerName(selectedNode.Parent);
            }
        }
        public async Task<List<RMSPTreeNode>> AssembleSitesAsync(RMProfileDto dto, RMBrowseTreeNodeSourceType type, bool needValidSiteExist = true)
        {
            return await AssembleRunableSitesAsync(dto, type, needValidSiteExist);
        }
        private async Task<List<RMSPTreeNode>> AssembleRunableSitesAsync(RMProfileDto dto, RMBrowseTreeNodeSourceType type = RMBrowseTreeNodeSourceType.SharepointOnline, bool needValidSiteExist = true)
        {
            List<RMSPTreeNode> nodeList = new List<RMSPTreeNode>();
            if (!string.IsNullOrEmpty(dto.Extension2))
            {
                logger.Info($"dto.Extension2 is not null or empty, json:{dto.Extension2}");
                var farmNode = this.GetFarmSPTreeNode(dto.Extension2);
                //nodeList = this.AssembleAllTreeNode(farmNode, type);
                nodeList = type == RMBrowseTreeNodeSourceType.Teams ? await AssembleAllTeamsTreeNodeAsync(farmNode) : await this.AssembleAllTreeNodeAsync(farmNode, needValidSiteExist, type);
            }
            return nodeList;
        }

        private async Task<List<RMSPTreeNode>> AssembleRunnableTeamsAsync(RMProfileDto dto)
        {
            List<RMSPTreeNode> nodeList = new List<RMSPTreeNode>();
            if (!string.IsNullOrEmpty(dto.Extension2))
            {
                var farmNode = this.GetFarmSPTreeNode(dto.Extension2);
                //nodeList = this.AssembleAllTreeNode(farmNode, type);
                nodeList = await this.AssembleAllTreeNodeAsync(farmNode);
            }
            return nodeList;
        }

        private async Task<List<RMSPTreeNode>> AssembleAllTreeNodeAsync(RMSPTreeNode farmNode)
        {
            List<RMSPTreeNode> treeNodes = new List<RMSPTreeNode>();
            foreach (var container in farmNode.Children)
            {
                if (container.CheckNumber == 1)
                {
                    List<RMSPTreeNode> allTeams = await RMSPTreeService.BrowseAsync(container, true, RMBrowseTreeNodeSourceType.Teams);
                    foreach (var teams in allTeams)
                    {
                        List<RMSPTreeNode> spoCollection = await RMSPTreeService.BrowseAsync(teams, true, RMBrowseTreeNodeSourceType.Teams);
                        List<RMSPTreeNode> allSiteCollectionsUnderTeams = await RMSPTreeService.BrowseAsync(spoCollection[0], true, RMBrowseTreeNodeSourceType.Teams);
                        allSiteCollectionsUnderTeams.ForEach(a => a.CheckNumber = 1);
                        treeNodes.AddRange(allSiteCollectionsUnderTeams.Select(GetCloneSiteCollection));
                    }
                    logger.Info("The current Container {0} is fully selected, and all Teams, including newly created ones, are browsed out", container.Name);
                }
                else
                {
                    logger.Info("The current Container {0} is in normal selection state", container.Name);
                    if (container.Children != null)
                    {
                        foreach (var teams in container.Children)
                        {
                            if (HasSelectNode(teams) && SiteCollectionUnderTeamsExists(teams))
                            {
                                treeNodes.Add(GetCloneSite(teams,teams));
                            }
                            else
                            {
                                logger.Debug("No select node in {0}", teams.Name);
                            }
                        }
                    }
                }
            }
            return treeNodes;
        }


        private async Task<List<RMSPTreeNode>> AssembleAllTreeNodeAsync(RMSPTreeNode farmNode, bool needValidSiteExist, RMBrowseTreeNodeSourceType type = RMBrowseTreeNodeSourceType.SharepointOnline)
        {
            List<RMSPTreeNode> treeNodes = new List<RMSPTreeNode>();
            foreach (var group in farmNode.Children)
            {
                List<RMSPTreeNode> allSiteUnderGroup = await RMSPTreeService.BrowseAsync(group, true, type);
                logger.Info($"Browse group {group.Name}, id:{group.Id} get all site count:{allSiteUnderGroup?.Count()}, json:{SerializerHelper.SerializeByJsonConvert(allSiteUnderGroup, true)}");
                if (group.CheckNumber == 1)
                {
                    allSiteUnderGroup.ForEach(a => a.CheckNumber = 1);
                    treeNodes.AddRange(allSiteUnderGroup.Select(o => GetCloneSite(o)));
                    logger.Info($"The current Container {group.Name}, id:{group.Id} is fully selected, and all Site Collections, including newly created ones, are browsed out");
                }
                else if (group.CheckNumber == 2)
                {
                    if (group.Children != null)
                    {
                        logger.Info($"The current Container {group.Name},id:{group.Id} check num is 2.children count:{group?.Children?.Count()}, json:{SerializerHelper.SerializeByJsonConvert(group.Children, true)}");
                        foreach (var site in group.Children)
                        {
                            if (HasSelectNode(site) && SiteExists(site))
                            {
                                var NotSelectSite = allSiteUnderGroup.Where(o => o.Id == site.Id).First();
                                NotSelectSite.Children = site.Children;
                                NotSelectSite.ChildrenIds = site.ChildrenIds;
                                NotSelectSite.ChildrenCount = site.ChildrenCount;
                                logger.Info($"The current Container {group.Name}, id:{group.Id} is in semi-selected state. Special processing node {site.Name}, id:{site.Id} ,Keep the children below it");
                            }
                            else
                            {
                                allSiteUnderGroup.Remove(allSiteUnderGroup.Where(o => o.Id == site.Id).FirstOrDefault());
                                allSiteUnderGroup.ForEach(a => a.CheckNumber = 1);
                                logger.Info($"The current Container {group.Name}, id:{group.Id} is in semi-selected state. Removed Node is {site.Name}, id:{site.Id}");
                            }
                        }
                        treeNodes.AddRange(allSiteUnderGroup.Select(o => GetCloneSite(o,group)));
                    }
                    else
                    {
                        logger.Info($"The current Container {group.Name},id:{group.Id} check num is 2, but children is null");
                    }
                }
                else
                {
                    logger.Info($"The current Container {group.Name},id:{group.Id} is in normal selection state, children count:{group?.Children?.Count()}");
                    if (group.Children != null)
                    {
                        foreach (var site in group.Children)
                        {
                            if (!needValidSiteExist || (HasSelectNode(site) && SiteExists(site)))
                            {
                                treeNodes.Add(GetCloneSite(site,group));
                                logger.Info($"Add site {site.Name}, id:{site.Id} under group {group.Name}, id:{group.Id} to run list");
                            }
                            else
                            {
                                logger.Debug($"No select node in {site.Name}, id:{site.Id}");
                            }
                        }
                    }
                }
            }
            return treeNodes;
        }

        private async Task<List<RMSPTreeNode>> AssembleAllTeamsTreeNodeAsync(RMSPTreeNode farmNode)
        {
            List<RMSPTreeNode> treeNodes = new List<RMSPTreeNode>();
            logger.Info($"Start to assemble all teams tree node, node:{SerializerHelper.SerializeByJsonConvert(farmNode, true)}");
            foreach (var group in farmNode.Children)
            {
                List<RMSPTreeNode> allTeamsUnderGroup = await RMTeamsTreeService.BrowseAsync(group, true);
                logger.Info("Browse group {0}, get all teams count:{1}, json:{2}", group.Name, allTeamsUnderGroup?.Count(), SerializerHelper.SerializeByJsonConvert(allTeamsUnderGroup, true));
                if (group.CheckNumber == 1)
                {
                    foreach(var teamsNode in allTeamsUnderGroup)
                    {
                        RMSPTreeNode virtualSiteCollectionNode = (await RMTeamsTreeService.BrowseAsync(teamsNode, true)).FirstOrDefault();
                        if (virtualSiteCollectionNode == null) continue;
                        List<RMSPTreeNode> allSiteCollectionUnderTeams = await RMTeamsTreeService.BrowseAsync(virtualSiteCollectionNode, true);

                        allSiteCollectionUnderTeams.ForEach(a => a.CheckNumber = 1);
                        treeNodes.AddRange(allSiteCollectionUnderTeams.Select(o => GetCloneTeamsSite(o, virtualSiteCollectionNode)));
                    }
                    logger.Info("The current Container {0} is fully selected, all teams node level and all sites node level, including newly created ones, are browsed out", group.Name);
                }
                else if (group.CheckNumber == 2)
                {
                    logger.Info("The current Container {0} is in semi-selected state CheckNumber == 2, children count:{1}", group.Name, group.Children?.Count());
                    if (group.Children != null)
                    {
                        foreach (var teams in group.Children)
                        {
                            if (HasSelectNode(teams) && TeamsExists(teams))
                            {
                                if (teams.Parent == null) teams.Parent = group;
                                await AssembleTeamsNodeAsync(teams, treeNodes);
                                allTeamsUnderGroup.Remove(allTeamsUnderGroup.Where(o => o.Id == teams.Id).FirstOrDefault());
                                logger.Info("The current group {0} is in semi-selected state. Special processing node {1} ,Keep the children below it", group.Name, teams.Name);
                            }
                            else
                            {
                                allTeamsUnderGroup.Remove(allTeamsUnderGroup.Where(o => o.Id == teams.Id).FirstOrDefault());
                                logger.Info("The current group {0} is in semi-selected state. Removed Node is {1}", group.Name, teams.Name);
                            }
                        }
                        foreach(var teams in allTeamsUnderGroup)
                        {
                            teams.CheckNumber = 1;
                            await AssembleTeamsNodeAsync(teams, treeNodes);
                        }
                    }
                }
                else
                {
                    logger.Info("The current Container {0} is in normal selection state", group.Name);
                    if (group.Children != null)
                    {
                        foreach (var teams in group.Children)
                        {
                            if (TeamsExists(teams))
                            {
                                if (teams.Parent == null) teams.Parent = group;
                                await AssembleTeamsNodeAsync(teams, treeNodes);
                            }
                            else
                            {
                                logger.Debug("No select node in {0}", teams.Name);
                            }
                        }
                    }
                }
            }
            return treeNodes;
        }

        private async Task AssembleTeamsNodeAsync(RMSPTreeNode teams, List<RMSPTreeNode> treeNodes)
        {
            RMSPTreeNode virtualSiteCollectionNode = (await RMTeamsTreeService.BrowseAsync(teams, true)).FirstOrDefault();
            if (virtualSiteCollectionNode == null) return;
            List<RMSPTreeNode> allSiteCollectionUnderTeams = await RMTeamsTreeService.BrowseAsync(virtualSiteCollectionNode, true);
            if (teams.CheckNumber == 1)
            {
                allSiteCollectionUnderTeams.ForEach(a => a.CheckNumber = 1);
                treeNodes.AddRange(allSiteCollectionUnderTeams.Select(o => GetCloneTeamsSite(o, virtualSiteCollectionNode)));
                logger.Info("The current teams {0} is fully selected, all sites node level, including newly created ones, are browsed out", teams.Name);
            }
            else if (teams.CheckNumber == 2)
            {
                if (teams.Children != null && teams.Children.Count > 0 && teams.Children[0].Children != null)
                {
                    foreach (var site in teams.Children[0].Children)
                    {
                        if (HasSelectNode(site) && SiteExists(site))
                        {
                            var NotSelectSite = allSiteCollectionUnderTeams.Where(o => o.Id == teams.Id).First();
                            NotSelectSite.Children = site.Children;
                            NotSelectSite.ChildrenIds = site.ChildrenIds;
                            NotSelectSite.ChildrenCount = site.ChildrenCount;
                            logger.Info("The current teams {0} is in semi-selected state. Special processing node {1} ,Keep the children below it", teams.Name, site.Name);
                        }
                        else
                        {
                            allSiteCollectionUnderTeams.Remove(allSiteCollectionUnderTeams.Where(o => o.Id == teams.Id).FirstOrDefault());
                            allSiteCollectionUnderTeams.ForEach(a => a.CheckNumber = 1);
                            logger.Info("The current teams {0} is in semi-selected state. Removed Node is {1}", teams.Name, site.Name);
                        }
                    }
                    treeNodes.AddRange(allSiteCollectionUnderTeams.Select(o => GetCloneTeamsSite(o, teams)));
                }
            }
            else
            {
                logger.Info("The current Container {0} is in normal selection state", teams.Name);
                if (teams.Children != null)
                {
                    foreach (var site in teams.Children.First().Children)
                    {
                        if (SiteExists(site) && HasSelectNode(site))
                        {
                            if (site.Parent == null) site.Parent = virtualSiteCollectionNode;
                            treeNodes.Add(GetCloneTeamsSite(site, teams));
                        }
                        else
                        {
                            logger.Debug("No select node in {0}", teams.Name);
                        }
                    }
                }
            }
        }

        private bool TeamsExists(RMSPTreeNode node)
        {
            if (node.Level == (int)NodeLevel.Office365GroupEntire)
            {
                var (teams, _) = RMRemoteNodeDao.GetTeamsGroupAndChannelsCollectionByTeamsId(node.SPObjectId);
                if (teams == null)
                {
                    logger.Warn("Teams not exits, {0},{1}", node.Name, node.Id);
                    return false;
                }
            }
            return true;
        }

        private bool SiteExists(RMSPTreeNode node)
        {
            if (node.Level == (int)NodeLevel.SiteCollection)
            {
                var site = RMRemoteNodeDao.GetRemoteSiteCollectionById(node.SPObjectId);
                if (site == null)
                {
                    logger.Warn("Site not exits, {0},{1}", node.Name, node.Id);
                    return false;
                }
            }
            return true;
        }

        private bool SiteCollectionUnderTeamsExists(RMSPTreeNode node)
        {
            while (node is { Level: (int)NodeLevel.SiteCollection })
            {
                var site = RMRemoteNodeDao.GetRemoteSiteCollectionById(node.SPObjectId);
                if (site == null)
                {
                    logger.Warn("Site not exits, {0},{1}", node.Name, node.Id);
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 原site中带了parent以及其children等其他大量无关的sites，所以需要清除，否则一旦在大数据环境下， tree会很大，导致插入数据库非常慢, 参考RECO-6200
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        private RMSPTreeNode GetCloneSite(RMSPTreeNode site, RMSPTreeNode container = null)
        {
            var tmpSite = site.Clone();
            if(site.Parent != null)
            {
                tmpSite.Parent = site.Parent == null ? null : site.Parent.Clone();
                tmpSite.Parent.Children = new List<RMSPTreeNode> { tmpSite };
                tmpSite.Parent.ChildrenIds = new List<string> { tmpSite.Id };
                tmpSite.Parent.Parent = null;
                RemoveParentUnderSite(tmpSite);
            }
            else
            {
                tmpSite.Parent = container;
                tmpSite.Parent.Children = new List<RMSPTreeNode> { tmpSite };
                tmpSite.Parent.ChildrenIds = new List<string> { tmpSite.Id };
                tmpSite.Parent.Parent = null;
                RemoveParentUnderSite(tmpSite);
            }
            //在run job时，会用到site节点的parent中的属性，因此这里保留了site的parent，但是去除了parent中除了此site外其他的子节点
            return tmpSite;
        }

        private RMSPTreeNode GetCloneSiteCollection(RMSPTreeNode site)
        {
            var tempSiteCollection = site.Clone();
            var result = site.Clone();
            while (tempSiteCollection != null && tempSiteCollection.Level != (int) NodeLevel.WebApplication)
            {
                result.Parent = site.Parent?.Clone();
                result.Parent.Children = [tempSiteCollection];
                result.Parent.ChildrenIds = [tempSiteCollection.Id];
                tempSiteCollection = tempSiteCollection.Parent;
            }
            return result;
        }

        private RMSPTreeNode GetCloneTeamsSite(RMSPTreeNode site, RMSPTreeNode parent = null)
        {
            var tmpSite = site.Clone();
            if (site.Parent != null)
            {
                tmpSite.Parent = site.Parent == null ? null : site.Parent.Clone();
                tmpSite.Parent.Children = new List<RMSPTreeNode> { tmpSite };
                tmpSite.Parent.ChildrenIds = new List<string> { tmpSite.Id };
                tmpSite.Parent.Parent.Parent.Parent = null;
                RemoveParentUnderSite(tmpSite);
            }
            else
            {
                tmpSite.Parent = parent;
                tmpSite.Parent.Children = new List<RMSPTreeNode> { tmpSite };
                tmpSite.Parent.ChildrenIds = new List<string> { tmpSite.Id };
                tmpSite.Parent.Parent.Parent.Parent = null;
                RemoveParentUnderSite(tmpSite);
            }
            //在run job时，会用到site节点的parent中的属性，因此这里保留了site的parent，但是去除了parent中除了此site外其他的子节点
            return tmpSite;
        }

        /// <summary>
        /// 对于site以下的节点，run job时，实际上用不到parent属性，因此给清空，防止出现tree过大的情况
        /// </summary>
        /// <param name="site"></param>
        private void RemoveParentUnderSite(RMSPTreeNode site)
        {
            if (site == null || site.Children == null) return;
            foreach(var child in site.Children)
            {
                child.Parent = null;
                RemoveParentUnderSite(child);
            }
        }

        /*private List<RMSPTreeNode> GetUnselectNodes(List<RMSPTreeNode> nodes)
        {
            List<RMSPTreeNode> tempNodes = new List<RMSPTreeNode>();
            if (nodes != null && nodes.Count > 0)
            {
                foreach (var site in nodes)
                {
                    if (!HasSelectNode(site))
                    {
                        tempNodes.Add(site);
                    }
                    else
                    {
                        logger.Debug("Node has been selected:{0}", site.Name);
                    }
                }
            }
            return tempNodes;
        }*/

        private bool HasSelectNode(RMSPTreeNode current)
        {
            if (current.CheckNumber != 0)
            {
                return true;
            }
            if (current.Children.IsNullOrEmpty())
            {
                return false;
            }
            else
            {
                foreach (RMSPTreeNode child in current.Children)
                {
                    if (HasSelectNode(child))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public string GetRemoteFarmId()
        {
            string remoteFarmId = string.Empty;
            //var client = new DAOAPIClientV1();
            //remoteFarmId = client.OnlineFarmId();
            return remoteFarmId;
        }







        public async Task<ShowProfilesReportPageInfo> GetProfilesAsync(ShowProfilesReportPageInfo pageInfo)
        {
            var isSPAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.SPOEnduser);
            var isEXOAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.EXOEnduser);
            var isPhyAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.PhysicalAdmin);
            var isFSAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.FSAdmin);
            var isOneDriveAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.OneDriveEnduser);
            var isSPOnPrem = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.SPOnPremEnduser);
            var isTeamsAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.TeamsEndUser);

            var isSOSPAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMSOPermissionMasks.SPOEnduser);
            var isSOOneDriveAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMSOPermissionMasks.OneDriveEnduser);
            var isBoxAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.BoxAdmin);
            var isGoogleAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.GoogleAdmin);

            var isTeamsSOAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMSOPermissionMasks.TeamsAdmin);
            var isTeamsSOEndUser = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMSOPermissionMasks.TeamsEndUser);
            var isEnableJPMCFeature = RMKeyValueDao.IsEnableJPMCFileSystemFeature();

            var sources = new HashSet<SourceFlag>
            {
                SourceFlag.All
            };

            var itemsFilesDueReportTypeSet = new HashSet<int>()
            {
                (int)JobType.DisposalReport
            };
            var createAndDestroyedReportTypeSet = new HashSet<int>()
            {
                (int)JobType.CreateAndDestroyedReport
            };
            var restoreReportTypeSet = new HashSet<int>()
            {
                (int)JobType.None
            };

            if (IsSOReportJobType((int)pageInfo.Type))
            {
                if (isSOSPAdmin)
                {
                    itemsFilesDueReportTypeSet.Add((int)JobType.ItemsFilesDueDisposal);
                    createAndDestroyedReportTypeSet.Add((int)JobType.CreateAndDestroyedFileReport);
                    sources.Add(SourceFlag.SharePoint);
                    restoreReportTypeSet.Add((int)JobType.RestoreReport);
                }

                if (isSOOneDriveAdmin)
                {
                    itemsFilesDueReportTypeSet.Add((int)JobType.OneDriveItemsFilesDueDisposalReport);
                    createAndDestroyedReportTypeSet.Add((int)JobType.OneDriveCreateAndDestroyedFileReport);
                    sources.Add(SourceFlag.OneDrive);
                    restoreReportTypeSet.Add((int)JobType.OneDriverRestoreReport);
                }


                if (isTeamsSOAdmin || isTeamsSOEndUser)
                {
                    itemsFilesDueReportTypeSet.Add((int)JobType.TeamsItemsFilesDueDisposalReport);
                    createAndDestroyedReportTypeSet.Add((int)JobType.TeamsCreateAndDestroyedFileReport);
                    sources.Add(SourceFlag.Teams);
                    restoreReportTypeSet.Add((int)JobType.TeamsRestoreReport);
                }
            }

            if (IsILReportJobType((int)pageInfo.Type))
            {
                if (isSPAdmin)
                {
                    itemsFilesDueReportTypeSet.Add((int)JobType.ItemsFilesDueDisposal);
                    createAndDestroyedReportTypeSet.Add((int)JobType.CreateAndDestroyedFileReport);
                    sources.Add(SourceFlag.SharePoint);
                    restoreReportTypeSet.Add((int)JobType.RestoreReport);
                }

                if (isEXOAdmin)
                {
                    itemsFilesDueReportTypeSet.Add((int)JobType.EXOItemsFilesDueDisposalReport);
                    createAndDestroyedReportTypeSet.Add((int)JobType.EXOCreateAndDestroyedFileReport);
                    sources.Add(SourceFlag.Exchange);
                }

                if (isPhyAdmin)
                {
                    itemsFilesDueReportTypeSet.Add((int)JobType.PhysicalItemsFilesDueDisposalReport);
                    createAndDestroyedReportTypeSet.Add((int)JobType.PhysicalCreateAndDestroyedFileReport);
                    sources.Add(SourceFlag.Physical);
                }

                if (isFSAdmin && !isEnableJPMCFeature)
                {
                    itemsFilesDueReportTypeSet.Add((int)JobType.FSItemsFilesDueDisposal);
                    createAndDestroyedReportTypeSet.Add((int)JobType.FSCreateAndDestroyedFileReport);
                    sources.Add(SourceFlag.FileSystem);
                }

                if (isOneDriveAdmin)
                {
                    itemsFilesDueReportTypeSet.Add((int)JobType.OneDriveItemsFilesDueDisposalReport);
                    createAndDestroyedReportTypeSet.Add((int)JobType.OneDriveCreateAndDestroyedFileReport);
                    sources.Add(SourceFlag.OneDrive);
                    restoreReportTypeSet.Add((int)JobType.OneDriverRestoreReport);
                }

                if (isSPOnPrem)
                {
                    itemsFilesDueReportTypeSet.Add((int)JobType.SPOnPremItemsFilesDueDisposal);
                    createAndDestroyedReportTypeSet.Add((int)JobType.SPOnPremCreateAndDestroyedFileReport);
                    sources.Add(SourceFlag.SharePointOnPrem);
                }

                //Box
                if (isBoxAdmin)
                {
                    itemsFilesDueReportTypeSet.Add((int)JobType.BoxItemsFilesDueDisposalReport);
                    createAndDestroyedReportTypeSet.Add((int)JobType.BoxCreateAndDestroyedFileReport);
                    sources.Add(SourceFlag.Box);
                }

                if (isGoogleAdmin)
                {
                    itemsFilesDueReportTypeSet.Add((int)JobType.GoogleItemsFilesDueDisposalReport);
                    createAndDestroyedReportTypeSet.Add((int)JobType.GoogleCreateAndDestroyedFileReport);
                    sources.Add(SourceFlag.Google);
                    restoreReportTypeSet.Add((int)JobType.GoogleRestoreReport);
                }

                if(isTeamsAdmin)
                {
                    itemsFilesDueReportTypeSet.Add((int)JobType.TeamsItemsFilesDueDisposalReport);
                    createAndDestroyedReportTypeSet.Add((int)JobType.TeamsCreateAndDestroyedFileReport);
                    sources.Add(SourceFlag.Teams);
                    restoreReportTypeSet.Add((int)JobType.TeamsRestoreReport);
                }
            }


            List<Expression> allExpressionList = new List<Expression>();
            ParameterExpression param = Expression.Parameter(typeof(RMProfile), "c");

            allExpressionList.Add(Expression4DynamicQuery.GetEqualExpression(typeof(RMProfile), param, "IsRemoved", false));

            List<Expression> typesExpressionList = new List<Expression>();

            allExpressionList.Add(Expression4DynamicQuery.GetInExpression(typeof(RMProfile), param, "Source", sources.Cast<object>()));

            if (pageInfo.Type == JobType.ItemsFilesDueDisposal)
            {
                typesExpressionList.AddRange(itemsFilesDueReportTypeSet.Select(ty => Expression4DynamicQuery.GetEqualExpression(typeof(RMProfile), param, "Type", ty)));
            }
            else if (pageInfo.Type == JobType.CreateAndDestroyedFileReport)
            {
                typesExpressionList.AddRange(createAndDestroyedReportTypeSet.Select(ty => Expression4DynamicQuery.GetEqualExpression(typeof(RMProfile), param, "Type", ty)));
            }
            else if (pageInfo.Type == JobType.RestoreReport)
            {
                typesExpressionList.AddRange(restoreReportTypeSet.Select(ty => Expression4DynamicQuery.GetEqualExpression(typeof(RMProfile), param, "Type", ty)));
            }
            else if (pageInfo.Type == JobType.SPOActionAuditReport)
            {
                if (isSOSPAdmin || isSPAdmin)
                {
                    typesExpressionList.Add(Expression4DynamicQuery.GetEqualExpression(typeof(RMProfile), param, "Type", (int)JobType.SPOActionAuditReport));
                }
                if (isSOOneDriveAdmin || isOneDriveAdmin)
                {
                    typesExpressionList.Add(Expression4DynamicQuery.GetEqualExpression(typeof(RMProfile), param, "Type", (int)JobType.OneDriveActionAuditReport));
                }
                if (isTeamsSOAdmin || isTeamsSOEndUser)
                {
                    typesExpressionList.Add(Expression4DynamicQuery.GetEqualExpression(typeof(RMProfile), param, "Type", (int)JobType.TeamsActionAuditReport));
                }
            }
            else if (pageInfo.Type == JobType.ArchivedSiteReport
                || (int)pageInfo.Type == JobTypeConstants.SOArchivedSiteReportPageType)
            {
                typesExpressionList.AddRange(JobTypeConstants.ArchivedSiteReportJobTypes
                    .Select(type => Expression4DynamicQuery.GetEqualExpression(typeof(RMProfile), param, "Type", type)));
            }

            if (typesExpressionList.Count > 0)
            {
                allExpressionList.Add(typesExpressionList.Aggregate(Expression.OrElse));
            }

            if (!(await IsAdminAsync() && IsILReportJobType((int)pageInfo.Type))
                && !(await IsSOAdminAsync() && IsSOReportJobType((int)pageInfo.Type)))
            {
                allExpressionList.Add(Expression4DynamicQuery.GetEqualExpression(typeof(RMProfile), param, "CreateProfileLogonUserId", TenantLocalValue.LogonUserId));
            }
            if (!string.IsNullOrEmpty(pageInfo.SearchValue))
            {
                allExpressionList.Add(Expression4DynamicQuery.GetContainsExpression(typeof(RMProfile), param, "Name", pageInfo.SearchValue));
            }

            int totalRecord = 0;
            int temp = (int)pageInfo.Type;
            Expression queryExpr = allExpressionList.Aggregate(Expression.AndAlso);
            var lambda = Expression.Lambda<Func<RMProfile, bool>>(queryExpr, param);
            logger.Info($"GetProfiles: {lambda}");
            List<RMProfile> profiles = profileDAO.GetProfiles(pageInfo.PageIndex, pageInfo.PageSize, out totalRecord, "Modified", pageInfo.IsDesc, lambda);
            pageInfo.Profiles = await profiles.ConvertAllAsync<RMProfile,RMProfileDto>(o => ConvertToProfileDtoAsync(o));
            pageInfo.TotalCount = totalRecord;
            pageInfo.SearchValue = "";
            return pageInfo;
        }

        private bool IsILReportJobType(int jobType)
        {
            if (JobTypeConstants.SPReportTypes.Contains(jobType))
            {
                return true;
            }
            if (JobTypeConstants.EXOReportTypes.Contains(jobType))
            {
                return true;
            }

            if (JobTypeConstants.PhysicalReportTypes.Contains(jobType))
            {
                return true;
            }

            if (JobTypeConstants.FSReportTypes.Contains(jobType))
            {
                return true;
            }

            if (JobTypeConstants.OneDriveReportTypes.Contains(jobType))
            {
                return true;
            }

            if (JobTypeConstants.SPOnPremReportTypes.Contains(jobType))
            {
                return true;
            }

            if (JobTypeConstants.GoogleReportTypes.Contains(jobType))
            {
                return true;
            }

            if (JobTypeConstants.TeamsReportTypes.Contains(jobType))
            {
                return true;
            }

            return false;
        }

        private bool IsSOReportJobType(int jobType)
        {
            if (jobType == JobTypeConstants.SOArchivedSiteReportPageType)
            {
                return true;
            }

            if (JobTypeConstants.SOSPReportTypes.Contains(jobType))
            {
                return true;
            }

            if (JobTypeConstants.SOOneDriveReportTypes.Contains(jobType))
            {
                return true;
            }

            if (JobTypeConstants.SOTeamsReportTypes.Contains(jobType))
            {
                return true;
            }
            return false;
        }

        public int GetPageIndexByProfileId(int profileId)
        {
            return profileDAO.GetPageIndexByProfileId(profileId);
        }

        public async Task<ShowProfilesReportPageInfo> GetAllProfilesAsync(ShowProfilesReportPageInfo pageInfo)
        {
            int totalRecord = 0;
            int temp = (int)pageInfo.Type;
            Expression<Func<RMProfile, bool>> queryExpr = profile => profile.Type == temp;
            if (!string.IsNullOrEmpty(pageInfo.SearchValue))
            {
                queryExpr = profile => profile.Name.Contains(pageInfo.SearchValue) && profile.Type == temp;
            }
            List<RMProfile> profiles = profileDAO.GetProfiles(pageInfo.PageIndex, pageInfo.PageSize, out totalRecord, "Modified", pageInfo.IsDesc, queryExpr);
            pageInfo.Profiles = await profiles.ConvertAllAsync<RMProfile, RMProfileDto>(o => ConvertToProfileDtoAsync(o));
            pageInfo.TotalCount = totalRecord;
            return pageInfo;
        }

        public string GetMetaDataColumnName(Guid webAppId)
        {
            return SPSettingDao.GetMedataColumn(webAppId);
        }

        public async Task<List<TermTreeNode>> GetRATermTreeNodesAsync()
        {
            List<TermTreeNode> groupNodes = new List<TermTreeNode>();
            List<RMTermGroup> termGroups = TermGroupDao.LoadTermGroup(false);
            foreach (var group in termGroups)
            {
                TermTreeNode groupNode = new TermTreeNode()
                {
                    ID = group.UniqueId,
                    Children = new Dictionary<Guid, TermTreeNode>()
                };
                List<RMTermSet> allRMTermSet = await TermSetDAO.LoadTermSetAsync(TermSetType.Business, group.UniqueId);
                foreach (RMTermSet termSet in allRMTermSet)
                {
                    TermTreeNode termSetNode = TermDao.GetRATermSetTree(termSet.UniqueId);
                    if (termSetNode != null)
                    {
                        termSetNode.ParentID = group.UniqueId;
                        groupNode.Children.Add(termSetNode.ID, termSetNode);
                    }
                }
                groupNodes.Add(groupNode);
            }

            return groupNodes;
        }

        public RMSPTreeNode GetFarmSPTreeNode(string ext2)
        {
            //var farmNode = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(ext2);
            var farmNode = SerializerHelper.DeserializeByJsonSerializer<RMSPTreeNode>(ext2, true);
            return farmNode;
        }

        //public RMFSTreeNode GetFarmFSTreeNode(string ext2)
        //{
        //    return SerializerHelper.DeserializeByDataContractSerializer<RMFSTreeNode>(ext2);
        //}

        //[Obsolete("方法已经停用")]
        //public Dictionary<int, Rule> GetRules(Guid webApplicationId, DateTime timePoint)
        //{
        //    List<Rule> rules = RuleService.GetRulesFromDA();
        //    foreach (var rule in rules)
        //    {
        //        foreach (var filter in rule.SOFilters)
        //        {
        //            if (filter.RuleType == PolicyRuleType.CreatedTime || filter.RuleType == PolicyRuleType.ModifiedTime
        //                || filter.RuleType == PolicyRuleType.ColumnDateTime)
        //            {
        //                switch (filter.Condition)
        //                {
        //                    case PolicyCondition.FromTo:
        //                        var fromDt = ConvertUtcDateTime(filter.Value.Value1);
        //                        var toDt = ConvertUtcDateTime(filter.Value.Value2);
        //                        if (toDt > timePoint)
        //                        {
        //                            filter.Value.Value2 = timePoint.ToString(AveDateTimeUtility.DATETYPEForAPI003);
        //                        }
        //                        break;
        //                    case PolicyCondition.Before:
        //                        var ltDt = ConvertUtcDateTime(filter.Value.Value1);
        //                        if (ltDt >= timePoint)
        //                        {
        //                            filter.Value.Value1 = timePoint.ToString(AveDateTimeUtility.DATETYPEForAPI003);
        //                        }
        //                        break;
        //                    case PolicyCondition.OlderThan:
        //                        filter.Value.Value2 = timePoint.ToString(AveDateTimeUtility.DATETYPEForAPI003);
        //                        filter.Condition = PolicyCondition.FromTo;
        //                        break;
        //                    default:
        //                        break;
        //                }
        //            }
        //        }
        //    }
        //    List<Guid> ruleIds = TermRuleInfos.GetAllRules();
        //    List<RMTermRuleAssociation> termRules = TermRuleInfos.GetTermWithRule();
        //    string metadataColumn = SPSettingService.GetMetadataColumn(webApplicationId);
        //    RuleAssembler ruleAssembler = new RuleAssembler(metadataColumn);
        //    Dictionary<int, Rule> ruleResults = new Dictionary<int, Rule>();
        //    var termIds = termRules.Select(t => t.TermId).Distinct().ToList();

        //    foreach (var termId in termIds)
        //    {
        //        //var term = TermDao.GetOneRMTermsByTermId(termId);
        //        List<RMTerm> terms = new List<RMTerm>();
        //        TermDao.GetAllInheritTermsByRootTerm(termId, ref terms, timePoint.Ticks);
        //        if (terms.Count == 0)
        //        {
        //            continue;
        //        }
        //        Dictionary<int, Rule> termOrderRules = new Dictionary<int, Rule>();
        //        var termIdRules = termRules.AsQueryable().Where(t => t.TermId.Equals(termId)).ToList();
        //        foreach (var termRule in termIdRules)
        //        {
        //            var rule = rules.AsQueryable().Where(r => r.Id.Equals(termRule.RuleId.ToString())).FirstOrDefault();
        //            if (rule != null)
        //            {
        //                termOrderRules.Add(termRule.RuleOrder, rule);
        //            }
        //        }
        //        if (termOrderRules.Count > 0)
        //        {
        //            foreach (var term in terms)
        //            {
        //                ruleAssembler.AddTermWithRule(term, termOrderRules);
        //            }
        //        }
        //    }
        //    ruleResults = ruleAssembler.GetRuleDicResult();
        //    return ruleResults;
        //}

        /// <summary>
        /// Term Rule Mapping Use for Document Level
        /// </summary>
        /// <param name="timePoint"></param>
        /// <returns></returns>
        public async Task<Dictionary<Guid, RMRuleItemCollection>> GetTermAndRuleMappingsAsync(DateTime timePoint)
        {
            List<RMTermRuleAssociation> trAssociations = TermRuleInfos.GetTermWithRule();
            Dictionary<int, List<Guid>> termRules = new Dictionary<int, List<Guid>>();
            foreach (var termId in trAssociations.Select(a => a.TermId).Distinct())
            {
                var rules = trAssociations
                    .Where(a => a.TermId == termId)
                    .OrderBy(a => a.RuleOrder)
                    .Select(a => a.RuleId)
                    .ToList();
                if (rules.Count > 0)
                {
                    termRules.Add(termId, rules);
                }
            }

            var termRuleMappings = new Dictionary<Guid, RMRuleItemCollection>();
            Dictionary<Guid, Rule> allRules = (await RuleService.GetRulesFromDAAsync()).ToDictionary(r => new Guid(r.Id));
            var allHasRuleTerms = TermDao.GetRMTermsByTermIds(termRules.Keys.ToArray());
            foreach (var term in allHasRuleTerms)
            {
                if (term.IsRemoved)
                {
                    continue;
                }
                RuleCollection commonRules = new RuleCollection() { Rules = new Dictionary<int, Rule>() };
                List<RMRuleItem> rmRules = new List<RMRuleItem>();
                bool hasUnCamlQueryableCondition = false;
                Rule rule;
                var ruleIds = termRules[term.Id];
                for (int idx = 0; idx < ruleIds.Count; idx++)
                {
                    if (allRules.TryGetValue(ruleIds[idx], out rule))
                    {
                        var ruleOBj = CloneSameRuleObject(rule);
                        commonRules.Rules.Add(idx, ruleOBj);
                        if (ruleOBj.PolicyLevel == PolicyLevel.Item || ruleOBj.PolicyLevel == PolicyLevel.Document || ruleOBj.PolicyLevel == PolicyLevel.Folder)
                        {
                            rmRules.Add(ConvertRuleChecker(ruleOBj, term, timePoint));
                        }
                        else
                        {
                            ModifyRuleChecker(ruleOBj, term, timePoint);
                        }

                    }
                }
                if (rmRules.Count > 0)
                {
                    if (rmRules.Exists(rc => rc.HasUnCamlQueryableCondition))
                    {
                        hasUnCamlQueryableCondition = true;
                    }
                }
                var refTerms = new List<RMTerm>();
                TermDao.GetAllInheritTermsByRootTerm(term.Id, ref refTerms, timePoint.Ticks);
                foreach (var refTerm in refTerms)
                {
                    RMRuleItemCollection tempRC;
                    if (!termRuleMappings.TryGetValue(refTerm.UniqueId, out tempRC))
                    {
                        tempRC = new RMRuleItemCollection();
                        tempRC.TermId = refTerm.UniqueId;
                        tempRC.TermName = refTerm.Name;
                        termRuleMappings.Add(refTerm.UniqueId, tempRC);
                    }
                    tempRC.HasUnCamlQueryableCondition = hasUnCamlQueryableCondition;
                    tempRC.CommonRules = commonRules;
                    tempRC.Rules = rmRules;

                    //foreach (var commonRule in commonRules.Rules.Values)
                    //{
                    //    if (commonRule.PolicyLevel == PolicyLevel.Document)
                    //    {
                    //        tempRC.HasDocumentLevelRule = true;
                    //        break;
                    //    }
                    //}

                }
            }

            return termRuleMappings;
        }

        public Rule CloneSameRuleObject(Rule rule)
        {
            string xml = SerializerHelper.SerializeByDataContractSerializer(rule);
            Rule result = SerializerHelper.DeserializeByDataContractSerializer<Rule>(xml);
            return result;
        }

        public void SyncReportJobDatas(IEnumerable<BaseReport> jobDetails, BaseJobDto jobInfo)
        {
            logger.Debug("Update Report Detail2. Job Id:{0}, Job Type:{1},receive detail count:{2}.", jobInfo.Id, jobInfo.JobType, jobDetails.IsNullOrEmpty() ? 0 : jobDetails.Count());

            RASimpleLocker.Locker locker = _simpleLocker.GetLocker(jobInfo.Id);

            lock (locker)
            {
                try
                {
                    AbstractReportWorker worker = null;
                    if (baeReportWorkerDictionary.ContainsKey(jobInfo.JobType))
                    {
                        worker = baeReportWorkerDictionary[jobInfo.JobType];
                    }
                    ArgumentCheck.NotNull(worker, nameof(worker));
                    worker.SaveReportJobDatas(jobDetails, jobInfo);
                }
                catch (Exception e)
                {
                    logger.Error("{0}, {1}", e.Message, e.StackTrace);
                }
                finally
                {
                    _simpleLocker.FreeLocker(locker.Key);
                }
            }
        }


        public void UpdateReportJobDatas(IEnumerable<BaseReport> jobReports, BaseJobDto jobInfo)
        {
            baseJobDto = jobInfo;
            lock (locker)
            {
                if (jobReports != null)
                {
                    logger.Debug("Update Report Detail1. Job Id:{0}, Job Type:{1},receive detail count:{2}.", jobInfo.Id, jobInfo.JobType, jobReports.IsNullOrEmpty() ? 0 : jobReports.Count());
                    reportsWaiting.AddRange(jobReports);
                }
            }
            if (reportsWaiting.Count > 10 || finalUpdate)
            {
                List<BaseReport> sends = new List<BaseReport>();
                sends.AddRange(reportsWaiting);
                reportsWaiting.Clear();
                sendStatus++;
                AveTenantThread updateReport = new AveTenantThread(new ParameterizedThreadStart(DoUpdateReportJobDatas));
                updateReport.Start(sends);
            }
        }

        private void DoUpdateReportJobDatas(object reports)
        {
            List<BaseReport> jobReports = (List<BaseReport>)reports;

            try
            {
                AbstractReportWorker worker = null;
                if (baeReportWorkerDictionary.ContainsKey(baseJobDto.JobType))
                {
                    worker = baeReportWorkerDictionary[baseJobDto.JobType];
                }
                ArgumentCheck.NotNull(worker, nameof(worker));
                worker.SaveReportJobDatas(jobReports, baseJobDto);
            }
            catch (Exception e)
            {
                logger.Error("{0}, {1}", e.Message, e.StackTrace);
            }
            finally
            {
                //free
                try
                {
                    sendStatus--;
                }
                catch (Exception e)
                {
                    logger.Warn("Dispose job reports error {0}", e.ToString());
                }
            }

        }

        public void FinalUpdateAndWaitCompleted()
        {
            finalUpdate = true;
            UpdateReportJobDatas(null, baseJobDto);
            while (true)
            {
                if (sendStatus == 0)
                {
                    break;
                }
                Thread.Sleep(500);
            }
        }
        public void SetRestoreReportDisplayMod(bool isDynamicSizeDisplay)
        {
            this.IsDynamicSizeDisplay = isDynamicSizeDisplay;
        }
        public async Task<string> GetCommonReportJobDatasAsync(ShowReportQuery query)
        {
            int totalCount = 0;
            ShowReportResult result = new ShowReportResult();
            StringBuilder condition = new StringBuilder();
            bool isFristCondition = true;
            var profile = await GetProfileByIdAsync(Convert.ToString(query.ProfileId));
            var reportJobType = (int)query.ReportJobType == JobTypeConstants.SOArchivedSiteReportPageType && profile != null
                ? profile.Type
                : query.ReportJobType;
            var jobInfo = new BaseJobDto() { Id = query.JobId, JobType = (int)reportJobType };
            Dictionary<string, object> addValues = new Dictionary<string, object>();
            var PARAMTERS = "@{0}";
            if (((!string.IsNullOrEmpty(query.SearchValue)) && query.SearcheKeys.Count > 0))
            {
                if (!ValidateQuerySearchKeys(query.SearcheKeys))
                {
                    return "";
                }
                if (!string.IsNullOrEmpty(query.SearchValue))
                {
                    query.SearchValue = query.SearchValue.TransferSpecialCharacterForReport();
                    condition.Append(" ( ");
                    //兼容Content Due Report老数据查询
                    if (query.ReportJobType == JobType.ItemsFilesDueDisposal)
                    {
                        query.SearcheKeys = FilterSearchKeys(jobInfo, query.SearcheKeys);
                    }

                    foreach (var searchKey in query.SearcheKeys)
                    {
                        var sKey = string.Format(PARAMTERS, searchKey);
                        var sValue = string.Format("%" + query.SearchValue + "%");
                        if (isFristCondition)
                        {
                            condition.AppendFormat("[{0}] LIKE {1} escape '|' ", searchKey, sKey);
                            addValues.Add(sKey, sValue);
                            isFristCondition = false;
                        }
                        else
                        {
                            condition.AppendFormat("Or [{0}] LIKE {1} escape '|' ", searchKey, sKey);
                            addValues.Add(sKey, sValue);
                        }
                    }
                    condition.Append(" ) ");
                }
            }
            if (query.ReportJobType == JobType.CreateAndDestroyedFileReport && (!string.IsNullOrEmpty(query.SearchValue) && query.SearcheKeys.Count > 0))
            {
                if (query.Operation == -1)
                {
                    condition.AppendFormat("And ( {0} = {1} Or {2} = {3} ) ", "Operation", 0, "Operation", 1);
                }
                else
                {
                    var sKey = string.Format(PARAMTERS, "Operation");
                    var sValue = query.Operation;
                    condition.AppendFormat("And {0} = {1} ", "Operation", sKey);
                    addValues.Add(sKey, sValue);
                }
            }
            else if (query.ReportJobType == JobType.CreateAndDestroyedFileReport && (string.IsNullOrEmpty(query.SearchValue) || query.SearcheKeys.Count <= 0))
            {
                if (query.Operation == -1)
                {
                    condition.AppendFormat(" ( {0} = {1} Or {2} = {3} ) ", "Operation", 0, "Operation", 1);
                }
                else
                {
                    var sKey = string.Format(PARAMTERS, "Operation");
                    var sValue = query.Operation;
                    condition.AppendFormat(" {0} = {1} ", "Operation", sKey);
                    addValues.Add(sKey, sValue);
                }
            }
            else if (query.ReportJobType == JobType.SPOActionAuditReport || query.ReportJobType == JobType.OneDriveActionAuditReport
                || query.ReportJobType == JobType.TeamsActionAuditReport)
            {
                logger.Info("Process ClientActionAuditReport filter");
                condition = new StringBuilder();
                isFristCondition = true;
                if (!string.IsNullOrEmpty(query.SearchValue))
                {
                    query.SearchValue = query.SearchValue.TransferSpecialCharacterForReport();
                    condition.Append(" ( ");
                    query.SearcheKeys = new List<string>() { "Url" };

                    foreach (var searchKey in query.SearcheKeys)
                    {
                        var sKey = string.Format(PARAMTERS, searchKey);
                        var sValue = string.Format("%" + query.SearchValue + "%");
                        if (isFristCondition)
                        {
                            condition.AppendFormat("[{0}] LIKE {1} escape '|' ", searchKey, sKey);
                            addValues.Add(sKey, sValue);
                            isFristCondition = false;
                        }
                        else
                        {
                            condition.AppendFormat("Or [{0}] LIKE {1} escape '|' ", searchKey, sKey);
                            addValues.Add(sKey, sValue);
                        }
                    }
                    condition.Append(" ) ");
                }
                if (!string.IsNullOrEmpty(query.FilterObjectString))
                {
                    var actionId = Int32.Parse(query.FilterObjectString);
                    if (actionId > 0)
                    {
                        var sKey = string.Format(PARAMTERS, "Event");
                        var sValue = actionId;
                        if (isFristCondition)
                        {
                            condition.AppendFormat(" {0}&{1}={0} ", "Event", sKey);
                            isFristCondition = false;
                        }
                        else
                        {
                            condition.AppendFormat(" And {0}&{1}={0} ", "Event", sKey);
                        }
                        addValues.Add(sKey, sValue);
                    }
                }
                if (query.FilterListObject!=null && query.FilterListObject.Count > 0)
                {
                    List<SqlParameter> paras = null;
                    var parameterizedStatement = DatabaseUtility.BuildInClause(query.FilterListObject, out paras);
                    if (isFristCondition)
                    {
                        condition.AppendFormat(" {0} in {1}", "UserName", parameterizedStatement);
                        isFristCondition = false;
                    }
                    else
                    {
                        condition.AppendFormat(" And {0} in {1}", "UserName", parameterizedStatement);
                    }
                    foreach (var ky in paras)
                    {
                        addValues.Add(ky.ParameterName, ky.Value);
                    }
                }
            }
            var filterLevels = query.FilterLevels;
            if (filterLevels != null && filterLevels.Count > 0)
            {
                List<string> levels = new List<string>();
                var i = 0;
                foreach (var filterLevel in filterLevels)
                {
                    var sKey = string.Format(PARAMTERS, "ObjectLevel" + i);
                    addValues.Add(sKey, filterLevel);
                    levels.Add(sKey);
                    i++;
                }
                condition.AppendFormat(" {0} ObjectLevel IN ({1}) ", !string.IsNullOrEmpty(condition.ToString()) ? " And " : "", string.Join(",", levels.ToArray()));
            }

            var sanitizedSortBy = SecurityUtils.SanitizeSQLParameterName(query.SortBy, true);

            jobInfo.AddValues = addValues;
            (result.Details,totalCount) = await GetReportJobDatasAsync(query.PageSize, query.CurrentPage, condition.ToString(),
                jobInfo, sanitizedSortBy, query.isAscending);



            result.TotalNumber = totalCount;

            if (result?.Details != null &&
                (query.ReportJobType == JobType.SPOActionAuditReport || query.ReportJobType == JobType.OneDriveActionAuditReport))
            {
                foreach(BaseReport report in result.Details)
                {
                    ClientSPAuditReport auditReport = report as ClientSPAuditReport;
                    if (report == null)
                    {
                        continue;
                    }
                    auditReport.EventTypeName = ManagementAPIReportConstants.I18nEvents.ContainsKey(auditReport.EventTypeName) ? ManagementAPIReportConstants.I18nEvents[auditReport.EventTypeName] : auditReport.EventTypeName;
                }
            }

            if (query != null)
            {
                if (profile.Type == JobType.TeamsItemsFilesDueDisposalReport)
                {
                    result.IsTeams = true;
                    result.IsSharePoint = false;
                }
                else if ((int)profile.Type >= 1000 && (int)profile.Type < 6000)
                {
                    result.IsSharePoint = false;
                }
                else
                {
                    result.IsSharePoint = true;
                }
            }
            return JsonConvert.SerializeObject(result);
        }

        public async Task<(IEnumerable<BaseReport>,int)> GetReportJobDatasAsync(int PageSize, int StartPage,
            string conditionFilter, BaseJobDto jobInfo, string sortKey = null, bool isAscending = true)
        {
            IEnumerable<BaseReport> result = null;
            int recordCount = 0;
            try
            {
                AbstractReportWorker worker = null;
                if (baeReportWorkerDictionary.ContainsKey(jobInfo.JobType))
                {
                    worker = baeReportWorkerDictionary[jobInfo.JobType];
                }
                ArgumentCheck.NotNull(worker, nameof(worker));
                result = worker.GetReportJobDatas(PageSize, StartPage, ref recordCount, conditionFilter, jobInfo, sortKey, isAscending);
                ResetDateTimeFormat();
                GeneralSettingModel gls = await mGeneralSettingService.GetGeneralSettingAsync();
                List<int> physicalNodeList = new List<int>() { (int)RMNodeLevel.PhysicalBottomLocation, (int)RMNodeLevel.PhysicalBox, (int)RMNodeLevel.PhysicalFile, (int)RMNodeLevel.PhysicalRecord, (int)RMNodeLevel.PhysicalCustom };
                foreach (var r in result)
                {
                    if (physicalNodeList.Contains(r.ObjectLevel))
                    {
                        //i18n RM_SPS_Location_RootNode -> My Registered Locations
                        r.Url = r.Url.Replace("RM_SPS_Location_RootNode/", $"{I18NEntity.GetString("RM_SPS_Location_RootNode")}/");
                    }
                    r.CreatedTimeStr = ConverTicksToString(r.CreatedTime, r.SPWebTimeZoneName, gls);
                    r.LastModifiedTimeStr = ConverTicksToString(r.LastModifiedTime, r.SPWebTimeZoneName, gls);
                    if (r is CreateAndDestroyedFileReport)
                    {
                        var cdReport = r as CreateAndDestroyedFileReport;
                        long utcTicks = 0;
                        if (long.TryParse(cdReport.OperationTime, out utcTicks) || EXOCreationAndDestroyedFileReportProcessor.TryGetOperationTimeUtcTicks(cdReport, out utcTicks))
                        {
                            cdReport.OperationTime = mGeneralSettingService.ConvertTiksToDateTime(gls, utcTicks, true).SimplifyFormatTime;
                        }
                        if (TenantService.IsNewOpusTenant())
                        {
                            cdReport.CDCreatedTimeStr = mGeneralSettingService.ConvertTiksToDateTime(gls, cdReport.CreatedTime, true).SimplifyFormatTime;
                            cdReport.CDLastModifiedTimeStr = mGeneralSettingService.ConvertTiksToDateTime(gls, cdReport.LastModifiedTime, true).SimplifyFormatTime;
                            if (cdReport.CreatedTime == 0)
                            {
                                cdReport.CDCreatedTimeStr = string.Empty;
                            }
                            if (cdReport.LastModifiedTime == 0)
                            {
                                cdReport.CDLastModifiedTimeStr = string.Empty;
                            }
                            cdReport.FileType = I18NEntity.GetString(cdReport.FileType);
                        }
                        else
                        {
                            cdReport.CDCreatedTimeStr = string.Empty;
                            cdReport.CDLastModifiedTimeStr = string.Empty;
                            cdReport.FileType = string.Empty;
                            cdReport.RuleName = string.Empty;
                            cdReport.RecordsId = string.Empty;
                        }

                    }
                    if (r is AvailableSpaceReport)
                    {
                        var arReport = r as AvailableSpaceReport;
                        arReport.Location = arReport.Location.Replace("RM_SPS_Location_RootNode/", $"{I18NEntity.GetString("RM_SPS_Location_RootNode")}/");
                    }
                    if (r is ClientSPAuditReport)
                    {
                        var cspReport = r as ClientSPAuditReport;
                        cspReport.EventCategoryType = ConvertAuditReportActionCategoryToString(cspReport.Event);
                        //cspReport.EventTypeName = cspReport.EventTypeName;
                        cspReport.OccurredTimeStr = mGeneralSettingService.ConvertTiksToDateTime(gls, cspReport.Occurred, true).SimplifyFormatTime;
                        cspReport.ObjectLevelI18NName = ConvertAuditReportObjectLevelToString(cspReport.ObjectLevel);
                    }
                    if (r is RestoreFileReport)
                    {
                        var restoreReport = r as RestoreFileReport;
                        restoreReport.StartTimeString = ConverTicksToString(restoreReport.StartTime, restoreReport.SPWebTimeZoneName, gls);
                        restoreReport.EndTimeString = ConverTicksToString(restoreReport.EndTime, restoreReport.SPWebTimeZoneName, gls);
                        if (IsDynamicSizeDisplay)
                        {
                            restoreReport.SizeString = JobDetailHelper.GetDataSizeToViewForScreenRestoreReport(restoreReport.Size);
                        }
                        else
                        {
                            restoreReport.SizeString = JobDetailHelper.GetDataSizeToViewForRestoreReport(restoreReport.Size);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("{0}, {1}", e.Message, e.StackTrace);
            }
            return (result,recordCount);
        }

        public ReportFilter GetReportJobFilterData(ShowReportQuery query)
        {
            ReportFilter result = null;
            var jobInfo = new BaseJobDto() { Id = query.JobId, JobType = (int)query.ReportJobType };
            try
            {
                AbstractReportWorker worker = null;
                if (baeReportWorkerDictionary.ContainsKey(jobInfo.JobType))
                {
                    worker = baeReportWorkerDictionary[jobInfo.JobType];
                }
                ArgumentCheck.NotNull(worker, nameof(worker));
                result = worker.GetReportJobFilterData(jobInfo);

            }
            catch (Exception e)
            {
                logger.Error("{0}, {1}", e.Message, e.StackTrace);
            }
            return result;
        }
        /// <summary>
        /// to do, for now ,if can't get the settings ,just return the default values, check need change or not
        /// </summary>
        /// <returns></returns>
        public SOArchiverSettings GetSOArchiverSettings()
        {
            try
            {
                return new SOArchiverSettings()
                {
                    IsDeleteRecord = false,
                    IsDeleteLinkFile = false,
                    SkipFileExtensions = new string[] { ".aspx", ".js", ".css", ".md", ".copilot" }
                };
            }
            catch (Exception e)
            {
                logger.Error("Get Archiver Config Error {0}", e.ToString());
                return new SOArchiverSettings()
                {
                    IsDeleteRecord = false,
                    IsDeleteLinkFile = false,
                    SkipFileExtensions = new string[] { ".aspx", ".js", ".css", ".md", ".copilot" }
                };
            }
        }

        public int GetReportJobDatas(string conditionFilter, BaseJobDto jobInfo)
        {
            int jobReportTotalCount = 0;
            try
            {
                AbstractReportWorker worker = null;
                if (baeReportWorkerDictionary.ContainsKey(jobInfo.JobType))
                {
                    worker = baeReportWorkerDictionary[jobInfo.JobType];
                }
                ArgumentCheck.NotNull(worker, nameof(worker));
                jobReportTotalCount = worker.GetCountForDetail(conditionFilter, jobInfo);
            }
            catch (Exception e)
            {
                logger.Error("{0}, {1}", e.Message, e.StackTrace);
            }
            return jobReportTotalCount;
        }

        [RACodeReview("Allen yin")]
        public async Task<List<ProfileSimpleInfo>> GetProfilesByTypesAsync(List<JobType> jobTypes, List<SourceFlag> sources)
        {
            List<ProfileSimpleInfo> profileList = new List<ProfileSimpleInfo>();
            IEnumerable<RMProfile> profiles = null;
            if ((await IsAdminAsync() && IsILReportJobType((int)jobTypes.First()))
                || (await IsSOAdminAsync() && IsSOReportJobType((int)jobTypes.First())))
            {
                profiles = profileDAO.GetProfilesByTypes(jobTypes, sources);
            }
            else
            {
                profiles = profileDAO.GetProfilesByTypes(jobTypes, sources, TenantLocalValue.LogonUserId);
            }
            if (profiles != null && profiles.Count() > 0)
            {
                foreach (var p in profiles)
                {
                    profileList.Add(new ProfileSimpleInfo() { Name = p.IsRemoved ? p.Name + I18NEntity.GetString("RM_JS_RC_ProfileNameDeleted") : p.Name, Id = p.Id.ToString(), PType = p.Type });
                }
            }
            return profileList;
        }

        public List<KeyValuePair<string, string>> GetProfilesByIds(List<int> ids)
        {
            List<KeyValuePair<string, string>> profileList = new List<KeyValuePair<string, string>>();
            var profiles = profileDAO.GetProfileByIds(ids);
            if (profiles != null && profiles.Count() > 0)
            {
                foreach (var p in profiles)
                {
                    profileList.Add(new KeyValuePair<string, string>(p.IsRemoved ? p.Name + I18NEntity.GetString("RM_JS_RC_ProfileNameDeleted") : p.Name, p.Id.ToString()));//TODO i18n
                }
            }
            return profileList;
        }

        private bool IsPhysical(int objectLevel)
        {
            switch (objectLevel)
            {
                case (int)RMReportObjectLevel.PhyBox:
                case (int)RMReportObjectLevel.PhyFolder:
                case (int)RMReportObjectLevel.PhyRecord:
                    return true;
                default:
                    return false;
            }
        }

        public async Task<string[][]> ConvertDueDisposalReportToArrayAsync(IEnumerable<BaseReport> reportDetails, string[][] datas, bool isSPReport = true)
        {
            int rowCount = 1;
            DueDisposalReport reportInfo = null;
            GeneralSettingModel gls = await mGeneralSettingService.GetGeneralSettingAsync();
            foreach (BaseReport report in reportDetails)
            {
                try
                {
                    var colIdx = 0;
                    reportInfo = report as DueDisposalReport;
                    datas[rowCount] = isSPReport ? new string[18] : new string[17];
                    datas[rowCount][colIdx++] = ConvertObjectLevelToString(reportInfo.ObjectLevel);
                    datas[rowCount][colIdx++] = reportInfo.TitleOrName;
                    datas[rowCount][colIdx++] = reportInfo.Url;
                    if (isSPReport)
                    {
                        datas[rowCount][colIdx++] = reportInfo.SiteCollectionTitle;
                    }
                    datas[rowCount][colIdx++] = reportInfo.BCSTermName;
                    datas[rowCount][colIdx++] = reportInfo.AppliedRuleName;
                    datas[rowCount][colIdx++] = reportInfo.DisposalClass;
                    //Related Records...
                    var relatedRecords = string.Empty;
                    if (!string.IsNullOrEmpty(reportInfo.RelatedRecords))
                    {
                        List<string> relatedRecordsList = new List<string>();
                        var result = SerializerHelper.DeserializeFromXmlString<List<AvePoint.RA.Contract.RMWeb.ReportCenter.ReportRelatedRecords>>(reportInfo.RelatedRecords);
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < result.Count; i++)
                        {
                            sb.Append(result[i].Name);
                            if (i < result.Count - 1)
                            {
                                sb.Append("; ");
                            }
                        }
                        relatedRecords = sb.ToString();
                    }
                    datas[rowCount][colIdx++] = relatedRecords;
                    datas[rowCount][colIdx++] = reportInfo.RelatedRecordsAction == 0 ? I18NEntity.GetString("RM_JS_RDM_RelatedRecordsAction_None") : I18NEntity.GetString("RM_JS_RDM_RelatedRecordsAction_Both");
                    datas[rowCount][colIdx++] = RuleHelper.ConvertDisposalActionToString(reportInfo.DisposalAction, IsPhysical(reportInfo.ObjectLevel));
                    datas[rowCount][colIdx++] = ConvertReportStatusToString(reportInfo.Status);
                    datas[rowCount][colIdx++] = reportInfo.Comment;
                    datas[rowCount][colIdx++] = ConvertManualApprovalToString(reportInfo.ManualApproval);
                    datas[rowCount][colIdx++] = ConvertExportTypeValueToString(reportInfo.ExportType);
                    datas[rowCount][colIdx++] = reportInfo.CreatedBy;
                    datas[rowCount][colIdx++] = ConverTicksToString(reportInfo.CreatedTime, reportInfo.SPWebTimeZoneName, gls);
                    datas[rowCount][colIdx++] = reportInfo.LastModifiedBy;
                    datas[rowCount][colIdx++] = ConverTicksToString(reportInfo.LastModifiedTime, reportInfo.SPWebTimeZoneName, gls);
                    rowCount++;
                }
                catch (Exception e)
                {
                    logger.Error($"Convert DueDisposal Report To Array {e}");
                    rowCount++;
                    throw;
                }
            }
            return datas;
        }

        public string[][] AssembleDueTimeFrameReportHeaderTittle(string[][] datas)
        {
            datas[0] = new string[15];
            datas[0][0] = I18NEntity.GetString("RM_JS_RC_TimeFrame_Time");
            datas[0][1] = I18NEntity.GetString("RM_JS_RC_ReportColumn_ObjectLevel");
            datas[0][2] = I18NEntity.GetString("RM_JS_RC_ReportColumn_TitleOrName");
            datas[0][3] = I18NEntity.GetString("RM_JS_RC_ReportColumn_Url");
            datas[0][4] = I18NEntity.GetString("RM_PRM_PRE_Column_Type");
            datas[0][5] = I18NEntity.GetString("RM_JS_RC_ReportColumn_CreateTime");
            datas[0][6] = I18NEntity.GetString("RM_JS_RC_ReportColumn_LastModifiedTime");
            datas[0][7] = I18NEntity.GetString("RM_JS_RC_TimeFrame_Operation");
            datas[0][8] = I18NEntity.GetString("RM_JS_RC_TimeFrame_By");
            datas[0][9] = I18NEntity.GetString("RM_JS_RC_ReportColumn_RecordsID");
            datas[0][10] = I18NEntity.GetString("RM_JS_RC_ReportColumn_BCSTermName");
            datas[0][11] = I18NEntity.GetString("RM_JS_MA_Grid_Rule");
            datas[0][12] = I18NEntity.GetString("RM_JS_Rule_DisposalClass_Title");
            datas[0][13] = I18NEntity.GetString("RM_JS_JMD_Grid_ApprovalStatus");
            datas[0][14] = I18NEntity.GetString("RM_JS_RC_ReportColumn_ApprovedBy");
            return datas;
        }
        public string[][] AssembleRestoreReportHeaderTittle(string[][] datas)
        {
            datas[0] = new string[8];
            datas[0][0] = I18NEntity.GetString("RM_JS_RC_ReportColumn_TitleOrName");
            datas[0][1] = I18NEntity.GetString("RM_JS_RC_ReportColumn_Url");
            datas[0][2] = I18NEntity.GetString("RM_JS_Export_Grid_Size");
            datas[0][3] = I18NEntity.GetString("RM_JS_RC_ReportColumn_RestoreBy");
            datas[0][4] = I18NEntity.GetString("RM_JS_RC_ReportColumn_JobId");
            datas[0][5] = I18NEntity.GetString("RM_JS_RC_ReportColumn_StartTime");
            datas[0][6] = I18NEntity.GetString("RM_JS_RC_ReportColumn_EndTime");
            datas[0][7] = I18NEntity.GetString("RM_JS_RC_ReportColumn_RestoreTo");
            return datas;
        }
        public string[][] ConvertDueTimeFrameReportToArray(IEnumerable<BaseReport> reportDetails, string[][] datas)
        {
            GeneralSettingModel gls = mGeneralSettingService.GetGeneralSettingAsync().GetAwaiter().GetResult();
            var isNewOpusTenant = TenantService.IsNewOpusTenant();
            CreateAndDestroyedFileReport reportInfo = null;
            int rowCount = 1;
            foreach (BaseReport report in reportDetails)
            {
                try
                {
                    reportInfo = report as CreateAndDestroyedFileReport;
                    datas[rowCount] = new string[15];
                    datas[rowCount][0] = reportInfo.OperationTime;
                    datas[rowCount][1] = ConvertObjectLevelToString(reportInfo.LevelStr);
                    datas[rowCount][2] = reportInfo.Title;
                    datas[rowCount][3] = reportInfo.Url;
                    datas[rowCount][4] = isNewOpusTenant ?  I18NEntity.GetString(reportInfo.FileType) : string.Empty;
                    datas[rowCount][5] = isNewOpusTenant ? (reportInfo.CreatedTime == 0 ? string.Empty : ConverTicksToString(reportInfo.CreatedTime, "", gls)) : string.Empty;
                    datas[rowCount][6] = isNewOpusTenant ? (reportInfo.LastModifiedTime == 0 ? string.Empty : ConverTicksToString(reportInfo.LastModifiedTime, "", gls)) : string.Empty;
                    datas[rowCount][7] = reportInfo.Operation == (int)OperationType.Created ?
                        I18NEntity.GetString("RM_JS_RC_TimeFrame_Create") : I18NEntity.GetString("RM_JS_RC_TimeFrame_Destroyed");
                    datas[rowCount][8] = reportInfo.OperationBy;
                    datas[rowCount][9] = isNewOpusTenant ? reportInfo.RecordsId : string.Empty;
                    datas[rowCount][10] = reportInfo.TermName;
                    datas[rowCount][11] = isNewOpusTenant ? reportInfo.RuleName : string.Empty;
                    datas[rowCount][12] = reportInfo.DisposalClass;
                    if (CheckApprovalStatusIsFinalStatus(reportInfo.InternalApprovedStatus, reportInfo.Url))
                    {
                        datas[rowCount][13] = reportInfo.InternalApprovedStatus == (int)SOApproveDBStatus.WorkflowComplete
                        ? $"{ConvertApprovalStatusToString(reportInfo.InternalApprovedStatus)} ({ConvertApprovalStatusToString(reportInfo.ApprovalStatus)})"
                        : ConvertApprovalStatusToString(reportInfo.ApprovalStatus);
                    }
                    else
                    {
                        datas[rowCount][13] = string.Empty;
                    }
                    datas[rowCount][14] = reportInfo.ApprovedBy;
                    rowCount++;
                }
                catch (Exception e)
                {
                    logger.Error($"Convert Due TimeFrame Report To Array failed {e}");
                    rowCount++;
                    throw;
                }

            }
            return datas;
        }
        public string[][] ConvertRestoreReportToArray(IEnumerable<BaseReport> reportDetails, string[][] datas)
        {
            RestoreFileReport reportInfo = null;
            int rowCount = 1;
            foreach (BaseReport report in reportDetails)
            {
                try
                {
                    reportInfo = report as RestoreFileReport;
                    datas[rowCount] = new string[8];
                    datas[rowCount][0] = reportInfo.TitleOrName;
                    datas[rowCount][1] = reportInfo.Url;
                    datas[rowCount][2] = ConvertUnitUtil.ConvertToKB(reportInfo.SizeString);
                    datas[rowCount][3] = reportInfo.RestoreBy;
                    datas[rowCount][4] = reportInfo.JobId;
                    datas[rowCount][5] = reportInfo.StartTimeString;
                    datas[rowCount][6] = reportInfo.EndTimeString;
                    datas[rowCount][7] = reportInfo.RestoreTo;
                    rowCount++;
                }
                catch (Exception e)
                {
                    logger.Error($"Convert Due restore Report To Array failed {e}");
                    rowCount++;
                    throw;
                }

            }
            return datas;
        }
        public string[][] AssembleClientAuditReportHeaderTittle(string[][] datas)
        {
            datas[0] = new string[6];
            datas[0][0] = I18NEntity.GetString("RM_JS_RC_ActionAudit_ShowReportCol_Time");
            datas[0][1] = I18NEntity.GetString("RM_JS_RC_ActionAudit_ShowReportCol_User");
            datas[0][2] = I18NEntity.GetString("RM_JS_RC_ActionAudit_ShowReportCol_Url");
            datas[0][3] = I18NEntity.GetString("RM_JS_RC_ActionAudit_ShowReportCol_ObjType"); //   RM_JS_RC_ReportColumn_ObjectLevel
            datas[0][4] = I18NEntity.GetString("RM_JS_RC_ActionAudit_ShowReportCol_Type");
            datas[0][5] = I18NEntity.GetString("RM_JS_RC_ActionAudit_ShowReportCol_Action");
            return datas;
        }

        public async Task<string[][]> ConvertClientAuditReportToArrayAsync(IEnumerable<BaseReport> reportDetails, string[][] datas)
        {
            GeneralSettingModel gls = await mGeneralSettingService.GetGeneralSettingAsync();
            ClientSPAuditReport reportInfo = null;
            int rowCount = 1;
            foreach (BaseReport report in reportDetails)
            {
                try
                {
                    reportInfo = report as ClientSPAuditReport;
                    datas[rowCount] = new string[6];
                    datas[rowCount][0] = ConverTicksToString(reportInfo.Occurred, "", gls);
                    datas[rowCount][1] = reportInfo.User;
                    datas[rowCount][2] = XmlUtil.RemoveInvalidXmlChars(reportInfo.Url);
                    datas[rowCount][3] = ConvertAuditReportObjectLevelToString(reportInfo.ObjectLevel);
                    datas[rowCount][4] = reportInfo.EventCategoryType;
                    datas[rowCount][5] = ManagementAPIReportConstants.I18nEvents.ContainsKey(reportInfo.EventTypeName) ? ManagementAPIReportConstants.I18nEvents[reportInfo.EventTypeName] : reportInfo.EventTypeName;
                    rowCount++;
                }
                catch (Exception e)
                {
                    logger.Error($"Convert Client Audit Report To Array failed {e}");
                    rowCount++;
                    throw;
                }

            }
            return datas;
        }

        private string ConvertAuditReportObjectLevelToString(int level)
        {
            switch (level)
            {
                case (int)ClientAuditObjType.SiteCollection: return I18NEntity.GetString("RM_JS_RC_ActionAudit_ObjType_SiteCollection");
                case (int)ClientAuditObjType.Site: return I18NEntity.GetString("RM_JS_RC_ActionAudit_ObjType_Site");
                case (int)ClientAuditObjType.List: return I18NEntity.GetString("RM_JS_RC_ActionAudit_ObjType_List");
                case (int)ClientAuditObjType.Folder: return I18NEntity.GetString("RM_JS_RC_ActionAudit_ObjType_Folder");
                case (int)ClientAuditObjType.Document: return I18NEntity.GetString("RM_JS_RC_ActionAudit_ObjType_Document");
                case (int)ClientAuditObjType.ListItem: return I18NEntity.GetString("RM_JS_RC_ActionAudit_ObjType_ListItem");
                default: return "";
            }
        }


        private string ConvertAuditReportActionCategoryToString(int action)
        {
            switch (action)
            {
                case (int)ReportCenterObject.AuditEventType.CheckOut: return I18NEntity.GetString("RM_JS_RC_ActionAudit_ActionType_CheckOut");
                case (int)ReportCenterObject.AuditEventType.CheckIn: return I18NEntity.GetString("RM_JS_RC_ActionAudit_ActionType_CheckIn");
                case (int)ReportCenterObject.AuditEventType.View: return I18NEntity.GetString("RM_JS_RC_ActionAudit_ActionType_View");
                case (int)ReportCenterObject.AuditEventType.Delete: return I18NEntity.GetString("RM_JS_RC_ActionAudit_ActionType_Delete");
                case (int)ReportCenterObject.AuditEventType.Update: return I18NEntity.GetString("RM_JS_RC_ActionAudit_ActionType_Update");
                case (int)ReportCenterObject.AuditEventType.Undelete: return I18NEntity.GetString("RM_JS_RC_ActionAudit_ActionType_Restore");
                case (int)ReportCenterObject.AuditEventType.Download: return I18NEntity.GetString("RM_JS_RC_ActionAudit_ActionType_Download");
                case (int)ReportCenterObject.AuditEventType.Search: return I18NEntity.GetString("RM_JS_RC_ActionAudit_ActionType_Search");
                case (int)ReportCenterObject.AuditEventType.CreateGroup: return I18NEntity.GetString("RM_JS_RC_ActionAudit_ActionType_CreateGroup");
                case (int)ReportCenterObject.AuditEventType.DeleteGroup: return I18NEntity.GetString("RM_JS_RC_ActionAudit_ActionType_DeleteGroup");
                case (int)ReportCenterObject.AuditEventType.AddGroupMember: return I18NEntity.GetString("RM_JS_RC_ActionAudit_ActionType_AddGroupMember");
                case (int)ReportCenterObject.AuditEventType.DeleteGroupMember: return I18NEntity.GetString("RM_JS_RC_ActionAudit_ActionType_DelGroupMember");
                case (int)ReportCenterObject.AuditEventType.CreatePermissionLevel: return I18NEntity.GetString("RM_JS_RC_ActionAudit_ActionType_CreatePermissionLevel");
                case (int)ReportCenterObject.AuditEventType.DeletePermissionLevel: return I18NEntity.GetString("RM_JS_RC_ActionAudit_ActionType_DeletePermissionLevel");
                case (int)ReportCenterObject.AuditEventType.ChangePermissionLevel: return I18NEntity.GetString("RM_JS_RC_ActionAudit_ActionType_ChangePermissionLevel");
                case (int)ReportCenterObject.AuditEventType.ChangePermission: return I18NEntity.GetString("RM_JS_RC_ActionAudit_ActionType_ChangePermission");
                case (int)ReportCenterObject.AuditEventType.InheritPermissionSetting: return I18NEntity.GetString("RM_JS_RC_ActionAudit_ActionType_InheritPermission");
                case (int)ReportCenterObject.AuditEventType.ProfileChange: return I18NEntity.GetString("RM_JS_RC_ActionAudit_ActionType_ProfileChange");
                case (int)ReportCenterObject.AuditEventType.SchemaChange: return I18NEntity.GetString("RM_JS_RC_ActionAudit_ActionType_SchemaChange");
                case (int)ReportCenterObject.AuditEventType.BreakPermissionInheritance: return I18NEntity.GetString("RM_JS_RC_ActionAudit_ActionType_BreakPermission");
                case (int)ReportCenterObject.AuditEventType.Others: return I18NEntity.GetString("RM_JS_RC_ActionAudit_ActionType_Others");
                default: return I18NEntity.GetString("RM_JS_RC_ActionAudit_ActionType_Others"); ;
            }
        }

        private string ConvertExportTypeValueToString(RMExportTypeValue exportTypeValue)
        {
            switch (exportTypeValue)
            {
                case RMExportTypeValue.None:
                    return I18NEntity.GetString("RM_JS_RDM_CreateRule_ExportType_None"); ;
                case RMExportTypeValue.Autonomy:
                    return I18NEntity.GetString("RM_JS_RDM_CreateRule_ExportType_Autonomy");
                case RMExportTypeValue.Concordance:
                    return I18NEntity.GetString("RM_JS_RDM_CreateRule_ExportType_Concordance");
                case RMExportTypeValue.EDRM:
                    return I18NEntity.GetString("RM_JS_RDM_CreateRule_ExportType_EDRM");
                case RMExportTypeValue.VEO:
                    return I18NEntity.GetString("RM_JS_RDM_CreateRule_ExportType_VEO");
                case RMExportTypeValue.NAA:
                    return I18NEntity.GetString("RM_JS_RDM_CreateRule_ExportType_NAA");
                case RMExportTypeValue.NARA:
                    return I18NEntity.GetString("RM_JS_RDM_CreateRule_ExportType_NARA");
                default:
                    return "";
            }
        }
        private string ConvertReportStatusToString(RMReportStatus reportStatus)
        {
            switch (reportStatus)
            {
                case RMReportStatus.Successful:
                    return I18NEntity.GetString("RM_JS_JMD_Status_Successful");
                case RMReportStatus.Failed:
                    return I18NEntity.GetString("RM_JS_JMD_Status_Failed");
                case RMReportStatus.Skip:
                    return I18NEntity.GetString("RM_JS_JMD_Status_Skipped");
                default:
                    return "";
            }
        }
        private string ConvertManualApprovalToString(RMDisposalManualApproval disposalManualApproval)
        {
            switch (disposalManualApproval)
            {
                case RMDisposalManualApproval.Nonsupport:
                    return I18NEntity.GetString("RM_JS_Common_Pending");
                case RMDisposalManualApproval.Yes:
                    return I18NEntity.GetString("RM_JS_Common_Yes");
                case RMDisposalManualApproval.No:
                    return I18NEntity.GetString("RM_JS_Common_No");
                default:
                    return "";
            }
        }
        private string ConvertTermStatusToString(RMTermStatus status)
        {
            switch (status)
            {
                case RMTermStatus.Avaliable:
                    return I18NEntity.GetString("RM_JS_RC_ReportColumn_TermStatus_Avaliable");
                case RMTermStatus.Retired:
                    return I18NEntity.GetString("RM_JS_RC_ReportColumn_TermStatus_Retired");
                case RMTermStatus.Invalid:
                    return I18NEntity.GetString("RM_JS_RC_ReportColumn_TermStatus_Invalid");
                case RMTermStatus.Removed:
                    return I18NEntity.GetString("RM_JS_RC_ReportColumn_TermStatus_Removed");
                default:
                    return string.Empty;
            }
        }
        private string ConvertObjectLevelToString(int level)
        {
            switch (level)
            {
                case (int)RMReportObjectLevel.Document: return I18NEntity.GetString("RM_JS_Rule_ObjectLevel_Document");
                case (int)RMReportObjectLevel.SiteCollection: return I18NEntity.GetString("RM_JS_Rule_ObjectLevel_SiteCollection");
                case (int)RMReportObjectLevel.Site: return I18NEntity.GetString("RM_JS_Rule_ObjectLevel_Site");
                case (int)RMReportObjectLevel.List: return I18NEntity.GetString("RM_JS_Rule_ObjectLevel_List");
                case (int)RMReportObjectLevel.Item: return I18NEntity.GetString("RM_JS_Rule_ObjectLevel_Item");
                case (int)RMReportObjectLevel.PhysicalFile: return I18NEntity.GetString("RM_JS_Rule_ObjectLevel_PhysicalFile");
                case (int)RMReportObjectLevel.PhysicalRecord: return I18NEntity.GetString("RM_JS_Rule_ObjectLevel_PhysicalRecord");
                case (int)RMReportObjectLevel.Folder: return I18NEntity.GetString("RM_JS_Rule_ObjectLevel_Folder");
                case (int)RMReportObjectLevel.ExchangeOnlineItem: return I18NEntity.GetString("RM_JS_Rule_ObjectLevel_ExchangeOnlineItem");
                case (int)RMReportObjectLevel.PhyBox: return I18NEntity.GetString("RM_PRM_PRE_TableItemType_Box");
                case (int)RMReportObjectLevel.PhyFolder: return I18NEntity.GetString("RM_PRM_PRE_TableItemType_File");
                case (int)RMReportObjectLevel.PhyRecord: return I18NEntity.GetString("RM_PRM_PRE_TableItemType_Record");
                case (int)RMReportObjectLevel.FSFile: return I18NEntity.GetString("RM_JS_Rule_ObjectLevel_FSFile");
                case (int)RMReportObjectLevel.FSFolder: return I18NEntity.GetString("RM_JS_Rule_ObjectLevel_FSFolder");
                case (int)RMReportObjectLevel.PhyCustom: return I18NEntity.GetString("RM_PRM_PRE_TableItemType_Container");
                case (int)RMReportObjectLevel.Attachment: return I18NEntity.GetString("RM_JS_Rule_ObjectLevel_Attachment");
                case (int)RMReportObjectLevel.DocumentVersion: return I18NEntity.GetString("RM_JS_Rule_ObjectLevel_DocumentVersion");
                case (int)RMReportObjectLevel.ItemVersion: return I18NEntity.GetString("RM_JS_Rule_ObjectLevel_ItemVersion");
                case (int)RMReportObjectLevel.BoxFile: return I18NEntity.GetString("RM_JS_Rule_ObjectLevel_BoxFile");
                case (int)RMReportObjectLevel.BoxFolder: return I18NEntity.GetString("RM_JS_Rule_ObjectLevel_BoxFolder");
                case (int)RMReportObjectLevel.GoogleFile: return I18NEntity.GetString("RM_JS_Rule_ObjectLevel_GoogleFile");
                case (int)RMReportObjectLevel.GoogleFolder: return I18NEntity.GetString("RM_JS_Rule_ObjectLevel_GoogleFolder");
                default: return "";
            }
        }

        private string ConvertApprovalStatusToString(int level)
        {
            switch (level)
            {
                case (int)SOApproveDBStatus.WaitingApprove: return I18NEntity.GetString("RM_JS_MA_ApproveStatus_WaitingApprove");
                case (int)SOApproveDBStatus.Approved: return I18NEntity.GetString("RM_JS_MA_ApproveStatus_Approved");
                case (int)SOApproveDBStatus.Rejected: return I18NEntity.GetString("RM_JS_MA_ApproveStatus_Rejected");
                case (int)SOApproveDBStatus.WorkflowInProgress: return I18NEntity.GetString("RM_JS_MA_WorkflowStatus_Inprogress");
                case (int)SOApproveDBStatus.WorkflowComplete: return I18NEntity.GetString("RM_JS_MA_WorkflowStatus_Complete");
                case (int)SOApproveDBStatus.Cancelled: return I18NEntity.GetString("RM_JS_MA_ApproveStatus_Cancelled");
                default: return "";
            }
        }

        private bool CheckApprovalStatusIsFinalStatus(int level, object dataMessage)
        {
            switch (level)
            {
                case (int)SOApproveDBStatus.WorkflowInProgress:
                case (int)SOApproveDBStatus.WaitingApprove:
                    return false;
                case (int)SOApproveDBStatus.Cancelled:
                case (int)SOApproveDBStatus.Approved:
                case (int)SOApproveDBStatus.Rejected:
                case (int)SOApproveDBStatus.WorkflowComplete:
                    return true;
                default:
                    logger.Warn(@$"fail check approve status is  final status,level:{level} ,data message:{dataMessage}");
                    return false;
            }
        }




        public string[][] AssembleDueDisposalReportHeaderTittle(string[][] datas, bool isSPReport = true)
        {
            var colIdx = 0;
            datas[0] = isSPReport ? new string[18] : new string[17];
            datas[0][colIdx++] = I18NEntity.GetString("RM_JS_RC_ReportColumn_ObjectLevel");
            datas[0][colIdx++] = I18NEntity.GetString("RM_JS_RC_ReportColumn_TitleOrName");
            datas[0][colIdx++] = I18NEntity.GetString("RM_JS_RC_ReportColumn_Url");
            if (isSPReport)
            {
                datas[0][colIdx++] = I18NEntity.GetString("RM_JS_RC_ReportColumn_SiteCollectionTitle");
            }
            datas[0][colIdx++] = I18NEntity.GetString("RM_JS_RC_ReportColumn_BCSTermName");
            datas[0][colIdx++] = I18NEntity.GetString("RM_JS_RC_ReportColumn_AppliedRuleName");
            datas[0][colIdx++] = I18NEntity.GetString("RM_JS_Rule_DisposalClass_Title");
            datas[0][colIdx++] = I18NEntity.GetString("RM_JS_RC_ReportColumn_RelatedRecords");
            datas[0][colIdx++] = I18NEntity.GetString("RM_JS_RC_ReportColumn_RelatedRecordsAction");
            datas[0][colIdx++] = I18NEntity.GetString("RM_JS_RC_ReportColumn_DisposalAction");
            datas[0][colIdx++] = I18NEntity.GetString("RM_JS_RC_ReportColumn_Status");
            datas[0][colIdx++] = I18NEntity.GetString("RM_JS_RC_ReportColumn_Comment");
            datas[0][colIdx++] = I18NEntity.GetString("RM_JS_RC_ReportColumn_ManualApproval");
            datas[0][colIdx++] = I18NEntity.GetString("RM_JS_RC_ReportColumn_ExportType");
            datas[0][colIdx++] = I18NEntity.GetString("RM_JS_RC_ReportColumn_CreatedBy");
            datas[0][colIdx++] = I18NEntity.GetString("RM_JS_RC_ReportColumn_CreatedTime");
            datas[0][colIdx++] = I18NEntity.GetString("RM_JS_RC_ReportColumn_LastModifiedBy");
            datas[0][colIdx++] = I18NEntity.GetString("RM_JS_RC_ReportColumn_LastModifiedTime");
            //datas[0][16] = I18NEntity.GetString("RM_JS_RC_ReportColumn_LifecycleStatus");
            //datas[0][17] = I18NEntity.GetString("RM_JS_RC_ReportColumn_Availablity");
            //datas[0][18] = I18NEntity.GetString("RM_JS_RC_ReportColumn_HomeLocation");
            //datas[0][19] = I18NEntity.GetString("RM_JS_RC_ReportColumn_CurrentHeldBy");
            //datas[0][20] = I18NEntity.GetString("RM_JS_RC_ReportColumn_Box");
            return datas;
        }
        public async Task<string[][]> ConvertBCSTermUsageReportToArrayAsync(IEnumerable<BaseReport> reportDetails, string[][] datas)
        {
            BCSTermUsageReport reportInfo = null;
            int rowCount = 1;
            GeneralSettingModel gls = await mGeneralSettingService.GetGeneralSettingAsync();
            foreach (BaseReport report in reportDetails)
            {
                try
                {
                    reportInfo = report as BCSTermUsageReport;
                    datas[rowCount] = new string[10];
                    datas[rowCount][0] = ConvertObjectLevelToString(reportInfo.ObjectLevel);
                    datas[rowCount][1] = reportInfo.TitleOrName;
                    datas[rowCount][2] = reportInfo.Url;
                    datas[rowCount][3] = reportInfo.BCSTermName;
                    datas[rowCount][4] = ConvertTermStatusToString(reportInfo.TermStatus);
                    datas[rowCount][5] = reportInfo.BCSTermFullPath;
                    datas[rowCount][6] = reportInfo.CreatedBy;
                    datas[rowCount][7] = ConverTicksToString(reportInfo.CreatedTime, reportInfo.SPWebTimeZoneName, gls);
                    datas[rowCount][8] = reportInfo.LastModifiedBy;
                    datas[rowCount][9] = ConverTicksToString(reportInfo.LastModifiedTime, reportInfo.SPWebTimeZoneName, gls);
                    rowCount++;
                }
                catch (Exception e)
                {
                    logger.Error($"Convert BCS Term Usage Report To Array failed {e}");
                    rowCount++;
                    throw;
                }
            }
            return datas;
        }

        public string[][] AssembleBCSTermUsageReportHeaderTittle(string[][] datas)
        {
            datas[0] = new string[10];
            datas[0][0] = I18NEntity.GetString("RM_JS_RC_ReportColumn_ObjectLevel");
            datas[0][1] = I18NEntity.GetString("RM_JS_RC_ReportColumn_TitleOrName");
            datas[0][2] = I18NEntity.GetString("RM_JS_RC_ReportColumn_Url");
            datas[0][3] = I18NEntity.GetString("RM_JS_RC_ReportColumn_BCSTermName");
            datas[0][4] = I18NEntity.GetString("RM_JS_RC_ReportColumn_TermStatus");
            datas[0][5] = I18NEntity.GetString("RM_JS_RC_ReportColumn_TermFullPath");
            datas[0][6] = I18NEntity.GetString("RM_JS_RC_ReportColumn_CreatedBy");
            datas[0][7] = I18NEntity.GetString("RM_JS_RC_ReportColumn_CreatedTime");
            datas[0][8] = I18NEntity.GetString("RM_JS_RC_ReportColumn_LastModifiedBy");
            datas[0][9] = I18NEntity.GetString("RM_JS_RC_ReportColumn_LastModifiedTime");
            //datas[0][10] = I18NEntity.GetString("RM_JS_RC_ReportColumn_LifecycleStatus");
            //datas[0][11] = I18NEntity.GetString("RM_JS_RC_ReportColumn_Availablity");
            //datas[0][12] = I18NEntity.GetString("RM_JS_RC_ReportColumn_HomeLocation");
            //datas[0][13] = I18NEntity.GetString("RM_JS_RC_ReportColumn_CurrentHeldBy");
            //datas[0][14] = I18NEntity.GetString("RM_JS_RC_ReportColumn_Box");
            return datas;
        }
        //public ValidationMessage CheckDocAveConnectionGlobalStorageSetting(ValidationType Type)
        //{
        //    bool isHasEmailSetting = GlobalSettingService.IsConfigEmailSetting();
        //    bool gssValidate = true, dcsValidate = true;
        //    ValidationMessage vm = new ValidationMessage();
        //    vm.Success = true;
        //    RMCPGlobalStorageSetting rmSettings = mGlobalStorageSettingDao.GetGlobalSettingInfoFromRA();
        //    gssValidate = rmSettings == null ? false : true;
        //    dcsValidate = DocAveControlSetting.DocAveControlHost == null || DocAveControlSetting.DocAveUsername == null ? false : true;

        //    string gssdom = string.Format("<a class='message-link' id='globalStorage-link' href='/cp/StorageSettings'>{0}</a>", I18NEntity.GetString("RM_CP_StorageSetting"));
        //    string dcsdom = string.Format("<a class='message-link' id='docAveConn-link' class='' href='/cp/DocAveConnection'>{0}</a>", I18NEntity.GetString("RM_CP_DocAveConn"));
        //    string esdom = string.Format("<a class='message-link' id='emailSetting-link' href='/cp/EmailSetting'>{0}</a>", I18NEntity.GetString("RM_CP_EmailSetting"));

        //    string gssMessage = string.Format(I18NEntity.GetString("RM_JS_Common_ValidationSettingMsg"), gssdom);
        //    string dcsMessage = string.Format(I18NEntity.GetString("RM_JS_Common_ValidationSettingMsg"), dcsdom);
        //    string docaveAndglobalMessage = string.Format(I18NEntity.GetString("RM_JS_Common_configDocaveConnectAndGlobalSetting"), dcsdom, gssdom);

        //    string esMessage = string.Format(I18NEntity.GetString("RM_JS_Common_ValidationSettingMsg"), esdom);
        //    switch (Type)
        //    {
        //        case ValidationType.GlobalStorageSetting:
        //            if (!gssValidate)
        //            {
        //                vm.Message = gssMessage;
        //                vm.Success = false;
        //            }
        //            break;
        //        case ValidationType.DocAveConnection:
        //            if (!dcsValidate)
        //            {
        //                vm.Message = dcsMessage;
        //                vm.Success = false;
        //            }
        //            break;
        //        case ValidationType.GlobalStorageAndDocAveConn:
        //            if (!gssValidate && !dcsValidate)
        //            {
        //                vm.Message = docaveAndglobalMessage;
        //                vm.Success = false;
        //            }
        //            else if (!gssValidate)
        //            {
        //                vm.Message = gssMessage;
        //                vm.Success = false;
        //            }
        //            else if (!dcsValidate)
        //            {
        //                vm.Message = dcsMessage;
        //                vm.Success = false;
        //            }
        //            break;
        //        case ValidationType.EmailSettings:
        //            if (!isHasEmailSetting)
        //            {
        //                vm.Message = esMessage;
        //                vm.Success = false;
        //            }
        //            break;
        //        default:
        //            break;
        //    }
        //    return vm;
        //}
        #endregion

        #region private method

        #region build term tree 回显,REC-896
        public List<RMTermDto> GetTermTree(string termJsonStr)
        {
            Dictionary<int, RMTermDto> termDic = JsonConvert.DeserializeObject<Dictionary<int, RMTermDto>>(termJsonStr);
            List<RMTermDto> termsFromDB = termDic.Values.ToList();
            List<RMTermDto> termsList = termsFromDB.FindAll(term => term.Type == "TermGroup");
            if (termsList.Count == 0)
            {
                termsList = TermGroupDao.LoadTermGroup(false).ConvertAll<RMTermDto>(o => ConvertToTermDto(o));
            }
            this.BuildTermTree(termsFromDB, termsList);
            return termsList;
        }
        private void BuildTermTree(List<RMTermDto> source, List<RMTermDto> result)
        {
            foreach (RMTermDto term in result)
            {
                term.subTerms = this.GetTermsByParentId(source, term);
                term.subTermCount = term.subTerms.Count;
                this.BuildTermTree(source, term.subTerms);
            }
        }

        private List<RMTermDto> GetTermsByParentId(List<RMTermDto> source, RMTermDto subterm)
        {
            return source.FindAll(term => term.ParentId == subterm.UniqueId).OrderBy(t => t.Name).ToList();
        }
        #endregion

        private RMProfile ConvertProfileToDBModel(RMProfileDto dto)
        {
            RMProfile profile = new RMProfile()
            {
                Id = dto.Id,
                Name = dto.ProfileName,
                Description = dto.Description,
                Type = (int)dto.Type,
                Extension1 = dto.Extension1,
                Extension2 = dto.Extension2,
                IsCreated = dto.IsCreated,
                IsDestoryed = dto.IsDestoryed,
                RangeType = (int)dto.RangeType,
                Extension3 = dto.Extension3,
                ObjectLevel = dto.ObjectLevel,
                ScheduleId = dto.scheduleInfo?.Id ?? dto.ScheduleId,
                // Modified = DateTime.Parse(dto.Modified).ToUniversalTime().Ticks
                CreateProfileLogonUserId = (string.IsNullOrEmpty(dto.CreateProfileUserId) ? TenantLocalValue.LogonUserId : dto.CreateProfileUserId)
            };
            return profile;
        }
        //逻辑改动term tree回显不需要对比DB
        //private List<RMTermDto> BuildRMTermSetTree(string termJsonStr)
        //{
        //    try
        //    {
        //        Dictionary<int, RMTermDto> termDic = JsonConvert.DeserializeObject<Dictionary<int, RMTermDto>>(termJsonStr);

        //        logger.Info("begin BuildRMTermSetTree ");
        //        List<RMTermDto> newTermDto = new List<RMTermDto>();
        //        List<RMTermDto> newTermSets = TermSetDAO.LoadTermSet().ConvertAll<RMTermDto>(o => ConvertToTermDto(o));
        //        List<RMTermDto> allTermGroup = TermGroupDao.LoadTermGroup().ConvertAll<RMTermDto>(o => ConvertToTermDto(o));
        //        //term group
        //        foreach (RMTermDto termGroup in allTermGroup)
        //        {
        //            termGroup.expand = true;
        //            newTermDto.Add(termGroup);
        //        }

        //        logger.Info("begin Load TermSet ");
        //        if (newTermSets.Count == 0)
        //        {
        //            logger.Warn("There is no RMTermSet in RMDB. ");
        //            throw new Exception("There is no RMTermSet in RMDB.");
        //        }
        //        //assembly TermSet with term
        //        foreach (RMTermDto termSet in newTermSets)
        //        {
        //            //List<RMTermDto> allTerm = TermDAO.GetTermFromTermSet(termSet.Id);
        //            //.ConvertAll<RMTermDto>(o => ConvertToTermDto(o));
        //            List<RMTerm> allTerm = TermDao.GetTermFromTermSet(termSet.Id);
        //            List<RMTermDto> terms = new List<RMTermDto>();
        //            //只会有一个TermGroup所以取第一个
        //            termSet.ParentId = newTermDto[0].UniqueId;
        //            if (termDic.ContainsKey(-1))
        //            {
        //                var termFromDb = termDic[-1];
        //                termSet.IsChecked = termFromDb.IsChecked;
        //                termSet.expand = termFromDb.expand;
        //                termSet.pageIndex = termFromDb.pageIndex;
        //            }
        //            if (allTerm.Count != 0)
        //            {
        //                //termSet.subTerms =  allTerm;
        //                termSet.subTerms = terms;
        //                foreach (RMTerm rmTerm in allTerm)
        //                {
        //                    if (termDic.ContainsKey(rmTerm.Id))
        //                    {
        //                        RMTermDto dto = termDic[rmTerm.Id];
        //                        //MergeProperties(rmTerm, dto);
        //                        MergeProperties(rmTerm, dto);
        //                        if (dto.IsLoaded)
        //                        {
        //                            this.BuildRMTerm(rmTerm, dto, termDic);
        //                        }
        //                        terms.Add(dto);
        //                    }
        //                    //this.BuildRMTerm(rmTerm);
        //                }
        //            }
        //        }
        //        //目前就一个TermGroup,所以直接取第一个
        //        newTermDto[0].subTerms = newTermSets;
        //        logger.Info("BuildRMTermSetTree Complete.");

        //        return newTermDto;
        //    }
        //    catch (Exception e)
        //    {
        //        logger.Error("There are some error in buildRMTermSetTree {0}", e.ToString());
        //        throw;
        //    }
        //}
        private string ConvertToJson(List<RMTermDto> terms)
        {
            return JsonConvert.SerializeObject(terms);
        }

        #region convertToDto

        private RMTermDto ConvertToTermDto(RMTermGroup term)
        {
            RMTermDto termDto = new RMTermDto()
            {
                Id = term.Id,
                Name = term.Name,
                Description = term.Description,
                UniqueId = term.UniqueId.ToString(),
                Type = term.Type,
                subTermCount = term.subTermCount,
                expand = true,
            };
            return termDto;
        }

        #endregion
        //逻辑改动term tree回显不需要对比DB
        //private void BuildRMTerm(RMTerm term, RMTermDto rmTerm, Dictionary<int, RMTermDto> termDic)
        //{
        //    List<RMTerm> allSubTerm = TermDao.GetTermFromParentTermWithoutDeletedTerm(rmTerm.Id);
        //    List<RMTermDto> terms = new List<RMTermDto>();
        //    //.ConvertAll<RMTermDto>(o => ConvertToTermDto(o));
        //    if (allSubTerm.Count != 0)
        //    {
        //        rmTerm.subTerms = terms;
        //        foreach (RMTerm subTerm in allSubTerm)
        //        {
        //            if (termDic.ContainsKey(subTerm.Id))
        //            {

        //                RMTermDto dto = termDic[subTerm.Id];
        //                MergeProperties(subTerm, dto);
        //                terms.Add(dto);
        //                if (dto.IsLoaded)
        //                {
        //                    this.BuildRMTerm(subTerm, dto, termDic);
        //                }
        //            }
        //        }

        //    }
        //}



        private async Task<RMProfileDto> ConvertToProfileDtoAsync(RMProfile dto)
        {
            RMProfileDto profile = new RMProfileDto()
            {
                Id = dto.Id,
                ProfileName = dto.Name,
                Description = dto.Description,
                Type = (JobType)dto.Type,
                Extension1 = dto.Extension1,
                Extension2 = dto.Extension2,
                Modified = (await mGeneralSettingService.ConvertTiksToDateTimeAsync(dto.Modified, true)).SimplifyFormatTime,
                IsCreated = dto.IsCreated,
                IsDestoryed = dto.IsDestoryed,
                RangeType = (TimeRangeType)dto.RangeType,
                CreateProfileUserId = dto.CreateProfileLogonUserId,
                Source = dto.Source,
                ScheduleId = dto.ScheduleId,
                Extension3 = dto.Extension3,
                ObjectLevel = dto.ObjectLevel
            };
            return profile;
        }


        private void ModifyRuleChecker(Rule rule, RMTerm term, DateTime timePoint)
        {
            foreach (var filter in rule.Filters)
            {
                filter.SequenceNo = filter.SequenceNo + 1;
                if (filter.Rule is CreatedRule || filter.Rule is ModifiedRule
                    || filter.Rule is ColumnDateTimeRule)
                {
                    switch (filter.Condition)
                    {
                        case PolicyCondition.OlderThan:
                            int num;
                            DateTime tempDt = DateTime.UtcNow;
                            if (int.TryParse(filter.Value.Value1, out num))
                            {
                                if (filter.Value.Value1Unit == PolicyValueUnit.Days)
                                {
                                    tempDt = timePoint.AddDays(-num);
                                }
                                else if (filter.Value.Value1Unit == PolicyValueUnit.Weeks)
                                {
                                    tempDt = timePoint.AddDays(-num * 7);
                                }
                                else if (filter.Value.Value1Unit == PolicyValueUnit.Months)
                                {
                                    tempDt = timePoint.AddMonths(-num);
                                }
                                else if (filter.Value.Value1Unit == PolicyValueUnit.Years)
                                {
                                    tempDt = timePoint.AddYears(-num);
                                }
                                filter.Value.Value1 = tempDt.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                                filter.Condition = PolicyCondition.Before;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            rule.Filters.Add(new FilterPolicy()
            {
                Condition = PolicyCondition.Before,
                Level = rule.PolicyLevel,
                Rule = new CreatedRule() { Value1 = "Created Time" },
                RuleType = PolicyRuleType.CreatedTime,
                Value = new PolicyValue(timePoint.ToString(APIDateTimeFormat.DATETYPEForAPI003)),
                SequenceNo = 1
            });

            logger.Info($"Before convert and or express:{rule.AndOrExpression[rule.PolicyLevel]}");
            //have a bug here should change order Created Time to last
            var tempStrs = rule.AndOrExpression[rule.PolicyLevel].Split(new char[] { ' ', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
            string andOrExpression = "(1 And (";
            foreach (var str in tempStrs)
            {
                int sequenceNo = 0;
                if (int.TryParse(str, out sequenceNo))
                {
                    sequenceNo++;
                    andOrExpression = string.Format("{0} {1}", andOrExpression, sequenceNo.ToString());
                }
                else
                {
                    andOrExpression = string.Format("{0} {1}", andOrExpression, str);
                }
            }
            andOrExpression += "))";
            rule.AndOrExpression = new Dictionary<PolicyLevel, string>()
            {
                { rule.PolicyLevel, andOrExpression }
            };
            logger.Info($"After convert and or express:{rule.AndOrExpression[rule.PolicyLevel]}");
        }
        private RMRuleItem ConvertRuleChecker(Rule rule, RMTerm term, DateTime timePoint)
        {
            RMRuleItem checker = new RMRuleItem();
            checker.HasUnCamlQueryableCondition = false;
            checker.RuleId = rule.Id;
            checker.RuleName = rule.Name;
            checker.IsMoveRule = RuleHelper.CheckMoveRule(rule);
            checker.ArchiverAction = (RMContentDisposalAction)RuleHelper.GetOperationTypeForSP(rule);
            checker.IsManualApproval = rule.IsManualApproval;
            checker.ExportType = rule.ExportInfo == null ? GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None : rule.ExportInfo.exportType;
            checker.DeleteRecords = rule.DeleteRecords;
            checker.RelatedRecordOption = (GCommon.Contract.StorageOptimization.Object.RelatedRecordOption)rule.RelatedRecordOption;
            checker.RuleFilters = new List<ArchiverRuleFilter>();
            checker.DisposalClass = rule.DisposalClass;
            if (rule.SOFilters != null)
            {
                foreach (var filter in rule.SOFilters)
                {
                    var arFilter = new ArchiverRuleFilter(filter);
                    checker.RuleFilters.Add(arFilter);
                    //不支持SP Query的Rule Type，HasUnCamlQueryableCondition赋值为true
                    if (!checker.HasUnCamlQueryableCondition)
                    {
                        if (arFilter.Condition == ArchiverFilterCondition.Matches || arFilter.Condition == ArchiverFilterCondition.DoesNotMatch || arFilter.Condition == ArchiverFilterCondition.DoesNotContain)
                        {
                            checker.HasUnCamlQueryableCondition = true;
                        }
                        else if (arFilter.RuleType == ArchiverFilterRuleType.ContentType && arFilter.Condition == ArchiverFilterCondition.Contains)
                        {
                            checker.HasUnCamlQueryableCondition = true;
                        }
                        else if (arFilter.RuleType == ArchiverFilterRuleType.CreatedBy || arFilter.RuleType == ArchiverFilterRuleType.ModifiedBy
                                 || arFilter.RuleType == ArchiverFilterRuleType.ParentListTypeID || arFilter.RuleType == ArchiverFilterRuleType.LastAccessedTime || arFilter.RuleType == ArchiverFilterRuleType.LastActiveTime
                                 || arFilter.RuleType == ArchiverFilterRuleType.ParentFolderName || arFilter.RuleType == ArchiverFilterRuleType.ParentFolderNameHeirarchically
                                 //Metadata Column Calm Query暂不支持
                                 || arFilter.RuleType == ArchiverFilterRuleType.MetadataTextColumn || arFilter.RuleType == ArchiverFilterRuleType.MetadataNumberColumn
                                 || arFilter.RuleType == ArchiverFilterRuleType.ParentLibraryName || arFilter.RuleType == ArchiverFilterRuleType.SensitivityLabelFullName
                                 || arFilter.RuleType == ArchiverFilterRuleType.SensitivityLabel || IsParentRule(arFilter.RuleType) || arFilter.RuleType == ArchiverFilterRuleType.OrphanedFolderRule)
                        {
                            checker.HasUnCamlQueryableCondition = true;
                        }
                    }
                }
            }
            if (rule.Filters != null)
            {
                foreach (var filter in rule.Filters)
                {
                    filter.SequenceNo = filter.SequenceNo + 1;
                    if (filter.Rule is ContentTypeRule)
                    {
                        filter.RuleType = PolicyRuleType.ContentType;
                    }
                    if (filter.Rule is CreatedRule || filter.Rule is ModifiedRule
                        || filter.Rule is ColumnDateTimeRule || filter.Rule is StubLastAccessTimeRule || filter.Rule is StubLastActiveTimeRule)
                    {
                        switch (filter.Condition)
                        {
                            // [REC-738] remove timepoint ref FromTo/Before
                            //case PolicyCondition.FromTo:
                            //    var fromDt = ConvertUtcDateTime(filter.Value.Value1);
                            //    var toDt = ConvertUtcDateTime(filter.Value.Value2);
                            //    if (toDt > timePoint)
                            //    {
                            //        filter.Value.Value2 = timePoint.ToString(AveDateTimeUtility.DATETYPEForAPI003);
                            //    }
                            //    break;
                            //case PolicyCondition.Before:
                            //    var ltDt = ConvertUtcDateTime(filter.Value.Value1);
                            //    if (ltDt >= timePoint)
                            //    {
                            //        filter.Value.Value1 = timePoint.ToString(AveDateTimeUtility.DATETYPEForAPI003);
                            //    }
                            //    break;
                            case PolicyCondition.OlderThan:
                                int num;
                                DateTime tempDt = DateTime.UtcNow;
                                if (int.TryParse(filter.Value.Value1, out num))
                                {
                                    try
                                    {

                                        if (filter.Value.Value1Unit == PolicyValueUnit.Days)
                                        {
                                            tempDt = timePoint.AddDays(-num);
                                        }
                                        else if (filter.Value.Value1Unit == PolicyValueUnit.Weeks)
                                        {
                                            tempDt = timePoint.AddDays(-num * 7);
                                        }
                                        else if (filter.Value.Value1Unit == PolicyValueUnit.Months)
                                        {
                                            tempDt = timePoint.AddMonths(-num);
                                        }
                                        else if (filter.Value.Value1Unit == PolicyValueUnit.Years)
                                        {
                                            tempDt = timePoint.AddYears(-num);
                                        }
                                    }
                                    catch (ArgumentOutOfRangeException)
                                    {
                                        logger.Warn($"The filter policy no.{filter.SequenceNo} of rule [{rule.Id}], name: [{rule.Name}] has time value less than min datetime. Force using min datetime");
                                        tempDt = DateTime.MinValue.AddDays(1); // avoid exception from converting to negative time zone
                                    }

                                    filter.Value.Value1 = tempDt.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                                    filter.Condition = PolicyCondition.Before;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }

                rule.Filters.Add(new FilterPolicy()
                {
                    Condition = PolicyCondition.Before,
                    Level = rule.PolicyLevel,
                    Rule = new CreatedRule() { Value1 = "Created Time" },
                    RuleType = PolicyRuleType.CreatedTime,
                    Value = new PolicyValue(timePoint.ToString(APIDateTimeFormat.DATETYPEForAPI003)),
                    SequenceNo = 1
                });
            }
            logger.Info($"Before convert and or express:{rule.AndOrExpression[rule.PolicyLevel]}");
            //have a bug here should change order Created Time to last
            var tempStrs = rule.AndOrExpression[rule.PolicyLevel].Split(new char[] { ' ', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
            string andOrExpression = "(1 And (";
            foreach (var str in tempStrs)
            {
                int sequenceNo = 0;
                if (int.TryParse(str, out sequenceNo))
                {
                    sequenceNo++;
                    andOrExpression = string.Format("{0} {1}", andOrExpression, sequenceNo.ToString());
                }
                else
                {
                    andOrExpression = string.Format("{0} {1}", andOrExpression, str);
                }
            }
            andOrExpression += "))";
            rule.AndOrExpression = new Dictionary<PolicyLevel, string>()
            {
                { rule.PolicyLevel, andOrExpression }
            };
            logger.Info($"After convert and or express:{rule.AndOrExpression[rule.PolicyLevel]}");
            return checker;
        }

        private bool IsParentRule(ArchiverFilterRuleType ruleType)
        {
            var parentRule = new List<ArchiverFilterRuleType> { ArchiverFilterRuleType.ParentLibraryText, ArchiverFilterRuleType.ParentLibraryNumber, ArchiverFilterRuleType.ParentLibraryBoolean, ArchiverFilterRuleType.ParentLibraryDateTime,
                ArchiverFilterRuleType.ParentSiteCollectionText, ArchiverFilterRuleType.ParentSiteCollectionNumber, ArchiverFilterRuleType.ParentSiteCollectionBoolean, ArchiverFilterRuleType.ParentSiteCollectionDateTime,
                 ArchiverFilterRuleType.PropertyBagText, ArchiverFilterRuleType.PropertyBagNumber, ArchiverFilterRuleType.PropertyBagBoolean, ArchiverFilterRuleType.PropertyBagDateTime};
            return parentRule.Contains(ruleType);
        }
        private RMRuleItem ConvertRuleCheckerForOneDrive(Rule rule, RMTerm term, DateTime timePoint)
        {
            RMRuleItem checker = new RMRuleItem();
            checker.HasUnCamlQueryableCondition = false;
            checker.RuleId = rule.Id;
            checker.RuleName = rule.Name;
            checker.IsMoveRule = RuleHelper.CheckMoveRule(rule);
            checker.ArchiverAction = (RMContentDisposalAction)RuleHelper.GetOperationTypeForOneDrive(rule);
            checker.IsManualApproval = rule.IsManualApproval;
            checker.ExportType = rule.ExportInfo == null ? GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None : rule.ExportInfo.exportType;
            checker.DeleteRecords = rule.DeleteRecords;
            checker.RelatedRecordOption = (GCommon.Contract.StorageOptimization.Object.RelatedRecordOption)rule.RelatedRecordOption;
            checker.RuleFilters = new List<ArchiverRuleFilter>();
            checker.DisposalClass = rule.DisposalClass;
            if (rule.SOFilters != null)
            {
                foreach (var filter in rule.SOFilters)
                {
                    var arFilter = new ArchiverRuleFilter(filter);
                    checker.RuleFilters.Add(arFilter);
                    //不支持SP Query的Rule Type，HasUnCamlQueryableCondition赋值为true
                    if (!checker.HasUnCamlQueryableCondition)
                    {
                        if (arFilter.Condition == ArchiverFilterCondition.Matches || arFilter.Condition == ArchiverFilterCondition.DoesNotMatch || arFilter.Condition == ArchiverFilterCondition.DoesNotContain)
                        {
                            checker.HasUnCamlQueryableCondition = true;
                        }
                        else if (arFilter.RuleType == ArchiverFilterRuleType.ContentType && arFilter.Condition == ArchiverFilterCondition.Contains)
                        {
                            checker.HasUnCamlQueryableCondition = true;
                        }
                        else if (arFilter.RuleType == ArchiverFilterRuleType.CreatedBy || arFilter.RuleType == ArchiverFilterRuleType.ModifiedBy
                                 || arFilter.RuleType == ArchiverFilterRuleType.ParentListTypeID || arFilter.RuleType == ArchiverFilterRuleType.LastAccessedTime || arFilter.RuleType == ArchiverFilterRuleType.LastActiveTime
                                 || arFilter.RuleType == ArchiverFilterRuleType.ParentFolderName || arFilter.RuleType == ArchiverFilterRuleType.ParentFolderNameHeirarchically
                                 //Metadata Column Calm Query暂不支持
                                 || arFilter.RuleType == ArchiverFilterRuleType.MetadataTextColumn || arFilter.RuleType == ArchiverFilterRuleType.MetadataNumberColumn
                                 || arFilter.RuleType == ArchiverFilterRuleType.SensitivityLabel || arFilter.RuleType == ArchiverFilterRuleType.SensitivityLabelFullName)
                        {
                            checker.HasUnCamlQueryableCondition = true;
                        }
                    }
                }
            }
            if (rule.Filters != null)
            {
                foreach (var filter in rule.Filters)
                {
                    filter.SequenceNo = filter.SequenceNo + 1;
                    if (filter.Rule is ContentTypeRule)
                    {
                        filter.RuleType = PolicyRuleType.ContentType;
                    }
                    if (filter.Rule is CreatedRule || filter.Rule is ModifiedRule
                        || filter.Rule is ColumnDateTimeRule || filter.Rule is StubLastAccessTimeRule || filter.Rule is StubLastActiveTimeRule)
                    {
                        switch (filter.Condition)
                        {
                            // [REC-738] remove timepoint ref FromTo/Before
                            //case PolicyCondition.FromTo:
                            //    var fromDt = ConvertUtcDateTime(filter.Value.Value1);
                            //    var toDt = ConvertUtcDateTime(filter.Value.Value2);
                            //    if (toDt > timePoint)
                            //    {
                            //        filter.Value.Value2 = timePoint.ToString(AveDateTimeUtility.DATETYPEForAPI003);
                            //    }
                            //    break;
                            //case PolicyCondition.Before:
                            //    var ltDt = ConvertUtcDateTime(filter.Value.Value1);
                            //    if (ltDt >= timePoint)
                            //    {
                            //        filter.Value.Value1 = timePoint.ToString(AveDateTimeUtility.DATETYPEForAPI003);
                            //    }
                            //    break;
                            case PolicyCondition.OlderThan:
                                int num;
                                DateTime tempDt = DateTime.UtcNow;
                                if (int.TryParse(filter.Value.Value1, out num))
                                {
                                    if (filter.Value.Value1Unit == PolicyValueUnit.Days)
                                    {
                                        tempDt = timePoint.AddDays(-num);
                                    }
                                    else if (filter.Value.Value1Unit == PolicyValueUnit.Weeks)
                                    {
                                        tempDt = timePoint.AddDays(-num * 7);
                                    }
                                    else if (filter.Value.Value1Unit == PolicyValueUnit.Months)
                                    {
                                        tempDt = timePoint.AddMonths(-num);
                                    }
                                    else if (filter.Value.Value1Unit == PolicyValueUnit.Years)
                                    {
                                        tempDt = timePoint.AddYears(-num);
                                    }
                                    filter.Value.Value1 = tempDt.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                                    filter.Condition = PolicyCondition.Before;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }

                rule.Filters.Add(new FilterPolicy()
                {
                    Condition = PolicyCondition.Before,
                    Level = rule.PolicyLevel,
                    Rule = new CreatedRule() { Value1 = "Created Time" },
                    RuleType = PolicyRuleType.CreatedTime,
                    Value = new PolicyValue(timePoint.ToString(APIDateTimeFormat.DATETYPEForAPI003)),
                    SequenceNo = 1
                });
            }

            logger.Info($"Before convert and or express:{rule.AndOrExpression[rule.PolicyLevel]}");
            //have a bug here should change order Created Time to last
            var tempStrs = rule.AndOrExpression[rule.PolicyLevel].Split(new char[] { ' ', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
            string andOrExpression = "(1 And (";
            foreach (var str in tempStrs)
            {
                int sequenceNo = 0;
                if (int.TryParse(str, out sequenceNo))
                {
                    sequenceNo++;
                    andOrExpression = string.Format("{0} {1}", andOrExpression, sequenceNo.ToString());
                }
                else
                {
                    andOrExpression = string.Format("{0} {1}", andOrExpression, str);
                }
            }
            andOrExpression += "))";
            rule.AndOrExpression = new Dictionary<PolicyLevel, string>()
            {
                { rule.PolicyLevel, andOrExpression }
            };
            logger.Info($"After convert and or express:{rule.AndOrExpression[rule.PolicyLevel]}");
            return checker;
        }

        private RMRuleItem ConvertRuleCheckerForGoogleDrive(Rule rule, RMTerm term, DateTime timePoint)
        {
            RMRuleItem checker = new RMRuleItem();
            checker.HasUnCamlQueryableCondition = false;
            checker.RuleId = rule.Id;
            checker.RuleName = rule.Name;
            checker.IsMoveRule = RuleHelper.CheckMoveRule(rule);
            checker.ArchiverAction = RMContentDisposalAction.None;
            checker.IsManualApproval = rule.IsManualApproval;
            checker.ExportType = rule.ExportInfo == null ? GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None : rule.ExportInfo.exportType;
            checker.DeleteRecords = rule.DeleteRecords;
            checker.RelatedRecordOption = (GCommon.Contract.StorageOptimization.Object.RelatedRecordOption)rule.RelatedRecordOption;
            checker.RuleFilters = new List<ArchiverRuleFilter>();
            checker.DisposalClass = rule.DisposalClass;

            return checker;
        }
        private RMRuleItem ConvertRuleCheckerForTeams(Rule rule, RMTerm term, DateTime timePoint)
        {
            RMRuleItem checker = new RMRuleItem();
            checker.HasUnCamlQueryableCondition = false;
            checker.RuleId = rule.Id;
            checker.RuleName = rule.Name;
            checker.IsMoveRule = RuleHelper.CheckMoveRule(rule);
            checker.ArchiverAction = (RMContentDisposalAction)RuleHelper.GetOperationTypeForTeams(rule);
            checker.IsManualApproval = rule.IsManualApproval;
            checker.ExportType = rule.ExportInfo == null ? GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None : rule.ExportInfo.exportType;
            checker.DeleteRecords = rule.DeleteRecords;
            checker.RelatedRecordOption = (GCommon.Contract.StorageOptimization.Object.RelatedRecordOption)rule.RelatedRecordOption;
            checker.RuleFilters = new List<ArchiverRuleFilter>();
            checker.DisposalClass = rule.DisposalClass;
            if (rule.TeamsRule.SOFilters != null)
            {
                foreach (var filter in rule.TeamsRule.SOFilters)
                {
                    var arFilter = new ArchiverRuleFilter(filter);
                    checker.RuleFilters.Add(arFilter);
                    //不支持SP Query的Rule Type，HasUnCamlQueryableCondition赋值为true
                    if (!checker.HasUnCamlQueryableCondition)
                    {
                        if (arFilter.Condition == ArchiverFilterCondition.Matches || arFilter.Condition == ArchiverFilterCondition.DoesNotMatch || arFilter.Condition == ArchiverFilterCondition.DoesNotContain)
                        {
                            checker.HasUnCamlQueryableCondition = true;
                        }
                        else if (arFilter.RuleType == ArchiverFilterRuleType.ContentType && arFilter.Condition == ArchiverFilterCondition.Contains)
                        {
                            checker.HasUnCamlQueryableCondition = true;
                        }
                        else if (arFilter.RuleType == ArchiverFilterRuleType.CreatedBy || arFilter.RuleType == ArchiverFilterRuleType.ModifiedBy
                                 || arFilter.RuleType == ArchiverFilterRuleType.ParentListTypeID || arFilter.RuleType == ArchiverFilterRuleType.LastAccessedTime || arFilter.RuleType == ArchiverFilterRuleType.LastActiveTime
                                 || arFilter.RuleType == ArchiverFilterRuleType.ParentFolderName || arFilter.RuleType == ArchiverFilterRuleType.ParentFolderNameHeirarchically
                                 //Metadata Column Calm Query暂不支持
                                 || arFilter.RuleType == ArchiverFilterRuleType.MetadataTextColumn || arFilter.RuleType == ArchiverFilterRuleType.MetadataNumberColumn)
                        {
                            checker.HasUnCamlQueryableCondition = true;
                        }
                    }
                }
            }
            if (rule.Filters != null)
            {
                foreach (var filter in rule.Filters)
                {
                    filter.SequenceNo = filter.SequenceNo + 1;
                    if (filter.Rule is ContentTypeRule)
                    {
                        filter.RuleType = PolicyRuleType.ContentType;
                    }
                    if (filter.Rule is CreatedRule || filter.Rule is ModifiedRule
                        || filter.Rule is ColumnDateTimeRule || filter.Rule is StubLastAccessTimeRule || filter.Rule is StubLastActiveTimeRule)
                    {
                        switch (filter.Condition)
                        {
                            // [REC-738] remove timepoint ref FromTo/Before
                            //case PolicyCondition.FromTo:
                            //    var fromDt = ConvertUtcDateTime(filter.Value.Value1);
                            //    var toDt = ConvertUtcDateTime(filter.Value.Value2);
                            //    if (toDt > timePoint)
                            //    {
                            //        filter.Value.Value2 = timePoint.ToString(AveDateTimeUtility.DATETYPEForAPI003);
                            //    }
                            //    break;
                            //case PolicyCondition.Before:
                            //    var ltDt = ConvertUtcDateTime(filter.Value.Value1);
                            //    if (ltDt >= timePoint)
                            //    {
                            //        filter.Value.Value1 = timePoint.ToString(AveDateTimeUtility.DATETYPEForAPI003);
                            //    }
                            //    break;
                            case PolicyCondition.OlderThan:
                                int num;
                                DateTime tempDt = DateTime.UtcNow;
                                if (int.TryParse(filter.Value.Value1, out num))
                                {
                                    if (filter.Value.Value1Unit == PolicyValueUnit.Days)
                                    {
                                        tempDt = timePoint.AddDays(-num);
                                    }
                                    else if (filter.Value.Value1Unit == PolicyValueUnit.Weeks)
                                    {
                                        tempDt = timePoint.AddDays(-num * 7);
                                    }
                                    else if (filter.Value.Value1Unit == PolicyValueUnit.Months)
                                    {
                                        tempDt = timePoint.AddMonths(-num);
                                    }
                                    else if (filter.Value.Value1Unit == PolicyValueUnit.Years)
                                    {
                                        tempDt = timePoint.AddYears(-num);
                                    }
                                    filter.Value.Value1 = tempDt.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                                    filter.Condition = PolicyCondition.Before;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }

                rule.Filters.Add(new FilterPolicy()
                {
                    Condition = PolicyCondition.Before,
                    Level = rule.PolicyLevel,
                    Rule = new CreatedRule() { Value1 = "Created Time" },
                    RuleType = PolicyRuleType.CreatedTime,
                    Value = new PolicyValue(timePoint.ToString(APIDateTimeFormat.DATETYPEForAPI003)),
                    SequenceNo = 1
                });
            }

            logger.Info($"Before convert and or express:{rule.AndOrExpression[rule.PolicyLevel]}");
            var tempStrs = rule.AndOrExpression[rule.PolicyLevel].Split(new char[] { ' ', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
            string andOrExpression = "(1 And (";
            foreach (var str in tempStrs)
            {
                int sequenceNo = 0;
                if (int.TryParse(str, out sequenceNo))
                {
                    sequenceNo++;
                    andOrExpression = string.Format("{0} {1}", andOrExpression, sequenceNo.ToString());
                }
                else
                {
                    andOrExpression = string.Format("{0} {1}", andOrExpression, str);
                }
            }
            andOrExpression += "))";
            rule.AndOrExpression = new Dictionary<PolicyLevel, string>()
            {
                { rule.PolicyLevel, andOrExpression }
            };
            logger.Info($"After convert and or express:{rule.AndOrExpression[rule.PolicyLevel]}");
            return checker;
        }

        /// <summary>
        /// 为了在转换的时候，取时间格式化字符串只读取一次DB
        /// </summary>
        /// <param name="timeZoneName"></param>
        private void ResetDateTimeFormat()
        {
            _datetimeFormat = null;
        }

        private void SetDateTimeFormat(GeneralSettingModel gls)
        {
            if (_datetimeFormat == null)
            {
                _datetimeFormat = mGeneralSettingService.GetDateTimeFormat(gls);
            }
        }

        private string ConverTicksToString(long ticks, string timeZoneName, GeneralSettingModel gls)
        {
            if (_datetimeFormat == null)
            {
                SetDateTimeFormat(gls);
            }
            if (string.IsNullOrEmpty(timeZoneName))
            {
                return ticks == 0 ? "" : mGeneralSettingService.ConvertTiksToDateTime(gls, ticks, true).SimplifyFormatTime;
            }
            else
            {

                Regex reg = new Regex(@"\(.*?\)");
                var matchResult = reg.Match(timeZoneName);
                return ticks == 0 ? "" : new DateTime(ticks).ToString(_datetimeFormat) + "  " + matchResult.Value;
            }
        }

        #endregion

        #region Discover term for report
        public async Task<Dictionary<Guid, RMTermIdentity>> GetTermIDsFromBCSTermTreeAsync(string ext1)
        {
            Dictionary<Guid, RMTermIdentity> termIdEntity = new Dictionary<Guid, RMTermIdentity>();
            try
            {
                Dictionary<int, RMTermDto> termDic = JsonConvert.DeserializeObject<Dictionary<int, RMTermDto>>(ext1);
                List<Guid> needDelTermIds = new List<Guid>();
                logger.Info("Begin build RMTermSet Tree for BCS Term Usage Report.");
                List<RMTermGroup> termGroup = TermGroupDao.LoadTermGroup(false);
                foreach (var group in termGroup)
                {
                    List<RMTermSet> newTermSets = await TermSetDAO.LoadTermSetAsync(TermSetType.Business, group.UniqueId);
                    if (newTermSets.Count == 0)
                    {
                        logger.Warn("There is no RMTermSet in RMDB. group name:{0} ", group.Name);
                        continue;
                        //throw new Exception("There is no RMTermSet in RMDB.");
                    }
                    //assembly TermSet with term
                    foreach (RMTermSet termSet in newTermSets)
                    {
                        var termSetPath = group.Name + "/" + termSet.Name;
                        List<RMTerm> allTerm = TermDao.GetTermFromTermSetWithoutDeletedTerm(termSet.Id);
                        List<RMTermDto> terms = new List<RMTermDto>();
                        RMTermDto termSetDto = null;
                        //只会有一个TermSet所以取第一个
                        if (termDic.ContainsKey(-termSet.Id))
                        {
                            termSetDto = termDic[-termSet.Id];
                            if (termDic.Count == 1 && termSetDto.IsChecked)
                            {
                                //只勾选TermSet
                                DiscoverAllTerm(termSetPath, RMTermStatus.Avaliable, allTerm, termDic, ref termIdEntity);
                            }
                            else
                            {
                                DiscoverTerm(termSetPath, termSetDto, RMTermStatus.Avaliable, allTerm, termDic, ref termIdEntity);
                            }
                        }
                        //else
                        //{
                        //    throw new Exception("no term cache.");
                        //}
                        //cache need remove orphan term ids
                        List<RMTerm> orphanedTerms = TermDao.GetOrphanedTerms(termSet.Id);
                        if (orphanedTerms != null && orphanedTerms.Count > 0)
                        {
                            foreach (var term in termIdEntity.Values)
                            {
                                if (orphanedTerms.Where(t => t.UniqueId.Equals(term.UniqueId)).FirstOrDefault() != null)
                                {
                                    needDelTermIds.Add(term.UniqueId);
                                }
                            }
                        }

                        List<RMTerm> retiredTerms = TermDao.GetretiredTerms(termSet.Id);
                        if (retiredTerms != null && retiredTerms.Count > 0)
                        {
                            foreach (var term in termIdEntity.Values)
                            {
                                if (retiredTerms.Where(t => t.UniqueId.Equals(term.UniqueId)).FirstOrDefault() != null)
                                {
                                    needDelTermIds.Add(term.UniqueId);
                                }
                            }
                        }
                    }
                }
                logger.Info("build RMTermSet Tree for BCS Term Usage Report Complete.");

                if (termIdEntity == null || termIdEntity.Count == 0)
                {
                    //throw new Exception("no term cache.");
                    logger.Warn($"no term for this report");
                    return termIdEntity;
                }
                //remove orphan term
                foreach (var id in needDelTermIds)
                {
                    if (termIdEntity.ContainsKey(id))
                    {
                        termIdEntity.Remove(id);
                    }
                }
                return termIdEntity;
            }
            catch (Exception e)
            {
                logger.Error("There are some error in build RMTermSet Tree for BCS Term Usage Report,ERROR: {0}", e.ToString());
                throw;
            }

        }

        private RMTermStatus GetTermStatus(RMTerm term, RMTermStatus parentStatus)
        {
            RMTermStatus status = RMTermStatus.Avaliable;
            if (term.IsDeprecated)
            {
                status = RMTermStatus.Retired;
            }
            else if (term.IsRemoved)
            {
                status = RMTermStatus.Removed;
            }
            else if (term.TermExpirationFrom > 0 || term.TermExpirationTo > 0)
            {
                long utcNow = DateTime.UtcNow.Ticks;
                if (term.TermExpirationFrom > 0 && utcNow < term.TermExpirationFrom)
                {
                    status = RMTermStatus.Retired;
                }
                if (term.TermExpirationTo > 0 && utcNow > term.TermExpirationTo)
                {
                    status = RMTermStatus.Retired;
                }
            }
            else if (term.BreakInheritFromParent && !(term.TermExpirationFrom > 0 || term.TermExpirationTo > 0))
            {
                status = RMTermStatus.Avaliable;
            }
            else if (!parentStatus.Equals(RMTermStatus.Retired))
            {
                status = parentStatus;
            }

            return status;
        }

        private void DiscoverTerm(string parentTermPath, RMTermDto parentDto, RMTermStatus parentStatus, List<RMTerm> subTerms, Dictionary<int, RMTermDto> termDic, ref Dictionary<Guid, RMTermIdentity> termIdEntity)
        {
            bool selectAll = parentDto.IsChecked && parentDto.IsLeafNode;

            foreach (RMTerm subTerm in subTerms)
            {
                string termFullPath = parentTermPath + "/" + subTerm.Name;
                List<RMTerm> allSubTerm = TermDao.GetTermFromParentTermWithoutDeletedTerm(subTerm.Id);
                RMTermDto subTermDto;

                if (selectAll)
                {
                    var indentity = new RMTermIdentity()
                    {
                        UniqueId = subTerm.UniqueId,
                        Name = subTerm.Name,
                        FullPath = termFullPath,
                        Status = GetTermStatus(subTerm, parentStatus)
                    };
                    termIdEntity.Add(subTerm.UniqueId, indentity);
                    DiscoverAllTerm(termFullPath, indentity.Status, allSubTerm, termDic, ref termIdEntity);
                }
                else if (termDic.TryGetValue(subTerm.Id, out subTermDto))
                {
                    if (subTermDto.IsChecked)
                    {
                        var indentity = new RMTermIdentity()
                        {
                            UniqueId = subTerm.UniqueId,
                            Name = subTerm.Name,
                            FullPath = termFullPath,
                            Status = GetTermStatus(subTerm, parentStatus)
                        };
                        termIdEntity.Add(subTerm.UniqueId, indentity);
                        DiscoverTerm(termFullPath, subTermDto, indentity.Status, allSubTerm, termDic, ref termIdEntity);
                    }
                    else if (!subTermDto.IsLeafNode)//没有勾选该节点,但节点已load过,需要check子节点勾选情况
                    {
                        DiscoverTerm(termFullPath, subTermDto, GetTermStatus(subTerm, parentStatus), allSubTerm, termDic, ref termIdEntity);
                    }
                }
            }
        }

        private void DiscoverAllTerm(string parentTermPath, RMTermStatus parentStatus, List<RMTerm> subTerms, Dictionary<int, RMTermDto> termDic, ref Dictionary<Guid, RMTermIdentity> termIdEntity)
        {
            foreach (RMTerm subTerm in subTerms)
            {
                string termFullPath = parentTermPath + "/" + subTerm.Name;
                var identity = new RMTermIdentity()
                {
                    UniqueId = subTerm.UniqueId,
                    Name = subTerm.Name,
                    FullPath = termFullPath,
                    Status = GetTermStatus(subTerm, parentStatus)
                };
                termIdEntity.Add(subTerm.UniqueId, identity);
                List<RMTerm> allSubTerm = TermDao.GetTermFromParentTermWithoutDeletedTerm(subTerm.Id);
                DiscoverAllTerm(termFullPath, identity.Status, allSubTerm, termDic, ref termIdEntity);
            }

        }


        #endregion

        #region public method of OrphanedTerm
        /// <summary>
        /// 获取RM中的OrphanedTerms
        /// </summary>
        /// <returns></returns>
        public async Task<Dictionary<Guid, RMTermIdentity>> GetOrphanedTermsOfRMAsync()
        {
            Dictionary<Guid, RMTermIdentity> termIdEntity = new Dictionary<Guid, RMTermIdentity>();
            List<RMTermGroup> termGroups = TermGroupDao.LoadTermGroup();
            foreach (var group in termGroups)
            {
                List<RMTermSet> newTermSets = await TermSetDAO.LoadTermSetAsync(TermSetType.Business, group.UniqueId);
                foreach (RMTermSet termSet in newTermSets)
                {
                    List<RMTerm> orphanedTerms = TermDao.GetOrphanedTerms(termSet.Id);
                    foreach (var term in orphanedTerms)
                    {
                        var identity = new RMTermIdentity()
                        {
                            UniqueId = term.UniqueId,
                            Name = term.Name,
                            FullPath = TermDao.GetTermNamesPathByTermId(term.UniqueId),
                            Status = GetOrphanedAndRetiredTermStatus(term)
                        };
                        termIdEntity.Add(term.UniqueId, identity);
                    }
                }
            }
            return termIdEntity;
        }

        public async Task<Dictionary<Guid, RMTermIdentity>> GetRetiredTermsOfRMAsync()
        {
            Dictionary<Guid, RMTermIdentity> termIdEntity = new Dictionary<Guid, RMTermIdentity>();
            List<RMTermGroup> termGroups = TermGroupDao.LoadTermGroup();
            foreach (var group in termGroups)
            {
                List<RMTermSet> newTermSets = await TermSetDAO.LoadTermSetAsync(TermSetType.Business, group.UniqueId);
                foreach (RMTermSet termSet in newTermSets)
                {
                    List<RMTerm> retiredTerms = TermDao.GetretiredTerms(termSet.Id);
                    foreach (var term in retiredTerms)
                    {
                        var identity = new RMTermIdentity()
                        {
                            UniqueId = term.UniqueId,
                            Name = term.Name,
                            FullPath = TermDao.GetTermNamesPathByTermId(term.UniqueId),
                            Status = GetOrphanedAndRetiredTermStatus(term)
                        };
                        termIdEntity.Add(term.UniqueId, identity);
                    }
                }
            }
            return termIdEntity;
        }
        /// <summary>
        /// 获取OrphanedTerm的状态 remove和Deprecated都显示成Retired
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        private RMTermStatus GetOrphanedAndRetiredTermStatus(RMTerm term)
        {
            RMTermStatus status = RMTermStatus.Retired;
            if (term.IsRemoved)
            {
                status = RMTermStatus.Removed;
            }
            else if (term.IsDeprecated)
            {
                status = RMTermStatus.Retired;
            }
            else
            {
                RMTerm returnTerm = TermDao.GetTermTimeSettings(term.Id);
                long utcNow = DateTime.UtcNow.Ticks;
                if (returnTerm.TermExpirationFrom > 0 && utcNow < returnTerm.TermExpirationFrom)
                {
                    status = RMTermStatus.Retired;
                }
                if (returnTerm.TermExpirationTo > 0 && utcNow > returnTerm.TermExpirationTo)
                {
                    status = RMTermStatus.Retired;
                }
            }
            return status;
        }

        public async Task<ShowProfilesReportPageInfo> GetTermUsageAndOrphanedTermProfilesAsync(ShowProfilesReportPageInfo pageInfo)
        {
            int totalRecord = 0;

            var isSPAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.SPOEnduser);
            var isEXOAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.EXOEnduser);
            var isPhyAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.PhysicalAdmin);
            var isFSAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.FSAdmin);
            var isOneDriveAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.OneDriveEnduser);
            var isSPOnPremAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.SPOnPremEnduser);
            var isBoxAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.BoxAdmin);
            var isGoogleAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.GoogleAdmin);
            var isTeamsAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.TeamsEndUser);

            var isEnableJPMCFeature = RMKeyValueDao.IsEnableJPMCFileSystemFeature();

            var termUsageReportTypeList = new List<int>()
            {
                (int)JobType.TermUsageReport
            };

            var sources = new List<SourceFlag>
            {
                SourceFlag.All
            };

            if (isSPAdmin)
            {
                termUsageReportTypeList.AddRange(new List<int>() { (int)JobType.BCSTermUsageReport, (int)JobType.OrphanedTermReport, (int)JobType.RetiredTermReport});
                sources.Add(SourceFlag.SharePoint);
            }

            if (isEXOAdmin)
            {
                termUsageReportTypeList.AddRange(new List<int>() { (int)JobType.EXOTermUsageReport, (int)JobType.EXOOrphanedTermUsageReport, (int)JobType.EXORetiredTermUsageReport });
                sources.Add(SourceFlag.Exchange);
            }

            if (isPhyAdmin)
            {
                termUsageReportTypeList.AddRange(new List<int>() { (int)JobType.PhysicalTermUsageReport, (int)JobType.PhysicalOrphanedTermUsageReport, (int)JobType.PhysicalRetiredTermUsageReport });
                sources.Add(SourceFlag.Physical);
            }

            if (isFSAdmin && !isEnableJPMCFeature)
            {
                termUsageReportTypeList.AddRange(new List<int>() { (int)JobType.FSBCSTermUsageReport, (int)JobType.FSOrphanedTermReport, (int)JobType.FSRetiredTermReport });
                sources.Add(SourceFlag.FileSystem);
            }

            if (isOneDriveAdmin)
            {
                termUsageReportTypeList.AddRange(new List<int>() { (int)JobType.OneDriveTermUsageReport, (int)JobType.OneDriveOrphanedTermUsageReport, (int)JobType.OneDriveRetiredTermUsageReport });
                sources.Add(SourceFlag.OneDrive);
            }

            if (isSPOnPremAdmin)
            {
                termUsageReportTypeList.AddRange(new List<int>() { (int)JobType.SPOnPremBCSTermUsageReport, (int)JobType.SPOnPremOrphanedTermReport, (int)JobType.SPOnPremRetiredTermReport });
                sources.Add(SourceFlag.SharePointOnPrem);
            }

            if (isBoxAdmin)
            {
                termUsageReportTypeList.AddRange(new List<int>() { (int)JobType.BoxBCSTermUsageReport, (int)JobType.BoxOrphanedTermUsageReport, (int)JobType.BoxRetiredTermUsageReport });
                sources.Add(SourceFlag.Box);
            }

            if (isGoogleAdmin)
            {
                termUsageReportTypeList.AddRange(new List<int>() { (int)JobType.GoogleBCSTermUsageReport, (int)JobType.GoogleOrphanedTermUsageReport, (int)JobType.GoogleRetiredTermUsageReport });
            }

            if (isTeamsAdmin)
            {
                termUsageReportTypeList.AddRange(new List<int>() { (int)JobType.TeamsBCSTermUsageReport, (int)JobType.TeamsOrphanedTermUsageReport, (int)JobType.TeamsRetiredTermUsageReport });
                sources.Add(SourceFlag.Teams);
            }

            List<Expression> allExpressionList = new List<Expression>();
            ParameterExpression param = Expression.Parameter(typeof(RMProfile), "c");

            allExpressionList.Add(Expression4DynamicQuery.GetEqualExpression(typeof(RMProfile), param, "IsRemoved", false));

            allExpressionList.Add(Expression4DynamicQuery.GetInExpression(typeof(RMProfile), param, "Source", sources.Cast<object>()));

            List<Expression> typesExpressionList = new List<Expression>();
            typesExpressionList.AddRange(termUsageReportTypeList.Select(ty => Expression4DynamicQuery.GetEqualExpression(typeof(RMProfile), param, "Type", ty)));
            if (typesExpressionList.Count > 0)
            {
                allExpressionList.Add(typesExpressionList.Aggregate(Expression.OrElse));
            }

            if (!(await IsAdminAsync() && IsILReportJobType((int)pageInfo.Type))
                && !(await IsSOAdminAsync() && IsSOReportJobType((int)pageInfo.Type)))
            {
                allExpressionList.Add(Expression4DynamicQuery.GetEqualExpression(typeof(RMProfile), param, "CreateProfileLogonUserId", TenantLocalValue.LogonUserId));
            }
            if (!string.IsNullOrEmpty(pageInfo.SearchValue))
            {
                allExpressionList.Add(Expression4DynamicQuery.GetContainsExpression(typeof(RMProfile), param, "Name", pageInfo.SearchValue));
            }

            Expression queryExpr = allExpressionList.Aggregate(Expression.AndAlso);
            var lambda = Expression.Lambda<Func<RMProfile, bool>>(queryExpr, param);
            logger.Info($"GetProfiles: {lambda}");
            List<RMProfile> profiles = profileDAO.GetProfiles(pageInfo.PageIndex, pageInfo.PageSize, out totalRecord, "Modified", pageInfo.IsDesc, lambda);
            pageInfo.Profiles = await profiles.ConvertAllAsync<RMProfile,RMProfileDto>(o => ConvertToProfileDtoAsync(o));
            pageInfo.TotalCount = totalRecord;
            return pageInfo;
        }
        /// <summary>
        /// 获取RA Term Tree 包含移除的Term
        /// </summary>
        /// <returns></returns>
        public async Task<List<TermTreeNode>> GetRATermTreeNodeOfOrphanedTermAsync()
        {
            List<TermTreeNode> groupNodes = new List<TermTreeNode>();
            List<RMTermGroup> termGroups = TermGroupDao.LoadTermGroup();
            foreach (var group in termGroups)
            {
                TermTreeNode groupNode = new TermTreeNode()
                {
                    ID = group.UniqueId,
                    Children = new Dictionary<Guid, TermTreeNode>()
                };
                List<RMTermSet> allRMTermSet = await TermSetDAO.LoadTermSetAsync(TermSetType.Business, group.UniqueId);
                foreach (RMTermSet termSet in allRMTermSet)
                {
                    TermTreeNode termSetNode = TermDao.GetRATermSetTreeOfOrphanedTerm(termSet.UniqueId);
                    if (termSetNode != null)
                    {
                        termSetNode.ParentID = group.UniqueId;
                        groupNode.Children.Add(termSetNode.ID, termSetNode);
                    }
                }
                groupNodes.Add(groupNode);
            }

            return groupNodes;
        }
        #endregion

        #region Available Space Report
        public async Task<ShowProfilesReportPageInfo> GetAvailableSpaceReportProfilesAsync(ShowProfilesReportPageInfo pageInfo)
        {
            var isAdmin = await IsAdminAsync();
            var isSOAdmin = await IsSOAdminAsync();
            var userId = TenantLocalValue.LogonUserId;
            int totalRecord = 0;
            int space = (int)JobType.AvailableSpaceReport;

            Expression<Func<RMProfile, bool>> queryExpr = profile => (profile.Type == space) && !profile.IsRemoved;
            if (!(isAdmin && IsILReportJobType((int)pageInfo.Type))
                && !(isSOAdmin && IsSOReportJobType((int)pageInfo.Type)))
            {
                queryExpr = profile => (profile.Type == space) && profile.CreateProfileLogonUserId == userId && !profile.IsRemoved;
            }
            if (!string.IsNullOrEmpty(pageInfo.SearchValue))
            {
                queryExpr = profile => profile.Name.Contains(pageInfo.SearchValue) && (profile.Type == space) && !profile.IsRemoved;
                if (!(isAdmin && IsILReportJobType((int)pageInfo.Type))
                    && !(isSOAdmin && IsSOReportJobType((int)pageInfo.Type)))
                {
                    queryExpr = profile => profile.Name.Contains(pageInfo.SearchValue) && (profile.Type == space) && profile.CreateProfileLogonUserId == userId && !profile.IsRemoved;
                }
            }
            List<RMProfile> profiles = profileDAO.GetProfiles(pageInfo.PageIndex, pageInfo.PageSize, out totalRecord, "Modified", pageInfo.IsDesc, queryExpr);
            pageInfo.Profiles = await profiles.ConvertAllAsync<RMProfile,RMProfileDto>(o => ConvertToProfileDtoAsync(o));
            pageInfo.TotalCount = totalRecord;
            return pageInfo;
        }
        public async Task<int> GetLocationTermIdFromProfileIdAsync(string profileId)
        {
            int termId = 0;
            RMProfileDto dto = await GetProfileByIdAsync(profileId);
            var termInfo = JsonConvert.DeserializeObject<LocationTermExt>(dto.Extension1);
            if (termInfo != null)
            {
                termId = termInfo.Id;
            }
            return termId;
        }
        public string[][] ConvertAvailableSpaceReportToArray(IEnumerable<BaseReport> reportDetails, string[][] datas)
        {
            AvailableSpaceReport reportInfo = null;
            int rowCount = 1;
            foreach (BaseReport report in reportDetails)
            {
                try
                {
                    reportInfo = report as AvailableSpaceReport;
                    datas[rowCount] = new string[3];
                    datas[rowCount][0] = reportInfo.Location;
                    datas[rowCount][1] = reportInfo.AvailableSpace.ToString();
                    datas[rowCount][2] = reportInfo.LocationSize.ToString();
                    rowCount++;
                }
                catch (Exception e)
                {
                    logger.Error($"Convert Available Space Report To Array failed {e}");
                    rowCount++;
                    throw;
                }
            }
            return datas;
        }

        public string[][] AssembleAvailableSpaceReportHeaderTittle(string[][] datas)
        {
            datas[0] = new string[3];
            datas[0][0] = I18NEntity.GetString("RM_JS_RC_ReportColumn_LocationPath");
            datas[0][1] = I18NEntity.GetString("RM_JS_RC_ReportColumn_AvailableSpace");
            datas[0][2] = I18NEntity.GetString("RM_JS_RC_ReportColumn_LocationSize");
            return datas;
        }
        #endregion

        public List<PolicyLevel> GetRuleLevels(Dictionary<Guid, RMRuleItemCollection> mTermAndRulesMapping)
        {
            List<PolicyLevel> levels = new List<PolicyLevel>();
            foreach (var ruleItemCollection in mTermAndRulesMapping.Values)
            {
                RuleCollection commonRules = ruleItemCollection.CommonRules;
                foreach (Rule rule in commonRules.Rules.Values)
                {
                    if (!levels.Contains(rule.PolicyLevel))
                    {
                        levels.Add(rule.PolicyLevel);
                    }
                }
            }
            return levels;
        }

        public bool CheckHasLowLevelRule(List<PolicyLevel> levels, PolicyLevel curLevel)
        {
            bool isHasLowLevelRule = false;
            List<PolicyLevel> lowLevels = levels.Where(l => (int)l > (int)curLevel).ToList();
            if (lowLevels.Count > 0)
            {
                isHasLowLevelRule = true;
            }
            return isHasLowLevelRule;
        }

        #region Exchange Online Report Associated...
        private List<RMEXOTreeNode> AssembleRunableMessageBox(RMProfileDto dto)
        {
            List<RMEXOTreeNode> messageBoxList = new List<RMEXOTreeNode>();
            if (!string.IsNullOrEmpty(dto.Extension2))
            {
                var farmNode = this.GetFarmEXOTreeNode(dto.Extension2);
                messageBoxList = this.AssembleSelectedEXOTreeNode(farmNode);
            }
            return messageBoxList;
        }

        public RMEXOTreeNode GetFarmEXOTreeNode(string ext2)
        {
            return SerializerHelper.DeserializeByDataContractSerializer<RMEXOTreeNode>(ext2);
        }

        private List<RMEXOTreeNode> AssembleSelectedEXOTreeNode(RMEXOTreeNode farmNode)
        {
            List<RMEXOTreeNode> treeNodes = new List<RMEXOTreeNode>();
            foreach (var group in farmNode.Children)
            {
                //IncludeNew...
                if (group.IncludeNew == 1)
                {
                    logger.Debug("Include new is enabled on group {0}", group.Name);
                    List<RMEXOTreeNode> messageBoxes = RMSPTreeService.BrowseExchangeTree(new RMEXOTreeNode() { Id = group.Id, Level = group.Level, Name = group.Name, Email = group.Email });
                    if (group.CheckNumber != 0)  //当前的Group选中, 并Include New.
                    {
                        messageBoxes.ForEach(a => a.CheckNumber = 1);
                        treeNodes.AddRange(messageBoxes);
                        continue;
                    }
                    else    //当前Group没选中, 但Include New
                    {
                        if (group.Children != null)
                        {
                            List<RMEXOTreeNode> newCreated = messageBoxes.Except(group.Children, new RADataBroker.Common.ConvertUtility.EXOTreeNodeComparer()).ToList();
                            logger.Info("New created messagebox under {0} count is {1}, detail is {2}", group.Name, newCreated.Count, string.Join(", ", newCreated.Select(a => a.Name).ToArray()));
                            newCreated.ForEach(a => a.CheckNumber = 1);
                            treeNodes.AddRange(newCreated);
                        }
                    }
                }
                if (group.Children != null)  //本身选中或者有选中子节点的Mailbox
                {
                    foreach (var messageBox in group.Children)
                    {
                        if (HasSelectNode(messageBox) && MailboxExists(messageBox))
                        {
                            treeNodes.Add(messageBox);
                        }
                        else
                        {
                            logger.Debug("No select node in {0}", messageBox.Name);
                        }
                    }
                }
                else
                {
                    if (group.CheckNumber != 0)  //当前的Group选中且未展开, 添加Group下的全部节点
                    {
                        List<RMEXOTreeNode> messageBoxes = RMSPTreeService.BrowseExchangeTree(group);
                        messageBoxes.ForEach(a => a.CheckNumber = 1);
                        treeNodes.AddRange(messageBoxes);
                    }
                }
            }
            return treeNodes;
        }

        private bool MailboxExists(RMEXOTreeNode node)
        {
            if (node.Level == (int)NodeLevel.ExchangeOnlineMailbox)
            {
                var mailbox = MailBoxDao.GetEmailById(node.Id.ToString());
                if (mailbox == null)
                {
                    return false;
                }
            }
            return true;
        }

        private bool HasSelectNode(RMEXOTreeNode current)
        {
            if (current.CheckNumber != 0)
            {
                return true;
            }
            if (current.Children.IsNullOrEmpty())
            {
                return false;
            }
            else
            {
                foreach (RMEXOTreeNode child in current.Children)
                {
                    if (HasSelectNode(child))
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        #endregion



        //适配不同Source类型的Rule
        private void RuleAdatper(int idx, Rule rule, RMTerm term, DateTime timePoint, List<RMRuleItem> rmRules, RuleCollection commonRules, SourceFlag flag)
        {
            if (flag == SourceFlag.Exchange)
            {
                Rule ruleOBj = null;
                if (rule.EXORule != null && rule.EXORule.SOFilters != null && rule.EXORule.SOFilters.Count > 0)
                {
                    rule.EXORule.Name = rule.Name;
                    ruleOBj = CloneSameRuleObject(rule.EXORule);
                }
                else
                {
                    ruleOBj = CloneSameRuleObject(rule);
                }
                commonRules.Rules.Add(idx, ruleOBj);

                ruleOBj.Filters = ConvertSOFiletrPolicyToFilterPolicy(ruleOBj.SOFilters);
                //if (ruleOBj.PolicyLevel == PolicyLevel.Item || ruleOBj.PolicyLevel == PolicyLevel.Document || ruleOBj.PolicyLevel == PolicyLevel.Folder)
                if (ruleOBj.PolicyLevel == PolicyLevel.ExchangeOnlineItem)
                {
                    rmRules.Add(ConvertRuleCheckerForEXO(ruleOBj, term, timePoint));
                }
                else
                {
                    ModifyRuleCheckerForEXO(ruleOBj, term, timePoint);
                }
            }
            else if (flag == SourceFlag.SharePoint)
            {
                var ruleOBj = CloneSameRuleObject(rule);
                commonRules.Rules.Add(idx, ruleOBj);
                if (ruleOBj.PolicyLevel == PolicyLevel.Item || ruleOBj.PolicyLevel == PolicyLevel.Document || ruleOBj.PolicyLevel == PolicyLevel.Folder)
                {
                    rmRules.Add(ConvertRuleChecker(ruleOBj, term, timePoint));
                }
                else
                {
                    ModifyRuleChecker(ruleOBj, term, timePoint);
                }
            }
            else if (flag == SourceFlag.OneDrive)
            {
                Rule ruleOBj = null;
                if (rule.OneDriveRule != null && rule.OneDriveRule.SOFilters != null && rule.OneDriveRule.SOFilters.Count > 0)
                {
                    rule.OneDriveRule.Name = rule.Name;
                    ruleOBj = CloneSameRuleObject(rule.OneDriveRule);
                }
                else
                {
                    ruleOBj = CloneSameRuleObject(rule);
                }
                commonRules.Rules.Add(idx, ruleOBj);

                ruleOBj.Filters = ConvertSOFiletrPolicyToFilterPolicy(ruleOBj.SOFilters);
                //if (ruleOBj.PolicyLevel == PolicyLevel.Item || ruleOBj.PolicyLevel == PolicyLevel.Document || ruleOBj.PolicyLevel == PolicyLevel.Folder)
                if (ruleOBj.PolicyLevel == PolicyLevel.Item || ruleOBj.PolicyLevel == PolicyLevel.Document || ruleOBj.PolicyLevel == PolicyLevel.Folder)
                {
                    rmRules.Add(ConvertRuleCheckerForOneDrive(ruleOBj, term, timePoint));
                }
                else
                {
                    ModifyRuleChecker(ruleOBj, term, timePoint);
                }
            }
            else if (flag == SourceFlag.FileSystem)
            {
                //TO DO FS
            }
            else if (flag == SourceFlag.Google)
            {
                Rule ruleObj = null;
                if (rule.GoogleDriveRule != null && rule.GoogleDriveRule.SOFilters != null && rule.GoogleDriveRule.SOFilters.Count > 0)
                {
                    rule.GoogleDriveRule.Name = rule.Name;
                    ruleObj = CloneSameRuleObject(rule);
                }
                else
                {
                    ruleObj = CloneSameRuleObject(rule);
                }
                commonRules.Rules.Add(idx, ruleObj);
                ruleObj.Filters = ConvertSOFiletrPolicyToFilterPolicy(ruleObj.SOFilters);
                if (ruleObj.PolicyLevel == PolicyLevel.Item || ruleObj.PolicyLevel == PolicyLevel.Document)
                {
                    rmRules.Add(ConvertRuleCheckerForGoogleDrive(ruleObj, term, timePoint));
                }
                else
                {
                    ModifyRuleChecker(ruleObj, term, timePoint);
                }
            }
            //TO DO SP on-premise
        }


        public Dictionary<Guid, RMRuleItemCollection> GetTermAndRuleMappingsNew(DateTime timePoint, SourceFlag flag)
        {
            List<RMTermRuleAssociation> trAssociations = TermRuleInfos.GetTermWithRule();
            Dictionary<int, List<Guid>> termRules = new Dictionary<int, List<Guid>>();
            foreach (var termId in trAssociations.Select(a => a.TermId).Distinct())
            {
                var rules = trAssociations
                    .Where(a => a.TermId == termId)
                    .OrderBy(a => a.RuleOrder)
                    .Select(a => a.RuleId)
                    .ToList();
                if (rules.Count > 0)
                {
                    termRules.Add(termId, rules);
                }
            }

            var termRuleMappings = new Dictionary<Guid, RMRuleItemCollection>();
            Dictionary<Guid, Rule> allRules = new Dictionary<Guid, Rule>();
            if (flag == SourceFlag.OneDrive)
            {
                allRules = RuleService.GetRulesFromRecords().Where(r => r.OneDriveRule != null && r.OneDriveRule.SOFilters != null && r.OneDriveRule.SOFilters.Count != 0).ToDictionary(r => new Guid(r.Id));
            }
            else if (flag == SourceFlag.Google)
            {
                allRules = RuleService.GetRulesFromRecords().Where(r => r.GoogleDriveRule != null && r.GoogleDriveRule.SOFilters != null && r.GoogleDriveRule.SOFilters.Count != 0).ToDictionary(r => new Guid(r.Id));
            }
            else
            {
                allRules = RuleService.GetRulesFromRecords().Where(r => r.SOFilters != null && r.SOFilters.Count != 0).ToDictionary(r => new Guid(r.Id));
            }
            var allHasRuleTerms = TermDao.GetRMTermsByTermIds(termRules.Keys.ToArray());
            foreach (var term in allHasRuleTerms)
            {
                if (term.IsRemoved)
                {
                    continue;
                }
                RuleCollection commonRules = new RuleCollection() { Rules = new Dictionary<int, Rule>() };
                List<RMRuleItem> rmRules = new List<RMRuleItem>();
                bool hasUnCamlQueryableCondition = false;
                Rule rule;
                var ruleIds = termRules[term.Id];
                for (int idx = 0; idx < ruleIds.Count; idx++)
                {
                    if (allRules.TryGetValue(ruleIds[idx], out rule))
                    {
                        RuleAdatper(idx, rule, term, timePoint, rmRules, commonRules, flag);
                    }
                }
                if (rmRules.Count > 0)
                {
                    if (rmRules.Exists(rc => rc.HasUnCamlQueryableCondition))
                    {
                        hasUnCamlQueryableCondition = true;
                    }
                }
                var refTerms = new List<RMTerm>();
                TermDao.GetAllInheritTermsByRootTerm(term.Id, ref refTerms, timePoint.Ticks);
                foreach (var refTerm in refTerms)
                {
                    RMRuleItemCollection tempRC;
                    if (!termRuleMappings.TryGetValue(refTerm.UniqueId, out tempRC))
                    {
                        tempRC = new RMRuleItemCollection();
                        tempRC.TermId = refTerm.UniqueId;
                        tempRC.TermName = refTerm.Name;
                        termRuleMappings.Add(refTerm.UniqueId, tempRC);
                    }
                    tempRC.HasUnCamlQueryableCondition = hasUnCamlQueryableCondition;
                    tempRC.CommonRules = commonRules;
                    tempRC.Rules = rmRules;
                }
            }

            return termRuleMappings;
        }

        public Dictionary<Guid, RMRuleItemCollection> GetTermAndRuleMappingsForEXO(DateTime timePoint)
        {
            List<RMTermRuleAssociation> trAssociations = TermRuleInfos.GetTermWithRule();
            Dictionary<int, List<Guid>> termRules = new Dictionary<int, List<Guid>>();
            foreach (var termId in trAssociations.Select(a => a.TermId).Distinct())
            {
                var rules = trAssociations
                    .Where(a => a.TermId == termId)
                    .OrderBy(a => a.RuleOrder)
                    .Select(a => a.RuleId)
                    .ToList();
                if (rules.Count > 0)
                {
                    termRules.Add(termId, rules);
                }
            }

            var termRuleMappings = new Dictionary<Guid, RMRuleItemCollection>();
            Dictionary<Guid, Rule> allRules = RuleService.GetRulesFromRecords().Where(r => r.EXORule != null && r.EXORule.SOFilters.Count != 0).ToDictionary(r => new Guid(r.Id));
            var allHasRuleTerms = TermDao.GetRMTermsByTermIds(termRules.Keys.ToArray());
            foreach (var term in allHasRuleTerms)
            {
                if (term.IsRemoved)
                {
                    continue;
                }
                RuleCollection commonRules = new RuleCollection() { Rules = new Dictionary<int, Rule>() };
                List<RMRuleItem> rmRules = new List<RMRuleItem>();
                bool hasUnCamlQueryableCondition = false;
                Rule rule;
                var ruleIds = termRules[term.Id];
                for (int idx = 0; idx < ruleIds.Count; idx++)
                {
                    if (allRules.TryGetValue(ruleIds[idx], out rule))
                    {
                        Rule ruleOBj = null;
                        if (rule.EXORule != null && rule.EXORule.SOFilters != null && rule.EXORule.SOFilters.Count > 0)
                        {
                            rule.EXORule.Name = rule.Name;
                            rule.EXORule.DisposalClass = rule.DisposalClass;
                            ruleOBj = CloneSameRuleObject(rule.EXORule);
                        }
                        else
                        {
                            ruleOBj = CloneSameRuleObject(rule);
                        }
                        commonRules.Rules.Add(idx, ruleOBj);

                        ruleOBj.Filters = ConvertSOFiletrPolicyToFilterPolicy(ruleOBj.SOFilters);
                        //if (ruleOBj.PolicyLevel == PolicyLevel.Item || ruleOBj.PolicyLevel == PolicyLevel.Document || ruleOBj.PolicyLevel == PolicyLevel.Folder)
                        if (ruleOBj.PolicyLevel == PolicyLevel.ExchangeOnlineItem)
                        {
                            rmRules.Add(ConvertRuleCheckerForEXO(ruleOBj, term, timePoint));
                        }
                        //else
                        //{
                        //    ModifyRuleChecker(ruleOBj, term, timePoint);
                        //}
                    }
                }
                if (rmRules.Count > 0)
                {
                    if (rmRules.Exists(rc => rc.HasUnCamlQueryableCondition))
                    {
                        hasUnCamlQueryableCondition = true;
                    }
                }
                var refTerms = new List<RMTerm>();
                TermDao.GetAllInheritTermsByRootTerm(term.Id, ref refTerms, timePoint.Ticks);
                foreach (var refTerm in refTerms)
                {
                    RMRuleItemCollection tempRC;
                    if (!termRuleMappings.TryGetValue(refTerm.UniqueId, out tempRC))
                    {
                        tempRC = new RMRuleItemCollection();
                        tempRC.TermId = refTerm.UniqueId;
                        tempRC.TermName = refTerm.Name;
                        termRuleMappings.Add(refTerm.UniqueId, tempRC);
                    }
                    tempRC.HasUnCamlQueryableCondition = hasUnCamlQueryableCondition;
                    tempRC.CommonRules = commonRules;
                    tempRC.Rules = rmRules;

                    //foreach (var commonRule in commonRules.Rules.Values)
                    //{
                    //    if (commonRule.PolicyLevel == PolicyLevel.Document)
                    //    {
                    //        tempRC.HasDocumentLevelRule = true;
                    //        break;
                    //    }
                    //}

                }
            }

            return termRuleMappings;
        }
        public List<FilterPolicy> ConvertSOFiletrPolicyToFilterPolicy(List<SOFilterPolicy> soFilters)
        {
            List<FilterPolicy> filerPolicies = new List<FilterPolicy>();
            foreach (var filter in soFilters)
            {
                FilterPolicy filterPolicy = new FilterPolicy();
                if (filter.Condition == PolicyCondition.Exactly || filter.Condition == PolicyCondition.Equals)
                {
                    filterPolicy.Condition = PolicyCondition.Equals;
                }
                else
                {
                    filterPolicy.Condition = filter.Condition;
                }
                filterPolicy.Level = filter.Level;
                filterPolicy.Rule = filter.Rule;
                filterPolicy.RuleType = filter.RuleType;
                filterPolicy.SequenceNo = filter.SequenceNo;
                filterPolicy.Value = filter.Value;

                filerPolicies.Add(filterPolicy);
            }
            return filerPolicies;
        }
        //TODO xwwang
        private void ModifyRuleCheckerForEXO(Rule rule, RMTerm term, DateTime timePoint)
        {
            List<PolicyLevel> filterLevels = new List<PolicyLevel>();
            foreach (var filter in rule.Filters)
            {
                if (!filterLevels.Contains(filter.Level))
                {
                    filterLevels.Add(filter.Level);
                }

                filter.SequenceNo = filter.SequenceNo + 1;
                if (filter.Rule is SendDateUTCRule || filter.Rule is SendDateRule)
                {
                    switch (filter.Condition)
                    {
                        case PolicyCondition.OlderThan:
                            int num;
                            DateTime tempDt = DateTime.UtcNow;
                            if (int.TryParse(filter.Value.Value1, out num))
                            {
                                if (filter.Value.Value1Unit == PolicyValueUnit.Days)
                                {
                                    tempDt = timePoint.AddDays(-num);
                                }
                                else if (filter.Value.Value1Unit == PolicyValueUnit.Weeks)
                                {
                                    tempDt = timePoint.AddDays(-num * 7);
                                }
                                else if (filter.Value.Value1Unit == PolicyValueUnit.Months)
                                {
                                    tempDt = timePoint.AddMonths(-num);
                                }
                                else if (filter.Value.Value1Unit == PolicyValueUnit.Years)
                                {
                                    tempDt = timePoint.AddYears(-num);
                                }
                                filter.Value.Value1 = tempDt.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                                filter.Condition = PolicyCondition.Before;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            rule.Filters.Add(new FilterPolicy()
            {
                Condition = PolicyCondition.Before,
                Level = PolicyLevel.ExchangeOnlineItem_Message, //rule.PolicyLevel,
                Rule = new SendDateUTCRule() { Value1 = "Send Time" },
                RuleType = PolicyRuleType.SendDate,
                Value = new PolicyValue(timePoint.ToString(APIDateTimeFormat.DATETYPEForAPI003)),
                SequenceNo = 1
            });
            //have a bug here should change order Created Time to last

            StringBuilder filterCombineModeString = new StringBuilder();
            foreach (var filterLevel in filterLevels)
            {
                filterCombineModeString.Append(rule.AndOrExpression[filterLevel]);
            }

            logger.Info($"Before convert and or express:{filterCombineModeString}");
            var tempStrs = filterCombineModeString.ToString().Split(new char[] { ' ', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
            string andOrExpression = "(1 And (";
            foreach (var str in tempStrs)
            {
                int sequenceNo = 0;
                if (int.TryParse(str, out sequenceNo))
                {
                    sequenceNo++;
                    andOrExpression = string.Format("{0} {1}", andOrExpression, sequenceNo.ToString());
                }
                else
                {
                    andOrExpression = string.Format("{0} {1}", andOrExpression, str);
                }
            }
            andOrExpression += "))";
            rule.AndOrExpression = new Dictionary<PolicyLevel, string>()
            {
                 //{ rule.PolicyLevel, andOrExpression }
                 { PolicyLevel.ExchangeOnlineItem_Message, andOrExpression }
            };
            logger.Info($"After convert and or express:{rule.AndOrExpression[PolicyLevel.ExchangeOnlineItem_Message]}");
        }
        private RMRuleItem ConvertRuleCheckerForEXO(Rule rule, RMTerm term, DateTime timePoint)
        {
            RMRuleItem checker = new RMRuleItem();
            checker.HasUnCamlQueryableCondition = true;
            checker.RuleId = rule.Id;
            checker.RuleName = rule.Name;
            checker.IsMoveRule = RuleHelper.CheckMoveRule(rule);
            checker.ArchiverAction = (RMContentDisposalAction)RuleHelper.GetOperationTypeForEXO(rule);
            checker.IsManualApproval = rule.IsManualApproval;
            checker.ExportType = rule.ExportInfo == null ? GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None : rule.ExportInfo.exportType;
            //checker.DeleteRecords = rule.DeleteRecords;
            //checker.RelatedRecordOption = (RelatedRecordOption)rule.RelatedRecordOption;
            checker.RuleFilters = new List<ArchiverRuleFilter>();
            List<PolicyLevel> filterLevels = new List<PolicyLevel>();

            foreach (var filter in rule.SOFilters)
            {
                var arFilter = new ArchiverRuleFilter(filter);
                checker.RuleFilters.Add(arFilter);
            }


            foreach (var filter in rule.Filters)
            {
                if (!filterLevels.Contains(filter.Level))
                {
                    filterLevels.Add(filter.Level);
                }


                filter.SequenceNo = filter.SequenceNo + 1;
                //if (filter.Rule is ContentTypeRule)
                //{
                //    filter.RuleType = PolicyRuleType.ContentType;
                //}
                if (filter.Rule is SendDateUTCRule || filter.Rule is SendDateRule)
                {
                    switch (filter.Condition)
                    {
                        #region[REC-738] remove timepoint ref FromTo/Before
                        //case PolicyCondition.FromTo:
                        //    var fromDt = ConvertUtcDateTime(filter.Value.Value1);
                        //    var toDt = ConvertUtcDateTime(filter.Value.Value2);
                        //    if (toDt > timePoint)
                        //    {
                        //        filter.Value.Value2 = timePoint.ToString(AveDateTimeUtility.DATETYPEForAPI003);
                        //    }
                        //    break;
                        //case PolicyCondition.Before:
                        //    var ltDt = ConvertUtcDateTime(filter.Value.Value1);
                        //    if (ltDt >= timePoint)
                        //    {
                        //        filter.Value.Value1 = timePoint.ToString(AveDateTimeUtility.DATETYPEForAPI003);
                        //    }
                        //    break;
                        #endregion
                        case PolicyCondition.OlderThan:
                            int num;
                            DateTime tempDt = DateTime.UtcNow;
                            if (int.TryParse(filter.Value.Value1, out num))
                            {
                                if (filter.Value.Value1Unit == PolicyValueUnit.Days)
                                {
                                    tempDt = timePoint.AddDays(-num);
                                }
                                else if (filter.Value.Value1Unit == PolicyValueUnit.Weeks)
                                {
                                    tempDt = timePoint.AddDays(-num * 7);
                                }
                                else if (filter.Value.Value1Unit == PolicyValueUnit.Months)
                                {
                                    tempDt = timePoint.AddMonths(-num);
                                }
                                else if (filter.Value.Value1Unit == PolicyValueUnit.Years)
                                {
                                    tempDt = timePoint.AddYears(-num);
                                }
                                filter.Value.Value1 = tempDt.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                                filter.Condition = PolicyCondition.Before;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }

            //Only ExchangeOnlineItem_Message

            rule.Filters.Add(new FilterPolicy()
            {
                Condition = PolicyCondition.Before,
                Level = PolicyLevel.ExchangeOnlineItem_Message, //rule.PolicyLevel,
                Rule = new SendDateUTCRule() { Value1 = "Send Time" },
                RuleType = PolicyRuleType.SendDate,
                Value = new PolicyValue(timePoint.ToString(APIDateTimeFormat.DATETYPEForAPI003)),
                SequenceNo = 1
            });

            //var filterp = new SOFilterPolicy()
            //{
            //    Condition = PolicyCondition.Before,
            //    Level = PolicyLevel.ExchangeOnlineItem_Message, //rule.PolicyLevel,
            //    Rule = new SendDateUTCRule() { Value1 = "Send Time" },
            //    RuleType = PolicyRuleType.SendDate,
            //    Value = new PolicyValue(timePoint.ToString(APIDateTimeFormat.DATETYPEForAPI003)),
            //    SequenceNo = 1
            //};
            //rule.SOFilters.Add(filterp);
            //rule.Filters.Add(ConvertSOFiletrPolicyToFilterPolicy(new List<SOFilterPolicy>() { filterp }).FirstOrDefault());
            //have a bug here should change order Created Time to last

            StringBuilder filterCombineModeString = new StringBuilder();
            foreach (var filterLevel in filterLevels)
            {
                filterCombineModeString.Append(rule.AndOrExpression[filterLevel]);
            }

            logger.Info($"Before convert and or express:{filterCombineModeString}");
            var tempStrs = filterCombineModeString.ToString().Split(new char[] { ' ', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
            string andOrExpression = "(1 And (";
            foreach (var str in tempStrs)
            {
                int sequenceNo = 0;
                if (int.TryParse(str, out sequenceNo))
                {
                    sequenceNo++;
                    andOrExpression = string.Format("{0} {1}", andOrExpression, sequenceNo.ToString());
                }
                else
                {
                    andOrExpression = string.Format("{0} {1}", andOrExpression, str);
                }
            }
            andOrExpression += "))";
            rule.AndOrExpression = new Dictionary<PolicyLevel, string>()
            {
                //{ rule.PolicyLevel, andOrExpression }
                 { PolicyLevel.ExchangeOnlineItem_Message, andOrExpression }
            };
            logger.Info($"After convert and or express:{rule.AndOrExpression[PolicyLevel.ExchangeOnlineItem_Message]}");
            return checker;
        }

        public List<string> FilterSearchKeys(BaseJobDto jobInfo, List<string> searchKeys)
        {
            var reportTableName = "ReportDetail";
            var reportFilePath = GetReportFilePath(jobInfo);
            return ReportCenterDao.FilterColumns(reportFilePath, reportTableName, searchKeys);
        }

        public string GetReportFilePath(BaseJobDto jobInfo)
        {
            AbstractReportWorker worker = null;
            if (baeReportWorkerDictionary.ContainsKey(jobInfo.JobType))
            {
                worker = baeReportWorkerDictionary[jobInfo.JobType];
            }
            ArgumentCheck.NotNull(worker, nameof(worker));
            return worker.DownloadReports(jobInfo);
        }

        public bool CheckFSRootNode(string treeNodesJsonStr)
        {
            var isValid = true;
            try
            {
                var treeNodes = JsonConvert.DeserializeObject<RMFSTreeNode>(treeNodesJsonStr);
                if (treeNodes.Id != RA.Common.RecordsConstants.FS_ROOT_GUID)
                {
                    isValid = false;
                }
            }
            catch (Exception ex)
            {
                logger.Error($"An error when CheckFSRootNode, message:{ex}");
                isValid = false;
            }
            return isValid;
        }

        public bool CheckBoxRootNode(string treeNodesJsonStr)
        {
            var isValid = true;
            try
            {
                var treeNodes = JsonConvert.DeserializeObject<BoxTreeNode>(treeNodesJsonStr);
                if (!string.Equals(treeNodes.Id, RA.Common.RecordsConstants.BOX_ROOT_GUID.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    isValid = false;
                }
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred when CheckBoxRootNode, message: {ex}");
                isValid = false;
            }
            return isValid;
        }

        private async Task<bool> IsAdminAsync()
        {
            return await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(Contract.RoleAssignments.RMPermissionMasks.ReportCenterAdmin);
        }

        public async Task<bool> IsSOAdminAsync()
        {
            //暂时先这么顶着，so还没有ReportCenterAdmin这类权限
            if (!(await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMSOPermissionMasks.CommonModuleAccess))
                || !(await SecurityGroupManagementService.GetUserScopePermissionsAsync(TenantLocalValue.LogonUserId)).IsAdmin)
            {
                return false;
            }
            return true;
        }

        private bool ValidateQuerySearchKeys(List<string> searchKeys)
        {
            var allowedSearchKeys = new List<string> { "BCSTermName", "TitleOrName", "Location", "SiteCollectionTitle" };
            if (searchKeys.Any(o => !allowedSearchKeys.Contains(o, StringComparer.OrdinalIgnoreCase)))
            {
                return false;
            }
            return true;
        }

        public RAReturnMessage RunExportReportJob(string reportParameters)
        {
            var returnMessage = new RAReturnMessage();
            try
            {
                var exportParameters = SerializerHelper.DeserializeByJsonConvert<ExportReportCommonModel>(reportParameters);
                if (exportParameters == null
                    || string.IsNullOrWhiteSpace(exportParameters.ReportJobType)
                    || string.IsNullOrWhiteSpace(exportParameters.ReportJobId)
                    || string.IsNullOrWhiteSpace(exportParameters.ProfileId))
                {
                    returnMessage.MessageType = RAMessageType.Failed;
                    returnMessage.ErrorMessage = "Export report parameters are incomplete.";
                    return returnMessage;
                }

                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                var jqDto = new JobQueueDto
                {
                    JobType = JobType.ExportReportDetails,
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    JobRunByUser = loginName,
                    Parameters = reportParameters,
                };
                returnMessage.MessageType = RAMessageType.Successful;
                returnMessage.Extension = JobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while Run Export Report Job. Error: {e}");
                returnMessage.MessageType = RAMessageType.Failed;
                returnMessage.ErrorMessage = e.Message;
            }
            return returnMessage;
        }

        [Audit(Module = AuditModule.ReportCenter, Category = AuditCategory.ReportCenter, Action = AuditAction.ExportReportDetailsJob, BeforeHandler = typeof(TermUsageOrDueForDisposalBeforeAuditHandler), AfterHandler = typeof(TermUsageOrDueForDisposalAfterAuditHandler))]
        public async Task<string> RealRunExportReportJobAsync(string reportParameters)
        {
            logger.Info("Start run export report details job.");
            var jobId = string.Empty;

            try
            {
                var username = TenantLocalValue.LogonUserEmail;
                var account = await AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail);
                jobId = JobMonitorService.CreateJob(JobType.ExportReportDetails, username, account.UserId);
                SubJobDao.UpdateSubJobCount(jobId, 1);
                string subJobId = CreateSubJob(jobId, 0, JobType.ExportReportDetails, JobStatus.InProgress, 1, reportParameters);
                DownloadDataInfoDao.Create(new RMDownloadDataInfo()
                {
                    FileDownloadTime = DateTime.UtcNow.Ticks,
                    JobId = jobId,
                    RecordsId = Guid.NewGuid(),
                    JobStatus = (int)DownloadContentJobStatus.Wait,
                    UserId = account.UserId,
                    Name = jobId + ".zip",
                    DownloadType = DownloadContentType.ReportContent,
                });

                logger.Info($"Real run export report details job: [{jobId}]");
                JobQueueService.HandleMessage(new JobQueueMessage
                {
                    JobId = subJobId,
                    JobType = JobType.ExportReportDetails,
                    CommandLine = $"{JobType.ExportReportDetails} {subJobId}",
                });
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while real run export report details job. Error: {e}");
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
                LastUpdateTime = DateTime.UtcNow.Ticks
            };
            if (jobState == JobStatus.Wait)
            {
                subJob.Runable = RecordsConstants.SubJob_Runnable_CanRun;
            }
            subJob.JobContext = new RMJobContext() { JobId = subJobId, Content = jobMessage };
            SubJobDao.CreateJob(subJob);
            logger.Info("Create sub job {0} sucessfull, type {1}, weight {2}, state {3}, string1 {4} ", subJob.Id, subJob.JobType, subJob.Weight, subJob.Status, string1);
            return subJobId;
        }
    }
    public enum OperationType
    {
        Created = 0,
        Destroyed = 1
    }
}
