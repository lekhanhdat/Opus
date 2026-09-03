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
using AvePoint.RA.Contract.Object;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvePoint.RA.DB.Model
{
    public class RMLocation : BaseModel
    {
        [Key]
        [Column(TypeName = "int", Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { set; get; }

        [Column(TypeName = "uniqueidentifier")]
        [Required]
        [Index]
        public Guid UniqueId { get; set; }

        [Column(TypeName = "int")]
        [Required]
        [Index]
        public int ParentId { get; set; }

        [Column(TypeName = "nvarchar")]
        [Required]
        public string Name { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(1024)]
        public string Description { get; set; }

        /// <summary>
        /// PhysicalRootLocation = 9000,
        /// PhysicalNormalLocation = 9100,
        /// PhysicalBottomLocation = 9200,
        /// PhyBox = 9300,
        /// PhyFile = 9400,
        /// PhyRecord = 9500
        /// </summary>
        [Column(TypeName = "int")]
        public int NodeType { get; set; }

        [Column(TypeName = "bit")]
        public bool IsRemoved { get; set; }

        [Column(TypeName = "float")]
        public double AvailableSpace { get; set; }

        /// <summary>
        /// Parent Path
        /// </summary>
        [Column(TypeName = "text")]
        public string DirPath { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string MetaInfo { get; set; }

        [Column(TypeName = "nvarchar")]
        public string CreatedUserId { get; set; }

        [Column(TypeName = "bigint")]
        public long CreatedTime { get; set; }

        [Column(TypeName = "nvarchar")]
        public string ModifiedUserId { get; set; }

        [Column(TypeName = "bigint")]
        public long ModifiedTime { get; set; }

        #region NotMapped
        /// <summary>
        /// 直接关联的Sub Location个数
        /// </summary>
        [NotMapped]
        public int SubLocationCount { get; set; }

        /// <summary>
        /// 直接关联的Sub Location集合
        /// </summary>
        [NotMapped]
        public List<RMLocation> SubLocations { get; set; }

        [NotMapped]
        public string PathForDisplay { get; set; }

        [NotMapped]
        public List<Guid> RMLocationSuiteAssociationIds { get; set; }

        [NotMapped]
        public IconStatus IconStatus { set; get; }
        #endregion
    }
}
