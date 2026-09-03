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
namespace AvePoint.Media.Storage.Cloud.Dropbox
{
    #region using
    using AvePoint.Media.Storage.Util;
    using System;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Script.Serialization;
    #endregion

    class DropboxUtil
    {

        internal static UInt64 GetSize(String jsonText, String pattern)
        {
            var result = default(UInt64);
            var matches = Regex.Matches(jsonText, pattern);
            foreach (var nextMatch in matches)
            {
                var temp = nextMatch.ToString().Split(':');
                result = Convert.ToUInt64(temp[1]);
            }
            return result;
        }

        internal static DropboxObject ParseJsonString(String jsonStr)
        {
            var js = new JavaScriptSerializer();
            var jsStr = jsonStr.Replace(".tag", "tag");
            return js.Deserialize<DropboxObject>(jsStr);
        }

        internal static String UrlEncode(String systemLocation, String highAndLowName)
        {
            return HttpUtility.UrlEncode(PathUtil.CombinePath(systemLocation, highAndLowName).Replace("\\", "/")).Replace("%2f", "/").Replace("+", "%20");
        }
    }
}
