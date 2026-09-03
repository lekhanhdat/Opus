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

namespace RACommon.SQLiteDatabase;

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;

public interface ISQLiteDatabaseHelper
{
    void ExecuteNonQuery(String commandText, Dictionary<String, Object> parameters);

    void ExecuteNonQueryWithCommandPrepare<T>(List<T> entities) where T : IInsertable;

    void ExecuteNonQuery<T>(List<T> entities) where T : IInsertable;

    void BatchExecuteNonQuery(Dictionary<String, Dictionary<String, Object>> commandInfoList);

    DataTable QueryDataTable(String commandText, Dictionary<String, Object> parameters);

    List<T> Query<T>(String commandText, Dictionary<String, Object> parameters) where T : IInsertable;

    IEnumerable<T> QueryEnumerable<T>(String commandText, Dictionary<String, Object> parameters) where T : IInsertable;

    DbDataReader GetExecuteReader(String commandText, Dictionary<String, Object> parameters);

    List<T> QueryForAllClass<T>(String commandText, Dictionary<String, Object> parameters) where T : class;

    Object ExecuteScalar(String commandText, Dictionary<String, Object> parameters = null!);

    IEnumerable<T> QuerySingleFieldEnumerable<T>(String commandText, Dictionary<String, Object> parameters = null!) where T : class;

    List<T> QuerySingleField<T>(String commandText, Dictionary<String, Object> parameters = null!) where T : class;

    public Boolean IsOpen { get; }

    void Open(String connectionString);

    void Close();
}
