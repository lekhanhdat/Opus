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
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Common
{
    internal class AveJsonUtility
    {
        private const string CacheKeyPrefix = "JsGridPropertyTypeSerializationInfo";
        private static long JS_BASELINE_TICKS;
        private const string JSON_REGEX = @"({|}|\(|\)|\]|\[)";

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
                catch (ArgumentOutOfRangeException)
                {
                }
                catch (FormatException)
                {
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "Js")]
        public static IEnumerable<PropertyInfo> GetPropertiesToSerializeForType(Type t)
        {
            string str = string.Format(CultureInfo.InvariantCulture, "{0} : {1}-{2}", new object[] { "JsGridPropertyTypeSerializationInfo", t.Assembly, t.FullName });
            IEnumerable<PropertyInfo> enumerable = HttpRuntime.Cache[str] as IEnumerable<PropertyInfo>;
            if (enumerable == null)
            {
                if (t.GetCustomAttributes(typeof(AveJsonIgnore), true).Length != 0)
                {
                    return new List<PropertyInfo>();
                }
                HttpRuntime.Cache[str] = enumerable = t.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Where<PropertyInfo>(delegate (PropertyInfo inf)
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

        /// <summary>
        /// 解析Json的字符串，并且返回Dictionary
        /// </summary>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        public static List<Dictionary<string, object>> GetDictionaryFromJsonString(string jsonString)
        {
            if (string.IsNullOrEmpty(jsonString))
            {
                return new List<Dictionary<string, object>>();
            }
            var result = new List<Dictionary<string, object>>();
            #region ADO-200292 由于Url中存在"}",",","{"等特殊字符，会导致字符串截取出现问题，此处把Url先截取处理避免影响之后的处理逻辑
            //Example "Url": "https:\u002f\u002fems823806.sharepoint.com\u002fsites\u002f'&~{}'",
            var urlKey = "\"Url\":\"";//"Url":"
            string urlString = string.Empty;
            var startIndex = jsonString.IndexOf(urlKey, StringComparison.OrdinalIgnoreCase) + urlKey.Length;
            if (startIndex >= urlKey.Length && startIndex < jsonString.Length)
            {
                var endIndex = jsonString.IndexOf('"', startIndex);//
                if (endIndex > startIndex)
                {
                    urlString = jsonString.Substring(startIndex, endIndex - startIndex + 1);
                    jsonString = jsonString.Remove(startIndex, urlString.Length);
                }
            }
            #endregion
            while (!string.IsNullOrEmpty(jsonString))
            {
                int startPos = jsonString.IndexOf('{');
                int endPos = jsonString.IndexOf('}');

                if (startPos < 0 || endPos < 0)
                {
                    break;
                }
                var tmpRs = new Dictionary<string, object>();
                var innerProperties = jsonString.Substring(startPos + 1, endPos - startPos - 1);
                if (innerProperties.IndexOf(urlKey, StringComparison.OrdinalIgnoreCase) > -1)
                {
                    innerProperties = innerProperties.Replace(urlKey, urlKey + urlString);
                }
                var propArray = innerProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                foreach (var arrElement in propArray)
                {
                    var splitPos = arrElement.IndexOf(':');

                    tmpRs.Add(arrElement.Substring(0, splitPos).Trim('\r', '"'), GetValue(arrElement.Substring(splitPos + 1)));
                }

                jsonString = jsonString.Substring(endPos + 1);

                result.Add(tmpRs);
            }
            return result;
        }


        private static object GetValue(string prop)
        {
            var guidPos = prop.IndexOf("\"\\/Guid(", StringComparison.OrdinalIgnoreCase);
            if (guidPos >= 0)
            {
                return new Guid(prop.Substring(guidPos + "\"\\/Guid(".Length, 36));
            }
            if (prop.IndexOf("http", StringComparison.OrdinalIgnoreCase) > 0 && prop.IndexOf(":\\u002f\\u002f", StringComparison.OrdinalIgnoreCase) > 0)//可以同时处理http和https协议的url。
            {
                return ReplaceSiteUrlSpecialCharacter(prop);
            }
            else
            {
                return prop.Trim(' ', '\r', '"');//如果判断property不是url，不会影响到原有逻辑。
            }
        }
        /// <summary>
        /// 此方法用于当365站点有特殊字符时，替换特殊字符为Json格式如“\uffff”为普通字符。
        /// </summary>
        /// <example>
        /// 当输入url为http:\\u002f\\u002fwin-6j981dpvsn2:4000/sites/\\u3042\\u3044\\u3046\\u3048\\u304a时
        /// 本方法会先以‘\\’将字符为分隔符打散为字符串数组，再根据json类型特殊字符的特性，“\uffff”来完成替换，由于Json字符
        /// 中的ffff与char的区间相同，均为16bit unicode，所以可以使用u后面的16进制数据直接转换为char。由于url的规则，不
        /// 允许“\”存在，故“\”可以作为分割的唯一标识符，url中包含字符u或U不会对结果造成任何影响。经过替换，得到返回结果
        /// http://win-6j981dpvsn2:4000/sites/あいうえお
        /// </example>
        /// <exception>
        /// 原则上说不会出现异常，一旦出现异常，直接返回仅仅替换‘/’的url，如果没有特殊字符不会出现问题，如果有特殊字符的json表示，也可以通过该表示定位到此处。
        /// </exception>
        /// <param name="url">从Json中直接取出的url</param>
        /// <returns>经过替换后的url</returns>
        private static String ReplaceSiteUrlSpecialCharacter(String url)
        {
            url = url.Trim(' ', '"');
            var stringList = url.Split('\\');
            StringBuilder sBuilder = new StringBuilder();
            if (stringList.Length > 0)
            {

                for (int i = 0; i < stringList.Length; i++)
                {
                    var stringTemp = stringList[i];
                    if (stringTemp.Length >= 5 && stringTemp[0].Equals('u'))
                    {
                        var charTemp = (char)int.Parse(stringTemp.Substring(1, 4), NumberStyles.HexNumber);
                        stringTemp = stringTemp.Replace(String.Format("u{0}", stringTemp.Substring(1, 4)), charTemp.ToString());
                    }
                    sBuilder.Append(stringTemp);
                }
            }
            return sBuilder.ToString();
        }
    }

    internal interface IAveJsonSerializable
    {
        // Methods
        string ToJson(AveJsonSerializer s);
    }

    public delegate string AveJsonEncoder(AveJsonSerializer s, object obj);
    public delegate string AveJsonGenerator(AveJsonSerializer s);
}
