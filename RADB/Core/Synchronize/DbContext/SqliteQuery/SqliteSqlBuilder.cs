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
using AvePoint.GCommon.Utility;
using AvePoint.RA.DB.Core.Synchronize.DbContext.TypeMapper;
using AvePoint.RA.DB.Core.Synchronize.DbManager;
using AvePoint.RA.DB.Model;

namespace AvePoint.RA.DB.Core.Synchronize.DbContext.SqliteQuery;

public abstract class SqliteSqlBuilder
{
    protected abstract PlaceHolder PlaceHolder { get; }
    
    public string Condition { get; set; }
    
    public Type Table { get; set; }
    
    private readonly List<SqliteSqlBuilder> _childNodes = [];
    
    
    protected string GetTableName()
    {
        var tableInfo = RMSynchronizeDbTableMapper.Get(Table);
        var schemaName = RMSynchronizeDbManager.GetSchemaName();

        return
            $"{SecurityUtils.SanitizeSQLSchemaName(schemaName)}${SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name)}";
    }
    
    public SqliteSqlBuilder SetConditions(string condition)
    {
        Condition = condition;
        return this;
    }
    
    
    public SqliteSqlBuilder Add(SqliteSqlBuilder child)
    {
        _childNodes.Add(child);
        return this;
    }
    
    public string BuildSql()
    {
        return _childNodes.GroupBy(childNode => childNode.PlaceHolder).OrderBy(group => group.Key)
            .Aggregate(Build(""), 
                (currentStringSql, nextGroupOfChild) => currentStringSql + nextGroupOfChild.Aggregate("",
                    (current, childNode) => current + childNode.Build(current)));
    }
    
    protected abstract string Build(string sqlString);
}

public enum PlaceHolder
{
    Select,
    Insert,
    Delete,
    Update,
    Where,
    OrderBy,
    LIMIT,
    OFFSET,
}