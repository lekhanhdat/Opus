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

namespace AvePoint.Wrapper.Common
{
    public class AveStringHelper
    {
        public static string TrimEnd(string str, params char[] trimchars)
        {            
            return string.IsNullOrEmpty(str) ? str : str.TrimEnd(trimchars);
        }

        public static object TrimEnd(object obj, params char[] trimchars)
        {
            return obj is string ? TrimEnd(obj as string, trimchars) : obj;
        }

        public static string Trim(string str, params char[] trimchars)
        {
            return string.IsNullOrEmpty(str) ? str : str.Trim(trimchars);
        }

        public static object Trim(object obj, params char[] trimchars)
        {
            return obj is string ? Trim(obj as string) : obj;
        }
    }
}
