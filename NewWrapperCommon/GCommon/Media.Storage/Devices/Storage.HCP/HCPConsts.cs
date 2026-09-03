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


namespace AvePoint.Media.Storage.HCP
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Globalization; 
    #endregion

    class HCPConsts
    {
        //HTTP
        public static readonly string KEY_COOKIE = "Cookie";
        public static readonly string KEY_COOKIE_VAL_PREFIX = "HCP-NS-Auth".ToLower(CultureInfo.InvariantCulture);
        public static readonly string KEY_X_HCP_TYPE = "X-HCP-Type";
        public static readonly string KEY_VAL_Directory = "directory";
        public static readonly string KEY_URL_Type = "type";
        public static readonly string KEY_Object_Size = "X-HCP-Size";

        //XML
        public const string XML_Selected_Namespace = "https://www.w3.org/2001/XMLSchema-instance";
    }
}
