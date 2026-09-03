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
using AvePoint.RA.Contract.FileSystemRegister.JPMC;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace AvePoint.RA.Service.Services.RMFileSystemSettings.JPMC
{
    public class FSConnectionRelatedJobFilterBuilder : FSBaseFilterBuilder<FSConnectionRelatedJobInfo, FSConnectionMonitorFilter>
    {
        protected override string GetColumnName(FSConnectionMonitorFilter filter) => filter.ColumnName;

        public FSConnectionRelatedJobFilterBuilder(List<FSConnectionMonitorFilter> filters) : base(filters) { }

        protected override Expression BuildExpression(FSConnectionMonitorFilter filter)
        {
            if(string.IsNullOrWhiteSpace(filter.ColumnName)) return null;
            if (filter?.ColumnValues == null || !filter.ColumnValues.Any()) return null;

            var col = filter.ColumnName;
            var vals = filter.ColumnValues;

            return col switch
            {
                nameof(FSConnectionRelatedJobInfo.JobId) => ContainsIgnoreCase(col, vals.FirstOrDefault()),
                nameof(FSConnectionRelatedJobInfo.ConnectionId) => Equals(col, new Guid(vals.FirstOrDefault())),
                nameof(FSConnectionRelatedJobInfo.FolderPath) => Equals(col, vals.FirstOrDefault()),
                nameof(FSConnectionRelatedJobInfo.JobRunBy) => In(col, vals.Select(value => value.Equals("System", StringComparison.OrdinalIgnoreCase) ? "RM_TS_RunSchedule" : value)),
                nameof(FSConnectionRelatedJobInfo.StartTime) or nameof(FSConnectionRelatedJobInfo.EndTime) => BuildTimeRangeExpression(col, vals),
                nameof(FSConnectionRelatedJobInfo.ConnectionPath) => BuildPathExpression(vals),
                _ => In(col, vals)
            };
        }

        private Expression BuildPathExpression(List<string> queryPaths)
        {
            if (queryPaths == null || queryPaths.Count ==0) return null;
            var connectionPathExpr = In(nameof(FSConnectionRelatedJobInfo.ConnectionPath), queryPaths);
            var folderPathExpr = In(nameof(FSConnectionRelatedJobInfo.FolderPath), queryPaths);
            return Expression.OrElse(connectionPathExpr, folderPathExpr);
        }
    }
}
