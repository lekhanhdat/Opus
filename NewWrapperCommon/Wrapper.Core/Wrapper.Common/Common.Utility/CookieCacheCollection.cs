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
using System.Net;
using System.Runtime.InteropServices;

namespace AvePoint.Wrapper.Common
{
    public class CookieCacheCollection : IEnumerable<INTERNET_CACHE_ENTRY_INFO>
    {
        private const int Error_No_More_Items = 259;
        private const int Error_Insufficient_Buffer = 122;
        private const string CookiePattern = "cookie:";

        public CookieCacheCollection(string url)
        {
            this.Url = url;
        }

        #region IEnumerable<Cookie> Members

        public IEnumerator<INTERNET_CACHE_ENTRY_INFO> GetEnumerator()
        {
            return new CookieCacheEnumerator(this);
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new CookieCacheEnumerator(this);
        }

        #endregion

        public string Url
        {
            get;
            set;
        }

        private class CookieCacheEnumerator : IEnumerator<INTERNET_CACHE_ENTRY_INFO>
        {
            private CookieCacheCollection mCookieCacheCol;
            private INTERNET_CACHE_ENTRY_INFO mCurrentCacheEntry;
            private int mRequiredSize = 0;
            private IntPtr mBuffer = IntPtr.Zero;
            private IntPtr mEnumHandle = IntPtr.Zero;
            private bool mIsFirst = true;

            public CookieCacheEnumerator(CookieCacheCollection cookieCacheCol)
            {
                mCookieCacheCol = cookieCacheCol;
            }

            #region IEnumerator<INTERNET_CACHE_ENTRY_INFO> Members

            public INTERNET_CACHE_ENTRY_INFO Current
            {
                get { return mCurrentCacheEntry; }
            }

            #endregion

            #region IDisposable Members

            public void Dispose()
            {
                if (mBuffer != IntPtr.Zero)
                {
                    Marshal.Release(mBuffer);
                }
            }

            #endregion

            #region IEnumerator Members

            object System.Collections.IEnumerator.Current
            {
                get { return mCurrentCacheEntry; }
            }

            public bool MoveNext()
            {
                if (mIsFirst)
                {
                    mIsFirst = false;
                    return FindFirst();
                }
                else
                {
                    return FindNext();
                }
            }

            public void Reset()
            {
                mIsFirst = true;
                mRequiredSize = 0;
            }


            private bool FindFirst()
            {
                WinInetNativeMethods.FindFirstUrlCacheEntry(CookiePattern, IntPtr.Zero, ref mRequiredSize);
                int win32Error = Marshal.GetLastWin32Error();
                if (win32Error == Error_No_More_Items)
                {
                    return false;
                }
                else if (win32Error == Error_Insufficient_Buffer)
                {
                    mBuffer = Marshal.AllocHGlobal((IntPtr)mRequiredSize);
                    mEnumHandle = WinInetNativeMethods.FindFirstUrlCacheEntry(CookiePattern, mBuffer, ref mRequiredSize);
                    if (mEnumHandle == IntPtr.Zero)
                    {
                        return false;
                    }
                    mCurrentCacheEntry = (INTERNET_CACHE_ENTRY_INFO)Marshal.PtrToStructure(mBuffer, typeof(INTERNET_CACHE_ENTRY_INFO));
                    mRequiredSize = 0;
                    return true;
                }
                return false;
            }

            private bool FindNext()
            {
                bool result = WinInetNativeMethods.FindNextUrlCacheEntry(mEnumHandle, IntPtr.Zero, ref mRequiredSize);
                int win32Error = Marshal.GetLastWin32Error();
                if (!result && win32Error == Error_No_More_Items)
                {
                    return false;
                }
                if (!result && win32Error == Error_Insufficient_Buffer)
                {
                    mBuffer = Marshal.ReAllocHGlobal(mBuffer, (IntPtr)mRequiredSize);
                    result = WinInetNativeMethods.FindNextUrlCacheEntry(mEnumHandle, mBuffer, ref mRequiredSize);
                    if (result)
                    {
                        mCurrentCacheEntry = (INTERNET_CACHE_ENTRY_INFO)Marshal.PtrToStructure(mBuffer, typeof(INTERNET_CACHE_ENTRY_INFO));
                        mRequiredSize = 0;
                        return true;
                    }
                }
                return false;
            }
            #endregion
        }
    }
}
