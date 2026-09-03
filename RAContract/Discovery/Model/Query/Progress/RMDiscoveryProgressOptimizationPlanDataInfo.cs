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
using AvePoint.RA.Contract.Discovery.Model.Configuration;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Office365;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Discovery.Model.Query.Progress
{
    public class RMDiscoveryProgressOptimizationPlanDataInfo
    {
        [JsonProperty("uniqueId")]
        public Guid UniqueId { get; set; }

        [JsonProperty("optimizingTime")]
        public string OptimizingTime { get; set; }

        [JsonProperty("timeRange")]
        public string TimeRange { get; set; }

        [JsonProperty("sizeRange")]
        public string SizeRange { get; set; }

        [JsonProperty("fileType")]
        public string FileType { get; set; }

        [JsonProperty("ms365DataType")]
        public MS365DataType MS365DataType { get; set; }

        [JsonProperty("scope")]
        public string Scope { get; set; }

        [JsonProperty("sites")]
        public List<string> Sites { get; set; } = [];
    }
}
