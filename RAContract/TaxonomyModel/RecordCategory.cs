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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AvePoint.RA.Contract.TaxonomyModel
{
    public class RecordCategory
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("retention_policy")]
        public RetentionPolicy RetentionPolicy { get; set; }
    }

    public class RetentionPolicy
    {
        [JsonProperty("retention_time")]
        public RetentionTime RetentionTime { get; set; }

        [JsonProperty("action")]
        public string Action { get; set; }

        [JsonProperty("manual_review")]
        public string ManualReview { get; set; }


        [JsonProperty("reference")]
        public string Reference { get; set; }

        [JsonProperty("reference_link")]
        public string ReferenceLink { get; set; }
    }

    public class RetentionTime
    {
        [JsonProperty("retention_time_number")]
        public int RetentionTimeNumber { get; set; }

        [JsonProperty("unit")]
        public string Unit { get; set; }

        [JsonProperty("policy_description")]
        public string PolicyDescription { get; set; }
    }
}
