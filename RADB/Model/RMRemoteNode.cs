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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AvePoint.RA.DB.Model.Synchronize;

namespace AvePoint.RA.DB.Model
{
    public class RMRemoteNode : BaseModel, IRMSynchronizeDbTable
    {
        [Key]
        [Column(TypeName = "varchar")]
        [MaxLength(64)]
        public string Id { set; get; }

        [Column(TypeName = "varchar")]
        [MaxLength(64)]
        public string ObjectId { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(400)]
        public string DomainName { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(1024)]
        public string UserName { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(1024)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Index]
        [Column(TypeName = "nvarchar")]
        [MaxLength(400)]
        public string Url { get; set; }

        [Index]
        [Column(TypeName = "varchar")]
        [MaxLength(64)]
        public string ParentId { get; set; }

        [Column(TypeName = "int")]
        public int State { get; set; }

        [Column(TypeName = "varchar")]
        [MaxLength(64)]
        public string AgentGroupId { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(512)]
        public string AgentGroupName { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(1024)]
        public string Description { get; set; }

        [Column(TypeName = "bigint")]
        public long ModifiedDate { get; set; }

        [Column(TypeName = "varchar")]
        [MaxLength(64)]
        public string BposMode { get; set; }

        [Column(TypeName = "bigint")]
        public long CreateTime { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(128)]
        public string TemplateName { get; set; }

        [Column(TypeName = "varchar")]
        [MaxLength(64)]
        public string SPVersion { get; set; }

        [Index]
        [Required]
        [Column(TypeName = "int")]
        public int NodeLevel { get; set; }

        [Index]
        [Column(TypeName = "nvarchar")]
        [MaxLength(256)]
        public string Name { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(256)]
        public string DisplayName { get; set; }

        [Column(TypeName = "varchar")]
        [MaxLength(2368)]
        public string AvailableAgentIds { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(400)]
        public string TemplateTitle { get; set; }

        [Column(TypeName = "bit")]
        public bool IsPublicWebSite { get; set; }

        [Column(TypeName = "int")]
        public int SiteCollectionType { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(400)]
        public string AdminUrl { get; set; }

        [Column(TypeName = "varchar")]
        [MaxLength(36)]
        public string ServiceAccountId { get; set; }

        [Index]
        [Column(TypeName = "varchar")]
        [MaxLength(36)]
        public string TenantId { get; set; }

        [Column(TypeName = "int")]
        public int AuthType { get; set; }

        [Column(TypeName = "int")]
        public int AppType { get; set; }

        [Column(TypeName = "int")]
        public int ScanSource { get; set; }

        [Index]
        [Column(TypeName = "nvarchar")]
        [MaxLength(64)]
        public string TeamId { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(64)]
        public string SecondParentId { get; set; }

        [Column(TypeName = "bit")]
        public bool FromDAO { get; set; }

        [Column(TypeName = "varchar")]
        [MaxLength(64)]
        public string AosId { get; set; }

        //provide to discovery Job
        [NotMapped]
        public RMDiscoveryJobFailedCause FailedCause { get; set; }
    }
}
