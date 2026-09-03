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



using System.Runtime.Serialization;
using AvePoint.Common.Module.JobMonitor.Entities;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Server.Job.Object;
using System.Collections.Generic;
using System;
using System.Reflection;

namespace AvePoint.GCommon.Contract.Server.Common.Monitor.Object.Detail
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class JobContextSearchDto
    {
        [DataMember]
        public string JobId { get; set; }
        /// <summary>
        /// 形如 JobContextType.AgentContext | JobContextType.Content | JobContextType.Content
        /// </summary>
        [DataMember]
        public JobContextType Types { get; set; }
    }
    [Flags]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum JobContextType
    {
        [EnumMember]
        Nil = 0,
        [EnumMember]
        Content = 1,
        [EnumMember]
        PlanSettings = 1 << 1,
        [EnumMember]
        AgentContext = 1 << 2
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    //[KnownType(typeof(SubJobDetailSearchDto))]
    [KnownType("GetKnownTypes")]
    public class JobDetailSearchDto : BaseDetailSearchDto
    {
        public static IEnumerable<Type> GetKnownTypes()
        {
            return AveKnownTypeContext.GetKnonwTypes(MethodBase.GetCurrentMethod().DeclaringType);
        }

        [DataMember]
        public BaseJobDto Job { get; set; }

        public JobDetailSearchDto Clone()
        {
            return new JobDetailSearchDto()
            {
                CommonSearch = this.CommonSearch,
                EntityTypes = this.EntityTypes != null ? this.EntityTypes.Clone() as JobReportDetailEntityType[] : null,
                Skip = this.Skip,
                States = this.States != null ? this.States.Clone() as JobReportDetailStatus[] : null,
                Take = this.Take,
                Job = this.Job,
                TimeZoneId = TimeZoneId,
                ZoneType = ZoneType,
                SearchType = this.SearchType,
                CurrentLogonUserId = this.CurrentLogonUserId
            };
        }
        [DataMember]
        public SearchType SearchType { get; set; }
        public void ResetTimeZone()
        {
            if (ZoneType == TimeZoneType.Local) Job.TimeZoneId = TimeZoneId;
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SubJobDetailSearchDto : JobDetailSearchDto
    {
        [DataMember]
        public SubJobDto SubJob { get; set; }

        public SubJobDetailSearchDto Clone()
        {
            return new SubJobDetailSearchDto()
            {
                CommonSearch = this.CommonSearch,
                EntityTypes = this.EntityTypes != null ? this.EntityTypes.Clone() as JobReportDetailEntityType[] : null,
                Skip = this.Skip,
                States = this.States != null ? this.States.Clone() as JobReportDetailStatus[] : null,
                Take = this.Take,
                Job = this.Job,
                SubJob = this.SubJob,
                TimeZoneId = TimeZoneId,
                ZoneType = ZoneType
            };
       }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SearchType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        ExceptTotleCount = 1
    }
}
