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
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common.Schedule.Object;

namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ScheduleTemplateDto
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public bool IsAuditorModeEnable { get; set; }

        [DataMember]
        public IntervalType AuditorInterval { get; set; }

        [DataMember]
        public int AuditorCount { get; set; }

        [DataMember]
        public bool IsScanModeEnable { get; set; }

        [DataMember]
        public IntervalType ScanInterval { get; set; }

        [DataMember]
        public int ScanCount { get; set; }

        [DataMember]
        public DateTime ScanStartTime { get; set; }

        [DataMember]
        public Dictionary<AdminEventType, bool> AuditorEventTypeInfos { get; set; }

        [DataMember]
        public Dictionary<AdminEventType, bool> ScanEventTypeInfos { get; set; }

        /// <summary>
        /// 标记此Template是否被设置为Default
        /// </summary>
        [DataMember]
        public bool IsDefault { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ScheduleTemplateAction
    {
        [EnumMember]
        None,
        [EnumMember]
        SetAsDefault,
        [EnumMember]
        CheckUsingStatus,
        [EnumMember]
        Delete,
    }
}
