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
using AvePoint.GCommon.Contract.CloudAppAdmin.Object;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Dedeplication;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FunctionSetting;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.Dashboard;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Setting;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Encryption;
using AvePoint.RA.Service.Services.Dashboard.AuditHandler;
using AvePoint.RA.Service.Services.Dashboard.Model;
using AvePoint.RA.Service.Services.RMGeneralSetting;
using AvePoint.RA.SharePoint.RMSharePointColumn;
using Microsoft.Azure.Cosmos;
using RATeams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AvePoint.GCommon.Utility.I18N.EventIds.Configuration;

namespace AvePoint.RA.Service.Services.Dashboard
{
    [Audit]
    public class DashboardService : RMServiceBase, IDashboardService
    {

        private static readonly RALogger Logger = RALogger.GetInstance(typeof(DashboardService));
        private IGeneralSettingService mGeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();
        private ISettingProfileService SettingProfileService => PlatformWindsorManager.GetService<ISettingProfileService>();

        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();

        private IJobQueueService JobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();

        public ISecurityGroupManagementService SecurityGroupManagementService => PlatformWindsorManager.GetService<ISecurityGroupManagementService>();

        private IRMSecurityTrimmingHelper SecurityTrimmingHelper => PlatformWindsorManager.GetService<IRMSecurityTrimmingHelper>();

        private ITenantService TenantService = PlatformWindsorManager.GetService<ITenantService>();

        private static IRMFunctionSettingDao FunctionSettingDao => PlatformWindsorManager.GetService<IRMFunctionSettingDao>();

        private static IRMKeyValueDao KeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private string LogonGroupId => TenantLocalValue.LogonGroupId;

        private RMPermissionMasks EndUserPermission => RMPermissionMasks.PhysicalEndUser | RMPermissionMasks.CommonModuleAccess;

        private RMPermissionMasks ReviewUserPermission => RMPermissionMasks.ManualReviewEnduser | RMPermissionMasks.CommonModuleAccess | RMPermissionMasks.JobMonitorEnduser;

        private static ILicenseHelperService LicenseHelperService => PlatformWindsorManager.GetService<ILicenseHelperService>();

        private static IDownloadDataInfoDao DownloadDataInfoDao => PlatformWindsorManager.GetService<IDownloadDataInfoDao>();

        private static IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();

        private static IRMArchiveSiteInfoDao RMArchiveSiteInfoDao => PlatformWindsorManager.GetService<IRMArchiveSiteInfoDao>();

        private static IRMArchiveTeamsGroupInfoDao RMArchiveTeamsGroupInfoDao => PlatformWindsorManager.GetService<IRMArchiveTeamsGroupInfoDao>();
        private static IRMRetentionSimulateInfosDao RetentionSimulateInfosDao => PlatformWindsorManager.GetService<IRMRetentionSimulateInfosDao>();
        private static IRMArchiveGDriveInfoDao _rMArchiveGDriveInfoDao => PlatformWindsorManager.GetService<IRMArchiveGDriveInfoDao>();
        private RMAesEncryptorWrapper AesEncryptorWrapper => new();

        #region Run Job
        [Audit(Module = AuditModule.ReportCenter, Category = AuditCategory.DashboardCollectionDataJob, Action = AuditAction.DashboardCollectionDataJob, AfterHandler = typeof(CollectionDataAfterAuditHandler))]
        public string RealRunDashboardJob(JobRunBy runBy)
        {
            Logger.Info("Start run dashboard job.");
            var jobId = string.Empty;

            try
            {
                var username = runBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
                var hasRunningJob = JobMonitorService.GetRunningJobsCount(JobType.Dashboard) > 0;
                jobId = JobMonitorService.CreateJob(JobType.Dashboard, username);
                if (hasRunningJob)
                {
                    Logger.Warn("A running dashboard job already exists.");
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_DSB_JobSkipped");
                    return jobId;
                }

                Logger.Info($"Real run dashboard job: [{jobId}]");
                JobQueueService.HandleMessage(new Contract.CloudService.JobQueueMessage
                {
                    JobId = jobId,
                    JobType = JobType.Dashboard,
                    RunBy = runBy,
                    CommandLine = $"{JobType.Dashboard} {jobId} {runBy}",
                });
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while real run dashboard job. Error: {e}");
                if (!string.IsNullOrEmpty(jobId))
                {
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, e.Message);
                }
            }

            return jobId;
        }

