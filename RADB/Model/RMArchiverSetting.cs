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
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Model
{
    public class RMArchiverSetting:BaseModel
    {
        [Key]
        [Column(TypeName = "uniqueidentifier", Order = 1)]
        public Guid Id { set; get; }
        [Column(TypeName = "uniqueidentifier")]
        [Index]
        public Guid SPObjectId { set; get; }

        [Column(TypeName = "uniqueidentifier")]
        [Index]
        public Guid SiteGroupId { set; get; }

        [Column(TypeName = "uniqueidentifier")]
        [Index]
        public Guid TeamsId { set; get; }

        [Column(TypeName = "uniqueidentifier")]
        [Index]
        public Guid SiteId { set; get; }
        [Column(TypeName = "int")]
        public int ContentSourceType { get; set; }
        [Column(TypeName = "nvarchar")]
        public string RuleContainerId { get; set; }
        [Column(TypeName = "bigint")]
        public long SettingTime { get; set; }
        [Column(TypeName = "bit")]
        public bool SupportLockedSite { get; set; }
        [Column(TypeName = "bit")]
        public bool SupportArchivedTeams { get; set; }
        [Column(TypeName = "bit")]
        public bool isEnableSuperUserDecrypt { get; set; }
        [Column(TypeName = "bit")]
        public bool isEnableRemoveRetentionLabel { get; set; }
        [Column(TypeName = "bit")]
        public bool isIncludeManagedMetadataService { get; set; }
        [Column(TypeName = "bit")]
        public bool isIncludeWorkflowDefinition { get; set; }
        [Column(TypeName = "int")]
        public int EnableArchiverManagement { get; set; }
        [Column(TypeName = "nvarchar")]
        public string ScheduleId { get; set; }
        [Column(TypeName = "nvarchar")]
        public string Url { get; set; }

        [Column(TypeName = "bit")]
        public bool? DAOMigrated { get; set; }
        [Column(TypeName = "nvarchar(max)")]
        public string CleanRestoredOption { get; set; }
        [Column(TypeName = "bigint")]
        [Required]
        public long CreateTime { get; set; }
    }
}
