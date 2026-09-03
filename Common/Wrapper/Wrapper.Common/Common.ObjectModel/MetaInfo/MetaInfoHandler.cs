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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.Wrapper.Common
{
    public class MetaInfoHandler
    {
        // Fields
        public Dictionary<string, MetaInfoProperty> m_MetaCollection = new Dictionary<string, MetaInfoProperty>();

        // Methods
        public MetaInfoHandler()
        {
            this.m_MetaCollection = new Dictionary<string, MetaInfoProperty>();
        }

        public MetaInfoHandler(byte[] propertyArray)
        {
            this.Parse(propertyArray);
        }

        public void Parse(byte[] propertyBytes)
        {
            string propertyString = "";
            if (AveCompressedUtility.IsTCompressedBytes(propertyBytes))
            {
                propertyString = AveCompressedUtility.GetTCompressedString(propertyBytes);
            }
            else
            {
                propertyString = Encoding.UTF8.GetString(propertyBytes);
            }
            Parse(propertyString);
        }

        public MetaInfoHandler(string propertyArray)
            : this()
        {
            this.Parse(propertyArray);
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

        public void Parse(string propertyBytes)
        {
            char[] chars = propertyBytes.ToCharArray();
            MetaInfoProperty property = new MetaInfoProperty();
            int startIndex = 0;
            int num2 = 0;
            bool flag = false;
            bool flag2 = false;
            bool flag3 = false;
            int num3 = 0;
            for (int i = 0; i < chars.Length; i++)
            {
                if (((chars[i] == ':') && (i > 0)) && (chars[i - 1] != '\\'))
                {
                    if (!flag)
                    {
                        flag = true;
                        property = new MetaInfoProperty();
                        property.Name = new string(chars, num3, i - num3);
                        property.Type = ConvertCharToType(chars[i + 1]);
                        property.Access = ConvertCharToAccess(chars[i + 2]);
                    }
                }
                else if (chars[i] == '|')
                {
                    if (!flag2)
                    {
                        flag2 = true;
                        num3 = i + 1;
                    }
                }
                else if ((chars[i] == '\n') || (chars[i] == '\r'))
                {
                    flag3 = true;
                    num2 = i;
                    if (num3 < i)
                    {
                        property.Value = new string(chars, num3, i - num3);
                        if (!string.IsNullOrEmpty(property.Name))
                        {
                            this.m_MetaCollection.Add(property.Name, property);
                        }
                    }
                    flag = false;
                    flag2 = false;
                    num3 = i + 1;
                }
                else if (flag3)
                {
                    flag3 = false;
                    property.TheString = new string(chars, startIndex, (num2 - startIndex) + 1);
                    startIndex = num2 + 1;
                }
            }
            property.TheString = new string(chars, startIndex, chars.Length - startIndex);
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
