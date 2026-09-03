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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace AvePoint.RA.DB.Model
{
    public class RMBoxConnection : BaseModel
    {
        [Key]
        [Column(TypeName = "uniqueidentifier")]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(255)]
        [Column(TypeName = "nvarchar")]
        public string Name { get; set; }

        [Column(TypeName = "nvarchar")]
        public string Description { get; set; }

        [Required]
        [Column(TypeName = "int")]
        public int AuthenticationType { get; set; }

        [Column(TypeName = "nvarchar")]
        public string EnterpriseId { get; set; }

        [Column(TypeName = "nvarchar")]
        public string ClientId { get; set; }

        [Column(TypeName = "nvarchar")]
        public string ClientSecret { get; set; }

        [MaxLength(255)]
        [Column(TypeName = "nvarchar")]
        public string EmailAddress { get; set; }

        [Column(TypeName = "nvarchar")]
        public string JsonFileName { get; set; }

        [Column(TypeName = "varbinary")]
        public byte[] JsonFileContent { get; set; }

        [Required]
        [Column(TypeName = "bigint")]
        public long Created { get; set; }

        [Required]
        [Column(TypeName = "bigint")]
        public long Modified { get; set; }

        [MaxLength(64)]
        [Column(TypeName = "nvarchar")]
        public string CreatedBy { get; set; }

        [MaxLength(64)]
        [Column(TypeName = "nvarchar")]
        public string ModifiedBy { get; set; }

        [Column(TypeName = "uniqueidentifier")]
        public Guid ConnectionGroupId { get; set; }

        [MaxLength(255)]
        [Column(TypeName = "nvarchar")]
        public string RedirectUrl { get; set; }

        [MaxLength(255)]
        [Column(TypeName = "nvarchar")]
        public string AccessToken { get; set; }

        [MaxLength(255)]
        [Column(TypeName = "nvarchar")]
        public string RefreshToken { get; set; }

        [NotMapped]
        public bool IsRelatedConnectionGroup => ConnectionGroupId != Guid.Empty;

        [NotMapped]
        public RMBoxConnectionGroup ConnectionGroup { get; set; }
    }
   

}