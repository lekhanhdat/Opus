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
using AvePoint.RA.Common.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RADiscovery.Query.Parameter
{
    public class DiscoveryQueryParameter
    {
        [JsonProperty("o365TenantId")]
        public Guid O365TenantId { get; set; }

        [JsonProperty("dataType")]
        public DiscoveryQueryDataType DataType { get; set; }

        [JsonProperty("fileExtensionQueryParameter")]
        public DiscoveryFileExtensionQueryParameter FileExtensionQueryParameter { get; set; }

        [JsonProperty("sizeRangeQueryParameter")]
        public DiscoverySizeRangeQueryParameter SizeRangeQueryParameter { get; set; }

        [JsonProperty("withoutDateQueryParameter")]
        public DiscoveryWithoutDateQueryParameter WithoutDateQueryParameter { get; set; }

        [JsonProperty("nodeQueryParameter")]
        public DiscoveryNodeQueryParameter NodeQueryParameter { get; set; }

        [JsonProperty("rotRuleQueryParameter")]
        public DiscoveryROTRuleQueryParameter ROTRuleQueryParameter { get; set; }

        [JsonProperty("needCalculateTotalDataTypes")]
        public List<DiscoveryTotalDataType> NeedCalculateTotalDataTypes { get; set; } = new();

        public string GetJsonInfo()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public enum DiscoveryQueryDataType
    {
        None = 0,
        Inactive = 1,
        Rot = 2,
    }

    public enum DiscoveryTotalDataType
    {
        None = 0,
        SizeAndCount = 1,
        Sites = 2,
    }
}
