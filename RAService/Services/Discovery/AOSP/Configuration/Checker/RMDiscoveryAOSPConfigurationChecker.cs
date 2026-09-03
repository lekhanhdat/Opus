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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Discovery.Model.Configuration.AOSP;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Office365;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao.Discovery.AOSP;
using AvePoint.RA.DB.Dao.Discovery.Impl.AOSP;
using AvePoint.RA.Service.Services.Discovery.Checker.Rule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.AOSP.Configuration.Checker
{
    public class RMDiscoveryAOSPConfigurationChecker
    {
        private readonly IRMDiscoveryAOSPNodeDao _nodeDao = new RMDiscoveryAOSPNodeDao();

        private readonly RMDiscoveryAOSPConfigurationInfo _configurationInfo;

        public RMDiscoveryAOSPConfigurationChecker(RMDiscoveryAOSPConfigurationInfo configurationInfo)
        {
            _configurationInfo = configurationInfo;
        }

        public async Task<(bool Succeed, string Message)> CheckAsync()
        {
            if (!(await CheckScopeInfoAsync() && CheckSizeRangInfo() && CheckWithoutInDataRangeInfo() && CheckInactiveDefinition() && CheckRotDefinition()))
            {
                return (false, "RM_FA_Discovery_RunJobFailed");
            }

            return (true, "");
        }

        public async Task<(bool Succeed, string Message)> CheckAppAsync(List<string> o365TenantIds, string appProfileId)
        {
            if (!await CheckHasEnoughAppsAsync(o365TenantIds, appProfileId))
            {
                return (false, "RM_FA_Discovery_NotFountAnyApp");
            }

            return(true, "");
        }

        private async Task<bool> CheckScopeInfoAsync()
        {
            var scopeInfo = _configurationInfo.ScopeInfo;
            if (!Enum.IsDefined(typeof(RMDiscoveryAOSPScopeType), scopeInfo.ScopeType) || scopeInfo.ScopeType == RMDiscoveryAOSPScopeType.None)
            {
                return false;
            }

            //if (scopeInfo.ScopeType == RMDiscoveryAOSPScopeType.Specify)
            //{
            //    if (!scopeInfo.SpecifyContainerIds.Any() || scopeInfo.SpecifyContainerIds.Any(item => item == Guid.Empty))
            //    {
            //        return false;
            //    }

            //    var containers = await _nodeDao.GetOpusContainersAsync(scopeInfo.SpecifyContainerIds);
            //    if (containers.Count != scopeInfo.SpecifyContainerIds.Count)
            //    {
            //        return false;
            //    }

            //    scopeInfo.ContentSources = [];
            //}

            if (scopeInfo.ScopeType == RMDiscoveryAOSPScopeType.DataSource)
            {
                if (!scopeInfo.ContentSources.Any())
                {
                    return false;
                }

                if (scopeInfo.ContentSources.Any(item => item != SourceFlag.SharePoint && item != SourceFlag.OneDrive))
                {
                    return false;
                }

                //scopeInfo.SpecifyContainerIds = [];
            }

            return true;
        }

        //private bool CheckExclusionInfo()
        //{
        //    var exclusionInfo = _configurationInfo.ExclusionInfo;
        //    return exclusionInfo.SharePointOnlineSiteSizeLimit >= 0 && exclusionInfo.OneDriveSiteSizeLimit >= 0;
        //}

        private bool CheckSizeRangInfo()
        {
            var sizeRangeInfoes = _configurationInfo.SizeRangeInfoes;
            if (sizeRangeInfoes.Count > 5 || sizeRangeInfoes.Count < 1) return false;
            for (int i = 0; i < sizeRangeInfoes.Count; i++)
            {
                var cur = sizeRangeInfoes[i];
                if (i > 0)
                {
                    var pre = sizeRangeInfoes[i - 1];
                    if (pre.LessThan > cur.GenerateEqual) return false;
                }
                if (cur.GenerateEqual >= cur.LessThan || cur.GenerateEqual < 0 || cur.LessThan < 0) return false;
            }
            return true;
        }

        private bool CheckWithoutInDataRangeInfo()
        {
            var withoutInDateDataInfoes = _configurationInfo.DateRangeInfoes;
            if (withoutInDateDataInfoes.Count > 10 || withoutInDateDataInfoes.Count < 1) return false;
            for (int i = 0; i < withoutInDateDataInfoes.Count; i++)
            {
                withoutInDateDataInfoes[i].UnitType = RMDiscoveryWithoutInUnitType.Month;
                var cur = withoutInDateDataInfoes[i];
                if (i > 0)
                {
                    var pre = withoutInDateDataInfoes[i - 1];
                    if (pre.UnitType > cur.UnitType)
                    {
                        return false;
                    }
                    else if (pre.UnitType == cur.UnitType)
                    {
                        if (pre.Unit >= cur.Unit) return false;
                    }
                }
                if (cur.Unit < 0) return false;
            }
            return true;
        }

        private bool CheckInactiveDefinition()
        {
            var inactiveDefinition = _configurationInfo.InactiveDefinition;
            if (!inactiveDefinition.Enable)
            {
                inactiveDefinition.Rules = new();
                return true;
            }

            return RMDiscoveryRuleDefinitionChecker.Create(inactiveDefinition.Rules)
                .CheckEmpty().CheckName().CheckOrder().CheckEnable()
                .CheckKind(RMDiscoveryRuleDefinitionKind.Inactive).CheckAnalyseMethod(RMDiscoveryRuleAnalyseMethod.Version)
                .Check();
        }

        private bool CheckRotDefinition()
        {
            var rotDefinition = _configurationInfo.RotDefinition;
            if (!rotDefinition.Enable)
            {
                rotDefinition.RedundantRules = new();
                rotDefinition.ObsoleteRules = new();
                rotDefinition.TrivialRules = new();
                return true;
            }

            var rotRules = new List<List<RMDiscoveryRuleDefinition>>
            {
                rotDefinition.RedundantRules,
                rotDefinition.ObsoleteRules,
                rotDefinition.TrivialRules
            };

            var isPassed = RMDiscoveryRuleDefinitionChecker.Create(rotRules.SelectMany(item => item))
                .CheckEmpty().CheckEnable()
                .CheckKind(RMDiscoveryRuleDefinitionKind.ROT)
                .CheckAnalyseMethod(RMDiscoveryRuleAnalyseMethod.Document, RMDiscoveryRuleAnalyseMethod.Version, RMDiscoveryRuleAnalyseMethod.DuplicatedDocument)
                .Check();

            if (!isPassed)
            {
                return false;
            }

            foreach (var rotRule in rotRules)
            {
                if (rotRule.Any())
                {
                    isPassed &= RMDiscoveryRuleDefinitionChecker.Create(rotRule)
                    .CheckOrder().CheckName().Check();
                }
            }

            return isPassed;
        }

        private async Task<bool> CheckHasEnoughAppsAsync(List<string> o365TenantIds, string appProfileId)
        {
            //var spSupportNodeLevels = new HashSet<NodeLevel>()
            //{
            //    NodeLevel.O365GroupSites,
            //    NodeLevel.SiteCollection
            //};
            //var odSupportNodeLevels = new HashSet<NodeLevel>()
            //{
            //    NodeLevel.SkyDrivePro
            //};

            //var supportNodeLevels = new HashSet<NodeLevel>();
            ////var specifyContainerIds = _configurationInfo.ScopeInfo.SpecifyContainerIds;

            //if (_configurationInfo.ScopeInfo.ScopeType == RMDiscoveryAOSPScopeType.DataSource)
            //{
            //    if (_configurationInfo.ScopeInfo.ContentSources.Contains(SourceFlag.SharePoint))
            //    {
            //        supportNodeLevels.UnionWith(spSupportNodeLevels);
            //    }

            //    if (_configurationInfo.ScopeInfo.ContentSources.Contains(SourceFlag.OneDrive))
            //    {
            //        supportNodeLevels.UnionWith(odSupportNodeLevels);
            //    }
            //}
            //else
            //{
            //    supportNodeLevels.UnionWith(spSupportNodeLevels);
            //    supportNodeLevels.UnionWith(odSupportNodeLevels);
            //}

            //var o365TenantIds = (await _nodeDao.GetOpusO365TenantIdsByContainerAsync(supportNodeLevels.ToList(), specifyContainerIds.ToArray())).ToHashSet();
            //var avaliableApps = RMAosApiClient.GetAllProfiles(TenantLocalValue.LogonGroupId);
            foreach(var o365TenantId in o365TenantIds)
            {
                if(!RMAosApiClient.ExistAppProfile(TenantLocalValue.LogonGroupId, o365TenantId, appProfileId, true))
                {
                    return false;
                }
            }
      
            //var avaliableAppO365TenantIds = avaliableApps.Select(item => new Guid(item.TenantId)).ToHashSet();
            //var intersectedO365TenantIds = o365TenantIds.Intersect(avaliableAppO365TenantIds).ToHashSet();
            //return intersectedO365TenantIds.Count == o365TenantIds.Count;
            return true;
        }
    }
}
