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
using AvePoint.Hybrid.Contract.Object;
using AvePoint.RA.Cache.Services;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.Certficate;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Extension;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Account;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.SignalR;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Task;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility.Common;
using AvePoint.RA.RACommonUtility.JobControl.JPMC;
using AvePoint.RA.RACommonUtility.MultiGeo;
using AvePoint.RA.Service.Services.ControlPanel.AuditHandler;
using AvePoint.RA.Service.Services.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RAExportCommon;
using System;
using System.Data.Entity.Validation;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Util.Security;
using static AvePoint.Common.AgentConstants;
using AvePoint.RA.Common.Security;

namespace AvePoint.RA.Service.Services.ControlPanel
{
    [Audit]
    public class AgentMgmtService : RMServiceBase, IAgentMgmtService
    {
        private RALogger logger = RALogger.GetInstance(typeof(AgentMgmtService));
        public IKeyValueService KeyValueService => PlatformWindsorManager.GetService<IKeyValueService>();
        public IRMAgentDao RMAgentDao => PlatformWindsorManager.GetService<IRMAgentDao>();
        public ICertificateService CertificateService => PlatformWindsorManager.GetService<ICertificateService>();

        public ITenantInfoDao TenantInfoDao => PlatformWindsorManager.GetService<ITenantInfoDao>();

        public IFSConnectionGroupDao FSConnectionGroupDao => PlatformWindsorManager.GetService<IFSConnectionGroupDao>();

        public IFSConnectionGroupWithAgentMemebershipDao FSConnectionGroupWithAgentMemebership => PlatformWindsorManager.GetService<IFSConnectionGroupWithAgentMemebershipDao>();

        public IMultiGeoDataCenterService MultiGeoDataCenterService => PlatformWindsorManager.GetService<IMultiGeoDataCenterService>();
        public IMultiGeoSettingService MultiGeoSettingService => PlatformWindsorManager.GetService<IMultiGeoSettingService>();
        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();

        private IHybridBrowserService HybridBrowserService => PlatformWindsorManager.GetService<IHybridBrowserService>();
        private IRMSubJobDao RMSubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();
        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private static readonly TimeSpan AgentStatusFlagsCacheTtl = TimeSpan.FromMinutes(5);

        private string AgentStatusFlagsCacheKey => $"AgentStatusFlags_{TenantLocalValue.LogonGroupId}";
        private const string CacheFieldHasMismatched = "HasMismatchedAgent";
        private const string CacheFieldHasMinorVersionMismatched = "HasMinorVersionMismatchedAgent";

