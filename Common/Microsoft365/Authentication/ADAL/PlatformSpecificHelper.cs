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
using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Microsoft365.Authentication.ADAL
{
	internal static class PlatformSpecificHelper
	{
		private static class NativeMethods
		{
			[DllImport("secur32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
			[return: MarshalAs(UnmanagedType.U1)]
			public static extern bool GetUserNameEx(int nameFormat, StringBuilder userName, ref uint userNameSize);
		}

		public static string GetProductName()
		{
			return ".NET";
		}

		public static string GetEnvironmentVariable(string variable)
		{
			string environmentVariable = Environment.GetEnvironmentVariable(variable);
			if (string.IsNullOrWhiteSpace(environmentVariable))
			{
				return null;
			}
			return environmentVariable;
		}

		public static string PlatformSpecificToLower(this string input)
		{
			return input.ToLower(CultureInfo.InvariantCulture);
		}

		public static string GetUserPrincipalName()
		{
			uint userNameSize = 0u;
			NativeMethods.GetUserNameEx(8, null, ref userNameSize);
			if (userNameSize == 0)
			{
				throw new AdalException("get_user_name_failed", new Win32Exception(Marshal.GetLastWin32Error()));
			}
			StringBuilder stringBuilder = new StringBuilder((int)userNameSize);
			if (!NativeMethods.GetUserNameEx(8, stringBuilder, ref userNameSize))
			{
				throw new AdalException("get_user_name_failed", new Win32Exception(Marshal.GetLastWin32Error()));
			}
			return stringBuilder.ToString();
		}

		public static string CreateSha256Hash(string input)
		{
            return input;

        }

		public static void CloseHttpWebResponse(WebResponse response)
		{
			response.Close();
		}
	}
}