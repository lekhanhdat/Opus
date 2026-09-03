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
using AvePoint.RA.Contract.Discovery.Model.Query.Office365.Parameter;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Discovery.Model.Query.AOSP.Parameter
{
    [DataContract]
    public class RMDiscoveryAOSPQueryParameter
    {
        [DataMember]
        [JsonProperty("o365TenantId")]
        public Guid O365TenantId { get; set; }
        [DataMember]
        [JsonProperty("dataType")]
        public RMDiscoveryQueryDataType DataType { get; set; }
        [DataMember]
        [JsonProperty("fileExtensionQueryParameter")]
        public RMDiscoveryAOSPFileExtensionQueryParameter FileExtensionQueryParameter { get; set; }
        [DataMember]
        [JsonProperty("sizeRangeQueryParameter")]
        public RMDiscoveryAOSPSizeRangeQueryParameter SizeRangeQueryParameter { get; set; }
        [DataMember]
        [JsonProperty("withoutDateQueryParameter")]
        public RMDiscoveryAOSPWithoutDateQueryParameter WithoutDateQueryParameter { get; set; }
        [DataMember]
        [JsonProperty("nodeQueryParameter")]
        public RMDiscoveryAOSPNodeQueryParameter NodeQueryParameter { get; set; }
        [DataMember]
        [JsonProperty("rotRuleQueryParameter")]
        public RMDiscoveryAOSPROTRuleQueryParameter ROTRuleQueryParameter { get; set; }

        public string ToJsonInfo()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    [DataContract]
    public enum RMDiscoveryQueryDataType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Inactive = 1,
        [EnumMember]
        Rot = 2,
    }
}
