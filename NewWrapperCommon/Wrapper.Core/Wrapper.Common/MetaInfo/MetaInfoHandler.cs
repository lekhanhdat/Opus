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

using AvePoint.GCommon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web;
using System.Globalization;

namespace AvePoint.Wrapper.Common
{
    public class MetaInfoHandler
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        // Fields
        private Dictionary<string, MetaInfoProperty> m_MetaCollection = new Dictionary<string, MetaInfoProperty>();
        private const char SpecifyChar = (Char)0x13;

        public MetaInfoHandler()
        {

        }
        public MetaInfoHandler(string propertyArray)
        {
            this.Parse(propertyArray);
        }

        public MetaInfoHandler(byte[] propertyArray)
        {
            this.Parse(propertyArray);
        }

        public MetaInfoHandler(string htmlProperty, bool isHTML)
        {
            if (isHTML)
            {
                this.ParseHtml(htmlProperty);
            }
            else
            {
                this.Parse(htmlProperty);
            }
        }

        /// <summary>
        /// htmlProperty的格式如: 
        /// name: vti_timecreated 
        /// value: TR|04 Jun 2015 06:50:10 -0000 (其中value[0]为type，value[1]为Access)
        /// </summary>
        /// <param name="htmlProperty"></param>
        public void ParseHtml(string htmlProperty)
        {
            using (StringReader reader = new StringReader(htmlProperty))
            {
                try
                {
                    string name = reader.ReadLine();
                    string value = reader.ReadLine();
                    while (name != null && value != null)
                    {
                        if (value.Length >= 3 && value.IndexOf('|') >= 2)
                        {
                            //value长度至少为3，否者type或者Access赋值不对;‘|’索引大于等于2保证type and Access有值
                            var property = new MetaInfoProperty();
                            property.Name = name;
                            property.Type = ConvertCharToType(value[0]);
                            property.Access = ConvertCharToAccess(value[1]);
                            property.Value = HttpUtility.HtmlDecode(value.Split('|')[1]);
                            this.m_MetaCollection[property.Name] = property;
                        }
                        name = reader.ReadLine();
                        value = reader.ReadLine();
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, "An error occurred while Parse Html {0}. error:{1}", htmlProperty, e);
                    throw;
                }
            }
        }

        public void Add(MetaInfoProperty property)
        {
            if ((property != null) && !string.IsNullOrEmpty(property.Name))
            {
                this.m_MetaCollection.Add(property.Name, property);
            }
        }

        public bool Contains(string key)
        {
            return this.m_MetaCollection.ContainsKey(key);
        }

        internal static char ConvertAccessToChar(MetaInfoAccess a)
        {
            switch (a)
            {
                case MetaInfoAccess.ReadOnly:
                    return 'R';

                case MetaInfoAccess.NoAccess:
                    return 'X';
            }
            return 'W';
        }

        internal static MetaInfoAccess ConvertCharToAccess(char c)
        {
            switch (c)
            {
                case 'X':
                    return MetaInfoAccess.NoAccess;

                case 'R':
                    return MetaInfoAccess.ReadOnly;
            }
            return MetaInfoAccess.ReadWrite;
        }

        internal static MetaInfoValueType ConvertCharToType(char c)
        {
            switch (c)
            {
                case 'B':
                    return MetaInfoValueType.Boolean;

                case 'D':
                    return MetaInfoValueType.Double;

                case 'E':
                    return MetaInfoValueType.Empty;

                case 'F':
                    return MetaInfoValueType.FileSystemTime;

                case 'I':
                    return MetaInfoValueType.Integer;

                case 'L':
                    return MetaInfoValueType.LongText;

                case 'T':
                    return MetaInfoValueType.Time;

                case 'U':
                    return MetaInfoValueType.IntegerVector;

                case 'V':
                    return MetaInfoValueType.StringVector;
            }
            return MetaInfoValueType.String;
        }

