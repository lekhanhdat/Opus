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
using System.Globalization;

namespace Microsoft365.Authentication.ADAL
{
	internal static class DateTimeHelper
	{
		public static long ConvertToTimeT(DateTime time)
		{
			DateTime d = new DateTime(1970, 1, 1, 0, 0, 0, 0);
			return (long)(time - d).TotalSeconds;
		}

		public static DateTime ConvertFromTimeT(long seconds)
		{
			return DateTime.SpecifyKind(new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds((double)seconds), DateTimeKind.Utc);
		}

		public static string BuildTimeString(DateTime utcTime)
		{
			return utcTime.ToString("yyyy-MM-ddTHH:mm:ss.068Z", CultureInfo.InvariantCulture);
		}
	}
}