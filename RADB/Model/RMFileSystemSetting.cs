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
using AvePoint.RA.Contract.FileSystemRegister;
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
    public class RMFileSystemSetting : BaseModel
    {
        [Key]
        [Column(TypeName = "int", Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { set; get; }

        [Column(TypeName = "uniqueidentifier")]
        [Index]
        public Guid ScopeId { set; get; }

        [Column(TypeName = "uniqueidentifier")]
        [Index]
        public Guid ConnectionGroupId { set; get; }

        [Column(TypeName = "nvarchar")]
        public string Name { get; set; }

        [Column(TypeName = "nvarchar")]
        public string FullPath { get; set; }

        [Column(TypeName = "uniqueidentifier")]
        public Guid TermSetId { set; get; }

        [Column(TypeName = "uniqueidentifier")]
        public Guid TermId { set; get; }

        [Column(TypeName = "uniqueidentifier")]
        public Guid DefaultTermId { set; get; }

        [Column(TypeName = "nvarchar")]
        public string TermSetName { get; set; }

        [Column(TypeName = "nvarchar")]
        public string TermName { get; set; }

        [Column(TypeName = "nvarchar")]
        public string DefaultTermName { get; set; }

        [Column(TypeName = "nvarchar")]
        public string DescriptionOfContainer { get; set; }

        [Column(TypeName = "nvarchar")]
        public string TermNameOfContainer { get; set; }

        [Column(TypeName = "uniqueidentifier")]
        public Guid TermIdOfContainer { set; get; }

        [Column(TypeName = "bit")]
        public bool IsEnableContainerLevelClassification { get; set; }

        #region use this for quick config custom setting.
        [Column(TypeName = "bit")]
        public bool HaveConfigSetting { get; set; }//to do lock this setting for get job node

        [Column(TypeName = "bigint")]
        public long SettingTime { get; set; }//update the datetime

        [Column(TypeName = "nvarchar(max)")]
        public string NodeInfo { get; set; }
        #endregion

        [Column(TypeName = "bit")]
        public bool NeedCheckDefaultValue { get; set; }

        //[Column(TypeName = "nvarchar")]
        //[MaxLength(255)]
        //public string CollectionJobId1 { get; set; }

        //[Column(TypeName = "nvarchar")]
        //[MaxLength(255)]
        //public string DisposalJobId1 { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string IdPath { get; set; }

        [Column(TypeName = "bit")]
        public bool EMailToRecordOwner { get; set; }

        [Column(TypeName = "bit"), DefaultValue(0)]
        public bool EnableRelatedRecords { set; get; }

        [Column(TypeName = "int"), DefaultValue(0)]
        public int ApplyExistType { get; set; }

        [Column(TypeName = "bit")]
        public bool IsNewEdited { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(255)]
        public string FSSettingJobId { get; set; }

        [Column(TypeName = "bit")]
        public bool IsActive { set; get; }

        [Column(TypeName = "nvarchar(max)")]
        public string AutoClassificationRules { get; set; }

        [Column(TypeName = "int"), DefaultValue(0)]
        public int DeployTermMethod { get; set; }

        [Column(TypeName = "int"), DefaultValue(1)]
        public int AutoJobOption { get; set; }

        [Column(TypeName = "bit"), DefaultValue(0)]
        public bool RunAutoFullJob { get; set; }

        [Column(TypeName = "int")]
        public ApprovalType ApprovalType { get; set; }

        [Column(TypeName = "varchar")]
        [MaxLength(64)]
        public string WorkflowReferenceId { get; set; }
        [Column(TypeName = "int")]
        public int EnableRecordManagement { get; set; }
        [Column(TypeName = "bit"), DefaultValue(0)]
        public bool IsAllowUserDownloadRCCReport { get; set; }
        [Column(TypeName = "nvarchar")]
        public string ClassCode { get; set; }
        [Column(TypeName = "nvarchar")]
        public string CountryCode { get; set; }
        [Column(TypeName = "int")]
        public RetentionScheduleType RetentionScheduleType { get; set; }
        [Column(TypeName = "bigint")]
        public long StartDate { get; set; }
        [Column(TypeName = "bit"), DefaultValue(0)]
        public bool ApplyExistDocument { get; set; }
        [Column(TypeName = "int"), DefaultValue(0)]
        public int EffectScope { get; set; } = 0;
    }
}
