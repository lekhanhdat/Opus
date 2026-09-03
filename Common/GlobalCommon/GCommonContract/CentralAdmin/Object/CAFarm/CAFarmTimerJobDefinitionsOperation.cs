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
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
#endregion

namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    [DataContract (Namespace = ContractConstants.Namespace)]
    public class CAFarmTimerJobDefinitionsOperation : CAOperation
    {
        [DataMember]
        public List<TimerJobDefinitionInfo> TimerJobs { get; set; }
        [DataMember]
        public List<string> Services { get; set; }
        [DataMember]
        public List<WebAppNameAndUrl> WebApplications { get; set; }
    }

    [DataContract (Namespace = ContractConstants.Namespace)]
    public class TimerJobDefinitionInfo : IComparable<TimerJobDefinitionInfo>
    {
        [DataMember]
        public string JobId { get; set; }
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string Service { get; set; }
        [DataMember]
        public string WebApplication { get; set; }
        [DataMember]
        public string ScheduleType { get; set; }
        [DataMember]
        public string ScheduleString { get; set; }
        [DataMember]
        public string LastRunTime { get; set; }
        [DataMember]
        public bool CanBeEnabled { get; set; }
        [DataMember]
        public bool CanBeDisabled { get; set; }
        [DataMember]
        public bool CanBeDeleted { get; set; }
        [DataMember]
        public TimerJobAction action { get; set; }

        #region IComparable<TimerJobDefinitionInfo> Members

        public int CompareTo(TimerJobDefinitionInfo other)
        {          
            if (other == null) return 1;
            return string.Compare(this.Title, other.Title, StringComparison.Ordinal);
        }

        #endregion
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class WebAppNameAndUrl
    {
        [DataMember]
        public string WebAppId { get; set; }
        [DataMember]
        public string WebAppName { get; set; }
        [DataMember]
        public string WebAppUrl { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum TimerJobAction
    {
        [EnumMember]
        Enable = 0,
        [EnumMember]
        Disable = 1,
        [EnumMember]
        Delete = 2,
        [EnumMember]
        RunNow = 3
    }
}
