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
using AvePoint.RA.Contract.CustomizeConnector.Enums;
using AvePoint.RA.Contract.CustomizeConnector.Model;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.CustomizeConnector
{
    public class ColumnInternalNameGenerator
    {

        private static readonly Dictionary<string, string> SymbolUnicode = new()
        {
            {"~", "%7E"},
            {"`", "%60"},
            {"!", "%21"},
            {"@", "%40"},
            {"#", "%23"},
            {"$", "%24"},
            {"%", "%25"},
            {"^", "%5E"},
            {"&", "%26"},
            {"*", "%2A"},
            {"(", "%28"},
            {")", "%29"},
            {"-", "%2D"},
            {"_", "%5F"},
            {"=", "%3D"},
            {"+", "%2B"},
            {"[", "%5B"},
            {"{", "%7B"},
            {"}", "%7D"},
            {"]", "%5D"},
            {"\\", "%5C"},
            {"|", "%7C"},
            {";", "%3B"},
            {":", "%3A"},
            {"\"", "%22"},
            {"\'", "%27"},
            {",", "%2C"},
            {"<", "%3C"},
            {".", "%2E"},
            {">", "%3E"},
            {"/", "%2F"},
            {"?", "%3F"},
            {" ", "%20"},
        };

        private readonly IRMCustomizeConnectorColumnDao CustomizeConnectorColumnDao = new RMCustomizeConnectorColumnDao();

        private readonly HashSet<string> InternalNames;

        private readonly HashSet<Guid> ExistColumns;

        public ColumnInternalNameGenerator()
        {
            var columnInfoes = CustomizeConnectorColumnDao.GetAll(CustomizeConnectorOrigin.BuildIn, CustomizeConnectorOrigin.ExternalCustomize).GetAwaiter().GetResult();
            InternalNames = columnInfoes.ToList().ConvertAll(item => item.InternalName).ToHashSet();
            ExistColumns = columnInfoes.ToList().ConvertAll(item => item.Id).ToHashSet();
        }

        public void Generate(List<CustomizeConnectorColumnInfo> columnInfoes)
        {
            foreach(var columnInfo in columnInfoes)
            {
                if(columnInfo.Origin == Contract.CustomizeConnector.Enums.CustomizeConnectorOrigin.BuildIn || ExistColumns.Contains(columnInfo.Id))
                {
                    continue;
                }

                var internalName  = UnicodeEncode(columnInfo.Name);
                var tempInternalName = internalName;
                for(var i = 1; InternalNames.Contains(tempInternalName); i++)
                {
                    tempInternalName = internalName + $"_{i}_";
                }

                columnInfo.InternalName = tempInternalName;
                InternalNames.Add(tempInternalName);
            }
        }

        private static string UnicodeEncode(string str)
        {
            var res = "";
            for (var i = 0; i < str.Length; i++)
            {
                var partRes = "_";
                var c = str.Substring(i, 1);
                var escapeChar = UnicodeEncodeByChar(c);
                if(escapeChar == c)
                {
                    int j;
                    for (j = i; j < str.Length; j++)
                    {
                        var enChar = str.Substring(j, 1);
                        var enEscapeChar = UnicodeEncodeByChar(enChar);
                        if(enChar == enEscapeChar)
                        {
                            partRes += enEscapeChar;
                            continue;
                        }
                        break;
                    }
                    i = j - 1;
                }
                else
                {
                    partRes += escapeChar;
                }

                res += partRes;
            }
            return res;
        }

        private static string UnicodeEncodeByChar(string c)
        {
            if(SymbolUnicode.TryGetValue(c, out var v))
            {
                return v;
            }

            var byteArr = Encoding.Unicode.GetBytes(c);
            if (byteArr[1] == 0)
            {
                return c;
            }

            var strBuilder = new StringBuilder();
            strBuilder.Append("x");
            strBuilder.Append(byteArr[1].ToString("X2"));
            strBuilder.Append(byteArr[0].ToString("X2"));
            return strBuilder.ToString();
        }
    }
}
