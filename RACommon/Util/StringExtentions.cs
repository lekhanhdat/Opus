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

using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AvePoint.Records.Core.Utilities.Extensions
{
    /*
     * there should be NO business logic. 
     * 
     */

    /// <summary>
    /// 
    /// </summary>
    public static class StringExtentions
    {
        private static readonly Dictionary<string, string> escapeMapping = new()
        {
            { "%", "%25" },
            { "'", "''" },
            { "&", "%26" },
            { "+", "%2b" },
            { "#", "%23" }
        };

        public static Guid ToMd5(this string source)
        {
            return HashCodeHelper.StringHash(source);
        }
        public static bool Eq(this string left, string right)
        {
            return left.Equals(right, StringComparison.OrdinalIgnoreCase);
        }

        #region 转义查询关键字中包含的SQL通配符
        /// <summary>
        /// 转义查询关键字中包含的SQL通配符
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string TransferSpecialCharacter(this string input)
        {
            if(!string.IsNullOrEmpty(input))
            {
                try
                {
                    MatchEvaluator evalor = new MatchEvaluator(match => $"\\{match.Value}");
                    return Regex.Replace(input, "[%_]", evalor, RegexOptions.None, RecordsConstants.REGEX_DEFAULT_MATCH_TIMEOUT);
                }
                catch
                {
                    return input;
                }
            }
            return input;
        }

        public static string TransferSpecialCharacterForReport(this string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                try
                {
                    MatchEvaluator evalor = new MatchEvaluator(match => $@"|{match.Value}");
                    return Regex.Replace(input, "[%_]", evalor, RegexOptions.None, RecordsConstants.REGEX_DEFAULT_MATCH_TIMEOUT);
                }
                catch
                {
                    return input;
                }
            }
            return input;
        }
        #endregion

        public static Boolean EndWithIgnoreCase(this String currentValue, String endValue)
        {
            return currentValue.EndsWith(endValue, StringComparison.OrdinalIgnoreCase);
        }

        public static string EscapeSpecialCharacters(this string source)
        {
            if (!string.IsNullOrEmpty(source))
            {
                escapeMapping.Keys.ToList().ForEach(specialChar =>
                {
                    if (source.Contains(specialChar))
                    {
                        source = source.Replace(specialChar, escapeMapping[specialChar]);
                    }
                });

            }
            return source;
        }
    }
}
