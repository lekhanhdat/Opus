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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Discovery.Model.Rule.Condition
{
    public enum RMDiscoveryFileSizeConditionType
    {
        None = 0,
        LessThanEquals = 1,
        GreaterThanEquals = 2,
        LessThan = 3,
        GreaterThan = 4,
        Equals = 5
    }

    public class RMDiscoveryFileSizeConditionValue
    {
        [JsonProperty("unit")]
        public int Unit { get; set; }

        [JsonProperty("unitType")]
        public RMDiscoveryFileSizeUnitType UnitType { get; set; }
    }

    public enum RMDiscoveryFileSizeUnitType
    {
        None = 0,
        KB = 1,
        MB = 2,
        GB = 3,
    }
}
