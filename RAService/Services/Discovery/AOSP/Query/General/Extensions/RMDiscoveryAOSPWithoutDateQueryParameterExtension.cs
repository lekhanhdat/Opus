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
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.Contract.Discovery.Model.Query.AOSP.Parameter;
using AvePoint.RA.Contract.Discovery.Model.Query.Office365.Parameter;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.AOSP.Query.General.Extensions
{
    public static class RMDiscoveryAOSPWithoutDateQueryParameterExtension
    {
        public static bool TryGetSqlDefinition(this RMDiscoveryAOSPWithoutDateQueryParameter parameter, string dataTableAlias, out RMDiscoverySqlDefinition sqlDefinition)
        {
            sqlDefinition = null;
            if (parameter.From < -1)
            {
                return false;
            }

            if (parameter.From == -1)
            {
                sqlDefinition = new()
                {
                    ConditionSql = $"{dataTableAlias}.WithoutInDate <= @To",
                    Parameters = new()
                    {
                        new SqlParameter("@To", parameter.To)
                    }
                };
                return true;
            }

            sqlDefinition = new()
            {
                ConditionSql = $"{dataTableAlias}.WithoutInDate > @From AND {dataTableAlias}.WithoutInDate <= @To",
                Parameters = new()
                {
                    new SqlParameter("@From", parameter.From),
                    new SqlParameter("@To", parameter.To)
                }
            };

            return true;
        }
        public static bool TryGetSFSqlDefinition(this RMDiscoveryAOSPWithoutDateQueryParameter parameter, string dataTableAlias, out RMDiscoverySqlDefinition sqlDefinition)
        {
            sqlDefinition = null;
            if (parameter.From <= -1 && parameter.To >= 999)
            {
                return false;
            }

            if (parameter.From == -1)
            {
                sqlDefinition = new()
                {
                    ConditionSql = $"{dataTableAlias}.ModifiedDateRange < @To",
                    Parameters = new()
                    {
                        new SqlParameter("@To", parameter.To)
                    }
                };
                return true;
            }

            if (parameter.To == 999)
            {
                sqlDefinition = new()
                {
                    ConditionSql = $"{dataTableAlias}.ModifiedDateRange >= @From",
                    Parameters = new()
                    {
                        new SqlParameter("@From", parameter.From)
                    }
                };
                return true;
            }

            sqlDefinition = new()
            {
                ConditionSql = $"{dataTableAlias}.ModifiedDateRange >= @From AND {dataTableAlias}.ModifiedDateRange < @To",
                Parameters = new()
                {
                    new SqlParameter("@From", parameter.From),
                    new SqlParameter("@To", parameter.To)
                }
            };

            return true;
        }
    }
}
