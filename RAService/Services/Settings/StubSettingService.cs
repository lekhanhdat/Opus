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
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Server.StubSetting;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Setting;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.DisposalStubDao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Utility;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Model.DisposalStub;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services;
using AvePoint.RA.Service.Services.Settings.AuditHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LeaveStubType = AvePoint.GCommon.Contract.StorageOptimization.Object.LeaveStubType;

namespace AvePoint.RA.Services.Settings
{
    [Audit]
    internal class StubSettingService : RMServiceBase, IStubSettingService
    {
        private IRMMiscProfileDao StubSettingDao => PlatformWindsorManager.GetService<IRMMiscProfileDao>();
        private IRuleManagerService mRuleManagerService;
        private static List<int> obsoleteStubTypes = new List<int> { (int)LeaveStubType.Aspx };
        private IUserService _UserService;
        private IUserService UserService => PlatformWindsorManager.GetService(ref _UserService);
        private IJobQueueService JobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();
        private IRMRemoteNodeDao RMRemoteNodeDao => PlatformWindsorManager.GetService<IRMRemoteNodeDao>();
        private ISPSettingTreeService RMSPTreeService => PlatformWindsorManager.GetService<ISPSettingTreeService>();
        private ITeamsSettingTreeService TeamsTreeService => PlatformWindsorManager.GetService<ITeamsSettingTreeService>();
        private IJobMonitorService RMJobService => PlatformWindsorManager.GetService<IJobMonitorService>();
        private IRMSubJobDao SubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();
        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private IArchiverSiteMasterIndexDao ArchiverSiteMasterIndexDao => PlatformWindsorManager.GetService<IArchiverSiteMasterIndexDao>();
        private IRMStubDisposalSiteInfoDao StubDisposalSiteInfoDao => PlatformWindsorManager.GetService<IRMStubDisposalSiteInfoDao>();
        private IRMSiteStubSettingMappingDao SiteStubSettingMappingDao => PlatformWindsorManager.GetService<IRMSiteStubSettingMappingDao>();

