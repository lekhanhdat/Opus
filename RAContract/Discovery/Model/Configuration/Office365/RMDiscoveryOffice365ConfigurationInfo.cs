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
using AvePoint.RA.Contract.Discovery.Model.Query;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Discovery.Model.Configuration.Office365
{
    [DataContract]
    public class RMDiscoveryOffice365ConfigurationInfo
    {
        [DataMember]
        [JsonProperty("scopeInfo")]
        public RMDiscoveryOffice365ScopeInfo ScopeInfo { get; set; } = new();

        [DataMember]
        [JsonProperty("exclusionInfo")]
        public RMDiscoveryExclusionInfo ExclusionInfo { get; set; }

        [DataMember]
        [JsonProperty("sizeRangeInfoes")]
        public List<RMDiscoverySizeRangeDataInfo> SizeRangeInfoes { get; set; }

        [DataMember]
        [JsonProperty("dateRangeInfoes")]
        public List<RMDiscoveryWithoutInDateDataInfo> DateRangeInfoes { get; set; }

        [DataMember]
        [JsonProperty("inactiveDefinition")]
        public RMDiscoveryOffice365InactiveDefinition InactiveDefinition { get; set; } = new();

        [DataMember]
        [JsonProperty("rotDefinition")]
        public RMDiscoveryOffice365RotDefinition RotDefinition { get; set; } = new();
    }
}
