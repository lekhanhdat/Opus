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

namespace AvePoint.Wrapper.Restore.NintexForm
{
    public class KnownControlTypes
    {
        public static readonly Guid Label = new Guid("{C0A89C70-0781-4BD4-8623-F73675005E00}");
        public static readonly Guid Choice = new Guid("{C0A89C70-0781-4BD4-8623-F73675005E02}");
        public static readonly Guid Cascading = new Guid("{F683B562-3BCC-4C3E-83F7-B2D6A26E9661}");
        public static readonly Guid DateTime = new Guid("{C0A89C70-0781-4BD4-8623-F73675005E03}");
        public static readonly Guid Boolean = new Guid("{C0A89C70-0781-4BD4-8623-F73675005E04}");
        public static readonly Guid HyperlinkColumn = new Guid("{A0C89D70-0781-4BD4-8623-A73675005A05}");
        public static readonly Guid SingleLineTextBox = new Guid("{C0A89C70-0781-4BD4-8623-F73675005E05}");
        public static readonly Guid MultiLineTextBox = new Guid("{C0A89C70-0781-4BD4-8623-F73675005E06}");
        public static readonly Guid Html = new Guid("{C0A89C70-0781-4BD4-8623-F73675005E07}");
        public static readonly Guid Image = new Guid("{C0A89C70-0781-4BD4-8623-F73675005E08}");
        public static readonly Guid Button = new Guid("{C0A89C70-0781-4BD4-8623-F73675005E09}");
        public static readonly Guid HyperLink = new Guid("{C0A89C70-0781-4BD4-8623-F73675005E10}");
        public static readonly Guid Line = new Guid("{C0A89C70-0781-4BD4-8623-F73675005E11}");
        public static readonly Guid PeoplePicker = new Guid("{C0A89C70-0781-4BD4-8623-F73675005E12}");
        public static readonly Guid Frame = new Guid("{C0A89C70-0781-4BD4-8623-F73675005E13}");
        public static readonly Guid Panel = new Guid("{C0A89C70-0781-4BD4-8623-F73675005E14}");
        public static readonly Guid Repeater = new Guid("{C0A89C70-0781-4BD4-8623-F73675005E16}");
        public static readonly Guid ListItem = new Guid("{2C285C16-D4E6-49EB-8A6A-D9AA41E9E71B}");
        public static readonly Guid ListView = new Guid("{4420D111-8869-49BB-8685-C1B6CDEC4873}");
        public static readonly Guid SharePointLookup = new Guid("{C0A89C70-0781-4BD4-8623-F73675005E15}");
        public static readonly Guid ExternalDataColumnControl = new Guid("{C0A89C70-0781-4BD4-8623-F73675005E21}");
        public static readonly Guid UnsupportedControl = new Guid("{6305A41E-EFA3-11E0-ADEB-9EDD4824019B}");
        public static readonly Guid CalculationControl = new Guid("{C0A89C70-0781-4BD4-8623-F73675005E17}");
        public static readonly Guid GeoLocationControl = new Guid("{C0A89C70-0781-4BD4-8623-F73675005E19}");
        public static readonly Guid PrintPdfPageBreakGuideControl = new Guid("{6EFF501C-EEBF-43E1-B25C-638A2A6D8791}");
        public static readonly Guid SharePointAttachment = new Guid("{5F8B447A-4195-485B-9A04-477D7F24BE73}");
        public static readonly Guid ChangeContentType = new Guid("{FF9F65FE-F979-4312-A35B-50F0D3769069}");
        public static readonly Guid DataAccess = new Guid("{7733D5BF-11C6-4BDC-A430-79C3065A796C}");
        public static readonly Guid WebService = new Guid("{AEADA2B6-24AD-46E2-894F-562C2A01D38A}");
    }
}
