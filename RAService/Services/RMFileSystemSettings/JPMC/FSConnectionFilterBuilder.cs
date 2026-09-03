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
using AvePoint.RA.Contract.FileSystemRegister;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace AvePoint.RA.Service.Services.RMFileSystemSettings.JPMC
{
    public class FSConnectionFilterBuilder : FSBaseFilterBuilder<FSConnection, FSConnectionFilter>
    {
        public FSConnectionFilterBuilder(List<FSConnectionFilter> filters) : base(filters) { }

        protected override string GetColumnName(FSConnectionFilter filter) => filter.ColumnName;

        protected override Expression BuildExpression(FSConnectionFilter filter)
        {
            if (string.IsNullOrWhiteSpace(filter.ColumnName)) return null;
            if (filter.ColumnValues == null || !filter.ColumnValues.Any()) return null;

            var col = filter.ColumnName;
            var values = filter.ColumnValues;

            return col switch
            {
                nameof(FSConnection.Name) => ContainsIgnoreCase(col, values.First()),
                nameof(FSConnection.GroupId) => Equals(col, new Guid(values.First())),
                nameof(FSConnection.LastModifiedTime) or nameof (FSConnection.LastSyncTime) => BuildTimeRangeExpression(col, values),
                _ => null
            };
        }
    }
}
