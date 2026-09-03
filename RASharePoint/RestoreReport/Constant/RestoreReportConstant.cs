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
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.RestoreReport.Constant
{
    public class RestoreReportConstant
    {
        #region  sc rpt
        public static string SC_TABLE_NAME = "restoreDetail";

        public static string SC_TABLE_FOLDER = "sc detail";

        public static string INSERT_DATA_INTO_SC_TABLE_SQL = "Insert into {0} (Level,Name,SourceURL,Size,JobId,StartTime,FinishTime,RestoreBy,RestoreTo,IsDaoMigration,IsEndUserOpt,Status,Comment) Values (@Level,@Name,@SourceURL,@Size,@JobId,@StartTime,@FinishTime,@RestoreBy,@RestoreTo,@IsDaoMigration,@IsEndUserOpt,@Status,@Comment)";

        public static string GET_DATA_FROM_SC_TABLE_SQL = @"select * from {0} limit {1} offset {2}";

        public static string GET_DATA_FROM_SC_TABLE_ON_CONDITION_SQL = @"select * from {0} where {1} limit {2} offset {3}";

        public const string GET_COUNT_FROM_SC_TABLE_SQL = "select count(*) from {0}";

        public const string GET_COUNT_FROM_SC_TABLE_ON_CONDITION_SQL = "select count(*) from {0} where {1}";

        public static string DELETE_DATA_FROM_SC_TABLE_SQL = @"Delete from {0} where {1};";

        public static string CREATE_SC_TABLE_SQL = @"Create table {0} (ID integer primary key autoincrement,Level nvarchar (500),Name nvarchar (500),SourceURL nvarchar (500),Size integer,JobId nvarchar (500),StartTime integer,FinishTime integer,RestoreBy nvarchar (500),RestoreTo nvarchar (500),IsDaoMigration nvarchar (500),ISEndUserOpt nvarchar (500),Status nvarchar (500),Comment nvarchar (500))";

        #endregion

    }
}
