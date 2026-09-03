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
using AvePoint.RA.Common.Configurations;
using Microsoft.AspNetCore.Mvc;
using System;

namespace AvePoint.RA.Web.Common
{
    public static class RAUrlHelper
    {
        public static string Script(this IUrlHelper helper, string value)
        {
            var version = RMGlobalConfiguration.EnvSetting.ProductVersion;
            if (string.IsNullOrEmpty(version))
            {
                return helper.Content(value);
            }
            else
            {
                return helper.Content(string.Format(value + "?_v={0}", version));
            }
        }

        public static string Script(this IUrlHelper helper, string value, bool isDebug)
        {
            if (!string.IsNullOrEmpty(value))
            {
                bool isMin = false;
                int idx = value.LastIndexOf(".min.", StringComparison.OrdinalIgnoreCase);
                if(idx > 0 && value.IndexOf('.', idx + 5) < 0)
                {
                    isMin = true;
                }
                if (isDebug)
                {
                    if (isMin)
                    {
                        value = value.Remove(idx, 4);
                    }
                }
                else
                {
                    if (!isMin)
                    {
                        idx = value.LastIndexOf('.');
                        if (idx > -1)
                        {
                            value = string.Format("{0}.min{1}", value.Substring(0, idx), value.Substring(idx));
                        }
                    }
                }
            }
            return Script(helper, value);
        }
    }
}