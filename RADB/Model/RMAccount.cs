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
using AvePoint.RA.Contract.RMWeb;
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
    public class RMAccount : BaseModel
    {
        [Key]
        [Column(TypeName = "int", Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { set; get; }

        [Index]
        [Column(TypeName = "varchar")]
        [MaxLength(64)]
        public string UserId { set; get; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(200)]
        public string UserPrincipalName { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(200)]
        public string DisplayName { get; set; }

        [Required]
        public RMActiveDirectoryObjectType ObjectType { get; set; }

        /// <summary>
        /// 1:在AOS中被删除,0:
        /// </summary>
        [Column(TypeName ="int")]
        [DefaultValue(0)]
        public int IsRemoved { get; set; }
        [Column(TypeName = "bigint")]
        public long CreateTime { get; set; }
        [Column(TypeName ="bigint")]
        public long LastUpdateTime { get; set; }

        [Index]
        [Column(TypeName = "varchar")]
        [MaxLength(64)]
        public string AADId { set; get; }


        [Column(TypeName = "nvarchar")]
        [MaxLength(200)]
        public string FirstName { set; get; }


        [Column(TypeName = "nvarchar")]
        [MaxLength(200)]
        public string LastName { set; get; }

    }
}
