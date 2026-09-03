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





namespace AvePoint.Media.Common
{
    #region using directives
    using System;
    using System.Diagnostics;
    using System.IO;
    #endregion

    public static class PathUtility
    {
        public static String PathRelativeTo(String path, String relativeToDirectory)
        {
            Debug.Assert(!relativeToDirectory.EndsWith("\\", StringComparison.OrdinalIgnoreCase));

            var fullPath = Path.GetFullPath(path);
            var fullCurrentDirectory = Path.GetFullPath(relativeToDirectory);

            var commonToSlashIndex = -1;
            for (int i = 0; i < fullPath.Length; i++)
            {
                var cFullPath = fullPath[i];
                if (i >= fullCurrentDirectory.Length)
                {
                    if (cFullPath == '\\')
                        commonToSlashIndex = i;
                    break;
                }
                var cCurrentDirectory = fullCurrentDirectory[i];

                if (cCurrentDirectory != cFullPath)
                {
                    if (Char.IsLower(cCurrentDirectory))
                        cCurrentDirectory = (Char)(cCurrentDirectory - (Char)('a' - 'A'));
                    if (Char.IsLower(cFullPath))
                        cFullPath = (Char)(cFullPath - (Char)('a' - 'A'));
                    if (cCurrentDirectory != cFullPath)
                        break;
                }

                if (cCurrentDirectory == '\\')
                    commonToSlashIndex = i;
            }

            // There is no common prefix between the two paths, we give up.
            if (commonToSlashIndex < 0)
                return path;

            var returnVal = String.Empty;
            int nextSlash = commonToSlashIndex;
            for (; ; )
            {
                if (nextSlash >= fullCurrentDirectory.Length)
                    break;
                if (returnVal.Length > 0)
                    returnVal += "\\";
                returnVal += @"..";
                if (nextSlash + 1 == fullCurrentDirectory.Length)
                    break;
                nextSlash = fullCurrentDirectory.IndexOf('\\', nextSlash + 1);
                if (nextSlash < 0)
                    break;
            }

            var rest = fullPath.Substring(commonToSlashIndex + 1);
            returnVal = Path.Combine(returnVal, rest);
            Debug.Assert(string.Compare(Path.GetFullPath(Path.Combine(relativeToDirectory, returnVal)), fullPath, StringComparison.OrdinalIgnoreCase) == 0);
            return returnVal;
        }
    }
}
