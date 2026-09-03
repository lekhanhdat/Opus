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
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Discovery.Model.Query.Office365.Parameter;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Discovery.Model.Profile
{
    [DataContract]
    public class RMDiscoveryProfileDataInfo
    {
        [DataMember]
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [DataMember]
        [JsonProperty("o365TenantId")]
        public Guid O365TenantId { get; set; }

        [DataMember]
        [JsonProperty("name")]
        public string Name { get; set; }

        [DataMember]
        [JsonProperty("sizeRange")]
        public int SizeRange { get; set; }

        [DataMember]
        [JsonProperty("sizeRangeQueryMode")]
        public RMDiscoverySizeRangeQueryMode SizeRangeQueryMode { get; set; }

        [DataMember]
        [JsonProperty("greaterThanEqualWithoutInDate")]
        public int GreaterThanEqualWithoutInDate { get; set; }

        [DataMember]
        [JsonProperty("lessThanEqualWithoutInDate")]
        public int LessThanEqualWithoutInDate { get; set;}

        [DataMember]
        [JsonProperty("fileExtensionIds")]
        public List<int> FileExtensionIds { get; set; }

        [DataMember]
        [JsonProperty("ruleIds")]
        public List<int> RuleIds { get; set; }

        [DataMember]
        [JsonProperty("sortBy")]
        public string SortBy { get; set; }

        [DataMember]
        [JsonProperty("status")]
        public RMDiscoveryJobStatus Status { get; set; }

        [DataMember]
        [JsonProperty("isBuildIn")]
        public bool IsBuildIn { get; set; }

        [DataMember]
        [JsonProperty("isDefault")]
        public bool IsDefault { get; set; }

        [DataMember]
        [JsonProperty("customColumns")]
        public List<RMDiscoveryTableColumnInfo> CustomColumns { get; set; } = [];

        [DataMember]
        [JsonProperty("avaliableRuleCategories")]
        public List<RMDiscoveryRuleCategory> AvaliableRuleCategories { get; set; } = [];

        [DataMember]
        [JsonProperty("modifiedTimeRangeLabel")]
        public string ModifiedTimeRangeLabel { get; set; }

        [DataMember]
        [JsonProperty("sizeRangeLabel")]
        public string SizeRangeLabel { get; set; }

        [DataMember]
        [JsonProperty("fileTypeLabel")]
        public string FileTypeLabel { get; set; }

        [DataMember]
        [JsonProperty("ruleInfoesLabel")]
        public string RuleInfoesLabel { get; set; }
    }
}
