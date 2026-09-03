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





#region using directives
using System.Collections.Generic;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
#endregion

namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAFarmTimerJobStatusOperation : CAOperation
    {
        [DataMember]
        public JobPageInfo PageInfo { get; set; }
        [DataMember]
        public TimerJobSearchType SearchType { get; set; }

        //Other information
        [DataMember]
        public List<ServerInfo> Servers { get; set; }
        [DataMember]
        public List<ServiceInfo> Services { get; set; }
        [DataMember]
        public List<WebAppNameAndUrl> WebApplications { get; set; }
        [DataMember]
        public List<TimerJobDefinitionInfo> JobDefinitons { get; set; }

        [DataMember]
        public int TotalScheduledJobs { get; set; }
        [DataMember]
        public int TotalRunningJobs { get; set; }
        [DataMember]
        public int TotalHistoryJobs { get; set; }

        //JobStatus
        [DataMember]
        public List<TimerJobStatus> ScheduledJobsStatus { get; set; }
        [DataMember]
        public List<TimerJobStatus> RunningJobsStatus { get; set; }
        [DataMember]
        public List<TimerJobStatus> HistoryJobsStatus { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class JobPageInfo
    {
        [DataMember]
        public TimerJobType JobType { get; set; }
        [DataMember]
        public int PageSize { get; set; }
        [DataMember]
        public int CurPage { get; set; }

        // Guid
        [DataMember]
        public string WebAppId { get; set; }
        [DataMember]
        public string ServiceId { get; set; }
        [DataMember]
        public string ServerId { get; set; }
        [DataMember]
        public string JobDefinitionId { get; set; }
        [DataMember]
        public int Status { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class TimerJobStatus
    {
        [DataMember]
        public string JobTitle { get; set; }
        [DataMember]
        public string Server { get; set; }
        [DataMember]
        public int Status { get; set; }
        [DataMember]
        public int Progress { get; set; }
        [DataMember]
        public string Started { get; set; }
        [DataMember]
        public string Ended { get; set; }
        [DataMember]
        public string WebApplication { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ServerInfo
    {
        [DataMember]
        public string ServerId { get; set; }
        [DataMember]
        public string ServerName { get; set; }
        [DataMember]
        public string ServerDisplayName { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ServiceInfo
    {
        [DataMember]
        public string ServiceId { get; set; }
        [DataMember]
        public string ServiceTypeName { get; set; }
        [DataMember]
        public string ServiceName { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum TimerJobType
    {
        [EnumMember]
        ScheduledJob = 0,
        [EnumMember]
        RunningJob = 1,
        [EnumMember]
        HistoryJob = 2,
        [EnumMember]
        AllJob = 3
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum TimerJobSearchType
    {
        [EnumMember]
        Service = 0,
        [EnumMember]
        WebApplication = 1,
        [EnumMember]
        Server = 2,
        [EnumMember]
        JobDefinition = 3,
        [EnumMember]
        All = 4
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RunningJobStatus
    {
        [EnumMember]
        Scheduled = 0,
        [EnumMember]
        Initialized = 1,
        [EnumMember]
        Succeeded = 2,
        [EnumMember]
        Failed = 3,
        [EnumMember]
        Retry = 4,
        [EnumMember]
        Aborted = 5,
        [EnumMember]
        Pausing = 6,
        [EnumMember]
        Paused = 7
    }
}
