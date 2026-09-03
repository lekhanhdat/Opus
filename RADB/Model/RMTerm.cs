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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvePoint.RA.DB.Model
{
    //[DataContract(Namespace = ContractConstants.Namespace)]
    public class RMTerm : BaseModel
    {
        [Key]
        [Column(TypeName = "int", Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { set; get; }
        //[Column(TypeName = "bigint")]
        //[Required]
        //public long CreatedTime { get; set; }
        //[Column(TypeName = "bigint")]
        //[Required]
        //public long LastModifiedTime { get; set; }
        [Column(TypeName = "int")]
        [Required]
        public int TermSetId { get; set; }
        [Column(TypeName = "uniqueidentifier")]
        [Required]
        [Index]
        public Guid UniqueId { get; set; }
        [Column(TypeName = "nvarchar")]
        [Required]
        public string Name { get; set; }
        [Column(TypeName = "nvarchar(max)")]
        [MaxLength(5000)]
        public string Description { get; set; }
        [Column(TypeName = "bit")]
        [Required]
        [Index(IsClustered = false)]
        public bool IsDeprecated { get; set; }
        [Column(TypeName = "bit")]
        [Required]
        [Index(IsClustered = false)]
        public bool IsRemoved { get; set; }
        [Column(TypeName = "bit")]
        [Required]
        [Index(IsClustered = false)]
        public bool BreakInheritFromParent { get; set; }
        [Column(TypeName = "nvarchar")]
        [MaxLength(512)]
        public string TimeZoneId { get; set; }
        [Column(TypeName = "nvarchar")]
        public string RuleInfo { get; set; }

        [Column(TypeName = "bigint")]
        public long TermExpirationFrom { get; set; }
        [Column(TypeName = "bigint")]
        public long TermExpirationTo { get; set; }
        [Column(TypeName = "bit")]
        [Required]
        public bool IsRootTerm { get; set; }

        [Column(TypeName = "bit")]
        [Required]
        public bool IsDayLight { get; set; }

        [Column(TypeName = "float")]
        public double AvailableSpace { get; set; }

        [Column(TypeName = "bit")]
        [Required]
        public bool IsDefaultTerm { get; set; }
        /// <summary>
        /// 0x0: disable, 0x1: sp enable, 0x2: exo enable
        /// </summary>
        [Column(TypeName = "int")]
        public int EnforceRetention { get; set; }
        /// <summary>
        /// label for EXO
        /// </summary>
        [Column(TypeName = "nvarchar")]
        public string EXORetentionLabel { get; set; }

        /// <summary>
        /// label for SP
        /// </summary>
        [Column(TypeName = "nvarchar")]
        public string SPRetentionLabel { get; set; }
        /// <summary>
        /// label for OneDrive
        /// </summary>
        [Column(TypeName = "nvarchar")]
        public string OneDriveRetentionLabel { get; set; }
        /// <summary>
        /// label for Teams
        /// </summary>
        [Column(TypeName = "nvarchar")]
        public string TeamsRetentionLabel { get; set; }

        [Column(TypeName = "bit")]
        [Required]
        public bool IsPermanent { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        [MaxLength(5000)]
        public string AdvanceSettings { get; set; }
        #region no use column
        //public Guid RuleId { get; set; }
        //[Column(TypeName = "nvarchar")]
        //[MaxLength(1024)]
        //public string RuleName{ get; set; }
        //[Column(TypeName = "bit")]
        //[Required]
        //public bool IsBreakInheritFromParent { get; set; }

        //[Column(TypeName = "bit")]
        //[Required]
        //public bool IsDeleted { get; set; }
        //[Column(TypeName = "bit")]
        //[Required]
        //public bool IsReused { get; set; }
        //[Column(TypeName = "uniqueidentifier")]
        //[Column(TypeName = "bigint")]
        //[Required]
        //public long CreatedTime { get; set; }
        #endregion


        /// <summary>
        /// not mapped property use for browse tree
        /// </summary>
        [NotMapped]
        public int subTermCount;
        [NotMapped]
        public List<RMTerm> subTerms;
        [NotMapped]
        public string Type { get { return "Term"; } }
        [NotMapped]
        public bool HaveParentSetting;
        //[NotMapped]
        //public List<RuleDisplayInfo> RuleInfos {get;set;}
        /// <summary>
        /// sharepoint setting  global and custom setting page keep review termTree page index
        /// </summary>
        [NotMapped]
        public int pageIndex;
        [NotMapped]
        public string TermExpirationFromStr;
        [NotMapped]
        public string TermExpirationToStr;
        [NotMapped]
        public bool IsExpired;
        [NotMapped]
        public bool IsLastLayTermBySearch;
        [NotMapped]
        public bool IsSPRemoved;
        [NotMapped]
        public bool IsSPDeprecated;
        [NotMapped]
        public int BoxsCount;
        [NotMapped]
        public string FullPath;
        [NotMapped]
        public List<string> FullPathList;
        [NotMapped]
        public bool IsChecked;
        [NotMapped]
        public bool SetAutoApply;
        [NotMapped]
        public Contract.JPMC.TermAdvanceSettings AdvanceSettingsObject;
    }
}
