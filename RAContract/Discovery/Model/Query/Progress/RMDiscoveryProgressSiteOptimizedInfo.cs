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
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Discovery.Model.Query.Progress
{
    public class RMDiscoveryProgressSiteOptimizedInfo
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("contentSource")]
        public SourceFlag ContentSource { get; set; }

        [JsonProperty("nextOptimizationTime")]
        public long NextOptimizationTime { get; set; }

        [JsonProperty("nextOptimizationTimeString")]
        public string NextOptimizationTimeString { get; set; }

        [JsonProperty("fileTotalSize")]
        public long FileTotalSize { get; set; }

        [JsonProperty("fileSumCount")]
        public long FileSumCount { get; set; }

        [JsonProperty("nextOptimizableFileTotalSize")]
        public long NextOptimizableFileTotalSize { get; set; }

        [JsonProperty("nextOptimizableVersionTotalSize")]
        public long NextOptimizableVersionTotalSize { get; set; }

        [JsonProperty("archived")]
        public long Archived { get; set; }

        [JsonProperty("deleted")]
        public long Deleted { get; set; }

        [JsonProperty("saving")]
        public long Saving { get; set; }
    }
}
