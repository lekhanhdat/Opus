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
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.MyHub.Model.FIlter.Types
{
    public class RMMyhubClassCodeFilter : IRMMyhubFilter
    {
        public string Name => "ClassCode";

        public RMMyhubSQLInfo GetSQL(string valueJson)
        {
            var value = Newtonsoft.Json.JsonConvert.DeserializeObject<RMMyhubDriveClassCodeFilterValue>(valueJson);
            if (value?.Option == null || value.Option.Length == 0)
            {
                return new RMMyhubSQLInfo();
            }

            var validOptions = value.Option
                .Where(option => !string.IsNullOrWhiteSpace(option))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (validOptions.Length == 0)
            {
                return new RMMyhubSQLInfo();
            }

            return BuildClassCodeSqlInfo(validOptions);
        }

        private static RMMyhubSQLInfo BuildClassCodeSqlInfo(string[] options)
        {
            var conditions = new List<string>();
            var parameters = new List<SqlParameter>();

            for (int i = 0; i < options.Length; i++)
            {
                string parameterName = $"@classCode{i}";
                conditions.Add($"c.classCode = {parameterName}");
                parameters.Add(new SqlParameter(parameterName, options[i]));
            }

            return new RMMyhubSQLInfo
            {
                SQL = $" AND ({string.Join(" OR ", conditions)}) ",
                SQLParameters = parameters.ToArray()
            };
        }

        public RMMyhubPageResult LoadAvaliableValues(RMMyhubPageInfo pageInfo)
        {
            throw new NotImplementedException();
        }

        public class RMMyhubDriveClassCodeFilterValue
        {
            public string[] Option { get; set; }
        }
    }
}