        private List<JobType> AgentJobTypes = new List<JobType>
        {
            JobType.FSArchiverRestore,
            JobType.FSDataSynchronization,
            JobType.FSDataSynchronizationSchedule,
            JobType.FSDisposal,
            JobType.FSDisposalSchedule,
            JobType.FSDisposalByClassCode,
            JobType.FSRetain,
            JobType.FSRetainSimulate,
            JobType.FSCreateAndDestroyedFileReport,
            JobType.FSItemsFilesDueDisposal,
            JobType.DiscoveryAnalysisFileSystemV1,
            JobType.DiscoveryFileSystemV1,
            JobType.SPOnPremUniqueIDSettingFullSchedule,
            JobType.SPOnPremUniqueIDSettingIncrementalSchedule,
            JobType.SPOnPremApplySetting,
            JobType.SPOnPremDataSync,
            JobType.SPOnPremDataSyncSchedule,
            JobType.SPOnPremTermSynchronization,
            JobType.SPOnPremTermSynchronizationSchedule,
            JobType.SPOnPremEnforceRuleAction,
            JobType.SPOnPremEnforceRuleActionSchedule,
            JobType.SPOnPremItemsFilesDueDisposal,
            JobType.SPOnPremCreateAndDestroyedFileReport,
            JobType.SPOnPremScanLocalNodes,
        };

        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.AgentManagement, Action = AuditAction.DownloadCertficate, BeforeHandler = typeof(AppManagementBeforeAuditHandler), AfterHandler = typeof(AppManagementAfterAuditHandler))]
        public RMCertificateDto DownloadCert(Guid certId)
        {
            return CertificateService.Get(certId);
        }

        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.AgentManagement, Action = AuditAction.AddClientId, BeforeHandler = typeof(AppManagementBeforeAuditHandler), AfterHandler = typeof(AppManagementAfterAuditHandler))]
        public Task<bool> SaveClientIdAsync(string clientId)
        {
            return KeyValueService.SaveAsync(new RMNameValueDto
            {
                Name = KeyNameCollection.AppManagementClientId,
                Value = clientId,
                Type = RMNameValueType.AppManagementClientId
            });

        }

        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.AgentManagement, Action = AuditAction.DownloadAgentConfigFile, BeforeHandler = typeof(AppManagementBeforeAuditHandler), AfterHandler = typeof(AppManagementAfterAuditHandler))]
        public AgentConfigurtion DownloadConfig(RMAgentDto agent)
        {
            var clientId = KeyValueService.Get(KeyNameCollection.AppManagementClientId, RMNameValueType.AppManagementClientId)?.Value;


            var cert = CertificateService.Get(agent.CertificateId);
            var conf = new AgentConfigurtion
            {
                Id = agent.Id.ToString(),
                CustomerId = TenantLocalValue.LogonGroupId,
                ClientId = clientId,
                //RecordsApiUrl = RMGlobalConfiguration.AppConfig[RMAppSettingKey.RECO_API_URL],
                //IdentityServiceUrl = RMGlobalConfiguration.AppConfig[RMAppSettingKey.PUBLIC_IDENTITY_SERVICE_URL],
                //SiginalRServiceUrl = RMGlobalConfiguration.AppConfig[RMAppSettingKey.PublicSignalRServerURL],
                CertificateContent = Convert.ToBase64String(cert.BinaryContent),
                CertificatePWD = cert.PWD,
                Version = agent.Version,
                InstallationCode = agent.InstallationCode,
                PackageId = agent.Id.ToString(),
                AuthCode = agent.AuthCode
            };

            return conf;
        }

        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.AgentManagement, Action = AuditAction.RegisterAgent, BeforeHandler = typeof(AppManagementBeforeAuditHandler), AfterHandler = typeof(AppManagementAfterAuditHandler))]
        public bool CreateAgent(RMAgentDto dto)
        {
            try
            {
                var entity = dto.Convert2Entity();
                RMAgentDao.Create(entity);
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while create agent information, agent name : {dto.Name}. error: {e.ToString()}");
                return false;
            }

            return true;
        }

        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.AgentManagement, Action = AuditAction.RegisterAgent, BeforeHandler = typeof(AppManagementBeforeAuditHandler), AfterHandler = typeof(AppManagementAfterAuditHandler))]
        public Guid? CreateAgentAndGetId(RMAgentDto dto)
        {
            try
            {
                var entity = dto.Convert2Entity();
                RMAgentDao.Create(entity);
                return entity.Id;
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while create agent information, agent name : {dto.Name}. error: {e.ToString()}");
                return null;
            }
        }

        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.AgentManagement, Action = AuditAction.RegisterAgent, BeforeHandler = typeof(AppManagementBeforeAuditHandler), AfterHandler = typeof(AppManagementAfterAuditHandler))]
        public async Task<Guid?> CreateReplicaAgentAndGetIdAsync(RMAgentDto dto)
        {
            try
            {
                var entity = dto.Convert2Entity();
                await RMAgentDao.CreateReplicaAgentAsync(entity);
                return entity.Id;
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while create replica agent information, agent name : {dto.Name}. error: {e.ToString()}");
                return null;
            }
        }

        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.AgentManagement, Action = AuditAction.DeleteAgent, BeforeHandler = typeof(AppManagementBeforeAuditHandler), AfterHandler = typeof(AppManagementAfterAuditHandler))]
        public Task<bool> DeleteAsync(Guid id)
        {
            return UpdateStatusAsync(id, Hybrid.Contract.Object.ServiceStatus.Deleted);
        }

        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.AgentManagement, Action = AuditAction.DisableAgent, BeforeHandler = typeof(AppManagementBeforeAuditHandler), AfterHandler = typeof(AppManagementAfterAuditHandler))]
        public Task<bool> DisableAsync(Guid id)
        {
            return UpdateStatusAsync(id, Hybrid.Contract.Object.ServiceStatus.Disabled);
        }

        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.AgentManagement, Action = AuditAction.EnableAgent, BeforeHandler = typeof(AppManagementBeforeAuditHandler), AfterHandler = typeof(AppManagementAfterAuditHandler))]
        public Task<bool> EnableAsync(Guid id)
        {
            return UpdateStatusAsync(id, Hybrid.Contract.Object.ServiceStatus.Active);
        }

        public RMAgentDto Get(Guid id, bool includeAuthCode = false)
        {
            try
            {
                var entity = RMAgentDao.Find(o => o.Id == id);
                var agent = entity?.Convert2Dto(includeAuthCode);
                if (agent != null)
                {
                    PopulateAgentDCDisplayNamesAsync(new List<RMAgentDto> { agent }).GetAwaiter().GetResult();
                }

                return agent;
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while get all agent information. error: {e.ToString()}");
            }

            return null;
        }

        public async Task<IList<RMAgentDto>> GetAllAsync()
        {
            try
            {
                var agents = (await RMAgentDao.FindListAsync(o => o.Status != Hybrid.Contract.Object.ServiceStatus.Deleted)).Select(o => o.Convert2Dto()).ToList();
                await PopulateAgentDCDisplayNamesAsync(agents);
                return agents;
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while get all agent information. error: {e.ToString()}");
            }

            return new List<RMAgentDto>();
        }

        public Task<IList<RMAgentDto>> GetAllActiveAgentsAsync()
        {
            return GetAllAgentsByStatusAsync(new List<ServiceStatus> { ServiceStatus.Active });
            //try
            //{
            //    return RMAgentDao.FindList(o => o.Status == Hybrid.Contract.Object.ServiceStatus.Active).Select(o => o.Convert2Dto()).ToList();
            //}
            //catch (Exception e)
            //{
            //    logger.Error($"An error occurred while get all agent information. error: {e}");
            //}

            //return new List<RMAgentDto>();
        }

        public async Task<IList<RMAgentDto>> GetAvailableAgentsBySourceTypeAsync(string tenantId, SourceType sourceType)
        {
            if (!HasLiense(tenantId, sourceType))
            {
                logger.Warn($"Has no license for source type: {sourceType} with tenantId: {tenantId}");
                return new List<RMAgentDto>();
            }

            var serviceError = sourceType.Map2Errors();
            var agents = (await GetAllAgentsByStatusAsync(new List<ServiceStatus> { ServiceStatus.Active, ServiceStatus.ActiveException }))
                .Where(o => o.SourceType.HasFlag(sourceType) && (o.Errors & serviceError) == ServiceErrors.None);
            return agents.ToList();
        }

        public async Task<IList<RMAgentDto>> GetAvailableAgentsAsync(string tenantId)
        {
            var licensed = new[] { SourceType.FileSystem, SourceType.SharePoint }.Where(st => HasLiense(tenantId, st)).ToList();

            if (!licensed.Any())
            {
                logger.Warn($"Has no license for source type with tenantId: {tenantId}");
                return new List<RMAgentDto>();
            }

            var agents = await GetAllAgentsByStatusAsync(new[] { ServiceStatus.Active, ServiceStatus.ActiveException });

            return licensed
                .SelectMany(st =>
                {
                    var err = st.Map2Errors();
                    return agents.Where(a => a.SourceType.HasFlag(st) && (a.Errors & err) == ServiceErrors.None);
                })
                .DistinctBy(a => a.Id)
                .ToList();
        }


        public async Task<IList<RMAgentDto>> GetAvailableAgentsBySourceTypeAndConnectionGroupIdAsync(string tenantId, SourceType sourceType, Guid connecitonGroupId)
        {
            var hasLicense = HasLiense(tenantId, sourceType);
            if (!hasLicense)
            {
                logger.Warn($"Has no license for source type: {sourceType} with tenantId : {tenantId}");
                return new List<RMAgentDto>();
            }

            var connectionGroup = FSConnectionGroupDao.GetGroupById(connecitonGroupId);
            if (connectionGroup == null)
            {
                logger.Warn($"Connection group [{connecitonGroupId}] was not found when retrieving available agents.");
                return new List<RMAgentDto>();
            }

            var serviceError = sourceType.Map2Errors();
            var accessType = connectionGroup.AccessConnectionType;
            var agents = (await GetAllAgentsByStatusAsync(new List<ServiceStatus> { ServiceStatus.Active, ServiceStatus.ActiveException }))
                .Where(o => o.SourceType.HasFlag(sourceType) && (o.Errors & serviceError) == ServiceErrors.None);

            if (accessType == Contract.FileSystemRegister.AccessConnectionType.All)
            {
                return FilterAgentsByDCInternalName(agents, string.Empty).ToList();
            }

            var underGroupAgentIds = (await FSConnectionGroupWithAgentMemebership.FindListAsync(item => item.ConnectionGroupId == connecitonGroupId)).Select(item => item.AgentId).ToList();
            return FilterAgentsByDCInternalName(
                    agents.Where(o => underGroupAgentIds.Contains(o.Id)),
                    connectionGroup.DCInternalName)
                .ToList();
        }

        private IEnumerable<RMAgentDto> FilterAgentsByDCInternalName(IEnumerable<RMAgentDto> agents, string dcInternalName)
        {
            var mainDCInternalName = MultiGeoDataCenterService.GetMainDC();
            if (string.IsNullOrWhiteSpace(dcInternalName))
            {
                return agents.Where(agent => string.IsNullOrWhiteSpace(agent.DCInternalName)
                    || (!string.IsNullOrWhiteSpace(mainDCInternalName)
                        && mainDCInternalName.Equals(agent.DCInternalName, StringComparison.OrdinalIgnoreCase)));
            }

            return agents.Where(agent => dcInternalName.Equals(agent.DCInternalName, StringComparison.OrdinalIgnoreCase));
        }

        private async Task PopulateAgentDCDisplayNamesAsync(IList<RMAgentDto> agents)
        {
            if (agents == null || agents.Count == 0)
            {
                return;
            }

            var multiGeoDCInfo = await MultiGeoDataCenterService.GetMultiGeoDCInformation();
            var dcDisplayNames = multiGeoDCInfo?.DCsSupported?
                .Where(dc => !string.IsNullOrWhiteSpace(dc.DCInternalName))
                .GroupBy(dc => dc.DCInternalName, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First().DCDisplayName ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var agent in agents)
            {
                if (agent == null)
                {
                    continue;
                }

                var targetDCInternalName = string.IsNullOrWhiteSpace(agent.DCInternalName)
                    ? multiGeoDCInfo?.MainDC
                    : agent.DCInternalName;

                agent.DCDisplayName = !string.IsNullOrWhiteSpace(targetDCInternalName)
                    && dcDisplayNames.TryGetValue(targetDCInternalName, out var dcDisplayName)
                    ? dcDisplayName
                    : string.Empty;
            }
        }

        private bool HasLiense(string tenantId, SourceType sourceType)
        {
            if (!TenantInfoDao.CheckTenantIsAvailable(tenantId)) return false;

            if (TenantInfoDao.CheckAdditionalDataSource(tenantId, (long)sourceType.Map2PaidForModule())) return true;

            if (sourceType != SourceType.FileSystem) return false;

            if (TenantInfoDao.CheckAdditionalProduct(tenantId, (long)PaidForProduct.OpusFileSystemDiscovery)) return true;

            var previewFeatureKeyValue = KeyValueService.Get("PreviewFeature");
            if (previewFeatureKeyValue != null && long.TryParse(previewFeatureKeyValue.Value, out var previewMask))
            {
                return ((PreviewFeature)previewMask).HasFlag(PreviewFeature.FileSystemDiscovery);
            }

            return false;
        }

        private async Task<IList<RMAgentDto>> GetAllAgentsByStatusAsync(IList<ServiceStatus> statusList)
        {
            try
            {
                return (await RMAgentDao.FindListAsync(o => statusList.Contains(o.Status))).Select(o => o.Convert2Dto()).ToList();
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while get all agent information. error: {e}");
            }

            return new List<RMAgentDto>();
        }

        public async Task<IList<RMAgentDto>> GetAllByFarmIdAsync(string farmId)
        {
            try
            {
                return (await RMAgentDao.FindListAsync(o => o.SourceType.HasFlag(SourceType.SharePoint) && o.FarmId == farmId && o.Status == Hybrid.Contract.Object.ServiceStatus.Active)).Select(o => o.Convert2Dto()).ToList();
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while get farm: [{farmId}] all agent information. Error: {e}");
            }

            return new List<RMAgentDto>();
        }

        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.AgentManagement, Action = AuditAction.EditAgent, BeforeHandler = typeof(AppManagementBeforeAuditHandler), AfterHandler = typeof(AppManagementAfterAuditHandler))]
        public async Task<string> UpdateAgentAsync(RMAgentDto dto)
        {
            try
            {
                var entity = RMAgentDao.Find(o => o.Id == dto.Id);
                if (entity != null)
                {
                    if(!string.IsNullOrEmpty(dto.Name))
                    {
                        entity.Name = dto.Name;
                        entity.Description = dto.Description;
                        entity.SourceType = dto.SourceType;
                        entity.CollectLog = dto.CollectLog;
                    }

                    if (dto.DCInternalName.IsNotNullOrEmpty())
                    {
                        entity.DCInternalName = dto.DCInternalName;
                    }

                    await RMAgentDao.UpdateAsync(entity);

                    if(!entity.SourceType.HasFlag(SourceType.FileSystem))
                    {
                        await FSConnectionGroupWithAgentMemebership.RemoveAllByAgentIdsAsync(new List<Guid> { entity.Id });
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while update agent information, agent name : {dto.Name}. error: {e.ToString()}");
                return "1";
            }

            return "0";
        }

        public async Task<bool> UpdateAgentRelateFarmIdAsync(Guid id, string farmId)
        {
            try
            {
                var entity = RMAgentDao.Find(o => o.Id == id);
                if (entity != null)
                {
                    entity.FarmId = farmId;
                    return await RMAgentDao.UpdateAsync(entity);
                }
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while update agent relate farm id, agent id : {id}. error: {e}");
            }

            return false;
        }

        public async Task<bool> UpdateAgentResourceUsageAsync(RMAgentDto dto)
        {
            try
            {
                var entity = RMAgentDao.Find(o => o.Id == dto.Id);
                if (entity == null)
                {
                    logger.Warn($"Agent doesn't exist. agent id : {dto.Id} , server name : {dto.ServerName}");
                    return false;
                }

                if (entity.Status == ServiceStatus.Disabled || entity.Status == ServiceStatus.Deleted)
                {
                    logger.Warn($"Will not update agent because it is disabled or deleted, agent id : {dto.Id}, server name : {dto.ServerName}");
                    return false;
                }
                entity.JobCounts = dto.JobCounts;
                entity.CPUHZ = dto.CPUHZ;
                entity.AvailableCPU = dto.CPUUsage;
                entity.TotalMemory = dto.TotalMemory;
                entity.AvailableMemeory = dto.AvailableMemeory;
                entity.TimeStamp = dto.TimeStamp;
                entity.OSVersionNumber = dto.OSVersionNumber;
                entity.OSName = dto.OSName;
                entity.ServerName = dto.ServerName;
                entity.Version = dto.Version;
                entity.Errors = dto.Errors;
                entity.IsSupportUpgrade = dto.IsSupportUpgrade;
                if(entity.Status != ServiceStatus.Upgrading)
                {
                    entity.Status = dto.Status;
                }

                return await RMAgentDao.UpdateAsync(entity);
            }
            catch (DbEntityValidationException e)
            {
                var validationErrors = string.Join(" | ", e.EntityValidationErrors.Select(entityError =>
                    $"Entity: {entityError.Entry?.Entity?.GetType().FullName ?? "Unknown"}, State: {entityError.Entry?.State.ToString() ?? "Unknown"}, Errors: {string.Join("; ", entityError.ValidationErrors.Select(validationError => $"[{validationError.PropertyName}] {validationError.ErrorMessage}"))}"));
                logger.Error($"An error occurred while update agent information, agent id : {dto.Id} , agent name : {dto.Name}. validation errors: {validationErrors}. error: {e}");
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while update agent information, agent id : {dto.Id} , agent name : {dto.Name}. error: {e.ToString()}");
            }

            return false;
        }

        public async Task<bool> UpdateInstallationCodeAsync(Guid id, string installationCode)
        {
            try
            {
                var entity = RMAgentDao.Find(o => o.Id == id);
                if (entity != null)
                {
                    entity.InstallationCode = installationCode;
                    return await RMAgentDao.UpdateAsync(entity);
                }
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while update agent '{id}' with installation code '{installationCode}'. error: {e.ToString()}");
            }

            return false;
        }

        public async Task<bool> UpdateAuthCodeAsync(Guid id, string authCode)
        {
            if (string.IsNullOrEmpty(authCode)) return false;
            try
            {
                var entity = RMAgentDao.Find(o => o.Id == id);
                if (entity != null)
                {
                    entity.AuthCode = authCode;
                    return await RMAgentDao.UpdateAsync(entity);
                }
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while update agent '{id}' with auth code '{authCode}'. error: {e.ToString()}");
            }

            return false;
        }

        public async Task<bool> UpdateStatusAsync(Guid id, Hybrid.Contract.Object.ServiceStatus status)
        {
            try
            {
                var entity = RMAgentDao.Find(o => o.Id == id && o.Status != ServiceStatus.Deleted);
                if (entity != null)
                {
                    if (status == ServiceStatus.Deleted && entity.Status == ServiceStatus.Active)
                    {
                        logger.Warn($"Cannot delete agent '{id}' because its current status is Active");
                        return false;
                    }

                    entity.Status = status;
                    var res = await RMAgentDao.UpdateAsync(entity);
                    if(status == ServiceStatus.Deleted)
                    {
                        await FSConnectionGroupWithAgentMemebership.RemoveAllByAgentIdsAsync(new List<Guid> { id });
                    }

                    return res;
                }
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while update agent '{id}' with status  '{status.ToString()}'. error: {e.ToString()}");
            }

            return false;
        }

        public async Task<int> UpdateAgentsStatusAsync(IEnumerable<Guid> ids, ServiceStatus status)
        {
            try
            {
                if (ids == null || !ids.Any())
                {
                    return 0;
                }
                var entities = await RMAgentDao.FindListAsync(o => ids.Contains(o.Id) && o.Status != ServiceStatus.Deleted);

                if (!entities.Any()) return 0;

                foreach (var entity in entities)
                {
                    entity.Status = ServiceStatus.Upgrading;
                }

                var result = RMAgentDao.BatchUpdate(entities.ToList());

                return result;
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while update agents for upgrade process. error: {e.ToString()}");
                return 0;
            }
        }

        public async System.Threading.Tasks.Task CheckAndUpdateStatusAsync(int seconds, ServiceStatus targetStatus)
        {
            try
            {
                var stamp = DateTime.UtcNow.AddSeconds(-seconds).Ticks;
                var entities = await RMAgentDao.FindListAsync(o => (o.Status == ServiceStatus.Active || o.Status == ServiceStatus.ActiveException) && o.TimeStamp < stamp);
                if (entities.Count == 0) return;
                logger.Info("Start to check and update agent status :{0}", TenantLocalValue.LogonGroupId);
                foreach (var entity in entities)
                {
                    entity.Status = targetStatus;
                }
                var count = RMAgentDao.BatchUpdate(entities);
                logger.Info($"Check and update agent status count : {count}, target status : {targetStatus}");
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while check and update agent status, error : {e.ToString()}");
            }
        }

        public async Task<bool> UpdateCertificateIdAsync(List<Guid> ids, Guid certificateId)
        {
            try
            {
                var entities = await RMAgentDao.FindListAsync(o => ids.Contains(o.Id));
                if (entities.Count == 0) return false;
                foreach (var entity in entities)
                {
                    entity.CertificateId = certificateId;
                }
                var count = RMAgentDao.BatchUpdate(entities);
                return count > 0;
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while update certificate Id '{certificateId}' to agents. error: {e.ToString()}");
            }

            return false;
        }

        public bool CheckAgentIsUnderGroup(Guid id)
        {
            return FSConnectionGroupWithAgentMemebership.CheckAgentIsUnderGroup(id);
        }

        public async Task<bool> CheckIfEnableFSUniqueIdSetting()
        {
            var fsUnqieVersion = new Version("15.6.0.168");
            var allAgents = await GetAllAgentsByStatusAsync(new List<ServiceStatus> { ServiceStatus.Active, ServiceStatus.InActive, ServiceStatus.Disabled });
            var result = allAgents.All(a => new Version(a.Version) >= fsUnqieVersion);
            return result;
        }
        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.AgentManagement, Action = AuditAction.UpgradeAgent, BeforeHandler = typeof(AppManagementBeforeAuditHandler), AfterHandler = typeof(AppManagementAfterAuditHandler))]
        public async Task<(List<RMAgentDto> dtos, RMAgentUpgradeResult)> UpgradeCloudAgentAsync(RMAgentUpgradeDto dto = null)
        {
            List<RMAgentDto> agentsToUpgrades = new List<RMAgentDto>();
            try
            {
                logger.Info("Starting UpgradeAgentFeatureExecutor...");
                var (hasActiveAgents, activeAgents) = await CollectAvailableAgentsSync(dto);

                if (!hasActiveAgents) return (null, RMAgentUpgradeResult.NoActiveAgent);

                var (latestVersion, agentsToUpgrade) = HasVersionDiscrepancyAsync(activeAgents);
                agentsToUpgrades.AddRange(agentsToUpgrade);
                logger.Info($"The latest cloud agent installer version: {latestVersion}");

                if (latestVersion.IsNullOrEmpty() || !agentsToUpgrade.Any())
                    return (agentsToUpgrade, RMAgentUpgradeResult.NoLatestVersion);
                var mainDC = MultiGeoDataCenterService.GetMainDC();
                var isMultigeoMainDC = !string.IsNullOrEmpty(mainDC) && string.Equals(mainDC, RMSSOHelper.CurrentDCName, StringComparison.OrdinalIgnoreCase);
                if (isMultigeoMainDC)
                {
                    var agentsToUpgradesInMainDC = await ProcessMultiGeoAgentsAsync(mainDC, agentsToUpgrade);
                    if (!agentsToUpgradesInMainDC.Any())
                    {
                        logger.Info("No agent need to upgrade in mainDC.");

                        return (agentsToUpgrade, RMAgentUpgradeResult.Success);
                    }
                    agentsToUpgrade = agentsToUpgradesInMainDC;
                }

                if (HasRunningAgentJobs()) return (agentsToUpgrade, RMAgentUpgradeResult.HasRunningJob);

                logger.Info($"The active agent IDs need to upgrade is: [{string.Join(",", agentsToUpgrade.Select(a => a.Id))}]");
                await HybridBrowserService.ProcessUpgradeCloudAgent(agentsToUpgrade.Select(a => a.Id), latestVersion);

                await UpdateAgentsStatusAsync(agentsToUpgrade.Select(a => a.Id), ServiceStatus.Upgrading);

                logger.Info("Finished sending upgrade request.");
                
                return (agentsToUpgrade, RMAgentUpgradeResult.Success);
            }
            catch (NotAvailableAgentException naae)
            {
                logger.Warn($"UpgradeAgentAsync failed: {naae.Message}", naae);
                return (null, RMAgentUpgradeResult.NoActiveAgent);
            }
            catch (Exception ex)
            {
                logger.Error($"UpgradeAgentAsync failed: {ex.Message}", ex);
                return (null, RMAgentUpgradeResult.Failed);
            }
        }

        private async Task<List<RMAgentDto>> ProcessMultiGeoAgentsAsync(string mainDC, IList<RMAgentDto> activeAgents)
        {
            var mainDCAgents = new List<RMAgentDto>();

            var agentsBySourceDC = activeAgents.GroupBy(a => string.IsNullOrEmpty(a.DCInternalName) || string.Equals(a.DCInternalName, mainDC, StringComparison.OrdinalIgnoreCase) ? mainDC : a.DCInternalName);

            var agentsNeedUpgradeInOtherDC = new List<RMAgentDto>();

            foreach (var group in agentsBySourceDC)
            {
                var otherDCName = group.Key;
                if (string.Equals(otherDCName, mainDC, StringComparison.OrdinalIgnoreCase))
                {
                    mainDCAgents = group.ToList();
                    continue;
                }
                logger.Info($"Starting upgrade agent in other DC: {otherDCName}");
                var result = await RAMultiGeoClient.RouteApiActionAsync<RMAgentUpgradeDto, (List<RMAgentDto>, RMAgentUpgradeResult)>(MultiGeoOperationType.UpgradeCloudAgent, new RMAgentUpgradeDto
                {
                    AgentsId = group.Select(a => a.Id).ToList(),
                    Mode = RMAgentUpgradeMode.Specific,
                }, [otherDCName]);

                if(result[otherDCName].Item1.IsNotNullOrEmpty()) agentsNeedUpgradeInOtherDC.AddRange(result[otherDCName].Item1);

            }
            if (agentsNeedUpgradeInOtherDC.Any())
            {
                await UpdateAgentsStatusAsync(agentsNeedUpgradeInOtherDC.Select(a => a.Id), ServiceStatus.Upgrading);
            }
            logger.Info("Finished sending upgrade request to all other DC.");
            return mainDCAgents;
        }

        private async Task<(bool, IList<RMAgentDto>)> CollectAvailableAgentsSync(RMAgentUpgradeDto dto)
        {
            return dto.Mode switch
            {
                RMAgentUpgradeMode.Specific => await HasAvailableAgentsByIdsAsync(dto.AgentsId),
                RMAgentUpgradeMode.AllAgent => await HasAvailableAgentsAsync(),
                _ => (false, new List<RMAgentDto>())
            };
        }

        private bool HasRunningAgentJobs()
        {
            return JobMonitorService.GetRunningJobs(AgentJobTypes).Any();
        }

        private async Task<(bool, IList<RMAgentDto>)> HasAvailableAgentsAsync()
        {
            logger.Info("Checking all active agents.");
            var serviceStatuses = new List<ServiceStatus> { ServiceStatus.Active, ServiceStatus.ActiveException };
            var activeAgents = await GetAvailableAgentsToUpradeAsync(serviceStatuses);
            var availableAgents = activeAgents.Where(a => a.IsSupportUpgrade).ToList();
            return (availableAgents.Any(), availableAgents);
        }

        private async Task<(bool, IList<RMAgentDto>)> HasAvailableAgentsByIdsAsync(List<Guid> agentsId)
        {
            logger.Info("Update specific agent, start to check active agents");
            var serviceStatuses = new List<ServiceStatus> { ServiceStatus.Active, ServiceStatus.ActiveException };
            var activeAgents = await GetAvailableAgentsToUpradeAsync(serviceStatuses, agentsId);
            var availableAgents = activeAgents.Where(a => a.IsSupportUpgrade).ToList();
            return (activeAgents.Any(), availableAgents);
        }

        private async Task<IList<RMAgentDto>> GetAvailableAgentsToUpradeAsync(IList<ServiceStatus> statusList, List<Guid> agentsId = null)
        {
            var ext = ServiceStatus.Upgrading.ToString();
            if (agentsId != null && agentsId.Any())
            {
                return (await RMAgentDao.FindListAsync(o => 
                agentsId.Contains(o.Id) 
                && statusList.Contains(o.Status)))
                .Select(o => o.Convert2Dto()).ToList();
            }
            else
            {
                return (await RMAgentDao.FindListAsync(o => statusList.Contains(o.Status))).Select(o => o.Convert2Dto()).ToList();
            }
        }

        private (string, List<RMAgentDto>) HasVersionDiscrepancyAsync(IList<RMAgentDto> activeAgents)
        {
            string latestVersion = $"{RMGlobalConfiguration.AppConfig[RMAppSettingKey.AGENT_LATEST_VERSION]}";
            if (latestVersion.IsNullOrEmpty())
            {
                logger.Warn("Could not found the latest Agent Installer version.");
                return ("", new ());
            }
            return (latestVersion, activeAgents.Where(a => CompareVersions(a.Version, latestVersion) > 0).ToList());
        }

        private int CompareVersions(string agentVersion, string latestVersion)
        {
            Version agent = new Version(agentVersion);
            Version latest = new Version(latestVersion);
            return latest.CompareTo(agent);
        }

        public bool HasAgentsInUpgradingProcess() => RMAgentDao.Exist(o => o.Status == ServiceStatus.Upgrading);

        public bool TryCreateSkippedJobIfAgentUpgrading(JobType jobType, string jobRunByUser, out string jobId)
        {
            jobId = string.Empty;
            if (HasAgentsInUpgradingProcess() && AgentJobTypes.Contains(jobType))
            {
            jobId = JobMonitorService.CreateJob(jobType, jobRunByUser);
            JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_JS_Comment_Skipped_AgentInUpgrading");
            logger.Warn($"Created skipped job [{jobId}] for job type [{jobType}] because there are agents in upgrading process.");
            return true;
        }
            return false;
        }

        public bool TryCreateSkippedJobIfRunningJobExceedLimit(JobType jobType, string jobRunByUser, out string jobId)
        {
            jobId = string.Empty;
            if(FSHighPerformanceUtility.IsEnabledJPMCFileSystemFeature() && AgentJobTypes.Contains(jobType))
            {
                var availableAgents = GetAllAgentsByStatusAsync(new List<ServiceStatus> { ServiceStatus.Active, ServiceStatus.ActiveException }).GetAwaiter().GetResult();
                if(availableAgents.Count == 0)
                {
                    jobId = JobMonitorService.CreateJob(jobType, jobRunByUser);
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_SS_FSNoAvailableAgent");
                    logger.Warn($"No available agent for job type [{jobType}].");
                    return true;
                }
                var tenantId = TenantLocalValue.LogonGroupId;
                var subJobRunningCount = RMSubJobDao.GetRunningAgentJob(AgentJobTypes).Count();
                var concurrencyBudgetUtil = new ConcurrencyBudgetUtil();
                int maxJobPerUser = concurrencyBudgetUtil.CalMaxJobByTenant(tenantId).GetAwaiter().GetResult();
                if (subJobRunningCount >= maxJobPerUser)
                {
                    logger.Warn($"The number of job is running is {subJobRunningCount} and max job per user is {maxJobPerUser}. Continue waiting for can running job");
                    return true;
                }
            }
            return false;
        }

        private bool IsMinorVersionMismatched(string currentVersion, string latestVersion)
        {
            var cur = new Version(currentVersion);
            var latest = new Version(latestVersion);
            return (latest.Major == cur.Major) && ((latest.Minor > cur.Minor) || (latest.Build > cur.Build) || (latest.Revision > cur.Revision));
        }

        public async Task<AgentQueryResult> QueryAgentsAsync(AgentQueryParams queryDto)
        {
            var result = new AgentQueryResult();
            var isEnableMultiGeo = await MultiGeoSettingService.IsEnableMultiGeoFeature();
            if (isEnableMultiGeo)
            {
                queryDto.MainDCName = MultiGeoDataCenterService.GetMainDC();
            }
            var dbAgents = RMAgentDao.QueryAgents(queryDto, out int totalCount);

            result.TotalCount = totalCount;
            result.Agents = dbAgents?.Select(o => o.Convert2Dto()).ToList() ?? [];
            await PopulateAgentDCDisplayNamesAsync(result.Agents);

            bool needsCalculation = false;

            if (queryDto.PageIndex == 1)
            {
                needsCalculation = true;
            }
            else
            {
                var cached = RedisCacheService.CacheProvider.HMGet(AgentStatusFlagsCacheKey, new List<string> { CacheFieldHasMismatched, CacheFieldHasMinorVersionMismatched });
                if (cached != null && cached.ContainsKey(CacheFieldHasMismatched) && cached.ContainsKey(CacheFieldHasMinorVersionMismatched))
                {
                    bool.TryParse(cached[CacheFieldHasMismatched], out bool hasMismatchedValue);
                    result.HasMismatchedAgent = hasMismatchedValue;

                    bool.TryParse(cached[CacheFieldHasMinorVersionMismatched], out bool hasMinorVersionMismatchedValue);
                    result.HasMinorVersionMismatchedAgent = hasMinorVersionMismatchedValue;
                }
                else
                {
                    needsCalculation = true;
                }
            }

            if (needsCalculation)
            {
                result.HasMismatchedAgent = RMAgentDao.Exist(o => o.Status == ServiceStatus.Mismatched);

                if (!result.HasMismatchedAgent)
                {
                    var allActiveAgents = await GetAllAgentsByStatusAsync(new List<ServiceStatus>
                    {
                        ServiceStatus.Active,
                        ServiceStatus.ActiveException
                    });
                    string latestVersion = $"{RMGlobalConfiguration.AppConfig[RMAppSettingKey.AGENT_LATEST_VERSION]}";
                    result.HasMinorVersionMismatchedAgent = !string.IsNullOrEmpty(latestVersion) && allActiveAgents.Any(a => IsMinorVersionMismatched(a.Version, latestVersion));
                }
                else
                {
                    result.HasMinorVersionMismatchedAgent = false;
                }

                var cacheValues = new Dictionary<string, string>()
                {
                    { CacheFieldHasMismatched, result.HasMismatchedAgent.ToString() },
                    { CacheFieldHasMinorVersionMismatched, result.HasMinorVersionMismatchedAgent.ToString() }
                };

                RedisCacheService.CacheProvider.HMSet(AgentStatusFlagsCacheKey, cacheValues, AgentStatusFlagsCacheTtl);
            }

            return result;
        }

        public async Task<AgentQueryResult> FilterAgentsByDCAsync(AgentQueryParams queryDto)
        {
            var result = new AgentQueryResult();
            queryDto.MainDCName = MultiGeoDataCenterService.GetMainDC();
            var dbAgents = RMAgentDao.QueryAgentsByDC(queryDto, out int totalCount);

            result.TotalCount = totalCount;
            result.Agents = dbAgents?.Select(o => o.Convert2Dto()).ToList() ?? [];
            await PopulateAgentDCDisplayNamesAsync(result.Agents);

            return result;
        }

        public async Task<IList<RMAgentDto>> GetAgentsByIdsAsync(List<Guid> agentIds)
        {
            try
            {
                if (agentIds == null || !agentIds.Any())
                {
                    return new List<RMAgentDto>();
                }

                var entities = await RMAgentDao.FindListAsync(o => agentIds.Contains(o.Id));
                var agents = entities.Select(o => o.Convert2Dto()).ToList();
                await PopulateAgentDCDisplayNamesAsync(agents);
                return agents;
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while get agents by ids [{string.Join(", ", agentIds)}]. Error: {e}");
                return new List<RMAgentDto>();
            }
        }

        public async Task<List<Guid>> GetAgentIdsOnMainDCAsync()
        {
            var mainDCName = RMKeyValueDao.GetValueByKey(KeyNameCollection.JPMCMultiGEOMainDC)?.Value ?? string.Empty;
            try
            {
                var activeStatuses = new[]{ ServiceStatus.Active, ServiceStatus.ActiveException};

                var agents = await RMAgentDao.FindListAsync(agent => Enumerable.Contains(activeStatuses, agent.Status) && (string.IsNullOrEmpty(agent.DCInternalName) || agent.DCInternalName == mainDCName));

                return agents.Select(agent => agent.Id).ToList();
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while getting agents in main DC '{mainDCName}'.Error {ex}");
                return [];
            }
        }
    }
}
