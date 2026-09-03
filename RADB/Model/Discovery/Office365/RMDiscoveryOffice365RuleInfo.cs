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
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.DB.Core.Discovery;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Model.Discovery.Office365
{
    [Table("RMRuleInfo")]
    public class RMDiscoveryOffice365RuleInfo : RMDiscoveryDBTable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(TypeName = "int")]
        public int Id { get; set; }

        [Column(TypeName = "uniqueidentifier")]
        public Guid UniqueId { get; set; }

        [Column(TypeName = "nvarchar")]
        public string Name { get; set; }

        [Column(TypeName = "nvarchar")]
        public string Description { get; set; }

        [Column(TypeName = "int")]
        public int Order { get; set; }

        [Column(TypeName = "bit")]
        public bool IsEnable { get; set; }

        [Column(TypeName = "int")]
        public RMDiscoveryRuleDefinitionKind DefinitionKind { get; set; }

        [Column(TypeName = "int")]
        public RMDiscoveryRuleAnalyseMethod AnalyseMethod { get; set; }

        [Column(TypeName = "int")]
        public RMDiscoveryRuleCategory Category { get; set; }

        [Column(TypeName = "nvarchar")]
        public string CriteriaInfoesJson { get; set; }

        [Column(TypeName = "bigint")]
        public long CreateTime { get; set; }

        [Column(TypeName = "bigint")]
        public long ModifiedTime { get; set; }

        [Column(TypeName = "bit")]
        public bool IsRemoved { get; set; }

        public RMDiscoveryCustomColumn ToCustomColumn()
        {
            var columnName = "c" + UniqueId.ToString().Replace("-", "").ToLower();
            return new RMDiscoveryCustomColumn(columnName, System.Data.SqlDbType.BigInt);
        }

        public string ToTagColumn()
        {
            return "tags_" + UniqueId.ToString().ToLower().Replace("-", "");
        }
    }
}
