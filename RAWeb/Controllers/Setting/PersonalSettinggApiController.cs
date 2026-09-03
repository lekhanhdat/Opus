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
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.PersonalSetting;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.SecurityTrimming.Model;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Controllers.Setting
{
    [RMApiAuthorize(RMPermissionMasks.EletricRecordExplorerEnduser | RMPermissionMasks.PhysicalEndUser, PermissionJoinType.Any, preferred: false)]
    public class PersonalSettinggApiController : BaseApiController
    {
        private IPersonalSettingService _PersonalSettingService;
        private IPersonalSettingService PersonalSettingService => PlatformWindsorManager.GetService(ref _PersonalSettingService);
        private IRMReportService _ReportService;
        private IRMReportService ReportService => PlatformWindsorManager.GetService(ref _ReportService);
        private IUserService _UserService;
        private IUserService UserService => PlatformWindsorManager.GetService(ref _UserService);
        private IExplorerService _ExplorerService;
        private IExplorerService ExplorerService => PlatformWindsorManager.GetService(ref _ExplorerService);
        private IJobMonitorService _JobMonitorService;
        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService(ref _JobMonitorService);
        
        /// <summary>
        /// return an int id if save succussfully, otherwise, return 0
        /// </summary>
        /// <param name="dto"></param>
        /// <returns>id of the record</returns>
        [HttpPost]
        public async Task<RMPersonalSettingSaveResult> SaveGlobalSearchCriteria([FromBody] RMExplorerSearchCriteriaDto dto)
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
                dto.Type = PersonalSettingType.GlobalSearchCriteria;
                if (dto.IsBuiltIn)
                {
                    dto.Setting = new RMExplorerSearchCriteriaSetting();
                }
                AssembleFSTree(dto);
                AssembleSPTree(dto);
                AssembleTeamsTree(dto);
                AssembleGoogleTree(dto);
                result.Id =  PersonalSettingService.Save(dto.Convert2PersonalSetting());
            }
            catch (SameNameException)
            {
                result.ErrorCode = RMPersonalSettingSaveResultErrorCode.SameName;
                Logger.Error($"A global search criteria setting with same name '{dto.Name}' already exists");
            }
            catch (NoPermissionException)
            {
                result.ErrorCode = RMPersonalSettingSaveResultErrorCode.NoPermission;
                Logger.Error($"Can't save search criteria because the current user {TenantLocalValue.LogonUserId} is not the owner of setting with id {dto.Id}");
            }
            catch (Exception e)
            {
                result.ErrorCode = RMPersonalSettingSaveResultErrorCode.Other;
                Logger.Error($"An error occurred while saving global search criteria. Error: {e.ToString()}");
            }

            return result;
        }
        [HttpPost]
        public string StartOfflineSearchJob(int profileId)
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
                    Logger.Warn("Invalid search profile id {0}", profileId);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message, e);
            }
            return jobId;
        }

        /// <summary>
        /// Update columns setting alone due to the difficulty of GUI coding.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<RMPersonalSettingSaveResult> SaveGlobalSearchCriteriaColumns([FromBody] RMExplorerSearchCriteriaDto dto)
        {
            var result = new RMPersonalSettingSaveResult();
            try
            {
                var item = await GetGlobalSearchCriteriaByIdAsync(dto.Id);
                ValidateOwner(item.Owner);
                item.Setting.ColumnsStr = dto.Setting.ColumnsStr;
                result.Id = PersonalSettingService.Save(item.Convert2PersonalSetting());
            }
            catch (NoPermissionException)
            {
                result.ErrorCode = RMPersonalSettingSaveResultErrorCode.NoPermission;
                Logger.Error($"Can't save search criteria because the current user {TenantLocalValue.LogonUserId} is not the owner of current setting with id {dto.Id}");
            }
            catch (Exception e)
            {
                result.ErrorCode = RMPersonalSettingSaveResultErrorCode.Other;
                Logger.Error($"An error occurred while saving global search criteria columns. Error: {e.ToString()}");
            }

            return result;
        }

        /// <summary>
        /// Return all of the fields, including setting value.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<RMExplorerSearchCriteriaDto> GetGlobalSearchCriteria([FromBody]int id)
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
                        Logger.Warn($"Can't get the setting with id {id} because it isn't shared to user {TenantLocalValue.LogonUserId}");
                        return null;
                    }
                }
                AssembleTree(result);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while get global search criteria by id : {id}, user id : {TenantLocalValue.LogonUserId}. Error: {e.ToString()}");
            }

            return result;
        }
        [HttpPost]
        public RMExplorerSearchCriteriaDto GetDSBActiveOrArchivedCriteria([FromBody]DSBInfo info)
        {
            try
            {
                return GetActiveOrArchivedCriteria(info);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        #region private method
        private RMExplorerSearchCriteriaDto GetActiveOrArchivedCriteria(DSBInfo info)
        {
            return RMExplorerSearchCriteriaDto.GetActiveOrArchivedCriteria(info);
        }
        private async Task<RMExplorerSearchCriteriaDto> GetDefaultDelayedLoanSettingAsync()
        {
            return await ExplorerService.IsPhysicalEndUserAsync() ? RMExplorerSearchCriteriaDto.GetDefaultDelayedLoanSetting(UserService.GetUserByUserId(TenantLocalValue.LogonUserId))
                        : RMExplorerSearchCriteriaDto.GetDefaultDelayedLoanSetting();
        }
        private void ValidateOwner(string owner)
        {
            if (!CheckOwner(owner)) throw new NoPermissionException();
        }

        private bool CheckOwner(string owner)
        {
            return owner == TenantLocalValue.LogonUserId;
        }

        private async Task<RMExplorerSearchCriteriaDto> GetGlobalSearchCriteriaByIdAsync(int id)
        {
            var personalSetting = PersonalSettingService.GetById(id);
            RMExplorerSearchCriteriaDto dto = personalSetting?.Convert2GlobalSearchCriteria();
            List<JMItemInfo> jms = new List<JMItemInfo>();
            ArgumentCheck.NotNull(dto, nameof(dto));
            if (!CheckOwner(dto.Owner))
            {
                var isShare2CurrentUser = PersonalSettingService.IsSharedToUser(TenantLocalValue.LogonUserId, dto.Id);
                if (isShare2CurrentUser)
                {
                    RMGlobalSearchSharedSettingDto sharedSetting = PersonalSettingService.GetSharedInfo(id); //当前Shared Profile所分享的Permission Group Id列表

                    var userAndGroupIds = await UserService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
                    List<int> currentGroupIdList = UserService.GetAllGroupIds(userAndGroupIds);   //当前User所在的Permission Group Id 列表
                    int[] sharedGroups = currentGroupIdList.Where(a => sharedSetting.SecurityGroups.Contains(a)).ToArray();    //前两个列表的交集， 理论上一定会有交集
                    Logger.Info($"Shared groups {string.Join(",", sharedGroups)}, of the profile {id}");
                    jms = await JobMonitorService.GetEndedJobByScopeIdAsync(id.ToString(), new int[] {0, 1, 2, 4 }, sharedGroups);
                    Logger.Info($"Final and running jobs on shared profile {id} are {string.Join(";", jms.Select(a => a.JobId).ToArray())}");
                }
            }
            else
            { 
                jms = await JobMonitorService.GetEndedJobByScopeIdAsync(id.ToString(), new int[] {0, 1, 2, 4}, TenantLocalValue.LogonUserId);
                Logger.Info($"Finaland running  jobs on profile {id} are {string.Join(";", jms.Select(a=>a.JobId).ToArray())}");
            }
            dto.OfflineJobs = new List<OfflineJobInfo>();
            foreach(var jm in jms)
            {
                Logger.Debug("Job {0} status is {1}", jm.JobId, jm.Status);
                if(jm.Status== JobStatus.Wait || jm.Status == JobStatus.InProgress)
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

        private void AssembleTree(RMExplorerSearchCriteriaDto dto)
        {
            AssembleTermTree(dto);
            AssembleFSJsonTree(dto);
            AssembleSPJsonTree(dto);
            AssembleTeamsJsonTree(dto);
            AssembleGoogleJsonTree(dto);
        }

        private void AssembleFSTree(RMExplorerSearchCriteriaDto dto)
        {
            if (dto == null || dto.Setting == null) return;
            if (!string.IsNullOrEmpty(dto.Setting.FSTreeStr)) //basic search
            {
                dto.Setting.FSTreeStr = RuleSPTreeUtil.BuildFSTreeXMLStr(dto.Setting.FSTreeStr);
            }

            if (dto.Setting.AdvancedSearchs != null) // advanced search
            {
                foreach(var advSearch in dto.Setting.AdvancedSearchs)
                {
                    if (!string.IsNullOrEmpty(advSearch.FSTreeStr))
                    {
                        advSearch.FSTreeStr = RuleSPTreeUtil.BuildFSTreeXMLStr(advSearch.FSTreeStr);
                    }
                }
            }
        }
        private void AssembleSPTree(RMExplorerSearchCriteriaDto dto)
        {
            if (dto == null || dto.Setting == null) return;
            if (!string.IsNullOrEmpty(dto.Setting.SPTreeStr)) //basic search
            {
                dto.Setting.SPTreeStr = RuleSPTreeUtil.BuildSPTreeXMLStr(dto.Setting.SPTreeStr);
            }

            if (dto.Setting.AdvancedSearchs != null) // advanced search
            {
                foreach(var advSearch in dto.Setting.AdvancedSearchs)
                {
                    if (!string.IsNullOrEmpty(advSearch.SPTreeStr))
                    {
                        advSearch.SPTreeStr = RuleSPTreeUtil.BuildSPTreeXMLStr(advSearch.SPTreeStr);
                    }
                }
            }
        }

        private void AssembleTeamsTree(RMExplorerSearchCriteriaDto dto)
        {
            if (dto == null || dto.Setting == null) return;
            if (!string.IsNullOrEmpty(dto.Setting.TeamsTreeStr)) //basic search
            {
                dto.Setting.TeamsTreeStr = RuleSPTreeUtil.BuildSPTreeXMLStr(dto.Setting.TeamsTreeStr);
            }

            if (dto.Setting.AdvancedSearchs != null) // advanced search
            {
                foreach (var advSearch in dto.Setting.AdvancedSearchs)
                {
                    if (!string.IsNullOrEmpty(advSearch.TeamsTreeStr))
                    {
                        advSearch.TeamsTreeStr = RuleSPTreeUtil.BuildSPTreeXMLStr(advSearch.TeamsTreeStr);
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

        private void AssembleFSJsonTree(RMExplorerSearchCriteriaDto dto)
        {
            if (dto == null || dto.Setting == null) return;
            if (!string.IsNullOrEmpty(dto.Setting.FSTreeStr)) //basic search
            {
                dto.Setting.FSTreeStr = RuleSPTreeUtil.ConvertXmlStrToFSTreeJsonStr(dto.Setting.FSTreeStr);
            }

            if (dto.Setting.AdvancedSearchs != null) // advanced search
            {
                foreach (var advSearch in dto.Setting.AdvancedSearchs)
                {
                    if (!string.IsNullOrEmpty(advSearch.FSTreeStr))
                    {
                        advSearch.FSTreeStr = RuleSPTreeUtil.ConvertXmlStrToFSTreeJsonStr(advSearch.FSTreeStr);
                    }
                }
            }
        }
        private void AssembleSPJsonTree(RMExplorerSearchCriteriaDto dto)
        {
            if (dto == null || dto.Setting == null) return;
            if (!string.IsNullOrEmpty(dto.Setting.SPTreeStr)) //basic search
            {
                dto.Setting.SPTreeStr = RuleSPTreeUtil.ConvertXmlStrToSPTreeJsonStr(dto.Setting.SPTreeStr);
            }

            if (dto.Setting.AdvancedSearchs != null) // advanced search
            {
                foreach (var advSearch in dto.Setting.AdvancedSearchs)
                {
                    if (!string.IsNullOrEmpty(advSearch.SPTreeStr))
                    {
                        advSearch.SPTreeStr = RuleSPTreeUtil.ConvertXmlStrToSPTreeJsonStr(advSearch.SPTreeStr);
                    }
                }
            }
        }

        private void AssembleTeamsJsonTree(RMExplorerSearchCriteriaDto dto)
        {
            if (dto == null || dto.Setting == null) return;
            if (!string.IsNullOrEmpty(dto.Setting.TeamsTreeStr)) //basic search
            {
                dto.Setting.TeamsTreeStr = RuleSPTreeUtil.ConvertXmlStrToSPTreeJsonStr(dto.Setting.TeamsTreeStr);
            }

            if (dto.Setting.AdvancedSearchs != null) // advanced search
            {
                foreach (var advSearch in dto.Setting.AdvancedSearchs)
                {
                    if (!string.IsNullOrEmpty(advSearch.TeamsTreeStr))
                    {
                        advSearch.TeamsTreeStr = RuleSPTreeUtil.ConvertXmlStrToSPTreeJsonStr(advSearch.TeamsTreeStr);
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
        #endregion
        /// <summary>
        /// Get all of the global search criteria without setting.
        /// e.g, name, id, if is default setting,...
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public List<RMExplorerSearchCriteriaDto> GetAllGlobalSearchCriteria()
        {
            var result = new List<RMExplorerSearchCriteriaDto>();
            var currentUser = TenantLocalValue.LogonUserId;
            var type = PersonalSettingType.GlobalSearchCriteria;
            try
            {
                //if there is no built-in setting, create it.
                var existBuiltIn = PersonalSettingService.ExistsBuiltIn(new RMPersonalSettingDto { Owner = currentUser, Type = type });
                if (!existBuiltIn)
                {
                    var builtInDto = RMExplorerSearchCriteriaDto.GetBuiltInSetting(PersonalSettingType.GlobalSearchCriteria);
                    PersonalSettingService.Save(builtInDto.Convert2PersonalSetting());
                }
                PersonalSettingService.UpgradeDefaultSetting(currentUser, type); //upgrade default setting if needed.
                result.AddRange(GetAllSearchCriteria());

                //if there is no default search criteria, then set the built-in as default
                if (!result.Exists(o => o.IsDefault))
                {
                    var builtIn = result.FirstOrDefault(o => o.IsBuiltIn);
                    if (builtIn != null) builtIn.IsDefault = true;
                }
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while get all global search criteria, user id : {currentUser}. Error: {e.ToString()}");
            }

            return result;
        }

        /// <summary>
        /// 得到所有的setting，包括别人share过来的
        /// </summary>
        /// <returns></returns>
        private List<RMExplorerSearchCriteriaDto> GetAllSearchCriteria()
        {
            var result = new List<RMExplorerSearchCriteriaDto>();
            var currentUser = TenantLocalValue.LogonUserId;
            var type = PersonalSettingType.GlobalSearchCriteria;
            //get self created settings
            var personalSettings = PersonalSettingService.GetByOwnerAndType(currentUser, type);
            if (personalSettings != null)
            {
                result.AddRange(personalSettings.Select(o => o.Convert2GlobalSearchCriteria()).ToList());
            }
            //get settings shared by others
            var sharedSettings = PersonalSettingService.GetSharedSettings(currentUser, type);
            if (sharedSettings?.Count > 0)
            {
                foreach(var sharedSetting in sharedSettings) //处理同名的情况
                {
                    var sharedDto = sharedSetting.Convert2GlobalSearchCriteria(isSharedBy: true);
                    ProcessDuplicateSharedName(result, sharedDto);
                    result.Add(sharedDto);
                }
            }

            return result;
        }

        /// <summary>
        /// 别的user share过来的setting，可能会有同名现象，这里处理可能出现的同名操作, e.g. Name(1), Name(2)
        /// </summary>
        /// <param name="result"></param>
        /// <param name="sharedDto"></param>
        private void ProcessDuplicateSharedName(List<RMExplorerSearchCriteriaDto> result, RMExplorerSearchCriteriaDto sharedDto)
        {
            var count = result.Count(o => o.Name == sharedDto.Name);
            if (count > 0)
            {
                sharedDto.Name = $"{sharedDto.Name}({count})";
            }
        }

        [HttpPost]
        public async Task<bool> Delete([FromBody]int id)
        {
            try
            {
                return await PersonalSettingService.DeleteAsync(new RMPersonalSettingDto { Owner = TenantLocalValue.LogonUserId, Id = id, Type = PersonalSettingType.GlobalSearchCriteria });
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while delete personal setting, user id : {TenantLocalValue.LogonUserId}. Error: {e.ToString()}");
                return false;
            }
        }

        [HttpPost]
        public bool SetAsDefault([FromBody]int id)
        {
            try
            {
                var all = GetAllSearchCriteria();
                var setting = all.FirstOrDefault(o => o.Id == id);
                if (setting == null) return false;
                
                return PersonalSettingService.SetAsDefault(new RMPersonalSettingDto { Owner = TenantLocalValue.LogonUserId, Id = id, Name = setting.Name });
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while set setting as default personal setting, id : {id}, user id : {TenantLocalValue.LogonUserId}. Error: {e.ToString()}");
            }
            return false;
        }

        #region share profile
        private bool ValidateParam(RMGlobalSearchSharedSettingDto dto)
        {
            return dto?.Id > 0 && dto?.SecurityGroups?.Count > 0;
        }

        private bool ValidateAdminPermission()
        {
            return UserService.IsMemberOfSecurityGroup((int)BuiltInGroupId.Admin, TenantLocalValue.LogonUserId);
        }

        [HttpPost]
        public bool CanShare()
        {
            return ValidateAdminPermission();
        }

        /// <summary>
        /// share the settings
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public RMPersonalSettingShareResult ShareGlobalSearchCriteria([FromBody] RMGlobalSearchSharedSettingDto dto)
        {
            var result = new RMPersonalSettingShareResult();
            try
            {
                if (!ValidateParam(dto))
                {
                    return new RMPersonalSettingShareResult() { HasError = true, ErrorCode = RMPersonalShareResultErrorCode.InvalidParameter };
                }
                if (!ValidateAdminPermission())
                {
                    return new RMPersonalSettingShareResult() { HasError = true, ErrorCode = RMPersonalShareResultErrorCode.NoPermission };
                }
                PersonalSettingService.Share(new RMPersonalSettingSecurityGroupMappingDto { Id = dto.Id, SecurityGroups = dto.SecurityGroups, Owner = TenantLocalValue.LogonUserId });
            }
            catch (Exception e)
            {
                result.HasError = true;
                result.ErrorCode = RMPersonalShareResultErrorCode.Others;
                Logger.Error($"An error occurred while share personal setting, id : {dto.Id}, user id : {TenantLocalValue.LogonUserId}. Error: {e.ToString()}");
            }
            return result;
        }

        [HttpPost]
        public RMPersonalSettingShareResult UnShareGlobalSearchCriteria([FromBody]int id)
        {
            var result = new RMPersonalSettingShareResult();
            try
            {
                if (!ValidateAdminPermission())
                {
                    return new RMPersonalSettingShareResult() { HasError = true, ErrorCode = RMPersonalShareResultErrorCode.NoPermission };
                }
                PersonalSettingService.CancelShare(TenantLocalValue.LogonUserId, id);
            }
            catch (Exception e)
            {
                result.HasError = true;
                result.ErrorCode = RMPersonalShareResultErrorCode.Others;
                Logger.Error($"An error occurred while cancel sharing personal setting, id : {id}, user id : {TenantLocalValue.LogonUserId}. Error: {e.ToString()}");
            }
            return result;
        }

        [HttpPost]
        public RMGlobalSearchSharedSettingDto GetGlobalSearchShareSetting([FromBody] int id)
        {
            var result = new RMGlobalSearchSharedSettingDto { Id = id };
            try
            {
                if (ValidateAdminPermission()) return PersonalSettingService.GetSharedInfo(id);
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while GetSharedGlobalSearchCriteria, id : {id}, user id : {TenantLocalValue.LogonUserId}. Error: {e.ToString()}");
            }
            return result;
        }
        #endregion
    }
}