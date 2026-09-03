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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AvePoint.RA.Contract.TemplateManagement;

namespace AvePoint.RA.DB.Model;

public class RMCustomBarcodeTemplateProperty : BaseModel
{
    [Key]
    [Column(TypeName = "int", Order = 1)]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column(TypeName = "int")]
    public int TemplateId { get; set; } // Foreign key to RMCustomBarcodeTemplate

    [Column(TypeName = "nvarchar")]
    public string Name { get; set; } // Name of the property

    [Column(TypeName = "int")]
    public int FontSize { get; set; } // Size of the property, if applicable    

    [Column(TypeName = "int")]
    public BarcodeTemplatePosition Position { get; set; } // Position of the property on the template

    [Column(TypeName = "bigint")]
    public long CreatedTime { get; set; }

    [Column(TypeName = "bigint")]
    public long ModifiedTime { get; set; }

    [Column(TypeName = "int")]
    public int SortOrder { get; set; } // Order of the property in the template
}
