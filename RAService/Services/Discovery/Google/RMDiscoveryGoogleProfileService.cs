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
using AvePoint.RA.Contract.Audit.Async;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Discovery.Model.Profile;
using AvePoint.RA.Contract.Discovery.Model.Query.Google.Parameter;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery.Google;
using AvePoint.RA.DB.Dao.Discovery.Impl.Google;
using AvePoint.RA.DB.Model.Discovery.Google;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.Discovery.Google.Audit;
using AvePoint.RA.Service.Services.Discovery.Google.Profile.Checker;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Google;

[AsyncAudit]
public class RMDiscoveryGoogleProfileService : IRMDiscoveryGoogleProfileService
{
    private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryGoogleProfileService));

    private readonly IRMDiscoveryGoogleProfileDao _profileDao = new RMDiscoveryGoogleProfileDao();

    private readonly IRMDiscoveryGoogleSizeRangeDao _sizeRangeDao = new RMDiscoveryGoogleSizeRangeDao();

    private readonly IRMDiscoveryGoogleFileExtensionDao _fileExtensionDao = new RMDiscoveryGoogleFileExtensionDao();

    private readonly IRMDiscoveryGoogleRuleInfoDao _ruleInfoDao = new RMDiscoveryGoogleRuleInfoDao();

    private readonly IRMDiscoveryGoogleWithoutInDateDao _withoutInDateDao = new RMDiscoveryGoogleWithoutInDateDao();

    private readonly IJobQueueService _jobQueueService = PlatformWindsorManager.GetService<IJobQueueService>();

    private readonly IJobMonitorService _jobMonitorService = PlatformWindsorManager.GetService<IJobMonitorService>();

    private readonly IRMDiscoveryGoogleJobManagementService _jobManagentService = new RMDiscoveryGoogleJobManagementService();

    [AsyncAudit(Module = AuditModule.Discovery, Category = AuditCategory.InactiveData, Action = AuditAction.AddInactiveProfileInfo, IAsyncBeforeHandler = typeof(RMDiscoveryGoogleConfigurationBeforeAuditHandler), IAsyncAfterHandler = typeof(RMDiscoveryGoogleConfigurationAfterAuditHandler))]
    public async Task<RAReturnMessage> AddInactiveProfileInfoAsync(RMDiscoveryGoogleProfileDataInfo dataInfo)
    {
        try
        {
            await CheckDiscoveryJobInfoAsync(false);
            var checker = new RMDiscoveryGoogleProfileChecker(dataInfo, RMDiscoveryProfileType.Inactive);
            var (succeed, failedMessage) = await checker.AddCheckAsync();
            if (!succeed)
            {
                return failedMessage;
            }

            var profileInfo = new RMDiscoveryGoogleProfileInfo
            {
                Id = Guid.NewGuid(),
                Name = dataInfo.Name,
                SizeRange = dataInfo.SizeRange,
                SizeRangeQueryMode = dataInfo.SizeRangeQueryMode,
                GreaterThanEqualWithoutInDate = dataInfo.GreaterThanEqualWithoutInDate,
                LessThanEqualWithoutInDate = dataInfo.LessThanEqualWithoutInDate,
                FileExtensionIdsJson = JsonConvert.SerializeObject(dataInfo.FileExtensionIds),
                RuleIdsJson = JsonConvert.SerializeObject(new List<int>()),
                SortBy = dataInfo.SortBy,
                CreatedTime = DateTime.UtcNow.Ticks,
                ModifiedTime = DateTime.UtcNow.Ticks,
                ScanType = RMDiscoveryJobType.Newly,
                PrevScanStatus = RMDiscoveryJobStatus.Finished,
                CurrentScanStatus = RMDiscoveryJobStatus.Waiting,
                ProfileType = RMDiscoveryProfileType.Inactive,
                IsBuildIn = false
            };

            await _profileDao.AddOrUpdateProfileInfoAsync(dataInfo.OrganizationId, profileInfo);

            var res = SendProfileJob(JobRunBy.Control, new RMDiscoveryGoogleProfileJobDefinition
            {
                RunMode = RMDiscoveryJobRunMode.Specify,
                JobType = RMDiscoveryJobType.Newly,
                GoogleOrganizationId = dataInfo.OrganizationId,
                ProfileType = RMDiscoveryProfileType.Inactive,
                SpecifyProfileId = profileInfo.Id
            });

            if (!res)
            {
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = I18NEntity.GetString("RM_HS_Criteria_View_Msg_ValidOtherError")
                };
            }

            return new RAReturnMessage
            {
                MessageType = RAMessageType.Successful,
            };
        }
        catch (Exception e)
        {
            _logger.Error($"An error occurred while add google organization [{dataInfo.OrganizationId}] inactive profile info. Error: {e}");
            return new RAReturnMessage
            {
                MessageType = RAMessageType.Failed,
                ErrorMessage = I18NEntity.GetString("RM_HS_Criteria_View_Msg_ValidOtherError")
            };
        }
    }

    [AsyncAudit(Module = AuditModule.Discovery, Category = AuditCategory.ROTData, Action = AuditAction.AddRotProfileInfo, IAsyncBeforeHandler = typeof(RMDiscoveryGoogleConfigurationBeforeAuditHandler), IAsyncAfterHandler = typeof(RMDiscoveryGoogleConfigurationAfterAuditHandler))]
    public async Task<RAReturnMessage> AddRotProfileInfoAsync(RMDiscoveryGoogleProfileDataInfo dataInfo)
    {
        try
        {
            await CheckDiscoveryJobInfoAsync(true);
            var checker = new RMDiscoveryGoogleProfileChecker(dataInfo, RMDiscoveryProfileType.ROT);
            var (succeed, failedMessage) = await checker.AddCheckAsync();
            if (!succeed)
            {
                return failedMessage;
            }

            var profileInfo = new RMDiscoveryGoogleProfileInfo
            {
                Id = Guid.NewGuid(),
                Name = dataInfo.Name,
                SizeRange = -1,
                SizeRangeQueryMode = RMDiscoveryGoogleSizeRangeQueryMode.GenerateThanEqual,
                GreaterThanEqualWithoutInDate = dataInfo.GreaterThanEqualWithoutInDate,
                LessThanEqualWithoutInDate = dataInfo.LessThanEqualWithoutInDate,
                FileExtensionIdsJson = JsonConvert.SerializeObject(dataInfo.FileExtensionIds),
                RuleIdsJson = JsonConvert.SerializeObject(dataInfo.RuleIds),
                SortBy = dataInfo.SortBy,
                CreatedTime = DateTime.UtcNow.Ticks,
                ModifiedTime = DateTime.UtcNow.Ticks,
                ScanType = RMDiscoveryJobType.Newly,
                PrevScanStatus = RMDiscoveryJobStatus.Finished,
                CurrentScanStatus = RMDiscoveryJobStatus.Waiting,
                ProfileType = RMDiscoveryProfileType.ROT,
                IsBuildIn = false
            };

            await _profileDao.AddOrUpdateProfileInfoAsync(dataInfo.OrganizationId, profileInfo);

            var res = SendProfileJob(JobRunBy.Control, new RMDiscoveryGoogleProfileJobDefinition
            {
                RunMode = RMDiscoveryJobRunMode.Specify,
                JobType = RMDiscoveryJobType.Newly,
                GoogleOrganizationId = dataInfo.OrganizationId,
                ProfileType = RMDiscoveryProfileType.ROT,
                SpecifyProfileId = profileInfo.Id
            });

            if (!res)
            {
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = I18NEntity.GetString("RM_HS_Criteria_View_Msg_ValidOtherError")
                };
            }

            return new RAReturnMessage
            {
                MessageType = RAMessageType.Successful,
            };
        }
        catch (Exception e)
        {
            _logger.Error($"An error occurred while add google organization [{dataInfo.OrganizationId}] rot profile info. Error: {e}");
            return new RAReturnMessage
            {
                MessageType = RAMessageType.Failed,
                ErrorMessage = I18NEntity.GetString("RM_HS_Criteria_View_Msg_ValidOtherError")
            };
        }
    }

    [AsyncAudit(Module = AuditModule.Discovery, Category = AuditCategory.InactiveData, Action = AuditAction.DeleteInactiveProfileInfo, IAsyncBeforeHandler = typeof(RMDiscoveryGoogleConfigurationBeforeAuditHandler), IAsyncAfterHandler = typeof(RMDiscoveryGoogleConfigurationAfterAuditHandler))]
    public async Task<RAReturnMessage> DeleteInactiveProfileInfoAsync(RMDiscoveryGoogleProfileDataInfo dataInfo)
    {
        try
        {
            var checker = new RMDiscoveryGoogleProfileChecker(dataInfo, RMDiscoveryProfileType.Inactive);
            var (succeed, failedMessage) = await checker.DeleteCheckAsync();
            if (!succeed)
            {
                return failedMessage;
            }

            await _profileDao.DeleteProfileFailedInfoesAsync(dataInfo.OrganizationId, dataInfo.Id);
            await _profileDao.DeleteProfileInfoAsync(dataInfo.OrganizationId, dataInfo.Id);
            await RMDiscoveryDBManager.DropGoogleInactiveProfileTablesAsync(dataInfo.OrganizationId, dataInfo.Id);
            return new RAReturnMessage
            {
                MessageType = RAMessageType.Successful
            };
        }
        catch (Exception e)
        {
            _logger.Error($"An error occurred while delete google organization [{dataInfo.OrganizationId}] inactive profile [{dataInfo.Id}] info. Error: {e}");
            return new RAReturnMessage
            {
                MessageType = RAMessageType.Failed,
                ErrorMessage = I18NEntity.GetString("RM_HS_Criteria_View_Msg_ValidOtherError")
            };
        }
    }

    [AsyncAudit(Module = AuditModule.Discovery, Category = AuditCategory.ROTData, Action = AuditAction.DeleteRotProfileInfo, IAsyncBeforeHandler = typeof(RMDiscoveryGoogleConfigurationBeforeAuditHandler), IAsyncAfterHandler = typeof(RMDiscoveryGoogleConfigurationAfterAuditHandler))]
    public async Task<RAReturnMessage> DeleteRotProfileInfoAsync(RMDiscoveryGoogleProfileDataInfo dataInfo)
    {
        try
        {
            var checker = new RMDiscoveryGoogleProfileChecker(dataInfo, RMDiscoveryProfileType.ROT);
            var (succeed, failedMessage) = await checker.DeleteCheckAsync();
            if (!succeed)
            {
                return failedMessage;
            }

            await _profileDao.DeleteProfileFailedInfoesAsync(dataInfo.OrganizationId, dataInfo.Id);
            await _profileDao.DeleteProfileInfoAsync(dataInfo.OrganizationId, dataInfo.Id);
            await RMDiscoveryDBManager.DropGoogleRotProfileTablesAsync(dataInfo.OrganizationId, dataInfo.Id);
            return new RAReturnMessage
            {
                MessageType = RAMessageType.Successful
            };
        }
        catch (Exception e)
        {
            _logger.Error($"An error occurred while delete google organization [{dataInfo.OrganizationId}] rot profile [{dataInfo.Id}] info. Error: {e}");
            return new RAReturnMessage
            {
                MessageType = RAMessageType.Failed,
                ErrorMessage = I18NEntity.GetString("RM_HS_Criteria_View_Msg_ValidOtherError")
            };
        }
    }

    public async Task<List<RMDiscoveryGoogleProfileDataInfo>> GetInactiveProfileInfoListAsync(string googleOrganizationId)
    {
        try
        {
            var sizeRanges = (await _sizeRangeDao.GetAllAsync()).Concat(new List<RMDiscoveryGoogleSizeRange> { new RMDiscoveryGoogleSizeRange { Id = -1, DisplayName = I18NEntity.GetString("RM_FA_Inactive_OptimizationTab_FileSizeRangeAll") } }).ToDictionary(item => item.Id, item => item.DisplayName);
            var fileTypes = (await _fileExtensionDao.GetAllAsync(googleOrganizationId)).ToDictionary(item => item.Id, item => I18NEntity.GetString(item.Name));
            var dateRanges = (await _withoutInDateDao.GetAllAsync()).ToDictionary(item => item.Id, item => item.Unit);
            var profileInfoes = await _profileDao.GetProfileInfoesAsync(googleOrganizationId, RMDiscoveryProfileType.Inactive);

            return profileInfoes.ConvertAll(item => new RMDiscoveryGoogleProfileDataInfo
            {
                Id = item.Id,
                OrganizationId = googleOrganizationId,
                Name = I18NEntity.GetString(item.Name),
                SizeRange = item.SizeRange,
                SizeRangeQueryMode = item.SizeRangeQueryMode,
                GreaterThanEqualWithoutInDate = item.GreaterThanEqualWithoutInDate,
                LessThanEqualWithoutInDate = item.LessThanEqualWithoutInDate,
                FileExtensionIds = JsonConvert.DeserializeObject<List<int>>(item.FileExtensionIdsJson),
                RuleIds = JsonConvert.DeserializeObject<List<int>>(item.RuleIdsJson),
                SortBy = item.SortBy,
                Status = item.CurrentScanStatus,
                IsBuildIn = item.IsBuildIn,
                IsDefault = item.IsDefault,
                ModifiedTimeRangeLabel = $"{I18NEntity.GetString("RM_FA_Inactive_SummaryTab_ModifiedFrom")} {(item.GreaterThanEqualWithoutInDate == -1 ? I18NEntity.GetString("RM_FA_Inactive_ModifiedOption_Latest") : dateRanges[item.GreaterThanEqualWithoutInDate] + " " + I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Months"))} {I18NEntity.GetString("RM_FA_Inactive_SummaryTab_ModifiedTo")} {(item.LessThanEqualWithoutInDate == 999 ? I18NEntity.GetString("RM_FA_Inactive_ModifiedOption_Max") : dateRanges[item.LessThanEqualWithoutInDate] + " " + I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Months"))}",
                SizeRangeLabel = sizeRanges[item.SizeRange],
                FileTypeLabel = JsonConvert.DeserializeObject<List<int>>(item.FileExtensionIdsJson).Count == 0 ? I18NEntity.GetString("RM_FA_Inactive_OptimizationTab_FileSizeRangeAll") : string.Join(", ", JsonConvert.DeserializeObject<List<int>>(item.FileExtensionIdsJson).ConvertAll(i => fileTypes[i])),
            });
        }
        catch (Exception e)
        {
            _logger.Error($"An error occurred while get google tenant [{googleOrganizationId}] inactive profile info. Error: {e}");
            return [];
        }
    }

    public async Task<List<RMDiscoveryGoogleProfileDataInfo>> GetRotProfileInfoListAsync(string googleOrganizationId)
    {
        try
        {
            var res = new List<RMDiscoveryGoogleProfileDataInfo>();

            var ruleInfoes = await _ruleInfoDao.GetRuleInfoesAsync(true, RMDiscoveryRuleDefinitionKind.ROT);
            var sizeRanges = (await _sizeRangeDao.GetAllAsync()).Concat(new List<RMDiscoveryGoogleSizeRange> { new RMDiscoveryGoogleSizeRange { Id = -1, DisplayName = I18NEntity.GetString("RM_FA_Inactive_OptimizationTab_FileSizeRangeAll") } }).ToDictionary(item => item.Id, item => item.DisplayName);
            var fileTypes = (await _fileExtensionDao.GetAllAsync(googleOrganizationId)).ToDictionary(item => item.Id, item => I18NEntity.GetString(item.Name));
            var dateRanges = (await _withoutInDateDao.GetAllAsync()).ToDictionary(item => item.Id, item => item.Unit);

            var profileInfoes = await _profileDao.GetProfileInfoesAsync(googleOrganizationId, RMDiscoveryProfileType.ROT);
            foreach (var item in profileInfoes)
            {
                var profileDataInfo = new RMDiscoveryGoogleProfileDataInfo
                {
                    Id = item.Id,
                    OrganizationId = googleOrganizationId,
                    Name = I18NEntity.GetString(item.Name),
                    SizeRange = item.SizeRange,
                    SizeRangeQueryMode = item.SizeRangeQueryMode,
                    GreaterThanEqualWithoutInDate = item.GreaterThanEqualWithoutInDate,
                    LessThanEqualWithoutInDate = item.LessThanEqualWithoutInDate,
                    FileExtensionIds = JsonConvert.DeserializeObject<List<int>>(item.FileExtensionIdsJson),
                    RuleIds = JsonConvert.DeserializeObject<List<int>>(item.RuleIdsJson),
                    SortBy = item.SortBy,
                    Status = item.CurrentScanStatus,
                    IsBuildIn = item.IsBuildIn,
                    IsDefault = item.IsDefault,
                    ModifiedTimeRangeLabel = $"{I18NEntity.GetString("RM_FA_Inactive_SummaryTab_ModifiedFrom")} {(item.GreaterThanEqualWithoutInDate == -1 ? I18NEntity.GetString("RM_FA_Inactive_ModifiedOption_Latest") : dateRanges[item.GreaterThanEqualWithoutInDate] + " " + I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Months"))} {I18NEntity.GetString("RM_FA_Inactive_SummaryTab_ModifiedTo")} {(item.LessThanEqualWithoutInDate == 999 ? I18NEntity.GetString("RM_FA_Inactive_ModifiedOption_Max") : dateRanges[item.LessThanEqualWithoutInDate] + " " + I18NEntity.GetString("RM_JS_RDM_CreateRule_Unit_Months"))}",
                    SizeRangeLabel = sizeRanges[item.SizeRange],
                    FileTypeLabel = JsonConvert.DeserializeObject<List<int>>(item.FileExtensionIdsJson).Count == 0 ? I18NEntity.GetString("RM_FA_Inactive_OptimizationTab_FileSizeRangeAll") : string.Join(", ", JsonConvert.DeserializeObject<List<int>>(item.FileExtensionIdsJson).ConvertAll(i => fileTypes[i])),
                };
                res.Add(profileDataInfo);

                if (profileDataInfo.IsBuildIn)
                {
                    profileDataInfo.AvailableRuleCategories = new List<RMDiscoveryRuleCategory> {
                            RMDiscoveryRuleCategory.Redundant,
                            RMDiscoveryRuleCategory.Obsolete,
                            RMDiscoveryRuleCategory.Trivial
                        };
                    profileDataInfo.RuleInfoLabel = I18NEntity.GetString("RM_FA_Inactive_OptimizationTab_FileSizeRangeAll");
                    continue;
                }

                var profileRuleInfoes = ruleInfoes.Where(item => profileDataInfo.RuleIds.Contains(item.Id)).ToList();
                var profileCustomColumns = profileRuleInfoes
                    .ConvertAll(item => new RMDiscoveryTableColumnInfo(item.Name, item.ToCustomColumn().Name, item.Id));
                profileDataInfo.CustomColumns = profileCustomColumns;
                profileDataInfo.AvailableRuleCategories = profileRuleInfoes.Select(item => item.Category).ToHashSet().ToList();
                profileDataInfo.RuleInfoLabel = string.Join(", ", profileRuleInfoes.Select(item => item.Name));
            }

            return res;
        }
        catch (Exception e)
        {
            _logger.Error($"An error occurred while get google organization [{googleOrganizationId}] rot profile infor. Error: {e}");
            return [];
        }
    }

    [AsyncAudit(Module = AuditModule.Discovery, Category = AuditCategory.InactiveData, Action = AuditAction.UpdateInactiveProfileInfo, IAsyncBeforeHandler = typeof(RMDiscoveryGoogleConfigurationBeforeAuditHandler), IAsyncAfterHandler = typeof(RMDiscoveryGoogleConfigurationAfterAuditHandler))]
    public async Task<RAReturnMessage> UpdateInactiveProfileInfoAsync(RMDiscoveryGoogleProfileDataInfo dataInfo)
    {
        try
        {
            await CheckDiscoveryJobInfoAsync(false);
            var checker = new RMDiscoveryGoogleProfileChecker(dataInfo, RMDiscoveryProfileType.Inactive);
            var (succeed, failedMessage) = await checker.UpdateCheckAsync();
            if (!succeed)
            {
                return failedMessage;
            }

            var profileInfo = await _profileDao.GetProfileInfoByIdAsync(dataInfo.OrganizationId, dataInfo.Id);
            profileInfo.Name = dataInfo.Name;
            profileInfo.SizeRange = dataInfo.SizeRange;
            profileInfo.SizeRangeQueryMode = dataInfo.SizeRangeQueryMode;
            profileInfo.SortBy = dataInfo.SortBy;
            profileInfo.GreaterThanEqualWithoutInDate = dataInfo.GreaterThanEqualWithoutInDate;
            profileInfo.LessThanEqualWithoutInDate = dataInfo.LessThanEqualWithoutInDate;
            profileInfo.FileExtensionIdsJson = JsonConvert.SerializeObject(dataInfo.FileExtensionIds);
            profileInfo.RuleIdsJson = JsonConvert.SerializeObject(new List<int>());
            profileInfo.SortBy = dataInfo.SortBy;
            profileInfo.ModifiedTime = DateTime.UtcNow.Ticks;
            profileInfo.ScanType = RMDiscoveryJobType.Newly;
            profileInfo.CurrentScanStatus = RMDiscoveryJobStatus.Waiting;
            profileInfo.ProfileType = RMDiscoveryProfileType.Inactive;
            profileInfo.IsBuildIn = false;

            await _profileDao.AddOrUpdateProfileInfoAsync(dataInfo.OrganizationId, profileInfo);

            var res = SendProfileJob(JobRunBy.Control, new RMDiscoveryGoogleProfileJobDefinition
            {
                RunMode = RMDiscoveryJobRunMode.Specify,
                JobType = RMDiscoveryJobType.Newly,
                GoogleOrganizationId = dataInfo.OrganizationId,
                ProfileType = RMDiscoveryProfileType.Inactive,
                SpecifyProfileId = profileInfo.Id
            });

            if (!res)
            {
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = I18NEntity.GetString("RM_HS_Criteria_View_Msg_ValidOtherError")
                };
            }

            return new RAReturnMessage
            {
                MessageType = RAMessageType.Successful,
            };
        }
        catch (Exception e)
        {
            _logger.Error($"An error occurred while update google organization [{dataInfo.OrganizationId}] inactive profile info. Error: {e}");
            return new RAReturnMessage
            {
                MessageType = RAMessageType.Failed,
                ErrorMessage = I18NEntity.GetString("RM_HS_Criteria_View_Msg_ValidOtherError")
            };
        }
    }

    [AsyncAudit(Module = AuditModule.Discovery, Category = AuditCategory.ROTData, Action = AuditAction.UpdateRotProfileInfo, IAsyncBeforeHandler = typeof(RMDiscoveryGoogleConfigurationBeforeAuditHandler), IAsyncAfterHandler = typeof(RMDiscoveryGoogleConfigurationAfterAuditHandler))]
    public async Task<RAReturnMessage> UpdateRotProfileInfoAsync(RMDiscoveryGoogleProfileDataInfo dataInfo)
    {
        try
        {
            await CheckDiscoveryJobInfoAsync(true);
            var checker = new RMDiscoveryGoogleProfileChecker(dataInfo, RMDiscoveryProfileType.ROT);
            var (succeed, failedMessage) = await checker.UpdateCheckAsync();
            if (!succeed)
            {
                return failedMessage;
            }

            var profileInfo = await _profileDao.GetProfileInfoByIdAsync(dataInfo.OrganizationId, dataInfo.Id);
            profileInfo.Name = dataInfo.Name;
            profileInfo.SizeRange = -1;
            profileInfo.SizeRangeQueryMode = RMDiscoveryGoogleSizeRangeQueryMode.GenerateThanEqual;
            profileInfo.SortBy = dataInfo.SortBy;
            profileInfo.GreaterThanEqualWithoutInDate = dataInfo.GreaterThanEqualWithoutInDate;
            profileInfo.LessThanEqualWithoutInDate = dataInfo.LessThanEqualWithoutInDate;
            profileInfo.FileExtensionIdsJson = JsonConvert.SerializeObject(dataInfo.FileExtensionIds);
            profileInfo.RuleIdsJson = JsonConvert.SerializeObject(dataInfo.RuleIds);
            profileInfo.SortBy = dataInfo.SortBy;
            profileInfo.ModifiedTime = DateTime.UtcNow.Ticks;
            profileInfo.ScanType = RMDiscoveryJobType.Newly;
            profileInfo.CurrentScanStatus = RMDiscoveryJobStatus.Waiting;
            profileInfo.ProfileType = RMDiscoveryProfileType.ROT;
            profileInfo.IsBuildIn = false;

            await _profileDao.AddOrUpdateProfileInfoAsync(dataInfo.OrganizationId, profileInfo);

            var res = SendProfileJob(JobRunBy.Control, new RMDiscoveryGoogleProfileJobDefinition
            {
                RunMode = RMDiscoveryJobRunMode.Specify,
                JobType = RMDiscoveryJobType.Newly,
                GoogleOrganizationId = dataInfo.OrganizationId,
                ProfileType = RMDiscoveryProfileType.ROT,
                SpecifyProfileId = profileInfo.Id
            });

            if (!res)
            {
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = I18NEntity.GetString("RM_HS_Criteria_View_Msg_ValidOtherError")
                };
            }

            return new RAReturnMessage
            {
                MessageType = RAMessageType.Successful,
            };
        }
        catch (Exception e)
        {
            _logger.Error($"An error occurred while update google organization [{dataInfo.OrganizationId}] rot profile info. Error: {e}");
            return new RAReturnMessage
            {
                MessageType = RAMessageType.Failed,
                ErrorMessage = I18NEntity.GetString("RM_HS_Criteria_View_Msg_ValidOtherError")
            };
        }
    }
    public bool SendProfileJob(JobRunBy runBy, RMDiscoveryGoogleProfileJobDefinition definition)
    {
        try
        {
            var jobQueueDto = new JobQueueDto
            {
                JobType = JobType.DiscoveryGoogleProfileJob,
                JobRunType = runBy,
                TenantGroupId = TenantLocalValue.LogonGroupId,
                JobRunByUser = runBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule",
                Parameters = JsonConvert.SerializeObject(definition)
            };

            _jobQueueService.AddToDBJobQueue(jobQueueDto);
            _logger.Info($"Successful send google profile job [{definition.RunMode}].");
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error($"An error occurred while send google profile job [{definition.RunMode}]. Error: {ex}");
            return false;
        }
    }

    public string RealRunProfileJob(JobQueueDto queueDto)
    {
        try
        {
            var jobId = _jobMonitorService.CreateJob(JobType.DiscoveryGoogleProfileJob, queueDto.JobRunByUser);

            _jobQueueService.HandleMessage(new JobQueueMessage
            {
                JobId = jobId,
                JobType = JobType.DiscoveryGoogleProfileJob,
                CommandLine = $"{JobType.DiscoveryGoogleProfileJob} {jobId}",
                Extension = queueDto.Parameters
            });

            _logger.Info($"Successful real run google profile job: [{queueDto.Parameters}].");

            return jobId;
        }
        catch (Exception e)
        {
            _logger.Error($"An error occurred while real run google profile job: [{queueDto.Parameters}]. Error: {e}");
            return string.Empty;
        }
    }
    private async Task CheckDiscoveryJobInfoAsync(bool checkRot)
    {
        var jobInfo = await _jobManagentService.GetLatestAsync();
        if (jobInfo.Status != RMDiscoveryJobStatus.Finished &&
            jobInfo.Status != RMDiscoveryJobStatus.Failed &&
            jobInfo.Status != RMDiscoveryJobStatus.Exception)
        {
            throw new Exception("The discovery google job is running. Didn't start profile job.");
        }

        if (checkRot && !jobInfo.EnableRot)
        {
            throw new Exception("The discovery google configuration not enable rot.");
        }
    }
}