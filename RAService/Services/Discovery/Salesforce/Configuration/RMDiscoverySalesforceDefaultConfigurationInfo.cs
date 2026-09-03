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
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Discovery.Model.Query;

namespace AvePoint.RA.Service.Services.Discovery.Salesforce.Configuration
{
    public class RMDiscoverySalesforceDefaultConfigurationInfo
    {
        public static List<RMDiscoveryWithoutInDateDataInfo> DEFAULT_DATE_RANGE_INFOES =>
        [
            new()
            {
                Unit = 1,
                UnitType = RMDiscoveryWithoutInUnitType.Month,
                Order = 0,
            },

            new()
            {
                Unit = 3,
                UnitType = RMDiscoveryWithoutInUnitType.Month,
                Order = 1,
            },

            new()
            {
                Unit = 5,
                UnitType = RMDiscoveryWithoutInUnitType.Month,
                Order = 2,
            },

            new()
            {
                Unit = 10,
                UnitType = RMDiscoveryWithoutInUnitType.Month,
                Order = 3,
            }

        ];

        public static List<RMDiscoverySizeRangeDataInfo> DEFAULT_SIZE_RANGE_INFOES =>
        [
            new()
            {
                GenerateEqual = 0,
                LessThan = 1,
                Order = 0,
                Name = "<1 MB",
            },

            new()
            {
                GenerateEqual = 1,
                LessThan = 50,
                Order = 1,
                Name = ">=1 MB",
            }

        ];
    }
}
