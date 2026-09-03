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
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;

namespace AvePoint.ObjectModel.Common
{
    internal class AveJsonUtility
    {
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(AveJsonUtility));
        private static long JS_BASELINE_TICKS;

        // Methods
        static AveJsonUtility()
        {
            DateTime time = new DateTime(0x7b2, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            JS_BASELINE_TICKS = time.Ticks;
        }

        public static string CanonicalizeJSName(string name)
        {
            return (new string(name[0], 1).ToLowerInvariant() + name.Substring(1));
        }

        public static long DateTicksToJsTicks(long dateTicks)
        {
            return ((dateTicks - JS_BASELINE_TICKS) / 0x2710L);
        }

        public static DateTime? DeserializeDateTime(string serializedDate)
        {
            DateTime? nullable = null;
            if (serializedDate != null)
            {
                serializedDate = UnquoteString(serializedDate);
                if (serializedDate.StartsWith(@"\/Date(", StringComparison.Ordinal))
                {
                    serializedDate = serializedDate.Substring(1, serializedDate.Length - 2);
                }
                if (!serializedDate.StartsWith("/Date(", StringComparison.Ordinal))
                {
                    return nullable;
                }
                try
                {
                    nullable = new DateTime(JsTicksToDateTicks(long.Parse(serializedDate.Substring(6, (serializedDate.Length - 2) - 6))));
                }
                catch (Exception ex)
                {
                    mLogger.Warn("fail desrialize date time ,ex:" + ex);
                }
            }
            return nullable;
        }

        public static string EncodeFunctionCall(string name, IEnumerable<string> args)
        {
            if (name == null)
            {
                return null;
            }
            StringBuilder builder = new StringBuilder(name.Replace(";", ";;"));
            foreach (string str in args)
            {
                if (str != null)
                {
                    builder.Append("<;>");
                    builder.Append(str.Replace(";", ";;"));
                }
            }
            return QuoteString(JsonEncode(builder.ToString()));
        }

        private static ConcurrentDictionary<string, IEnumerable<PropertyInfo>> cache = new ConcurrentDictionary<string, IEnumerable<PropertyInfo>>();

        public static IEnumerable<PropertyInfo> GetPropertiesToSerializeForType(Type t)
        {
            string str = string.Format(CultureInfo.InvariantCulture, "{0} : {1}-{2}", new object[] { "JsGridPropertyTypeSerializationInfo", t.Assembly, t.FullName });
            IEnumerable<PropertyInfo> enumerable = null;
            cache.TryGetValue(str, out enumerable);
            if (enumerable == null)
            {
                if (t.GetCustomAttributes(typeof(AveJsonIgnore), true).Length != 0)
                {
                    return new List<PropertyInfo>();
                }
                cache[str] = enumerable = t.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Where<PropertyInfo>(delegate(PropertyInfo inf)
                {
                    return inf.CanRead && (inf.GetCustomAttributes(typeof(AveJsonIgnore), true).Length == 0) && AveTypeHelper.IsBasicType(inf.PropertyType);
                });
            }
            return enumerable;
        }

        public static bool IsValidLeftHandJsonString(string json)
        {
            return !Regex.IsMatch(json, @"({|}|\(|\)|\]|\[)", RegexOptions.CultureInvariant | RegexOptions.Multiline);
        }

        public static string JsonEncode(string s)
        {
            if (!string.IsNullOrEmpty(s))
            {
                return s.Replace(@"\", @"\\").Replace("\"", "\\\"").Replace("/", @"\/").Replace("\b", @"\b").Replace("\f", @"\f").Replace("\n", @"\n").Replace("\r", @"\r").Replace("\t", @"\t").Replace("\0", string.Empty);
            }
            return s;
        }

        public static long JsTicksToDateTicks(long jsTicks)
        {
            return ((0x2710L * jsTicks) + JS_BASELINE_TICKS);
        }

        public static string QuoteString(string s)
        {
            return string.Format(CultureInfo.InvariantCulture, "\"{0}\"", new object[] { s });
        }

        public static string SerializeDateTime(DateTime dt)
        {
            return ("\"\\/Date(" + DateTicksToJsTicks(dt.Ticks).ToString(CultureInfo.InvariantCulture) + ")\\/\"");
        }

        public static string SerializeToJsonFromProperties(AveJsonSerializer s, object obj)
        {
            if (obj == null || obj is Type || obj is System.IO.Stream)
            {
                return null;
            }
            Dictionary<string, object> o = new Dictionary<string, object>();
            foreach (PropertyInfo info in GetPropertiesToSerializeForType(obj.GetType()))
            {
                o[CanonicalizeJSName(info.Name)] = info.GetValue(obj, null);
            }

            return ((s == null) ? AveJsonSerializer.Shared : s).SerializeToJson(o);
        }

        public static string UnquoteString(string s)
        {
            if (s == null)
            {
                return null;
            }
            if (s.StartsWith("\"", StringComparison.Ordinal) && s.EndsWith("\"", StringComparison.Ordinal))
            {
                return s.Substring(1, s.Length - 2);
            }
            return s;
        }
    }

    internal interface IAveJsonSerializable
    {
        // Methods
        string ToJson(AveJsonSerializer s);
    }

    public class AveJsonIgnore : Attribute
    {
    }

    public delegate string AveJsonEncoder(AveJsonSerializer s, object obj);
    public delegate string AveJsonGenerator(AveJsonSerializer s);
}
