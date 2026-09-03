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
namespace System
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;


    public static class StringExtension
    {
        //public static SecureString ToSecureString(this string item)
        //{
        //    if (!string.IsNullOrEmpty(item))
        //    {
        //        var secureString = new SecureString();
        //        foreach (var c in item)
        //        {
        //            secureString.AppendChar(c);
        //        }

        //        return secureString;
        //    }

        //    return null;
        //}

        public static string ToPlainString(this SecureString secureString)
        {
        	if (secureString == null)
            {
                return null;
            }
            IntPtr intPtr = Marshal.SecureStringToBSTR(secureString);
            try
            {
                if (intPtr == IntPtr.Zero)
                {
                    return string.Empty;
                }
                return Marshal.PtrToStringBSTR(intPtr);
            }
            finally
            {
                //Marshal.FreeBSTR(intPtr);
                Marshal.ZeroFreeBSTR(intPtr);
            }
        }

        public static int GetHashCodeV1(this SecureString secureString)
        {
            if (secureString != null)
            {
                return secureString.ToPlainString().GetHashCode();
            }

            return 0;
        }
    }
}