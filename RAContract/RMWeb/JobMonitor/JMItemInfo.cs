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
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.JobMonitor;

namespace AvePoint.RA.Contract.RMWeb.JobMonitor
{
    [Serializable]
    [DataContract]
    public class JMItemInfo
    {
        [DataMember]
        public Guid Id { get; set; }
        [DataMember]
        public string JobId { get; set; }
        [DataMember]
        public string TaskName { get; set; }
        [DataMember]
        public string JobType { get; set; }
        [DataMember]
        public int JobTypeCode { get; set; }

        //public string Module { get; set; }
        [DataMember]
        public string StartTime { get; set; }
        [DataMember]
        public string EndTime { get; set; }
        [DataMember]
        public JobStatus Status { get; set; }
        [DataMember]
        public int Progress { get; set; }
        [DataMember]
        public int ProfileId { get; set; }
        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public string LastUpdateTime { get; set; }
        [DataMember]
        public int NodeType { get; set; }
        [DataMember]
        public string Comment { get; set; }
        [DataMember]
        public int? MigrationJobStatus { get; set; }
        [DataMember]
        public string AdditionalInformation { get; set; }
        [DataMember]
        public string Joblocation { get; set; }

        [DataMember]
        public string SiteUrl { get; set; }

        [DataMember]
        public JobPriority JobPriority { get; set; }

        [DataMember]
        public long SubJobCount { get; set; }

        [DataMember]
        public JobVersion JobVersion { get; set; }

        [DataMember]
        public bool IsUnMergedJob => JobVersion == JobVersion.UnMerged;
    }


    [Serializable]
    public class DisposalJMItemInfo : JMItemInfo
    {
        [DataMember]
        public int Order { get; set; }
    }

    [Serializable]
    public class AOSPJMItemInfo : JMItemInfo
    {
        [DataMember]
        public List<AOSPJobSiteStatus> jobSiteStatuses { get; set; }
    }

    [Serializable]
    public class DisposalJMItemResult
    {
        public List<DisposalJMItemInfo> Items { get; set; }

        public bool IsDeleted;
    }

    [Serializable]
    public class AOSPJobSiteStatus
    {
        [DataMember]
        public string SiteUrl { get; set; }

        [DataMember]
        public JobStatus SiteStatus { get; set; }
        
        [DataMember]
        public string Comment { get; set; }
    }

}
