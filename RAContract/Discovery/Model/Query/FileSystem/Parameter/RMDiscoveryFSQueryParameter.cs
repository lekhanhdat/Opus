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
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace AvePoint.RA.Contract.Discovery.Model.Query.FileSystem.Parameter
{
    [DataContract]
    public class RMDiscoveryFSQueryParameter
    {
        [DataMember]
        [JsonProperty("dataType")]
        public RMDiscoveryQueryDataType DataType { get; set; }
        [DataMember]
        [JsonProperty("fileExtensionQueryParameter")]
        public RMDiscoveryFSFileExtensionQueryParameter FileExtensionQueryParameter { get; set; }
        [DataMember]
        [JsonProperty("sizeRangeQueryParameter")]
        public RMDiscoveryFSSizeRangeQueryParameter SizeRangeQueryParameter { get; set; }
        [DataMember]
        [JsonProperty("withoutDateQueryParameter")]
        public RMDiscoveryFSWithoutInDateQueryParameter WithoutDateQueryParameter { get; set; }
        [DataMember]
        [JsonProperty("nodeQueryParameter")]
        public RMDiscoveryFSNodeQueryParameter NodeQueryParameter { get; set; }
        [DataMember]
        [JsonProperty("rotRuleQueryParameter")]
        public RMDiscoveryFSROTRuleQueryParameter ROTRuleQueryParameter { get; set; }

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
