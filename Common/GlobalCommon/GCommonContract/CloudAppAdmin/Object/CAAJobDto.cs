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

namespace AvePoint.GCommon.Contract.CloudAppAdmin.Object
{
    using AvePoint.GCommon.Contract.CloudAppAdmin.Message;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Server.Common;
    using AvePoint.GCommon.Contract.Server.Common.Schedule.Object;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAAJobDto : BaseJobDto
    {
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_1)]
        public int OperationType { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_1)]
        public string ADInfo { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAAJobParams
    {
        [DataMember]
        public CAAPlanDto PlanInfo { get; set; }

        [DataMember]
        public ScheduleDto Schedule { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AzureADManagementReportDto
    {
        [DataMember]
        public string Type { get; set; }
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string Action { get; set; }
        [DataMember]
        public RecordStatus RecordStatus { get; set; }
        [DataMember]
        public DateTime StartTime { get; set; }
        [DataMember]
        public DateTime FinishTime { get; set; }
        [DataMember]
        public string Comment { get; set; }

        //for mailbox access
        [DataMember]
        public string UserPrincipalName { get; set; }
        [DataMember]
        public string MailboxName { get; set; }
        [DataMember]
        public DateTime DatetimeForPE { get; set; }
        [DataMember]
        public SimpleADUser SimpleUser { get; set; }
        [DataMember]
        public string ProfileName { get; set; }
        [DataMember]
        public CAAPERuleCategory RuleCategory { get; set; }

        [DataMember]
        public CAAPERuleReportItem PEReportItem { get; set; }

        [DataMember]
        public CAAPERuleConflictDetailContent PEConflictItem { get; set; }

        [DataMember]
        public string RemarkJson1 { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RecordStatus
    {
        [EnumMember]
        Success = 0,

        [EnumMember]
        Failed = 1,

        [EnumMember]
        Skipped = 2,

        [EnumMember]
        Complying = 3
    }
}