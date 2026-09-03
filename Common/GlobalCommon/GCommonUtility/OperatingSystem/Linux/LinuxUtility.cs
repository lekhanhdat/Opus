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
namespace AvePoint.GCommon.Utility
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    internal static class LinuxUtility
    {
        public static void GetValues(string[] lines, List<ClassPropertyMatch> matches)
        {
            var count = matches.Count;
            foreach (string l in lines)
            {
                foreach (ClassPropertyMatch m in matches)
                {
                    Match match = m.Regex.Match(l);
                    if (match.Groups[0].Success)
                    {
                        string value = match.Groups[1].Value;
                        m.SetValue(value);
                    }
                }
                if (count == 0)
                    break;
            }
        }

        public static string GetValue(string[] lines, string pattern)
        {
            var regex = new Regex(pattern);
            foreach (string l in lines)
            {
                Match match = regex.Match(l);
                if (match.Groups[0].Success)
                {
                    return match.Groups[1].Value;
                }
            }
            return null;
        }
    }
}