        internal static MetaInfoValueType ConvertObjectTypeToMataInfoType(object value)
        {
            if (((value is short) || (value is int)) || (value is long))
            {
                return MetaInfoValueType.Integer;
            }
            if (value is DateTime)
            {
                return MetaInfoValueType.Time;
            }
            if (value is double)
            {
                return MetaInfoValueType.Double;
            }
            if (value is bool)
            {
                return MetaInfoValueType.Boolean;
            }
            return MetaInfoValueType.String;
        }

        internal static char ConvertTypeToChar(MetaInfoValueType a)
        {
            switch (a)
            {
                case MetaInfoValueType.Integer:
                    return 'I';

                case MetaInfoValueType.Time:
                    return 'T';

                case MetaInfoValueType.StringVector:
                    return 'V';

                case MetaInfoValueType.Boolean:
                    return 'B';

                case MetaInfoValueType.FileSystemTime:
                    return 'F';

                case MetaInfoValueType.IntegerVector:
                    return 'U';

                case MetaInfoValueType.Double:
                    return 'D';

                case MetaInfoValueType.LongText:
                    return 'L';

                case MetaInfoValueType.Empty:
                    return 'E';
            }
            return 'S';
        }

        public MetaInfoProperty GetObject(string key)
        {
            return this[key];
        }

        public void Parse(string propertyString)
        {
            string[] mSplitedString = propertyString.Replace("\r\n", SpecifyChar.ToString()).Split(new char[] { SpecifyChar });
            ParseCore(mSplitedString);
        }

        public void Parse(byte[] propertyBytes)
        {
            string propertyString = AveCompressedUtility.GetTCompressedString(propertyBytes);
            Parse(propertyString);
        }

        private void ParseCore(string[] mSplitedString)
        {
            for (int i = 0; i < mSplitedString.Length; i++)
            {
                var mStr = mSplitedString[i];
                try
                {
                    MetaInfoProperty property;
                    if (TryConvertToMetaInfoProperty(mStr, out property))
                    {
                        this.m_MetaCollection.Add(property.Name, property);
                    }
                }
                catch (Exception e)
                {
                    log.Warn("Get Value Error:{0}. Reason:{1}.", !String.IsNullOrEmpty(mStr) ? mStr : "mStr is Empty", e);
                    continue;
                }
            }
        }

