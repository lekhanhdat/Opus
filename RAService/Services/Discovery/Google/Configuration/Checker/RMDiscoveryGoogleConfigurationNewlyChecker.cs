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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Google;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.Service.Services.Discovery.Checker.Rule;

namespace AvePoint.RA.Service.Services.Discovery.Google.Configuration.Checker
{
    public class RMDiscoveryGoogleConfigurationNewlyChecker
    {
        private readonly IRMDiscoveryOffice365NodeDao _nodeDao = new RMDiscoveryOffice365NodeDao();

        private readonly RMDiscoveryGoogleConfigurationInfo _configurationInfo;

        public RMDiscoveryGoogleConfigurationNewlyChecker(RMDiscoveryGoogleConfigurationInfo configurationInfo)
        {
            _configurationInfo = configurationInfo;
        }

        public async Task<(bool Succeed, string Message)> CheckAsync()
        {
            if (!(await CheckScopeInfoAsync() && CheckSizeRangInfo() && CheckWithoutInDataRangeInfo() &&  CheckRotDefinition()))
            {
                return (false, "RM_FA_Discovery_RunJobFailed");
            }
            return (true, "");
        }

        private async Task<bool> CheckScopeInfoAsync()
        {
            var scopeInfo = _configurationInfo.ScopeInfo;
            if (!Enum.IsDefined(typeof(RMDiscoveryGoogleScopeType), scopeInfo.ScopeType) || scopeInfo.ScopeType == RMDiscoveryGoogleScopeType.None)
            {
                return false;
            }

            if (scopeInfo.ScopeType == RMDiscoveryGoogleScopeType.Specify)
            {
                if (!scopeInfo.SpecifyContainerIds.Any() || scopeInfo.SpecifyContainerIds.Any(item => item == Guid.Empty))
                {
                    return false;
                }

                var containers = await _nodeDao.GetOpusContainersAsync(scopeInfo.SpecifyContainerIds);
                if (containers.Count != scopeInfo.SpecifyContainerIds.Count)
                {
                    return false;
                }
            }

            return true;
        }

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
                .CheckAnalyseMethod(RMDiscoveryRuleAnalyseMethod.Document, RMDiscoveryRuleAnalyseMethod.Version, RMDiscoveryRuleAnalyseMethod.DuplicatedDocument, RMDiscoveryRuleAnalyseMethod.GoogleDocument)
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
    }
}
