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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AvePoint.RA.DB.Core.Synchronize.DbContext.Base;
using AvePoint.RA.DB.Core.Synchronize.DbContext.SqliteQuery;
using AvePoint.RA.DB.Core.Synchronize.DbContext.Utility.RecordQuery;
using AvePoint.RA.DB.Core.Synchronize.DbManager;
using AvePoint.RA.DB.Dao.SynchronizeDao;
using AvePoint.RA.DB.Model;

namespace AvePoint.RA.DB.Dao.SynchronizeRemoteNodeDao.Imp;

public class KeyValueSqliteDao : IKeyValueSqliteDao
{
    private static ISynchronizeDbContext GetDbContext() => RMSynchronizeDbManager.GetContext();
    
    public async Task<bool> UpsertAsync(string key, string value)
    {
        RMKeyValue keyValue = new()
        {
            Key = key,
            Value = value
        };
        await using var context = GetDbContext();
        var sql = new RecordQuery
        {
            PlaceHolder = PlaceHolder.Delete,
            Table = typeof(RMKeyValue),
            Filters =
            [
                new QueryFilter
                {
                    ColumnName = nameof(RMKeyValue.Key),
                    Operator = Operator.Equal,
                    Value = $"'{key}'"
                }
            ],
        };
        await context.ExecuteNonQueryAsync(sql.BuildSqlString());
        var result =  await context.ExecuteInsertAsync([keyValue]);
        return result > 0;
    }

    public async Task<string> GetValueByKeyAsync(string key)
    {
        await using var context = GetDbContext();
        var sql = new RecordQuery
        {
            PlaceHolder = PlaceHolder.Select,
            Table = typeof(RMKeyValue),
            Filters =
            [
                new QueryFilter
                {
                    ColumnName = nameof(RMKeyValue.Key),
                    Operator = Operator.Equal,
                    Value = $"'{key}'"
                },
            ]
        };

        var result = await context.ExecuteQueryAsync<RMKeyValue>(sql.BuildSqlString()).ToListAsync();
        return result.FirstOrDefault()?.Value;
    }
}