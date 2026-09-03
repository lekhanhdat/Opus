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
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.ObjectModel.Common
{
    public class AveFieldMultiColumnValue
    {
        private System.Collections.Generic.List<string> m_subColumnValues;
        /// <summary>Gets the delimiter that is used to separate "column" values.</summary>
        /// <returns>A string that contains the delimiter.</returns>
        public static string Delimiter
        {
            get
            {
                return ";#";
            }
        }

        public System.Collections.Generic.List<string> ColumnValues
        {
            get
            {
                return this.m_subColumnValues;
            }
        }
        /// <summary>Gets the number of "columns" that are represented in the multicolumn field.</summary>
        /// <returns>A 32-bit integer that indicates the number of "columns".</returns>
        public int Count
        {
            get
            {
                return this.m_subColumnValues.Count;
            }
        }
        /// <summary>Gets or sets the value of a "column" at the specified index.</summary>
        /// <param name="index">A 32-bit integer that specifies the index in relation to the "columns".</param>
        public string this[int index]
        {
            get
            {
                if (this.m_subColumnValues.Count == 0)
                {
                    return string.Empty;
                }
                return this.m_subColumnValues[index];
            }
            set
            {
                this.m_subColumnValues[index] = value;
            }
        }
        /// <summary>Initializes a new instance of the <see cref="T:Microsoft.SharePoint.SPFieldMultiColumnValue" /> class.</summary>
        public AveFieldMultiColumnValue()
        {
            this.m_subColumnValues = new System.Collections.Generic.List<string>();
        }
        /// <summary>Initializes a new instance of the <see cref="T:Microsoft.SharePoint.SPFieldMultiColumnValue" /> class based on the specified number of columns.</summary>
        /// <param name="numberOfSubColumns">A 32-bit integer that specifies the number of columns.</param>
        public AveFieldMultiColumnValue(int numberOfSubColumns)
        {
            this.m_subColumnValues = new System.Collections.Generic.List<string>(numberOfSubColumns);
            for (int i = 0; i < numberOfSubColumns; i++)
            {
                this.Add(string.Empty);
            }
        }
        /// <summary>Initializes a new instance of the <see cref="T:Microsoft.SharePoint.SPFieldMultiColumnValue" /> class based on the specified field value.</summary>
        /// <param name="fieldValue">A string that specifies the the number of "columns" and the value contained by each. The string consists of delimiter characters (#;) that separate the "column" values, for example, "MyFirstValue#; MySecondValue#;MyThirdValue".</param>
        public AveFieldMultiColumnValue(string fieldValue)
        {
            this.m_subColumnValues = AveFieldMultiColumnValue.ParseMultiColumnValue(fieldValue);
        }
        internal static System.Collections.Generic.List<string> ParseMultiColumnValue(string fieldValue)
        {
            return AveFieldMultiColumnValue.ParseMultiColumnValue(fieldValue, false);
        }
        
        internal static System.Collections.Generic.List<string> ParseMultiColumnValue(string fieldValue, bool bIncludeEmpty)
        {
            System.Collections.Generic.List<string> result = null;
            if (!AveFieldMultiColumnValue.TryParseMultiColumnValue(fieldValue, bIncludeEmpty, out result))
            {
                throw new System.ArgumentException();
            }
            return result;
        }
        internal static bool TryParseMultiColumnValue(string fieldValue, bool bIncludeEmpty, out System.Collections.Generic.List<string> subColumnValues)
        {
            subColumnValues = new System.Collections.Generic.List<string>();
            if (string.IsNullOrEmpty(fieldValue))
            {
                return true;
            }
            string text = Delimiter;
            if (text.Length != 2)
            {
                return false;
            }
            char c = text[0];
            char c2 = text[1];
            string oldValue = new string(c, 2);
            string newValue = new string(c, 1);
            int num = 0;
            if (fieldValue.StartsWith(text, System.StringComparison.Ordinal))
            {
                if (bIncludeEmpty)
                {
                    subColumnValues.Add(string.Empty);
                }
                num = text.Length;
            }
            int i = num;
            bool flag = false;
            while (i < fieldValue.Length)
            {
                if (fieldValue[i] == c)
                {
                    i++;
                    if (i >= fieldValue.Length)
                    {
                        break;
                    }
                    if (fieldValue[i] == c2)
                    {
                        if (i - 1 > num)
                        {
                            string text2 = fieldValue.Substring(num, i - num - 1);
                            if (flag)
                            {
                                text2 = text2.Replace(oldValue, newValue);
                            }
                            subColumnValues.Add(text2);
                            flag = false;
                        }
                        else
                        {
                            subColumnValues.Add(string.Empty);
                        }
                        i++;
                        num = i;
                    }
                    else
                    {
                        if (fieldValue[i] != c)
                        {
                            return false;
                        }
                        i++;
                        flag = true;
                    }
                }
                else
                {
                    i++;
                }
            }
            if (i > num)
            {
                string text3 = fieldValue.Substring(num, i - num);
                if (flag)
                {
                    text3 = text3.Replace(oldValue, newValue);
                }
                subColumnValues.Add(text3);
            }
            else
            {
                if (bIncludeEmpty)
                {
                    subColumnValues.Add(string.Empty);
                }
            }
            return true;
        }
        /// <summary>Adds a "column" to the multicolumn field.</summary>
        /// <param name="subColumnValue">A string that specifies the "column" value to add.</param>
        public void Add(string subColumnValue)
        {
            this.m_subColumnValues.Add(subColumnValue);
        }
        /// <summary>Returns a string that contains the "column" values separated by their delimiting characters.</summary>
        /// <returns>A string that contains the "column" values.</returns>
        public override string ToString()
        {
            return AveFieldMultiColumnValue.ConvertMultiColumnValueToString(this.m_subColumnValues, true);
        }
        internal static string ConvertMultiColumnValueToString(System.Collections.Generic.List<string> subColumnValues, bool bAddLeadingTailingDelimiter)
        {
            return AveFieldMultiColumnValue.ConvertMultiColumnValueToString(subColumnValues, bAddLeadingTailingDelimiter, false);
        }
        internal static string ConvertMultiColumnValueToString(System.Collections.Generic.List<string> subColumnValues, bool bAddLeadingTailingDelimiter, bool bPreserveEmpty)
        {
            bool flag = false;
            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder(255);
            for (int i = 0; i < subColumnValues.Count; i++)
            {
                string text = subColumnValues[i];
                if (!string.IsNullOrEmpty(text))
                {
                    text = text.Replace(";", ";;");
                }
                if (!string.IsNullOrEmpty(text))
                {
                    flag = true;
                }
                if (bAddLeadingTailingDelimiter || i != 0)
                {
                    stringBuilder.Append(";#");
                }
                stringBuilder.Append(text);
            }
            if (flag || bPreserveEmpty)
            {
                if (bAddLeadingTailingDelimiter)
                {
                    stringBuilder.Append(";#");
                }
                return stringBuilder.ToString();
            }
            return string.Empty;
        }
    }

}
