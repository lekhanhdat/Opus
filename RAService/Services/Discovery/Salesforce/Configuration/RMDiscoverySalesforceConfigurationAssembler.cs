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

using System.Collections.Generic;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Salesforce;
using AvePoint.RA.Contract.Discovery.Model.Query;

namespace AvePoint.RA.Service.Services.Discovery.Salesforce.Configuration
{
    public class RMDiscoverySalesforceConfigurationAssembler
    {
        private RMDiscoverySalesforceScopeInfo _scopeInfo;
        private List<RMDiscoverySizeRangeDataInfo> _listSizeRange;
        private List<RMDiscoveryWithoutInDateDataInfo> _listWithoutInDate;

        public static RMDiscoverySalesforceConfigurationAssembler Instance => new();

        public RMDiscoverySalesforceConfigurationAssembler AddScopeInfo(RMDiscoverySalesforceScopeInfo scopeInfo)
        {
            _scopeInfo = scopeInfo;
            return this;
        }
        public RMDiscoverySalesforceConfigurationAssembler AddSizeRangeInfo(List<RMDiscoverySizeRangeDataInfo> listSizeRange)
        {
            _listSizeRange = listSizeRange;
            _listSizeRange?.RemoveAt(_listSizeRange.Count - 1);
            return this;
        }
        public RMDiscoverySalesforceConfigurationAssembler AddWithoutInDateInfo(List<RMDiscoveryWithoutInDateDataInfo> listWithoutInDate)
        {
            _listWithoutInDate = listWithoutInDate;
            return this;
        }

        public RMDiscoverySalesforceConfigurationInfo Assemble()
        {
            var res = new RMDiscoverySalesforceConfigurationInfo()
            {
                ScopeInfo = _scopeInfo,
                SizeRangeInfoes = _listSizeRange,
                DateRangeInfoes = _listWithoutInDate
            };
            return res;
        }
    }
}
