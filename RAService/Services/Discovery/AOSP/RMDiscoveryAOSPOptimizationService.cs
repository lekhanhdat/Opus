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
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AvePoint.RA.DB.Dao.Discovery.AOSP;
using AvePoint.RA.DB.Dao.Discovery.Impl.AOSP;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Office365;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Contract.Discovery.Model.Configuration.AOSP;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.DB.Model.Discovery.AOSP;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using SerializerHelper = AvePoint.RA.Common.Global.Utils.SerializerHelper;
using AvePoint.RA.Contract.CloudService;
using Newtonsoft.Json;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Service.JobMonitor;
using Storage;
using AvePoint.GCommon.Contract.ReportCenter.Object;
using ProfileType = AvePoint.GCommon.Contract.Server.Common.Profile.Object.ProfileType;
using ProfileTimeUnit = AvePoint.GCommon.Contract.Server.Common.Profile.Object.TimeUnit;
using AvePoint.RA.Contract;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using System.Linq;

namespace AvePoint.RA.Service.Services.Discovery.AOSP
{
    public class RMDiscoveryAOSPOptimizationService : IRMDiscoveryAOSPOptimizationService
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryAOSPOptimizationService));

        private readonly IRMDiscoveryAOSPOptimizationSettingsInfoDao _optimizationSettingsInfoDao = new RMDiscoveryAOSPOptimizationSettingsInfoDao();

        private readonly IRMDiscoveryAOSPSiteOptimizationMappingTableDao _siteOptimizationMappingTableDao = new RMDiscoveryAOSPSiteOptimizationMappingTableDao();

        private readonly IJobMonitorService _jobMonitorService = PlatformWindsorManager.GetService<IJobMonitorService>();

        private readonly IJobQueueService _jobQueueService = PlatformWindsorManager.GetService<IJobQueueService>();

        private readonly ITenantService _tenantService = PlatformWindsorManager.GetService<ITenantService>();

        private readonly IRMDiscoveryAOSPJobDao _jobDao = new RMDiscoveryAOSPJobDao();

        private readonly IRMDiscoveryAOSPNodeDao _nodeDao = new RMDiscoveryAOSPNodeDao();

        private readonly IRMMiscProfileDao _miscProfileDao = PlatformWindsorManager.GetService<IRMMiscProfileDao>();

        private readonly IRMArchiverSettingsService _archiverSettingsService = PlatformWindsorManager.GetService<IRMArchiverSettingsService>();

        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>();

        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private IJobMonitorService JobMonitorService => _jobMonitorService;
        private readonly ILoginService _loginService = PlatformWindsorManager.GetService<ILoginService>();
        private readonly IGeneralSettingService _generalSettingService = PlatformWindsorManager.GetService<IGeneralSettingService>();
        public async Task<RMDiscoveryReturnMessage> SaveOptimizationSettingAsync(RMDiscoveryAOSPOptimizationSetting setting)
        {
            RMDiscoveryReturnMessage status = new RMDiscoveryReturnMessage() { MessageType = RAMessageType.Successful, ErrorMessage = string.Empty };
            try
            {
                _logger.Info("start check the tenant is New opus tenant.");
                _logger.Info("Whether to enable locked supportLockedSite ：" + setting.SupportLockedSite);
                _logger.Info("Aosp RMDiscoveryAOSPOptimizationSetting siteInfo ：" + setting.SiteInfos);
                var isNewTenant = await _tenantService.InitAOSPTenantAsync(setting.LogonUserName);
                if (isNewTenant)
                {
                    RMKeyValueDao.Save(new RMKeyValue() { Key = "RunDisposalInRecords", Value = "True" });
                    await _loginService.InitSecurityProfileAsync();
                    await _generalSettingService.VerifyAndCreateDefaultSecurityProfileAsync();
                }
                if (!_tenantService.IsNewOpusTenant())
                {
                    status.MessageType = RAMessageType.Failed;
                    status.ErrorMessage = I18NEntity.GetString("RM_DSO_DiscoverJobRunningSaveFailed");
                    return status;
                }
                _logger.Info("finish check the tenant is New opus tenant.");
                await _tenantService.CheckAndUpdateAOSPTenantAsync();
                AddSyncArchivedSiteInfoKey();
                if (!setting.UseArchiverProfile)
                {
                    var (has, mainJobInfo) = await _jobDao.TryGetProcessingMainJobAsync(setting.O365TenantId);
                    if (has)
                    {
                        status.MessageType = RAMessageType.Failed;
                        status.ErrorMessage = "RM_DSO_DiscoverJobRunningSaveFailed";
                        return status;
                    }
                }
                var storageId = RecordsConstants.AVEPOINT_DEFAULT_STORAGEID;
                if (!string.IsNullOrEmpty(setting.StorageId))
                {
                    storageId = setting.StorageId;
                }

                var storageInfo = StorageDeviceService.GetStorageDeviceById(storageId);
                if (storageInfo == null)
                {
                    _logger.Error("Can not get the avepoint storage.");
                    status.MessageType = RAMessageType.Failed;
                    status.ErrorMessage = "RM_DSO_GlobalStorageNotAvailable";
                    return status;
                }
                var indexDevice = StorageDeviceService.GetIndexDevice();
                if (indexDevice == null)
                {
                    await StorageDeviceService.SetUsingDeviceByIdAsync(storageInfo.Id, SettingProfilesType.IndexDevice, storageInfo.Name);
                    indexDevice = StorageDeviceService.GetIndexDevice();
                    if(indexDevice == null)
                    {
                        _logger.Error("Can not get the default storage index device.");
                        status.MessageType = RAMessageType.Failed;
                        status.ErrorMessage = "RM_DSO_GlobalStorageNotAvailable";
                        return status;
                    }
                }
                var settingId = Guid.NewGuid();
                var mappingInfos = new List<RMDiscoveryAOSPSiteOptimizationMappingInfo>();
                setting.SelectedStorage.Id = storageInfo.Id;
                setting.SelectedStorage.Name = storageInfo.Name;
                var settingInfo = new RMDiscoveryAOSPOptimizationSettingsInfo()
                {
                    SettingId = settingId,
                    Type = 1,
                    NextTime = setting.ScheduleParameter.StartTime.Ticks == 0 ? DateTime.UtcNow.Ticks : setting.ScheduleParameter.StartTime.Ticks,
                    Setting = SerializerHelper.SerializeByDataContractSerializer(setting),
                    Status = (int)DiscoverOptimizationScheduleStatus.Ready
                };
                _logger.Info($"AOSP optimization setting serialized. SettingId:{settingId}, SupportLockedSite:{setting.SupportLockedSite}");
                if (!setting.UseArchiverProfile)
                {
                    var exsitSettingInfo = await _optimizationSettingsInfoDao.GetSettingInfoBySettingAsync(settingInfo.Setting, new Guid(setting.O365TenantId));
                    if (exsitSettingInfo != null && exsitSettingInfo.Status == (int)DiscoverOptimizationScheduleStatus.Ready)
                    {
                        status.MessageType = RAMessageType.Failed;
                        status.ErrorMessage = "RM_DSO_DiscoverJobRunningSaveFailedByMultipleClick";
                        return status;
                    }
                }
                var jobId = JobMonitorService.GenerateJobId(JobType.DiscoveryAOSPOptimization);
                settingInfo.JobId = jobId;
                status.JobId = jobId;
                if (!setting.UseArchiverProfile)
                {
                    await _optimizationSettingsInfoDao.AddOrUpdateAsync(settingInfo, new Guid(setting.O365TenantId));

                    var siteIds = setting.NodeIds;
                    foreach (var siteId in siteIds)
                    {
                        var siteNode = await _nodeDao.GetDiscoverySiteInfoAsync(new Guid(setting.O365TenantId), new Guid(siteId));
                        var mappingInfo = new RMDiscoveryAOSPSiteOptimizationMappingInfo()
                        {
                            NodeId = siteNode.Id,
                            SettingId = settingId,
                        };
                        mappingInfos.Add(mappingInfo);
                    }
                    await _siteOptimizationMappingTableDao.AddOrUpdateAsync(mappingInfos, new Guid(setting.O365TenantId));
                    SendOptimizationCalculateJob(new Guid(setting.O365TenantId), settingId);
                }
                else
                {
                    _logger.Info($"retention rule is enable:EnableRetainArchivedData:{setting.EnableRetainArchivedData},RemoveRelatedJobsFromJobMonitor:{setting.RemoveRelatedJobsFromJobMonitor}, start persist retention profile for AOSP tenant:{setting.O365TenantId}, settingId:{settingId}");
                    await PersistAOSPArchiverRetentionProfilesAsync(setting, storageInfo);
                    _logger.Info($"finish persist retention profile for AOSP tenant:{setting.O365TenantId}, settingId:{settingId}");
                    setting.JobId = jobId;
                    SendOptimizationArchiverProfileJob(new Guid(setting.O365TenantId), settingId, setting);
                }
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

        public async Task<RAReturnMessage> RunRetentionJob()
        {
            _logger.Info($"RunRetention Job start. LogonGroupId: {TenantLocalValue.LogonGroupId}, LogonUserEmail: {TenantLocalValue.LogonUserEmail}");

            try
            {
                var jobId = await _archiverSettingsService.RealRunArchiverRetentionJobAsync(JobRunBy.Control, TenantLocalValue.LogonUserEmail);
                if (string.IsNullOrWhiteSpace(jobId))
                {
                    return new RAReturnMessage
                    {
                        MessageType = RAMessageType.Failed,
                        FaildType = RAFailedType.None,
                        Extension = string.Empty
                    };
                }

                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Successful,
                    FaildType = RAFailedType.None,
                    Extension = jobId
                };
            }
            catch (Exception ex)
            {
                _logger.Error($"RunRetention Job failed, ERROR:{ex}");
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    FaildType = RAFailedType.None,
                    Extension = string.Empty,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<RAReturnMessage> UpdateArchiveProfileRetentionAsync(RMDiscoveryAOSPArchiveProfileRetentionRequest request)
        {
            var status = new RAReturnMessage() { MessageType = RAMessageType.Successful, ErrorMessage = string.Empty };
            try
            {
                var archiveProfileId = request?.ArchiveProfileId;
                var enableRetainArchivedData = request?.EnableRetainArchivedData ?? false;
                if (string.IsNullOrWhiteSpace(archiveProfileId))
                {
                    status.MessageType = RAMessageType.Failed;
                    status.ErrorMessage = "ArchiveProfileId is required.";
                    return status;
                }

                var profiles = (await _miscProfileDao.FindListAsync(p =>
                    p.Type == (int)ProfileType.AOSPArchiverRuleForRevIM &&
                    p.ArchiveProfileId == archiveProfileId)).ToList();

                if (profiles.Count == 0)
                {
                    CreateAOSPArchiveProfileRetentionRule(request);
                    _logger.Info($"Create archive profile retention success. ArchiveProfileId:{archiveProfileId}, EnableRetainArchivedData:{enableRetainArchivedData}");
                    return status;
                }

                var targetIsRemoved = !enableRetainArchivedData;
                foreach (var profile in profiles)
                {
                    var hasChanges = false;

                    if (profile.IsRemoved != targetIsRemoved)
                    {
                        profile.IsRemoved = targetIsRemoved;
                        hasChanges = true;
                    }

                    var rule = SerializerHelper.DeserializeByDataContractSerializer<Rule>(profile.Extension);
                    var retentionRule = rule?.StoreContentRetentionInfos?.FirstOrDefault();
                    if (retentionRule != null)
                    {
                        if (request?.RetentionKeepValue.HasValue == true && request.RetentionKeepValue.Value > 0 && retentionRule.KeepValue != request.RetentionKeepValue.Value)
                        {
                            retentionRule.KeepValue = request.RetentionKeepValue.Value;
                            hasChanges = true;
                        }

                        if (request?.RetentionKeepUnit.HasValue == true)
                        {
                            var keepUnit = request.RetentionKeepUnit.Value == ProfileTimeUnit.None ? ProfileTimeUnit.Year : request.RetentionKeepUnit.Value;
                            var dateUnit = ConvertTimeUnitToDateUnit(keepUnit);
                            if (retentionRule.ArchiveDateUnit != dateUnit)
                            {
                                retentionRule.ArchiveDateUnit = dateUnit;
                                hasChanges = true;
                            }
                        }

                        if (request?.RemoveRelatedJobsFromJobMonitor.HasValue == true && retentionRule.RemoveTheJob != request.RemoveRelatedJobsFromJobMonitor.Value)
                        {
                            retentionRule.RemoveTheJob = request.RemoveRelatedJobsFromJobMonitor.Value;
                            hasChanges = true;
                        }

                        if (request?.DeleteRelatedStubsFromOriginalLocations.HasValue == true)
                        {
                            var keepStub = !request.DeleteRelatedStubsFromOriginalLocations.Value;
                            if (retentionRule.KeepOrphanedStub4CompatibilityExistingRule != keepStub)
                            {
                                retentionRule.KeepOrphanedStub4CompatibilityExistingRule = keepStub;
                                hasChanges = true;
                            }
                        }

                        if (hasChanges)
                        {
                            rule.ModifyTime = DateTime.UtcNow.Ticks;
                            profile.Extension = SerializerHelper.SerializeByDataContractSerializer(rule);
                        }
                    }

                    if (hasChanges)
                    {
                        profile.ModifiedTime = DateTime.UtcNow.Ticks;
                        await _miscProfileDao.UpdateAsync(profile);
                    }
                }

                _logger.Info($"Update archive profile retention success. ArchiveProfileId:{archiveProfileId}, EnableRetainArchivedData:{enableRetainArchivedData}");
                return status;
            }
            catch (Exception e)
            {
                status.MessageType = RAMessageType.Failed;
                status.ErrorMessage = e.Message;
                _logger.Error($"Update archive profile retention failed. ArchiveProfileId:{request?.ArchiveProfileId}, Error:{e}");
                return status;
            }
        }


        public async Task<RAReturnMessage> DeleteArchiveProfileAsync(IEnumerable<string> archiveProfileIds)
        {
            var status = new RAReturnMessage() { MessageType = RAMessageType.Successful, ErrorMessage = string.Empty };
            try
            {
                var normalizedIds = (archiveProfileIds ?? Enumerable.Empty<string>())
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (normalizedIds.Count == 0)
                {
                    status.MessageType = RAMessageType.Failed;
                    status.ErrorMessage = "ArchiveProfileId is required.";
                    return status;
                }

                var profiles = (await _miscProfileDao.FindListAsync(p =>
                    p.Type == (int)ProfileType.AOSPArchiverRuleForRevIM &&
                    normalizedIds.Contains(p.ArchiveProfileId))).ToList();

                if (profiles.Count == 0)
                {
                    _logger.Warn($"Delete archive profile skipped because no rules were found. ArchiveProfileIds:{string.Join(",", normalizedIds)}");
                    return status;
                }

                await _miscProfileDao.BatchDeleteAsync(profiles.Select(p => p.Id).ToList());

                _logger.Info($"Delete archive profile success. ArchiveProfileIds:{string.Join(",", normalizedIds)}, RuleCount:{profiles.Count}");
                return status;
            }
            catch (Exception e)
            {
                status.MessageType = RAMessageType.Failed;
                status.ErrorMessage = e.Message;
                _logger.Error($"Delete archive profile failed. ArchiveProfileIds:{string.Join(",", archiveProfileIds ?? Enumerable.Empty<string>())}, Error:{e}");
                return status;
            }
        }

        private async Task PersistAOSPArchiverRetentionProfilesAsync(RMDiscoveryAOSPOptimizationSetting setting, StorageDeviceDto storageInfo)
        {
            if (!setting.UseArchiverProfile)
            {
                _logger.Info("Skip persisting AOSP archiver retention profiles because UseArchiverProfile is false.");
                return;
            }

            try
            {
                var ruleDefinitions = setting.RuleDefinition?.ToList() ?? new List<RMDiscoveryRuleDefinition>();
                _logger.Info($"Start persisting AOSP archiver retention profiles. TenantId:{setting.O365TenantId}, ArchiveProfileId:{setting.ArchiverProfileId}, RuleCount:{ruleDefinitions.Count}, EnableRetainArchivedData:{setting.EnableRetainArchivedData}, RemoveRelatedJobsFromJobMonitor:{setting.RemoveRelatedJobsFromJobMonitor}, DeleteRelatedStubsFromOriginalLocations:{setting.DeleteRelatedStubsFromOriginalLocations}");

                var existingProfiles = await _miscProfileDao.FindListAsync(p =>
                    p.Type == (int)ProfileType.AOSPArchiverRuleForRevIM &&
                    p.ArchiveProfileId == setting.ArchiverProfileId);
                var existingProfileMap = existingProfiles.ToDictionary(p => p.Id, StringComparer.OrdinalIgnoreCase);
                var activeRuleIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                _logger.Info($"Found existing retention profiles. ArchiveProfileId:{setting.ArchiverProfileId}, ExistingProfileCount:{existingProfiles.Count}");

                foreach (var ruleDefinition in ruleDefinitions)
                {
                    var ruleId = ruleDefinition.UniqueId.ToString();
                    _logger.Info($"Processing retention rule. ArchiveProfileId:{setting.ArchiverProfileId}, RuleId:{ruleId}, RuleName:{ruleDefinition.Name}");

                    if (!setting.EnableRetainArchivedData)
                    {
                        if (existingProfileMap.TryGetValue(ruleId, out var disabledProfile) && !disabledProfile.IsRemoved)
                        {
                            disabledProfile.IsRemoved = true;
                            disabledProfile.ModifiedTime = DateTime.UtcNow.Ticks;
                            await _miscProfileDao.UpdateAsync(disabledProfile);
                            _logger.Info($"Disabled existing retention profile because retain archived data is disabled. ArchiveProfileId:{setting.ArchiverProfileId}, RuleId:{ruleId}");
                        }
                        else
                        {
                            _logger.Info($"No active existing profile to disable. ArchiveProfileId:{setting.ArchiverProfileId}, RuleId:{ruleId}");
                        }
                        continue;
                    }

                    activeRuleIds.Add(ruleId);

                    var rule = BuildAOSPRetentionRule(setting, storageInfo, ruleDefinition);
                    var profile = new RMMiscProfile()
                    {
                        Id = rule.Id,
                        Type = (int)ProfileType.AOSPArchiverRuleForRevIM,
                        Name = string.IsNullOrWhiteSpace(rule.Name) ? $"AOSP-Retention-{rule.Id}" : rule.Name,
                        ArchiveProfileId = setting.ArchiverProfileId,
                        ModifiedTime = DateTime.UtcNow.Ticks,
                        Extension = SerializerHelper.SerializeByDataContractSerializer(rule),
                        IsRemoved = false,
                    };

                    if (existingProfileMap.TryGetValue(profile.Id, out var existingProfile))
                    {
                        profile.DAOMigrated = existingProfile.DAOMigrated;
                        await _miscProfileDao.UpdateAsync(profile);
                        _logger.Info($"Updated existing retention profile. ArchiveProfileId:{setting.ArchiverProfileId}, RuleId:{profile.Id}");
                    }
                    else
                    {
                        _miscProfileDao.Create(profile);
                        _logger.Info($"Created new retention profile. ArchiveProfileId:{setting.ArchiverProfileId}, RuleId:{profile.Id}");
                    }
                }

                var staleProfiles = existingProfiles.Where(p => !activeRuleIds.Contains(p.Id) && !p.IsRemoved).ToList();
                foreach (var existingProfile in staleProfiles)
                {
                    existingProfile.IsRemoved = true;
                    existingProfile.ModifiedTime = DateTime.UtcNow.Ticks;
                    await _miscProfileDao.UpdateAsync(existingProfile);
                    _logger.Info($"Marked stale retention profile as removed. ArchiveProfileId:{setting.ArchiverProfileId}, RuleId:{existingProfile.Id}");
                }

                _logger.Info($"Finished persisting AOSP archiver retention profiles. ArchiveProfileId:{setting.ArchiverProfileId}, ActiveRuleCount:{activeRuleIds.Count}, RemovedStaleProfileCount:{staleProfiles.Count}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Persisting AOSP archiver retention profiles failed. TenantId:{setting.O365TenantId}, ArchiveProfileId:{setting.ArchiverProfileId}, Error:{ex}");
                throw;
            }
        }

        private void CreateAOSPArchiveProfileRetentionRule(RMDiscoveryAOSPArchiveProfileRetentionRequest request)
        {
            var ruleId = Guid.NewGuid().ToString();
            var keepValue = request.RetentionKeepValue.GetValueOrDefault(1);
            if (keepValue <= 0)
            {
                keepValue = 1;
            }

            var keepUnit = request.RetentionKeepUnit.GetValueOrDefault(ProfileTimeUnit.Year);
            if (keepUnit == ProfileTimeUnit.None)
            {
                keepUnit = ProfileTimeUnit.Year;
            }

            var rule = new Rule()
            {
                Id = ruleId,
                Name = "AOSP retention rule",
                ModifyTime = DateTime.UtcNow.Ticks,
                ProfileType = ProfileType.AOSPArchiverRuleForRevIM,
                IncludeNew = "1",
                IsEnableRetention = false,
                IsEnableStoreContentRetention = true,
                StoreContentRetentionInfos = new List<RetentionRule>
                {
                    new RetentionRule()
                    {
                        SetupDataRetention = true,
                        KeepValue = keepValue,
                        ArchiveDateUnit = ConvertTimeUnitToDateUnit(keepUnit),
                        RetentionDataTimeType = KeepDateType.ArchiveTime,
                        DeleteTheData = true,
                        RemoveTheJob = request.RemoveRelatedJobsFromJobMonitor.GetValueOrDefault(true),
                        KeepOrphanedStub4CompatibilityExistingRule = !request.DeleteRelatedStubsFromOriginalLocations.GetValueOrDefault(false),
                    }
                }
            };

            _miscProfileDao.Create(new RMMiscProfile()
            {
                Id = ruleId,
                Type = (int)ProfileType.AOSPArchiverRuleForRevIM,
                Name = rule.Name,
                ArchiveProfileId = request.ArchiveProfileId,
                ModifiedTime = DateTime.UtcNow.Ticks,
                Extension = SerializerHelper.SerializeByDataContractSerializer(rule),
                IsRemoved = !request.EnableRetainArchivedData.GetValueOrDefault(false),
            });
        }

        private Rule BuildAOSPRetentionRule(RMDiscoveryAOSPOptimizationSetting setting, StorageDeviceDto storageInfo, RMDiscoveryRuleDefinition ruleDefinition)
        {
            var keepValue = setting.RetentionKeepValue > 0 ? setting.RetentionKeepValue : 1;
            var keepUnit = setting.RetentionKeepUnit == ProfileTimeUnit.None ? ProfileTimeUnit.Year : setting.RetentionKeepUnit;
            var retentionDataTimeType = setting.RetentionDataTimeType == KeepDateType.None
                ? KeepDateType.ArchiveTime
                : setting.RetentionDataTimeType;

            return new Rule()
            {
                Id = ruleDefinition.UniqueId.ToString(),
                Name = string.IsNullOrWhiteSpace(ruleDefinition.Name)
                    ? (string.IsNullOrWhiteSpace(setting.ArchiverProfileName) ? "AOSP retention rule" : setting.ArchiverProfileName)
                    : ruleDefinition.Name,
                ModifyTime = DateTime.UtcNow.Ticks,
                ProfileType = ProfileType.AOSPArchiverRuleForRevIM,
                IncludeNew = "1",
                StoragePolicyId = storageInfo?.Id,
                StoragePolicyName = storageInfo?.Name,
                IsEnableRetention = false,
                IsEnableStoreContentRetention = true,
                StoreContentRetentionInfos = new List<RetentionRule>
                {
                    new RetentionRule()
                    {
                        SetupDataRetention = true,
                        KeepValue = keepValue,
                        ArchiveDateUnit = ConvertTimeUnitToDateUnit(keepUnit),
                        RetentionDataTimeType = retentionDataTimeType,
                        DeleteTheData = true,
                        RemoveTheJob = setting.RemoveRelatedJobsFromJobMonitor,
                        KeepOrphanedStub4CompatibilityExistingRule = !setting.DeleteRelatedStubsFromOriginalLocations,
                    }
                }
            };
        }

        private static DateUnit ConvertTimeUnitToDateUnit(ProfileTimeUnit timeUnit)
        {
            return timeUnit switch
            {
                ProfileTimeUnit.Day => DateUnit.Day,
                ProfileTimeUnit.Week => DateUnit.Week,
                ProfileTimeUnit.Month => DateUnit.Month,
                ProfileTimeUnit.Year => DateUnit.Year,
                _ => DateUnit.Year,
            };
        }

        private void AddSyncArchivedSiteInfoKey()
        {
            try 
            {
                var keyValue = RMKeyValueDao.GetValueByKey(KeyNameCollection.SyncArchivedSiteInfo);
                if (keyValue == null || !bool.TryParse(keyValue.Value, out bool value) || !value)
                {
                    RMKeyValueDao.SaveOrUpdateAsync(new RMKeyValue()
                    {
                        Key = KeyNameCollection.SyncArchivedSiteInfo,
                        Value = "true",
                    });
                    _logger.Info("Add SyncArchivedSiteInfo key success.");
                }
            }
            catch(Exception e)
            {
                _logger.Error($"Add SyncArchivedSiteInfo key has error: {e.Message}.");
            }
        }

        private void SendOptimizationCalculateJob(Guid o365TenantId, Guid settingId)
        {
            try
            {
                JobQueueDto jqDto = new()
                {
                    JobType = JobType.DiscoveryAOSPOptimizationCalculate,
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
        private void SendOptimizationArchiverProfileJob(Guid o365TenantId, Guid settingId, RMDiscoveryAOSPOptimizationSetting setting)
        {
            try
            {
                JobQueueDto jqDto = new()
                {
                    JobType = JobType.DiscoveryAOSPOptimization,
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    JobRunByUser = "RM_TS_RunSchedule",
                    Parameters = SerializerHelper.SerializeByDataContractSerializer(new RMDiscoverAOSPOptimizationJobInfo() {
                        o365Info = new RMDiscoveryAOSPTenantInfo()
                        {
                            UniqueId = o365TenantId
                        },
                        settingInfo = new RMDiscoveryAOSPOptimizationSettingsInfo()
                        {
                            Setting = SerializerHelper.SerializeByDataContractSerializer(setting)
                        }
                    }),
                };
                _logger.Info($"Add Queue for DiscoveryAOSPOptimization job. TenantId:{o365TenantId}, SettingId:{settingId}, JobType:{JobType.DiscoveryAOSPOptimization}");
                _jobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while send optimization calculate job. Error: {e}");
            }
        }
        public string RealRunOptimizationCalculateJob(string parameters)
        {
            try
            {
                var parameterList = JsonConvert.DeserializeObject<List<string>>(parameters);
                var jobId = _jobMonitorService.CreateJob(JobType.DiscoveryAOSPOptimizationCalculate, "RM_TS_RunSchedule");
                _jobQueueService.HandleMessage(new JobQueueMessage()
                {
                    JobId = jobId,
                    JobType = JobType.DiscoveryAOSPOptimizationCalculate,
                    CommandLine = $"{JobType.DiscoveryAOSPOptimizationCalculate} {jobId} {parameterList[0]} {parameterList[1]}",
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
