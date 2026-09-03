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
using System.Text;
using System.Threading.Tasks;

namespace RADiscovery.Query.Parameter
{
    public class DiscoveryWithoutDateQueryParameter
    {
        [JsonProperty("from")]
        public int From { get; set; }

        [JsonProperty("to")]
        public int To { get; set; }

        public (bool has, DiscoverySqlDefinition sqlDefinition) TryGetSqlDefinition(string tableAlias)
        {
            if (From <= -1 && To >= 999)
            {
                return (false, new());
            }

            if (From == -1)
            {
                return (true, new()
                {
                    Sql = $"{tableAlias}.WithoutInDate <= @To",
                    Parameters = new()
                    {
                        new SqlParameter("@To", To)
                    }
                });
            }

            if (To == 999)
            {
                return (true, new()
                {
                    Sql = $"{tableAlias}.WithoutInDate > @From",
                    Parameters = new()
                    {
                        new SqlParameter("@From", From)
                    }
                });
            }

            return (true, new()
            {
                Sql = $"{tableAlias}.WithoutInDate > @From AND {tableAlias}.WithoutInDate <= @To",
                Parameters = new()
                {
                    new SqlParameter("@From", From),
                    new SqlParameter("@To", To)
                }
            });
        }
    }
}
