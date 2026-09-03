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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvePoint.RA.DB.Model
{
    /// <summary>
    /// TermSet 和 term之间的关系，termId为主键
    /// </summary>
    [RACodeReview("Allen Yin", comment: "IsRemoved 和term表冗余，后期容易导致信息不一致的问题")]
    public class RMTermSetMembership : BaseModel
    {
        [Column(TypeName = "int")]
        [Required]
        [Key]
        public int TermId { get; set; }

        [Column(TypeName = "int")]
        [Required]
        [Index("parentTerm", IsClustered = false, Order = 2)]
        public int TermSetId { get; set; }

        /// <summary>
        /// 当此值为0时，代表为第一层的term
        /// </summary>
        [Column(TypeName = "int")]
        [Required]
        [Index("parentTerm", IsClustered = false, Order = 1)]
        public int ParentTermId { get; set; }

        [Column(TypeName = "nvarchar")]
        public string TermName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Column(TypeName = "text")]
        [Required]
        public string Path { get; set; }

        /// <summary>
        /// 后期扩展使用，区分多个TermSet下的Term
        /// </summary>
        [Column(TypeName = "bit")]
        [Required]
        public bool IsSource { get; set; }

        /// <summary>
        /// 此term是否被删除
        /// </summary>
        [Column(TypeName = "bit")]
        [Required]
        [Index("removeTerm", IsClustered = false)]
        public bool IsRemoved { get; set; }
    }
}
