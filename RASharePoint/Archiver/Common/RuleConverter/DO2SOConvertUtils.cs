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
using AvePoint.RA.Contract.Discovery.Model.Rule.Condition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.Archiver.Common.RuleConverter
{
    internal class DO2SOConvertUtils
    {
        public static PolicyValueUnit ConvertFileSizeUnitType(RMDiscoveryFileSizeUnitType discoveryFileSizeUnitType)
        {
            if (discoveryFileSizeUnitType == RMDiscoveryFileSizeUnitType.KB)
            {
                return PolicyValueUnit.KB;
            }
            else if (discoveryFileSizeUnitType == RMDiscoveryFileSizeUnitType.MB)
            {
                return PolicyValueUnit.MB;
            }
            else if (discoveryFileSizeUnitType == RMDiscoveryFileSizeUnitType.GB)
            {
                return PolicyValueUnit.GB;
            }
            else
            {
                return PolicyValueUnit.None;
            }
        }

        public static PolicyValueUnit ConvertDateTimeUnitType(RMDiscoveryDateUnitType discoveryDateTimeUnitType)
        {
            if (discoveryDateTimeUnitType == RMDiscoveryDateUnitType.Day)
            {
                return PolicyValueUnit.Days;
            }
            else if (discoveryDateTimeUnitType == RMDiscoveryDateUnitType.Week)
            {
                return PolicyValueUnit.Weeks;
            }
            else if (discoveryDateTimeUnitType == RMDiscoveryDateUnitType.Month)
            {
                return PolicyValueUnit.Months;
            }
            else if (discoveryDateTimeUnitType == RMDiscoveryDateUnitType.Year)
            {
                return PolicyValueUnit.Years;
            }
            else
            {
                return PolicyValueUnit.None;
            }
        }
    }
}
