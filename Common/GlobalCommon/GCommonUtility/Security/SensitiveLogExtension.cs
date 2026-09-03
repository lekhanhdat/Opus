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
using System.Threading.Tasks;

namespace AvePoint.GCommon
{
    public static class SensitiveLogExtension
    {
        /// <summary>
        /// Do not display the file name in the log, the file name is distinguished from parent path by "/"
        /// </summary>
        public static string FormatURLInLog(this string url, int itemId = -1, Guid itemGuid = default(Guid))
        {
            if (string.IsNullOrEmpty(url) || !url.Contains("/"))
            {
                return $"<{itemId};{itemGuid.ToString()}>";
            }
            string parentUrl = url.Substring(0, url.LastIndexOf('/'));
            if (itemId == -1 && itemGuid == default(Guid))
            {
                return parentUrl + "/";
            }
            else
            {
                return parentUrl + "/" + $"<{itemId};{itemGuid.ToString()}>";
            }
        }

        /// <summary>
        /// Do not display the file name in the log, the file name is distinguished from parent path by "\"
        /// </summary>
        public static string FormatFilePathInLog(this string filePath, int itemId = -1)
        {
            if (string.IsNullOrEmpty(filePath) || !filePath.Contains("\\"))
            {
                return filePath;
            }
            string parentPath = filePath.Substring(0, filePath.LastIndexOf('\\'));
            if (itemId == -1)
            {
                return parentPath + "\\";
            }
            else
            {
                return parentPath + "\\" + itemId;
            }
        }
    }
}
