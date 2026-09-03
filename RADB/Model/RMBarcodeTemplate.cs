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
    public class RMBarcodeTemplate : BaseModel
    {
        [Key]
        [Column(TypeName = "int", Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { set; get; }


        /// <summary>
        /// 1:Box Template; 2:Folder Template
        /// 
        /// </summary>
        [Column(TypeName = "int")]
        public int Type { set; get; }

        //[Column(TypeName = "blob")]
        [Column(TypeName = "varbinary(max)")]
        public byte[] ImageColumnA { set; get; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(255)]
        public string ColumnB { set; get; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(255)]
        public string ColumnC { set; get; }


        //[Column(TypeName = "nvarchar")]
        //[MaxLength(255)]
        //public string ColumnD { set; get; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(255)]
        public string ColumnE { set; get; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(255)]
        public string ColumnF { set; get; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(255)]
        public string Prefix { set; get; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(255)]
        public string ImageName { set; get; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(255)]
        public string ImageType { set; get; }

        [Column(TypeName = "bigint")]
        public long ModifyTime { get; set; }

        public List<string> ColumnDList { get; set; }
    }
}
