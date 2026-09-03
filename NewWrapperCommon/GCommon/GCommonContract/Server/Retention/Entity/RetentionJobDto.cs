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
using System.Text;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.GCommon.Contract.PlatformRecovery.Object;

namespace AvePoint.GCommon.Contract.Server.Retention
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RetentionJobDto : BaseJobDto
    {
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_1)]
        public RetentionActionType ActionType { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_2)]
        public PlanCategory Module { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_2)]
        public string StoragePolicyIds { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_3)]
        public PRPlatformType PlatformType { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_3)]
        public string TriggerJobId { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_4)]
        public string NotificationProfileId { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_5)]
        public string DataSize { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_6)]
        public string MediaId { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_7)]
        public string AgentId { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.CLOB_1)]
        public string JobDetail { get; set; }

        /// <summary>
        /// Retention扩展属性
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.CLOB_2)]
        public string RetentionJobExtentionStr { get; set; }

        [DataMember]
        public RetentionJobExtentionDto RetentionJobExtentionDto { get; set; }

        [DataMember]
        public string StoragePolicyNames { get; set; }
    }
}
