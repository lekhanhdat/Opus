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
using AvePoint.GCommon.Contract.AccountManager.Object;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.Audit.Async;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Office365;
using AvePoint.RA.Contract.Discovery.Model.Query.Office365.Parameter;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.Discovery.Office365.Audit;
using AvePoint.RA.Service.Services.JobQueue;
using AvePoint.RA.Service.Services.RMSharePointSettings.AuditHandler;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.Extensions.Azure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AvePoint.GCommon.Utility.I18N.ContextValues.Configuration;

namespace AvePoint.RA.Service.Services.Discovery.Office365
{
    [AsyncAudit]
    public class RMDiscoveryOffice365OptimizationService : IRMDiscoveryOffice365OptimizationService
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryOffice365OptimizationService));
        private readonly IRMDiscoveryOffice365OptimizationSettingsInfoDao _optimizationSettingsInfoDao = new RMDiscoveryOffice365OptimizationSettingsInfoDao();
        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>();
        private readonly IRMDiscoveryOffice365SiteOptimizationMappingTableDao _siteOptimizationMappingTableDao = new RMDiscoveryOffice365SiteOptimizationMappingTableDao();
        private readonly IJobMonitorService _jobMonitorService = PlatformWindsorManager.GetService<IJobMonitorService>();
        private readonly IJobQueueService _jobQueueService = PlatformWindsorManager.GetService<IJobQueueService>();
        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private readonly IRMDiscoveryOffice365NodeDao _nodeDao = new RMDiscoveryOffice365NodeDao();
        private readonly IRMDiscoveryOffice365JobDao _jobDao = new RMDiscoveryOffice365JobDao();
        private const int MappingInsertBatchSize = 1000;

        [AsyncAudit(Module = AuditModule.Discovery, Category = AuditCategory.DiscoveryConfiguration, Action = AuditAction.SaveOptimizationDataSetting, IAsyncBeforeHandler = typeof(RMDiscoveryOffice365ConfigurationBeforeAuditHandler))]
        public async Task<RAReturnMessage> SaveOptimizationSettingAsync(RMDiscoveryOffice365OptimizationSetting setting, List<string> importUrls,bool useImportSite)
        {
            RAReturnMessage status = new RAReturnMessage() { MessageType = RAMessageType.Successful, ErrorMessage = string.Empty };
            try
            {
                if (!CheckActionLicence(setting.ProcessActionParameter))
                {
                    status.MessageType = RAMessageType.Failed;
                    status.ErrorMessage = "Can not save this setting,because the licence does not support delected action.";
                    return status;
                }
                var (has, mainJobInfo) = await _jobDao.TryGetProcessingMainJobAsync();
                if (has)
                {
                    status.MessageType = RAMessageType.Failed;
                    status.ErrorMessage = I18NEntity.GetString("RM_DSO_DiscoverJobRunningSaveFailed");
                    return status;
                }
                var indexDevice = StorageDeviceService.GetIndexDevice();
                if (indexDevice == null)
                {
                    status.MessageType = RAMessageType.Failed;
                    status.ErrorMessage = I18NEntity.GetString("RM_DSO_GlobalStorageNotAvailable");
                    return status;
                }
                var tenantId = new Guid(setting.O365TenantId);
                Guid settingId = Guid.NewGuid();
                List<RMDiscoveryOffice365SiteOptimizationMappingInfo> mappingInfos = new List<RMDiscoveryOffice365SiteOptimizationMappingInfo>();
                RMDiscoveryOffice365OptimizationSettingsInfo setitngInfo = new RMDiscoveryOffice365OptimizationSettingsInfo()
                {
                    SettingId = settingId,
                    Type = 1,
                    NextTime = setting.ScheduleParameter.StartTime.Ticks == 0 ? DateTime.UtcNow.Ticks : setting.ScheduleParameter.StartTime.Ticks,
                    Setting = SerializerHelper.SerializeByDataContractSerializer(setting),
                    Status = (int)DiscoverOptimizationScheduleStatus.Ready
                };
                var exsitSettingInfo = await _optimizationSettingsInfoDao.GetSettingInfoBySettingAsync(setitngInfo.Setting, tenantId);
                if (exsitSettingInfo != null && exsitSettingInfo.Status == (int)DiscoverOptimizationScheduleStatus.Ready)
                {
                    status.MessageType = RAMessageType.Failed;
                    status.ErrorMessage = I18NEntity.GetString("RM_DSO_DiscoverJobRunningSaveFailedByMultipleClick");
                    return status;
                }
                await _optimizationSettingsInfoDao.AddOrUpdateAsync(setitngInfo, tenantId);
                List<int> siteIds = new List<int>();
                bool useImportFile = false;
                if (setting.NodeQueryParameter.ViewMode == RMDiscoveryNodeViewMode.Container)
                {
                    useImportFile = true;
                    var siteInfos = await _nodeDao.GetSiteInfosByContainerIds(tenantId, setting.NodeQueryParameter.ContainerIds);
                    foreach (var info in siteInfos)
                    {
                        siteIds.Add(info.Id);
                    }
                }
                else
                {
                    siteIds = setting.NodeQueryParameter.SiteIds;
                }
                // Resolve imported URLs to site IDs and keep only the selected scope intersection.
                if (useImportFile && importUrls != null && importUrls.Count > 0)
                {
                    var importSiteInfos = await _nodeDao.GetSiteInfosBySiteUrl(tenantId, importUrls);
                    var resolvedImportSiteInfos = importSiteInfos ?? new List<RMDiscoveryOffice365SiteInfo>();
                    var selectedSiteIdSet = new HashSet<int>(siteIds);
                    var importSiteIdSet = new HashSet<int>(resolvedImportSiteInfos.Select(s => s.Id));
                    var importUrlSet = new HashSet<string>(resolvedImportSiteInfos.Select(s => s.Url), StringComparer.OrdinalIgnoreCase);

                    var notFoundUrls = importUrls.Where(url => !importUrlSet.Contains(url)).ToList();
                    var filteredSiteIds = siteIds.Where(id => !importSiteIdSet.Contains(id)).ToList();
                    var importSiteInfosOutsideSelectedScope = resolvedImportSiteInfos.Where(info => !selectedSiteIdSet.Contains(info.Id)).ToList();
                    _logger.Info($"SaveOptimizationSettingAsync import scope diagnostics. tenant:{tenantId}, selectedSiteIds:{siteIds.Count}, importUrls:{importUrls.Count}, distinctImportUrls:{importUrls.Distinct(StringComparer.OrdinalIgnoreCase).Count()}, matchedImportSiteInfos:{resolvedImportSiteInfos.Count}, matchedImportSiteIds:{importSiteIdSet.Count}, notFoundImportUrls:{notFoundUrls.Count}, selectedSiteIdsFilteredOut:{filteredSiteIds.Count}, importSiteIdsOutsideSelectedScope:{importSiteInfosOutsideSelectedScope.Count}.");

                    foreach (var url in notFoundUrls)
                    {
                        _logger.Warn($"importUrls contains url not found in siteInfos: {url}");
                    }

                    foreach (var id in filteredSiteIds)
                    {
                        _logger.Info($"siteId {id} is not in importUrls, filtered out from siteIds");
                    }

                    foreach (var info in importSiteInfosOutsideSelectedScope)
                    {
                        _logger.Info($"importUrls contains url found in siteInfos but outside selected siteIds: siteId {info.Id}, url {info.Url}");
                    }

                    siteIds = siteIds.Where(id => importSiteIdSet.Contains(id)).ToList();
                }
                else if(useImportFile && useImportSite)
                {
                    siteIds = new List<int>();
                }
                foreach (var nodeId in siteIds)
                {
                    RMDiscoveryOffice365SiteOptimizationMappingInfo mappingInfo = new RMDiscoveryOffice365SiteOptimizationMappingInfo()
                    {
                        NodeId = nodeId,
                        SettingId = settingId,
                    };
                    mappingInfos.Add(mappingInfo);
                }
                var totalMappings = mappingInfos.Count;
                if (totalMappings == 0)
                {
                    _logger.Info($"SaveOptimizationSettingAsync found no mapping records to upsert. tenant:{tenantId}.");
                }
                else
                {
                    var inserted = 0;
                    var batchIndex = 0;
                    while (inserted < totalMappings)
                    {
                        var batch = mappingInfos.Skip(inserted).Take(MappingInsertBatchSize).ToList();
                        _logger.Info($"SaveOptimizationSettingAsync writing batch {batchIndex} with {batch.Count} records. tenant:{tenantId}.");
                        await _siteOptimizationMappingTableDao.AddOrUpdateAsync(batch, tenantId);
                        inserted += batch.Count;
                        batchIndex++;
                    }
                    _logger.Info($"SaveOptimizationSettingAsync finished writing mappings. total:{totalMappings}, tenant:{tenantId}.");
                }

                SendOptimizationCalculateJob(tenantId, settingId);
                return status;
            }
            catch (Exception e)
            {
                status.MessageType = RAMessageType.Failed;
                status.ErrorMessage = e.Message;
                _logger.Error($"save optimization failed,error;{e}");
                return status;
            }
        }

        [AsyncAudit(Module = AuditModule.Discovery, Category = AuditCategory.DiscoveryConfiguration, Action = AuditAction.SaveOptimizationDataSetting, IAsyncBeforeHandler = typeof(RMDiscoveryOffice365ConfigurationBeforeAuditHandler))]
        public Task<RAReturnMessage> SaveDiscoveryPlanProOptimizationSettingAsync(List<string> profiles)
        {
            return Task.FromResult(SendDiscoveryPlanProQueueJob(JobType.DiscoveryPlanProOptimization, profiles));
        }
        private bool IsEnableDeleteOnlySetting()
        {
            var key = RMKeyValueDao.GetValueByKey("EnableDeleteOnlyOption");
            bool.TryParse(key?.Value, out bool result);
            return result;
        }
        private bool IsEnableArchiveOnlySetting()
        {
            var key = RMKeyValueDao.GetValueByKey("EnableArchiveOnlyOption");
            bool.TryParse(key?.Value, out bool result);
            return result;
        }
        [AsyncAudit(Module = AuditModule.Discovery, Category = AuditCategory.DiscoveryConfiguration, Action = AuditAction.SaveOptimizationDataPreScanSetting, IAsyncBeforeHandler = typeof(RMDiscoveryOffice365ConfigurationBeforeAuditHandler))]
        public async Task<RAReturnMessage> SaveOptimizationPreScanSettingAsync(RMDiscoveryOffice365OptimizationSetting setting)
        {
            RAReturnMessage status = new RAReturnMessage() { MessageType = RAMessageType.Successful, ErrorMessage = string.Empty };
            try
            {
                var (has, mainJobInfo) = await _jobDao.TryGetProcessingMainJobAsync();
                if (has)
                {
                    status.MessageType = RAMessageType.Failed;
                    status.ErrorMessage = I18NEntity.GetString("RM_DSO_DiscoverJobRunningSaveFailed");
                    return status;
                }
                var tenantId = new Guid(setting.O365TenantId);
                Guid settingId = Guid.NewGuid();
                var exsitSettingInfo = await _optimizationSettingsInfoDao.GetSettingInfoBySettingAsync(SerializerHelper.SerializeByDataContractSerializer(setting), tenantId);
                List<long> siteIds = new List<long>();
                if (setting.NodeQueryParameter.ViewMode == RMDiscoveryNodeViewMode.Container)
                {
                    var siteInfos = await _nodeDao.GetSiteInfosByContainerIds(tenantId, setting.NodeQueryParameter.ContainerIds);
                    foreach (var info in siteInfos)
                    {
                        siteIds.Add(info.Id);
                    }
                }
                else
                {
                    siteIds = setting.NodeQueryParameter.SiteIds.Select(siteId => (long)siteId).ToList();
                }


                RMDiscoverOptimizationPreScanJobInfo jobParaInfo = new RMDiscoverOptimizationPreScanJobInfo();
                jobParaInfo.SettingInfo = setting;
                jobParaInfo.SiteIds = siteIds.Distinct().ToList(); ;

                try
                {
                    JobQueueDto jqDto = new JobQueueDto()
                    {
                        JobType = JobType.DiscoveryPreScan,
                        JobRunType = JobRunBy.Control,
                        TenantGroupId = TenantLocalValue.LogonGroupId,
                        JobRunByUser = TenantLocalValue.LogonUserEmail,
                        Parameters = SerializerHelper.SerializeByDataContractSerializer(jobParaInfo),
                    };
                    _jobQueueService.AddToDBJobQueue(jqDto);
                }
                catch (Exception e)
                {
                    _logger.Error($"An error occurred while send optimization preScan job. Error: {e}");
                }
                return status;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while save optimization pre scan setting. Error: {e}");
                status.MessageType = RAMessageType.Failed;
                status.ErrorMessage = e.Message;
                return status;
            }
        }

        [AsyncAudit(Module = AuditModule.Discovery, Category = AuditCategory.DiscoveryConfiguration, Action = AuditAction.SaveOptimizationDataPreScanSetting, IAsyncBeforeHandler = typeof(RMDiscoveryOffice365ConfigurationBeforeAuditHandler))]
        public Task<RAReturnMessage> SaveDiscoveryPlanProScanSettingAsync(List<string> profiles)
        {
            return Task.FromResult(SendDiscoveryPlanProQueueJob(JobType.DiscoveryPlanProScan, profiles));
        }
        public async Task<RAReturnMessage> RunCleanUpDuplicateDataJob(string cleanupInfo, string O365TenantId)
        {
            RAReturnMessage status = new RAReturnMessage() { MessageType = RAMessageType.Successful, ErrorMessage = string.Empty };
            try
            {
                var cleanupInfoDto = new RMDiscoveryOffice365CleanupInfoDto()
                {
                    CleanupInfo = JsonConvert.DeserializeObject<RMDiscoveryOffice365CleanupInfo>(cleanupInfo),
                    O365TenantId = O365TenantId,
                };

                var storagePolicyId = cleanupInfoDto.CleanupInfo?.StoragePolicyId;
                if (string.IsNullOrWhiteSpace(storagePolicyId))
                {
                    _logger.Warn("RunCleanUpDuplicateDataJob failed: StoragePolicyId is missing or empty.");
                    status.MessageType = RAMessageType.Failed;
                    status.ErrorMessage = I18NEntity.GetString("RM_FA_Discovery_RunJobFailed");
                    return status;
                }

                var storageDevice = StorageDeviceService.GetStorageDeviceById(storagePolicyId);
                if (storageDevice == null)
                {
                    _logger.Warn($"RunCleanUpDuplicateDataJob failed: Storage '{storagePolicyId}' does not exist or has been deleted.");
                    status.MessageType = RAMessageType.Failed;
                    status.ErrorMessage = I18NEntity.GetString("RM_FA_Discovery_RunJobFailed");
                    return status;
                }

                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.CleanUpDuplicateDatas,
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    JobRunByUser = TenantLocalValue.LogonUserEmail,
                    Parameters = SerializerHelper.SerializeByDataContractSerializer(cleanupInfoDto),
                };
                _jobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while send optimization preScan job. Error: {e}");
                status.MessageType = RAMessageType.Failed;
                status.ErrorMessage = e.Message;
            }
            return status;
        }
        private bool CheckActionLicence(ProcessActionParameter para)
        {
            bool needCheckUserLicense = false;
            if ((para.FileAction == FileAction.Remove || para.VersionAction == VersionAction.RemoveVersion) && !IsEnableDeleteOnlySetting())
            {
                _logger.Info("not config db enable delete only, need check user license");
                needCheckUserLicense = true;
            }
            if (para.FileAction == FileAction.Archive)
            {
                if (!IsEnableArchiveOnlySetting())
                {
                    _logger.Warn("not config db enable archive only");
                    return false;
                }
                needCheckUserLicense = true;
            }
            if (needCheckUserLicense && !IsPrePaidConsumption())
            {
                return false;
            }
            return true;
        }
        private bool IsPrePaidConsumption()
        {
            try
            {
                var client = AosApiUtility.GetAosModernClient(TenantLocalValue.LogonGroupId);
                var info = client.LicenseService.GetLicenseAsync(RecordsConstants.RECORDS_APPLICATION_NAME).GetAwaiter().GetResult();
                if (info.Extension is Cloud.Sdk.Data.AosModern.CloudRecordsExtension)
                {
                    Cloud.Sdk.Data.AosModern.CloudRecordsExtension extension = info.Extension as Cloud.Sdk.Data.AosModern.CloudRecordsExtension;
                    if (extension.SaleType == Cloud.Sdk.Data.AosModern.SaleType.PrePaidConsumption)
                    {
                        //RMKeyValueDao.SaveAsync(new DB.Model.RMKeyValue() { Key= keyString ,Value="true"}).GetAwaiter().GetResult();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                //}
                return false;
            }
            catch (Exception e)
            {
                _logger.Error($"some thing went wrong when check Delete only action enabled,error{e.ToString()}");
                return false;
            }
        }

        private void SendOptimizationCalculateJob(Guid o365TenantId, Guid settingId)
        {
            try
            {
                JobQueueDto jqDto = new()
                {
                    JobType = JobType.DiscoveryOptimizationCalculate,
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    JobRunByUser = "RM_TS_RunSchedule",
                    Parameters = JsonConvert.SerializeObject(new List<string>
                    {
                        settingId.ToString(),
                        o365TenantId.ToString(),
                    }),
                };
                _jobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while send optimization calculate job. Error: {e}");
            }
        }

        private RAReturnMessage SendDiscoveryPlanProQueueJob(JobType jobType, List<string> profiles)
        {
            RAReturnMessage status = new RAReturnMessage()
            {
                MessageType = RAMessageType.Successful,
                ErrorMessage = "Success",
                FaildType = RAFailedType.None,
                Extension = jobType.ToString(),
            };
            try
            {
                var filteredProfiles = (profiles ?? new List<string>())
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Select(id => id.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (!filteredProfiles.Any())
                {
                    status.MessageType = RAMessageType.Failed;
                    status.ErrorMessage = I18NEntity.GetString("RM_DSO_DiscoverJobRunningSaveFailed");
                    return status;
                }

                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = jobType,
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    JobRunByUser = TenantLocalValue.LogonUserEmail,
                    Parameters = SerializerHelper.SerializeByDataContractSerializer(filteredProfiles),
                };
                _jobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while send discovery plan pro job. jobType:{jobType}, error:{e}");
            }
            return status;
        }

        public string RealRunOptimizationCalculateJob(string parameters)
        {
            try
            {
                var parameterList = JsonConvert.DeserializeObject<List<string>>(parameters);
                var jobId = _jobMonitorService.CreateJob(JobType.DiscoveryOptimizationCalculate, "RM_TS_RunSchedule");
                _jobQueueService.HandleMessage(new JobQueueMessage()
                {
                    JobId = jobId,
                    JobType = JobType.DiscoveryOptimizationCalculate,
                    CommandLine = $"{JobType.DiscoveryOptimizationCalculate} {jobId} {parameterList[0]} {parameterList[1]}",
                });
                return jobId;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while real run optimization calculate job. Error: {e}");
                return string.Empty;
            }
        }
    }
}
