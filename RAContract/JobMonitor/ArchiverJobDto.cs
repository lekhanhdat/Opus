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
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.JobMonitor
{
    public class ArchiverJobDto
    {
        public string Id { get; set; }
        public int JobType { get; set; }
        public int JobCategory { get; set; }
        public string PlanId { get; set; }
    }
    [DataContract]
    public class ArchiverJobMonitorDto
    {
        [DataMember]
        public JobStatus JobStatus { get; set; }
        [DataMember]
        public long LastUpdateTime { get; set; }
        [DataMember]
        public int Progress { get; set; }
        [DataMember]
        public List<ArchiverSubJobDto> SubjobInfoes { get; set; }
    }
    [DataContract]
    public class ArchiverSubJobDto
    {
        [DataMember]
        public JobStatus JobStatus { get; set; }
        [DataMember]
        public string SubJobId { get; set; }
    }
}
