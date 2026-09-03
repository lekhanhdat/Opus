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
using System.ComponentModel;
using System.Data;

namespace Microsoft.Office.Project.Server.Schema
{
	// Token: 0x020007DD RID: 2013
	internal static class TypedDataSetUtilities
	{
		// Token: 0x0600C126 RID: 49446 RVA: 0x0025A58C File Offset: 0x0025878C
		public static void AllowNullsInNonTypedColumns(DataTable table, params string[] typedColumnNames)
		{
			HashSet<string> typedColumnsSet = new HashSet<string>(typedColumnNames);
			Action<DataColumn> allowNullInNonTypedColumns = delegate(DataColumn column)
			{
				if (!column.AllowDBNull && !typedColumnsSet.Contains(column.ColumnName))
				{
					column.AllowDBNull = true;
				}
			};
			table.ColumnChanged += delegate(object sender, DataColumnChangeEventArgs e)
			{
				allowNullInNonTypedColumns(e.Column);
			};
			table.ColumnChanging += delegate(object sender, DataColumnChangeEventArgs e)
			{
				allowNullInNonTypedColumns(e.Column);
			};
			table.Columns.CollectionChanged += delegate(object sender, CollectionChangeEventArgs e)
			{
				if (e.Action == CollectionChangeAction.Refresh)
				{
					foreach (object obj in table.Columns)
					{
						DataColumn obj2 = (DataColumn)obj;
						allowNullInNonTypedColumns(obj2);
					}
					return;
				}
				DataColumn dataColumn = e.Element as DataColumn;
				if (e.Action == CollectionChangeAction.Remove)
				{
					typedColumnsSet.Remove(dataColumn.ColumnName);
					return;
				}
				allowNullInNonTypedColumns(dataColumn);
			};
		}
	}
}
