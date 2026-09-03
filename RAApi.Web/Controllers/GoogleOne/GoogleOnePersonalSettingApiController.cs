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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Api.Contract;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.PersonalSetting;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using Cloud.sdk.Data.Opus.GoogleOne.Common;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Api.Web.Controllers.GoogleOne
{
    [Route("api/googleone/personalsettings")]
    public class GoogleOnePersonalSettingApiController : GoogleOneApiBaseController
    {
        private IRALogger _logger = RALogger.GetInstance(typeof(GoogleOnePersonalSettingApiController));
        private IPersonalSettingService PersonalSettingService => PlatformWindsorManager.GetService<IPersonalSettingService>();
        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();
        private IExplorerService ExplorerService => PlatformWindsorManager.GetService<IExplorerService>();
        private IRMReportService ReportService => PlatformWindsorManager.GetService<IRMReportService>();

        private readonly ITaxonomyService _taxonomyService = PlatformWindsorManager.GetService<ITaxonomyService>();

        private IUserService UserService => PlatformWindsorManager.GetService<IUserService>();


        [HttpGet("globalsearch/criteria/getall")]
        public async Task<List<RMExplorerSearchCriteriaDto>> GetAllGlobalSearchCriteria()
        {
            var result = new List<RMExplorerSearchCriteriaDto>();
            var currentUser = TenantLocalValue.LogonUserId;
            var type = PersonalSettingType.GoogleGlobalSearchCriteria;
            try
            {
                //if there is no built-in setting, create it.
                var existBuiltIn = PersonalSettingService.ExistsBuiltIn(new RMPersonalSettingDto { Owner = currentUser, Type = type });
                if (!existBuiltIn)
                {
                    var builtInDto = RMExplorerSearchCriteriaDto.GetBuiltInSetting(PersonalSettingType.GoogleGlobalSearchCriteria);
                    PersonalSettingService.Save(builtInDto.Convert2PersonalSetting());
                }
                PersonalSettingService.UpgradeDefaultSetting(currentUser, type); //upgrade default setting if needed.
                result.AddRange(await GetAllSearchCriteria());

                //if there is no default search criteria, then set the built-in as default
                if (!result.Exists(o => o.IsDefault))
                {
                    var builtIn = result.FirstOrDefault(o => o.IsBuiltIn);
                    if (builtIn != null) builtIn.IsDefault = true;
                }
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while get all global search criteria, user id : {currentUser}. Error: {e.ToString()}");
            }

            return result;
        }

        [HttpPost("globalsearch/criteria/profile/get")]
        public async Task<RMExplorerSearchCriteriaDto> GetGlobalSearchCriteriaByProfileId([FromBody] int id)
        {
            RMExplorerSearchCriteriaDto result = null;
            try
            {
                if (id == RMGlobalSearchDefautSettingId.DelayedLoan)
                {
                    return await GetDefaultDelayedLoanSettingAsync();
                }
                //var personalSetting = PersonalSettingService.GetByOwnerAndId(TenantLocalValue.LogonUserId, id);
                //result = personalSetting?.Convert2GlobalSearchCriteria();
                result = await GetGlobalSearchCriteriaByIdAsync(id);
                if (!CheckOwner(result.Owner))
                {
                    var isShare2CurrentUser = PersonalSettingService.IsSharedToUser(TenantLocalValue.LogonUserId, result.Id);
                    if (!isShare2CurrentUser)
                    {
                        _logger.Warn($"Can't get the setting with id {id} because it isn't shared to user {TenantLocalValue.LogonUserId}");
                        return null;
                    }
                }
                AssembleTree(result);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while get global search criteria by id : {id}, user id : {TenantLocalValue.LogonUserId}. Error: {e.ToString()}");
            }

            return result;
        }

        [HttpPost("globalsearch/criteria/save")]
        public async Task<string> SaveGlobalSearchCriteria([FromBody] RMExplorerSearchCriteriaDto dto)
        {
            var result = new RMPersonalSettingSaveResult();
            try
            {
                if (dto.Id > 0) //if is a saved setting, should validate if current user is the owner, only owner can edit it.
                {
                    var old = await GetGlobalSearchCriteriaByIdAsync(dto.Id);
                    ValidateOwner(old.Owner);
                }
                dto.Validate();
                dto.Owner = TenantLocalValue.LogonUserId;
                dto.Type = PersonalSettingType.GoogleGlobalSearchCriteria;
                if (dto.IsBuiltIn)
                {
                    dto.Setting = new RMExplorerSearchCriteriaSetting();
                }
                AssembleGoogleTree(dto);
                result.Id = PersonalSettingService.Save(dto.Convert2PersonalSetting());
            }
            catch (SameNameException)
            {
                result.ErrorCode = RMPersonalSettingSaveResultErrorCode.SameName;
                _logger.Error($"A global search criteria setting with same name '{dto.Name}' already exists");
                return I18NEntity.GetString("RM_HS_Criteria_View_Msg_ValidDuplicateViewName");
            }
            catch (NoPermissionException)
            {
                result.ErrorCode = RMPersonalSettingSaveResultErrorCode.NoPermission;
                _logger.Error($"Can't save search criteria because the current user {TenantLocalValue.LogonUserId} is not the owner of setting with id {dto.Id}");
                return I18NEntity.GetString("RM_AR_Storage_Unknow_ErrorMessage");
            }
            catch (Exception e)
            {
                result.ErrorCode = RMPersonalSettingSaveResultErrorCode.Other;
                _logger.Error($"An error occurred while saving global search criteria. Error: {e.ToString()}");
                return I18NEntity.GetString("RM_AR_Storage_Unknow_ErrorMessage");
            }

            return JsonConvert.SerializeObject(result);
        }

        private async Task<List<RMExplorerSearchCriteriaDto>> GetAllSearchCriteria()
        {
            var result = new List<RMExplorerSearchCriteriaDto>();
            var currentUser = TenantLocalValue.LogonUserId;
            var type = PersonalSettingType.GoogleGlobalSearchCriteria;
            //get self created settings
            var personalSettings = PersonalSettingService.GetByOwnerAndTypeForGoogleOne(currentUser, type);
            if (personalSettings != null)
            {
                var test = new RMPersonalSettingDto();
                result.AddRange((await Task.WhenAll(personalSettings.Select(o => Convert2GlobalSearchCriteriaAllProfileForGoogleOne(o))))
                 .Where(x => x != null));
            }
            return result;
        }

        private async Task<RMExplorerSearchCriteriaDto> Convert2GlobalSearchCriteriaForGoogleOne(RMPersonalSettingDto dto, bool isSharedBy = false)
        {
            RMExplorerSearchCriteriaDto result = new RMExplorerSearchCriteriaDto
            {
                Id = dto.Id,
                Name = dto.IsBuiltIn ? I18NEntity.GetString(dto.Name) : dto.Name,
                Type = dto.Type,
                IsDefault = dto.IsDefault,
                IsBuiltIn = dto.IsBuiltIn,
                IsSharedBy = isSharedBy,
                Owner = dto.Owner,
                Setting = (dto.ContentStr != null && dto.Type == PersonalSettingType.GoogleGlobalSearchCriteria) ? JsonConvert.DeserializeObject<RMExplorerSearchCriteriaSetting>(dto.ContentStr) : null
            };
            if (result.Setting != null && result.Setting.AdvancedSearchs != null)
            {
                try
                {
                    foreach (var setting in result.Setting.AdvancedSearchs)
                    {
                        if (!string.IsNullOrEmpty(setting.ContentStr))
                        {

                            ExplorerSearchOptionV3 searchOption = SerializerHelper.DeserializeByJsonConvert<ExplorerSearchOptionV3>(setting.ContentStr);
                            if (searchOption != null && (searchOption.ColumnOperationLogic == ExplorerSearchColumnOperationLogic.Contains))
                            {
                                var originString = searchOption.Value.Replace("\"", "");
                                if (searchOption.Value.Contains("*") && (originString.Split("*").Length - 1 != originString.Replace("\"", "").Length))
                                {
                                    result.IsOffline = true;
                                    List<JMItemInfo> jms = new List<JMItemInfo>();
                                    result.OfflineJobs = new List<OfflineJobInfo>();
                                    jms = await JobMonitorService.GetEndedJobByScopeIdAsync(result.Id.ToString(), new int[] { 0, 1, 2, 4 }, TenantLocalValue.LogonUserId);
                                    foreach (var jm in jms)
                                    {

                                        OfflineJobInfo info = new OfflineJobInfo();
                                        info.JobId = jm.JobId;
                                        info.StartTime = jm.StartTime;
                                        result.OfflineJobs.Add(info);
                                    }

                                }
                            }
                        }                 
                    }
                }
                catch
                {
                    result.IsOffline = false;
                }
            }
            return result;
        }

        [HttpPost("globalsearch/criteria/profile/delete")]
        public async Task<bool> Delete([FromBody] int id)
        {
            try
            {
                return await PersonalSettingService.DeleteAsync(new RMPersonalSettingDto { Owner = TenantLocalValue.LogonUserId, Id = id, Type = PersonalSettingType.GlobalSearchCriteria });
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while delete personal setting, user id : {TenantLocalValue.LogonUserId}. Error: {e.ToString()}");
                return false;
            }
        }

        [HttpPost("globalsearch/criteria/profile/setdefault")]
        public async Task<bool> SetAsDefault([FromBody] int id)
        {
            try
            {
                var all = await GetAllSearchCriteria();
                var setting = all.FirstOrDefault(o => o.Id == id);
                if (setting == null) return false;

                return await PersonalSettingService.SetAsDefaultForGoogleOne(new RMPersonalSettingDto { Owner = TenantLocalValue.LogonUserId, Id = id, Name = setting.Name });
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while set setting as default personal setting, id : {id}, user id : {TenantLocalValue.LogonUserId}. Error: {e.ToString()}");
            }
            return false;
        }

        [HttpPost("globalsearch/offlinejob/start")]
        public Task<string> StartOfflineSearchJob([FromBody] int profileId)
        {
            string jobId = null;
            try
            {
                RMPersonalSettingDto profileWithoutContent = PersonalSettingService.GetById(profileId);
                if (profileWithoutContent != null)
                {
                    jobId = PersonalSettingService.RunSearchOffline(profileId);
                }
                else
                {
                    _logger.Warn("Invalid search profile id {0}", profileId);
                }
            }
            catch (Exception e)
            {
                _logger.Error(e.Message, e);
            }
            return Task.FromResult(jobId);
        }

        private async Task<RMExplorerSearchCriteriaDto> Convert2GlobalSearchCriteriaAllProfileForGoogleOne(RMPersonalSettingDto dto, bool isSharedBy = false)
        {
            RMExplorerSearchCriteriaDto result = null;

            if (dto.IsBuiltIn)
            {
                result = await Convert2GlobalSearchCriteriaForGoogleOne(dto, isSharedBy);
                return result;
            }
            else
            {
                var contentSourceStr = JsonConvert.DeserializeObject<RMExplorerSearchCriteriaSetting>(dto.ContentStr);
                if (contentSourceStr.AdvancedSearchs != null)
                {
                    SourceFlag source = SourceFlag.None;
                    var isMatched = contentSourceStr.AdvancedSearchs.Any(content =>
                    {
                        var contentSearch = JsonConvert.DeserializeObject<ExplorerSearchOptionV3>(content.ContentStr);
                        return contentSearch.Column.Id == QueryCloumnIds.SourceFlag && contentSearch.Value.Contains($"{(int)SourceFlag.Google}") == true;
                    });
                    if (isMatched)
                    {
                        result = await Convert2GlobalSearchCriteriaForGoogleOne(dto, isSharedBy);
                        AssembleGoogleJsonTree(result);
                        return result;
                    }
                }
                return null;
            }
        }

        private void ProcessDuplicateSharedName(List<RMExplorerSearchCriteriaDto> result, RMExplorerSearchCriteriaDto sharedDto)
        {
            var count = result.Count(o => o.Name == sharedDto.Name);
            if (count > 0)
            {
                sharedDto.Name = $"{sharedDto.Name}({count})";
            }
        }

        private async Task<RMExplorerSearchCriteriaDto> GetGlobalSearchCriteriaByIdAsync(int id)
        {
            var personalSetting = PersonalSettingService.GetById(id);
            RMExplorerSearchCriteriaDto dto = await Convert2GlobalSearchCriteriaForGoogleOne(personalSetting);
            List<JMItemInfo> jms = new List<JMItemInfo>();
            ArgumentCheck.NotNull(dto, nameof(dto));
            if (!CheckOwner(dto.Owner))
            {
                var isShare2CurrentUser = PersonalSettingService.IsSharedToUser(TenantLocalValue.LogonUserId, dto.Id);
                if (isShare2CurrentUser)
                {
                    RMGlobalSearchSharedSettingDto sharedSetting = PersonalSettingService.GetSharedInfo(id);

                    var userAndGroupIds = await UserService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
                    List<int> currentGroupIdList = UserService.GetAllGroupIds(userAndGroupIds); 
                    int[] sharedGroups = currentGroupIdList.Where(a => sharedSetting.SecurityGroups.Contains(a)).ToArray();   
                    _logger.Info($"Shared groups {string.Join(",", sharedGroups)}, of the profile {id}");
                    jms = await JobMonitorService.GetEndedJobByScopeIdAsync(id.ToString(), new int[] { 0, 1, 2, 4 }, sharedGroups);
                    _logger.Info($"Final and running jobs on shared profile {id} are {string.Join(";", jms.Select(a => a.JobId).ToArray())}");
                }
            }
            else
            {
                jms = await JobMonitorService.GetEndedJobByScopeIdAsync(id.ToString(), new int[] { 0, 1, 2, 4 }, TenantLocalValue.LogonUserId);
                _logger.Info($"Finaland running  jobs on profile {id} are {string.Join(";", jms.Select(a => a.JobId).ToArray())}");
            }
            dto.OfflineJobs = new List<OfflineJobInfo>();
            foreach (var jm in jms)
            {
                _logger.Debug("Job {0} status is {1}", jm.JobId, jm.Status);
                if (jm.Status == JobStatus.Wait || jm.Status == JobStatus.InProgress)
                {
                    dto.HasRunningJob = true;
                    continue;
                }
                OfflineJobInfo info = new OfflineJobInfo();
                info.JobId = jm.JobId;
                info.StartTime = jm.StartTime;
                dto.OfflineJobs.Add(info);
            }
            return dto;
        }

        private async Task<RMExplorerSearchCriteriaDto> GetDefaultDelayedLoanSettingAsync()
        {
            return await ExplorerService.IsPhysicalEndUserAsync() ? RMExplorerSearchCriteriaDto.GetDefaultDelayedLoanSetting(UserService.GetUserByUserId(TenantLocalValue.LogonUserId))
                        : RMExplorerSearchCriteriaDto.GetDefaultDelayedLoanSetting();
        }

        private bool CheckOwner(string owner)
        {
            return owner == TenantLocalValue.LogonUserId;
        }

        private void ValidateOwner(string owner)
        {
            if (!CheckOwner(owner)) throw new NoPermissionException();
        }

        private void AssembleTree(RMExplorerSearchCriteriaDto dto)
        {
            AssembleTermTree(dto);
            AssembleGoogleJsonTree(dto);
        }

        private void AssembleTermTree(RMExplorerSearchCriteriaDto dto)
        {
            if (dto == null || dto.Setting == null) return;

            if (!string.IsNullOrEmpty(dto.Setting.TermTreeStr)) //basic search
            {
                dto.Setting.TermTreeStr = JsonConvert.SerializeObject(ReportService.GetTermTree(dto.Setting.TermTreeStr));
            }

            if (dto.Setting.AdvancedSearchs != null) // advanced search
            {
                foreach (var advSearch in dto.Setting.AdvancedSearchs)
                {
                    if (!string.IsNullOrEmpty(advSearch.TermTreeStr))
                    {
                        advSearch.TermTreeStr = JsonConvert.SerializeObject(ReportService.GetTermTree(advSearch.TermTreeStr));
                    }
                }
            }
        }

        private void AssembleGoogleTree(RMExplorerSearchCriteriaDto dto)
        {
            if (dto == null || dto.Setting == null) return;
            if (!string.IsNullOrEmpty(dto.Setting.GoogleTreeStr)) //basic search
            {
                dto.Setting.GoogleTreeStr = RuleSPTreeUtil.BuildGoogleTreeXmlStr(dto.Setting.GoogleTreeStr);
            }

            if (dto.Setting.AdvancedSearchs != null) // advanced search
            {
                foreach (var advSearch in dto.Setting.AdvancedSearchs)
                {
                    if (!string.IsNullOrEmpty(advSearch.GoogleTreeStr))
                    {
                        advSearch.GoogleTreeStr = RuleSPTreeUtil.BuildGoogleTreeXmlStr(advSearch.GoogleTreeStr);
                    }
                }
            }
        }

        private void AssembleGoogleJsonTree(RMExplorerSearchCriteriaDto dto)
        {
            if (dto == null || dto.Setting == null) return;
            if (!string.IsNullOrEmpty(dto.Setting.GoogleTreeStr)) //basic search
            {
                dto.Setting.GoogleTreeStr = RuleSPTreeUtil.ConvertXmlStrToGoogleTreeJsonStr(dto.Setting.GoogleTreeStr);
            }

            if (dto.Setting.AdvancedSearchs != null) // advanced search
            {
                foreach (var advSearch in dto.Setting.AdvancedSearchs)
                {
                    if (!string.IsNullOrEmpty(advSearch.GoogleTreeStr))
                    {
                        advSearch.GoogleTreeStr = RuleSPTreeUtil.ConvertXmlStrToGoogleTreeJsonStr(advSearch.GoogleTreeStr);
                    }
                }
            }
        }
    }
}
