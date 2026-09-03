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

using AvePoint.GCommon.Contract.Server.Job.Object;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.FileSystem.Core
{
    public abstract class ReportEntry
    {
      
        public enum ActionType
        {
            CreateHoldTypeWithRecord = 2304,

            ReuseHoldTypeWithRecord = 2305,

            CancelHoldByRecords = 2306,

            SusPendRecords = 2307,

            CancelHold = 2308,

            SuspendHold = 2309,

            DeleteHold = 2310,

            CreateHold = 2312,

            ChangeHoldCreate = 2313,

            ChangeHoldReuse = 2314,
        }
        public string Name { get; set; }
        public string Type { get; set; }
        public DateTime FinishTime { get; set; }
        public string Url { get; set; }
        public string Comment { get; set; }
        public int Status { get; set; }
        public ActionType Action { get; set; }

        public abstract FSJobDetail ToJobDetail();

    }
}
