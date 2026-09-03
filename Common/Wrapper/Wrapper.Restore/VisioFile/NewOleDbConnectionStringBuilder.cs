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

namespace AvePoint.Wrapper.Restore
{
    public class NewOleDbConnectionStringBuilder
    {
        Dictionary<string, string> connectionKeyValuePairs = new Dictionary<string, string>();

        private NewOleDbConnectionStringBuilder(Dictionary<string, string> connectionKeyValuePairs)
        {
            this.connectionKeyValuePairs = connectionKeyValuePairs;
        }

        public static NewOleDbConnectionStringBuilder Parse(string connectionStr)
        {
            Dictionary<string, string> connectionKeyValuePairs = new Dictionary<string, string>();
            int index = 0;
            while (index < connectionStr.Length)
            {
                string key = ReadKey(connectionStr, ref index);
                string value = ReadValue(connectionStr, ref index);
                connectionKeyValuePairs[key] = value;
            }
            return new NewOleDbConnectionStringBuilder(connectionKeyValuePairs);
        }

        public void Set(string key, string value)
        {
            if (connectionKeyValuePairs.ContainsKey(key))
            {
                connectionKeyValuePairs[key] = value;
            }
        }

        public string Get(string key)
        {
            if (connectionKeyValuePairs.ContainsKey(key))
            {
                return connectionKeyValuePairs[key];
            }
            else
            {
                return null;
            }
        }

        public override string ToString()
        {
            StringBuilder finalConnectionStr = new StringBuilder();
            foreach (KeyValuePair<string, string> connectionStr in connectionKeyValuePairs)
            {
                finalConnectionStr.Append(connectionStr.Key);
                finalConnectionStr.Append("=");
                finalConnectionStr.Append(connectionStr.Value);
                finalConnectionStr.Append(";");
            }
            return finalConnectionStr.ToString();
        }

        public static string ReadKey(string connectionStr, ref int index)
        {
            int keyStartIndex = index;
            SeekToChar(connectionStr, '=', ref index);

            if (index < connectionStr.Length)
            {
                return connectionStr.Substring(keyStartIndex, index - keyStartIndex - 1);
            }
            else
            {
                return null;
            }
        }

        public static string ReadValue(string connectionStr, ref int index)
        {
            int valueStartIndex = index;
            if (connectionStr[index] == '"')
            {
                ReadQuotation(connectionStr, ref index);
            }
            else
            {
                ReadSemicolon(connectionStr, ref index);
            }

            if (index < connectionStr.Length)
            {
                return connectionStr.Substring(valueStartIndex, index - valueStartIndex - 1);
            }
            else if (index == connectionStr.Length)
            {
                return connectionStr.Substring(valueStartIndex, index - valueStartIndex);
            }
            else
            {
                return null;
            }
        }

        public static void ReadQuotation(string connectionStr, ref int index)
        {         
            SeekToChar(connectionStr, '"', ref index);
            SeekToChar(connectionStr, '"', ref index);
            ReadSemicolon(connectionStr, ref index);
        }       

        public static void ReadSemicolon(string connectionStr, ref int index)
        {
            SeekToChar(connectionStr, ';', ref index);
        }

        private static void SeekToChar(string str, char c, ref int index)
        {
            while (index < str.Length && str[index++] != c) { };
        }
    }
}
