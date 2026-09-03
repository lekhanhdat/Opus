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
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.RA.Common;
using AvePoint.RA.Common.RMRuleManagement;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RATeams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Common.Filters
{
    public class ValidCreateRuleParameterFilter : BaseActionFilter
    {
        private RALogger logger = RALogger.GetInstance(typeof(ValidCreateRuleParameterFilter));

        public IUserService UserService => PlatformWindsorManager.GetService<IUserService>();

        public ILicenseHelperService LicenseHelperService => PlatformWindsorManager.GetService<ILicenseHelperService>();

        public IRMScopeRoleAssignmentDao RMScopeRoleAssignmentDao => PlatformWindsorManager.GetService<IRMScopeRoleAssignmentDao>();

        public IRMLocationDao RMLocationDao => PlatformWindsorManager.GetService<IRMLocationDao>();

        private static readonly List<ArchiverFilterRuleType> PropertyBagCriterias = [
            ArchiverFilterRuleType.PropertyBagText,
            ArchiverFilterRuleType.PropertyBagBoolean,
            ArchiverFilterRuleType.PropertyBagNumber,
            ArchiverFilterRuleType.PropertyBagDateTime
        ];
        public ValidCreateRuleParameterFilter()
        {
        }
        protected override async Task OnActionAuthenticatedAsync(ActionExecutingContext actionContext)
        {
            var ruleInfos = actionContext.ActionArguments.Values.First() as RMRuleInfos;
            if (!(await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.SPOAdmin)) && !(await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMSOPermissionMasks.SPOAdmin)))
            {
                if (LicenseHelperService.HasOpusILLicense && ruleInfos.ModelType == GCommon.Contract.StorageOptimization.Object.RuleModel.Records)
                {
                    Dictionary<SecurityTermLevel, List<Guid>> nodelevelAndUniqueId = new Dictionary<SecurityTermLevel, List<Guid>>();
                    List<string> userAndGroupUserIds = await UserService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
                    if ((bool)ruleInfos?.IsSpSource && ruleInfos.MoveDto != null && ruleInfos.MoveDto.SPTree != null)
                    {
                        if (!ruleInfos.MoveDto.IsSpecifyLocation)
                        {
                            string containerId = RuleSPTreeUtil.GetContainerNode(ruleInfos.MoveDto.SPTree) == null ? string.Empty : RuleSPTreeUtil.GetContainerNode(ruleInfos.MoveDto.SPTree).Id;
                            if (TeamsPermissionHelper.HasUpgradeTeamsFeature() && containerId == "41cfe969-e07b-45cb-a7d0-b022f967e929")
                            {
                                var contentSourcePermission = RMScopeRoleAssignmentDao.GetSourceFlagsByUser(userAndGroupUserIds);
                                if (!contentSourcePermission.Contains((int)SourceFlag.Teams))
                                {
                                    logger.Info("No access on container.");
                                    actionContext.Result = new ObjectResult("Access  Denied(container)") { StatusCode = (int)HttpStatusCode.Forbidden };
                                }
                            }
                            else if (!RMScopeRoleAssignmentDao.HavePermissionOnContainerId(new Guid(containerId), userAndGroupUserIds))
                            {
                                logger.Info("No access on container.");
                                actionContext.Result = new ObjectResult("Access  Denied(container)") { StatusCode = (int)HttpStatusCode.Forbidden };
                            }
                        }
                    }
                    if (ruleInfos.IsExoSource && ruleInfos.EXORule != null && ruleInfos.EXORule.MoveDto != null && ruleInfos.EXORule.MoveDto.SPTree != null)
                    {
                        if (!ruleInfos.EXORule.MoveDto.IsSpecifyLocation)
                        {
                            string containerId = RuleSPTreeUtil.GetContainerNode(ruleInfos.EXORule.MoveDto.SPTree) == null ? string.Empty : RuleSPTreeUtil.GetContainerNode(ruleInfos.EXORule.MoveDto.SPTree).Id;
                            if (!RMScopeRoleAssignmentDao.HavePermissionOnContainerId(new Guid(containerId), userAndGroupUserIds))
                            {
                                logger.Info("No access on container.");
                                actionContext.Result = new ObjectResult("Access  Denied(container)") { StatusCode = (int)HttpStatusCode.Forbidden };
                            }
                        }
                    }
                    if(ruleInfos.IsPhySource && ruleInfos.PhysicalRule?.MoveDto?.PhysicalTreeNode != null)
                    {
                        if (!ruleInfos.PhysicalRule.MoveDto.IsSpecifyLocation)
                        {
                            Guid topLocationId = RMLocationDao.LoadTopLocationIdBySubLocation(new Guid(ruleInfos.PhysicalRule.MoveDto.PhysicalTreeNode.LocationId));
                            if (!RMScopeRoleAssignmentDao.HavePermissionOnContainerId(topLocationId, userAndGroupUserIds))
                            {
                                logger.Info("No access on container.");
                                actionContext.Result = new ObjectResult("Access  Denied(container)") { StatusCode = (int)HttpStatusCode.Forbidden };
                            }
                        }
                    }
                }
            }
            if (ruleInfos.ModelType == GCommon.Contract.StorageOptimization.Object.RuleModel.SOArchiver)
            {
                if (ruleInfos.EXORule != null || ruleInfos.FSRule != null || ruleInfos.AzureFileRule != null || ruleInfos.PhysicalRule != null || ruleInfos.ConnectorRule != null || ruleInfos.SPLocalRule != null)
                {
                    logger.Info("No access on source.");
                    actionContext.Result = new ObjectResult("Access  Denied(source)") { StatusCode = (int)HttpStatusCode.Forbidden };
                    return;
                }
            }
            if (!IsRuleSupportPropertyBagCriteria(ruleInfos))
            {
                actionContext.Result = new ObjectResult("Parent site property criteria only supports SharePoint content source and document level rule") { StatusCode = (int)HttpStatusCode.OK };
                return;
            }
            if (!CheckRuleSupportLatestSubfolderDisposalDate(ruleInfos))
            {
                actionContext.Result = new ObjectResult("Latest sub folder Disposal action due date only supports physical content source and physical box level rule") { StatusCode = (int)HttpStatusCode.OK };
                return;
            }
            if (!IsValidateCalculateADDActionRule(ruleInfos))
            {
                actionContext.Result = new ObjectResult("Calcualate action due date criteria only supports Physical Records content source with physical folder level") { StatusCode = (int)HttpStatusCode.OK };
                return;
            }
            return;
        }
        private bool IsRuleSupportPropertyBagCriteria(RMRuleInfos ruleInfoes)
        {
            var rulesNotSupportingPropertyBag = new[]
            {
                ruleInfoes.EXORule,
                ruleInfoes.PhysicalRule,
                ruleInfoes.FSRule,
                ruleInfoes.SPLocalRule,
                ruleInfoes.OneDriveRule,
                ruleInfoes.AzureFileRule,
                ruleInfoes.ConnectorRule,
                ruleInfoes.GoogleDriveRule,
                ruleInfoes.TeamsRule
            };

            if (rulesNotSupportingPropertyBag.Any(r => r?.RuleFilters?.Any(f => PropertyBagCriterias.Contains(f.RuleType)) == true))
            {
                return false;
            }

            var filterSPORule = ruleInfoes.RuleFilters?.FirstOrDefault(x => PropertyBagCriterias.Contains(x.RuleType));

            if (filterSPORule != null && filterSPORule.Level != PolicyLevel.Document)
            {
                return false;
            }

            return true;
        }

        private bool CheckRuleSupportLatestSubfolderDisposalDate(RMRuleInfos ruleInfos)
        {
            bool HasLatestSubfolderDisposalDateFilter(IEnumerable<RuleFilter> filters)
            {
                return filters?.Any(f => f.RuleType == ArchiverFilterRuleType.LastestSubfolderDisposalDate) == true;
            }

            if (HasLatestSubfolderDisposalDateFilter(ruleInfos.RuleFilters))
            {
                return false;
            }

            // Content sources that do not support this filter
            var unsupportedRules = new[]
            {
                ruleInfos.EXORule,
                ruleInfos.FSRule,
                ruleInfos.SPLocalRule,
                ruleInfos.OneDriveRule,
                ruleInfos.AzureFileRule,
                ruleInfos.ConnectorRule,
                ruleInfos.GoogleDriveRule,
                ruleInfos.TeamsRule,
                ruleInfos.BoxRule,
            };

            if (unsupportedRules.Any(r => HasLatestSubfolderDisposalDateFilter(r?.RuleFilters)))
            {
                return false;
            }

            var physicalRule = ruleInfos.PhysicalRule;

            if (physicalRule == null)
            {
                return true;
            }

            // Physical rule:
            // - Non-Box level: filter is not supported
            if ((physicalRule.RuleLevel != PolicyLevel.PhysicalBox || ruleInfos.RuleLevel != PolicyLevel.List) &&
                HasLatestSubfolderDisposalDateFilter(physicalRule.RuleFilters))
            {
                return false;
            }

            // - List level: only OlderThan condition is supported
            if (physicalRule.RuleLevel == PolicyLevel.PhysicalBox &&
                physicalRule.RuleFilters?.Any(r =>
                    r.RuleType == ArchiverFilterRuleType.LastestSubfolderDisposalDate &&
                    r.Condition != ArchiverFilterCondition.OlderThan) == true)
            {
                return false;
            }

            return true;
        }
        private bool IsValidateCalculateADDActionRule(RMRuleInfos ruleInfoes)
        {
            var nonPhysicalRules = new[]
            {
                ruleInfoes.EXORule,
                ruleInfoes.FSRule,
                ruleInfoes.SPLocalRule,
                ruleInfoes.OneDriveRule,
                ruleInfoes.AzureFileRule,
                ruleInfoes.BoxRule,
                ruleInfoes.GoogleDriveRule,
                ruleInfoes.TeamsRule,
                ruleInfoes.ConnectorRule
            };
            if (nonPhysicalRules.Any(r => r?.IsCalculationDisposalDate == true) || ruleInfoes.IsCalculationDisposalDate == true)
            { 
                return false;
            }
            if(ruleInfoes.PhysicalRule != null && ruleInfoes.PhysicalRule.IsCalculationDisposalDate)
            {
                if (ruleInfoes.PhysicalRule.RuleLevel == PolicyLevel.PhysicalFile 
                    && ruleInfoes.PhysicalRule.RuleFilters.Any(f => f.Condition == ArchiverFilterCondition.OlderThan && f.RuleType == ArchiverFilterRuleType.ModifiedTime))
                {
                    return true;
                }
                return false;
            }
            return true;
        }
    }
}
