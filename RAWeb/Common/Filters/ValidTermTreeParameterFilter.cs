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
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Common.Filters
{
    public class ValidTermTreeParameterFilter : BaseActionFilter
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(ValidTermTreeParameterFilter));
        public IUserService UserService => PlatformWindsorManager.GetService<IUserService>();
        public IRuleManagerService RuleManagerService => PlatformWindsorManager.GetService<IRuleManagerService>();

        public ISecurityGroupManagementService SecurityGroupManagementService => PlatformWindsorManager.GetService<ISecurityGroupManagementService>();
        public ITermSetDao TermSetDao => PlatformWindsorManager.GetService<ITermSetDao>();

        public IRMSecurityGroupDao SecurityGroupDao => PlatformWindsorManager.GetService<IRMSecurityGroupDao>();
        public ITermGroupDao TermGroupDao => PlatformWindsorManager.GetService<ITermGroupDao>();
        public ITermDao TermDao => PlatformWindsorManager.GetService<ITermDao>();
        public IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        //public ITermDao TermDao = new TermDao();
        //public IRMSecurityGroupDao SecurityGroupDao = new RMSecurityGroupDao();
        //public ITermGroupDao TermGroupDao = new TermGroupDao();
        private string action;

        const string EnvironmentName = "21V China North";

        private readonly Regex REGEX_DIGIT_AND_CHAR_AND_SPECIAL_CHAR = new(@"^[\sA-Za-z0-9!""#$%&'()*+,./:;<=>?@[\\\]^_`{|}~-]+$");

        public ValidTermTreeParameterFilter()
        {

        }
        public ValidTermTreeParameterFilter(string type)
        {
            action = type;
        }

        protected override async Task OnActionAuthenticatedAsync(ActionExecutingContext actionContext)
        {
            Object parmObj = actionContext.ActionArguments.Values.FirstOrDefault();
            if (parmObj != null)
            {
                if (action.Equals("ChangeTerm") && RMKeyValueDao.IsEnableJPMCFileSystemFeature())
                {
                    Logger.Warn("This feature is not available for JPMC.");
                    actionContext.Result = new ObjectResult("Invalid permission") { StatusCode = (int)HttpStatusCode.Forbidden };
                    return;
                }

                Dictionary<SecurityTermLevel, List<Guid>> nodelevelAndUniqueId = GetIdsByParam(parmObj);
                if (!ValidateTermRetention(parmObj))
                {
                    actionContext.Result = new ObjectResult("Access  Denied") { StatusCode = (int)HttpStatusCode.Forbidden };
                }
                var treePage = parmObj as TreePage;
                if (!(treePage != null && treePage.ShowAllTerms) && !nodelevelAndUniqueId.IsNullOrEmpty())
                {
                    if (!(await ValidatePermissionAsync(nodelevelAndUniqueId)))
                    {
                        if (action.Equals("AddOrUpdatePhysicalObject"))
                        {
                            actionContext.Result = new ObjectResult("RM_NotPermission_CurrentTermDifferentScope") { StatusCode = (int)HttpStatusCode.NotFound };
                        }
                        else
                        {
                            actionContext.Result = new ObjectResult("RM_NotPermission_CurrentTermDifferentScope") { StatusCode = (int)HttpStatusCode.Forbidden };
                        }
                    }
                }
                if (!(await ValidateTermRuleIsAvailableAsync(parmObj)))
                {
                    actionContext.Result = new ObjectResult("RM_NotPermission_ForUsedTerm") { StatusCode = (int)HttpStatusCode.Forbidden };
                }

                if (action.Equals("AddOrUpdatePhysicalObject"))
                {
                    PhysicalObjectDto dto = parmObj as PhysicalObjectDto;
                    if (dto != null)
                    {
                        _ = dto.MetaInfo.TryGetValue(DefaultColumnIDs.Barcode, out string value);
                        if (!string.IsNullOrEmpty(value))
                        {
                            if (value.Length > 26 || !REGEX_DIGIT_AND_CHAR_AND_SPECIAL_CHAR.IsMatch(value))
                            {
                                actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                            }
                        }
                    }
                }
            }
        }

        public bool ValidateTermRetention(object parmObj)
        {
            var environmentName = RMGlobalConfiguration.EnvSetting[RMEnvSettingKey.ENVIRONMENT_NAME];
            if ((action.Equals("SaveTermSettings") || action.Equals("InheritSettingToParent")) && parmObj as TermSettingsInfo != null)
            {
                TermSettingsInfo termSettingsInfo = parmObj as TermSettingsInfo;
                if(environmentName.ToLowerInvariant() != EnvironmentName.ToLowerInvariant())
                {
                    return true;
                }
                if (termSettingsInfo.EnforceRetention != 0
                    || !string.IsNullOrEmpty(termSettingsInfo.SPRetentionLabel)
                    || !string.IsNullOrEmpty(termSettingsInfo.OneDriveRetentionLabel)
                    || !string.IsNullOrEmpty(termSettingsInfo.EXORetentionLabel))
                {
                    return false;
                }
            }
            return true;
        }

        public async Task<bool> ValidateTermRuleIsAvailableAsync(object parmObj)
        {
            if (action.Equals("SaveTermSettings") && parmObj as TermSettingsInfo != null)
            {
                TermSettingsInfo termSettingsInfo = parmObj as TermSettingsInfo;
                var scopeRuleContainers = SecurityTrimmingHelper.GetRuleScopeByTermId(TenantLocalValue.LogonGroupId, TenantLocalValue.LogonUserId, termSettingsInfo.tId.ToString());
                var associateAvailableRule = await RuleManagerService.GetSimpleRecordsRulesFromDBAsync(scopeRuleContainers);
                var availableRuleIds = associateAvailableRule.Select(r => r.RuleId).ToList();
                if (termSettingsInfo.infos.Any(i => !availableRuleIds.Contains(i.RuleId)))
                {
                    return false;
                }
            }
            return true;
        }
        public async Task<bool> ValidatePermissionAsync(Dictionary<SecurityTermLevel, List<Guid>> nodeTypeAndUniqueIds)
        {
            var forPhysicalViewTypes = new SecurityTermLevel[] { SecurityTermLevel.TermGroupForPhysicalView, SecurityTermLevel.TermSetForPhysicalView, SecurityTermLevel.TermForPhysicalView };
            var typeAndMapping = nodeTypeAndUniqueIds.FirstOrDefault();
            bool hasPermission;
            if (forPhysicalViewTypes.Contains(typeAndMapping.Key))
            {
                FilterTermObjOption filterOption = new FilterTermObjOption
                {
                    NeedCheckPermission = true,
                    FilterByContentSource = true,
                    ExcludeBuiltIn = true,
                    ForPhysicalView = true,
                    SourceFlag = SourceFlag.Physical
                };
                SecurityTermLevel securityTermLevel = typeAndMapping.Key;
                switch (typeAndMapping.Key)
                {
                    case SecurityTermLevel.TermGroupForPhysicalView:
                        securityTermLevel = SecurityTermLevel.TermGroup;
                        break;
                    case SecurityTermLevel.TermSetForPhysicalView:
                        securityTermLevel = SecurityTermLevel.TermSet;
                        break;
                    case SecurityTermLevel.TermForPhysicalView:
                        securityTermLevel = SecurityTermLevel.Term;
                        break;
                    default:
                        break;
                }
                hasPermission = await SecurityGroupManagementService.DoesUserHasPermisionToTermAsync(TenantLocalValue.LogonUserId, securityTermLevel, typeAndMapping.Value, filterOption);
            }
            else
            {
                hasPermission = await SecurityGroupManagementService.DoesUserHasPermisionToTermAsync(TenantLocalValue.LogonUserId, typeAndMapping.Key, typeAndMapping.Value);
            }
            return hasPermission;
        }

        /// <summary>
        /// 第一个参数是nodelevel  第二个参数是node的unique id
        /// </summary>
        /// <param name="parmObj"></param>
        /// <returns></returns>
        private Dictionary<SecurityTermLevel, List<Guid>> GetIdsByParam(object parmObj)
        {
            Dictionary<SecurityTermLevel, List<Guid>> result = new Dictionary<SecurityTermLevel, List<Guid>>();
            if (action.Equals("RenameTermGroup") && parmObj as TermInfo != null)
            {
                TermInfo termInfo = parmObj as TermInfo;
                int groupId = termInfo.TermId;
                RMTermGroup group = TermGroupDao.GetRMTermGruop(groupId);
                if (group != null)
                {
                    List<Guid> termGroupId = new List<Guid>();
                    termGroupId.Add(group.UniqueId);
                    result.Add(SecurityTermLevel.TermGroup, termGroupId);
                }
            }
            if (action.Equals("CreateTermSet") && parmObj as TermInfo != null)
            {
                TermInfo termInfo = parmObj as TermInfo;
                List<Guid> termGroupId = new List<Guid>();
                termGroupId.Add(termInfo.TermGroupUniqueId);
                result.Add(SecurityTermLevel.TermGroup, termGroupId);
            }
            if (action.Equals("SaveTermGroup") && parmObj as TermInfo != null)
            {
                TermInfo termInfo = parmObj as TermInfo;
                int groupId = termInfo.TermGroupId;
                RMTermGroup group = TermGroupDao.GetRMTermGruop(groupId);
                if (group != null)
                {
                    List<Guid> termGroupId = new List<Guid>();
                    termGroupId.Add(group.UniqueId);
                    result.Add(SecurityTermLevel.TermGroup, termGroupId);
                }
            }
            if (action.Equals("DeleteTermGroup"))
            {
                Guid groupId = new Guid(parmObj.ToString());
                List<Guid> termGroupIds = new List<Guid>();
                termGroupIds.Add(groupId);
                result.Add(SecurityTermLevel.TermGroup, termGroupIds);
            }
            if ((action.Equals("CreateTerm") || action.Equals("SaveTermSet")) && parmObj as TermInfo != null)
            {
                TermInfo termInfo = parmObj as TermInfo;
                RMTermSet termset = TermSetDao.GetRMTermSet(termInfo.TermSetId);
                if (termset != null)
                {
                    List<Guid> termSetId = new List<Guid>();
                    termSetId.Add(termset.UniqueId);
                    result.Add(SecurityTermLevel.TermSet, termSetId);
                }
            }
            if (action.Equals("RenameTermSet") && parmObj as TermInfo != null)
            {
                TermInfo termInfo = parmObj as TermInfo;
                RMTermSet termset = TermSetDao.GetRMTermSet(termInfo.TermId);
                if (termset != null)
                {
                    List<Guid> termSetId = new List<Guid>();
                    termSetId.Add(termset.UniqueId);
                    result.Add(SecurityTermLevel.TermSet, termSetId);
                }
            }
            if (action.Equals("SaveTermSettings") && parmObj as TermSettingsInfo != null)
            {
                TermSettingsInfo termSettingsInfo = parmObj as TermSettingsInfo;
                RMTerm term = TermDao.GetRMTermByTermId(termSettingsInfo.tId);
                if (term != null)
                {
                    int termSetId = term.TermSetId;
                    RMTermSet termset = TermSetDao.GetRMTermSet(termSetId);
                    if (termset != null)
                    {
                        List<Guid> termSetIds = new List<Guid>();
                        termSetIds.Add(termset.UniqueId);
                        result.Add(SecurityTermLevel.TermSet, termSetIds);
                    }
                }
            }
            if (action.Equals("DeleteRootTerms"))
            {
                int termSetId;
                if (int.TryParse(parmObj.ToString(), out termSetId))
                {
                    RMTermSet termset = TermSetDao.GetRMTermSet(termSetId);
                    if (termset != null)
                    {
                        List<Guid> termSetIds= new List<Guid>();
                        termSetIds.Add(termset.UniqueId);
                        result.Add(SecurityTermLevel.TermSet, termSetIds);
                    }
                }
            }
            if (action.Equals("OperatorTerm"))
            {
                int termId;
                if (int.TryParse(parmObj.ToString(), out termId))
                {
                    RMTerm term = TermDao.GetRMTermByTermId(termId);
                    if (term != null)
                    {
                        int termSetId = term.TermSetId;
                        RMTermSet termset = TermSetDao.GetRMTermSet(termSetId);
                        if (termset != null)
                        {
                            List<Guid> termSetIds = new List<Guid>();
                            termSetIds.Add(termset.UniqueId);
                            result.Add(SecurityTermLevel.TermSet, termSetIds);
                        }
                    }
                }
            }
            if ((action.Equals("BrowseTermUsageTermTree") || action.Equals("GetChildrenTreeNodes")) && parmObj as TreePage != null)
            {
                TreePage treePage = parmObj as TreePage;
                if (treePage.NodeType == "Root" || treePage.NodeType == "TermGroup")
                {
                    // do nothing
                }
                else if (treePage.NodeType == "TermSet")
                {
                    int termSetId;
                    if (int.TryParse(treePage.NodeId, out termSetId))
                    {
                        RMTermSet termset = TermSetDao.GetRMTermSet(termSetId);
                        if (termset != null)
                        {
                            List<Guid> termSetIds = new List<Guid>();
                            termSetIds.Add(termset.UniqueId);
                            result.Add(SecurityTermLevel.TermSet, termSetIds);
                        }
                    }
                }
                else if (treePage.NodeType == "Term")
                {
                    int termId;
                    if (int.TryParse(treePage.NodeId, out termId))
                    {
                        RMTerm term = TermDao.GetRMTermByTermId(termId);
                        if (term != null)
                        {
                            List<Guid> termIds = new List<Guid>();
                            termIds.Add(term.UniqueId);
                            result.Add(SecurityTermLevel.Term, termIds);
                        }
                    }
                }
            }
            if (action.Equals("BrowseTermManageTree") && parmObj as TreePage != null)
            {
                TreePage treePage = parmObj as TreePage;
                if (treePage.NodeType == "Term")
                {
                    int termId;
                    if (int.TryParse(treePage.NodeId, out termId))
                    {
                        RMTerm term = TermDao.GetRMTermByTermId(termId);
                        if (term != null)
                        {
                            List<Guid> termIds = new List<Guid>();
                            termIds.Add(term.UniqueId);
                            result.Add(SecurityTermLevel.Term, termIds);
                        }
                    }
                }
            }
            if (action.Equals("TreeViewChildrenNodes"))
            {
                TermTreeView termTreeView = parmObj as TermTreeView;
                int termId;
                if (int.TryParse(termTreeView.TermId, out termId))
                {
                    RMTerm term = TermDao.GetRMTermByTermId(termId);
                    if (term != null)
                    {
                        int termSetId = term.TermSetId;
                        RMTermSet termset = TermSetDao.GetRMTermSet(termSetId);
                        if (termset != null)
                        {
                            List<Guid> termSetIds = new List<Guid>();
                            termSetIds.Add(termset.UniqueId);
                            result.Add(SecurityTermLevel.TermSetForPhysicalView, termSetIds);
                            //Valid by content sources settings
                        }
                    }
                }
            }
            if (action.Equals("ChangeTerm") && parmObj as ChangeTermDto != null)
            {
                ChangeTermDto dto = parmObj as ChangeTermDto;
                if (dto.TermInfo != null)
                {
                    if (dto.PhyRecordIds?.Count > 0)
                    {
                        result.Add(SecurityTermLevel.TermForPhysicalView, new List<Guid> { dto.TermInfo.UniqueId });
                        //Valid by content sources settings
                    }
                }
            }
            if (action.Equals("QueryDataList") && parmObj as ExplorerQueryDto != null)
            {
                ExplorerQueryDto dto = parmObj as ExplorerQueryDto;
                if (dto != null && dto.FilterOption != null && dto.FilterOption.TermIds != null)
                {
                    if (dto.FilterOption.TermIds.Count > 0)
                    {
                        result.Add(SecurityTermLevel.Term, dto.FilterOption.TermIds.Where(o => o.HasValue).Select(t => t.Value).ToList());
                    }
                }
            }
            if (action.Equals("QueryDataListV2") && parmObj as ExplorerQueryV2Dto != null)
            {
                ExplorerQueryV2Dto dto = parmObj as ExplorerQueryV2Dto;
                if (dto != null && dto.QueryOption != null && dto.QueryOption.FilterOption != null && dto.QueryOption.FilterOption.TermIds != null)
                {
                    if (dto.QueryOption.FilterOption.TermIds.Count > 0)
                    { 
                        result.Add(SecurityTermLevel.Term, dto.QueryOption.FilterOption.TermIds);
                    }
                }
            }
            if (action.Equals("AddOrUpdatePhysicalObject") && parmObj as PhysicalObjectDto != null)
            {
                PhysicalObjectDto dto = parmObj as PhysicalObjectDto;
                if (dto != null && dto.TermId != Guid.Empty)
                {
                    result.Add(SecurityTermLevel.TermForPhysicalView, new List<Guid> { dto.TermId });
                    //Valid by content sources settings
                }
            }
            if (action.Equals("PhysicalMove"))
            {
                //if (parmObj is PhysicalMoveDto dto && dto != null && dto.SourcePhyRecordIds.Count > 0)
                //{
                //    ExplorerDao explorerDao = new ExplorerDao();
                //    List<Record> records = explorerDao.GetRecordByIds(dto.SourcePhyRecordIds);
                //    var usedTermIds = records.Select(r => r.TermId).Distinct().ToList();
                //    result.Add(SecurityTermLevel.TermForPhysicalView, usedTermIds);
                //}
            }
            
            return result;
        }


    }
}