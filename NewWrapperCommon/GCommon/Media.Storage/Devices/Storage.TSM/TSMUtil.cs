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

namespace AvePoint.Media.Storage.TSM
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.IO;
    using AvePoint.GCommon;
    using AvePoint.Media.Storage.Util;
    #endregion

    class TSMUtil
    {

        public static string[] SplitHighName(string fullName)
        {
            fullName = fullName.Replace('/', '\\');
            int index = fullName.LastIndexOf('\\');
            if (index >= 0)
            {
                if (index == 0)
                {
                    return new string[] { "", fullName };
                }
                else
                {
                    return new string[] { fullName.Substring(0, index), fullName.Substring(index, fullName.Length - index) };
                }
            }
            return null;
        }
        public static StorageInfo FormateTsmNode(StorageInfo info)
        {
            StorageInfo nodeInfo = info.Clone();
            string[] names = SplitHighName(nodeInfo.HighPlusLowName);
            string hl = nodeInfo.HighName;
            string ll = nodeInfo.LowName;
            if (names != null)
            {
                hl = names[0];
                ll = names[1];
            }
            if (!string.IsNullOrEmpty(hl))
            {
                if (hl.Contains("/"))
                {
                    hl = hl.Replace("/", "\\");
                }

                if (!hl.StartsWith("\\", StringComparison.OrdinalIgnoreCase))
                {
                    nodeInfo.HighName = "\\" + hl.TrimEnd(new char[] { '\\' });
                }
                else
                {
                    nodeInfo.HighName = hl.TrimEnd(new char[] { '\\' });
                }
                
            }
            if (!string.IsNullOrEmpty(ll))
            {

                if (ll.Contains("/"))
                {
                    ll = ll.Replace("/", "\\");
                }

                if (!ll.StartsWith("\\", StringComparison.OrdinalIgnoreCase))
                {
                    nodeInfo.LowName = "\\" + ll;
                }
                else
                {
                    nodeInfo.LowName = ll;
                }
            }
            return nodeInfo;
        }

        /// <summary>
        /// Add delimiter for string 
        /// </summary>
        /// <param name="str">The original string </param>
        /// <returns>The complete string </returns>
        public static string AddDelimiter(string str)
        {
            if (str.Equals(string.Empty))
            {
                return string.Empty;
            }
            if (!str.StartsWith("\\", StringComparison.OrdinalIgnoreCase))
            {
                return ("\\" + str).Trim();
            }
            else
            {
                return str;
            }
        }

        /// <summary>
        /// Add before and after the delimiter
        /// </summary>
        /// <param name="str">The original string</param>
        /// <returns>The complete string </returns>
        public static string AddDelimiterAndDirMark(string str)
        {
            return ("\\" + str + "\\").Trim();
        }


        public static string EncodeToAscii(string unicodeString)
        {
            Encoding ascii = Encoding.ASCII;
            Encoding unicode = Encoding.Unicode;

            byte[] unicodeBytes = unicode.GetBytes(unicodeString);

            byte[] asciiBytes = Encoding.Convert(unicode, ascii, unicodeBytes);

            char[] asciiChars = new char[ascii.GetCharCount(asciiBytes, 0, asciiBytes.Length)];
            ascii.GetChars(asciiBytes, 0, asciiBytes.Length, asciiChars, 0);
            string asciiString = new string(asciiChars);

            return asciiString;
        }

    }

    class TSMConst
    {
        public const string tsmResourceRoot = @"storage\tsm";
        public const string tsmApiFullName = tsmResourceRoot + @"\api\StorageTSMAPI.dll";
    }
}
