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

namespace AvePoint.RA.DB.Model.Discovery.AOSP
{
    [Table("RMAOSPMainJobs")]
    public class RMDiscoveryAOSPMainJob : RMDiscoveryDBTable
    {
        [Key]
        [Column(TypeName = "uniqueidentifier")]
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [Column(TypeName = "bigint")]
        [JsonProperty("startTime")]
        public long StartTime { get; set; }

        [Column(TypeName = "bigint")]
        [JsonProperty("endTime")]
        public long EndTime { get; set; }

        [Column(TypeName = "bit")]
        [JsonProperty("needToReRegisterTags")]
        public bool NeedToReRegisterTags { get; set; }

        [Column(TypeName = "int")]
        [JsonProperty("containersCount")]
        public int ContainersCount { get; set; }

        [Column(TypeName = "int")]
        [JsonProperty("sitesCount")]
        public int SitesCount { get; set; }

        [Column(TypeName = "bigint")]
        [JsonProperty("lastModifiedTime")]
        public long LastModifiedTime { get; set; }

        [Column(TypeName = "uniqueidentifier")]
        [JsonProperty("parentId")]
        public Guid ParentId { get; set; }

        [Column(TypeName = "uniqueidentifier")]
        [JsonProperty("mainJobId")]
        public Guid MainJobId { get; set; }

        [Column(TypeName = "int")]
        [JsonProperty("status")]
        public RMDiscoveryJobStatus Status { get; set; }

        [Column(TypeName = "int")]
        [JsonProperty("profileJobInitStatus")]
        [DefaultValue((int)RMDiscoveryJobStatus.None)]
        public RMDiscoveryJobStatus ProfileJobInitStatus { get; set; }

        [Column(TypeName = "int")]
        [JsonProperty("type")]
        [DefaultValue((int)RMDiscoveryJobType.Newly)]
        public RMDiscoveryJobType Type { get; set; }

        [Column(TypeName = "nvarchar")]
        [JsonProperty("o365TenantId")]
        public string O365TenantId { get; set; }

        [Column(TypeName = "nvarchar")]
        [JsonProperty("appProfileId")]
        public string AppProfileId { get; set; }

        //[Column(TypeName = "int")]
        //[JsonProperty("version")]
        //[DefaultValue(0)]
        //public RMDiscoveryJobVersion Version { get; set; }

        // [Column(TypeName = "nvarchar")]
        // [JsonProperty("additionalInfo")]
        // public string AdditionalInfo { get; set; }

        [Column(TypeName = "nvarchar")]
        [JsonProperty("comment")]
        public string Comment { get; set; }
    }
}
