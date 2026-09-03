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
using AvePoint.RA.Contract.Audit.JPMC;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Service.Services.RMFileSystemSettings;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace AvePoint.RA.Service.Services.Audit.JPMC
{
    public class FSAuditFilterBuilder : FSBaseFilterBuilder<RMFSAudit, FSAuditQueryFilter>
    {
        public FSAuditFilterBuilder(List<FSAuditQueryFilter> filters) : base(filters) { }

        protected override Expression BuildExpression(FSAuditQueryFilter filter)
        {
            if (string.IsNullOrWhiteSpace(filter.ColumnName)) return null;
            if (filter.ColumnValues == null || filter.ColumnValues.Count == 0) return null;

            var col = GetColumnName(filter);
            var values = filter.ColumnValues;

            return col switch
            {
                nameof(RMFSAudit.ObjectName) => ContainsIgnoreCase(col, values[0]),
                nameof(RMFSAudit.ExecutedTime) => BuildTimeRangeExpression(col, values,filter.MyhubTimeZoneId),
                nameof(RMFSAudit.AuditLevel) => BuildAuditLevelExpression(values[0]),
                nameof(RMFSAudit.FullPath) => null, //Handled in BuildAuditLevelExpression for folder level 
                _ => In(col, values)
            };
        }

        protected override string GetColumnName(FSAuditQueryFilter filter) => filter.ColumnName;

        private Expression BuildAuditLevelExpression(string auditLevelStr)
        {
            if (string.IsNullOrWhiteSpace(auditLevelStr) || !int.TryParse(auditLevelStr, out var auditLevel))
                return null;

            var auditLevelExpr = Equals(nameof(RMFSAudit.AuditLevel), auditLevel);
            if (auditLevel != (int)FSAuditLevel.Folder) return auditLevelExpr;
            return BuildFolderLevelExpression(auditLevelExpr);
        }

        private Expression BuildFolderLevelExpression(Expression auditLevelExpression)
        {
            var targetPath = base._filters.FirstOrDefault(f => f.ColumnName == nameof(RMFSAudit.FullPath))?.ColumnValues?.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(targetPath)) return auditLevelExpression;

            var currentPathExpression = Equals(nameof(RMFSAudit.FullPath), targetPath);
            var previousPathExpression = Equals(nameof(RMFSAudit.PreviousPath), targetPath);
            var moveFileAuditTypeExpression = Equals(nameof(RMFSAudit.AuditType), (int)FSAuditType.MoveFile);

            var folderAuditWithCurrentPathExpression = Expression.AndAlso(auditLevelExpression, currentPathExpression);
            var movedFilePathMatchExpression = Expression.OrElse(currentPathExpression, previousPathExpression);
            var moveFileExpression = Expression.AndAlso(movedFilePathMatchExpression, moveFileAuditTypeExpression);

            return Expression.OrElse(folderAuditWithCurrentPathExpression, moveFileExpression);
        }
    }
}