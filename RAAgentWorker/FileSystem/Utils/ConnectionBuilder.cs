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
using AvePoint.Media.Storage.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RAFileSystem.FileSystem.Utils
{
    public class ConnectionBuilder
    {
        public static readonly string SniaPrefix = "snia-xam://";

        public static readonly string DocAvePrefix = "docave-xam://";

        private static readonly string protocolPattern = "[(" + SniaPrefix + ")|(" + DocAvePrefix + ")]";

        private static readonly string vimPattern = "(?:([^/\\?]+)\\?)";

        private static readonly string conParamPattern = "([^=^&]+=[^=^&]*(?:\\&[^=^&]+\\=[^=^&]*)*)?";

        public static readonly string ConnectionPattern = protocolPattern + vimPattern + conParamPattern;

        private static readonly Regex connectionRegex = RegexHelper.Create(ConnectionPattern);

        private static readonly Regex paramsRegex = RegexHelper.Create("[\\&]{0,1}([^=^&]+)\\=([^=^&]*)");

        public string Protocal { get; set; } = DocAvePrefix;


        public string StorageName { get; set; }

        public Dictionary<string, string> Params { get; } = new Dictionary<string, string>();


        public string this[string key]
        {
            get
            {
                if (!Params.TryGetValue(key, out var value))
                {
                    return string.Empty;
                }

                return value;
            }
            set
            {
                Params[key] = value;
            }
        }

        public static ConnectionBuilder ValueOf(string connectionString)
        {
            Match match = connectionRegex.Match(connectionString, 0, connectionString.Length);
            if (!match.Success)
            {
                throw new InvalidXRIException(connectionString);
            }

            ConnectionBuilder connectionBuilder = new ConnectionBuilder
            {
                Protocal = (connectionString.StartsWith(DocAvePrefix, StringComparison.OrdinalIgnoreCase) ? DocAvePrefix : SniaPrefix),
                StorageName = match.Groups[1].Value
            };
            string value = match.Groups[2].Value;
            if (!string.IsNullOrEmpty(value) && paramsRegex.IsMatch(value))
            {
                match = paramsRegex.Match(value, 0, value.Length);
                while (match.Success)
                {
                    string key = match.Groups[1].Value.ToLower(CultureInfo.InvariantCulture);
                    string value2 = match.Groups[2].Value.Trim();
                    connectionBuilder.Params.Add(key, ValueDecode(value2));
                    match = match.NextMatch();
                }
            }

            return connectionBuilder;
        }

        public static string UNC2XRIString(string location, string username, string password)
        {
            return $"{DocAvePrefix}fs_vim?location={location}&name={username}&secret={password}";
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder(80);
            stringBuilder.Append(Protocal);
            stringBuilder.Append(StorageName);
            if (Params.Count > 0)
            {
                stringBuilder.Append('?');
            }

            foreach (KeyValuePair<string, string> param in Params)
            {
                if (!string.IsNullOrEmpty(param.Value))
                {
                    stringBuilder.Append(param.Key).Append('=').Append(ValueEncode(param.Value))
                        .Append('&');
                }
            }

            return stringBuilder.ToString().TrimEnd('&');
        }

        private static string ValueEncode(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            return value.Replace("%", "%25").Replace("&", "%26").Replace("=", "%3D")
                .Replace("^", "%5e");
        }

        private static string ValueDecode(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            return value.Replace("%3D", "=").Replace("%26", "&").Replace("%25", "%")
                .Replace("%5e", "^");
        }
    }
}
