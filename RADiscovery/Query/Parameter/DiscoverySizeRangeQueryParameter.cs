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
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace RADiscovery.Query.Parameter
{
    public class DiscoverySizeRangeQueryParameter
    {
        [JsonProperty("queryMode")]
        public DiscoverySizeRangeQueryMode QueryMode { get; set; }

        [JsonProperty("sizeRange")]
        public int SizeRange { get; set; }

        public (bool has, DiscoverySqlDefinition sqlDefinition) TryGetSqlDefinition(string tableAlias)
        {
            var sql = QueryMode switch
            {
                DiscoverySizeRangeQueryMode.Range => "SizeRange = @SizeRange",
                DiscoverySizeRangeQueryMode.LessThanEqual => "SizeRange <= @SizeRange",
                DiscoverySizeRangeQueryMode.GenerateThanEqual => "SizeRange >= @SizeRange",
                _ => string.Empty,
            };

            if(string.IsNullOrWhiteSpace(sql))
            {
                return (false, new());
            }

            return (true, new DiscoverySqlDefinition
            {
                Sql = $"{tableAlias}.{sql}",
                Parameters = new()
                {
                    new SqlParameter("@SizeRange", SizeRange)
                }
            });
        }
    }

    public enum DiscoverySizeRangeQueryMode
    {
        None = 0,
        Range = 1,
        GenerateThanEqual = 2,
        LessThanEqual = 3
    }
}