        public bool SchduleRunDashboardJob(JobRunBy runBy)
        {
            var id = string.Empty;
            var runJobUserName = runBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";

            if (!LicenseHelperService.HasOpusILLicense && !LicenseHelperService.HasOpusGoogleLicense && !LicenseHelperService.HasGoogleControlLicense && runBy != JobRunBy.ChangeTab)
            {
                Logger.Info($"SO only license can not run schedule dashboard job. OpusILLicense {LicenseHelperService.HasOpusILLicense}, OpusGoogleLicense {LicenseHelperService.HasOpusGoogleLicense}, GoogleControlLicense {LicenseHelperService.HasGoogleControlLicense}");
                return !string.IsNullOrEmpty(id);
            }

            try
            {
                var queue = new JobQueueDto
                {
                    JobType = JobType.Dashboard,
                    JobRunType = runBy,
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    JobRunByUser = runJobUserName,
                    Parameters = null
                };

                id = JobQueueService.AddToDBJobQueue(queue);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while run sharepoint on-premise scan node job. Error: {e}");
            }
            return !string.IsNullOrEmpty(id);
        }
        #endregion

        public bool ExistsJobQueue()
        {
            return JobQueueService.GetMessagesCount(LogonGroupId, JobType.Dashboard) > 0;
        }

        public bool HasRunningJob()
        {
            return JobMonitorService.GetRunningJobsCount(JobType.Dashboard) > 0;
        }

        #region Permission

