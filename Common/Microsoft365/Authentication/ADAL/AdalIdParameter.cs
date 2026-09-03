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
namespace Microsoft365.Authentication.ADAL
{
	internal static class AdalIdParameter
	{
		/// <summary>
		/// ADAL Flavor: .NET or WinRT
		/// </summary>
		public const string Product = "x-client-SKU";

		/// <summary>
		/// ADAL assembly version
		/// </summary>
		public const string Version = "x-client-Ver";

		/// <summary>
		/// CPU platform with x86, x64 or ARM as value
		/// </summary>
		public const string CpuPlatform = "x-client-CPU";

		/// <summary>
		/// Version of the operating system. This will not be sent on WinRT
		/// </summary>
		public const string OS = "x-client-OS";

		/// <summary>
		/// Device model. This will not be sent on .NET
		/// </summary>
		public const string DeviceModel = "x-client-DM";
	}
}