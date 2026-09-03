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



namespace AvePoint.Media.Storage.Cloud.Amazon
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Globalization;
    #endregion

    class AmazonConstants
    {
        public const string Authorization_HEADER = "Authorization";
        public static readonly string AWS3_REST_HEADER_PREFIX = "X-AMZ-".ToLower(CultureInfo.InvariantCulture);
        public static readonly string AWS_REST_METADATA_HEADER_PREFIX = "X-AMZ-META-".ToLower(CultureInfo.InvariantCulture);
        public static readonly string AWS3_ALTERNATIVE_DATE = "X-AMZ-DATE".ToLower(CultureInfo.InvariantCulture);
        
        //parameters for get bucket
        public const string PREFIX = "prefix";
        public const string MARKER = "marker";
        public const string MAX_KEYS = "max-keys";
        public const string DELIMITER = "delimiter";

        //Amazon s3 region type
        public const string US_WEST = "uswest";
        public const string EU = "eu";
        public const string US = "usstandard";
        public const string APAC = "apac";
        public const string TOKYO = "tokyo";
        public const string OREGON = "oregon";
        public const string SYDNEY = "sydney";
        public const string SAO_PAULO = "saopaulo";
        public const string EU_Frankfurt = "frankfurt";

        //Amazon s3 region
        public static readonly string REGION_US_EAST = "US-EAST-1".ToLower(CultureInfo.InvariantCulture);
        public static readonly string REGION_US_WEST = "US-WEST-1".ToLower(CultureInfo.InvariantCulture);
        public static readonly string REGION_EU = "EU-WEST-1".ToLower(CultureInfo.InvariantCulture);
        public static readonly string REGION_APAC = "AP-SOUTHEAST-1".ToLower(CultureInfo.InvariantCulture);
        public static readonly string REGION_TOKYO = "AP-NORTHEAST-1".ToLower(CultureInfo.InvariantCulture);
        public static readonly string REGION_SYDNEY = "AP-SOUTHEAST-2".ToLower(CultureInfo.InvariantCulture);//Sydney
        public static readonly string REGION_OREGON = "US-WEST-2".ToLower(CultureInfo.InvariantCulture); //Oregon
        public static readonly string REGION_SAOPAULO = "SA-EAST-1".ToLower(CultureInfo.InvariantCulture);//Sao Paulo
        public static readonly string REGION_FRANKFURT = "eu-central-1".ToLower(CultureInfo.InvariantCulture);
    }
}
