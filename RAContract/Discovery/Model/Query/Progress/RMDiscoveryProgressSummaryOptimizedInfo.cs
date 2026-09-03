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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Discovery.Model.Query.Progress
{
    public class RMDiscoveryProgressSummaryOptimizedInfo
    {
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

        [JsonProperty("archivedCount")]
        public long ArchivedCount { get; set; }

        [JsonProperty("spArchived")]
        public long SPArchived { get; set; }

        [JsonProperty("spArchivedCount")]
        public long SPArchivedCount { get; set; }

        [JsonProperty("oneArchived")]
        public long OneArchived { get; set; }
        
        [JsonProperty("oneArchivedCount")]
        public long OneArchivedCount { get; set; }

        [JsonProperty("deleted")]
        public long Deleted { get; set; }

        [JsonProperty("deletedCount")]
        public long DeletedCount { get; set; }

        [JsonProperty("spDeleted")]
        public long SPDeleted { get; set; }

        [JsonProperty("spDeletedCount")]
        public long SPDeletedCount { get; set; }

        [JsonProperty("oneDeleted")]
        public long OneDeleted { get; set; }

        [JsonProperty("oneDeletedCount")]
        public long OneDeletedCount { get; set; }
    }
}
