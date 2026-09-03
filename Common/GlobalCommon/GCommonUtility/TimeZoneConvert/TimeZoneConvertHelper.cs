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
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TimeZoneConverter;

namespace AvePoint.GCommon.Utility.TimeZoneConvert
{
    public static class TimeZoneConvertHelper
    {
        public static TimeZoneInfo FindSystemTimeZoneById(string windowsId)
        {
            try
            {
                var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
                if (isWindows)
                {
                    return TimeZoneInfo.FindSystemTimeZoneById(windowsId);
                }
                else
                {
                    TimeZoneInfo returnTZ = null;
                    if (TimeZoneInfo.TryConvertWindowsIdToIanaId(windowsId, out string ianaId))
                    {
                        //logger.Info($"Convert windows id[{windowsId}] to iana id[{ianaId}] successfully");
                        returnTZ = TimeZoneInfo.FindSystemTimeZoneById(ianaId);
                    }
                    if (returnTZ == null)
                    {
                        return TZConvert.GetTimeZoneInfo(windowsId);
                    }
                    return returnTZ;
                }
            }
            catch (Exception)
            {
                return TimeZoneInfo.Utc;
            }
        }
    }
}
