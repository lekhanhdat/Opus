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
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Server.Job.Object;

namespace AvePoint.GCommon.Contract.Server.Retention
{
    /*
     JobDetail       real info
     Remark3 <-- --> JobId
     Remark4 <-- --> PlanName
     Remark5 <-- --> Description
     Remark6 <-- --> Storage Policy
     Remark7 <-- --> LogicalDevice
     Remark8 <-- --> Move Data To or Update
     Remark9 <-- --> Action
     MediaHost <-- --> Media
     Option <-- --> Size（string）
     Type <-- --> Status
     Message <-- --> Comment
    */
    public class DPDataRetentionReport
    {
        [DataMember]
        public Dictionary<string, string> JobInformation { get; set; }

        [DataMember]
        public int ColumnCount { get; set; }

        [DataMember]
        public List<JobDetail> JobDetails { get; set; }
    }

    [DataContract]
    public class RetentionJobsInfo
    {
        [DataMember]
        public List<JobDetail> RetentionJobs { get; set; }
    }
}
