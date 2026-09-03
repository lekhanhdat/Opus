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
using AvePoint.RA.Contract.Explorer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.ReportCenter.Model
{
    [DataContract]
    public class DisposalReportModel
    {
        [DataMember]
        [JsonProperty(PropertyName = "source")]
        public SourceFlag Source { get; set; }
        [DataMember]
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }
        [DataMember]
        [JsonProperty(PropertyName = "profileName")]
        public string ProfileName { get; set; }
        [DataMember]
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }
        [DataMember]
        [JsonProperty(PropertyName = "applyRuleBeforeTime")]
        public string ApplyRuleBeforeTime { get; set; }
        [DataMember]
        [JsonProperty(PropertyName = "checkedTreeStructure")]
        public string CheckedTreeStructure { get; set; }
        [DataMember]
        [JsonProperty(PropertyName = "modified")]
        public long Modified { get; set; }
        [DataMember]
        [JsonProperty(PropertyName = "created")]
        public string CreateBy { get; set; }
    }
}
