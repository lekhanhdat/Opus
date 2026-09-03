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
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using System;
using System.Collections.Generic;
using System.Text;

namespace AvePoint.RA.Contract.RMWeb.JobMonitor
{
    public class RestoreFileReport : BaseReport
    {
        public long Size { get; set; }
        public string RestoreBy { get; set; }
        public string JobId { get; set; }
        public long StartTime { get; set; }
        public long EndTime { get; set; }
        public string StartTimeString { get; set; }
        public string EndTimeString { get; set; }
        public string SizeString { get; set; }
        public string RestoreTo { get; set; }
        public int IsDaoMigration { get; set; }
        public int IsEndUserOpt { get; set; }
        public JobDetailsStatus Status { get; set; }
        public string Comment { get; set; }
    }
}
