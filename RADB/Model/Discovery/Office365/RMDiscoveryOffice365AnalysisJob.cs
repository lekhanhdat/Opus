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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Model.Discovery.Office365
{
    [Table("RMAnalysisJobs")]
    public class RMDiscoveryOffice365AnalysisJob : RMDiscoveryDBTable
    {
        [Key]
        [Column(TypeName = "uniqueidentifier")]
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [Column(TypeName = "uniqueidentifier")]
        [JsonProperty("mainJobId")]
        public Guid MainJobId { get; set; }

        [Column(TypeName = "uniqueidentifier")]
        [JsonProperty("discoveryJobId")]
        public Guid DiscoveryJobId { get; set; }

        [Column(TypeName = "uniqueidentifier")]
        [JsonProperty("o365TenantId")]
        public Guid O365TenantId { get; set; }

        [Column(TypeName = "uniqueidentifier")]
        [JsonProperty("containerId")]
        public Guid ContainerId { get; set; }

        [Column(TypeName = "uniqueidentifier")]
        [JsonProperty("siteId")]
        public Guid SiteId { get; set; }

        [Column(TypeName = "nvarchar")]
        [JsonProperty("url")]
        public string Url { get; set; }

        [Column(TypeName = "int")]
        [JsonProperty("status")]
        public RMDiscoveryJobStatus Status { get; set; }

        [Column(TypeName = "int")]
        [JsonProperty("failedCause")]
        [DefaultValue(0)]
        public RMDiscoveryJobFailedCause FailedCause { get; set; }

        [Column(TypeName = "bigint")]
        [JsonProperty("startTime")]
        public long StartTime { get; set; }

        [Column(TypeName = "bigint")]
        [JsonProperty("endTime")]
        public long EndTime { get; set; }

        [Column(TypeName = "bigint")]
        [JsonProperty("lastModifiedTime")]
        public long LastModifiedTime { get; set; }
    }
}
