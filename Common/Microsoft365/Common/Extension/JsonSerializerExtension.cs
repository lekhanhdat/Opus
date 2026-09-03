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
using Newtonsoft.Json;

namespace System.Text.Json
{
    public static class JsonSerializerExtension
    {
        private static JsonSerializerOptions WriteIntentIgnoreDefaultOption =
            new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = Serialization.JsonIgnoreCondition.WhenWritingDefault
            };

        public static String ToIndentedJson<T>(this T obj)
        {
            return JsonSerializer.Serialize(obj, WriteIntentIgnoreDefaultOption);
        }

        public static String ToIndentedJson(this string jsonStr)
        {
            var obj = JsonConvert.DeserializeObject(jsonStr);
            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }
    }

}