        internal static bool TryConvertToMetaInfoProperty(string str, out MetaInfoProperty property)
        {
            //仿照数据库读取MetadataInfo,解析key value.数据库中存储，会在key中':'前和'\'前自动加一个'\'
            //Index\:CategoriesLookup|Author:SW|0a2e7d02-abe1-4b45-900a-b6678ac4f5cd
            //how\:are\\\:you\\:SW|ABC
            int index1 = 0;
            while ((index1 = str.IndexOf(':', index1)) > 0)
            {             
                var preStr = str.Substring(0, index1);
                var afterStr = str.Substring(index1);
                //支持key中包含多个':'和'\'的解析，记录每个'：'前面‘\’的个数。
                int charCount = preStr.Length - preStr.TrimEnd('\\').Length;
                if (!Convert.ToBoolean(charCount % 2))
                {
                    //如果为偶数个，该':'即为key value分隔符，break
                    str = preStr.Substring(0, preStr.Length - charCount / 2) + afterStr; //截取掉数据库中存储多加的'\'
                    index1 -= charCount / 2;
                    break;
                }
                str = preStr.Substring(0, preStr.Length - (charCount + 1) / 2) + afterStr; //截取掉数据库中存储多加的'\'
                index1 -= (charCount - 1) / 2;
            }
            if (index1 <= 0)
            {
                property = null;
                return false;
            }
            property = new MetaInfoProperty();
            var index2 = str.IndexOf('|', index1);
            property.Name = str.Substring(0, index1);
            string typeStr = index2 > 0 ? str.Substring(index1 + 1, 1).ToUpper(CultureInfo.InvariantCulture) : String.Empty;
            property.Type = ConvertCharToType(Convert.ToChar(typeStr));
            string accessStr = index2 > 0 ? str.Substring(index1 + 2, 1).ToUpper(CultureInfo.InvariantCulture) : String.Empty;
            property.Access = ConvertCharToAccess(Convert.ToChar(accessStr));
            string value = index2 > 0 ? str.Substring(index2 + 1) : str.Substring(index1 + 1);
            value = value.Replace(@"\\", @"\");//经过测试，value中如果有反斜线，会在前面加上一个反斜线。 而冒号前面没有。
            property.Value = ConvertMetaInfoValueType(property.Type, value);
            property.TheString = str;

            return true;
        }

        internal static object ConvertMetaInfoValueType(MetaInfoValueType type, string value)
        {
            Object obj = null;
            switch (type)
            {
                case MetaInfoValueType.Boolean:
                    bool boolValue;
                    if (Boolean.TryParse(value, out boolValue))
                    {
                        obj = boolValue;
                    }
                    break;

                case MetaInfoValueType.Integer:
                    int intValue;
                    if (Int32.TryParse(value, out intValue))
                    {
                        obj = intValue;
                    }
                    break;

                case MetaInfoValueType.Double:
                    double doubleValue;
                    if (Double.TryParse(value, out doubleValue))
                    {
                        obj = doubleValue;
                    }
                    break;

                case MetaInfoValueType.Time:
                case MetaInfoValueType.FileSystemTime:
                    DateTime timeValue;
                    if (DateTime.TryParse(value, out timeValue))
                    {
                        obj = timeValue;
                    }
                    break;

                case MetaInfoValueType.Empty:
                    obj = String.Empty;
                    break;

                default:
                    obj = value;
                    break;
            }
            return obj;
        }

        public void Remove(string key)
        {
            if (this.Contains(key))
            {
                this.m_MetaCollection.Remove(key);
            }
        }

        public void ScrubClean()
        {
            this.Remove("vti_author");
            this.Remove("vti_modifiedby");
        }

        public Dictionary<string, string> ToStringDictionary()
        {
            Dictionary<string, string> hashtable = new Dictionary<string, string>(this.m_MetaCollection.Count);
            foreach (KeyValuePair<string, MetaInfoProperty> pair in this.m_MetaCollection)
            {
                hashtable.Add(pair.Key, pair.Value.Value.ToString());
            }
            return hashtable;
        }

        public Hashtable ToDictionary()
        {
            Hashtable hashtable = new Hashtable(this.m_MetaCollection.Count);
            foreach (KeyValuePair<string, MetaInfoProperty> pair in this.m_MetaCollection)
            {
                hashtable.Add(pair.Key, pair.Value);
            }
            return hashtable;
        }

        public Hashtable ToHashtable()
        {
            Hashtable hashtable = new Hashtable(this.m_MetaCollection.Count);
            foreach (KeyValuePair<string, MetaInfoProperty> pair in this.m_MetaCollection)
            {
                hashtable.Add(pair.Key, pair.Value.Value);
            }
            return hashtable;
        }

        public override string ToString()
        {
            if (this.m_MetaCollection.Count == 0)
            {
                return string.Empty;
            }
            StringBuilder builder = new StringBuilder();
            foreach (KeyValuePair<string, MetaInfoProperty> pair in this.m_MetaCollection)
            {
                builder.Append(pair.Value.TheString);
            }
            return builder.ToString();
        }

        public string ToUpdateString()
        {
            if (this.m_MetaCollection.Count == 0)
            {
                return string.Empty;
            }
            StringBuilder builder = new StringBuilder();
            foreach (KeyValuePair<string, MetaInfoProperty> pair in this.m_MetaCollection)
            {
                builder.Append(pair.Value.TheUpdateString);
            }
            return builder.ToString();
        }

        // Properties
        public MetaInfoProperty this[string key]
        {
            get
            {
                if (!this.Contains(key))
                {
                    return null;
                }
                return this.m_MetaCollection[key];
            }
        }

        internal Dictionary<string, MetaInfoProperty> MetaCollection
        {
            get
            {
                return this.m_MetaCollection;
            }
        }
    }
}
