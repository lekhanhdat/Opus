﻿/********************************************************************
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
using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit.Async;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.DalServices;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.Discovery.DiscoveryPlan;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Office365;
using AvePoint.RA.Contract.Discovery.Model.PlanProfile;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Setting;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Impl.PlanProfile;
using AvePoint.RA.DB.Dao.Discovery.PlanProfile;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Model.Discovery.Plan;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.Service.Services.Discovery.FileSystem.Audit;
using AvePoint.RA.Service.Services.Discovery.PlanProfile.Audit;
using AvePoint.RA.Service.Services.Discovery.PlanProfile.Common;
using AvePoint.RA.SharePoint.Common;
using AvePoint.Wrapper.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ScheduleType = AvePoint.RA.Contract.Schedule.ScheduleType;

namespace AvePoint.RA.Service.Services.Discovery.Plan
{
    [AsyncAudit]
    public class RMDiscoveryPlanProfileService : IRMDiscoveryPlanProfileService 
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryPlanProfileService));
        private readonly IRMDiscoveryPlanProfileDao _planProfileDao = PlatformWindsorManager.GetService<IRMDiscoveryPlanProfileDao>();
        private readonly IScheduleService _scheduleService = PlatformWindsorManager.GetService<IScheduleService>();
        private readonly IStorageDeviceService _storageDeviceService = PlatformWindsorManager.GetService<IStorageDeviceService>();
        private readonly IGeneralSettingService _generalSettingService = PlatformWindsorManager.GetService<IGeneralSettingService>();
        private IJobQueueService mJobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();
        private IJobMonitorService RMJobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();
        private IRMSubJobDao SubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();
        private IDalService DalService => PlatformWindsorManager.GetService<IDalService>();
        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private IRMDiscoveryPlanDalJobDao RMDiscoveryPlanDalJobDao = new RMDiscoveryPlanDalJobDao();
        private IRMDiscoveryPlanDalJobConfiguration RMDiscoveryPlanDalJobConfigurationDao = new RMDiscoveryDalJobConfigurationDao();
        private readonly IRMRemoteNodeService _remoteNodeService = PlatformWindsorManager.GetService<IRMRemoteNodeService>();
        private readonly IRMDiscoveryPlanSiteMappingDao _planSiteMappingDao = PlatformWindsorManager.GetService<IRMDiscoveryPlanSiteMappingDao>();
        private readonly IRMRemoteNodeDao _remoteNodeDao = PlatformWindsorManager.GetService<IRMRemoteNodeDao>();
        private readonly IStubSettingService _stubSettingService = PlatformWindsorManager.GetService<IStubSettingService>();
        private readonly IRMKeyValueDao _keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private readonly IJobMonitorDao _jobMonitorDao = PlatformWindsorManager.GetService<IJobMonitorDao>();

        private const int DateTimeCriteriaCategory = 4;
        private static readonly string[] CriteriaDateTimeFormats =
        {
            "yyyy/M/d H:mm",
            "yyyy/MM/dd HH:mm",
            "yyyy/M/d HH:mm",
            "yyyy/MM/dd H:mm",
            "yyyy/M/d H:mm:ss",
            "yyyy/MM/dd HH:mm:ss"
        };

        public async Task<RMDiscoveryPlanProfileInfo> GetByIdAsync(int id)
        {
            try
            {
                var profile = await _planProfileDao.GetByIdAsync(id);
                if (profile == null) return null;

                var gls = await _generalSettingService.GetGeneralSettingAsync();
                var dto = MapToDto(profile, gls);
                var schedule = await _scheduleService.GetScheduleAsync(id.ToString(), ScheduleType.DiscoveryPlanSchedule);
                dto.ScheduleSetting = MapScheduleToDto(schedule);
                return dto;
            }
            catch (Exception ex)
            {
                _logger.Error($"An error occurred while getting Plan Profile by id: {id}. Error: {ex}");
                throw;
            }
        }

        public async Task<RMDiscoveryPlanProfilePageInfo> GetPagedAsync(RMDiscoveryPlanProfilePageRequest request)
        {
            try
            {
                var (totalCount, items) = await _planProfileDao.GetPagedAsync(request);

                var gls = await _generalSettingService.GetGeneralSettingAsync();
                var dtos = items?.Select(item => MapToDto(item, gls)).ToList() ?? new List<RMDiscoveryPlanProfileInfo>();

                var enrichTasks = dtos.Select(async dto =>
                {
                    var scheduleTask = _scheduleService.GetScheduleAsync(dto.Id.ToString(), ScheduleType.DiscoveryPlanSchedule);
                    var mappingCountTask = _planSiteMappingDao.GetTotalMappingSitesAsync(dto.Id);

                    await Task.WhenAll(scheduleTask, mappingCountTask);

                    dto.ScheduleSetting = MapScheduleToDto(scheduleTask.Result);
                    dto.TotalMappingSites = mappingCountTask.Result;
                });

                await Task.WhenAll(enrichTasks);

                return new RMDiscoveryPlanProfilePageInfo
                {
                    TotalCount = totalCount,
                    PageIndex = request.PageIndex,
                    PageSize = request.PageSize,
                    Items = dtos
                };
            }
            catch (Exception ex)
            {
                _logger.Error($"An error occurred while getting paged Plan Profiles. Error: {ex}");
                throw;
            }
        }

        public async Task<List<string>> GetAllSelectedSiteByProfileIdAsync(int profileId)
        {
            try
            {
                if (profileId <= 0) return new List<string>();

                return await _planSiteMappingDao.GetNodeIdsByProfileId(profileId);
            }
            catch (Exception ex)
            {
                _logger.Error($"An error occurred while getting all selected sites for profile: {profileId}. Error: {ex}");
                throw;
            }
        }

        [AsyncAudit(Module = AuditModule.Discovery, Category = AuditCategory.DiscoveryPlanProfile, Action = AuditAction.CreateDiscoveryPlanProfile, IAsyncBeforeHandler = typeof(RMDiscoveryPlanProfileServiceBeforeAuditHandler), IAsyncAfterHandler = typeof(RMDiscoveryPlanProfileServiceAfterAuditHandler))]
        public async Task<int> CreateAsync(RMDiscoveryPlanProfileInfo profileInfo)
        {
            try
            {
                if (profileInfo == null) throw new ArgumentNullException(nameof(profileInfo));
                await RMDiscoveryDBManager.InitPlanTablesAsync();
                ValidatePlanProfileInfo(profileInfo);

                ValidateStorageExists(profileInfo.StorageLocationId, profileInfo.StorageName);

                var entity = await MapToEntityAsync(profileInfo);
                int profileId = await _planProfileDao.InsertAsync(entity);

                if (profileInfo.SiteMappings != null && profileInfo.SiteMappings.Any())
                {
                    await _planSiteMappingDao.UpdateMappingsAsync(profileId, profileInfo.SiteMappings);
                }

                if (profileInfo.ScheduleSetting != null && !profileInfo.ScheduleSetting.NoSchedule)
                {
                    try
                    {
                        var scheduleInfo = MapToScheduleInfo(profileInfo.ScheduleSetting, profileId);

                        if (string.IsNullOrEmpty(profileInfo.ScheduleSetting.EndTime))
                        {
                            scheduleInfo.EndTime = scheduleInfo.StartTime;
                        }

                        if (string.IsNullOrEmpty(scheduleInfo.EndTime))
                        {
                            scheduleInfo.EndTime = scheduleInfo.StartTime;
                        }

                        string scheduleId = await _scheduleService.CreateScheduleWithoutAuditAsync(scheduleInfo);
                        _logger.Info($"Schedule created with id: {scheduleId} for Plan Profile id: {profileId}.");
                    }
                    catch (Exception scheduleEx)
                    {
                        _logger.Error($"Schedule creation failed for Plan Profile id: {profileId}. Initiating rollback. Error: {scheduleEx}");
                        try
                        {
                            await _planProfileDao.DeleteAsync(profileId);
                            _logger.Info($"Rollback successful: Plan Profile id: {profileId} has been deleted.");
                        }
                        catch (Exception deleteEx)
                        {
                            _logger.Error($"Rollback failed: Could not delete orphaned Plan Profile id: {profileId}. Error: {deleteEx}");
                        }

                        throw new InvalidOperationException($"Schedule creation failed. Plan Profile creation has been rolled back. Error: {scheduleEx.Message}", scheduleEx);
                    }
                }

                return profileId;
            }
            catch (Exception ex)
            {
                _logger.Error($"An error occurred while creating Plan Profile. Error: {ex}");
                throw;
            }
        }

        [AsyncAudit(Module = AuditModule.Discovery, Category = AuditCategory.DiscoveryPlanProfile, Action = AuditAction.UpdateDiscoveryPlanProfile, IAsyncBeforeHandler = typeof(RMDiscoveryPlanProfileServiceBeforeAuditHandler), IAsyncAfterHandler = typeof(RMDiscoveryPlanProfileServiceAfterAuditHandler))]
        public async Task<bool> UpdateAsync(RMDiscoveryPlanProfileInfo profileInfo)
        {
            try
            {
                if (profileInfo == null) throw new ArgumentNullException(nameof(profileInfo));

                ValidatePlanProfileInfo(profileInfo);

                ValidateStorageExists(profileInfo.StorageLocationId, profileInfo.StorageName);

                var entity = await MapToEntityAsync(profileInfo);
                bool profileUpdated = await _planProfileDao.UpdateAsync(entity);
                if (!profileUpdated) return false;

                if (profileInfo.SiteMappings != null && profileInfo.SiteMappings.Any())
                {
                    await _planSiteMappingDao.UpdateMappingsAsync(profileInfo.Id, profileInfo.SiteMappings);
                }

                var existingSchedule = await _scheduleService.GetScheduleAsync(profileInfo.Id.ToString(), ScheduleType.DiscoveryPlanSchedule);

                if (profileInfo.ScheduleSetting != null && !profileInfo.ScheduleSetting.NoSchedule)
                {
                    var scheduleInfo = MapToScheduleInfo(profileInfo.ScheduleSetting, profileInfo.Id);
                    if (string.IsNullOrEmpty(profileInfo.ScheduleSetting.EndTime))
                    {
                        scheduleInfo.EndTime = scheduleInfo.StartTime;
                    }
                    if (existingSchedule != null)
                    {
                        scheduleInfo.Id = existingSchedule.Id;
                        await _scheduleService.UpdateScheduleWithoutAuditAsync(scheduleInfo);
                    }
                    else
                    {
                        await _scheduleService.CreateScheduleWithoutAuditAsync(scheduleInfo);
                    }
                }
                else if (existingSchedule != null)
                {
                    _scheduleService.DeleteScheduleWithoutAudit(existingSchedule.Id);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"An error occurred while updating Plan Profile with id: {profileInfo?.Id}. Error: {ex}");
                throw;
            }
        }

        [AsyncAudit(Module = AuditModule.Discovery, Category = AuditCategory.DiscoveryPlanProfile, Action = AuditAction.DeleteDiscoveryPlanProfile, IAsyncBeforeHandler = typeof(RMDiscoveryPlanProfileServiceBeforeAuditHandler), IAsyncAfterHandler = typeof(RMDiscoveryPlanProfileServiceAfterAuditHandler))]
        public async Task<bool> DeleteAsync(List<int> ids)
        {
            if (ids == null || !ids.Any()) return false;

            try
            {
                await _planSiteMappingDao.DeleteByPlanProfileIdsAsync(ids);

                foreach (var id in ids)
                {
                    var schedule = await _scheduleService.GetScheduleAsync(id.ToString(), ScheduleType.DiscoveryPlanSchedule);
                    if (schedule != null)
                    {
                        _scheduleService.DeleteScheduleWithoutAudit(schedule.Id);
                    }
                }

                return await _planProfileDao.DeleteByIdsAsync(ids);
            }
            catch (Exception ex)
            {
                _logger.Error($"An error occurred while deleting Plan Profiles with ids: {string.Join(",", ids)}. Error: {ex}");
                throw;
            }
        }

        public Task<RMRemoteSiteCollectionPageInfo> GetAllSiteCollectionNodesAsync(RMRemoteSiteCollectionPageRequest request)
        {
            try
            {
                if (request == null || request.PageIndex < 1 || request.PageSize < 1)
                {
                    throw new ArgumentException("Invalid pagination parameters.");
                }

                var pagedSiteCollections = _remoteNodeService.GetAllRemoteSiteCollections(request.PageIndex, request.PageSize, request.Key);

                var pageResult = new RMRemoteSiteCollectionPageInfo
                {
                    TotalCount = pagedSiteCollections.TotalCount,
                    PageIndex = pagedSiteCollections.PageIndex,
                    PageSize = pagedSiteCollections.PageSize,
                    Items = pagedSiteCollections.Items
                };
                return Task.FromResult(pageResult);
            }
            catch (Exception e)
            {
                _logger.Error("An error occurred while get authorised remote site collections by user. Error: {0}", e);
                throw;
            }
        }
        public async Task<RMRemoteSiteCollectionPageInfo> GetMappedSitesPagedAsync(RMRemoteSiteCollectionPageRequest request)
        {
            try
            {
                if (request == null) throw new ArgumentNullException(nameof(request));

                List<string> selectedNodeIds = await _planSiteMappingDao.GetNodeIdsByPlanProfileIdAsync(request.PlanProfileId);

                int totalCount;
                var items = _remoteNodeDao.GetMappedRemoteSitesPaged(
                    request.PageIndex,
                    request.PageSize,
                    request.Key,
                    selectedNodeIds,
                    out totalCount);

                return new RMRemoteSiteCollectionPageInfo
                {
                    TotalCount = totalCount,
                    PageIndex = request.PageIndex,
                    PageSize = request.PageSize,
                    Items = items
                };
            }
            catch (Exception ex)
            {
                _logger.Error($"Error in GetMappedSitesPagedAsync for PlanProfileId: {request?.PlanProfileId}. Error: {ex}");
                throw;
            }
        }

        public async Task<bool> EnableAIMessageAsync()
        {
            try
            {
                var dalJob = _jobMonitorDao.GetJobsByJobType(Contract.JobMonitor.JobType.DiscoveryDalJob);
                if (dalJob.Any(j => j.Status == (int)JobStatus.Finished || j.Status == (int)JobStatus.FinishWithException))
                {
                    _logger.Info($"There are completed jobs of type DiscoveryDalJob. JobIds: {string.Join(", ", dalJob.Select(j => j.Id))}. Returning true.");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error in EnableAIMessageAsync. Error: {ex}");
                throw;
            }
        }

        #region Validation

        private void ValidateStorageExists(string storageLocationId, string storageName)
        {
            if (string.IsNullOrWhiteSpace(storageLocationId))
                return;

            var storage = _storageDeviceService.GetStorageDeviceById(storageLocationId);
            if (storage == null)
                throw new InvalidOperationException($"Storage '{storageName}' with ID '{storageLocationId}' does not exist.");
        }

        private static void ValidatePlanProfileInfo(RMDiscoveryPlanProfileInfo profileInfo)
        {
            if (string.IsNullOrWhiteSpace(profileInfo.Name))
                throw new ArgumentException("Plan Profile name must not be null or empty.", nameof(profileInfo.Name));

            if (string.IsNullOrWhiteSpace(profileInfo.StorageLocationId))
                throw new ArgumentException("Storage Location ID must not be null or empty.", nameof(profileInfo.StorageLocationId));

            if (string.IsNullOrWhiteSpace(profileInfo.StorageName))
                throw new ArgumentException("Storage Name must not be null or empty.", nameof(profileInfo.StorageName));

            if (!Enum.IsDefined(typeof(RMDiscoveryPlanAction), profileInfo.Action))
                throw new ArgumentException($"Invalid Action value: {profileInfo.Action}.", nameof(profileInfo.Action));

            if (!IsValidActionOptions(profileInfo.ActionOptions))
                throw new ArgumentException($"Invalid ActionOptions value: {profileInfo.ActionOptions}.", nameof(profileInfo.ActionOptions));
        }

        private static bool IsValidActionOptions(RMDiscoveryPlanActionOptions value)
        {
            if (value == RMDiscoveryPlanActionOptions.None) return true;

            var allFlags = (RMDiscoveryPlanActionOptions[])Enum.GetValues(typeof(RMDiscoveryPlanActionOptions));
            int validMask = allFlags.Aggregate(0, (mask, flag) => mask | (int)flag);

            return ((int)value & ~validMask) == 0;
        }

        #endregion

        public async Task<RMDiscoveryTriggerDalJob> GetConfigurationInfoAsync()
        {
            return await RMDiscoveryPlanDalJobConfigurationDao.GetAsync<RMDiscoveryTriggerDalJob>(Contract.Discovery.Model.RMDiscoveryConfigurationType.Office365NewlyScope);
        }
        public async Task<RAReturnMessage> TriggerDalJob(RMDiscoveryTriggerDalJob settingDto, JobRunBy jobRunBy)
        {
            _logger.Info($"Triggering DAL job with settings: {JsonConvert.SerializeObject(settingDto)} and JobRunBy: {jobRunBy}");
            string id = string.Empty;
            RAReturnMessage msg = new RAReturnMessage();
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                //var loginName = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.DiscoveryDalJob,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                    Parameters = settingDto == null ? null : SerializerHelper.SerializeByDataContractSerializer(settingDto)
                };

                id = mJobQueueService.AddToDBJobQueue(jqDto);
                if (string.IsNullOrEmpty(id))
                {
                    msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
            }
            catch (Exception ex)
            {
                _logger.Error("error occurred while Apply class code,ERROR:{0}", ex.ToString());
            }

            return msg;
        }

        [AsyncAudit(Module = AuditModule.Discovery, Category = AuditCategory.DiscoveryPlanProfile, Action = AuditAction.SaveDiscoveryPlanDalJobConfiguration, IAsyncBeforeHandler = typeof(RMDiscoveryPlanProfileServiceBeforeAuditHandler), IAsyncAfterHandler = typeof(RMDiscoveryPlanProfileServiceAfterAuditHandler))]
        public async Task<string> RealRunTriggerDalJob(JobRunBy jobRunBy, string jobRunByUser, string param)
        {
            string jobId = string.Empty;
            try
            {
                jobId = RMJobMonitorService.CreateJob(JobType.DiscoveryDalJob, jobRunByUser);
                var jobRunnings = RMJobMonitorService.GetRunningJobs(new List<Contract.JobMonitor.JobType> { Contract.JobMonitor.JobType.DiscoveryDalJob });
                if (jobRunnings.Any() && jobRunnings.Where(j => j.Status == (int)JobStatus.InProgress).Any())
                {
                    _logger.Info($"There are already running jobs of type DiscoveryDalJob. JobId: {jobId}. Skipping triggering new job.");
                    RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_SS_JobSkip");
                    return jobId;
                }
                await RMDiscoveryDBManager.InitPlanTablesAsync();
                var scope = SerializerHelper.DeserializeByDataContractSerializer<RMDiscoveryTriggerDalJob>(param);
                await RMDiscoveryPlanDalJobConfigurationDao.AddOrUpdateAsync(new RMDiscoveryDalJobConfiguration()
                {
                    ConfigurationType = RMDiscoveryConfigurationType.Office365NewlyScope,
                    ValueJson = JsonConvert.SerializeObject(scope),
                    CreateTime = DateTime.UtcNow.Ticks,
                    ModifiedTime = DateTime.UtcNow.Ticks
                });
                var (suceed, items) = await GetWillTriggerJobsAsync(scope);
                if (!suceed)
                {
                    _logger.Error("Failed to get site to trigger job");
                    RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, "Faild to get site to trigger job");
                    return jobId;
                }
                if(!items.Any())
                {
                    _logger.Info("No site to trigger job");
                    RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "No site to trigger job");
                    return jobId;
                }
                int subJobCount = items.Count;
                SubJobDao.UpdateSubJobCount(jobId, subJobCount);
                List<RMRemoteNode> tempList = new List<RMRemoteNode>();
                int currentSubjobIndex = 0;
                RMJobMonitorService.UpdateJob(jobId, 1, (int)JobStatus.InProgress, 0);
                var registeredTenants = new HashSet<Guid>();
                foreach (var (o365TenantId, container, sites) in items)
                {
                    string tenantId = o365TenantId.ToString();
                    if (!registeredTenants.Contains(o365TenantId))
                    {
                        await DalService.InitializeTenantAsync(TenantLocalValue.LogonGroupId, tenantId);
                        await DalService.RegisterConnectorAsync(TenantLocalValue.LogonGroupId, tenantId);
                        await DalService.RegisterConnectorDefinitionAsync();
                        registeredTenants.Add(o365TenantId);
                    }
                    var dalJobId = await DalService.TriggerJobAsync(tenantId, new List<Cloud.Sdk.Data.Dal.ConnectorType> { Cloud.Sdk.Data.Dal.ConnectorType.MicrosoftItemBatchConnector }, sites.Select(s => s.Url).ToList());
                    
                    await DalService.IngestContainerIdAsync(tenantId, container.Id, sites.Select(site => site.ObjectId).ToList());
                    string subJobId = await CreateSubJob(jobId, dalJobId, container.Id, currentSubjobIndex, sites.Count, sites);
                    currentSubjobIndex++;
                }
                return jobId;
            }
            catch (Exception ex)
            {
                _logger.Error("Error occurred while RealRunTriggerDalJob. JobId:{0}, ERROR:{1}", jobId, ex.ToString());

                if (!string.IsNullOrEmpty(jobId))
                {
                    try
                    {
                        RMJobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, ex.Message);
                    }
                    catch (Exception updateEx)
                    {
                        _logger.Error(
                            "Failed to update job status to Failed. JobId:{0}, ERROR:{1}",
                            jobId,
                            updateEx.ToString());
                    }
                }

                return string.Empty;
            }

        }

        private async Task<string> CreateSubJob(string jobId, Guid dalJobId, string containerId, int currentSubjobIndex, int siteCount, List<RMRemoteNode> sites)
        {
            try
            {
                Guid subJobId = Guid.NewGuid();
                var subJob = new RMDiscoveryPlanDalJob()
                {
                    Id = subJobId,
                    MainJobId = jobId,
                    StartTime = DateTime.UtcNow.Ticks,
                    SitesCount = siteCount,
                    DalJobId = dalJobId,
                    Status = RMDalJobStatus.Pending,
                    Extension = SerializerHelper.SerializeByDataContractSerializer(sites.Select(s => s.Url).ToList())
                };
                await RMDiscoveryPlanDalJobDao.AddOrUpdateJobAsync(subJob);
                _logger.Info($"Create sub job [{subJobId}] for dal job [{dalJobId}] with container [{containerId}] and site count [{siteCount}].");
                return subJobId.ToString();
            }
            catch (Exception ex)
            {
                _logger.Error($"An error occurred while creating sub job for dal job [{dalJobId}] with container [{containerId}] and site count [{siteCount}]. Error: {ex}");
                return string.Empty;
            }
        }

        private async Task<(bool succeed, List<(Guid o365TenantId, RMRemoteNode container, List<RMRemoteNode> sites)> items)> GetWillTriggerJobsAsync(RMDiscoveryTriggerDalJob scopeInfo)
        {
            try
            {
                var res = new List<(Guid o365TenantId, RMRemoteNode container, List<RMRemoteNode> sites)>();

                var willTriggerContainers = await GetWillTriggerJobContainersAsync(scopeInfo);
                _logger.Info($"Will trigger jobs count: [{willTriggerContainers.Count}].");
                foreach (var willTriggerContainer in willTriggerContainers)
                {
                    var sites = await _planProfileDao.GetOpusSitesAsync(new Guid(willTriggerContainer.Id)).ToListAsync();
                    if (!sites.Any())
                    {
                        _logger.Info($"Container [{willTriggerContainer.Id}] has no sites that need to trigger  job.");
                        continue;
                    }
                    var tenantSitesMapping = sites.GroupBy(item => item.TenantId).ToDictionary(item => item.Key, item => item.ToList());
                    foreach (var tenantSites in tenantSitesMapping)
                    {
                        tenantSites.Value.ForEach(item =>
                        {
                            try
                            {
                                if (item.NodeLevel == (int)NodeLevel.O365GroupSites)
                                {
                                    var remoteNode = RABrowserClient.GetRemoteSiteCollectionWithBposByUrl(item.Url);
                                    var factory = MultiAppUtil.CreateAveObjectModelFactory(item.Url, PoolUserUtil.GetAveBPOSAccountInfo(remoteNode.Bpos, item.Url), AveContextKind.ClientObjectModel);
                                    using var site = factory.CreateSite();
                                    item.ObjectId = site.ID.ToString();
                                }
                            }
                            catch (AveSkipLockSiteException e)
                            {
                                _logger.Error($"An error occurred while accessing the site, The site is locked. URL:[{item.Url}] SiteState:[{e.SiteState}]");
                                item.ObjectId = Guid.Empty.ToString();
                            }
                            catch (Exception ex)
                            {
                                _logger.Error($"An error occurred while accessing the site, URL:[{item.Url}], message: {ex}");
                                item.ObjectId = Guid.Empty.ToString();
                            }
                        });

                        var targetSites = tenantSites.Value.Where(item => item.NodeLevel != (int)NodeLevel.O365GroupSites
                                                                    || (item.NodeLevel == (int)NodeLevel.O365GroupSites && !item.ObjectId.Equals(Guid.Empty.ToString()))).ToList();
                        res.Add((new Guid(tenantSites.Key), willTriggerContainer, targetSites));
                    }
                }

                _logger.Info($"Successful allocate will trigger jobs: [{res.Count}].");
                return (true, res);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while get will trigger jobs. Error: {e}");
                return (false, []);
            }
        }

        private async Task<List<RMRemoteNode>> GetWillTriggerJobContainersAsync(RMDiscoveryTriggerDalJob scopeInfo)
        {
            if (scopeInfo.ScopeType == RMDiscoveryOffice365ScopeType.DataSource)
            {
                return await _planProfileDao.GetOpusContainersAsync([.. scopeInfo.ContentSources]);
            }

            return await _planProfileDao.GetOpusContainersAsync(scopeInfo.SpecifyContainerIds);
        }


        #region Mapping Methods

        private RMDiscoveryPlanProfileInfo MapToDto(RMDiscoveryPlanProfile entity, GeneralSettingModel gls)
        {
            var storageDevice = string.IsNullOrWhiteSpace(entity.StorageLocationId) ? null : _storageDeviceService.GetStorageDeviceById(entity.StorageLocationId);

            return new RMDiscoveryPlanProfileInfo
            {
                Id = entity.Id,
                Name = entity.Name,
                Action = entity.Action,
                ActionOptions = entity.ActionOptions,
                PreviousVersion = entity.PreviousVersion,
                CriteriaInfoes = DeserializeCriteria(entity.Rules, gls),
                StorageLocationId = entity.StorageLocationId,
                StorageName = storageDevice?.Name ?? string.Empty,
                StubSetting = !string.IsNullOrEmpty(entity.StubSettingId) ? _stubSettingService.GetStubSettingById(entity.StubSettingId) : null
            };
        }

        private List<RMDiscoveryRuleCriteriaInfo> DeserializeCriteria(string rules, GeneralSettingModel gls)
        {
            if (string.IsNullOrEmpty(rules)) return new List<RMDiscoveryRuleCriteriaInfo>();

            try
            {
                var criteriaInfoes = JsonConvert.DeserializeObject<List<RMDiscoveryRuleCriteriaInfo>>(rules)
                       ?? new List<RMDiscoveryRuleCriteriaInfo>();

                foreach (var criteria in criteriaInfoes)
                {
                    if (criteria.ConditionInfo == null || (int)criteria.ConditionInfo.Category != DateTimeCriteriaCategory)
                        continue;

                    var value = criteria.ConditionInfo.Value;
                    // Preserve existing JSON-based relative date rules (e.g. {"unit":"1","unitType":1})
                    if (string.IsNullOrWhiteSpace(value) || value.TrimStart().StartsWith("{"))
                        continue;

                    if (long.TryParse(value, out long ticks))
                    {
                        var timeModel = _generalSettingService.ConvertTiksToDateTime(gls, ticks, true);
                        criteria.ConditionInfo.Value = timeModel?.SimplifyFormatTime ?? value;
                    }
                }

                return criteriaInfoes;
            }
            catch (JsonException ex)
            {
                _logger.Error($"Failed to deserialize Plan Profile criteria rules. Error: {ex}");
                return new List<RMDiscoveryRuleCriteriaInfo>();
            }
        }

        private async Task<RMDiscoveryPlanProfile> MapToEntityAsync(RMDiscoveryPlanProfileInfo dto)
        {
            bool isKeepVersionArchive = (dto.ActionOptions & RMDiscoveryPlanActionOptions.KeepCurrentAndSpecifiedArchiveRest) == RMDiscoveryPlanActionOptions.KeepCurrentAndSpecifiedArchiveRest;
            bool isKeepVersionDestroy = (dto.ActionOptions & RMDiscoveryPlanActionOptions.KeepCurrentAndPrevious) == RMDiscoveryPlanActionOptions.KeepCurrentAndPrevious;

            int safePreviousVersion = (isKeepVersionArchive || isKeepVersionDestroy) ? dto.PreviousVersion : 0;

            return new RMDiscoveryPlanProfile
            {
                Id = dto.Id,
                Name = dto.Name,
                Action = dto.Action,
                ActionOptions = dto.ActionOptions,
                PreviousVersion = safePreviousVersion,
                StubSettingId = dto.StubSetting?.Id ?? string.Empty,
                Rules = dto.CriteriaInfoes != null && dto.CriteriaInfoes.Count > 0 ? JsonConvert.SerializeObject(await ConvertCriteriaDatesToTicksAsync(dto.CriteriaInfoes)) : string.Empty,
                StorageLocationId = string.IsNullOrWhiteSpace(dto.StorageLocationId) ? string.Empty : dto.StorageLocationId,
                Extension1 = string.Empty,
                Extension2 = string.Empty
            };
        }

        private async Task<List<RMDiscoveryRuleCriteriaInfo>> ConvertCriteriaDatesToTicksAsync(List<RMDiscoveryRuleCriteriaInfo> criteriaInfoes)
        {
            var gls = await _generalSettingService.GetGeneralSettingAsync();

            foreach (var criteria in criteriaInfoes)
            {
                if (criteria.ConditionInfo == null || (int)criteria.ConditionInfo.Category != DateTimeCriteriaCategory)
                    continue;

                var value = criteria.ConditionInfo.Value;
                if (string.IsNullOrWhiteSpace(value))
                    continue;

                if (value.TrimStart().StartsWith("{"))
                    continue;

                if (long.TryParse(value, out _))
                    continue;

                if (DateTime.TryParseExact(
                        value,
                        CriteriaDateTimeFormats,
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None,
                        out var localDateTime))
                {
                    try
                    {
                        var timeZone = GeneralSettingConfig.FindSystemTimeZoneById(gls.TimeZoneId);
                        var utcDateTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified), timeZone);
                        criteria.ConditionInfo.Value = utcDateTime.Ticks.ToString();
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn($"Failed to convert Date/Time criteria value '{value}' to UTC Ticks. Keeping original value. Error: {ex.Message}");
                    }
                }
            }

            return criteriaInfoes;
        }

        private static ScheduleInfo MapToScheduleInfo(RMDiscoveryPlanScheduleInfo dto, int planId)
        {
            return new ScheduleInfo
            {
                Id = !string.IsNullOrEmpty(dto.Id) ? dto.Id : Guid.NewGuid().ToString(),
                ProfileId = planId.ToString(),
                JobCategory = ScheduleType.DiscoveryPlanSchedule,
                NoSchedule = dto.NoSchedule,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                NextTime = dto.NextTime,
                TimeZoneId = dto.TimeZoneId,
                IsDaylightSaving = dto.IsDaylightSaving,
                EndType = dto.EndType,
                OccurrencesTotal = dto.OccurrencesTotal,
                Occurrences = dto.Occurrences,
                Interval = dto.Interval,
                IntervalType = dto.IntervalType,
                DayOfMonth = dto.DayOfMonth,
                WeekType = dto.WeekType
            };
        }

        private static RMDiscoveryPlanScheduleInfo MapScheduleToDto(ScheduleInfo schedule)
        {
            if (schedule == null) return null;

            return new RMDiscoveryPlanScheduleInfo
            {
                Id = schedule.Id,
                NoSchedule = schedule.NoSchedule,
                StartTime = schedule.StartTime,
                EndTime = schedule.EndTime,
                NextTime = schedule.NextTime,
                TimeZoneId = schedule.TimeZoneId,
                IsDaylightSaving = schedule.IsDaylightSaving,
                EndType = schedule.EndType,
                OccurrencesTotal = schedule.OccurrencesTotal,
                Occurrences = schedule.Occurrences,
                Interval = schedule.Interval,
                IntervalType = schedule.IntervalType,
                DayOfMonth = schedule.DayOfMonth,
                WeekType = schedule.WeekType
            };
        }

        #endregion

        public async Task<bool> GetPlanChatDisplayConfiguration()
        {
            bool result = false;
            try
            {
                string res = await _keyValueDao.GetValueByKeyAsync(RMKeyValuesConstants.DiscoveryShowPlanChat);
                if (string.IsNullOrEmpty(res))
                {
                    _logger.Warn($"{RMKeyValuesConstants.DiscoveryShowPlanChat} is not configured.");
                    return result;
                }
                
                if (bool.TryParse(res.Trim(), out result))
                {
                    _logger.Info($"The value of {RMKeyValuesConstants.DiscoveryShowPlanChat} is {res}");
                    return result;
                }
                else
                {
                    _logger.Error($"The value of {RMKeyValuesConstants.DiscoveryShowPlanChat} is invalid.");
                }
            }
            catch (Exception ex) {
                _logger.Error($"An error occurred while get the value from table [RMKeyValues] by key {RMKeyValuesConstants.DiscoveryShowPlanChat}.  Error: {ex}");
            }
            return result;
        }


    }
}
