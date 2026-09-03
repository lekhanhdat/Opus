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
using AvePoint.GCommon.Utility.I18N;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.Audit.Async;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.Discovery.ExportDiscoveryProfile;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Office365;
using AvePoint.RA.Contract.Discovery.Model.Profile;
using AvePoint.RA.Contract.Discovery.Model.Query.Office365.Parameter;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.JobMonitor;
using AvePoint.RA.Service.Services.Discovery.Office365.Audit;
using AvePoint.RA.Service.Services.Discovery.Office365.Profile.Checker;
using AvePoint.RA.Service.Services.JobQueue;
using AvePoint.RA.Service.Services.ManualApproval.AuditHandler;
using AvePoint.RA.Service.Services.TermManagement.AuditHandler;
using Cloud.Sdk.Data.AosModern;
using AvePoint.RA.Service.Services.Discovery.Office365.Query.Profile.ExportDataQuerier;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JobStatus = AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus;
using AvePoint.RA.Common.Extension;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using SfMetadataApi;

namespace AvePoint.RA.Service.Services.Discovery.Office365
{
    [AsyncAudit]
    public class RMDiscoveryOffice365ProfileService : IRMDiscoveryOffice365ProfileService
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryOffice365ProfileService));

        private readonly IRMDiscoveryOffice365ProfileDao _profileDao = new RMDiscoveryOffice365ProfileDao();

        private readonly IRMDiscoveryOffice365SizeRangeDao _sizeRangeDao = new RMDiscoveryOffice365SizeRangeDao();

        private readonly IRMDiscoveryOffice365FileExtensionDao _fileExtensionDao = new RMDiscoveryOffice365FileExtensionDao();

        private readonly IRMDiscoveryOffice365RuleInfoDao _ruleInfoDao = new RMDiscoveryOffice365RuleInfoDao();

        private readonly IRMDiscoveryOffice365WithoutInDateDao _withoutInDateDao = new RMDiscoveryOffice365WithoutInDateDao();

        private readonly IJobQueueService _jobQueueService = PlatformWindsorManager.GetService<IJobQueueService>();

        private readonly IJobMonitorService _jobMonitorService = PlatformWindsorManager.GetService<IJobMonitorService>();

        private readonly IRMDiscoveryOffice365ConfigurationService _office365ConfigurationService = new RMDiscoveryOffice365ConfigurationService();

        private readonly IRMDiscoveryOffice365JobManagentService _jobManagentService = new RMDiscoveryOffice365JobManagentService();
        private readonly IAccountDao AccountDao =  PlatformWindsorManager.GetService<IAccountDao>();
        private static IDownloadDataInfoDao DownloadDataInfoDao => PlatformWindsorManager.GetService<IDownloadDataInfoDao>();

        private readonly IRMDiscoveryOffice365SiteOptimizationMappingTableDao _siteOptimizationMappingTableDao = new RMDiscoveryOffice365SiteOptimizationMappingTableDao();

        #region Inactive
        public async Task<List<RMDiscoveryProfileDataInfo>> GetInactiveProfileInfoesAsync(Guid o365TenantId)
        {
            try
            {
                var sizeRanges = (await _sizeRangeDao.GetAllAsync()).Concat(new List<RMDiscoveryOffice365SizeRange> { new RMDiscoveryOffice365SizeRange { Id = -1, DisplayName = I18NEntity.GetString("RM_FA_Inactive_OptimizationTab_FileSizeRangeAll") } }).ToDictionary(item => item.Id, item => item.DisplayName);
                var fileTypes = (await _fileExtensionDao.GetAllAsync(o365TenantId)).ToDictionary(item => item.Id, item => I18NEntity.GetString(item.Name));
                var dateRanges = (await _withoutInDateDao.GetAllAsync()).ToDictionary(item => item.Id, item => item.Unit);

                var profileInfoes = await _profileDao.GetProfileInfoesAsync(o365TenantId, RMDiscoveryProfileType.Inactive);
                return profileInfoes.ConvertAll(item => new RMDiscoveryProfileDataInfo
                {
                    Id = item.Id,
                    O365TenantId = o365TenantId,
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
                _logger.Error($"An error occurred while get o365 tenant [{o365TenantId}] inactive profile infoes. Error: {e}");
                return [];
            }
        }


        [AsyncAudit(Module = AuditModule.Discovery, Category = AuditCategory.InactiveData, Action = AuditAction.AddInactiveProfileInfo, IAsyncBeforeHandler = typeof(RMDiscoveryOffice365ConfigurationBeforeAuditHandler), IAsyncAfterHandler = typeof(RMDiscoveryOffice365ConfigurationAfterAuditHandler))]
        public async Task<RAReturnMessage> AddInactiveProfileInfoAsync(RMDiscoveryProfileDataInfo dataInfo)
        {
            try
            {
                await CheckDiscoveryJobInfoAsync(false);
                var checker = new RMDiscoveryProfileChecker(dataInfo, RMDiscoveryProfileType.Inactive);
                var (succeed, failedMessage) = await checker.AddCheckAsync();
                if (!succeed)
                {
                    return failedMessage;
                }

                var profileInfo = new RMDiscoveryOffice365ProfileInfo
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

                await _profileDao.AddOrUpdateProfileInfoAsync(dataInfo.O365TenantId, profileInfo);

                var res = SendProfileJob(JobRunBy.Control, new RMDiscoveryProfileJobDefinition
                {
                    RunMode = RMDiscoveryJobRunMode.Specify,
                    JobType = RMDiscoveryJobType.Newly,
                    O365TenantId = dataInfo.O365TenantId,
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
                _logger.Error($"An error occurred while add o365 tenant [{dataInfo.O365TenantId}] inactive profile info. Error: {e}");
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = I18NEntity.GetString("RM_HS_Criteria_View_Msg_ValidOtherError")
                };
            }
        }

        [AsyncAudit(Module = AuditModule.Discovery, Category = AuditCategory.InactiveData, Action = AuditAction.UpdateInactiveProfileInfo, IAsyncBeforeHandler = typeof(RMDiscoveryOffice365ConfigurationBeforeAuditHandler), IAsyncAfterHandler = typeof(RMDiscoveryOffice365ConfigurationAfterAuditHandler))]
        public async Task<RAReturnMessage> UpdateInactiveProfileInfoAsync(RMDiscoveryProfileDataInfo dataInfo)
        {
            try
            {
                await CheckDiscoveryJobInfoAsync(false);
                var checker = new RMDiscoveryProfileChecker(dataInfo, RMDiscoveryProfileType.Inactive);
                var (succeed, failedMessage) = await checker.UpdateCheckAsync();
                if (!succeed)
                {
                    return failedMessage;
                }

                var profileInfo = await _profileDao.GetProfileInfoByIdAsync(dataInfo.O365TenantId, dataInfo.Id);
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

                await _profileDao.AddOrUpdateProfileInfoAsync(dataInfo.O365TenantId, profileInfo);

                var res = SendProfileJob(JobRunBy.Control, new RMDiscoveryProfileJobDefinition
                {
                    RunMode = RMDiscoveryJobRunMode.Specify,
                    JobType = RMDiscoveryJobType.Newly,
                    O365TenantId = dataInfo.O365TenantId,
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
                _logger.Error($"An error occurred while update o365 tenant [{dataInfo.O365TenantId}] inactive profile info. Error: {e}");
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = I18NEntity.GetString("RM_HS_Criteria_View_Msg_ValidOtherError")
                };
            }
        }

        [AsyncAudit(Module = AuditModule.Discovery, Category = AuditCategory.InactiveData, Action = AuditAction.DeleteInactiveProfileInfo, IAsyncBeforeHandler = typeof(RMDiscoveryOffice365ConfigurationBeforeAuditHandler), IAsyncAfterHandler = typeof(RMDiscoveryOffice365ConfigurationAfterAuditHandler))]
        public async Task<RAReturnMessage> DeleteInactiveProfileInfoAsync(RMDiscoveryProfileDataInfo dataInfo)
        {
            try
            {
                var checker = new RMDiscoveryProfileChecker(dataInfo, RMDiscoveryProfileType.Inactive);
                var (succeed, failedMessage) = await checker.DeleteCheckAsync();
                if (!succeed)
                {
                    return failedMessage;
                }

                await _profileDao.DeleteProfileFailedInfoesAsync(dataInfo.O365TenantId, dataInfo.Id);
                await _profileDao.DeleteProfileInfoAsync(dataInfo.O365TenantId, dataInfo.Id);
                await RMDiscoveryDBManager.DropOffice365InactiveProfileTablsAsync(dataInfo.O365TenantId, dataInfo.Id);
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Successful
                };
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while delete o365 tenant [{dataInfo.O365TenantId}] inactive profile [{dataInfo.Id}] info. Error: {e}");
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = I18NEntity.GetString("RM_HS_Criteria_View_Msg_ValidOtherError")
                };
            }
        }
        #endregion

        #region Rot
        public async Task<List<RMDiscoveryProfileDataInfo>> GetRotProfileInfoesAsync(Guid o365TenantId)
        {
            try
            {
                var res = new List<RMDiscoveryProfileDataInfo>();

                var ruleInfoes = await _ruleInfoDao.GetRuleInfoesAsync(true, RMDiscoveryRuleDefinitionKind.ROT);
                var sizeRanges = (await _sizeRangeDao.GetAllAsync()).Concat(new List<RMDiscoveryOffice365SizeRange> { new RMDiscoveryOffice365SizeRange { Id = -1, DisplayName = I18NEntity.GetString("RM_FA_Inactive_OptimizationTab_FileSizeRangeAll") } }).ToDictionary(item => item.Id, item => item.DisplayName);
                var fileTypes = (await _fileExtensionDao.GetAllAsync(o365TenantId)).ToDictionary(item => item.Id, item => I18NEntity.GetString(item.Name));
                var dateRanges = (await _withoutInDateDao.GetAllAsync()).ToDictionary(item => item.Id, item => item.Unit);

                var profileInfoes = await _profileDao.GetProfileInfoesAsync(o365TenantId, RMDiscoveryProfileType.ROT);
                foreach (var item in profileInfoes)
                {
                    var profileDataInfo = new RMDiscoveryProfileDataInfo
                    {
                        Id = item.Id,
                        O365TenantId = o365TenantId,
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
                        profileDataInfo.AvaliableRuleCategories = new List<RMDiscoveryRuleCategory> {
                            RMDiscoveryRuleCategory.Redundant,
                            RMDiscoveryRuleCategory.Obsolete,
                            RMDiscoveryRuleCategory.Trivial
                        };
                        profileDataInfo.RuleInfoesLabel = I18NEntity.GetString("RM_FA_Inactive_OptimizationTab_FileSizeRangeAll");
                        continue;
                    }

                    var profileRuleInfoes = ruleInfoes.Where(item => profileDataInfo.RuleIds.Contains(item.Id)).ToList();
                    var profileCustomColumns = profileRuleInfoes
                        .ConvertAll(item => new RMDiscoveryTableColumnInfo(item.Name, item.ToCustomColumn().Name, item.Id));
                    profileDataInfo.CustomColumns = profileCustomColumns;
                    profileDataInfo.AvaliableRuleCategories = profileRuleInfoes.Select(item => item.Category).ToHashSet().ToList();
                    profileDataInfo.RuleInfoesLabel = string.Join(", ", profileRuleInfoes.Select(item => item.Name));
                }

                return res;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while get o365 tenant [{o365TenantId}] rot profile infoes. Error: {e}");
                return [];
            }
        }

        [AsyncAudit(Module = AuditModule.Discovery, Category = AuditCategory.ROTData, Action = AuditAction.AddRotProfileInfo, IAsyncBeforeHandler = typeof(RMDiscoveryOffice365ConfigurationBeforeAuditHandler), IAsyncAfterHandler = typeof(RMDiscoveryOffice365ConfigurationAfterAuditHandler))]
        public async Task<RAReturnMessage> AddRotProfileInfoAsync(RMDiscoveryProfileDataInfo dataInfo)
        {
            try
            {
                await CheckDiscoveryJobInfoAsync(true);
                var checker = new RMDiscoveryProfileChecker(dataInfo, RMDiscoveryProfileType.ROT);
                var (succeed, failedMessage) = await checker.AddCheckAsync();
                if (!succeed)
                {
                    return failedMessage;
                }

                var profileInfo = new RMDiscoveryOffice365ProfileInfo
                {
                    Id = Guid.NewGuid(),
                    Name = dataInfo.Name,
                    SizeRange = -1,
                    SizeRangeQueryMode = RMDiscoverySizeRangeQueryMode.GenerateThanEqual,
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

                await _profileDao.AddOrUpdateProfileInfoAsync(dataInfo.O365TenantId, profileInfo);

                var res = SendProfileJob(JobRunBy.Control, new RMDiscoveryProfileJobDefinition
                {
                    RunMode = RMDiscoveryJobRunMode.Specify,
                    JobType = RMDiscoveryJobType.Newly,
                    O365TenantId = dataInfo.O365TenantId,
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
                _logger.Error($"An error occurred while add o365 tenant [{dataInfo.O365TenantId}] rot profile info. Error: {e}");
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = I18NEntity.GetString("RM_HS_Criteria_View_Msg_ValidOtherError")
                };
            }
        }

        [AsyncAudit(Module = AuditModule.Discovery, Category = AuditCategory.ROTData, Action = AuditAction.UpdateRotProfileInfo, IAsyncBeforeHandler = typeof(RMDiscoveryOffice365ConfigurationBeforeAuditHandler), IAsyncAfterHandler = typeof(RMDiscoveryOffice365ConfigurationAfterAuditHandler))]
        public async Task<RAReturnMessage> UpdateRotProfileInfoAsync(RMDiscoveryProfileDataInfo dataInfo)
        {
            try
            {
                await CheckDiscoveryJobInfoAsync(true);
                var checker = new RMDiscoveryProfileChecker(dataInfo, RMDiscoveryProfileType.ROT);
                var (succeed, failedMessage) = await checker.UpdateCheckAsync();
                if (!succeed)
                {
                    return failedMessage;
                }

                var profileInfo = await _profileDao.GetProfileInfoByIdAsync(dataInfo.O365TenantId, dataInfo.Id);
                profileInfo.Name = dataInfo.Name;
                profileInfo.SizeRange = -1;
                profileInfo.SizeRangeQueryMode = RMDiscoverySizeRangeQueryMode.GenerateThanEqual;
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

                await _profileDao.AddOrUpdateProfileInfoAsync(dataInfo.O365TenantId, profileInfo);

                var res = SendProfileJob(JobRunBy.Control, new RMDiscoveryProfileJobDefinition
                {
                    RunMode = RMDiscoveryJobRunMode.Specify,
                    JobType = RMDiscoveryJobType.Newly,
                    O365TenantId = dataInfo.O365TenantId,
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
                _logger.Error($"An error occurred while update o365 tenant [{dataInfo.O365TenantId}] rot profile info. Error: {e}");
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = I18NEntity.GetString("RM_HS_Criteria_View_Msg_ValidOtherError")
                };
            }
        }

        [AsyncAudit(Module = AuditModule.Discovery, Category = AuditCategory.ROTData, Action = AuditAction.DeleteRotProfileInfo, IAsyncBeforeHandler = typeof(RMDiscoveryOffice365ConfigurationBeforeAuditHandler), IAsyncAfterHandler = typeof(RMDiscoveryOffice365ConfigurationAfterAuditHandler))]
        public async Task<RAReturnMessage> DeleteRotProfileInfoAsync(RMDiscoveryProfileDataInfo dataInfo)
        {
            try
            {
                var checker = new RMDiscoveryProfileChecker(dataInfo, RMDiscoveryProfileType.ROT);
                var (succeed, failedMessage) = await checker.DeleteCheckAsync();
                if (!succeed)
                {
                    return failedMessage;
                }

                await _profileDao.DeleteProfileFailedInfoesAsync(dataInfo.O365TenantId, dataInfo.Id);
                await _profileDao.DeleteProfileInfoAsync(dataInfo.O365TenantId, dataInfo.Id);
                await RMDiscoveryDBManager.DropOffice365RotProfileTablsAsync(dataInfo.O365TenantId, dataInfo.Id);
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Successful
                };
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while delete o365 tenant [{dataInfo.O365TenantId}] rot profile [{dataInfo.Id}] info. Error: {e}");
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = I18NEntity.GetString("RM_HS_Criteria_View_Msg_ValidOtherError")
                };
            }
        }
        #endregion

        public bool SendProfileJob(JobRunBy runBy, RMDiscoveryProfileJobDefinition definition)
        {
            try
            {
                var jobQueueDto = new JobQueueDto
                {
                    JobType = JobType.DiscoveryProfileJob,
                    JobRunType = runBy,
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    JobRunByUser = runBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule",
                    Parameters = JsonConvert.SerializeObject(definition)
                };

                _jobQueueService.AddToDBJobQueue(jobQueueDto);
                _logger.Info($"Successful send profile job [{definition.RunMode}].");
                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while send profile job [{definition.RunMode}]. Error: {e}");
                return false;
            }
        }

        public string RealRunProfileJob(JobQueueDto queueDto)
        {
            try
            {
                var jobId = _jobMonitorService.CreateJob(JobType.DiscoveryProfileJob, queueDto.JobRunByUser);

                _jobQueueService.HandleMessage(new JobQueueMessage
                {
                    JobId = jobId,
                    JobType = JobType.DiscoveryProfileJob,
                    CommandLine = $"{JobType.DiscoveryProfileJob} {jobId}",
                    Extension = queueDto.Parameters
                });

                _logger.Info($"Successful real run profile job: [{queueDto.Parameters}].");

                return jobId;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while real run profile job: [{queueDto.Parameters}]. Error: {e}");
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
                throw new Exception("The discovery job is running. Didn't start profile job.");
            }

            if (!jobInfo.Version.IsOffice365NewVersion())
            {
                throw new Exception("The discovery job is not version 3.");
            }

            if (checkRot && !jobInfo.EnableRot)
            {
                throw new Exception("The discovery configuration not enable rot.");
            }
        }

        [AsyncAudit(Module = AuditModule.Discovery, Category = AuditCategory.DiscoveryDataOptimization, Action = AuditAction.ExportO365Profile, IAsyncBeforeHandler = typeof(RMDiscoveryOffice365ConfigurationBeforeAuditHandler), IAsyncAfterHandler = typeof(RMDiscoveryOffice365ConfigurationAfterAuditHandler))]
        public RAReturnMessage RunExportProfileDiscoveryDataAnalysisForOffice365Job(DiscoveryO365DataAnalysis o365DataAnalysis)
        {
            _logger.Debug("start run export discovery data analysis");
            string id = string.Empty;
            RAReturnMessage msg = new RAReturnMessage();
            try
            {
                var dto = new JobQueueDto
                {
                    JobType = JobType.DiscoveryExportO365Profile,
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    JobRunByUser = TenantLocalValue.LogonUserEmail,
                    Parameters = JsonConvert.SerializeObject(o365DataAnalysis)
                };
                var jobId = _jobQueueService.AddToDBJobQueue(dto);
                if (string.IsNullOrEmpty(jobId))
                {
                    _logger.Error("Failed to add the run export discovery data analysis to the job queue. Job ID is null or empty.");
                    msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
                else
                {
                    _logger.Info($"Successfully added the run export discovery data analysis [{jobId}] to the job queue.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error("error occurred while running export discovery data analysis job, ERROR:{0}", ex.ToString());
            }
            return msg;
        }

        public async Task<string> RealRunExportProfileDiscoveryDataAnalysisForOffice365Job(JobRunBy jobRunBy, string jobRunByUser, string parameters)
        {
            string jobId = string.Empty;
            JobType jobType = JobType.DiscoveryExportO365Profile;
            DiscoveryO365DataAnalysis o365DataAnalysis = JsonConvert.DeserializeObject<DiscoveryO365DataAnalysis>(parameters);
            string o365TenantId = o365DataAnalysis.TenantId;
            string profileType = o365DataAnalysis.ProfileType.ToString();
            string profileId = o365DataAnalysis.ProfileId.ToString();
            bool isDescending = o365DataAnalysis.IsDesc;
            List<BaseJobDto> exportJobs = _jobMonitorService.GetRunningJobs([jobType]);
            var account = await AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail);

            bool isSkip = false;
            if (exportJobs != null && exportJobs.Count > 0)
            {
                var otherExportJobs = exportJobs.Where(j => !j.Id.Equals(jobId)).ToList();
                if (otherExportJobs != null && otherExportJobs.Count > 0)
                {
                    isSkip = true;
                }
            }
            jobId = _jobMonitorService.CreateJob(jobType, jobRunByUser);
            if (!isSkip)
            {
                StartExportDiscoveryDataAnalysisProfile(profileId, profileType, o365TenantId, jobId, jobType, account.UserId, isDescending);
                _logger.Info("Begin control export discovery data analysis job. JobId:{0}, JobType: {1}", jobId, jobType);
            }
            else
            {
                _jobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_DiscoveryExportO365Profile_JobSkip");
                _logger.Info(I18NEntity.GetString("RM_DiscoveryExportO365Profile_JobSkip"));
            }
            return jobId;
        }

        public void StartExportDiscoveryDataAnalysisProfile(string profileId, string discoveryType, string o365TenantId, string jobId, JobType jobType, string userId, bool isDescending)
        {
            DownloadDataInfoDao.Create(new RMDownloadDataInfo()
            {
                FileDownloadTime = DateTime.UtcNow.Ticks,
                JobId = jobId,
                RecordsId = Guid.NewGuid(),
                JobStatus = (int)DownloadContentJobStatus.Wait,
                UserId = userId,
                Name = jobId + ".zip",
                DownloadType = DownloadContentType.ExportDiscoveryProfile,
            });

            _jobQueueService.HandleMessage(new JobQueueMessage()
            {
                JobId = jobId,
                JobType = jobType,
                CommandLine = string.Format("{0} {1} {2} {3} {4} {5}", jobType, jobId, o365TenantId, discoveryType, profileId, isDescending),
            });
        }

        public async Task GenerateExportProfileAsync(ExportDiscoveryProfileParam exportParam, RMDiscoveryProfileDataInfo profile)
        {
            _logger.Info($"[Export] Begin Generate Export Profile.");
            string reportFilePath = exportParam.FolderPath + Path.DirectorySeparatorChar + exportParam.FileName + ".CSV";

            if (!Directory.Exists(exportParam.FolderPath))
            {
                Directory.CreateDirectory(exportParam.FolderPath);
            }

            using (RA.Common.PerformanceScope scope = new RA.Common.PerformanceScope("discoverymanagement.exportprofiles"))
            {
                var discoveryType = RA.Common.Extension.EnumExtension.ToEnum<RMDiscoveryProfileType>(exportParam.DiscoveryType);
                var ruleKind = ConverDiscoveryTypeToRuleDefinitionKind(exportParam.DiscoveryType);
                List<RMDiscoveryOffice365RuleInfo> scanRules = new();
                scanRules = await _ruleInfoDao.GetRuleInfoesAsync(true, ruleKind);
                if (ruleKind == RMDiscoveryRuleDefinitionKind.ROT)
                {
                    var ruleIds = new HashSet<int>(profile.RuleIds);
                    scanRules = scanRules.Where(item => ruleIds.Contains(item.Id)).ToList();
                }
                _logger.Info($"[Export] Get inactive rules successfully");
                var ruleColumnIds = scanRules?.Select(item => item.ToCustomColumn().Name).ToList();
                var ruleDisplayMappings = scanRules.ToDictionary(r => r.ToCustomColumn().Name, r => r.Name);

                var costConfig = await GetOffice365CostSavingInfoAsync();

                var exportDataQuerier = new RMDiscoveryExportDataQuerier(exportParam);
                var columnOrder = ReportUtil.GetColumnOrder(discoveryType, ruleColumnIds, ruleDisplayMappings);
                ReportUtil.WriteCsvHeader(reportFilePath, columnOrder);

                long totalCount = 0;
                while (true)
                {
                    using CheckJobStopScope jobStopScope = new();
                    using PerformanceScope _ = new("discoverymanagement.exportprofiles.querydata");
                    var siteItems = await exportDataQuerier.QueryExportDataAsync(ruleColumnIds);
                    if (siteItems == null || siteItems.Items.Count == 0)
                        break;

                    totalCount += siteItems.Items.Count;
                    _logger.Info($"[Export] Query export data successfully, current page: {exportParam.PageIndex + 1}, totalCount: {totalCount}");

                    List<long> allInScopeSiteIds = [];
                    int retryTimes = 0, maxRetryTimes = 5;
                    while (retryTimes < maxRetryTimes)
                    {
                        try
                        {
                            allInScopeSiteIds = await _siteOptimizationMappingTableDao.GetAllInScopeSiteIds(exportParam.O365TenantId, siteItems.Items.Select(item => Convert.ToInt64(item["Id"])));
                            break;
                        }
                        catch (Exception ex)
                        {
                            retryTimes++;
                            _logger.Error($"[Export] Get all in-scope site IDs failed. Error: {ex}");
                            if (retryTimes >= maxRetryTimes)
                            {
                                throw new Exception($"[Export] Failed to get all in-scope site IDs after {maxRetryTimes} attempts.", ex);
                            }
                            _logger.Info($"[Export] Retrying to get all in-scope site IDs. Attempt {retryTimes} of {maxRetryTimes}.");
                            await Task.Delay(2000 * retryTimes);
                        }
                    }
                    _logger.Info($"[Export] Get all in-scope site IDs successfully, count: {allInScopeSiteIds.Count}");

                    foreach (var site in siteItems.Items)
                    {
                        site["InScope"] = await GetInScopeAsync(site, allInScopeSiteIds);

                        if (discoveryType == RMDiscoveryProfileType.Inactive)
                        {
                            CalculateInactiveCostSaving(site, costConfig);
                        }
                        else
                        {
                            CalculateRotCostSaving(site, costConfig);
                        }
                    }

                    ReportUtil.AppendDiscoveryDataToCsv(reportFilePath, siteItems.Items, ruleColumnIds, ruleDisplayMappings, discoveryType);

                    exportParam.PageIndex++;
                }
            }
        }

        public async Task<RMDiscoveryProfileDataInfo> GetProfileInfoByIdAsync(Guid o365TenantId, Guid profileId, string discoveryType)
        {
            using (RA.Common.PerformanceScope scope = new RA.Common.PerformanceScope("discoverymanagement.getprofiles"))
            {
                var type = RA.Common.Extension.EnumExtension.ToEnum<RMDiscoveryProfileType>(discoveryType);
                var profileInfo = await _profileDao.GetProfileInfoByIdAsync(o365TenantId, profileId);
                var dateRanges = (await _withoutInDateDao.GetAllAsync()).ToDictionary(item => item.Id, item => item.Unit);
                var fileTypes = (await _fileExtensionDao.GetAllAsync(o365TenantId)).ToDictionary(item => item.Id, item => I18NEntity.GetString(item.Name));

                var dataInfo = new RMDiscoveryProfileDataInfo
                {
                    Id = profileInfo.Id,
                    Name = profileInfo.Name,
                    ModifiedTimeRangeLabel = $"RM_FA_Inactive_SummaryTab_ModifiedFrom {(profileInfo.GreaterThanEqualWithoutInDate == -1 ? "RM_FA_Inactive_ModifiedOption_Latest"
                        : dateRanges[profileInfo.GreaterThanEqualWithoutInDate] + " " + "RM_JS_RDM_CreateRule_Unit_Months")} RM_FA_Inactive_SummaryTab_ModifiedTo {(profileInfo.LessThanEqualWithoutInDate == 999 ? "RM_FA_Inactive_ModifiedOption_Max"
                        : dateRanges[profileInfo.LessThanEqualWithoutInDate] + " " + "RM_JS_RDM_CreateRule_Unit_Months")}",
                    FileTypeLabel = JsonConvert.DeserializeObject<List<int>>(profileInfo.FileExtensionIdsJson).Count == 0 ? "RM_FA_Inactive_OptimizationTab_FileSizeRangeAll" : string.Join(", ", JsonConvert.DeserializeObject<List<int>>(profileInfo.FileExtensionIdsJson).ConvertAll(i => fileTypes[i])),
                    RuleIds = JsonConvert.DeserializeObject<List<int>>(profileInfo.RuleIdsJson),
                    IsBuildIn = profileInfo.IsBuildIn,
                    SortBy = profileInfo.SortBy,
                };

                if (type == RMDiscoveryProfileType.ROT)
                {
                    if (dataInfo.IsBuildIn)
                    {
                        dataInfo.AvaliableRuleCategories = new List<RMDiscoveryRuleCategory> {
                            RMDiscoveryRuleCategory.Redundant,
                            RMDiscoveryRuleCategory.Obsolete,
                            RMDiscoveryRuleCategory.Trivial
                        };
                        dataInfo.RuleInfoesLabel = "RM_FA_Inactive_OptimizationTab_FileSizeRangeAll";
                        return dataInfo;
                    }                  
                    var ruleInfoes = await _ruleInfoDao.GetRuleInfoesAsync(true, RMDiscoveryRuleDefinitionKind.ROT);
                    var profileRuleInfoes = ruleInfoes.Where(item => dataInfo.RuleIds.Contains(item.Id)).ToList();
                    dataInfo.RuleInfoesLabel = string.Join(", ", profileRuleInfoes.Select(item => item.Name));
                }
                else
                {
                    var sizeRanges = (await _sizeRangeDao.GetAllAsync()).Concat(new List<RMDiscoveryOffice365SizeRange> { new() 
                        { Id = -1, 
                          DisplayName = "RM_FA_Inactive_OptimizationTab_FileSizeRangeAll"
                        } 
                    }).ToDictionary(item => item.Id, item => item.DisplayName);
                    dataInfo.SizeRangeLabel = profileInfo.SizeRange == -1 ? "RM_FA_Inactive_OptimizationTab_FileSizeRangeAll" : sizeRanges[profileInfo.SizeRange];
                }

                return dataInfo;
            }
        }

        private RMDiscoveryRuleDefinitionKind ConverDiscoveryTypeToRuleDefinitionKind (string discoveryType)
        {
            var profileType = AvePoint.RA.Common.Extension.EnumExtension.ToEnum<RMDiscoveryProfileType>(discoveryType);
            return profileType switch
            {
                RMDiscoveryProfileType.None => RMDiscoveryRuleDefinitionKind.None,
                RMDiscoveryProfileType.Inactive => RMDiscoveryRuleDefinitionKind.Inactive,
                RMDiscoveryProfileType.ROT => RMDiscoveryRuleDefinitionKind.ROT,
                _ => throw new ArgumentOutOfRangeException(nameof(discoveryType), $"Unknown DiscoveryType: {discoveryType}")
            };
        }

        private async Task<string> GetInScopeAsync(Dictionary<string, object> obj, List<long> allInScopeSiteIds)
        {
            if (allInScopeSiteIds.Contains(Convert.ToInt64(obj["Id"]))) return I18NEntity.GetString("RM_JS_Common_Yes");

            return I18NEntity.GetString("RM_JS_Common_No");
        }

        private void CalculateRotCostSaving(Dictionary<string, object> site, RMDiscoveryOffice365CostSavingInfo costConfig)
        {
            var contentSource = Convert.ToInt32(site.GetValueOrDefault("ContentSource") ?? 0);
            var rate = GetRateByContentSource(contentSource, costConfig);

            var redundantGB = GetDiscoveryDisplaySizeInGb(site.GetValueOrDefault("RCategoryFileTotalSize"));
            var obsoleteGB = GetDiscoveryDisplaySizeInGb(site.GetValueOrDefault("OCategoryFileTotalSize"));
            var trivialGB = GetDiscoveryDisplaySizeInGb(site.GetValueOrDefault("TCategoryFileTotalSize"));

            var totalGB = redundantGB + obsoleteGB + trivialGB;

            site["CostSavingMonthlyByRedundant"] = CalculateCostSaving(site.GetValueOrDefault("RCategoryFileTotalSize"), rate).ToString();
            site["CostSavingMonthlyByObsolete"] = CalculateCostSaving(site.GetValueOrDefault("OCategoryFileTotalSize"), rate).ToString();
            site["CostSavingMonthlyByTrivial"] = CalculateCostSaving(site.GetValueOrDefault("TCategoryFileTotalSize"), rate).ToString();
            site["CostSavingMonthlyBySize"] = CalculateCostSaving(totalGB, rate).ToString();
        }

        private void CalculateInactiveCostSaving(Dictionary<string, object> site, RMDiscoveryOffice365CostSavingInfo costConfig)
        {
            var contentSource = Convert.ToInt32(site.GetValueOrDefault("ContentSource") ?? 0);
            var rate = GetRateByContentSource(contentSource, costConfig);
            var inactiveGB = GetDiscoveryDisplaySizeInGb(site.GetValueOrDefault("InactiveFileTotalSize"));
            site["CostSaving"] = CalculateCostSaving(inactiveGB, rate).ToString();
        }

        private async Task<RMDiscoveryOffice365CostSavingInfo> GetOffice365CostSavingInfoAsync()
        {
            return await _office365ConfigurationService.GetCostSavingInfoAsync();
        }

        private double GetRateByContentSource(int contentSource, RMDiscoveryOffice365CostSavingInfo config)
        {
            return contentSource switch
            {
                (int)SourceFlag.SharePoint => Math.Max(config.SPStoragePrice - config.ArchivedDataStoragePrice, 0),
                (int)SourceFlag.OneDrive => Math.Max(config.ODStoragePrice - config.ArchivedDataStoragePrice, 0),
                _ => 0
            };
        }

        private double CalculateCostSaving(double sizeInGB, double rate)
        {
            return Math.Round(sizeInGB * rate, 0, MidpointRounding.AwayFromZero);
        }

        private double CalculateCostSaving(object bytes, double rate)
        {
            return CalculateCostSaving(GetDiscoveryDisplaySizeInGb(bytes), rate);
        }

        private double GetDiscoveryDisplaySizeInGb(object bytes)
        {
            if (!double.TryParse(bytes?.ToString(), out var byteValue))
            {
                return 0;
            }

            var sizeInGb = byteValue / 1024d / 1024d / 1024d;
            if (Math.Abs(sizeInGb % 1) < 1E-06)
            {
                return sizeInGb;
            }

            var roundedSize = Math.Round(sizeInGb, 2, MidpointRounding.AwayFromZero);
            return sizeInGb > 0 && roundedSize == 0 ? 0.01d : roundedSize;
        }
    }
}