        private RALogger logger = RALogger.GetInstance(typeof(StubSettingService));
        protected IRuleManagerService RuleManagerService
        {
            get
            {
                if (mRuleManagerService == null)
                {
                    mRuleManagerService = (IRuleManagerService)PlatformWindsorManager.GetService(typeof(IRuleManagerService));
                }
                return mRuleManagerService;
            }
        }
        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.StubSetting, Action = AuditAction.StubSettingCreate, BeforeHandler = typeof(ArchiverSettingsBeforeAuditHandler), AfterHandler = typeof(ArchiverSettingsAfterAuditHandler))]
        public async Task<RAReturnMessage> CreateStubSettingAsync(StubSettingDto dto)
        {
            RAReturnMessage result = new RAReturnMessage() { MessageType = RAMessageType.Successful };
            if (dto.Name.Length > 255)
            {
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = I18NEntity.GetString("RM_TM_NameLenTooLongMsg");
                return result;
            }
            if (obsoleteStubTypes.Contains(dto.StubType))
            {
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = I18NEntity.GetString("RM_TM_UseObsoleteStubTypeMessage");
                return result;
            }
            RMMiscProfile profile = MiscProfileConvert.ConvertStubSettingDtoToRMMiscProfile(dto);
            var temp = await GetStubMiscProfileByNameAsync(profile.Name);
            if (temp != null)
            {
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = I18NEntity.GetString("RM_AR_Stub_Name_ErrorMessage");
                return result;
            }
            else
            {
                int status = StubSettingDao.Create(profile);
                if (status == (int)CreateOrEditStatus.Success)
                {
                    return result;
                }
                else
                {
                    result.MessageType = RAMessageType.Failed;
                    result.ErrorMessage = I18NEntity.GetString("RM_AR_Storage_Unknow_ErrorMessage");
                    return result;
                }
            }
        }

        public bool MagrateDAOStubSetting(StubSettingDto dto)
        {
            if (dto.Name.Length > 255)
            {
                dto.Name = dto.Name.Substring(0, 255);
            }
            RMMiscProfile profile = MiscProfileConvert.ConvertStubSettingDtoToRMMiscProfile(dto, true);
            int status = StubSettingDao.Create(profile);
            return status == (int)CreateOrEditStatus.Success;
        }

        public async Task<StubSettingDto> GetStubTemplateByNameAsync(string name)
        {
            var profile = await GetStubMiscProfileByNameAsync(name);
            if (profile == null || profile.IsRemoved)
            {
                logger.Warn($"Stub setting with name {name} is marked as removed, skip loading operation.");
                return null;
            }
            return MiscProfileConvert.ConvertRMMiscProfileToStubSettingDto(profile);
        }
        private async Task<RMMiscProfile> GetStubMiscProfileByNameAsync(string name)
        {
            var profile = (await StubSettingDao.FindListAsync(s => s.Name.Equals(name) && s.Type == (int)AvePoint.GCommon.Contract.Server.Common.Profile.Object.ProfileType.StubSetting && !s.IsRemoved)).FirstOrDefault();
            return profile;
        }
        public async Task<StubSettingDto> GetStubTemplateByIdAsync(string id)
        {
            var profile = (await StubSettingDao.FindListAsync(s => s.Id.Equals(id) && s.Type == (int)AvePoint.GCommon.Contract.Server.Common.Profile.Object.ProfileType.StubSetting && !s.IsRemoved)).FirstOrDefault();
            return MiscProfileConvert.ConvertRMMiscProfileToStubSettingDto(profile);
        }

        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.StubSetting, Action = AuditAction.StubSettingDelete, BeforeHandler = typeof(ArchiverSettingsBeforeAuditHandler), AfterHandler = typeof(ArchiverSettingsAfterAuditHandler))]
        public async Task<RAReturnMessage> DeleteStubSettingAsync(List<string> ids)
        {
            var allRules = RuleManagerService.GetRulesFromRecords();
            RAReturnMessage result = new RAReturnMessage() { MessageType = RAMessageType.Successful };
            var oneDriveRule = allRules.Where(r => r.OneDriveRule != null && r.OneDriveRule.SOFilters != null && r.OneDriveRule.SOFilters.Count > 0).ToList();
            var archiverRule = allRules.Where(r => r.SOFilters != null && r.SOFilters.Count > 0).ToList();
            List<string> usedStubSettingIds = new List<string>();
            usedStubSettingIds.AddRange(oneDriveRule.Where(a => a.StubTemplateId != null).Select(a => a.StubTemplateId).ToList());
            usedStubSettingIds.AddRange(archiverRule.Where(a => a.StubTemplateId != null).Select(a => a.StubTemplateId).ToList());
            foreach (string id in ids)
            {
                if (!usedStubSettingIds.Contains(id))
                {
                    var stub = this.GetStubSettingDtoById(id);
                    if (stub == null || stub.IsRemoved)
                    {
                        logger.Warn($"Stub setting with id {id} is already removed, skip delete operation.");
                        continue;
                    }
                    int re = await StubSettingDao.SoftDeleteAsync(MiscProfileConvert.ConvertStubSettingDtoToRMMiscProfile(stub));
                }
                else
                {
                    result.MessageType = RAMessageType.Failed;
                    List<string> ruleNames = oneDriveRule.Where(a => a.StubTemplateId == id).Select(a => a.Name).ToList();
                    ruleNames.AddRange(archiverRule.Where(a => a.StubTemplateId == id).Select(a => a.Name).ToList());
                    ruleNames = ruleNames.Distinct().ToList();
                    string tempRulNames = string.Empty;
                    foreach (var temp in ruleNames)
                    {
                        tempRulNames = tempRulNames + temp + ',';
                    }
                    tempRulNames = tempRulNames.TrimEnd(',');
                    result.ErrorMessage = string.Format(I18NEntity.GetString("RM_AR_Stub_Delete_ErrorMessage"), StubSettingDao.Load(id).Name, tempRulNames);
                }
            }
            return result;
        }

        public StubSettingResult GetAllStubSettings(StubSettingResult pageInfobool)
        {
            List<RMMiscProfile> profiles = StubSettingDao.LoadAll(pageInfobool);
            foreach (var pro in profiles)
            {
                var tempDto = MiscProfileConvert.ConvertRMMiscProfileToStubSettingDto(pro);
                if (tempDto == null || tempDto.IsRemoved)
                {
                    logger.Warn($"Stub setting with id {pro.Id} is marked as removed, skip loading operation.");
                    continue;
                }
                pageInfobool.StubSettingUIDtosList.Add(MiscProfileConvert.ConvertToStubSettingUIDto(tempDto));
            }
            return pageInfobool;
        }

        public List<int> GetAllUsingObsoleteStubTypes()
        {
            if (!UserService.IsMemberOfSecurityGroup((int)BuiltInGroupId.Admin, TenantLocalValue.LogonUserId))
            {
                return new List<int>();
            }
            List<StubSettingUIDto> stubs = GetAllStubSettingsNotPaged();
            return stubs.Where(stub => obsoleteStubTypes.Contains(stub.StubType)).DistinctBy(stub => stub.StubType).Select(stub => stub.StubType).ToList();
        }

        public StubSettingUIDto GetStubSettingById(string id)
        {
            RMMiscProfile profile = StubSettingDao.Load(id);
            var tempDto = MiscProfileConvert.ConvertRMMiscProfileToStubSettingDto(profile);
            if (tempDto == null || tempDto.IsRemoved)
            {
                logger.Warn($"Stub setting with id {id} is marked as removed, skip loading operation.");
                return null;
            }
            return MiscProfileConvert.ConvertToStubSettingUIDto(tempDto);
        }

        public StubSettingDto GetStubSettingDtoById(string id)
        {
            RMMiscProfile profile = StubSettingDao.Load(id);
            var tempDto = MiscProfileConvert.ConvertRMMiscProfileToStubSettingDto(profile);
            if (tempDto == null || tempDto.IsRemoved)
            {
                logger.Warn($"Stub setting with id {id} is marked as removed, skip loading operation.");
                return null;
            }
            return tempDto;
        }

        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.StubSetting, Action = AuditAction.StubSettingUpdate, BeforeHandler = typeof(ArchiverSettingsBeforeAuditHandler), AfterHandler = typeof(ArchiverSettingsAfterAuditHandler))]
        public async Task<RAReturnMessage> UpdateStubSettingAsync(StubSettingDto dto)
        {
            RAReturnMessage result = new RAReturnMessage() { MessageType = RAMessageType.Successful };
            if (dto.Name.Length > 255)
            {
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = I18NEntity.GetString("RM_TM_NameLenTooLongMsg");
                return result;
            }
            RMMiscProfile profile = MiscProfileConvert.ConvertStubSettingDtoToRMMiscProfile(dto);
            profile.ModifiedTime = DateTime.UtcNow.Ticks;

            var existStubSameName = await GetStubTemplateByNameAsync(profile.Name);
            if (existStubSameName != null && !string.Equals(existStubSameName.Id, profile.Id, StringComparison.OrdinalIgnoreCase))
            {
                logger.Error($"Failed to update stub setting because the name {profile.Name} is already used by another stub setting with Id {existStubSameName.Id}.");
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = I18NEntity.GetString("RM_AR_Stub_Name_ErrorMessage");
                return result;
            }

            var temp = await GetStubTemplateByIdAsync(profile.Id);
            if (temp == null || temp.IsRemoved)
            {
                logger.Error($"Failed to update stub setting because the stub setting with id {profile.Id} does not exist or is marked as removed.");
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = I18NEntity.GetString("RM_AR_Storage_Unknow_ErrorMessage");
                return result;
            }

            if (temp.Id == profile.Id)
            {
                if (obsoleteStubTypes.Contains(dto.StubType))
                {
                    if (temp.StubType != dto.StubType)
                    {
                        result.MessageType = RAMessageType.Failed;
                        result.ErrorMessage = I18NEntity.GetString("RM_TM_UseObsoleteStubTypeMessage");
                        return result;
                    }
                }

                int status = await StubSettingDao.UpdateAsync(profile);
                if (status == (int)CreateOrEditStatus.Success)
                {
                    if (temp.IsEnabledRetention != dto.IsEnabledRetention
                        || temp.RetentionValue != dto.RetentionValue
                        || temp.RetentionUnit != dto.RetentionUnit)
                    {
                        SiteStubSettingMappingDao.UpdateRetentionInfoByStubTemplateId(Guid.Parse(profile.Id), dto.IsEnabledRetention, dto.RetentionValue, (int)dto.RetentionUnit);

                        var affectedMappings = await SiteStubSettingMappingDao.GetAllMappingsByStubTemplateAsync(Guid.Parse(profile.Id));

                        if (affectedMappings != null && affectedMappings.Any())
                        {
                            var affectedSiteUrls = affectedMappings.Select(m => m.SiteCollectionUrl).Distinct().ToList();

                            foreach (var siteUrl in affectedSiteUrls)
                            {
                                await RecalculateAndUpdateSiteMinRetentionAsync(siteUrl);
                            }
                        }
                    }
                    return result;
                }
                else
                {
                    logger.Error($"Failed to update stub setting with id {profile.Id}.");
                    result.MessageType = RAMessageType.Failed;
                    result.ErrorMessage = I18NEntity.GetString("RM_AR_Storage_Unknow_ErrorMessage");
                    return result;
                }
            }
            else
            {
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = I18NEntity.GetString("RM_AR_Storage_Unknow_ErrorMessage");
                return result;
            }
        }

        private async Task RecalculateAndUpdateSiteMinRetentionAsync(string siteUrl)
        {
            var allSiteMappings = await SiteStubSettingMappingDao.GetAllMappingsBySiteUrlAsync(siteUrl);

            if (allSiteMappings == null || !allSiteMappings.Any()) return;

            long siteMinRetentionTicks = long.MaxValue;
            bool hasActiveRetention = false;

            foreach (var mapping in allSiteMappings)
            {
                if (mapping.IsEnabledRetention && mapping.FirstStubCreatedTime > 0)
                {
                    long expirationTicks = CalculateExpirationTicks(mapping.FirstStubCreatedTime, mapping.RetentionValue, mapping.RetentionUnit);

                    if (expirationTicks < siteMinRetentionTicks)
                    {
                        siteMinRetentionTicks = expirationTicks;
                        hasActiveRetention = true;
                    }
                }
            }

            var siteInfo = await StubDisposalSiteInfoDao.GetStubDisposalSiteInfoBySiteUrlAsync(siteUrl);

            if (siteInfo != null)
            {
                if (hasActiveRetention)
                {
                    await StubDisposalSiteInfoDao.UpdateMinRetentionTimeAsync(siteInfo.Id, siteMinRetentionTicks);
                }
                else
                {
                    await StubDisposalSiteInfoDao.UpdateMinRetentionTimeAsync(siteInfo.Id, long.MaxValue);
                }
            }
        }

        private long CalculateExpirationTicks(long createdTicks, int retentionValue, DateUnit retentionUnit)
        {
            var createdTime = new DateTime(createdTicks, DateTimeKind.Utc);
            try
            {
                switch (retentionUnit)
                {
                    case DateUnit.Day: return createdTime.AddDays(retentionValue).Ticks;
                    case DateUnit.Week: return createdTime.AddDays(7 * retentionValue).Ticks;
                    case DateUnit.Month: return createdTime.AddMonths(retentionValue).Ticks;
                    case DateUnit.Year: return createdTime.AddYears(retentionValue).Ticks;
                    default: return long.MaxValue;
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                return long.MaxValue; // Tránh lỗi tràn số nếu cộng quá lớn
            }
        }

        public List<StubSettingUIDto> GetAllStubSettingsNotPaged()
        {
            List<StubSettingUIDto> result = new List<StubSettingUIDto>();
            List<RMMiscProfile> profiles = StubSettingDao.LoadAllByTypeNotPage(ProfileType.StubSetting);
            if (profiles != null && profiles.Count > 0)
            {
                foreach (var pro in profiles)
                {
                    if (pro.IsRemoved)
                    {
                        logger.Warn($"Stub setting with id {pro.Id} is marked as removed, skip loading operation.");
                        continue;
                    }
                    var tempDto = MiscProfileConvert.ConvertRMMiscProfileToStubSettingDto(pro);
                    result.Add(MiscProfileConvert.ConvertToStubSettingUIDto(tempDto));
                }
            }
            return result;
        }

        public HashSet<string> GetAllStubSettingNames()
        {
            var result = new HashSet<string>();
            List<RMMiscProfile> profiles = StubSettingDao.LoadAllByTypeNotPage(ProfileType.StubSetting);
            if (profiles != null && profiles.Count > 0)
            {
                foreach (var pro in profiles)
                {
                    if (pro.IsRemoved)
                    {
                        logger.Warn($"Stub setting with id {pro.Id} is marked as removed, skip loading operation.");
                        continue;
                    }
                    result.Add(pro.Name);
                }
            }
            return result;
        }

        public RAReturnMessage RunConvertStubJob(ConvertStubDto dto)
        {
            string id = string.Empty;
            RAReturnMessage result = new RAReturnMessage() { MessageType = RAMessageType.Successful };
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                //var loginName = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.ConvertStub,
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                    Parameters = SerializerHelper.SerializeByDataContractSerializer(dto)
                };

                id = JobQueueService.AddToDBJobQueue(jqDto);
                if (string.IsNullOrEmpty(id))
                {
                    result = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while run convert stub job,ERROR:{0}", ex.ToString());
                result = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
            }

            return result;
        }
        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.RunConvertStubJob, AfterHandler = typeof(ArchiverJobAfterAuditHandler))]
        public async Task<string> RealRunConvertStubJob(JobRunBy jobRunBy, string JobRunByUser, string param)
        {
            JobType jobType = JobType.ConvertStub;
            ConvertStubDto jobInfo = SerializerHelper.DeserializeByDataContractSerializer<ConvertStubDto>(param);
            var loginName = TenantLocalValue.LogonUserEmail;
            if (jobInfo.NodeSetting.Type == ContentSourceType.Teams)
            {
                return await RealRunTeamsConvertStubJobOnSelectedNode(jobRunBy, loginName, jobType, jobInfo.NodeSetting, jobInfo.StubType, jobInfo.StubTemplateId);
            }
            return await RealRunConvertStubJobOnSelectedNode(jobRunBy, loginName, jobType, jobInfo.NodeSetting, jobInfo.StubType, jobInfo.StubTemplateId);
        }

        private async Task<string> RealRunConvertStubJobOnSelectedNode(JobRunBy jobRunBy, string jobRunByUser, JobType jobType, RMSPTreeNode selectedNode, LeaveStubType stubType, Guid stubId)
        {
            string jobId = string.Empty;
            string nodeUrl = selectedNode.FullPath;

            logger.Info("Start RealRunConvertStubJobOnSelectedNode");

            var mIndexJobs = RMJobService.GetRunningJobs(JobTypeConstants.JobLevelConflictJobTypes);
            if (mIndexJobs.Count > 0)
            {
                //has move index or dedup job, need skip.
                logger.Warn("Current has move index job or dedup running");
                jobId = RMJobService.CreateJobWithScopeId(JobType.ConvertStub, jobRunByUser, nodeUrl);
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                return jobId;
            }

            List<RMSPTreeNode> availableNode = await AssembleConvertStubRunnableNode(selectedNode);
            if (availableNode.IsNullOrEmpty())
            {
                logger.Warn("No available sc to run");
                var message = selectedNode.Level == (int)NodeLevel.WebApplication
                    ? $"RM_SP_NoSiteCollectionUnderGroup{I18NEntity.Separator}{selectedNode.Name}"
                    : $"RM_JM_Report_Skip_NoAvailableSites";
                jobId = RMJobService.CreateJobWithScopeId(JobType.ConvertStub, jobRunByUser, nodeUrl);
                RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, message);
                return jobId;
            }
            var runningSiteUrls = RMJobService.GetRunningArchiverJobSiteUrl(JobTypeConstants.ArchiveSiteConflictType, availableNode.Select(node => node.GetSiteCollectionNode().FullPath));
            availableNode = RuleSPTreeUtil.FilterSCAvailableNodeByRunningUrl(availableNode, runningSiteUrls, selectedNode);
            if (availableNode.Count == 0)
            {
                logger.Warn($"not exsite can run job,will skip current job");
                jobId = RMJobService.CreateJobWithScopeId(JobType.ConvertStub, jobRunByUser, nodeUrl);
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                return jobId;
            }
            availableNode = FilterSiteCollectionNotRunArchiver(availableNode);
            if (availableNode.Count == 0)
            {
                logger.Warn($"not exsite can run job,will skip current job");
                jobId = RMJobService.CreateJobWithScopeId(JobType.ConvertStub, jobRunByUser, nodeUrl);
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_NotArchiverSiteCollection");
                return jobId;
            }
            jobId = RMJobService.CreateJobWithScopeId(JobType.ConvertStub, jobRunByUser, nodeUrl
                , jobConflictExtension: RuleSPTreeUtil.GenerateArchiveJobMonitorExtension(selectedNode, TreeMode.SO));
            logger.Info($"real run job node count after filter is {availableNode.Count}");
            int subJobCount = availableNode.Count;
            SubJobDao.UpdateSubJobCount(jobId, subJobCount);
            int currentSubjobIndex = 0;
            var subJobCountInConfigFile = RMKeyValueDao.GetSubJobCountFromDB((int)jobType);
            foreach (var node in availableNode)
            {
                var convertStubDto = new ConvertStubDto()
                {
                    StubType = stubType,
                    StubTemplateId = stubId,
                    NodeSetting = node
                };

                string subJobId = CreateSubJobForConvertStub(jobId, currentSubjobIndex, jobType, subJobCount, convertStubDto, currentSubjobIndex < subJobCountInConfigFile, node.FullPath, node.O365TenantId);
                logger.Debug("Start sub job {0}", subJobId);
                if (currentSubjobIndex < subJobCountInConfigFile)
                {
                    JobQueueService.HandleMessage(new JobQueueMessage()
                    {
                        JobId = subJobId,
                        RunBy = jobRunBy,
                        JobType = jobType,
                        CommandLine = string.Format("{0} {1}", jobType, subJobId),
                    });
                }
                currentSubjobIndex++;
            }
            return jobId;
        }

        public List<RMSPTreeNode> FilterAvailableNodeByRunningUrl(List<RMSPTreeNode> availableNode, string nodeFullPath, Tuple<List<string>, Dictionary<RunningSiteInfo, List<string>>> runningUrl)
        {
            List<RMSPTreeNode> result = new List<RMSPTreeNode>();

            foreach (var node in availableNode)
            {
                var sitePath = node.GetSiteCollectionNode()?.FullPath;

                if (availableNode.Count == 1)
                {
                    if (runningUrl.Item2.Count == 0)
                    {
                        logger.Info($"There is no running job and the current node can run convert stub job, Node path: {node.FullPath}");
                        result.Add(node);
                        break; // No running jobs, add and exit  
                    }

                    bool hasRunningJob = false;
                    foreach (var runningInfo in runningUrl.Item2)
                    {
                        if (runningInfo.Key.IsGroupLevelRunning)
                        {
                            logger.Info($"convert stub Current node is one of the container sites, Node path: {runningInfo.Key.ScopeString}");
                            if (runningInfo.Value.Any(url => url.Equals(sitePath)))
                            {
                                hasRunningJob = true;
                                logger.Info($"convert stub Current node has running job, URL: {sitePath}, Node path: {node.FullPath}");
                                break;
                            }
                            else
                            {
                                logger.Info($"convert stub Current node has no running job, URL: {sitePath}, Node path: {node.FullPath}");
                            }
                        }
                        else if (runningInfo.Key.IsOtherJobRunning)
                        {
                            hasRunningJob = runningInfo.Value.Any(url => url.Equals(sitePath));
                        }
                        else
                        {
                            //对于running job，如果running job是非container节点run job，则判断是不是同一个site url.
                            //如果不是，直接可以run job.
                            //如果是，则需要判断是否有包含关系，没有包含关系，则可以run job.
                            hasRunningJob = (runningInfo.Value.Any(url => url.Equals(sitePath)) && (IsPrefixWithSlash(nodeFullPath, runningInfo.Key.ScopeString) || IsPrefixWithSlash(runningInfo.Key.ScopeString, nodeFullPath)));
                        }
                        if (hasRunningJob)
                        {
                            logger.Info($"convert stub Current node has running job not group level, URL: {sitePath}, Node path: {node.FullPath}");
                            break;
                        }
                    }
                    if (hasRunningJob)
                    {
                        logger.Info($"Current node has running convert stub job, Node path: {nodeFullPath}");
                    }
                    else
                    {
                        logger.Info($"Current node has no running convert stub job, URL: {sitePath}, Node path: {node.FullPath}");
                        result.Add(node);
                    }
                }
                else
                {
                    logger.Info($"Current node is one of the container sites, Node path: {node.FullPath}");
                    if (!runningUrl.Item1.Contains(sitePath))
                    {
                        logger.Info($"Current node has no running convert stub job, URL: {sitePath}, Node path: {node.FullPath}");
                        result.Add(node);
                    }
                    else
                    {
                        logger.Info($"Current node has running convert stub job, URL: {sitePath}, Node path: {node.FullPath}");
                    }
                }
            }

            return result;
        }

        private async Task<string> RealRunTeamsConvertStubJobOnSelectedNode(JobRunBy jobRunBy, string jobRunByUser, JobType jobType, RMSPTreeNode selectedNode, LeaveStubType stubType, Guid stubId)
        {
            string jobId = string.Empty;
            string teamsUrl = selectedNode.GetTeamsNode()?.DisplayName ?? (RMRemoteNodeDao.GetTeamsGroupAndChannelsCollectionByTeamsId(selectedNode.GetTeamsNode()?.SPObjectId).Item1?.url ?? string.Empty);
            string nodeFullPath = selectedNode.FullPath;
            string nodeUrl = selectedNode.Level == (int)NodeLevel.Office365GroupEntire ? selectedNode.DisplayName ?? teamsUrl : nodeFullPath;
            logger.Info("Start RealRunTeamsConvertStubJobOnSelectedNode");

            var mIndexJobs = RMJobService.GetRunningJobs(JobTypeConstants.JobLevelConflictJobTypes);
            if (mIndexJobs.Count > 0)
            {
                //has move index or dedup job, need skip.
                logger.Warn("Current has move index job or dedup running");
                jobId = RMJobService.CreateJobWithScopeId(JobType.ConvertStub, jobRunByUser, nodeFullPath);
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                return jobId;
            }

            List<RMSPTreeNode> availableNode = await AssembleTeamsConvertStubRunnableNode(selectedNode);
            if (availableNode.IsNullOrEmpty())
            {
                logger.Warn("No available sc to run");
                var message = selectedNode.Level == (int)NodeLevel.WebApplication
                    ? $"RM_Teams_NoTeamsGroupUnderGroup{I18NEntity.Separator}{selectedNode.Name}"
                    : $"RM_JM_Report_Skip_NoAvailableSites";
                jobId = RMJobService.CreateJobWithScopeId(JobType.ConvertStub, jobRunByUser, nodeFullPath);
                RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, message);
                return jobId;
            }

            var runningUrls = RMJobService.GetRunningTeamsArchiverJobSiteUrl(JobTypeConstants.ArchiveTeamsConflictType, true,
                RuleSPTreeUtil.BuildSearchFilter(selectedNode, availableNode));
            availableNode = RuleSPTreeUtil.FilterTeamsAvailableNodeByRunningUrl(availableNode, runningUrls, selectedNode);

            if (availableNode.Count == 0)
            {
                logger.Warn($"not exsite can run job,will skip current job");
                jobId = RMJobService.CreateJobWithScopeId(JobType.ConvertStub, jobRunByUser, nodeFullPath);
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                return jobId;
            }
            availableNode = FilterSiteCollectionNotRunArchiver(availableNode);
            if (availableNode.Count == 0)
            {
                logger.Warn($"not exsite can run job,will skip current job");
                jobId = RMJobService.CreateJobWithScopeId(JobType.ConvertStub, jobRunByUser, nodeFullPath);
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_NotArchiverSiteCollection");
                return jobId;
            }
            jobId = RMJobService.CreateJobWithScopeIdForTeams(JobType.ConvertStub, jobRunByUser, nodeFullPath, nodeUrl
                , jobConflictExtension: RuleSPTreeUtil.GenerateTeamsArchiveJobMonitorExtension(selectedNode, TreeMode.SO));
            logger.Info($"real run job node count after filter is {availableNode.Count}");
            int subJobCount = availableNode.Count;
            SubJobDao.UpdateSubJobCount(jobId, subJobCount);
            int currentSubjobIndex = 0;
            var subJobCountInConfigFile = RMKeyValueDao.GetSubJobCountFromDB((int)jobType);
            foreach (var node in availableNode)
            {
                var convertStubDto = new ConvertStubDto()
                {
                    StubType = stubType,
                    StubTemplateId = stubId,
                    NodeSetting = node
                };

                string subJobId = CreateSubJobForConvertStub(jobId, currentSubjobIndex, jobType, subJobCount, convertStubDto, currentSubjobIndex < subJobCountInConfigFile, node.FullPath, node.O365TenantId);
                logger.Debug("Start sub job {0}", subJobId);
                if (currentSubjobIndex < subJobCountInConfigFile)
                {
                    JobQueueService.HandleMessage(new JobQueueMessage()
                    {
                        JobId = subJobId,
                        RunBy = jobRunBy,
                        JobType = jobType,
                        CommandLine = string.Format("{0} {1}", jobType, subJobId),
                    });
                }
                currentSubjobIndex++;
            }
            return jobId;
        }

        private static bool IsPrefixWithSlash(string prefix, string path)
        {
            if (!path.StartsWith(prefix))
            {
                return false;
            }
            //Same node
            if (prefix.Equals(path))
            {
                return true;
            }
            //Site/A
            //Site/AB
            //Site/A/B
            string remaining = path.Substring(prefix.Length);
            return remaining.StartsWith("/");
        }

        private List<RMSPTreeNode> FilterSiteCollectionNotRunArchiver(List<RMSPTreeNode> availableNode)
        {
            var archiverSiteCollectionUrls = ArchiverSiteMasterIndexDao.GetAllBackupSiteCollectionDistinctUrl();
            return availableNode.Where(a => archiverSiteCollectionUrls.Contains(a.FullPath)).ToList();
        }

        private string CreateSubJobForConvertStub(string jobId, int currentSubjobIndex, JobType jobType, int subJobCount, ConvertStubDto tempDto, bool sendNow, string scope, string o365TenantId)
        {
            string subJobId = string.Format(jobId + "_{0:D3}", currentSubjobIndex);
            var subJob = new RMSubJob() { Id = subJobId, ParentId = jobId, StartTime = DateTime.UtcNow.Ticks, JobType = (int)jobType, Progress = 0, Status = (int)JobStatus.Wait, Weight = 100d / subJobCount, O365TenantId = o365TenantId };
            subJob.Runable = sendNow ? RecordsConstants.SubJob_Runnable_Runing : RecordsConstants.SubJob_Runnable_Waiting;
            subJob.JobContext = new RMJobContext()
            {
                JobId = subJobId,
                Settings = SerializerHelper.SerializeByDataContractSerializer(tempDto)
            };
            subJob.String1 = scope;
            SubJobDao.CreateJob(subJob);
            logger.Info("Create sub job {0} sucessfull, type {1}, weight {2} , Path {3}", subJob.Id, subJob.JobType, subJob.Weight, scope);
            return subJobId;
        }

        private string GetSPContainerId(RMSPTreeNode selectedNode)
        {
            return TreeNodeUtil.GetSPContainderId(selectedNode);
        }

        private async Task<List<RMSPTreeNode>> AssembleConvertStubRunnableNode(RMSPTreeNode selectedNode)
        {
            List<RMSPTreeNode> availableNode = new List<RMSPTreeNode>();
            if (selectedNode.Level == (int)NodeLevel.WebApplication)
            {
                List<RMSPTreeNode> sites = await RMSPTreeService.BrowseAsync(selectedNode);
                if (sites.IsNullOrEmpty())
                {
                    return availableNode;
                }
                foreach (RMSPTreeNode site in sites)
                {
                    availableNode.Add(site);
                }
            }
            else
            {
                var siteNode = selectedNode.GetSiteCollectionNode();
                if (ValidateSiteExist(siteNode))
                {
                    selectedNode.O365TenantId = siteNode.O365TenantId;
                    availableNode.Add(selectedNode);
                }
                else
                {
                    logger.Info("Site collection not exist, site:{0}", selectedNode.Name);
                }
            }
            return availableNode;
        }

        public async Task<List<RMSPTreeNode>> AssembleTeamsConvertStubRunnableNode(RMSPTreeNode selectedNode)
        {
            List<RMSPTreeNode> availableNode = new List<RMSPTreeNode>();
            if (selectedNode.Level == (int)NodeLevel.WebApplication)
            {
                List<RMSPTreeNode> teamsNodes = await TeamsTreeService.BrowseAsync(selectedNode, false);
                if (teamsNodes.IsNullOrEmpty())
                {
                    return availableNode;
                }
                foreach (RMSPTreeNode teams in teamsNodes)
                {
                    var sites = await TeamsTreeService.BrowseDirectSitesByTeamNode(RMDtoConverter.ConvertRMTree2SPTree(teams));
                    availableNode.AddRange(sites);
                }
            }
            else if (selectedNode.Level == (int)NodeLevel.Office365GroupEntire)
            {
                if (ValidateTeamsExist(selectedNode))
                {
                    var sites = await TeamsTreeService.BrowseDirectSitesByTeamNode(RMDtoConverter.ConvertRMTree2SPTree(selectedNode));
                    availableNode.AddRange(sites);
                }
                else
                {
                    logger.Info("Teams not exist, teams:{0}", selectedNode.Name);
                }
            }
            else
            {
                if (ValidateSiteExist(selectedNode))
                {
                    availableNode.Add(selectedNode);
                }
                else
                {
                    logger.Info("Site not exist, site:{0}", selectedNode.Name);
                }
            }
            return availableNode;
        }

        private bool ValidateTeamsExist(RMSPTreeNode selectedNode)
        {
            RemoteSiteCollection site = null;
            try
            {
                site = RMRemoteNodeDao.GetTeamsGroupAndChannelsCollectionByTeamsId(selectedNode.Id).Item1;
                selectedNode.O365TenantId = site?.TenantId;
            }
            catch (Exception e)
            {
                logger.Error("get sp node error:{0}", e.ToString());
            }
            return site != null ? true : false;
        }

        private bool ValidateSiteExist(RMSPTreeNode selectedNode)
        {
            RemoteSiteCollection site = null;
            try
            {
                site = RMRemoteNodeDao.GetRemoteSiteCollectionById(selectedNode.Id);
                selectedNode.O365TenantId = site?.TenantId;
            }
            catch (Exception e)
            {
                logger.Error("get sp node error:{0}", e.ToString());
            }
            return site != null ? true : false;
        }

        public RAReturnMessage RunStubDisposalJob(JobRunBy jobRunBy, string jobRunByUser)
        {
            logger.Info($"Start StubDisposal job.");
            string id = string.Empty;
            RAReturnMessage msg = new RAReturnMessage();
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.StubDisposal,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName
                };

                id = JobQueueService.AddToDBJobQueue(jqDto);
                if (string.IsNullOrEmpty(id))
                {
                    msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while StubDisposal,ERROR:{0}", ex.ToString());
                msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
            }

            return msg;
        }

        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.TimerJobSettings, Action = AuditAction.RunStubDisposalJob, AfterHandler = typeof(ArchiverJobAfterAuditHandler))] // Todo: Handle audit
        public async Task<string> RealRunStubDisposalJobAsync(JobRunBy jobRunBy, string jobRunByUser)
        {
            logger.Info($"Start Stub Disposal job.");

            string jobId = string.Empty;
            JobType jobType = JobType.StubDisposal;

            var mJobs = RMJobService.GetRunningJobs([jobType]); // JobTypeConstants.ArchiverIndexConflictJobTypes
            if (mJobs.Count > 0)
            {
                jobId = RMJobService.CreateJob(jobType, jobRunByUser);
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_SS_JobSkip");
                return jobId;
            }

            //Todo: conflict job logic with other job types ?

            jobId = RMJobService.CreateJob(jobType, jobRunByUser);

            var infoes = StubDisposalSiteInfoDao.GetStubDisposalSiteInfoesByRetentionTime(DateTime.UtcNow.Ticks);

            if (infoes.IsNullOrEmpty())
            {
                logger.Info($"No site need to run stub disposal job.");
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_JS_StubDisposal_NoSite");
                return jobId;
            }

            CreateSubJobsForDisposalStub(jobRunBy, jobId, infoes);

            return jobId;
        }

        private void CreateSubJobsForDisposalStub(JobRunBy jobRunBy, string mainJobId, List<RMStubDisposalSiteInfo> stubDisposalSiteInfoes)
        {
            JobType jobType = JobType.StubDisposal;
            double subJobWeight = 100d / stubDisposalSiteInfoes.Count;

            var currentIndex = 0;
            var subJobCountInConfigFile = RMKeyValueDao.GetSubJobCountFromDB((int)jobType);
            var startDisposalTime = DateTime.UtcNow;
            SubJobDao.UpdateSubJobCount(mainJobId, stubDisposalSiteInfoes.Count);

            foreach (var siteInfo in stubDisposalSiteInfoes)
            {
                var node = RMRemoteNodeDao.GetRemoteSiteCollectionByUrl(siteInfo.SiteCollectionUrl);

                if (node == null)
                {
                    logger.Warn($"Site collection not exist, site url:{siteInfo.SiteCollectionUrl}, skip creating disposal stub sub job.");
                    continue;
                }

                bool sendNow = currentIndex < subJobCountInConfigFile;
                var tempDto = new StubDisposalSiteInfoDto()
                {
                    Id = siteInfo.Id,
                    SiteCollectionUrl = siteInfo.SiteCollectionUrl,
                    MinRetentionTime = siteInfo.MinRetentionTime,
                    StartDisposalTime = startDisposalTime
                };

                string jobParams = SerializerHelper.SerializeByJsonConvert(tempDto);

                string subJobId = string.Format(mainJobId + "_{0:D3}", currentIndex);

                var subJob = new RMSubJob()
                {
                    Id = subJobId,
                    ParentId = mainJobId,
                    StartTime = DateTime.UtcNow.Ticks,
                    JobType = (int)jobType,
                    Progress = 0,
                    Status = (int)JobStatus.Wait,
                    String1 = siteInfo.SiteCollectionUrl,
                    O365TenantId = node.TenantId,
                    Weight = subJobWeight,
                    Runable = sendNow ? RecordsConstants.SubJob_Runnable_Runing : RecordsConstants.SubJob_Runnable_Waiting,
                    JobContext = new RMJobContext() { JobId = subJobId, Settings = jobParams }
                };
                SubJobDao.CreateJob(subJob);
                logger.Info("Create sub job {0} sucessfull, type {1}, weight {2} ", subJob.Id, subJob.JobType, subJobWeight);

                if (sendNow)
                {
                    logger.Info($"Start the dedup sub job: {subJobId}, site url:{siteInfo.SiteCollectionUrl}");
                    JobQueueService.HandleMessage(new JobQueueMessage()
                    {
                        JobId = subJobId,
                        RunBy = jobRunBy,
                        JobType = jobType,
                        CommandLine = string.Format("{0} {1}", jobType, subJobId),
                    });
                }
                currentIndex++;
            }
        }
    }
}
