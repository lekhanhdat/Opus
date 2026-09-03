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
using Cloud.sdk.Data.Opus.GoogleOne.Common;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.MyHub.Model.FIlter.Types
{
    public class RMMyhubLastSyncTimeFilter : IRMMyhubFilter
    {
        public string Name => "LastSyncTime";

        public RMMyhubSQLInfo GetSQL(string valueJson)
        {
            return new();
        }
        //传入分页信息，返回结果和总数
        public RMMyhubPageResult LoadAvaliableValues(RMMyhubPageInfo pageInfo)
        {
            throw new NotImplementedException();
        }
    }

    public enum RMMyhubDriveLastSyncTimeFilterOption
    {
        AnyTime = 1,
        WithIn = 2,
        Between = 3
    }
    public enum RMMyhubDriveLastSyncTimeWithinFilter
    {
        Days = 1,
        Weeks = 2,
        Months = 3,
        Years = 4
    }
    public class RMMyhubDriveLastSyncTimeFilterValue
    {
        public RMMyhubDriveLastSyncTimeFilterOption Option { get; set; }
        public RMMyhubDriveLastSyncTimeWithinFilter WithinOption { get; set; }
        public int WithinNumber { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}

