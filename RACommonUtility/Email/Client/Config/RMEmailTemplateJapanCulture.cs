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
using System.Collections.ObjectModel;

namespace AvePoint.RA.RACommonUtility.Email.Client.Config
{
    public static class RMEmailTemplateJapanCulture
    {

        private static readonly string JAPAN_FONT_FAMILY = "font-family: Meiryo UI";

        public const string JAPAN_CULTURE = "ja-JP";
        private static Dictionary<string, string> _JAPAN_FONT_FAMILY_MAPPING = new Dictionary<string, string>()
        {
            {"font-family: segoe ui", JAPAN_FONT_FAMILY},
            {"font-family:segoe ui", JAPAN_FONT_FAMILY},
            {"font-family: Segoe UI", JAPAN_FONT_FAMILY},
            {"font-family:Segoe UI", JAPAN_FONT_FAMILY},
            {"font-family: arial", JAPAN_FONT_FAMILY},
            {"font-family:arial", JAPAN_FONT_FAMILY},
            {"font-family: Arial", JAPAN_FONT_FAMILY},
            {"font-family:Arial", JAPAN_FONT_FAMILY},
        };
        public static ReadOnlyDictionary<string, string> JAPAN_FONT_FAMILY_MAPPING = new ReadOnlyDictionary<string, string>(_JAPAN_FONT_FAMILY_MAPPING);
       
    }
}
