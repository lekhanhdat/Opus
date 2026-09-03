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
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.Service.Services.AccountManager;
using AvePoint.RA.Service.Services.ControlPanel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Org.BouncyCastle.Bcpg.OpenPgp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Common.Filters
{
    /// <summary>
    /// 此Comment之后需要Remove掉！！！TO DO ylgu
    //大体思路：
    //根据Explorer Action中的RecordId（从各种参数中获取）
    //反向查找Record对象，从参数或者Record对象中判断Record flag 以及 container id
    //从而用User来判断对SourceFlag(SP, Exo, etc) 和 对应container 是否有权限。
    //TO DO：
    //在api controler 中增加filtet 标签， 在filter内部逻辑中兼容各个参数。
    //API controller 如有考虑不全的，需要额外增加filter 或者 其它方式判断。
    /// </summary>
    public class ValidateExplorerActionFilter : BaseActionFilter
    {
        private RALogger logger = RALogger.GetInstance(typeof(ValidateExplorerActionFilter));
        public IUserService UserService => PlatformWindsorManager.GetService<IUserService>();
        public IRMSecurityTrimmingHelper SecurityTrimmingHelper => PlatformWindsorManager.GetService<IRMSecurityTrimmingHelper>();
        private string action;

        const string EnvironmentName = "21V China North";

        public IExplorerQueryService ExplorerQueryService
        {
            get
            {
                return (IExplorerQueryService)PlatformWindsorManager.GetService(typeof(IExplorerQueryService));
            }
        }
        private IPermissionManagementService mPermissionManagementService { get; set; }
        public IPermissionManagementService PermissionManagementService
        {
            get
            {
                if (mPermissionManagementService == null)
                {
                    mPermissionManagementService = (IPermissionManagementService)PlatformWindsorManager.GetService(typeof(IPermissionManagementService));
                }
                return mPermissionManagementService;
            }
        }
        private readonly ISecurityGroupManagementService _securityGroupManagementService = PlatformWindsorManager.GetService<ISecurityGroupManagementService>();

        RMScopeRoleAssignmentDao RMScopeRoleAssignmentDao { get { return new RMScopeRoleAssignmentDao(); } }
        public ValidateExplorerActionFilter()
        {

        }
        public ValidateExplorerActionFilter(string type)
        {
            action = type;
        }
        protected override async Task OnActionAuthenticatedAsync(ActionExecutingContext actionContext)
        {
            if (await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.ManageHold)) { return; }
            List<string> userAndGroupUserIds = await UserService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
            List<Guid> recordIds = new List<Guid>();
            Object parmObj = actionContext.ActionArguments.Values.FirstOrDefault();
            if (parmObj != null)
            {
                var changeTermDto = parmObj as ChangeTermDto;
                if(changeTermDto != null && changeTermDto.CanReclassifyAllTerm)
                {
                    return;
                }
                var idMappings = GetIdsByParam(parmObj);
                var allRecordIds = GetRecordIds(idMappings);

                recordIds = await FilteredIdByAdminAsync(idMappings);
                if (recordIds != null && recordIds.Count > 0)
                {
                    (bool valid, string errorMessage) validResult = await ValidatePermissionAsync(recordIds, userAndGroupUserIds);
                    if (!(validResult.valid))
                    {
                        actionContext.Result = new ObjectResult(validResult.errorMessage) { StatusCode = (int)HttpStatusCode.Forbidden };
                        return;
                    }
                }
                var errorMessage = await ValidateEmailNotificatonAsync(parmObj);
                if (errorMessage.IsNotNullOrEmpty())
                {
                    actionContext.Result = new ObjectResult(errorMessage) { StatusCode = (int)HttpStatusCode.Forbidden };
                }
            }

        }
        public async Task<(bool, string)> ValidatePermissionAsync(List<Guid> recordIds, List<string> userAndGroupUserIds)
        {
            bool havePermission = true;
            ExplorerDao ExplorerDao = new ExplorerDao();
            List<Record> allRecord = ExplorerDao.GetRecordByIds(recordIds);
            var data = allRecord.GroupBy(r => r.SourceFlag, s => s.Id).ToDictionary(r => (SourceFlag)r.Key, s => s.ToList());

            if (action == "RelatedRecords" && data.Keys.Any(key => key == SourceFlag.SharePoint))
            {
                var environmentName = RMGlobalConfiguration.EnvSetting[RMEnvSettingKey.ENVIRONMENT_NAME];
                if (environmentName.ToLowerInvariant() == EnvironmentName.ToLowerInvariant())
                {
                    return (false, "Access Denied container");
                }
            }

            //admin不需要check权限
            var recordsId = await FilteredIdByAdminAsync(data);
            var recList = allRecord.Where(r => recordsId.Contains(r.Id)).ToList();
            if (recList.Count > 0) 
            {
                (bool valid, string errorMessage) validResult = await ValidateDataSourcePermissionAsync(recList);
                if (!validResult.valid)
                {
                    return validResult;
                }
                List<string> containerIds = recList.Where(r => r.SourceFlag == 1 || r.SourceFlag == 3 || r.SourceFlag == 0 || r.SourceFlag == 6).Select(r => r.ContainerId).Distinct().ToList();
                
                if (containerIds.Count > 0 && !RMScopeRoleAssignmentDao.ValidateContainerIdPermission(containerIds, userAndGroupUserIds))
                {
                    logger.Info($"No access on container");
                    return (false, "No access on container");
                }
            }
            
            return (havePermission, "");
        }

        public List<Guid> GetRecordIds(Dictionary<SourceFlag, List<Guid>> idMapping)
        {
            List<Guid> recordIds = new List<Guid>();
            foreach (var keyValuePair in idMapping)
            {
                recordIds.AddRange(keyValuePair.Value.ToList());
            }
            return recordIds.ToList();
        }

        public Task<bool> ValidateTermPermissionAsync(List<Guid> recordIds)
        {
            ExplorerDao ExplorerDao = new ExplorerDao();
            List<Record> records = ExplorerDao.GetRecordByIds(recordIds);
            return HaveTermPermissionAsync(records);
        }

        public async Task<bool> HaveTermPermissionAsync(List<Record> records)
        {
            bool hasPermission = true;
            try
            {
                var usedTermIds = records.Where(r => r.SourceFlag != (int)SourceFlag.Physical).Select(r => r.TermId).Distinct().ToList();
                var termPermDto = await ExplorerQueryService.GetSecurityTermDtoAsync();
                switch (termPermDto.TermPermissionType)
                {
                    case Contract.RMWeb.CP.TermPermissionMethod.All:
                        hasPermission = true;
                        break;
                    case Contract.RMWeb.CP.TermPermissionMethod.SpecifyScope:
                    case Contract.RMWeb.CP.TermPermissionMethod.None:
                        termPermDto.TermObjIds.Add(Guid.Empty);
                        var intersection = usedTermIds.Intersect(termPermDto.TermObjIds);
                        hasPermission = intersection.Count() < usedTermIds.Count() ? false : true;
                        break;
                    default:
                        hasPermission = false;
                        break;
                }
                if (!hasPermission)
                {
                    logger.Info("No term permission.");
                }
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred in checking term permission. Error:{e.ToString()}");
            }
            return hasPermission;
        }
        public async Task<(bool, string)> ValidateDataSourcePermissionAsync(List<Record> allRecord)
        {
            string errorMessage = "";
            bool valid = true;
            if (allRecord.Count > 0)
            {
                if (allRecord.Where(r => string.IsNullOrEmpty(r.ContainerId) && (r.SourceFlag == 1 || r.SourceFlag == 3)).FirstOrDefault() != null)
                {
                    logger.Info($"record data need upgrade");
                    errorMessage = "record data need upgrade";
                    valid = false;
                    return (valid, errorMessage);
                }
                if (allRecord.Any(r => r.SourceFlag == 1 || r.SourceFlag == 0))
                {
                    if (!(await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.SPOEnduser)))
                    {
                        logger.Info($"User have no sp access {TenantLocalValue.LogonUserId}");
                        errorMessage = "User have no sp access";
                        valid = false;
                        return (valid, errorMessage);
                    }
                }
                if (allRecord.Any(r => r.SourceFlag == 2))
                {
                    if (!(await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.FSEnduser)))
                    {
                        logger.Info($"User have no file system access {TenantLocalValue.LogonUserId}");
                        errorMessage = "User have no file system access";
                        valid = false;
                        return (valid, errorMessage);
                    }
                }
                if (allRecord.Any(r => r.SourceFlag == 3))
                {
                    if (!(await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.EXOEnduser)))
                    {
                        logger.Info($"User have no exchange access {TenantLocalValue.LogonUserId}");
                        errorMessage = "User have no exchange access";
                        valid = false;
                        return (valid, errorMessage);
                    }
                }
                if (allRecord.Any(r => r.SourceFlag == 4))
                {
                    if (!(await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.PhysicalEndUser)))
                    {
                        logger.Info($"User have no physical access {TenantLocalValue.LogonUserId}");
                        errorMessage = "User have no physical access";
                        valid = false;
                        return (valid, errorMessage);
                    }
                    var userAndGroupIds = await UserService.GetUserAndGroupIdsAsync(TenantLocalValue.LogonUserId);
                    var userPermissionIds = PermissionManagementService.GetScopePermissionIds(userAndGroupIds);
                    var physcialRecords = allRecord.Where(r => r.SourceFlag == 4).ToList();
                    var permissionIds = physcialRecords.Where(r => r.ScopePermissionId != 0).Select(r => r.ScopePermissionId).Distinct().ToList();
                    if (permissionIds != null && permissionIds.Count > 0 && permissionIds.Any(p => !userPermissionIds.Contains(p)))
                    {
                        logger.Info($"User have no permission for some record. TenantId: {TenantLocalValue.LogonUserId}");
                        errorMessage = "User have no permission for some record";
                        valid = false;
                        return (valid, errorMessage);
                    }
                }

                if (allRecord.Any(r => r.SourceFlag == 5))
                {
                    if (!(await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.SPOnPremEnduser)))
                    {
                        logger.Info($"User have no sp on premise access {TenantLocalValue.LogonUserId}");
                        errorMessage = "User have no sp on premise access";
                        valid = false;
                        return (valid, errorMessage);
                    }
                }

                if (allRecord.Any(r => r.SourceFlag == 6))
                {
                    if (!(await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.OneDriveEnduser)))
                    {
                        logger.Info($"User have no sp access {TenantLocalValue.LogonUserId}");
                        errorMessage = "User have no od access";
                        valid = false;
                        return (valid, errorMessage);
                    }
                }

                if (allRecord.Any(r => r.SourceFlag == 7))
                {
                    if ( !(await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.AzureFSAdmin)))
                    {
                        logger.Info($"User have no azure file share access {TenantLocalValue.LogonUserId}");
                        errorMessage = "User have no azure file share access";
                        valid = false;
                        return (valid, errorMessage);
                    }
                }

                //if (allRecord.Any(r => r.SourceFlag == 8))
                //{
                //    if (!(await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.BoxAdmin)))
                //    {
                //        logger.Info($"User have no box access {TenantLocalValue.LogonUserId}");
                //        return false;
                //    }
                //}

                if (allRecord.Any(r => r.SourceFlag == (int)SourceFlag.Teams))
                {
                    if (!(await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.TeamsEndUser)))
                    {
                        logger.Info($"User have no sp access {TenantLocalValue.LogonUserId}");
                        errorMessage = "User have no teams access";
                        valid = false;
                        return (valid, errorMessage);
                    }
                }
            }
            else 
            {
                logger.Info($"do not need to check permission {TenantLocalValue.LogonUserId}");
            }
           
            return (valid, errorMessage);
        }

        private Dictionary<SourceFlag, List<Guid>> GetIdsByParam(object parmObj)
        {
            //SourceFlag.None 代表不明确数据源
            Dictionary<SourceFlag, List<Guid>> result = new Dictionary<SourceFlag, List<Guid>>();
            if (parmObj as ChangeHoldDto != null)
            {
                result.Add(SourceFlag.None, ((ChangeHoldDto)parmObj).recordsId);
            }
            else if (parmObj as UpdateHoldDto != null)
            {
                result.Add(SourceFlag.None, ((UpdateHoldDto)parmObj).ReletedIds);
            }

            else if (parmObj as ChangeTermDto != null)
            {
                var changeTermDto = parmObj as ChangeTermDto;
                if (changeTermDto.EXORecordIds?.Count > 0)
                {
                    result.Add(SourceFlag.Exchange, changeTermDto.EXORecordIds);
                }
                if (changeTermDto.FSRecordIds?.Count > 0)
                {
                    result.Add(SourceFlag.FileSystem, changeTermDto.FSRecordIds);
                    //Validate fs permission
                }
                if (changeTermDto.PhyRecordIds?.Count > 0)
                {
                    //Validate physcial permission
                    //To Do validate physical container ylgu!!!
                    result.Add(SourceFlag.Physical, changeTermDto.PhyRecordIds);
                }
                if (changeTermDto.SPOnPremRecordIds?.Count > 0)
                {
                    //Validate physcial permission
                    //To Do validate physical container ylgu!!!
                    result.Add(SourceFlag.SharePointOnPrem, changeTermDto.SPOnPremRecordIds);
                }
                if (changeTermDto.RecordIds?.Count > 0)
                {
                    result.Add(SourceFlag.SharePoint, changeTermDto.RecordIds);
                }
                if (changeTermDto.AzureFileShareRecordIds?.Count > 0)
                {
                    result.Add(SourceFlag.AzureFileShare, changeTermDto.AzureFileShareRecordIds);
                }
            }
            else if (parmObj as UpdateRecordsDto != null)
            {
                var updateRecordsDto = parmObj as UpdateRecordsDto;
                var tempList = new List<Guid>();
                if (updateRecordsDto.ReletedIds?.Count > 0) 
                {
                    tempList.AddRange(updateRecordsDto.ReletedIds);
                }
                if (updateRecordsDto.DeleteReletedIds?.Count > 0)
                {
                    tempList.AddRange(updateRecordsDto.DeleteReletedIds);
                }
                tempList.Add(updateRecordsDto.Id);
                result.Add(SourceFlag.None, tempList);
                
            }

            else if (Guid.TryParse(parmObj?.ToString(), out Guid parmGuid))
            {
                result.Add(SourceFlag.None, new List<Guid>() { parmGuid });
            }
            else if (parmObj as List<Guid> != null)
            {
                var ids = parmObj as List<Guid>;
                result.Add(SourceFlag.None, ids);
            }
            else if (parmObj as DetailQueryDto != null)
            {
                var detail = parmObj as DetailQueryDto;
                result.Add(SourceFlag.None, new List<Guid>() { detail.Id });
            }
            else if (parmObj as MoveToDto != null)
            {
                var moveToDto = parmObj as MoveToDto;
                result.Add(SourceFlag.None,  moveToDto.SourceRecords?.Select(r => r.Id).ToList());
            }
           
            return result;
        }
        private async Task<List<Guid>> FilteredIdByAdminAsync(Dictionary<SourceFlag, List<Guid>> keyValuePairs)
        {
            List<Guid> recordIds = new List<Guid>();
            IUserService UserService = new UserService();
            if (keyValuePairs.ContainsKey(SourceFlag.FileSystem) && await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.FSAdmin))
            {
                keyValuePairs.Remove(SourceFlag.FileSystem);
            }
            if (keyValuePairs.ContainsKey(SourceFlag.SharePoint) && await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.SPOAdmin))
            {
                keyValuePairs.Remove(SourceFlag.SharePoint);
            }
            if (keyValuePairs.ContainsKey(SourceFlag.Exchange) && await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.EXOAdmin))
            {
                keyValuePairs.Remove(SourceFlag.Exchange);
            }
            if (keyValuePairs.ContainsKey(SourceFlag.Physical) && (await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.PhysicalAdmin)))
            {
                keyValuePairs.Remove(SourceFlag.Physical);
            }
            if (keyValuePairs.ContainsKey(SourceFlag.SharePointOnPrem) && await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.SPOnPremEnduser))
            {
                keyValuePairs.Remove(SourceFlag.SharePointOnPrem);
            }
            if (keyValuePairs.ContainsKey(SourceFlag.OneDrive) && await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.OneDriveAdmin))
            {
                keyValuePairs.Remove(SourceFlag.OneDrive);
            }
            if (keyValuePairs.ContainsKey(SourceFlag.AzureFileShare) && await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.AzureFSAdmin))
            {
                keyValuePairs.Remove(SourceFlag.AzureFileShare);
            }
            if (keyValuePairs.ContainsKey(SourceFlag.Box) && await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.BoxAdmin))
            {
                keyValuePairs.Remove(SourceFlag.Box);
            }
            if (keyValuePairs.ContainsKey(SourceFlag.Google) && await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.GoogleAdmin))
            {
                keyValuePairs.Remove(SourceFlag.Google);
            }
            if (keyValuePairs.ContainsKey(SourceFlag.Teams) && await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.TeamsAdmin))
            {
                keyValuePairs.Remove(SourceFlag.Teams);
            }
            foreach (var item in keyValuePairs)
            {
                recordIds.AddRange(item.Value.ToList());
            }
            return recordIds;
        }
        private async Task<string> ValidateEmailNotificatonAsync(Object parmObj)
        {
            if (parmObj as UpdateHoldDto != null)
            {
                var dto = (UpdateHoldDto)parmObj;
                if (dto.HoldSetting.EmailNotification != null && dto.HoldSetting.EmailNotification.IsEnabled)
                {
                    if (dto.HoldSetting.EmailNotification.ReminderDurationDays <= 0 || dto.HoldSetting.EmailNotification.ReminderDurationDays > 365)
                    {
                        return "Reminder duration must be greater than 0 and smaller 365";
                    }

                    if (dto.HoldSetting.EmailNotification.EmailRecipients == null || !dto.HoldSetting.EmailNotification.EmailRecipients.Any()) return "At least one notification recipient is required.";
                    foreach (var email in dto.HoldSetting.EmailNotification.EmailRecipients)
                    {
                        var userPermission = await _securityGroupManagementService.GetUserScopePermissionsAsync(email.UserId);
                        if (!_securityGroupManagementService.HasManageHoldsPermission(userPermission))
                        {
                            return "User send email have no permission access hold.";
                        }
                    }
                }
            }
            return string.Empty;

        }
    }
}
