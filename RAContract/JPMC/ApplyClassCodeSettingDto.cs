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
using AvePoint.RA.Contract.Object;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.JPMC
{
    [DataContract(IsReference = true)]
    [JsonObject]
    public class ApplyClassCodeSettingDto
    {
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string ClassCode { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string CountryCode { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int RetentionType { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public long StartDate { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool ApplyToExistingDoc { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public List<RMFSTreeNode> FSTreeNode { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public string TermId { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool NeedToUpdateConnectionGroup { set; get; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool IsConnectionGroup { set; get; }
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public bool IsMyhubClassify { set; get; } = false;

    }

    [DataContract(IsReference = true)]
    [JsonObject]
    public class OlderThanTimeDto
    {
        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public int Number { set; get; }

        [DataMember(EmitDefaultValue = false)]
        [JsonProperty]
        public PolicyValueUnit PolicyValueUnit { set; get; }

    }
}
