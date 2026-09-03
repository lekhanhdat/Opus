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
using AvePoint.RA.Contract.JobMonitor;
using System.Collections.Generic;

namespace AvePoint.RA.Contract.RMWeb.JobMonitor
{
    public class JMJobSummary
    {
        public JobType JobType { get; set; }
        public string JobId { get; set; }
        public string ProfileName{ get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string JobRunBy { get; set; }
        public JobStatus Status { get; set; }
        public string Scope { get; set; }
        public string Comment { get; set; }
        public RMJobSummaryInfos DisposalSummary { get; set; }
        public string ProgressSCStr { get; set; }
        public string ProgressFileCountStr { get; set; }
        public bool IsNewJob { get; set; }
        public string EstimatedOptimizeDataSize { get; set; } = string.Empty;

    }
    public class RMJobSummaryInfos
    {
        public JobType JobType { get; set; }
        public string JobId { get; set; }
        public List<RMJobSummaryItem> SummaryItem { get; set; }
    }
    public class RMJobSummaryItem
    {
        public string Title { get; set; }

        public List<RMJobSummaryRow> SummaryRow { get; set; }

    }
    public class RMJobSummaryRow
    {
        //public RMSummaryRowType Type { get; set; }

        public string Key { get; set; }

        public string Value { get; set; }

    }
    //public enum RMSummaryRowType
    //{
    //    Normal,
    //    Schedule
    //}
}