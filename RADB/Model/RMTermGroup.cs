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
using AvePoint.RA.Contract.CodeView;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvePoint.RA.DB.Model
{
    [RACodeReview("Allen Yin",comment:"Id 与其他表有外键关系")]
    public class RMTermGroup:BaseModel
    {
        [Key]
        [Column(TypeName = "int", Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { set; get; }

        [Column(TypeName = "uniqueidentifier")]
        [Required]
        public Guid UniqueId { get; set; }

        [Column(TypeName = "nvarchar")]
        [Required]
        public string Name { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        [MaxLength(5000)]
        public string Description { get; set; }

        [Column(TypeName = "bit")]
        public bool UsingMMSSpecified { get; set; }

        [Column(TypeName = "int")]
        [DefaultValue((int)TermSyncOption.None)]
        public TermSyncOption GoogleTermSyncOption { get; set; } = TermSyncOption.None;

        [Column(TypeName = "int")]
        [DefaultValue((int)TermSyncOption.All)]
        public TermSyncOption M365TermSyncOption { get; set; } = TermSyncOption.All;

        [Column(TypeName = "bit")]
        public bool IsRemoved { get; set; }

        [NotMapped]
        public string Type { get { return "TermGroup"; } }

        [NotMapped]
        public int subTermCount;

        [NotMapped]
        public List<RMTermSet> subTerms; //for search
    }

    public enum TermSyncOption
    {
        None = 0,
        Specified = 1,
        All = 2
    }
}
