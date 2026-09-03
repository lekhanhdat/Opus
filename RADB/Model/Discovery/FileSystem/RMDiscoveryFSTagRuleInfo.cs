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
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvePoint.RA.DB.Model.Discovery.FileSystem
{
    [Table("RMFSTagRuleInfo")]
    public class RMDiscoveryFSTagRuleInfo : RMDiscoveryDBTable
    {
        [Key]
        [Column(TypeName = "int")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column(TypeName = "uniqueidentifier")]
        public Guid TagId { get; set; }

        [Column(TypeName = "nvarchar")]
        public string Name { get; set; }

        [Column(TypeName = "nvarchar")]
        public string Definition { get; set; }

        [Column(TypeName = "bigint")]
        public long UpdateTime { get; set; }

        [Column(TypeName = "bit")]
        public bool NeedCalculation { get; set; }

        [Column(TypeName = "int")]
        public int MaxLength { get; set; }

        public string ToTagColumn()
        {
            return "tags_" + TagId.ToString("N");
        }

        public RMDiscoveryFSTagRuleInfoDto ConvertToDto()
        {
            return new RMDiscoveryFSTagRuleInfoDto
            {
                Id = this.TagId,
                Name = this.Name,
                Definition = this.Definition,
                MaxLength = this.MaxLength,
                NeedCalculation = this.NeedCalculation,
                UpdateTime = this.UpdateTime
            };
        }
    }

    public class RMDiscoveryFSTagRuleInfoDto
    {
        [JsonProperty]
        public Guid Id { get; set; }

        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public string Definition { get; set; }

        [JsonProperty]
        public long UpdateTime { get; set; }

        [JsonProperty]
        public bool NeedCalculation { get; set; }

        [JsonProperty]
        public int MaxLength { get; set; }
    }
}
