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
using Google.Apis.Admin.Reports.reports_v1.Data;

namespace RAGoogle.Extension;

public static class GoogleActivityExtension
{
    public static string TryGetValueByName(this Activity.EventsData events, string name)
    {
        var result = string.Empty;
        if (events.Parameters.IsNotNullOrEmpty())
        {
            var paraData = events.Parameters.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (paraData != null)
            {
                result = paraData.Value;
            }
        }
        return result;
    }

    public static bool? TryGetBoolValueByName(this Activity.EventsData events, string name)
    {
        if (events.Parameters.IsNotNullOrEmpty())
        {
            var paraData = events.Parameters.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (paraData != null)
            {
                return paraData.BoolValue;
            }
        }
        return null;
    }
}
