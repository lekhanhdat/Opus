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
using AvePoint.RA.Contract.TemplateManagement;

namespace AvePoint.RA.Contract.RMWeb.TemplateManagement.Barcode;

public class BarcodeTemplateSuiteDto
{
    public int Id { get; set; } // Unique identifier for the template
    public Guid SuiteId { get; set; } // Unique identifier for the template
    public bool IsDefault { get; set; } // true: default template, false: custom template
    public string Name { get; set; }
    public string Description { get; set; }
    public BarcodeTemplateLabelType LabelType { get; set; } // 0: Rectangle289x199mm
}

public class BarcodeDefaultTemplateDto : BarcodeTemplateSuiteDto
{
    public List<BarcodeTemplateDto> Templates { get; set; } // List of columns for the default template
}

public class BarcodeCustomTemplateDto : BarcodeTemplateSuiteDto
{
   public List<BarcodeCustomTemplateInfo> Templates { get; set; } // List of custom templates in the suite
}

public class BarcodeCustomTemplateInfo
{
    public int TemplateId { get; set; } // Unique identifier for the custom template
    public BarcodeTemplateType Type { get; set; } // 0: Box, 1: Folder
    public BarcodeTemplateLogoProperties LogoProperties { get; set; } // Properties for the logo image
    public List<BarcodeTemplatePropertyDto> Properties { get; set; } // List of properties for the custom template
}

public class BarcodeTemplateLogoProperties
{
    public string LogoImgBase64Str { get; set; } // Raw bytes of the logo image (was Base64 string)
    public string LogoImgName { get; set; } // Name of the logo image file
    public string LogoImgType { get; set; } // Type of the logo image (e.g., "image/png", "image/jpeg")
    public BarcodeTemplatePosition Position { get; set; } // Position of the logo on the template
}

public class BarcodeTemplatePropertyDto
{
    public int Id { get; set; } // Unique identifier for the property
    public int TemplateId { get; set; } // Foreign key to the template
    public string Name { get; set; } // Name of the property
    public string DisplayName { get; set; } // Display name of the property
    public int FontSize { get; set; } // Size of the property, if applicable
    public BarcodeTemplatePosition Position { get; set; } // Position of the property on the template
}