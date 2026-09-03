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


namespace AvePoint.Media.Storage.S3Compatible.REST
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    #endregion
    class S3CompatibleConstants
    {
        //headers
        public static readonly String Authorization_HEADER = "Authorization";
        public static readonly String S3Compatible_REST_HEADER_PREFIX = "X-AMZ-".ToLower(CultureInfo.InvariantCulture);
        public static readonly String S3Compatible_REST_METADATA_HEADER_PREFIX = "X-AMZ-META-".ToLower(CultureInfo.InvariantCulture);
        public static readonly String S3Compatible_ALTERNATIVE_DATE = "X-AMZ-DATE".ToLower(CultureInfo.InvariantCulture);

        //parameters for get bucket
        public static readonly String PREFIX = "prefix";
        public static readonly String MARKER = "marker";
        public static readonly String MAX_KEYS = "max-keys";
        public static readonly String DELIMITER = "delimiter";

        //request propetry
        public static readonly Int32 DefaultHttpRequestTimeOut = 30 * 60 * 1000;
    }
}