        public async Task<bool> IsAdminAsync()
        {
            return (await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.ControlPanelAdmin));
        }        
        
        public Task<bool> IsSOAdminAsync()
        {
            return SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMSOPermissionMasks.ControlPanelAdmin);
        }

        public async Task<bool> IsEndUserAsync()
        {
            var permission = await SecurityTrimmingHelper.GetUserPermissionAsync<RMPermissionMasks>();

            return permission == (RMPermissionMasks.PhysicalEndUser | RMPermissionMasks.CommonModuleAccess);
        }

        public async Task<int> GetEndUserPermissionAsync()
        {
            var result = DashboardEndUserPermission.None;
            var permission = await SecurityTrimmingHelper.GetUserPermissionAsync<RMPermissionMasks>();
            if (permission == (EndUserPermission | ReviewUserPermission))
            {
                result |= DashboardEndUserPermission.EndUser | DashboardEndUserPermission.ReviewEndUser;
            }
            if (permission == EndUserPermission)
            {
                result |= DashboardEndUserPermission.EndUser;
            }
            if (permission == ReviewUserPermission)
            {
                result |= DashboardEndUserPermission.ReviewEndUser;
            }   
            return (int)result;
        }

        #endregion

        [Audit(Module = AuditModule.ReportCenter, Category = AuditCategory.DashboardCollectionDataJob, Action = AuditAction.EditArchiverPriceConfig, AfterHandler = typeof(CollectionDataAfterAuditHandler))]
        public async Task<bool> SaveSOPriceConfigurationAsync(ArchiverPriceConfiguration priceConfiguration)
        {
            try
            {
                var securityConfig = AesEncryptorWrapper.Encrypt(SerializerHelper.SerializeByDataContractSerializer(priceConfiguration));
                var result = await FunctionSettingDao.AddOrUpdateSettingInfoAsync(FunctionSettingType.SODashboardPriceSetting, securityConfig);
                return result;
            }
            catch
            {
                throw;
            }

        }

        public async Task<ArchiverPriceConfiguration> GetSOPriceConfigurationAsync()
        {
            try
            {
                var defaultSecurityConfig = AesEncryptorWrapper.Encrypt(SerializerHelper.SerializeByDataContractSerializer(new ArchiverPriceConfiguration() { SharePointStoragePrice = 0.20, ArchivedStoragePrice = 0.00 }));
                await FunctionSettingDao.NotExistCreateIt(FunctionSettingType.SODashboardPriceSetting, defaultSecurityConfig);
                var securityConfig = await FunctionSettingDao.GetSettingInfo(FunctionSettingType.SODashboardPriceSetting);
                var realConfig = AesEncryptorWrapper.Decrypt(securityConfig);
                var config = SerializerHelper.DeserializeByDataContractSerializer<ArchiverPriceConfiguration>(realConfig);
                return config;
            }
            catch
            {
                throw;
            }
        }


        public async Task<SOSummaryTotalDataDetails> GetSOTotalDataInfos(string o365TenantId, string siteId)
        {
            try
            {
                var result = new SOSummaryTotalDataDetails();
                var siteInfo = await RMArchiveSiteInfoDao.GetArchiverSiteInfoBySiteAndTenant(o365TenantId, siteId);
                ArchiverDataUnit archivedSizeUnit = ArchiverDataUnit.GB, archiverFileUnit = ArchiverDataUnit.K, deleteSizeUnit = ArchiverDataUnit.GB, deleteFileUnit = ArchiverDataUnit.K;
                double archiverSize = FormatSize(siteInfo.ArchivedSize, ref archivedSizeUnit);
                result.ArchiverTotalSize = archiverSize < 0.005 ? "0" : archiverSize.ToString("F2");
                result.ArchiverDataSizeUnit = archivedSizeUnit;
                
                double deleteSize = FormatSize(siteInfo.DeletedSize, ref deleteSizeUnit);
                result.DeleteTotalSize = deleteSize < 0.005 ? "0" : deleteSize.ToString("F2");
                result.DeleteDataSizeUnit = deleteSizeUnit;

                double archiverFileCount = FormatFileCount(siteInfo.FileNumber, ref archiverFileUnit);
                result.ArchiverTotalFileCount = archiverFileCount < 0.005 ? "0" : archiverFileCount.ToString("F2");
                result.ArchiverFileCountUnit = archiverFileUnit;

                double deleteFileCount = FormatFileCount(siteInfo.DeleteFileNumbers, ref deleteFileUnit);
                result.DeleteTotalFileCount = deleteFileCount < 0.005 ? "0" : deleteFileCount.ToString("F2");
                result.DeleteFileCountUnit = deleteFileUnit;
                return result;
            }
            catch (Exception ex)
            {
                Logger.Error($"Get SO total data has errors: {ex}");
            }
            return new SOSummaryTotalDataDetails();
        }

        public async Task<SOSummaryTotalDataDetails> GetSOTotalDataInfosByTenant(string o365TenantId)
        {
            try
            {
                var result = new SOSummaryTotalDataDetails();
                var opusArchiverInfo = await RMArchiveSiteInfoDao.GetArchiverSiteInfoByTenant(o365TenantId);
                
                result.ArchiverTotalSize = (opusArchiverInfo.ArchivedSize * 1073741824).ToString();

                result.DeleteTotalSize = (opusArchiverInfo.DeletedSize * 1073741824).ToString();

                result.ArchiverTotalFileCount = (opusArchiverInfo.FileNumber * 1000).ToString();

                result.DeleteTotalFileCount = (opusArchiverInfo.DeleteFileNumbers).ToString();
                return result;
            }
            catch (Exception ex)
            {
                Logger.Error($"Get SO total data has errors: {ex}");
            }
            return new SOSummaryTotalDataDetails();
        }

        private double FormatFileCount(double dbFileCount, ref ArchiverDataUnit fileUnit)
        {
            double fileCount = dbFileCount;
            if (fileCount >= 2 * 1000)
            {
                fileCount /= 1000;
                fileUnit = ArchiverDataUnit.Million;
            }

            return fileCount;
        }

        private double FormatSize(double dbSize, ref ArchiverDataUnit archivedSizeUnit)
        {
            double size = dbSize;
            if (size >= 10000)
            {
                size /= 1024;
                archivedSizeUnit = ArchiverDataUnit.TB;
            }

            return size;
        }

        public async Task<TenantArchiverDataInfo> GetTenantArchivedDataInfo(Guid o365TenatId)
        {
            var isAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMSOPermissionMasks.ControlPanelAdmin);
            if (!isAdmin)
            {
                return new()
                {
                    HasLicense = false,
                    HasCollectData = false,
                };
            }

            var tenantArchivedInfo = await RMArchiveSiteInfoDao.GetArchiverDataSizeByTenantAsync(o365TenatId);
            var config = await GetSOPriceConfigurationAsync();
            var totalArchivedSize = tenantArchivedInfo.ArchivedDataSize;
            var totalFileNumber = tenantArchivedInfo.ArchivedFileNumber;
            var annaulCostSavings = Math.Round(totalArchivedSize, 2) * (config.SharePointStoragePrice - config.ArchivedStoragePrice) * 12;
            tenantArchivedInfo.AnnualCostSavings = annaulCostSavings;

            var unit = ArchiverDataUnit.GB;
            if (totalArchivedSize >= 10000)
            {
                totalArchivedSize /= 1024;
                unit = ArchiverDataUnit.TB;
            }

            var fileUnit = ArchiverDataUnit.K;
            if (totalFileNumber >= 2 * 1000)
            {
                totalFileNumber /= 1000;
                fileUnit = ArchiverDataUnit.Million;
            }

            tenantArchivedInfo.ArchivedDataSize = totalArchivedSize;
            tenantArchivedInfo.ArchivedFileNumber = totalFileNumber;
            tenantArchivedInfo.ArchivedDataSizeUnit = unit.ToString();
            tenantArchivedInfo.ArchivedFileNumberUnit = fileUnit.ToString();
            tenantArchivedInfo.HasLicense = TenantService.CheckLicenseWithAdditionalProduct(TenantLocalValue.LogonGroupId, PaidForProduct.OpusSO);
            tenantArchivedInfo.HasCollectData = true;

            var result = KeyValueDao.GetValueByKey("SyncArchivedSiteInfo");
            if(result == null || !bool.Parse(result.Value))
            {
                tenantArchivedInfo.HasCollectData = false;
            }

            return tenantArchivedInfo;
        }

        public async Task<TenantArchiverDataInfo> GetTenantArchivedDataInfo(Guid o365TenantId, int type)
        {
            try
            {
                var isAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMSOPermissionMasks.ControlPanelAdmin);
                if (!isAdmin)
                {
                    return new()
                    {
                        HasLicense = false,
                        HasCollectData = false,
                    };
                }
                var tenantArchivedInfo = await RMArchiveSiteInfoDao.GetArchiverDataSizeByTenantAsync(o365TenantId);
                var totalArchivedSize = tenantArchivedInfo.ArchivedDataSize;
                if (type == (int)ArchiverInfoType.EstimatedCostSaving)
                {
                    var config = await GetSOPriceConfigurationAsync();
                    var annaulCostSavings = Math.Round(totalArchivedSize, 2) * (config.SharePointStoragePrice - config.ArchivedStoragePrice) * 12;
                    tenantArchivedInfo.AnnualCostSavings = annaulCostSavings;
                }
                if (type == (int)ArchiverInfoType.ArchiverData)
                {
                    var unit = ArchiverDataUnit.GB;
                    if (totalArchivedSize >= 10000)
                    {
                        totalArchivedSize /= 1024;
                        unit = ArchiverDataUnit.TB;
                    }
                    tenantArchivedInfo.ArchivedDataSizeUnit = unit.ToString();
                    tenantArchivedInfo.ArchivedDataSize = totalArchivedSize;
                }
                if (type == (int)ArchiverInfoType.ArchiverFiles)
                {
                    var totalFileNumber = tenantArchivedInfo.ArchivedFileNumber;

                    var fileUnit = ArchiverDataUnit.K;
                    if (totalFileNumber >= 2 * 1000)
                    {
                        totalFileNumber /= 1000;
                        fileUnit = ArchiverDataUnit.Million;
                    }

                    tenantArchivedInfo.ArchivedFileNumber = totalFileNumber;
                    tenantArchivedInfo.ArchivedFileNumberUnit = fileUnit.ToString();
                }
                tenantArchivedInfo.HasLicense = TenantService.CheckLicenseWithAdditionalProduct(TenantLocalValue.LogonGroupId, PaidForProduct.OpusSO);
                tenantArchivedInfo.HasCollectData = true;

                var result = KeyValueDao.GetValueByKey("SyncArchivedSiteInfo");
                if (result == null || !bool.Parse(result.Value))
                {
                    tenantArchivedInfo.HasCollectData = false;
                }

                return tenantArchivedInfo;
            }
            catch(Exception e)
            {
                Logger.Error($"GetTenantArchivedDataInfo has errors: {e}");
                throw;
            }
        }

        public async Task<bool> IsRunSODashboardJobAsync()
        {
            try
            {
                var settingResult = KeyValueDao.GetValueByKey("SyncArchivedSiteInfo");
                var needSyncArchivedTeamsGroup = TeamsPermissionHelper.HasUpgradeTeamsFeature() &&
                    (KeyValueDao.GetValueByKey(KeyNameCollection.HasSyncArchivedTeamsGroup) == null ||
                    KeyValueDao.GetValueByKey(KeyNameCollection.HasUpdateEmail4ArchivedSite) == null);
                if (settingResult == null || needSyncArchivedTeamsGroup)
                {
                    Logger.Info("Cuurent user not sync so dashboard job");
                    var isSOAdmin = await IsSOAdminAsync();
                    var isHasRunningJob = HasRunningJob();
                    var isHasWaitingJob = JobQueueService.GetMessagesCount(TenantLocalValue.LogonGroupId, JobType.Dashboard) > 0;
                    if (isSOAdmin && !isHasRunningJob && !isHasWaitingJob)
                    {
                        SchduleRunDashboardJob(JobRunBy.ChangeTab);
                    }
                    return false;
                }
                return true;
            }
            catch
            {
                throw;
            }
        }

        public async Task<RAReturnMessage> RunExportArchiverRetentionSimulateInfoJobAsync()
        {
            var returnMessage = new RAReturnMessage();
            try
            {

                var mainJob = RetentionSimulateInfosDao.GetAll().FirstOrDefault(r => r.SourceFlag == (int)SourceFlag.All);
                if (mainJob == null || mainJob.MergeReportState != (int)MergeIndexState.Succeed || mainJob.FileNumber == 0)
                {
                    throw new Exception(I18NEntity.GetString("RM_DSB_Report_ExportNoData"));
                }

                ArchiverExportReportDto reportDto = new ArchiverExportReportDto()
                {
                    ReportType = ReportType.AllRetentionSimulate
                };
                var loginName = TenantLocalValue.LogonUserEmail;
                var jqDto = new JobQueueDto
                {
                    JobType = JobType.ArchiverExport,
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    JobRunByUser = loginName,
                    Parameters = SerializerHelper.SerializeByDataContractSerializer(reportDto),
                };
                returnMessage.MessageType = RAMessageType.Successful;
                returnMessage.Extension = JobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while run export ArchiverRetentionSimulateInfoJob. Error: {e}");
                returnMessage.MessageType = RAMessageType.Failed;
                returnMessage.ErrorMessage = e.Message;
            }
            return returnMessage;
        }

        public async Task<RAReturnMessage> RunExportArchiverSiteInfoJobAsync(ArchiverExportReportDto reportDto)
        {
            var returnMessage = new RAReturnMessage();
            try
            {
                if (!(await PreCheckExportSiteAsync()))
                {
                    throw new Exception(I18NEntity.GetString("RM_DSB_Report_ExportNoSite"));
                }

                //var timeZoneId = (await mGeneralSettingService.GetGeneralSettingAsync()).TimeZoneId;
                //var timeZone = GeneralSettingConfig.FindSystemTimeZoneById(timeZoneId);
                //reportDto.StartTime += timeZone.BaseUtcOffset;
                //reportDto.EndTime += timeZone.BaseUtcOffset;

                var loginName = TenantLocalValue.LogonUserEmail;
                var jqDto = new JobQueueDto
                {
                    JobType = JobType.ArchiverExport,
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    JobRunByUser = loginName,
                    Parameters = SerializerHelper.SerializeByDataContractSerializer(reportDto),
                };
                returnMessage.MessageType = RAMessageType.Successful;
                returnMessage.Extension = JobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while run export archiver site info job message. Error: {e}");
                returnMessage.MessageType = RAMessageType.Failed;
                returnMessage.ErrorMessage = e.Message;
            }
            return returnMessage;

        }

        public async Task<RAReturnMessage> RunArchiverDeduplicationReportJobAsync(DedeplicationExportReportDto reportDto)
        {
            var returnMessage = new RAReturnMessage();
            try
            {
                if (!SettingProfileService.IsEnableArchiverDeduplication())
                {
                    returnMessage.MessageType = RAMessageType.Failed;
                    return returnMessage;
                }
                var loginName = TenantLocalValue.LogonUserEmail;
                var timeZoneId = (await mGeneralSettingService.GetGeneralSettingAsync()).TimeZoneId;
                var timeZone = GeneralSettingConfig.FindSystemTimeZoneById(timeZoneId);
                reportDto.DedupFrom -= timeZone.BaseUtcOffset;
                reportDto.DedupTo -= timeZone.BaseUtcOffset;

                var jqDto = new JobQueueDto
                {
                    JobType = JobType.ArchiverDeduplicationReport,
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    JobRunByUser = loginName,
                    Parameters = SerializerHelper.SerializeByDataContractSerializer(reportDto),
                };
                returnMessage.MessageType = RAMessageType.Successful;
                returnMessage.Extension = JobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while run export archiver site info job message. Error: {e}");
                returnMessage.MessageType = RAMessageType.Failed;
                returnMessage.ErrorMessage = e.Message;
            }
            return returnMessage;

        }

        [Audit(Module = AuditModule.ReportCenter, Category = AuditCategory.ReportCenter, Action = AuditAction.RunArchiverExportJob, AfterHandler = typeof(CollectionDataAfterAuditHandler))]
        public async Task<string> RealRunExportArchiverSiteInfoJobAsync(string param)
        {
            Logger.Info("Start run export under review data job.");
            var jobId = string.Empty;

            try
            {
                var username = TenantLocalValue.LogonUserEmail;
                var account = await AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail);
                jobId = JobMonitorService.CreateJob(JobType.ArchiverExport, username, account.UserId);

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

                Logger.Info($"Real run export under review job: [{jobId}]");
                JobQueueService.HandleMessage(new JobQueueMessage
                {
                    JobId = jobId,
                    JobType = JobType.ArchiverExport,
                    CommandLine = $"{JobType.ArchiverExport} {jobId}",
                    Extension = param,
                });
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while real run export archiver site info job. Error: {e}");
                if (!string.IsNullOrEmpty(jobId))
                {
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, e.Message);
                }
            }

            return jobId;
        }

        [Audit(Module = AuditModule.ReportCenter, Category = AuditCategory.ReportCenter, Action = AuditAction.RunArchiverDedupReportJob, AfterHandler = typeof(CollectionDataAfterAuditHandler))]
        public async Task<string> RealRunExportArchiverDedupSiteInfoJobAsync(string param)
        {
            Logger.Info("Start run export under review data job.");
            var jobId = string.Empty;

            try
            {
                var username = TenantLocalValue.LogonUserEmail;
                var account = await AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail);
                jobId = JobMonitorService.CreateJob(JobType.ArchiverDeduplicationReport, username, account.UserId);

                DownloadDataInfoDao.Create(new RMDownloadDataInfo()
                {
                    FileDownloadTime = DateTime.UtcNow.Ticks,
                    JobId = jobId,
                    RecordsId = Guid.NewGuid(),
                    JobStatus = (int)DownloadContentJobStatus.Wait,
                    UserId = account.UserId,
                    Name = jobId + ".zip",
                    DownloadType = DownloadContentType.ExportDeduplicationReport,
                });

                Logger.Info($"Real run export under review job: [{jobId}]");
                JobQueueService.HandleMessage(new JobQueueMessage
                {
                    JobId = jobId,
                    JobType = JobType.ArchiverDeduplicationReport,
                    CommandLine = $"{JobType.ArchiverDeduplicationReport} {jobId}",
                    Extension = param,
                });
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while real run export archiver dedup site info job. Error: {e}");
                if (!string.IsNullOrEmpty(jobId))
                {
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, e.Message);
                }
            }

            return jobId;
        }

        private static async Task<bool> PreCheckExportSiteAsync()
        {
            var totalCount = await RMArchiveSiteInfoDao.GetArchiverSitesTotalCount4DashboardAsync();
            return totalCount > 0;
        }

        #region Teams
        private static async Task<bool> PreCheckExportTeamsGroupAsync()
        {
            var totalCount = await RMArchiveTeamsGroupInfoDao.GetArchiverTeamsGroupTotalCountAsync();
            return totalCount > 0;
        }

        #endregion
        public async Task<RAReturnMessage> RunExportArchiverGDriveInfoJobAsync(ArchiverExportReportDto reportDto)
        {
            var returnMessage = new RAReturnMessage();
            try
            {
                if (!(await PreCheckExportGDriveAsync()))
                {
                    throw new Exception(I18NEntity.GetString("RM_DSB_Report_ExportNoDrive"));
                }

                var timeZoneId = (await mGeneralSettingService.GetGeneralSettingAsync()).TimeZoneId;
                var timeZone = GeneralSettingConfig.FindSystemTimeZoneById(timeZoneId);
                reportDto.StartTime += timeZone.BaseUtcOffset;
                reportDto.EndTime += timeZone.BaseUtcOffset;

                var loginName = TenantLocalValue.LogonUserEmail;
                var jqDto = new JobQueueDto
                {
                    JobType = JobType.ArchiverExport,
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    JobRunByUser = loginName,
                    Parameters = SerializerHelper.SerializeByDataContractSerializer(reportDto),
                };
                returnMessage.MessageType = RAMessageType.Successful;
                returnMessage.Extension = JobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while run export archiver site info job message. Error: {e}");
                returnMessage.MessageType = RAMessageType.Failed;
                returnMessage.ErrorMessage = e.Message;
            }
            return returnMessage;
        }
        private static async Task<bool> PreCheckExportGDriveAsync()
        {
            var totalCount = await _rMArchiveGDriveInfoDao.GetGoogleArchiverTotalCount4DashboardAsync();
            return totalCount > 0;
        }
    }
}
