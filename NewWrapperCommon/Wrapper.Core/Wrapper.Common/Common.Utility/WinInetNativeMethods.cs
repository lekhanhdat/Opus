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
using System.Text;
using System.Runtime.InteropServices;

namespace AvePoint.Wrapper.Common
{
    public class WinInetNativeMethods
    {
        [DllImport("wininet.dll", SetLastError=true, CharSet=CharSet.Auto)]
        public static extern IntPtr FindFirstUrlCacheEntry(
            [MarshalAs(UnmanagedType.LPTStr)] string searchPattern,
            IntPtr cacheEntryInfo,
            ref int bufferSize);

        [DllImport("wininet.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool FindNextUrlCacheEntry(
            IntPtr hEnumHandle,
            IntPtr nextCacheEntryInfo,
            ref int nextCacheEntryInfoBufferSize);

        [DllImport("wininet.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool FindCloseUrlCache(
            IntPtr enumHandle);

        [DllImport("wininet.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool DeleteUrlCacheEntry(
            [MarshalAs(UnmanagedType.LPTStr)]string sourceUrlName);

        [DllImport("wininet.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool InternetGetCookie(
            [MarshalAs(UnmanagedType.LPTStr)]string url,
            [MarshalAs(UnmanagedType.LPTStr)]string cookeName, 
            StringBuilder cookieData, 
            ref int size);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct INTERNET_CACHE_ENTRY_INFO
    {
        public int dwStructSize; 
        public IntPtr lpszSourceUrlName; 
        public IntPtr lpszLocalFileName; 
        public int CacheEntryType; 
        public int dwUseCount; 
        public int dwHitRate; 
        public int dwSizeLow; 
        public int dwSizeHigh; 
        public System.Runtime.InteropServices.ComTypes.FILETIME LastModifiedTime;
        public System.Runtime.InteropServices.ComTypes.FILETIME ExpireTime;
        public System.Runtime.InteropServices.ComTypes.FILETIME LastAccessTime;
        public System.Runtime.InteropServices.ComTypes.FILETIME LastSyncTime; 
        public IntPtr lpHeaderInfo; 
        public int dwHeaderInfoSize; 
        public IntPtr lpszFileExtension; 
        public int dwExemptDelta; 
    }

 
}
