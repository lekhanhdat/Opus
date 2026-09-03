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
using System.Runtime.InteropServices;

namespace RAFileSystem.FileSystem.DataSync.Utils
{
    internal static class FileTypeDescriptionResolver
    {
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern System.IntPtr SHGetFileInfo(
            string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi,
            uint cbFileInfo, uint uFlags);

        private const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;
        private const uint SHGFI_TYPENAME = 0x000000400;
        private const uint FILE_ATTRIBUTE_NORMAL = 0x80;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SHFILEINFO
        {
            public System.IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        public static string Resolve(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
            {
                return string.Empty;
            }

            string dummyFileName = "file." + extension.TrimStart('.');
            var shfi = new SHFILEINFO();
            SHGetFileInfo(dummyFileName, FILE_ATTRIBUTE_NORMAL, ref shfi,
                (uint)Marshal.SizeOf(shfi), SHGFI_TYPENAME | SHGFI_USEFILEATTRIBUTES);

            return shfi.szTypeName ?? string.Empty;
        }
    }
}