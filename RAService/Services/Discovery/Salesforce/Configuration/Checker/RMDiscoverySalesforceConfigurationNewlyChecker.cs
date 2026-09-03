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
using System.Linq;
using System.Threading.Tasks;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Salesforce;
using AvePoint.RA.Contract.Salesforce;

namespace AvePoint.RA.Service.Services.Discovery.Salesforce.Configuration.Checker
{
    public class RMDiscoverySalesforceConfigurationNewlyChecker(RMDiscoverySalesforceConfigurationInfo configurationInfo)
    {
        private readonly IRMDiscoverySalesforceDataQueryService _dataQueryService = PlatformWindsorManager.GetService<IRMDiscoverySalesforceDataQueryService>();

        public async Task<(bool Succeed, string Message)> CheckAsync()
        {
            if (!(await CheckScopeInfoAsync() && CheckSizeRangInfo() && CheckWithoutInDataRangeInfo()))
            {
                return (false, "RM_FA_Discovery_RunJobFailed");
            }

            return (true, "");
        }
        
        private async Task<bool> CheckScopeInfoAsync()
        {
            var organizations = configurationInfo.ScopeInfo.Organizations;
            if (organizations.Count > 1) return false;
            var aosSalesforceOrganizations = await _dataQueryService.GetAllOrganizations();
            var notExistOrganizations = organizations.Where(organization => !aosSalesforceOrganizations.Contains(organization));
            return !notExistOrganizations.Any() && configurationInfo.ScopeInfo.Organizations.IsNotNullOrEmpty();
        }
        
        private bool CheckSizeRangInfo()
        {
            var sizeRangeInfos = configurationInfo.SizeRangeInfoes;
            if (sizeRangeInfos.Count > 5 || sizeRangeInfos.Count < 1) return false;
            for (int i = 0; i < sizeRangeInfos.Count; i++)
            {
                var cur = sizeRangeInfos[i];
                if (i > 0)
                {
                    var pre = sizeRangeInfos[i - 1];
                    if (pre.LessThan > cur.GenerateEqual) return false;
                }
                if (cur.GenerateEqual >= cur.LessThan || cur.GenerateEqual < 0 || cur.LessThan < 0) return false;
            }
            return true;
        }
        
        private bool CheckWithoutInDataRangeInfo()
        {
            var withoutInDateDataInfos = configurationInfo.DateRangeInfoes;
            if (withoutInDateDataInfos.Count > 10 || withoutInDateDataInfos.Count < 1) return false;
            for (int i = 0; i < withoutInDateDataInfos.Count; i++)
            {
                withoutInDateDataInfos[i].UnitType = RMDiscoveryWithoutInUnitType.Month;
                var cur = withoutInDateDataInfos[i];
                if (i > 0)
                {
                    var pre = withoutInDateDataInfos[i - 1];
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
    }
}
