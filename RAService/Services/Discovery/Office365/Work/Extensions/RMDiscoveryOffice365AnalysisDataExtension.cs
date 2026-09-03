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
using AngleSharp.Common;
using AvePoint.RA.DB.Core.Discovery;
using AvePoint.RA.DB.Model.Discovery;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work.Extensions
{
    public static class RMDiscoveryOffice365AnalysisDataExtension
    {
        public static string GetValue(this ExpandoObject data, string key, string defaultValue = null)
        {
            var res = data.TryGet(key);
            if (res == null)
            {
                return defaultValue;
            }
            return res.ToString();
        }

        public static T GetValue<T>(this ExpandoObject data, string key, T defaultValue = default) where T : struct
        {
            var res = data.TryGet<T>(key);
            if(res == null)
            {
                return defaultValue;
            }
            return res.Value;
        }

        public static ExpandoObject ConvertToExpandoObject(this IDictionary<string, object> propertyDics)
        {
            if (propertyDics == null) return null;

            IDictionary<string, object> expandoObject = new ExpandoObject();
            foreach (var d in propertyDics)
            {
                expandoObject.Add(d);
            }

            return expandoObject as ExpandoObject;
        }
    }
}
