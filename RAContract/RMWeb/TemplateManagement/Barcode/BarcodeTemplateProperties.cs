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
using AvePoint.RA.Contract.TemplateManagement;

namespace AvePoint.RA.Contract.RMWeb.TemplateManagement.Barcode;

public class BarcodeTemplateProperties
{
    public string LogoImageBase64 { get; set; } // Base64 encoded string of the logo image
    public string LogoImageName { get; set; } // Name of the logo image file
    public string LogoImageType { get; set; } // Type of the logo image (e.g. PNG, JPG)
    public string LogoImagePrefix { get; set; } // Prefix for the logo image, if applicable
    public BarcodeTemplatePosition LogoImagePosition { get; set; } // 0: Above, 1: Under
}
