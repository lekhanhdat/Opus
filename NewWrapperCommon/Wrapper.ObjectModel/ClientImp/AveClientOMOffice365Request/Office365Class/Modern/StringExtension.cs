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

namespace AvePoint.ObjectModel.ClientOM
{
    static class StringExtension
    {
        public static string Trim(this string source, bool trimStart, bool trimEnd, params string[] trims)
        {
            if (string.IsNullOrEmpty(source))
            {
                return null;
            }

            string result = source.Trim();
            if (trims == null || trims.Length <= 0)
            {
                result = result.Trim();
                return source;
            }
            foreach (var trim in trims)
            {
                if (string.IsNullOrEmpty(trim))
                {
                    result = result.Trim();
                    return source;
                }
                if (trimStart)
                {
                    while (result.StartsWith(trim, System.StringComparison.OrdinalIgnoreCase))
                    {
                        result = result.Substring(trim.Length);
                    }
                }
                if (trimEnd)
                {
                    while (result.EndsWith(trim, System.StringComparison.OrdinalIgnoreCase))
                    {
                        result = result.Substring(0, result.Length - trim.Length);
                    }
                }
            }
            return result;
        }
    }
}
