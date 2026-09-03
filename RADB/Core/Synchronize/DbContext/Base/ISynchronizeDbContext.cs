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
using System.Data.SQLite;
using System.Threading.Tasks;
using AvePoint.RA.DB.Model;
using System.Data.SQLite;
using AvePoint.RA.DB.Core.Synchronize.DbContext.TypeMapper;

namespace AvePoint.RA.DB.Core.Synchronize.DbContext.Base;

public interface ISynchronizeDbContext : IAsyncDisposable
{
    Task OpenAsync(string connectionString);
    
    Task<T> ExecuteScalarAsync<T>(string sql, params SQLiteParameter[] parameters);
    
    Task<int> ExecuteNonQueryAsync(string sql, params SQLiteParameter[] parameters);
    
    IAsyncEnumerable<T> ExecuteQueryAsync<T>(string sql, params SQLiteParameter[] parameters);
    
    Task<int> ExecuteInsertAsync<T>(IEnumerable<T> item);
    
    List<ISynchronizeDbTableSet> GetTableSets(string schemaName);

    void CreateDatabase(string dbPath);

    Task CloseAsync();
}