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
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Microsoft365.Authentication.ADAL
{
	/// <summary>
	/// This class adds additional query parameters or headers to the requests sent to STS. This can help us in
	/// collecting statistics and potentially on diagnostics.
	/// </summary>
	/// <summary>
	/// This class adds additional query parameters or headers to the requests sent to STS. This can help us in
	/// collecting statistics and potentially on diagnostics.
	/// </summary>
	internal class AdalIdHelper
	{
		private static class NativeMethods
		{
			private struct SYSTEM_INFO
			{
				public short wProcessorArchitecture;

				public short wReserved;

				public int dwPageSize;

				public IntPtr lpMinimumApplicationAddress;

				public IntPtr lpMaximumApplicationAddress;

				public IntPtr dwActiveProcessorMask;

				public int dwNumberOfProcessors;

				public int dwProcessorType;

				public int dwAllocationGranularity;

				public short wProcessorLevel;

				public short wProcessorRevision;
			}


			[DllImport("kernel32.dll")]
			private static extern void GetNativeSystemInfo(ref SYSTEM_INFO lpSystemInfo);

			public static string GetProcessorArchitecture()
			{
				try
				{
					SYSTEM_INFO lpSystemInfo = default(SYSTEM_INFO);
					GetNativeSystemInfo(ref lpSystemInfo);
					switch (lpSystemInfo.wProcessorArchitecture)
					{
					case 6:
					case 9:
						return "x64";
					case 5:
						return "ARM";
					case 0:
						return "x86";
					default:
						return "Unknown";
					}
				}
				catch
				{
					return "Unknown";
				}
			}
		}

		public static string GetProcessorArchitecture()
		{
			return NativeMethods.GetProcessorArchitecture();
		}

		public static void AddAsQueryParameters(RequestParameters parameters)
		{
			NetworkPlugin.RequestCreationHelper.AddAdalIdParameters(parameters);
		}

		public static void AddAsHeaders(IHttpWebRequest request)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			NetworkPlugin.RequestCreationHelper.AddAdalIdParameters(dictionary);
			HttpHelper.AddHeadersToRequest(request, dictionary);
		}

		public static string GetAdalVersion()
		{
			string fullName = typeof(AdalIdHelper).GetTypeInfo().Assembly.FullName;
			Regex regex = new Regex("Version=[\\d]+.[\\d]+.[\\d]+.[\\d]+");
			Match match = regex.Match(fullName);
			if (match.Success)
			{
				string[] array = match.Groups[0].Value.Split(new char[1]
				{
					'='
				}, StringSplitOptions.None);
				return array[1];
			}
			return null;
		}

		public static string GetAssemblyFileVersion()
		{
			var customAttribute = typeof(AdalIdHelper).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();
			if (customAttribute == null)
			{
				return string.Empty;
			}
			return customAttribute.Version;
		}

		public static string GetAssemblyInformationalVersion()
		{
			AssemblyInformationalVersionAttribute customAttribute = typeof(AdalIdHelper).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
			if (customAttribute == null)
			{
				return string.Empty;
			}
			return customAttribute.InformationalVersion;
		}
	}
}