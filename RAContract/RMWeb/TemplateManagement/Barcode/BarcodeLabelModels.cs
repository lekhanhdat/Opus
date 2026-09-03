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
using System.Collections.Generic;
using AvePoint.RA.Contract.TemplateManagement;

namespace AvePoint.RA.Contract.RMWeb.TemplateManagement.Barcode
{
    // Moved from ReportWordUtil: lightweight models used by Word export helpers
    public class LabelItem
    {
        public string Barcode { get; set; }
        public List<PropertyItem> Properties { get; set; } = new List<PropertyItem>();
        public LogoItem Logo { get; set; }
    }

    public class PropertyItem
    {
        public string Name { get; set; }
        public string Value { get; set; }
        // top | bottom | left | right | center (default)
        public BarcodeTemplatePosition Position { get; set; }
        // optional font size in half-points (e.g., 16 => ~8pt)
        public int? FontSize { get; set; }
    }

    public class LogoItem
    {
        public bool Enabled { get; set; }
        // topleft | topright | left | right | center | bottomleft | bottomright
        public BarcodeTemplatePosition Position { get; set; }
        public byte[] ImageBytes { get; set; }
        // Expected pixel size for the logo image
        public int Width { get; set; } = 100;
        public int Height { get; set; } = 50;
        public string FileName { get; set; } = "logo";
        // Optional MIME type like image/png or image/jpeg; used to pick ImagePartType
        public string Mime { get; set; } = "image/png";
    }
}
