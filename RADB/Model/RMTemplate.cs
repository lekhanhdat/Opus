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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AvePoint.RA.Contract.TemplateManagement;

namespace AvePoint.RA.DB.Model
{
    public class RMTemplate : BaseModel
    {
        [Key]
        [Column(TypeName = "int", Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { set; get; }

        [Column(TypeName = "uniqueidentifier")]
        public Guid UniqueId { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(2000)]
        public string Name { get; set; }

        /// <summary>
        /// 1:Box Template; 2:Folder Template
        /// 
        /// </summary>
        [Column(TypeName = "int")]
        public TemplateType Type { set; get; }


        [Column(TypeName = "nvarchar")]
        [MaxLength(800)]
        public string Prefix { set; get; }

        [Column(TypeName = "int")]
        public int? NumberOfDigits { set; get; }

        [Obsolete]
        [Column(TypeName = "int")]
        public int ParentId { set; get; }

        [Obsolete]
        [Column(TypeName = "uniqueidentifier")]
        public Guid ParentUniqueId { set; get; }


        #region Base Info

        [Column(TypeName = "float")]
        public double Size { get; set; }

        [Column(TypeName = "int")]
        public int Creater { set; get; }

        [Column(TypeName = "datetime2")]
        public DateTime CreatedOn { get; set; }

        [Column(TypeName = "int")]
        public int Modifier { set; get; }

        [Column(TypeName = "datetime2")]
        public DateTime LastModifiedOn { get; set; }

        #endregion

        [Column(TypeName = "nvarchar(max)")]
        public string ColumnSchema { set; get; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(2000)]
        public string Description { get; set; }
    }
}
