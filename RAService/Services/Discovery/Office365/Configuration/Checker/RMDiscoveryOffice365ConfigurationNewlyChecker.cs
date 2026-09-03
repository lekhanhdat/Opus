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
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Contract.Discovery.Model.Configuration;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Office365;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Service.Services.Discovery.Checker.Rule;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.CommonUtil;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Configuration.Checker
{
    public class RMDiscoveryOffice365ConfigurationNewlyChecker
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryOffice365ConfigurationNewlyChecker));

        private readonly IRMDiscoveryOffice365NodeDao _nodeDao = new RMDiscoveryOffice365NodeDao();

        private readonly RMDiscoveryOffice365ConfigurationInfo _configurationInfo;

        public RMDiscoveryOffice365ConfigurationNewlyChecker(RMDiscoveryOffice365ConfigurationInfo configurationInfo)
        {
            _configurationInfo = configurationInfo;
        }

        public async Task<(bool Succeed, string Message)> CheckAsync()
        {
            if (_configurationInfo == null)
            {
                _logger.Warn("Office 365 newly configuration check failed: configuration is null.");
                return (false, "RM_FA_Discovery_RunJobFailed");
            }

            if (!(await CheckScopeInfoAsync() && CheckSizeRangInfo() && CheckWithoutInDataRangeInfo() && CheckInactiveDefinition() && CheckRotDefinition()))
            {
                _logger.Warn("Office 365 newly configuration check failed in base validation.");
                return (false, "RM_FA_Discovery_RunJobFailed");
            }

            if (!await CheckHasEnoughAppsAsync())
            {
                return (false, "RM_FA_Discovery_NotFountAnyApp");
            }

            return (true, "");
        }

        private async Task<bool> CheckScopeInfoAsync()
        {
            var scopeInfo = _configurationInfo.ScopeInfo;
            if (scopeInfo == null)
            {
                _logger.Warn("Scope check failed: scope info is null.");
                return false;
            }

            if (!Enum.IsDefined(typeof(RMDiscoveryOffice365ScopeType), scopeInfo.ScopeType) || scopeInfo.ScopeType == RMDiscoveryOffice365ScopeType.None)
            {
                _logger.Warn($"Scope check failed: invalid scope type [{scopeInfo.ScopeType}].");
                return false;
            }

            if (scopeInfo.ScopeType == RMDiscoveryOffice365ScopeType.Specify)
            {
                if (scopeInfo.SpecifyContainerIds == null || !scopeInfo.SpecifyContainerIds.Any() || scopeInfo.SpecifyContainerIds.Any(item => item == Guid.Empty))
                {
                    _logger.Warn("Scope check failed: specify container ids are empty or contain empty Guid.");
                    return false;
                }

                var containers = await _nodeDao.GetOpusContainersAsync(scopeInfo.SpecifyContainerIds);
                if (containers.Count != scopeInfo.SpecifyContainerIds.Count)
                {
                    _logger.Warn($"Scope check failed: specify container ids count [{scopeInfo.SpecifyContainerIds.Count}] does not match existing container count [{containers.Count}].");
                    return false;
                }

                scopeInfo.ContentSources = [];
            }

            if (scopeInfo.ScopeType == RMDiscoveryOffice365ScopeType.DataSource)
            {
                if (scopeInfo.ContentSources == null || !scopeInfo.ContentSources.Any())
                {
                    _logger.Warn("Scope check failed: content sources are empty in data source scope.");
                    return false;
                }

                if (scopeInfo.ContentSources.Any(item => item != SourceFlag.SharePoint && item != SourceFlag.OneDrive))
                {
                    _logger.Warn($"Scope check failed: content sources contain unsupported value. Sources: [{string.Join(",", scopeInfo.ContentSources)}].");
                    return false;
                }

                scopeInfo.SpecifyContainerIds = [];
            }

            return true;
        }

        private bool CheckExclusionInfo()
        {
            var exclusionInfo = _configurationInfo.ExclusionInfo;
            return exclusionInfo.SharePointOnlineSiteSizeLimit >= 0 && exclusionInfo.OneDriveSiteSizeLimit >= 0;
        }

        private bool CheckSizeRangInfo()
        {
            var sizeRangeInfoes = _configurationInfo.SizeRangeInfoes;
            if (sizeRangeInfoes == null)
            {
                _logger.Warn("Size range check failed: size range list is null.");
                return false;
            }

            if (sizeRangeInfoes.Count > 5 || sizeRangeInfoes.Count < 1)
            {
                _logger.Warn($"Size range check failed: invalid size range count [{sizeRangeInfoes.Count}]. Allowed range is [1,5].");
                return false;
            }

            for (int i = 0; i < sizeRangeInfoes.Count; i++)
            {
                var cur = sizeRangeInfoes[i];
                if (i > 0)
                {
                    var pre = sizeRangeInfoes[i - 1];
                    if (pre.LessThan > cur.GenerateEqual)
                    {
                        _logger.Warn($"Size range check failed at index [{i}]: previous less-than [{pre.LessThan}] is greater than current generate-equal [{cur.GenerateEqual}].");
                        return false;
                    }
                }
                if (cur.GenerateEqual >= cur.LessThan || cur.GenerateEqual < 0 || cur.LessThan < 0)
                {
                    _logger.Warn($"Size range check failed at index [{i}]: invalid boundary values generate-equal [{cur.GenerateEqual}], less-than [{cur.LessThan}].");
                    return false;
                }
            }
            return true;
        }

        private bool CheckWithoutInDataRangeInfo()
        {
            var withoutInDateDataInfoes = _configurationInfo.DateRangeInfoes;
            if (withoutInDateDataInfoes == null)
            {
                _logger.Warn("Date range check failed: date range list is null.");
                return false;
            }

            if (withoutInDateDataInfoes.Count > 10 || withoutInDateDataInfoes.Count < 1)
            {
                _logger.Warn($"Date range check failed: invalid date range count [{withoutInDateDataInfoes.Count}]. Allowed range is [1,10].");
                return false;
            }

            for (int i = 0; i < withoutInDateDataInfoes.Count; i++)
            {
                withoutInDateDataInfoes[i].UnitType = RMDiscoveryWithoutInUnitType.Month;
                var cur = withoutInDateDataInfoes[i];
                if (i > 0)
                {
                    var pre = withoutInDateDataInfoes[i - 1];
                    if (pre.UnitType > cur.UnitType)
                    {
                        _logger.Warn($"Date range check failed at index [{i}]: previous unit type [{pre.UnitType}] is greater than current unit type [{cur.UnitType}].");
                        return false;
                    }
                    else if (pre.UnitType == cur.UnitType)
                    {
                        if (pre.Unit >= cur.Unit)
                        {
                            _logger.Warn($"Date range check failed at index [{i}]: previous unit [{pre.Unit}] must be less than current unit [{cur.Unit}] when unit type is same.");
                            return false;
                        }
                    }
                }
                if (cur.Unit < 0)
                {
                    _logger.Warn($"Date range check failed at index [{i}]: unit [{cur.Unit}] cannot be negative.");
                    return false;
                }
            }
            return true;
        }

        private bool CheckInactiveDefinition()
        {
            var inactiveDefinition = _configurationInfo.InactiveDefinition;
            if (inactiveDefinition == null)
            {
                _logger.Warn("Inactive definition check failed: inactive definition is null.");
                return false;
            }

            if (!inactiveDefinition.Enable)
            {
                inactiveDefinition.Rules = new();
                return true;
            }

            var isPassed = RMDiscoveryRuleDefinitionChecker.Create(inactiveDefinition.Rules)
                .CheckEmpty().CheckName().CheckOrder().CheckEnable()
                .CheckKind(RMDiscoveryRuleDefinitionKind.Inactive).CheckAnalyseMethod(RMDiscoveryRuleAnalyseMethod.Version)
                .Check();

            if (!isPassed)
            {
                _logger.Warn($"Inactive definition check failed: invalid inactive rules. Rule count [{inactiveDefinition.Rules?.Count ?? 0}].");
            }

            return isPassed;
        }

        private bool CheckRotDefinition()
        {
            var rotDefinition = _configurationInfo.RotDefinition;
            if (rotDefinition == null)
            {
                _logger.Warn("ROT definition check failed: ROT definition is null.");
                return false;
            }

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
                _logger.Warn("ROT definition check failed: basic ROT rule validation failed.");
                return false;
            }

            for (int i = 0; i < rotRules.Count; i++)
            {
                var rotRule = rotRules[i];
                if (rotRule.Count != 0)
                {
                    var isCurrentCategoryPassed = RMDiscoveryRuleDefinitionChecker.Create(rotRule)
                        .CheckOrder().CheckName().Check();
                    if (!isCurrentCategoryPassed)
                    {
                        _logger.Warn($"ROT definition check failed: ROT category index [{i}] rule order or name validation failed.");
                    }

                    isPassed &= isCurrentCategoryPassed;
                }
            }

            return isPassed;
        }

        private async Task<bool> CheckHasEnoughAppsAsync()
        {
            var spSupportNodeLevels = new HashSet<NodeLevel>()
            {
                NodeLevel.O365GroupSites,
                NodeLevel.SiteCollection
            };
            var odSupportNodeLevels = new HashSet<NodeLevel>()
            {
                NodeLevel.SkyDrivePro
            };

            var supportNodeLevels = new HashSet<NodeLevel>();
            var specifyContainerIds = _configurationInfo.ScopeInfo.SpecifyContainerIds;

            if (_configurationInfo.ScopeInfo.ScopeType == RMDiscoveryOffice365ScopeType.DataSource)
            {
                if (_configurationInfo.ScopeInfo.ContentSources.Contains(SourceFlag.SharePoint))
                {
                    supportNodeLevels.UnionWith(spSupportNodeLevels);
                }

                if (_configurationInfo.ScopeInfo.ContentSources.Contains(SourceFlag.OneDrive))
                {
                    supportNodeLevels.UnionWith(odSupportNodeLevels);
                }
            }
            else
            {
                supportNodeLevels.UnionWith(spSupportNodeLevels);
                supportNodeLevels.UnionWith(odSupportNodeLevels);
            }

            var o365TenantIds = (await _nodeDao.GetOpusO365TenantIdsByContainerAsync(supportNodeLevels.ToList(), specifyContainerIds.ToArray())).ToHashSet();
            var avaliableApps = RMAosApiClient.GetAllProfiles(TenantLocalValue.LogonGroupId);
            var avaliableAppO365TenantIds = avaliableApps.Select(item => new Guid(item.TenantId)).ToHashSet();

            var intersectedO365TenantIds = o365TenantIds.Intersect(avaliableAppO365TenantIds).ToHashSet();
            var isPassed = intersectedO365TenantIds.Count == o365TenantIds.Count;
            if (!isPassed)
            {
                var missingTenantCount = o365TenantIds.Except(avaliableAppO365TenantIds).Count();
                _logger.Warn($"App profile check failed: available app tenants [{avaliableAppO365TenantIds.Count}] do not cover required tenants [{o365TenantIds.Count}]. Missing tenant count [{missingTenantCount}].");
            }

            return isPassed;
        }
    }
}
